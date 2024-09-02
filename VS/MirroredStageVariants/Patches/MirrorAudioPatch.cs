using HarmonyLib;
using MirroredStageVariants.Components;
using MirroredStageVariants.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MirroredStageVariants.Patches
{
    static class MirrorAudioPatch
    {
        delegate int SetObjectPositionDelegate(ulong akObjectId, Vector3 position, Vector3 forward, Vector3 up);

        static GameObject _setObjectPositionSourceObject;
        static readonly FieldInfo _setObjectPositionSourceObject_FI = AccessTools.DeclaredField(typeof(MirrorAudioPatch), nameof(_setObjectPositionSourceObject));

        static SetObjectPositionDelegate _origSetPosition;

        static readonly List<IDetour> _audioEngineHooks = [];

        public static void Apply()
        {
            foreach (MethodInfo soundEngineMethod in typeof(AkSoundEngine).GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (!string.Equals(soundEngineMethod.Name, "SetObjectPosition", StringComparison.OrdinalIgnoreCase))
                    continue;

                ParameterInfo[] parameters = soundEngineMethod.GetParameters();
                if (parameters.Length < 1)
                    continue;

                ParameterInfo targetParam = parameters[0];
                if (targetParam.ParameterType != typeof(GameObject))
                    continue;

                _audioEngineHooks.Add(new ILHook(soundEngineMethod, il =>
                {
                    ILCursor c = new ILCursor(il);

                    c.Emit(OpCodes.Ldarg, targetParam.Position);
                    c.Emit(OpCodes.Stsfld, _setObjectPositionSourceObject_FI);
                }));
            }

            NativeDetour setObjectPositionHook = new NativeDetour(SymbolExtensions.GetMethodInfo(() => AkSoundEnginePINVOKE.CSharp_SetObjectPosition(default, default, default, default)), SymbolExtensions.GetMethodInfo(() => AkSoundEngine_SetObjectPosition(default, default, default, default)));
            _origSetPosition = setObjectPositionHook.GenerateTrampoline<SetObjectPositionDelegate>();

            _audioEngineHooks.Add(setObjectPositionHook);
        }

        public static void Undo()
        {
            foreach (IDetour audioEngineHook in _audioEngineHooks)
            {
                audioEngineHook?.Dispose();
            }

            _audioEngineHooks.Clear();

            _origSetPosition = null;
        }

        static bool tryGetSoundEmitterMirrorTransformation(GameObject obj, out Matrix4x4 transformation)
        {
            if (StageMirrorController.CurrentlyIsMirrored)
            {
                AkGameObj akObj = obj ? obj.GetComponent<AkGameObj>() : null;

                List<AkAudioListener> listeners = akObj && !akObj.IsUsingDefaultListeners ? akObj.ListenerList : AkAudioListener.DefaultListeners.ListenerList;
                foreach (AkAudioListener listener in listeners)
                {
                    if (listener.GetComponent<MirrorCamera>())
                    {
                        transformation = listener.transform.GlobalTransformationFromLocal(Matrix4x4.Scale(new Vector3(-1f, 1f, 1f)));
                        return true;
                    }
                }
            }

            transformation = Matrix4x4.identity;
            return false;
        }

        static int AkSoundEngine_SetObjectPosition(ulong akObjectId, Vector3 position, Vector3 forward, Vector3 up)
        {
            // Safety check
            ulong sourceObjectAkId = AkSoundEngine.GetAkGameObjectID(_setObjectPositionSourceObject);
            if (sourceObjectAkId == akObjectId)
            {
                if (tryGetSoundEmitterMirrorTransformation(_setObjectPositionSourceObject, out Matrix4x4 mirrorTransform))
                {
                    position = mirrorTransform.MultiplyPoint(position);
                    forward = mirrorTransform.MultiplyVector(forward);
                    up = mirrorTransform.MultiplyVector(up);
                }
            }
            else
            {
                Log.Warning($"Object id's do not match! Something has gone wrong with the audio engine hook. sourceObject={_setObjectPositionSourceObject}, sourceObjectId={sourceObjectAkId}, objectId={akObjectId}");
            }

            _setObjectPositionSourceObject = null;
            return _origSetPosition(akObjectId, position, forward, up);
        }
    }
}
