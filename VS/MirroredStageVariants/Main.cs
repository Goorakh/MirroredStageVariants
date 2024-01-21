using BepInEx;
using BepInEx.Configuration;
using MirroredStageVariants.Components;
using MirroredStageVariants.ModCompatibility;
using MirroredStageVariants.Patches;
using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace MirroredStageVariants
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Gorakh";
        public const string PluginName = "MirroredStageVariants";
        public const string PluginVersion = "1.0.0";

        internal static Main Instance { get; private set; }

        public Material MirrorMaterial { get; private set; }

        public Shader SwapRectsShader { get; private set; }

        public Shader MirrorOverlayShader { get; private set; }

#if DEBUG
        public Material DebugDrawUV { get; private set; }
#endif

        public static ConfigEntry<float> MirrorChance { get; private set; }

        public static ConfigEntry<bool> MirrorNonStages { get; private set; }

        public static ConfigEntry<InvertParticlePositionsOnRender.InvertMode> DamageNumberInvertMode { get; private set; }

        void Awake()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Log.Init(Logger);

            Instance = SingletonHelper.Assign(Instance, this);

            initConfigs(Config);

            loadAssets();

            MirrorAudioPatch.Apply();
            InvertScreenCoordinatesPatch.Apply();
            InvertInputPatch.Apply();
            InvertScreenBlurPatch.Apply();
            InvertDamageNumberPositionsPatch.Apply();
            InvertTypewriteTextControllerPatch.Apply();

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
            InvertTypewriteTextControllerPatch.Undo();

            Instance = SingletonHelper.Unassign(Instance, this);
        }

        static void initConfigs(ConfigFile file)
        {
            MirrorChance = file.Bind("General", "Mirror Chance", 50f, new ConfigDescription("The percent chance that any given stage will be mirrored", new AcceptableValueRange<float>(0f, 100f)));

            MirrorNonStages = file.Bind("General", "Mirror Non-Stages", true, "If non-stage scenes (menu, cutscenes) should be mirrored, if enabled, they are always mirrored, if disabled, they are never mirrored.");

            DamageNumberInvertMode = file.Bind("General", "Damage Number Mirror Mode", InvertParticlePositionsOnRender.InvertMode.Mirror, new ConfigDescription($"How the damage numbers should be rendered when a stage is mirrored, this cannot be perfect due to some technical limitations (I'm not smart enough), all of them look bad in their own special way, so pick your poison:\n\n{nameof(InvertParticlePositionsOnRender.InvertMode.Mirror)}: Mirrors the entire numbers layer on the screen. Numbers will appear in the correct position, but the number itself will be flipped.\n\n{nameof(InvertParticlePositionsOnRender.InvertMode.FlipPosition)}: Attempts to only swap the positions of the damage numbers, horribly over-engineered and barely works, but I spent too much time making it to let it go to waste.\n\n{nameof(InvertParticlePositionsOnRender.InvertMode.DontMirror)}: Damage number aren't touched, numbers will be correct and readable, but will appear on the wrong side of the screen.\n\n{nameof(InvertParticlePositionsOnRender.InvertMode.DontRender)}: Turns off damage numbers entirely if the stage is mirrored."));

            if (RiskOfOptionsCompat.IsEnabled)
            {
                RiskOfOptionsCompat.AddConfigOptions();
            }
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
            MirrorOverlayShader = assetBundle.LoadAsset<Shader>("MirrorOverlay");

#if DEBUG
            DebugDrawUV = assetBundle.LoadAsset<Material>("DebugDrawUV");
#endif
        }
    }
}
