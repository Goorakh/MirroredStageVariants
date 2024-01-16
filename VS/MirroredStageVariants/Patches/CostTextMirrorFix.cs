using MirroredStageVariants.Components;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MirroredStageVariants.Patches
{
    static class CostTextMirrorFix
    {
        [SystemInitializer]
        static void Init()
        {
            GameObject costHologramContent = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/CostHologramContent.prefab").WaitForCompletion();
            if (costHologramContent)
            {
                ScaleOnAwakeIfMirrored scaleOnAwakeIfMirrored = costHologramContent.AddComponent<ScaleOnAwakeIfMirrored>();
                scaleOnAwakeIfMirrored.ScaleMultiplier = new Vector3(-1f, 1f, 1f);
            }
            else
            {
                Log.Error("Failed to load cost hologram prefab");
            }
        }
    }
}
