using HarmonyLib;
using MirroredStageVariants.Components;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.CameraModes;
using System;
using UnityEngine;

namespace MirroredStageVariants.Patches
{
    static class InvertInputPatch
    {
        [SystemInitializer]
        static void Init()
        {
            On.RoR2.CameraModes.CameraModeBase.ApplyLookInput += CameraModeBase_ApplyLookInput;

            IL.RoR2.PlayerCharacterMasterController.Update += PlayerCharacterMasterController_Update;
        }

        private static void CameraModeBase_ApplyLookInput(On.RoR2.CameraModes.CameraModeBase.orig_ApplyLookInput orig, CameraModeBase self, ref CameraModeBase.CameraModeContext context, ref CameraModeBase.ApplyLookInputArgs args)
        {
            if (StageMirrorController.CurrentlyIsMirrored)
            {
                args.lookInput.x *= -1f;
            }
            orig(self, ref context, ref args);
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
