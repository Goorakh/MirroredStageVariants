using HarmonyLib;
using MirroredStageVariants.Components;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using RoR2.UI;
using System;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace MirroredStageVariants.Patches
{
    static class InvertTypewriteTextControllerPatch
    {
        static ILHook TypewriteTextController_SetTypingTime_UpdateCurrentLabel_Hook;

        public static void Apply()
        {
            On.RoR2.UI.AssignStageToken.Start += AssignStageToken_Start;
            On.RoR2.UI.TypewriteTextController.SetTypingTime += TypewriteTextController_SetTypingTime;

            MethodInfo TypewriteTextController_SetTypingTime_UpdateCurrentLabel_MI = AccessTools.GetDeclaredMethods(typeof(TypewriteTextController)).FirstOrDefault(m => m.Name.StartsWith("<SetTypingTime>g__UpdateCurrentLabel|"));

            if (TypewriteTextController_SetTypingTime_UpdateCurrentLabel_MI is not null)
            {
                TypewriteTextController_SetTypingTime_UpdateCurrentLabel_Hook = new ILHook(TypewriteTextController_SetTypingTime_UpdateCurrentLabel_MI, TypewriteTextController_SetTypingTime_UpdateCurrentLabel);
            }
            else
            {
                Log.Error("Failed to find local function UpdateCurrentLabel");
            }
        }

        public static void Undo()
        {
            On.RoR2.UI.AssignStageToken.Start -= AssignStageToken_Start;
            On.RoR2.UI.TypewriteTextController.SetTypingTime -= TypewriteTextController_SetTypingTime;

            TypewriteTextController_SetTypingTime_UpdateCurrentLabel_Hook?.Dispose();
            TypewriteTextController_SetTypingTime_UpdateCurrentLabel_Hook = null;
        }

        static void AssignStageToken_Start(On.RoR2.UI.AssignStageToken.orig_Start orig, AssignStageToken self)
        {
            InvertTypewriterInfo invertTypewriterInfo = self.gameObject.AddComponent<InvertTypewriterInfo>();
            invertTypewriterInfo.InvertedLabels = [self.titleText, self.subtitleText];

            orig(self);
        }

        static void TypewriteTextController_SetTypingTime(On.RoR2.UI.TypewriteTextController.orig_SetTypingTime orig, TypewriteTextController self, float typingTime)
        {
            InvertTypewriterInfo.Current = self.GetComponent<InvertTypewriterInfo>();
            try
            {
                orig(self, typingTime);
            }
            finally
            {
                InvertTypewriterInfo.Current = null;
            }
        }

        static void TypewriteTextController_SetTypingTime_UpdateCurrentLabel(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ParameterDefinition localsParameter = null;
            TypeDefinition resolvedLocalsType = null;

            foreach (ParameterDefinition parameter in il.Method.Parameters)
            {
                TypeReference typeRef = parameter.ParameterType.GetElementType();

                if (!typeRef.Name.StartsWith("<>c__DisplayClass"))
                    continue;

                if (typeRef.DeclaringType == null || !typeRef.DeclaringType.Is(typeof(TypewriteTextController)))
                    continue;

                TypeDefinition resolvedType = typeRef.SafeResolve();
                if (resolvedType == null)
                    continue;

                resolvedLocalsType = resolvedType;
                localsParameter = parameter;

                break;
            }

            if (localsParameter == null)
            {
                Log.Error("Failed to find locals parameter");
                return;
            }

            FieldDefinition currentLabelField = resolvedLocalsType.FindField("currentLabel");
            if (currentLabelField == null || !currentLabelField.FieldType.Is(typeof(TextMeshProUGUI)))
            {
                Log.Error("Failed to find currentLabel local field");
                return;
            }

            if (c.TryGotoNext(MoveType.Before,
                              x => x.MatchCallOrCallvirt(AccessTools.DeclaredPropertySetter(typeof(TMP_Text), nameof(TMP_Text.maxVisibleCharacters)))))
            {
                ILLabel applyMirroredVisibleCharactersCountLabel = c.DefineLabel();
                ILLabel afterPatchLabel = c.DefineLabel();

                c.Emit(OpCodes.Ldarg, localsParameter);
                c.Emit(OpCodes.Ldfld, currentLabelField);
                c.EmitDelegate((TextMeshProUGUI label) =>
                {
                    return StageMirrorController.CurrentlyIsMirrored &&
                           InvertTypewriterInfo.Current &&
                           Array.IndexOf(InvertTypewriterInfo.Current.InvertedLabels, label) != -1;
                });

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

        [DisallowMultipleComponent]
        class InvertTypewriterInfo : MonoBehaviour
        {
            public static InvertTypewriterInfo Current;

            public TextMeshProUGUI[] InvertedLabels;
        }
    }
}
