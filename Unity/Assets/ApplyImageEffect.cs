using UnityEngine;

public class ApplyImageEffect : MonoBehaviour
{
    public Material ImageEffectMaterial;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (ImageEffectMaterial)
        {
            Graphics.Blit(src, dest, ImageEffectMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}
