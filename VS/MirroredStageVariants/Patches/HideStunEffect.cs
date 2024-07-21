using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MirroredStageVariants.Patches
{
    static class HideStunEffect
    {
        const string key = "RoR2/Base/Common/VFX/StunVfx.prefab";
        static AsyncOperationHandle<GameObject> handle;

        public static void Apply()
        {
            handle = Addressables.LoadAssetAsync<GameObject>(key);
            handle.Completed += _ => showText(false);
        }

        public static void Undo() => showText(true);

        static void showText(bool enabled)
        {
            handle.WaitForCompletion().GetComponentInChildren<TextMeshPro>().enabled = enabled;
        }
    }
}
