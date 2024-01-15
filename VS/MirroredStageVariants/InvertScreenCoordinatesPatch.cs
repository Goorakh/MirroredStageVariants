using MonoMod.RuntimeDetour;
using RoR2;
using UnityEngine;

namespace MirroredStageVariants
{
    // UNFINISHED
    // DOES NOT WORK
    // FUCK

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

        [SystemInitializer]
        static void Init()
        {
            Camera_WorldToScreenPoint_Hook = new Hook(() => default(Camera).WorldToScreenPoint(default, default), genericInvertScreenCoordinateResult);
            Camera_WorldToViewportPoint_Hook = new Hook(() => default(Camera).WorldToViewportPoint(default, default), genericInvertScreenCoordinateResult);

            Camera_ViewportToWorldPoint_Hook = new Hook(() => default(Camera).ViewportToWorldPoint(default, default), genericInvertScreenCoordinateInput);
            Camera_ScreenToWorldPoint_Hook = new Hook(() => default(Camera).ScreenToWorldPoint(default, default), genericInvertScreenCoordinateInput);

            Camera_ViewportPointToRay_Hook = new Hook(() => default(Camera).ViewportPointToRay(default(Vector2), default), genericInvertScreenRayInput);
            Camera_ScreenPointToRay_Hook = new Hook(() => default(Camera).ScreenPointToRay(default(Vector2), default), genericInvertScreenRayInput);
        }

        static void Undo()
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

        static float invertScreenCoordinate(float coordinate)
        {
            return Util.Remap(coordinate, 0f, 1f, 1f, 0f);
        }

        static void invertScreenCoordinate(ref float coordinate)
        {
            coordinate = invertScreenCoordinate(coordinate);
        }

        static Vector3 genericInvertScreenCoordinateResult(ScreenCoordinateDelegate orig, Camera self, Vector3 position, Camera.MonoOrStereoscopicEye eye)
        {
            Vector3 result = orig(self, position, eye);

            if (isMirrored(self))
            {
                invertScreenCoordinate(ref result.x);
            }

            return result;
        }

        static Vector3 genericInvertScreenCoordinateInput(ScreenCoordinateDelegate orig, Camera self, Vector3 position, Camera.MonoOrStereoscopicEye eye)
        {
            if (isMirrored(self))
            {
                invertScreenCoordinate(ref position.x);
            }

            return orig(self, position, eye);
        }

        static Ray genericInvertScreenRayInput(ScreenRayDelegate orig, Camera self, Vector2 pos, Camera.MonoOrStereoscopicEye eye)
        {
            if (isMirrored(self))
            {
                invertScreenCoordinate(ref pos.x);
            }

            return orig(self, pos, eye);
        }
    }
}
