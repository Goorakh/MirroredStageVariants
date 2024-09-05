using BepInEx;
using BepInEx.Configuration;
using MirroredStageVariants.ModCompatibility;
using MirroredStageVariants.Patches;
using System.Diagnostics;

namespace MirroredStageVariants
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Gorakh";
        public const string PluginName = "MirroredStageVariants";
        public const string PluginVersion = "1.2.4";

        internal static Main Instance { get; private set; }

        public static ConfigEntry<float> MirrorChance { get; private set; }

        public static ConfigEntry<bool> MirrorNonStages { get; private set; }

        public static ConfigEntry<bool> MirrorHiddenRealms { get; private set; }

        void Awake()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Log.Init(Logger);

            Instance = SingletonHelper.Assign(Instance, this);

            initConfigs(Config);

            MirrorAudioPatch.Apply();
            InvertScreenCoordinatesPatch.Apply();
            InvertInputPatch.Apply();
            InvertScreenBlurPatch.Apply();
            InvertDamageNumberPositionsPatch.Apply();
            InvertTypewriteTextControllerPatch.Apply();
            HideStunEffect.Apply();

            stopwatch.Stop();
            Log.Message_NoCallerPrefix($"Initialized in {stopwatch.Elapsed.TotalSeconds:F2}s");
        }

        void OnDestroy()
        {
            MirrorAudioPatch.Undo();
            InvertScreenCoordinatesPatch.Undo();
            InvertInputPatch.Undo();
            InvertScreenBlurPatch.Undo();
            InvertDamageNumberPositionsPatch.Undo();
            InvertTypewriteTextControllerPatch.Undo();
            HideStunEffect.Undo();

            Instance = SingletonHelper.Unassign(Instance, this);
        }

        static void initConfigs(ConfigFile file)
        {
            MirrorChance = file.Bind("General", "Mirror Chance", 50f, new ConfigDescription("The percent chance that any given stage will be mirrored", new AcceptableValueRange<float>(0f, 100f)));

            MirrorNonStages = file.Bind("General", "Mirror Non-Stages", true, "If non-stage scenes (menu, cutscenes) should be mirrored, if enabled, they are always mirrored, if disabled, they are never mirrored.");

            MirrorHiddenRealms = file.Bind("General", "Mirror Hidden Realms", true, "If stages outside the normal stage one through five rotation can be mirrored.");

            if (RiskOfOptionsCompat.IsEnabled)
            {
                RiskOfOptionsCompat.AddConfigOptions();
            }
        }
    }
}
