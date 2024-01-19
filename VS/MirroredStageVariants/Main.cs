using BepInEx;
using HG.Reflection;
using MirroredStageVariants.Patches;
using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace MirroredStageVariants
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Gorakh";
        public const string PluginName = "MirroredStageVariants";
        public const string PluginVersion = "1.0.0";

        internal static Main Instance { get; private set; }

        public Material MirrorMaterial { get; private set; }

        public Shader SwapRectsShader { get; private set; }

#if DEBUG
        public Material DebugDrawUV { get; private set; }
#endif

        void Awake()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Log.Init(Logger);

            Instance = SingletonHelper.Assign(Instance, this);

            loadAssets();

            MirrorAudioPatch.Apply();
            InvertScreenCoordinatesPatch.Apply();
            InvertInputPatch.Apply();
            InvertScreenBlurPatch.Apply();
            InvertDamageNumberPositionsPatch.Apply();

            stopwatch.Stop();
            Log.Info_NoCallerPrefix($"Initialized in {stopwatch.Elapsed.TotalSeconds:F2} seconds");
        }

        void OnDestroy()
        {
            MirrorAudioPatch.Undo();
            InvertScreenCoordinatesPatch.Undo();
            InvertInputPatch.Undo();
            InvertScreenBlurPatch.Undo();
            InvertDamageNumberPositionsPatch.Undo();

            Instance = SingletonHelper.Unassign(Instance, this);
        }

        void loadAssets()
        {
            string assetBundlePath = Path.Combine(Path.GetDirectoryName(Info.Location), "assets");
            if (!File.Exists(assetBundlePath))
            {
                Log.Error($"Assets file not found, expected path: {assetBundlePath}");
                return;
            }

            AssetBundle assetBundle;
            try
            {
                assetBundle = AssetBundle.LoadFromFile(assetBundlePath);
            }
            catch (Exception e)
            {
                Log.Error_NoCallerPrefix(e);
                return;
            }

            if (!assetBundle)
                return;

            MirrorMaterial = assetBundle.LoadAsset<Material>("Mirror");
            SwapRectsShader = assetBundle.LoadAsset<Shader>("SwapRects");

#if DEBUG
            DebugDrawUV = assetBundle.LoadAsset<Material>("DebugDrawUV");
#endif
        }
    }
}
