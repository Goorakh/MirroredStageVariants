using MirroredStageVariants.Components;
using MirroredStageVariants.Utils;
using MirroredStageVariants.Utils.Extensions;
using RoR2;
using RoR2BepInExPack.GameAssetPathsBetter;
using UnityEngine;

namespace MirroredStageVariants.Patches
{
    static class CostTextMirrorFix
    {
        [SystemInitializer]
        static void Init()
        {
            AssetLoadUtils.LoadAssetAsync<GameObject>(RoR2_Base_Common_VFX.CostHologramContent_prefab).OnSuccess(costHologramContent =>
            {
                ScaleOnAwakeIfMirrored scaleOnAwakeIfMirrored = costHologramContent.AddComponent<ScaleOnAwakeIfMirrored>();
                scaleOnAwakeIfMirrored.ScaleMultiplier = new Vector3(-1f, 1f, 1f);
            });
        }
    }
}
