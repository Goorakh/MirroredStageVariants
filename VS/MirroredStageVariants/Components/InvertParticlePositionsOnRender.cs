using MirroredStageVariants.Utils;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace MirroredStageVariants.Components
{
    [DisallowMultipleComponent]
    public class InvertParticlePositionsOnRender : MonoBehaviour
    {
        static Camera _manualRenderCamera;

        [SystemInitializer]
        static void Init()
        {
            GameObject manualRenderCameraObject = new GameObject("ManualRenderCamera");
            manualRenderCameraObject.AddComponent<SetDontDestroyOnLoad>();
            _manualRenderCamera = manualRenderCameraObject.AddComponent<Camera>();
            _manualRenderCamera.enabled = false;
        }

        readonly List<ApplyInvertShader> _ownedInvertAppliers = [];

        Camera _currentRenderingCamera;
        int _originalLayer;

        void OnEnable()
        {
            CameraEvents.OnPreCull += onPreCull;
            CameraEvents.OnPostRender += onPostRender;
        }

        void OnDisable()
        {
            CameraEvents.OnPreCull -= onPreCull;
            CameraEvents.OnPostRender -= onPostRender;

            if (_currentRenderingCamera)
            {
                gameObject.layer = _originalLayer;
                _currentRenderingCamera = null;
            }

            removeAllInvertAppliers();
        }

        void removeAllInvertAppliers()
        {
            foreach (ApplyInvertShader applyInvertShader in _ownedInvertAppliers)
            {
                applyInvertShader.Owner = null;
                Destroy(applyInvertShader);
            }

            _ownedInvertAppliers.Clear();
        }

        void onPreCull(Camera cam)
        {
            if (cam == _manualRenderCamera)
                return;

            if (!cam.ShouldRenderObjectAsMirrored(gameObject))
                return;

            if (_currentRenderingCamera)
            {
                if (_currentRenderingCamera != cam)
                {
                    Log.Warning($"Cannot swap UV position of {name} rendered from {cam}, already being rendered by {_currentRenderingCamera}");
                }

                return;
            }

            _currentRenderingCamera = cam;

            _originalLayer = gameObject.layer;
            gameObject.layer = LayerIndex.manualRender.intVal;
        }

        void onPostRender(Camera cam)
        {
            if (cam == _manualRenderCamera)
                return;

            if (_currentRenderingCamera != cam)
                return;

            ApplyInvertShader applyInvertShader = _ownedInvertAppliers.Find(a => a.Camera == cam);
            if (!applyInvertShader)
            {
                applyInvertShader = cam.gameObject.AddComponent<ApplyInvertShader>();
                applyInvertShader.Owner = this;
                _ownedInvertAppliers.Add(applyInvertShader);
            }

            _manualRenderCamera.CopyFrom(cam);
            _manualRenderCamera.transform.position = cam.transform.position;
            _manualRenderCamera.transform.rotation = cam.transform.rotation;

            _manualRenderCamera.cullingMask = LayerIndex.manualRender.mask;
            _manualRenderCamera.clearFlags = CameraClearFlags.Color;
            _manualRenderCamera.backgroundColor = Color.clear;

            _manualRenderCamera.targetTexture = applyInvertShader.TargetTexture;
            _manualRenderCamera.Render();

            gameObject.layer = _originalLayer;

            _currentRenderingCamera = null;
        }

        class ApplyInvertShader : MonoBehaviour
        {
            public InvertParticlePositionsOnRender Owner;

            public Camera Camera { get; private set; }

            public RenderTexture TargetTexture { get; private set; }
            public Material Material { get; private set; }

            void Awake()
            {
                Camera = GetComponent<Camera>();
                updateRenderTexture();

                Shader mirrorOverlay = Assets.MirrorOverlayShader;
                if (mirrorOverlay)
                {
                    Material = new Material(mirrorOverlay);

                    if (TargetTexture)
                    {
                        Material.SetTexture(CommonShaderIDs._OverlayTex, TargetTexture);
                    }
                }
            }

            void updateRenderTexture()
            {
                if (TargetTexture)
                {
                    if (Camera && TargetTexture.width == Camera.pixelWidth && TargetTexture.height == Camera.pixelHeight)
                        return;

                    RenderTexture.ReleaseTemporary(TargetTexture);
                    TargetTexture = null;
                }

                if (Camera)
                {
                    TargetTexture = RenderTexture.GetTemporary(new RenderTextureDescriptor(Camera.pixelWidth, Camera.pixelHeight, RenderTextureFormat.ARGB32)
                    {
                        sRGB = true,
                        useMipMap = false
                    });

                    TargetTexture.filterMode = FilterMode.Bilinear;

                    if (Material)
                    {
                        Material.SetTexture(CommonShaderIDs._OverlayTex, TargetTexture);
                    }
                }
            }

            void FixedUpdate()
            {
                updateRenderTexture();

                if (!Owner)
                {
                    Destroy(this);
                }
            }

            void OnDestroy()
            {
                if (Material)
                {
                    Destroy(Material);
                }

                if (TargetTexture)
                {
                    RenderTexture.ReleaseTemporary(TargetTexture);
                }
            }

            void OnRenderImage(RenderTexture source, RenderTexture destination)
            {
                if (Material)
                {
                    Graphics.Blit(source, destination, Material);
                }
                else
                {
                    Graphics.Blit(source, destination);
                }
            }
        }
    }
}
