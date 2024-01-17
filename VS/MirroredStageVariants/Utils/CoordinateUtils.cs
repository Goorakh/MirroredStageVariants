using RoR2;
using UnityEngine;

namespace MirroredStageVariants.Utils
{
    public static class CoordinateUtils
    {
        public static float GetInvertedScreenXCoordinate(float coordinate, Rect space)
        {
            float leftEdge = space.xMin;
            float rightEdge = space.xMax;

            return Util.Remap(coordinate, leftEdge, rightEdge, rightEdge, leftEdge);
        }

        public static void InvertScreenXCoordinate(ref float coordinate, Rect space)
        {
            coordinate = GetInvertedScreenXCoordinate(coordinate, space);
        }
    }
}
