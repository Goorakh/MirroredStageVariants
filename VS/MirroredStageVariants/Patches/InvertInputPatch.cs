using HarmonyLib;
using MirroredStageVariants.Components;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RoR2;
using System.Reflection;
using UnityEngine;

namespace MirroredStageVariants.Patches
{
    static class InvertInputPatch
    {
        [SystemInitializer]
        static void Init()
        {
            MethodInfo cameraRigEarlyUpdateMethod = typeof(CameraRigController).GetMethod(nameof(CameraRigController.EarlyUpdate), (BindingFlags)(~0b0));
            if (cameraRigEarlyUpdateMethod != null)
            {
                new Hook(cameraRigEarlyUpdateMethod, new On.RoR2.CameraRigController.hook_EarlyUpdate(CameraRigController_EarlyUpdate), new HookConfig
                {
                    Priority = 100,
                });
            }
            else
            {
                Log.Error("Failed to find method CameraRigController.EarlyUpdate");
            }

            IL.RoR2.PlayerCharacterMasterController.Update += PlayerCharacterMasterController_Update;
        }

        static void CameraRigController_EarlyUpdate(On.RoR2.CameraRigController.orig_EarlyUpdate orig, CameraRigController self)
        {
            // This profile can potentially be outdated since it is assigned inside of the orig() call,
            // but they delay will be at most a single update tick, so probably fine
            UserProfile userProfile = self.cameraModeContext.viewerInfo.userProfile;
            if (userProfile == null || !MirrorCamera.IsMirrored(self.sceneCam))
            {
                orig(self);
                return;
            }

            static bool invertAndReturnOriginal(ref bool value)
            {
                bool origValue = value;
                value = !value;
                return origValue;
            }

            bool origMouseLookInvertX = invertAndReturnOriginal(ref userProfile.mouseLookInvertX);
            bool origStickLookInvertX = invertAndReturnOriginal(ref userProfile.stickLookInvertX);
            bool origGyroLookInvertX = invertAndReturnOriginal(ref userProfile.gyroLookInvertX);

            using var _ = new InvertScreenCoordinatesPatch.DisableScope();

            try
            {
                orig(self);
            }
            finally
            {
                userProfile.mouseLookInvertX = origMouseLookInvertX;
                userProfile.stickLookInvertX = origStickLookInvertX;
                userProfile.gyroLookInvertX = origGyroLookInvertX;
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

                    cursor.Emit(OpCodes.Ldarg_0);
                    cursor.Emit(OpCodes.Ldloca, moveInputLocalIndex);
                    cursor.EmitDelegate(tryMirrorMoveInput);
                    static void tryMirrorMoveInput(PlayerCharacterMasterController playerMasterController, ref Vector2 moveInput)
                    {
                        if (!playerMasterController)
                            return;

                        NetworkUser networkUser = playerMasterController.networkUser;
                        if (!networkUser)
                            return;

                        CameraRigController cameraRigController = networkUser.cameraRigController;
                        if (!cameraRigController)
                            return;

                        if (MirrorCamera.IsMirrored(cameraRigController.sceneCam))
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
