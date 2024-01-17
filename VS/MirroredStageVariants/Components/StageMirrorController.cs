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

        public static bool CurrentStageIsMirrored => !_instance || (Commands.OverrideStageIsMirrored ?? _instance._isMirrored);

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
            _isMirrored = RoR2Application.rng.nextNormalizedFloat <= 0.5f;

#if DEBUG
            Log.Debug($"mirrored={_isMirrored}");
#endif
        }
    }
}
