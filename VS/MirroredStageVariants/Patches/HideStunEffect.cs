using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MirroredStageVariants.Patches
{
    static class HideStunEffect
    {
        static TextMeshPro _stunVfxText;

        public static void Apply()
        {
            if (_stunVfxText)
            {
                setTextVisibility(false);
            }
            else
            {
                AsyncOperationHandle<GameObject> loadOperation = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/StunVfx.prefab");
                loadOperation.Completed += handle =>
                {
                    _stunVfxText = null;

                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        GameObject stunVfx = handle.Result;

                        _stunVfxText = stunVfx.GetComponentInChildren<TextMeshPro>();
                        setTextVisibility(false);
                    }
                    else
                    {
                        Log.Error_NoCallerPrefix($"Failed to load StunVfx: {handle.OperationException}");
                    }
                };
            }
        }

        public static void Undo()
        {
            setTextVisibility(true);
            _stunVfxText = null;
        }

        static void setTextVisibility(bool enabled)
        {
            if (!_stunVfxText)
                return;

            _stunVfxText.enabled = enabled;
        }
    }
}
