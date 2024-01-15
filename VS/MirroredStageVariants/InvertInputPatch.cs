using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.CameraModes;
using System;
using System.Linq;
using UnityEngine;

namespace MirroredStageVariants
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

            if (context.cameraInfo.sceneCam && context.cameraInfo.sceneCam.GetComponent<MirrorCamera>())
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
                              x => x.MatchCall(AccessTools.DeclaredConstructor(typeof(Vector2), new Type[] { typeof(float), typeof(float) }))))
            {
                ILCursor cursor = foundCursors[2];
                int patchIndex = cursor.Index + 1;

                int moveInputLocalIndex = -1;
                if (cursor.TryGotoPrev(x => x.MatchLdloca(out moveInputLocalIndex)))
                {
                    cursor.Index = patchIndex;

                    cursor.Emit(OpCodes.Ldarg_0);
                    cursor.Emit(OpCodes.Ldloca, moveInputLocalIndex);
                    cursor.EmitDelegate((PlayerCharacterMasterController playerController, ref Vector2 moveInput) =>
                    {
                        CharacterMaster master = playerController.master;
                        if (!master)
                            return;

                        CharacterBody body = master.GetBody();
                        if (!body)
                            return;

                        CameraRigController cameraRigController = CameraRigController.readOnlyInstancesList.FirstOrDefault(c => c.targetBody == body);
                        if (cameraRigController.sceneCam && cameraRigController.sceneCam.GetComponent<MirrorCamera>())
                        {
                            moveInput.x *= -1f;
                        }
                    });
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
