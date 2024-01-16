using MirroredStageVariants.Components;
using MirroredStageVariants.Utils;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace MirroredStageVariants.Patches
{
    static class MirrorAudioPatch
    {
        delegate Vector3 AkGameObj_GetVectorDelegate(AkGameObj self);

        static Hook AkGameObj_MirrorPosition_Hook;
        static Hook AkGameObj_MirrorForwardVector_Hook;
        static Hook AkGameObj_MirrorUpVector_Hook;

        public static void Apply()
        {
            AkGameObj_MirrorPosition_Hook = new Hook(() => default(AkGameObj).GetPosition(), AkGameObj_GenericTryMirrorPositionResult);
            AkGameObj_MirrorForwardVector_Hook = new Hook(() => default(AkGameObj).GetForward(), AkGameObj_GenericTryMirrorVectorResult);
            AkGameObj_MirrorUpVector_Hook = new Hook(() => default(AkGameObj).GetUpward(), AkGameObj_GenericTryMirrorVectorResult);
        }

        public static void Undo()
        {
            AkGameObj_MirrorPosition_Hook?.Undo();
            AkGameObj_MirrorForwardVector_Hook?.Undo();
            AkGameObj_MirrorUpVector_Hook?.Undo();
        }

        static Matrix4x4 getSoundEmitterMirrorTransformation(AkGameObj self)
        {
            if (StageMirrorController.CurrentStageIsMirrored)
            {
                List<AkAudioListener> listeners = self.IsUsingDefaultListeners ? AkAudioListener.DefaultListeners.ListenerList : self.ListenerList;
                if (listeners.Count == 1)
                {
                    AkAudioListener listener = listeners[0];
                    if (listener.GetComponent<MirrorCamera>())
                    {
                        return listener.transform.GlobalTransformationFromLocal(Matrix4x4.Scale(new Vector3(-1f, 1f, 1f)));
                    }
                }
#if DEBUG
                else if (listeners.Count > 1)
                {
                    Log.Debug($"{self} has {listeners.Count} listeners, position hook cannot be used");
                }
#endif
            }

            return Matrix4x4.identity;
        }

        static Vector3 AkGameObj_GenericTryMirrorPositionResult(AkGameObj_GetVectorDelegate orig, AkGameObj self)
        {
            return getSoundEmitterMirrorTransformation(self).MultiplyPoint(orig(self));
        }

        static Vector3 AkGameObj_GenericTryMirrorVectorResult(AkGameObj_GetVectorDelegate orig, AkGameObj self)
        {
            return getSoundEmitterMirrorTransformation(self).MultiplyVector(orig(self));
        }
    }
}
