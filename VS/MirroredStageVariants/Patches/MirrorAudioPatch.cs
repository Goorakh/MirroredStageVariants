using HarmonyLib;
using MirroredStageVariants.Components;
using MonoMod.RuntimeDetour;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace MirroredStageVariants.Patches
{
    static class MirrorAudioPatch
    {
        delegate int SetObjectPositionDelegate(ulong akObjectId, Vector3 position, Vector3 forward, Vector3 up);

        static SetObjectPositionDelegate _origSetPosition;

        static readonly Dictionary<ulong, AkObjectData> _akObjects = [];

        sealed class AkObjectData
        {
            public readonly ulong Id;

            public readonly GameObject GameObject;
            public readonly AkGameObj AkObj;

            public Vector3 Position { get; private set; }
            public Vector3 Forward { get; private set; }
            public Vector3 Up { get; private set; }

            public AkObjectData(ulong id, GameObject gameObject)
            {
                Id = id;
                GameObject = gameObject;
                AkObj = gameObject.GetComponent<AkGameObj>();

                Transform transform = gameObject.transform;

                Position = AkObj ? AkObj.GetPosition() : transform.position;
                Forward = AkObj ? AkObj.GetForward() : transform.forward;
                Up = AkObj ? AkObj.GetUpward() : transform.up;
            }

            bool tryGetListenerMirrorTransformation(out Matrix4x4 mirrorTransform)
            {
                List<AkAudioListener> listeners = AkObj && !AkObj.IsUsingDefaultListeners ? AkObj.ListenerList : AkAudioListener.DefaultListeners.ListenerList;
                foreach (AkAudioListener listener in listeners)
                {
                    if (listener.TryGetComponent(out MirrorCamera mirrorCamera))
                    {
                        mirrorTransform = mirrorCamera.MirrorAroundCameraTransformation;
                        return true;
                    }
                }

                mirrorTransform = Matrix4x4.identity;
                return false;
            }

            public void UpdatePosition(Vector3 position, Vector3 forward, Vector3 up)
            {
                Position = position;
                Forward = forward;
                Up = up;
            }

            public void GetMirrorPosition(out Vector3 position, out Vector3 forward, out Vector3 up)
            {
                position = Position;
                forward = Forward;
                up = Up;

                if (tryGetListenerMirrorTransformation(out Matrix4x4 mirrorTransform))
                {
                    position = mirrorTransform.MultiplyPoint(position);
                    forward = mirrorTransform.MultiplyVector(forward);
                    up = mirrorTransform.MultiplyVector(up);
                }
            }

            public AKRESULT UpdateMirrorPosition()
            {
                GetMirrorPosition(out Vector3 position, out Vector3 forward, out Vector3 up);

                if (_origSetPosition == null)
                {
                    Log.Error("Missing set position function");
                    return AKRESULT.AK_Fail;
                }

                return (AKRESULT)_origSetPosition(Id, position, forward, up);
            }
        }

        [SystemInitializer]
        static void Init()
        {
            NativeDetour setObjectPositionHook = new NativeDetour(SymbolExtensions.GetMethodInfo(() => AkSoundEnginePINVOKE.CSharp_SetObjectPosition(default, default, default, default)), SymbolExtensions.GetMethodInfo(() => AkSoundEngine_SetObjectPosition(default, default, default, default)));
            _origSetPosition = setObjectPositionHook.GenerateTrampoline<SetObjectPositionDelegate>();

            new Hook(SymbolExtensions.GetMethodInfo(() => AkSoundEngine.PostRegisterGameObjUserHook(default, default, default)), AkSoundEngine_PostRegisterGameObjUserHook);

            new Hook(SymbolExtensions.GetMethodInfo(() => AkSoundEngine.PostUnregisterGameObjUserHook(default, default, default)), AkSoundEngine_PostUnregisterGameObjUserHook);

            new Hook(SymbolExtensions.GetMethodInfo(() => AkSoundEngine.ClearRegisteredGameObjects()), AkSoundEngine_ClearRegisteredGameObjects);

            MirrorCamera.OnMirrorTransformationChanged += onCameraMirrorTransformationChanged;
        }

        static void onCameraMirrorTransformationChanged()
        {
            foreach (AkObjectData akObjData in _akObjects.Values)
            {
                akObjData.UpdateMirrorPosition();
            }
        }

        delegate void orig_AkSoundEngine_PostRegisterGameObjUserHook(AKRESULT result, GameObject gameObject, ulong id);
        static void AkSoundEngine_PostRegisterGameObjUserHook(orig_AkSoundEngine_PostRegisterGameObjUserHook orig, AKRESULT result, GameObject gameObject, ulong id)
        {
            orig(result, gameObject, id);

            if (result == AKRESULT.AK_Success && gameObject && !_akObjects.ContainsKey(id))
            {
                _akObjects.Add(id, new AkObjectData(id, gameObject));
            }
        }

        delegate void orig_AkSoundEngine_PostUnregisterGameObjUserHook(AKRESULT result, GameObject gameObject, ulong id);
        static void AkSoundEngine_PostUnregisterGameObjUserHook(orig_AkSoundEngine_PostUnregisterGameObjUserHook orig, AKRESULT result, GameObject gameObject, ulong id)
        {
            orig(result, gameObject, id);

            if (result == AKRESULT.AK_Success)
            {
                _akObjects.Remove(id);
            }
        }

        delegate void orig_AkSoundEngine_ClearRegisteredGameObjects();
        static void AkSoundEngine_ClearRegisteredGameObjects(orig_AkSoundEngine_ClearRegisteredGameObjects orig)
        {
            orig();
            _akObjects.Clear();
        }

        static int AkSoundEngine_SetObjectPosition(ulong akObjectId, Vector3 position, Vector3 forward, Vector3 up)
        {
            if (_akObjects.TryGetValue(akObjectId, out AkObjectData objectData))
            {
                objectData.UpdatePosition(position, forward, up);
                objectData.GetMirrorPosition(out position, out forward, out up);
            }

            return _origSetPosition(akObjectId, position, forward, up);
        }
    }
}
