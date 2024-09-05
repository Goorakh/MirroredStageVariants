using RoR2;
using UnityEngine;

namespace MirroredStageVariants
{
    static class Commands
    {
        public static bool? OverrideStageIsMirrored { get; private set; }

        [ConCommand(commandName = "msv_force_all", helpText = "Sets or clears mirror override [mirrored/normal/random]")]
        static void CCOverrideStageIsMirrored(ConCommandArgs args)
        {
            if (args.Count < 1)
            {
                if (OverrideStageIsMirrored.HasValue)
                {
                    Debug.Log($"All stages forced {(OverrideStageIsMirrored.Value ? "mirrored" : "normal")}");
                }
                else
                {
                    Debug.Log("No active override");
                }

                return;
            }

            bool? newOverride = args.GetArgString(0).ToLower() switch
            {
                "mirrored" or "mirror" or "m" => true,
                "normal" or "n" => false,
                "random" or "r" or _ => null
            };

            if (OverrideStageIsMirrored != newOverride)
            {
                OverrideStageIsMirrored = newOverride;

                if (OverrideStageIsMirrored.HasValue)
                {
                    Debug.Log($"All stages forced {(OverrideStageIsMirrored.Value ? "mirrored" : "normal")}");
                }
                else
                {
                    Debug.Log("All stages mirrored override cleared");
                }
            }
        }
    }
}
