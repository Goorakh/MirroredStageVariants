using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using Path = System.IO.Path;

namespace MirroredStageVariants
{
    static class Assets
    {
        interface IAssetReference
        {
            UnityEngine.Object Asset { get; set; }
        }

        class AssetLoadOperation(string assetName, AssetBundleRequest loadRequest, IAssetReference assetReference)
        {
            public string AssetName { get; } = assetName;

            public AssetBundleRequest LoadRequest { get; } = loadRequest ?? throw new ArgumentNullException(nameof(loadRequest));

            public IAssetReference AssetReference { get; } = assetReference ?? throw new ArgumentNullException(nameof(assetReference));

            public bool IsDone => LoadRequest.isDone;

            public void Update()
            {
                if (LoadRequest.isDone)
                {
                    UnityEngine.Object asset = LoadRequest.asset;
                    if (!asset)
                    {
                        Log.Error($"Missing asset '{AssetName}', check editor export!");
                        return;
                    }

                    AssetReference.Asset = asset;
                }
            }
        }

        class AssetReference<T> : IAssetReference where T : UnityEngine.Object
        {
            public T Asset;

            UnityEngine.Object IAssetReference.Asset
            {
                get => Asset;
                set => Asset = (T)value;
            }
        }

        readonly static AssetReference<Material> _mirrorMaterialRef = new AssetReference<Material>();
        public static Material MirrorMaterial => _mirrorMaterialRef.Asset;

        readonly static AssetReference<Shader> _mirrorOverlayShaderRef = new AssetReference<Shader>();
        public static Shader MirrorOverlayShader => _mirrorOverlayShaderRef.Asset;

#if DEBUG
        readonly static AssetReference<Material> _debugDrawUVRef = new AssetReference<Material>();
        public static Material DebugDrawUV => _debugDrawUVRef.Asset;
#endif

        [SystemInitializer]
        static IEnumerator Init()
        {
            string assetBundlePath = Path.Combine(Path.GetDirectoryName(Main.Instance.Info.Location), "mirror_assets");
            if (!File.Exists(assetBundlePath))
            {
                Log.Error($"Assets file not found, expected path: {assetBundlePath}");
                yield break;
            }

#if DEBUG
            Log.Debug($"Loading AssetBundle '{assetBundlePath}'");
#endif

            AssetBundleCreateRequest assetBundleLoad = AssetBundle.LoadFromFileAsync(assetBundlePath);
            while (!assetBundleLoad.isDone)
            {
                yield return null;
            }

            AssetBundle assetBundle = assetBundleLoad.assetBundle;
            if (!assetBundle)
            {
                Log.Error("Failed to load asset bundle");
                yield break;
            }

            AssetLoadOperation getLoadOperation<T>(string name, AssetReference<T> dest) where T : UnityEngine.Object
            {
                return new AssetLoadOperation(name, assetBundle.LoadAssetAsync<T>(name), dest);
            }

            List<AssetLoadOperation> loadOperations = [];

            loadOperations.Add(getLoadOperation("Mirror", _mirrorMaterialRef));

            loadOperations.Add(getLoadOperation("MirrorOverlay", _mirrorOverlayShaderRef));

#if DEBUG
            loadOperations.Add(getLoadOperation("DebugDrawUV", _debugDrawUVRef));
#endif

#if DEBUG
            Log.Debug($"Loading {loadOperations.Count} asset(s)...");
#endif

            while (loadOperations.Count > 0)
            {
                for (int i = loadOperations.Count - 1; i >= 0; i--)
                {
                    AssetLoadOperation loadOperation = loadOperations[i];

                    loadOperation.Update();
                    if (loadOperation.IsDone)
                    {
#if DEBUG
                        Log.Debug($"Finished loading asset '{loadOperation.AssetName}'");
#endif

                        loadOperations.RemoveAt(i);
                    }
                }

                yield return null;
            }
        }
    }
}
