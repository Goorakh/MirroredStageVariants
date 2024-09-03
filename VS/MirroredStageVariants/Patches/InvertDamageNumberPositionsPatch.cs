using MirroredStageVariants.Components;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MirroredStageVariants.Patches
{
    static class InvertDamageNumberPositionsPatch
    {
        public static void Apply()
        {
            IL.RoR2.DamageNumberManager.SpawnDamageNumber += DamageNumberManager_SpawnDamageNumber;
        }

        public static void Undo()
        {
            IL.RoR2.DamageNumberManager.SpawnDamageNumber -= DamageNumberManager_SpawnDamageNumber;
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

                c.EmitDelegate(tryMirrorParticle);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                void tryMirrorParticle(ref ParticleSystem.EmitParams emitParams)
                {
                    if (StageMirrorController.CurrentlyIsMirrored)
                    {
                        emitParams.rotation3D = (Quaternion.Euler(emitParams.rotation3D) * Quaternion.Euler(0f, 180f, 0f)).eulerAngles;
                    }
                }
            }
        }
    }
}
