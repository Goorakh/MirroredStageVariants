using MirroredStageVariants.Components;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MirroredStageVariants.Patches
{
    static class CostTextMirrorFix
    {
        [SystemInitializer]
        static void Init()
        {
            AsyncOperationHandle<GameObject> costHologramLoadHandle = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/CostHologramContent.prefab");
            costHologramLoadHandle.Completed += handle =>
            {
                GameObject costHologramContent = handle.Result;

                ScaleOnAwakeIfMirrored scaleOnAwakeIfMirrored = costHologramContent.AddComponent<ScaleOnAwakeIfMirrored>();
                scaleOnAwakeIfMirrored.ScaleMultiplier = new Vector3(-1f, 1f, 1f);
            };
        }
    }
}
