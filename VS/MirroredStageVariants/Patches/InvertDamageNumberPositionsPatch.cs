using MirroredStageVariants.Components;
using MirroredStageVariants.Utils;
using MirroredStageVariants.Utils.Extensions;
using RoR2;
using RoR2BepInExPack.GameAssetPathsBetter;
using UnityEngine;

namespace MirroredStageVariants.Patches
{
    static class InvertDamageNumberPositionsPatch
    {
        [SystemInitializer]
        static void Init()
        {
            AssetLoadUtils.LoadAssetAsync<GameObject>(RoR2_Base_Core.DamageNumberManager_prefab).OnSuccess(damageNumberManager =>
            {
                damageNumberManager.AddComponent<FlipParticleSystemIfMirrored>();
            });

            AssetLoadUtils.LoadAssetAsync<GameObject>(RoR2_DLC1_CritGlassesVoid.CritGlassesVoidExecuteEffect_prefab).OnSuccess(effectPrefab =>
            {
                Transform fakeDamageNumbersTransform = effectPrefab.transform.Find("FakeDamageNumbers");
                if (fakeDamageNumbersTransform)
                {
                    fakeDamageNumbersTransform.gameObject.AddComponent<FlipParticleSystemIfMirrored>();
                }
                else
                {
                    Log.Error("Failed to find fake damage number emitter on CritGlassesVoidExecuteEffect");
                }
            });
        }
    }
}
