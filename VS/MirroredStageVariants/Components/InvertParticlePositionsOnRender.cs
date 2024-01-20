using HG;
using MirroredStageVariants.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MirroredStageVariants.Components
{
    [DisallowMultipleComponent]
    public class InvertParticlePositionsOnRender : MonoBehaviour
    {
        public enum InvertMode
        {
            Mirror,
            FlipPosition,
            DontMirror,
            DontRender
        }

        static ParticleSystem.Particle[] _particlesBuffer = [];

        static Camera _manualRenderCamera;

        [SystemInitializer]
        static void Init()
        {
            GameObject manualRenderCameraObject = new GameObject("ManualRenderCamera");
            manualRenderCameraObject.AddComponent<SetDontDestroyOnLoad>();
            _manualRenderCamera = manualRenderCameraObject.AddComponent<Camera>();
            _manualRenderCamera.enabled = false;
        }

        InvertMode _mode = InvertMode.DontRender;
        public InvertMode Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                if (_mode == value)
                    return;

                _mode = value;

                removeAllInvertAppliers();
                enabled = _mode != InvertMode.DontMirror;
            }
        }

        readonly List<ApplyInvertShader> _ownedInvertAppliers = [];

        ParticleSystem _particleSystem;

        Camera _currentRenderingCamera;
        int _originalLayer;

        void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

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

        ScreenPositionSwapInfo[] getParticleSwapInfos(Camera cam)
        {
            if (!_particleSystem)
                return [];

            int particleCount = _particleSystem.particleCount;
            ArrayUtils.EnsureCapacity(ref _particlesBuffer, particleCount);

            _particleSystem.GetParticles(_particlesBuffer);

            Rect cameraRect = cam.rect;
            Rect cameraPixelRect = cam.pixelRect;

            List<ScreenPositionSwapInfo> swaps = [];
            for (int i = 0; i < particleCount; i++)
            {
                ParticleSystem.Particle particle = _particlesBuffer[i];

                Vector3 position = particle.position;

                Vector3 screenPoint = cam.WorldToScreenPoint(position);

                Vector3 flippedScreenPoint = screenPoint;
                flippedScreenPoint.x = CoordinateUtils.GetInvertedScreenXCoordinate(flippedScreenPoint.x, cameraPixelRect);

                swaps.Add(new ScreenPositionSwapInfo
                {
                    PositionA = CoordinateUtils.Remap(screenPoint, cameraPixelRect, cameraRect),
                    PositionB = CoordinateUtils.Remap(flippedScreenPoint, cameraPixelRect, cameraRect),
                    Size = CoordinateUtils.Remap(new Vector2(75f, 30f), cameraPixelRect, cameraRect)
                });
            }

            return swaps.ToArray();
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

            if (Mode != InvertMode.DontRender)
            {
                ApplyInvertShader applyInvertShader = _ownedInvertAppliers.Find(a => a.Camera == cam);
                if (!applyInvertShader)
                {
                    applyInvertShader = cam.gameObject.AddComponent<ApplyInvertShader>();
                    applyInvertShader.Owner = this;
                    _ownedInvertAppliers.Add(applyInvertShader);
                }

                if (Mode == InvertMode.FlipPosition)
                {
                    applyInvertShader.SetSwapPositions(getParticleSwapInfos(cam));
                }

                _manualRenderCamera.CopyFrom(cam);
                _manualRenderCamera.transform.position = cam.transform.position;
                _manualRenderCamera.transform.rotation = cam.transform.rotation;

                _manualRenderCamera.cullingMask = LayerIndex.manualRender.mask;
                _manualRenderCamera.clearFlags = CameraClearFlags.Color;
                _manualRenderCamera.backgroundColor = Color.clear;

                _manualRenderCamera.targetTexture = applyInvertShader.TargetTexture;
                _manualRenderCamera.Render();
            }

            gameObject.layer = _originalLayer;

            _currentRenderingCamera = null;
        }

        public struct ScreenPositionSwapInfo
        {
            public Vector2 PositionA;
            public Vector2 PositionB;

            public Vector2 Size;
        }

        class ApplyInvertShader : MonoBehaviour
        {
            public InvertParticlePositionsOnRender Owner;

            public int InputTextureShaderID => Owner.Mode switch
            {
                InvertMode.Mirror => CommonShaderIDs._OverlayTex,
                InvertMode.FlipPosition => CommonShaderIDs._InputTex,
                _ => throw new NotImplementedException(),
            };

            public Camera Camera { get; private set; }

            public RenderTexture TargetTexture { get; private set; }
            public Material Material { get; private set; }

            void Awake()
            {
                Camera = GetComponent<Camera>();
                updateRenderTexture();
            }

            void Start()
            {
                if (Owner)
                {
                    switch (Owner.Mode)
                    {
                        case InvertMode.Mirror:
                            Shader mirrorOverlay = Main.Instance ? Main.Instance.MirrorOverlayShader : null;
                            if (mirrorOverlay)
                            {
                                Material = new Material(mirrorOverlay);
                            }

                            break;
                        case InvertMode.FlipPosition:
                            Shader flipPositionsShader = Main.Instance ? Main.Instance.SwapRectsShader : null;
                            if (flipPositionsShader)
                            {
                                Material = new Material(flipPositionsShader);
                            }

                            break;
                    }

                    if (Material && TargetTexture)
                    {
                        Material.SetTexture(InputTextureShaderID, TargetTexture);
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
                        Material.SetTexture(InputTextureShaderID, TargetTexture);
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

            public void SetSwapPositions(ScreenPositionSwapInfo[] swaps)
            {
                if (!Material)
                    return;

                const int SWAP_POSITIONS_MAX_SIZE = 100;

                int swapCount = Mathf.Min(SWAP_POSITIONS_MAX_SIZE, swaps.Length);

                Vector4[] swapPositions = new Vector4[SWAP_POSITIONS_MAX_SIZE];
                Vector4[] swapSizes = new Vector4[SWAP_POSITIONS_MAX_SIZE];

                for (int i = 0; i < swapCount; i++)
                {
                    ScreenPositionSwapInfo swapInfo = swaps[i];

                    swapPositions[i] = new Vector4(swapInfo.PositionA.x, swapInfo.PositionA.y, swapInfo.PositionB.x, swapInfo.PositionB.y);
                    swapSizes[i] = swapInfo.Size;
                }

                Material.SetInt(CommonShaderIDs._SwapCount, swapCount);
                Material.SetVectorArray(CommonShaderIDs._SwapPositions, swapPositions);
                Material.SetVectorArray(CommonShaderIDs._SwapSizes, swapSizes);
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
