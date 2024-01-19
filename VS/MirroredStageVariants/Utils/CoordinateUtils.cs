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

        public static Vector2 Remap(Vector2 value, Rect inputSpace, Rect outputSpace)
        {
            float x = Util.Remap(value.x, inputSpace.xMin, inputSpace.xMax, outputSpace.xMin, outputSpace.xMax);
            float y = Util.Remap(value.y, inputSpace.yMin, inputSpace.yMax, outputSpace.yMin, outputSpace.yMax);

            return new Vector2(x, y);
        }
    }
}
