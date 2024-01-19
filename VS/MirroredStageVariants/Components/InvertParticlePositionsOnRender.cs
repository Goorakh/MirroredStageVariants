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
        }

        ApplyUVPositionSwaps.SwapInfo[] getParticleSwapInfos(Camera cam)
        {
            if (!_particleSystem)
                return [];

            int particleCount = _particleSystem.particleCount;
            ArrayUtils.EnsureCapacity(ref _particlesBuffer, particleCount);

            _particleSystem.GetParticles(_particlesBuffer);

            Rect cameraRect = cam.rect;
            Rect cameraPixelRect = cam.pixelRect;

            List<ApplyUVPositionSwaps.SwapInfo> swaps = [];
            for (int i = 0; i < particleCount; i++)
            {
                ParticleSystem.Particle particle = _particlesBuffer[i];

                Vector3 position = particle.position;

                Vector3 screenPoint = cam.WorldToScreenPoint(position);

                Vector3 flippedScreenPoint = screenPoint;
                flippedScreenPoint.x = CoordinateUtils.GetInvertedScreenXCoordinate(flippedScreenPoint.x, cameraPixelRect);

                swaps.Add(new ApplyUVPositionSwaps.SwapInfo
                {
                    PositionA = CoordinateUtils.Remap(screenPoint, cameraPixelRect, cameraRect),
                    PositionB = CoordinateUtils.Remap(flippedScreenPoint, cameraPixelRect, cameraRect),
                    Size = CoordinateUtils.Remap(new Vector2(50f, 20f), cameraPixelRect, cameraRect)
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

            ApplyUVPositionSwaps manualRenderUVPositionSwaps = cam.GetComponent<ApplyUVPositionSwaps>() ?? cam.gameObject.AddComponent<ApplyUVPositionSwaps>();

            manualRenderUVPositionSwaps.SetSwapPositions(getParticleSwapInfos(cam));

            if (manualRenderUVPositionSwaps.Material)
            {
                _manualRenderCamera.CopyFrom(cam);
                _manualRenderCamera.transform.position = cam.transform.position;
                _manualRenderCamera.transform.rotation = cam.transform.rotation;

                _manualRenderCamera.cullingMask = LayerIndex.manualRender.mask;
                _manualRenderCamera.clearFlags = CameraClearFlags.Color;
                _manualRenderCamera.backgroundColor = Color.clear;

                _manualRenderCamera.targetTexture = manualRenderUVPositionSwaps.InputTexture;
                _manualRenderCamera.Render();
            }

            gameObject.layer = _originalLayer;

            _currentRenderingCamera = null;
        }

        class ApplyUVPositionSwaps : MonoBehaviour
        {
            public struct SwapInfo
            {
                public Vector2 PositionA;
                public Vector2 PositionB;

                public Vector2 Size;
            }

            public Camera Camera { get; private set; }

            public RenderTexture InputTexture { get; private set; }
            public Material Material { get; private set; }

            void Awake()
            {
                Camera = GetComponent<Camera>();
                updateRenderTexture();
            }

            void updateRenderTexture()
            {
                if (InputTexture)
                {
                    if (Camera && InputTexture.width == Camera.pixelWidth && InputTexture.height == Camera.pixelHeight)
                        return;

                    RenderTexture.ReleaseTemporary(InputTexture);
                    InputTexture = null;
                }

                if (Camera)
                {
                    InputTexture = RenderTexture.GetTemporary(new RenderTextureDescriptor(Camera.pixelWidth, Camera.pixelHeight, RenderTextureFormat.ARGB32)
                    {
                        sRGB = true,
                        useMipMap = false
                    });

                    InputTexture.filterMode = FilterMode.Bilinear;

                    if (Material)
                    {
                        Material.SetTexture(CommonShaderIDs._InputTex, InputTexture);
                    }
                }
            }

            void FixedUpdate()
            {
                updateRenderTexture();
            }

            public void SetSwapPositions(SwapInfo[] swaps)
            {
                if (!Material)
                {
                    Shader shader = Main.Instance ? Main.Instance.SwapRectsShader : null;
                    if (!shader)
                        return;

                    Material = new Material(shader);

                    if (InputTexture)
                    {
                        Material.SetTexture(CommonShaderIDs._InputTex, InputTexture);
                    }
                }

                const int SWAP_POSITIONS_MAX_SIZE = 100;

                int swapCount = Mathf.Min(SWAP_POSITIONS_MAX_SIZE, swaps.Length);

                Vector4[] swapPositions = new Vector4[SWAP_POSITIONS_MAX_SIZE];
                Vector4[] swapSizes = new Vector4[SWAP_POSITIONS_MAX_SIZE];

                for (int i = 0; i < swapCount; i++)
                {
                    SwapInfo swapInfo = swaps[i];

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

                if (InputTexture)
                {
                    RenderTexture.ReleaseTemporary(InputTexture);
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
