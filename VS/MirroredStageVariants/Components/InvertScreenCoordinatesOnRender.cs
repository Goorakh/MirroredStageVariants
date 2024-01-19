using MirroredStageVariants.Utils;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MirroredStageVariants.Components
{
    [DisallowMultipleComponent]
    public class InvertScreenCoordinatesOnRender : MonoBehaviour
    {
        [SystemInitializer]
        static void Init()
        {
            {
                GameObject bearProcEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bear/BearProc.prefab").WaitForCompletion();
                if (bearProcEffect)
                {
                    Transform textRoot = bearProcEffect.transform.Find("TextCamScaler");
                    if (textRoot)
                    {
                        textRoot.gameObject.AddComponent<InvertScreenCoordinatesOnRender>();
                    }
                    else
                    {
                        Log.Error($"Failed to find {bearProcEffect} text root");
                    }
                }
                else
                {
                    Log.Error($"Failed to load bear proc effect");
                }
            }

            {
                GameObject bearVoidProcEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/BearVoid/BearVoidProc.prefab").WaitForCompletion();
                if (bearVoidProcEffect)
                {
                    Transform textRoot = bearVoidProcEffect.transform.Find("TextCamScaler");
                    if (textRoot)
                    {
                        textRoot.gameObject.AddComponent<InvertScreenCoordinatesOnRender>();
                    }
                    else
                    {
                        Log.Error($"Failed to find {bearVoidProcEffect} text root");
                    }
                }
                else
                {
                    Log.Error($"Failed to load void bear proc effect");
                }
            }
        }

        Camera _currentRenderingCamera;
        Vector3 _preRenderPosition;

        void OnEnable()
        {
            CameraEvents.OnPreCull += onPreCull;
            CameraEvents.OnPostRender += onPostRender;
        }

        void OnDisable()
        {
            restorePreRenderPosition();

            CameraEvents.OnPreCull -= onPreCull;
            CameraEvents.OnPostRender -= onPostRender;
        }

        void storePreRenderPosition()
        {
            _preRenderPosition = transform.position;
        }

        void restorePreRenderPosition()
        {
            if (_currentRenderingCamera)
            {
                transform.position = _preRenderPosition;
            }
        }

        void onPreCull(Camera camera)
        {
            if (!camera.ShouldRenderObjectAsMirrored(gameObject))
                return;

            if (_currentRenderingCamera)
            {
                if (_currentRenderingCamera != camera)
                {
                    Log.Warning($"Cannot store position of {name} rendered from {camera}, already being rendered by {_currentRenderingCamera}");
                }

                return;
            }

            _currentRenderingCamera = camera;
            storePreRenderPosition();

            Vector3 viewportPoint = camera.WorldToViewportPoint(transform.position);
            CoordinateUtils.InvertScreenXCoordinate(ref viewportPoint.x, camera.rect);
            transform.position = camera.ViewportToWorldPoint(viewportPoint);
        }

        void onPostRender(Camera camera)
        {
            if (_currentRenderingCamera != camera)
                return;

            restorePreRenderPosition();
            _currentRenderingCamera = null;
        }
    }
}
