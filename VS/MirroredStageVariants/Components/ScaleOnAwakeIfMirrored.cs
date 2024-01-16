using UnityEngine;

namespace MirroredStageVariants.Components
{
    [DefaultExecutionOrder(-1)]
    public class ScaleOnAwakeIfMirrored : MonoBehaviour
    {
        public Vector3 ScaleMultiplier = Vector3.one;

        void Awake()
        {
            if (StageMirrorController.CurrentStageIsMirrored)
            {
                transform.localScale = Vector3.Scale(transform.localScale, ScaleMultiplier);
            }
        }
    }
}
