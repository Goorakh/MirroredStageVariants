using RoR2;
using System;
using UnityEngine;

namespace MirroredStageVariants
{
    static class Commands
    {
        public static bool? OverrideStageIsMirrored { get; private set; }

        [ConCommand(commandName = "msv_force_all_stages_mirrored", helpText = "Sets or clears mirror override [0/1/clear]")]
        static void CCOverrideStageIsMirrored(ConCommandArgs args)
        {
            args.CheckArgumentCount(1);

            bool? newOverride = args.TryGetArgBool(0);
            if (!newOverride.HasValue)
            {
                string argString = args.GetArgString(0);
                if (string.Equals(argString, bool.TrueString, StringComparison.OrdinalIgnoreCase))
                {
                    newOverride = true;
                }
                else if (string.Equals(argString, bool.FalseString, StringComparison.OrdinalIgnoreCase))
                {
                    newOverride = false;
                }
            }

            if (OverrideStageIsMirrored != newOverride)
            {
                OverrideStageIsMirrored = newOverride;

                if (OverrideStageIsMirrored.HasValue)
                {
                    Debug.Log($"All stages mirrored forced {(OverrideStageIsMirrored.Value ? "enabled" : "disabled")}");
                }
                else
                {
                    Debug.Log("All stages mirrored override cleared");
                }
            }
        }
    }
}
