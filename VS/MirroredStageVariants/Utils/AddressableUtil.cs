using RoR2.ContentManagement;
using System.Runtime.CompilerServices;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MirroredStageVariants.Utils
{
    public static class AddressableUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncOperationHandle<T> LoadTempAssetAsync<T>(string assetKey) where T : UnityEngine.Object
        {
            return AssetAsyncReferenceManager<T>.LoadAsset(new AssetReferenceT<T>(assetKey), AsyncReferenceHandleUnloadType.Preload);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncOperationHandle<T> LoadTempAssetAsync<T>(AssetReferenceT<T> assetReference) where T : UnityEngine.Object
        {
            return AssetAsyncReferenceManager<T>.LoadAsset(assetReference, AsyncReferenceHandleUnloadType.Preload);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncOperationHandle<T> LoadAssetAsync<T>(string assetKey, AsyncReferenceHandleUnloadType unloadType = AsyncReferenceHandleUnloadType.AtWill) where T : UnityEngine.Object
        {
            return AssetAsyncReferenceManager<T>.LoadAsset(new AssetReferenceT<T>(assetKey), unloadType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncOperationHandle<T> LoadAssetAsync<T>(AssetReferenceT<T> assetReference, AsyncReferenceHandleUnloadType unloadType = AsyncReferenceHandleUnloadType.AtWill) where T : UnityEngine.Object
        {
            return AssetAsyncReferenceManager<T>.LoadAsset(assetReference, unloadType);
        }
    }
}
