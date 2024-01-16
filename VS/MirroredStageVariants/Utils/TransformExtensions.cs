using UnityEngine;

namespace MirroredStageVariants.Utils
{
    public static class TransformExtensions
    {
        public static Matrix4x4 GlobalTransformationFromLocal(this Transform transform, Matrix4x4 localTransformation)
        {
            return transform.localToWorldMatrix *
                   localTransformation *
                   transform.worldToLocalMatrix;
        }
    }
}
