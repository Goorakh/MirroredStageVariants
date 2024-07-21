using RoR2;
using UnityEngine;

namespace MirroredStageVariants.Components
{
    public class StageMirrorController : MonoBehaviour
    {
        [SystemInitializer]
        static void Init()
        {
            On.RoR2.Stage.OnEnable += (orig, self) =>
            {
                orig(self);

                if (!self.GetComponent<StageMirrorController>())
                {
                    self.gameObject.AddComponent<StageMirrorController>();
                }
            };
        }

        static StageMirrorController _instance;

        public static bool CanMirrorScene(SceneDef scene)
        {
            if (!scene)
                return false;

            switch (scene.cachedName)
            {
                case "lobby":
                case "logbook":
                    return false;
            }

            if (Main.MirrorHiddenRealms.Value)
                return true;

            return scene.stageOrder > 0 && scene.stageOrder <= Run.stagesPerLoop;
        }

        public static bool CurrentlyIsMirrored
        {
            get
            {
                if (!CanMirrorScene(SceneCatalog.mostRecentSceneDef))
                    return false;

                if (!_instance)
                    return Main.MirrorNonStages.Value;

                if (Commands.OverrideStageIsMirrored.HasValue)
                    return Commands.OverrideStageIsMirrored.Value;

                return _instance._isMirrored;
            }
        }

        bool _isMirrored;

        void OnEnable()
        {
            SingletonHelper.Assign(ref _instance, this);
        }

        void OnDisable()
        {
            SingletonHelper.Unassign(ref _instance, this);
        }

        void Awake()
        {
            var rng = Run.instance ? new(Run.instance.stageRng) : RoR2Application.rng;
            _isMirrored = rng.nextNormalizedFloat <= Main.MirrorChance.Value / 100f;

#if DEBUG
            Log.Debug($"mirrored={_isMirrored}");
#endif
        }
    }
}
