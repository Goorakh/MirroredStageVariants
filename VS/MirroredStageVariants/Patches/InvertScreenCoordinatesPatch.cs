using MirroredStageVariants.Components;
using MonoMod.RuntimeDetour;
using RoR2;
using UnityEngine;

namespace MirroredStageVariants.Patches
{
    static class InvertScreenCoordinatesPatch
    {
        delegate Vector3 ScreenCoordinateDelegate(Camera self, Vector3 position, Camera.MonoOrStereoscopicEye eye);

        delegate Ray ScreenRayDelegate(Camera self, Vector2 pos, Camera.MonoOrStereoscopicEye eye);

        static Hook Camera_WorldToScreenPoint_Hook;
        static Hook Camera_WorldToViewportPoint_Hook;

        static Hook Camera_ViewportToWorldPoint_Hook;
        static Hook Camera_ScreenToWorldPoint_Hook;

        static Hook Camera_ViewportPointToRay_Hook;
        static Hook Camera_ScreenPointToRay_Hook;

        public static void Apply()
        {
            Camera_WorldToScreenPoint_Hook = new Hook(() => default(Camera).WorldToScreenPoint(default, default), invertPixelCoordinateResult);
            Camera_WorldToViewportPoint_Hook = new Hook(() => default(Camera).WorldToViewportPoint(default, default), invertNormalizedCoordinateResult);

            Camera_ScreenToWorldPoint_Hook = new Hook(() => default(Camera).ScreenToWorldPoint(default, default), invertPixelCoordinateInput);
            Camera_ViewportToWorldPoint_Hook = new Hook(() => default(Camera).ViewportToWorldPoint(default, default), invertNormalizedCoordinateInput);

            Camera_ScreenPointToRay_Hook = new Hook(() => default(Camera).ScreenPointToRay(default(Vector2), default), invertRayPixelCoordinateInput);
            Camera_ViewportPointToRay_Hook = new Hook(() => default(Camera).ViewportPointToRay(default(Vector2), default), invertRayNormalizedCoordinateInput);
        }

        public static void Undo()
        {
            Camera_WorldToScreenPoint_Hook?.Undo();
            Camera_WorldToViewportPoint_Hook?.Undo();

            Camera_ViewportToWorldPoint_Hook?.Undo();
            Camera_ScreenToWorldPoint_Hook?.Undo();

            Camera_ViewportPointToRay_Hook?.Undo();
            Camera_ScreenPointToRay_Hook?.Undo();
        }

        static bool isMirrored(Camera camera)
        {
            return StageMirrorController.CurrentStageIsMirrored && camera.GetComponent<MirrorCamera>();
        }

        static float invertScreenCoordinate(float coordinate, Rect space)
        {
            float leftEdge = space.xMin;
            float rightEdge = space.xMax;

            return Util.Remap(coordinate, leftEdge, rightEdge, rightEdge, leftEdge);
        }

        static void invertScreenCoordinate(ref float coordinate, Rect space)
        {
            coordinate = invertScreenCoordinate(coordinate, space);
        }

        static Vector3 invertPixelCoordinateResult(ScreenCoordinateDelegate orig, Camera self, Vector3 position, Camera.MonoOrStereoscopicEye eye)
        {
            Vector3 result = orig(self, position, eye);

            if (isMirrored(self))
            {
                invertScreenCoordinate(ref result.x, self.pixelRect);
            }

            return result;
        }

        static Vector3 invertPixelCoordinateInput(ScreenCoordinateDelegate orig, Camera self, Vector3 position, Camera.MonoOrStereoscopicEye eye)
        {
            if (isMirrored(self))
            {
                invertScreenCoordinate(ref position.x, self.pixelRect);
            }

            return orig(self, position, eye);
        }

        static Vector3 invertNormalizedCoordinateResult(ScreenCoordinateDelegate orig, Camera self, Vector3 position, Camera.MonoOrStereoscopicEye eye)
        {
            Vector3 result = orig(self, position, eye);

            if (isMirrored(self))
            {
                invertScreenCoordinate(ref result.x, self.rect);
            }

            return result;
        }

        static Vector3 invertNormalizedCoordinateInput(ScreenCoordinateDelegate orig, Camera self, Vector3 position, Camera.MonoOrStereoscopicEye eye)
        {
            if (isMirrored(self))
            {
                invertScreenCoordinate(ref position.x, self.rect);
            }

            return orig(self, position, eye);
        }

        static Ray invertRayPixelCoordinateInput(ScreenRayDelegate orig, Camera self, Vector2 pos, Camera.MonoOrStereoscopicEye eye)
        {
            if (isMirrored(self))
            {
                invertScreenCoordinate(ref pos.x, self.pixelRect);
            }

            return orig(self, pos, eye);
        }

        static Ray invertRayNormalizedCoordinateInput(ScreenRayDelegate orig, Camera self, Vector2 pos, Camera.MonoOrStereoscopicEye eye)
        {
            if (isMirrored(self))
            {
                invertScreenCoordinate(ref pos.x, self.rect);
            }

            return orig(self, pos, eye);
        }
    }
}
