using MirroredStageVariants.Components;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using UnityEngine;

namespace MirroredStageVariants.Patches
{
    static class InvertDamageNumberPositionsPatch
    {
        public static void Apply()
        {
            On.RoR2.DamageNumberManager.Awake += DamageNumberManager_Awake;
            IL.RoR2.DamageNumberManager.SpawnDamageNumber += DamageNumberManager_SpawnDamageNumber;
        }

        public static void Undo()
        {
            On.RoR2.DamageNumberManager.Awake -= DamageNumberManager_Awake;
            IL.RoR2.DamageNumberManager.SpawnDamageNumber -= DamageNumberManager_SpawnDamageNumber;
        }

        static void DamageNumberManager_Awake(On.RoR2.DamageNumberManager.orig_Awake orig, DamageNumberManager self)
        {
            orig(self);
            self.gameObject.AddComponent<InvertParticlePositionsOnRender>();
        }

        static void DamageNumberManager_SpawnDamageNumber(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            int emitParamsLocalIndex = -1;
            if (c.TryGotoNext(MoveType.After,
                              x => x.MatchLdloca(out emitParamsLocalIndex),
                              x => x.MatchInitobj<ParticleSystem.EmitParams>()))
            {
                c.Emit(OpCodes.Ldloca, emitParamsLocalIndex);
                c.EmitDelegate((ref ParticleSystem.EmitParams emitParams) =>
                {
                    if (StageMirrorController.CurrentlyIsMirrored)
                    {
                        emitParams.rotation3D = (Quaternion.Euler(emitParams.rotation3D) * Quaternion.Euler(0f, 180f, 0f)).eulerAngles;
                    }
                });
            }
        }
    }
}
