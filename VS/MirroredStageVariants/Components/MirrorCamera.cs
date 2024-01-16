using RoR2;
using UnityEngine;

namespace MirroredStageVariants.Components
{
    class MirrorCamera : MonoBehaviour
    {
        [SystemInitializer]
        static void Init()
        {
            On.RoR2.SceneCamera.Awake += (orig, self) =>
            {
                orig(self);
                self.gameObject.AddComponent<MirrorCamera>();
            };
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Material mirrorMaterial = Main.Instance ? Main.Instance.MirrorMaterial : null;
            if (StageMirrorController.CurrentStageIsMirrored && mirrorMaterial)
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
