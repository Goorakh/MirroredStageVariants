using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

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
            Run instance = Run.instance;

            if (instance)
            {
                ulong seed = NetworkServer.active && NetworkServer.dontListen ? instance.seed
                    : unchecked((ulong)instance.NetworkstartTimeUtc._binaryValue);

                seed ^= Hash128.Compute(SceneCatalog.mostRecentSceneDef.cachedName).u64_0;
                rng = new(seed);

                for (int i = 0; i < instance.stageClearCount; i++)
                {
                    rng.Next();
                }
            }

            IsMirrored = rng.nextNormalizedFloat <= Main.MirrorChance.Value / 100f;

#if DEBUG
            Log.Debug($"mirrored={IsMirrored}");
#endif
        }
    }
}
