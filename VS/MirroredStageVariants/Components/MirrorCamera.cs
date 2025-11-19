using MirroredStageVariants.Utils;
using RoR2;
using System;
using UnityEngine;

namespace MirroredStageVariants.Components
{
    public sealed class MirrorCamera : MonoBehaviour
    {
        [SystemInitializer]
        static void Init()
        {
            On.RoR2.SceneCamera.Awake += (orig, self) =>
            {
                orig(self);
                self.gameObject.AddComponent<MirrorCamera>();
            };
        }

        public static event Action OnMirrorTransformationChanged;

        Matrix4x4 _mirrorAroundCameraTransformation = Matrix4x4.identity;
        public Matrix4x4 MirrorAroundCameraTransformation
        {
            get
            {
                return _mirrorAroundCameraTransformation;
            }
            private set
            {
                if (_mirrorAroundCameraTransformation.Equals(value))
                    return;

                _mirrorAroundCameraTransformation = value;
                OnMirrorTransformationChanged?.Invoke();
            }
        }

        void OnEnable()
        {
            recalculateMirrorTransformation();
        }

        void Update()
        {
            recalculateMirrorTransformation();
        }

        void recalculateMirrorTransformation()
        {
            if (StageMirrorController.CurrentlyIsMirrored)
            {
                MirrorAroundCameraTransformation = transform.GlobalTransformationFromLocal(Matrix4x4.Scale(new Vector3(-1f, 1f, 1f)));
            }
            else
            {
                MirrorAroundCameraTransformation = Matrix4x4.identity;
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Material mirrorMaterial = Assets.MirrorMaterial;
            if (StageMirrorController.CurrentlyIsMirrored && mirrorMaterial)
            {
                Graphics.Blit(source, destination, mirrorMaterial);
            }
            else
            {
                Graphics.Blit(source, destination);
            }
        }
    }
}
