using BepInEx.Bootstrap;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MirroredStageVariants.ModCompatibility
{
    static class RiskOfOptionsCompat
    {
        public static bool IsEnabled => Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AddConfigOptions()
        {
            const string MOD_GUID = Main.PluginGUID;
            const string MOD_NAME = "Mirrored Stage Variants";

            ModSettingsManager.AddOption(new StepSliderOption(Main.MirrorChance, new StepSliderConfig
            {
                formatString = "{0}%",
                min = 0f,
                max = 100f,
                increment = 0.1f
            }), MOD_GUID, MOD_NAME);

            ModSettingsManager.AddOption(new CheckBoxOption(Main.MirrorNonStages), MOD_GUID, MOD_NAME);
            ModSettingsManager.AddOption(new CheckBoxOption(Main.MirrorHiddenRealms), MOD_GUID, MOD_NAME);

            ModSettingsManager.SetModDescription($"Options for {MOD_NAME}", MOD_GUID, MOD_NAME);

            FileInfo iconFile = null;

            DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(Main.Instance.Info.Location));
            do
            {
                FileInfo[] files = dir.GetFiles("icon.png", SearchOption.TopDirectoryOnly);
                if (files != null && files.Length > 0)
                {
                    iconFile = files[0];
                    break;
                }

                dir = dir.Parent;
            } while (dir != null && dir.Exists && !string.Equals(dir.Name, "plugins", StringComparison.OrdinalIgnoreCase));

            if (iconFile != null)
            {
                Texture2D iconTexture = new Texture2D(256, 256);
                if (iconTexture.LoadImage(File.ReadAllBytes(iconFile.FullName)))
                {
                    Sprite iconSprite = Sprite.Create(iconTexture, new Rect(0f, 0f, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f));
                    iconSprite.name = $"{Main.PluginName}Icon";

                    ModSettingsManager.SetModIcon(iconSprite, MOD_GUID, MOD_NAME);
                }
            }
        }
    }
}
