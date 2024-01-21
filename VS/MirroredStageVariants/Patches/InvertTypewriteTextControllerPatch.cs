using HarmonyLib;
using MirroredStageVariants.Components;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RoR2.UI;
using System.Linq;
using System.Reflection;
using TMPro;

namespace MirroredStageVariants.Patches
{
    static class InvertTypewriteTextControllerPatch
    {
        static ILHook TypewriteTextController_SetTypingTime_UpdateCurrentLabel_Hook;

        public static void Apply()
        {
            MethodInfo TypewriteTextController_SetTypingTime_UpdateCurrentLabel_MI = AccessTools.GetDeclaredMethods(typeof(TypewriteTextController)).FirstOrDefault(m => m.Name.StartsWith("<SetTypingTime>g__UpdateCurrentLabel|"));

            if (TypewriteTextController_SetTypingTime_UpdateCurrentLabel_MI is not null)
            {
                TypewriteTextController_SetTypingTime_UpdateCurrentLabel_Hook = new ILHook(TypewriteTextController_SetTypingTime_UpdateCurrentLabel_MI, TypewriteTextController_SetTypingTime);
            }
            else
            {
                Log.Error("Failed to find local function UpdateCurrentLabel");
            }
        }

        public static void Undo()
        {
            TypewriteTextController_SetTypingTime_UpdateCurrentLabel_Hook?.Undo();
        }

        static void TypewriteTextController_SetTypingTime(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.Before,
                              x => x.MatchCallOrCallvirt(AccessTools.DeclaredPropertySetter(typeof(TMP_Text), nameof(TMP_Text.maxVisibleCharacters)))))
            {
                ILLabel applyMirroredVisibleCharactersCountLabel = c.DefineLabel();
                ILLabel afterPatchLabel = c.DefineLabel();

                c.EmitDelegate(() => StageMirrorController.CurrentlyIsMirrored);
                c.Emit(OpCodes.Brtrue, applyMirroredVisibleCharactersCountLabel);

                c.Index++;

                c.Emit(OpCodes.Br, afterPatchLabel);

                c.MarkLabel(applyMirroredVisibleCharactersCountLabel);
                c.MoveAfterLabels();
                c.EmitDelegate((TMP_Text label, int maxVisibleCharacters) =>
                {
                    label.firstVisibleCharacter = label.text.Length - maxVisibleCharacters;
                });

                c.MarkLabel(afterPatchLabel);
            }
            else
            {
                Log.Error("Failed to find patch location");
            }
        }
    }
}
