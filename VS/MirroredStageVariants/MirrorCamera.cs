using UnityEngine;

namespace MirroredStageVariants
{
    class MirrorCamera : MonoBehaviour
    {
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Material mirrorMaterial = Main.Instance ? Main.Instance.MirrorMaterial : null;

            if (mirrorMaterial)
            {
                Graphics.Blit(source, destination, mirrorMaterial);
            }
            else
            {
                Graphics.Blit(source, destination);
            }
        }
    }
}
