using MirroredStageVariants.Utils;
using MirroredStageVariants.Utils.Extensions;
using RoR2;
using RoR2BepInExPack.GameAssetPathsBetter;
using TMPro;
using UnityEngine;

namespace MirroredStageVariants.Patches
{
    static class HideStunEffect
    {
        [SystemInitializer]
        static void Init()
        {
            AssetLoadUtils.LoadAssetAsync<GameObject>(RoR2_Base_Common_VFX.StunVfx_prefab).OnSuccess(stunVfx =>
            {
                TextMeshPro stunVfxText = stunVfx.GetComponentInChildren<TextMeshPro>();
                if (stunVfxText)
                {
                    stunVfxText.enabled = false;
                }
            });
        }
    }
}
