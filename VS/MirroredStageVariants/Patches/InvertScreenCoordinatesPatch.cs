using MirroredStageVariants.Components;
using MirroredStageVariants.Utils;
using MonoMod.RuntimeDetour;
using RoR2;
using UnityEngine;

namespace MirroredStageVariants.Patches
{
    static class InvertScreenCoordinatesPatch
    {
        delegate Vector3 ScreenCoordinateDelegate(Camera self, Vector3 position, Camera.MonoOrStereoscopicEye eye);

        delegate Ray ScreenRayDelegate(Camera self, Vector2 pos, Camera.MonoOrStereoscopicEye eye);

        [SystemInitializer]
        static void Init()
        {
            new Hook(() => default(Camera).WorldToScreenPoint(default, default), invertPixelCoordinateResult);
            new Hook(() => default(Camera).WorldToViewportPoint(default, default), invertNormalizedCoordinateResult);

            new Hook(() => default(Camera).ScreenToWorldPoint(default, default), invertPixelCoordinateInput);
            new Hook(() => default(Camera).ViewportToWorldPoint(default, default), invertNormalizedCoordinateInput);

            new Hook(() => default(Camera).ScreenPointToRay(default(Vector2), default), invertRayPixelCoordinateInput);
            new Hook(() => default(Camera).ViewportPointToRay(default(Vector2), default), invertRayNormalizedCoordinateInput);
        }

        static bool isMirrored(Camera camera)
        {
            return StageMirrorController.CurrentlyIsMirrored && camera.GetComponent<MirrorCamera>();
        }

        static Vector3 invertPixelCoordinateResult(ScreenCoordinateDelegate orig, Camera self, Vector3 position, Camera.MonoOrStereoscopicEye eye)
        {
            Vector3 result = orig(self, position, eye);

            if (isMirrored(self))
            {
                CoordinateUtils.InvertScreenXCoordinate(ref result.x, self.pixelRect);
            }

            return result;
        }

        static Vector3 invertPixelCoordinateInput(ScreenCoordinateDelegate orig, Camera self, Vector3 position, Camera.MonoOrStereoscopicEye eye)
        {
            if (isMirrored(self))
            {
                CoordinateUtils.InvertScreenXCoordinate(ref position.x, self.pixelRect);
            }

            return orig(self, position, eye);
        }

        static Vector3 invertNormalizedCoordinateResult(ScreenCoordinateDelegate orig, Camera self, Vector3 position, Camera.MonoOrStereoscopicEye eye)
        {
            Vector3 result = orig(self, position, eye);

            if (isMirrored(self))
            {
                CoordinateUtils.InvertScreenXCoordinate(ref result.x, self.rect);
            }

            return result;
        }

        static Vector3 invertNormalizedCoordinateInput(ScreenCoordinateDelegate orig, Camera self, Vector3 position, Camera.MonoOrStereoscopicEye eye)
        {
            if (isMirrored(self))
            {
                CoordinateUtils.InvertScreenXCoordinate(ref position.x, self.rect);
            }

            return orig(self, position, eye);
        }

        static Ray invertRayPixelCoordinateInput(ScreenRayDelegate orig, Camera self, Vector2 pos, Camera.MonoOrStereoscopicEye eye)
        {
            if (isMirrored(self))
            {
                CoordinateUtils.InvertScreenXCoordinate(ref pos.x, self.pixelRect);
            }

            return orig(self, pos, eye);
        }

        static Ray invertRayNormalizedCoordinateInput(ScreenRayDelegate orig, Camera self, Vector2 pos, Camera.MonoOrStereoscopicEye eye)
        {
            if (isMirrored(self))
            {
                CoordinateUtils.InvertScreenXCoordinate(ref pos.x, self.rect);
            }

            return orig(self, pos, eye);
        }
    }
}
