using MirroredStageVariants.Components;
using RoR2;

namespace MirroredStageVariants.Patches
{
    static class InvertDamageNumberPositionsPatch
    {
        public static void Apply()
        {
            On.RoR2.DamageNumberManager.Awake += DamageNumberManager_Awake;
        }

        public static void Undo()
        {
            On.RoR2.DamageNumberManager.Awake -= DamageNumberManager_Awake;
        }

        static void DamageNumberManager_Awake(On.RoR2.DamageNumberManager.orig_Awake orig, DamageNumberManager self)
        {
            orig(self);
            self.gameObject.AddComponent<InvertParticlePositionsOnRender>();
        }
    }
}
