using MirroredStageVariants.Components;
using RoR2;
using UnityEngine;

namespace MirroredStageVariants.Utils
{
    public static class CameraUtils
    {
        public static bool ShouldRenderObjectAsMirrored(this Camera camera, GameObject obj)
        {
            if (StageMirrorController.CurrentlyIsMirrored && (camera.cullingMask & (1 << obj.layer)) != 0)
            {
                if (camera.GetComponent<MirrorCamera>())
                {
                    return true;
                }

                CameraRigController cameraRigController;
                if (camera.TryGetComponent(out UICamera uiCamera))
                {
                    cameraRigController = uiCamera.cameraRigController;
                }
                else
                {
                    cameraRigController = camera.GetComponentInParent<CameraRigController>();
                }

                if (cameraRigController && cameraRigController.sceneCam && cameraRigController.sceneCam.GetComponent<MirrorCamera>())
                {
                    return true;
                }
            }

            return false;
        }
    }
}
