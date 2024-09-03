using MirroredStageVariants.Utils;
using UnityEngine;

namespace MirroredStageVariants.Components
{
    [DisallowMultipleComponent]
    public class MirrorTextLabelIfMirrored : MonoBehaviour
    {
        bool _isRenderingAsMirrored;
        Vector3 _originalScale;

        void OnEnable()
        {
            CameraEvents.OnPreRender += onPreRender;
            CameraEvents.OnPostRender += onPostRender;
        }

        void OnDisable()
        {
            CameraEvents.OnPreRender -= onPreRender;
            CameraEvents.OnPostRender -= onPostRender;
        }

        void onPreRender(Camera cam)
        {
            _isRenderingAsMirrored = cam.ShouldRenderObjectAsMirrored(gameObject);
            if (_isRenderingAsMirrored)
            {
                _originalScale = transform.localScale;

                Vector3 scale = _originalScale;
                scale.x *= -1f;
                transform.localScale = scale;
            }
        }

        void onPostRender(Camera cam)
        {
            if (_isRenderingAsMirrored)
            {
                transform.localScale = _originalScale;
                _isRenderingAsMirrored = false;
            }
        }
    }
}
