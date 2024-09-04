using HarmonyLib;
using MirroredStageVariants.Components;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.CameraModes;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MirroredStageVariants.Patches
{
    static class InvertInputPatch
    {
        public static void Apply()
        {
            On.RoR2.CameraModes.CameraModeBase.CollectLookInput += CameraModeBase_CollectLookInput;

            IL.RoR2.PlayerCharacterMasterController.Update += PlayerCharacterMasterController_Update;
        }

        public static void Undo()
        {
            On.RoR2.CameraModes.CameraModeBase.CollectLookInput -= CameraModeBase_CollectLookInput;

            IL.RoR2.PlayerCharacterMasterController.Update -= PlayerCharacterMasterController_Update;
        }

        static void CameraModeBase_CollectLookInput(On.RoR2.CameraModes.CameraModeBase.orig_CollectLookInput orig, CameraModeBase self, ref CameraModeBase.CameraModeContext context, out CameraModeBase.CollectLookInputResult result)
        {
            orig(self, ref context, out result);

            if (StageMirrorController.CurrentlyIsMirrored)
            {
                result.lookInput.x *= -1f;
            }
        }

        static void PlayerCharacterMasterController_Update(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILCursor[] foundCursors;
            if (c.TryFindNext(out foundCursors,
                              x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo<Rewired.Player>(_ => _.GetAxis(default(int)))),
                              x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo<Rewired.Player>(_ => _.GetAxis(default(int)))),
                              x => x.MatchCall(AccessTools.DeclaredConstructor(typeof(Vector2), [typeof(float), typeof(float)]))))
            {
                ILCursor cursor = foundCursors[2];
                int patchIndex = cursor.Index + 1;

                int moveInputLocalIndex = -1;
                if (cursor.TryGotoPrev(x => x.MatchLdloca(out moveInputLocalIndex)))
                {
                    cursor.Index = patchIndex;

                    if (!cursor.TryGotoNext(MoveType.After,
                                            x => x.MatchCallOrCallvirt<InputBankTest>(nameof(InputBankTest.SetRawMoveStates))))
                    {
                        Log.Error("Failed to find raw move state call");
                    }

                    cursor.Emit(OpCodes.Ldloca, moveInputLocalIndex);
                    cursor.EmitDelegate(tryMirrorMoveInput);
                    static void tryMirrorMoveInput(ref Vector2 moveInput)
                    {
                        if (StageMirrorController.CurrentlyIsMirrored)
                        {
                            moveInput.x *= -1f;
                        }
                    }
                }
                else
                {
                    Log.Error("Failed to find move input local index");
                }
            }
            else
            {
                Log.Error("Failed to find patch location");
            }
        }
    }
}
