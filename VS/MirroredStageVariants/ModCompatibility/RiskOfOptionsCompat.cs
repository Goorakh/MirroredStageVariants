using BepInEx.Bootstrap;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;

namespace MirroredStageVariants.ModCompatibility
{
    static class RiskOfOptionsCompat
    {
        public static bool IsEnabled => Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");

        public static void AddConfigOptions()
        {
            const string MOD_GUID = Main.PluginGUID;
            const string MOD_NAME = "Mirrored Stage Variants";

            ModSettingsManager.SetModDescription($"Options for {MOD_NAME}", MOD_GUID, MOD_NAME);

            ModSettingsManager.AddOption(new StepSliderOption(Main.MirrorChance, new StepSliderConfig
            {
                formatString = "{0}%",
                min = 0f,
                max = 100f,
                increment = 0.1f
            }), MOD_GUID, MOD_NAME);

            ModSettingsManager.AddOption(new CheckBoxOption(Main.MirrorNonStages), MOD_GUID, MOD_NAME);
        }
    }
}
