using MirroredStageVariants.Components;
using RoR2;

namespace MirroredStageVariants.Patches
{
    static class InvertDamageNumberPositionsPatch
    {
        public static void Apply()
        {
            On.RoR2.DamageNumberManager.Awake += DamageNumberManager_Awake;

            Main.DamageNumberInvertMode.SettingChanged += DamageNumberInvertMode_SettingChanged;
        }

        public static void Undo()
        {
            On.RoR2.DamageNumberManager.Awake -= DamageNumberManager_Awake;

            Main.DamageNumberInvertMode.SettingChanged -= DamageNumberInvertMode_SettingChanged;
        }

        static void DamageNumberManager_Awake(On.RoR2.DamageNumberManager.orig_Awake orig, DamageNumberManager self)
        {
            orig(self);

            InvertParticlePositionsOnRender invertParticlePositionsOnRender = self.gameObject.AddComponent<InvertParticlePositionsOnRender>();
            invertParticlePositionsOnRender.Mode = Main.DamageNumberInvertMode.Value;
        }

        static void DamageNumberInvertMode_SettingChanged(object sender, System.EventArgs e)
        {
            if (!DamageNumberManager.instance)
                return;

            if (DamageNumberManager.instance.TryGetComponent(out InvertParticlePositionsOnRender invertParticlePositionsOnRender))
            {
                invertParticlePositionsOnRender.Mode = Main.DamageNumberInvertMode.Value;
            }
        }
    }
}
