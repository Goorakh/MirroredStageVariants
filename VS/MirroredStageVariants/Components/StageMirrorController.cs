using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace MirroredStageVariants.Components
{
    public sealed class StageMirrorController : MonoBehaviour
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
        public static StageMirrorController Instance => _instance;

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

            if (MirroredStageVariantsPlugin.MirrorHiddenRealms.Value)
                return true;

            return scene.stageOrder > 0 && scene.stageOrder <= 5;
        }

        public static bool CurrentlyIsMirrored
        {
            get
            {
                if (!CanMirrorScene(SceneCatalog.mostRecentSceneDef))
                    return false;

                if (!_instance)
                    return MirroredStageVariantsPlugin.MirrorNonStages.Value;

                if (Commands.OverrideStageIsMirrored.HasValue)
                    return Commands.OverrideStageIsMirrored.Value;

                return _instance.IsMirrored;
            }
        }

        public bool IsMirrored { get; private set; }

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
            Xoroshiro128Plus rng = RoR2Application.rng;

            if (Run.instance)
            {
                ulong seed = (ulong)(Run.instance.NetworkstartTimeUtc._binaryValue + Run.instance.stageClearCount);

                rng = new Xoroshiro128Plus(seed);
            }

            IsMirrored = rng.nextNormalizedFloat <= MirroredStageVariantsPlugin.MirrorChance.Value / 100f;

            Log.Debug($"mirrored={IsMirrored}");
        }
    }
}
