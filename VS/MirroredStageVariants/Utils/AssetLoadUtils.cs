using RoR2.ContentManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MirroredStageVariants.Utils
{
    public static class AssetLoadUtils
    {
        public static AsyncOperationHandle<T> LoadAssetAsync<T>(string guid, bool autoUnload = false) where T : UnityEngine.Object
        {
            return LoadAssetAsync(new AssetReferenceT<T>(guid), autoUnload);
        }

        public static AsyncOperationHandle<T> LoadAssetAsync<T>(AssetReferenceT<T> assetReference, bool autoUnload = false) where T : UnityEngine.Object
        {
            AsyncOperationHandle<T> handle = AssetAsyncReferenceManager<T>.LoadAsset(assetReference);
            if (autoUnload)
            {
                handle.Completed += handle =>
                {
                    AssetAsyncReferenceManager<T>.UnloadAsset(assetReference);
                };
            }

            return handle;
        }
    }
}
