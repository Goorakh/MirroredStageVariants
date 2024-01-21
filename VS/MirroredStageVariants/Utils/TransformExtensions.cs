using System.Collections.Generic;
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

        public static IEnumerable<Transform> GetAllChildrenRecursive(this Transform transform)
        {
            yield return transform;

            for (int i = 0; i < transform.childCount; i++)
            {
                foreach (Transform child in transform.GetChild(i).GetAllChildrenRecursive())
                {
                    yield return child;
                }
            }
        }
    }
}
