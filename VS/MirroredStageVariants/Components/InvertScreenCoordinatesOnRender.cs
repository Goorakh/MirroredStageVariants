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
        static event Camera.CameraCallback onPreCullEvent;
        static event Camera.CameraCallback onPostRenderEvent;

        static InvertScreenCoordinatesOnRender()
        {
            Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, (Camera.CameraCallback)((Camera cam) => onPreCullEvent?.Invoke(cam)));
            Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, (Camera.CameraCallback)((Camera cam) => onPostRenderEvent?.Invoke(cam)));
        }

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
            onPreCullEvent += onPreCull;
            onPostRenderEvent += onPostRender;
        }

        void OnDisable()
        {
            restorePreRenderPosition();

            onPreCullEvent -= onPreCull;
            onPostRenderEvent -= onPostRender;
        }

        bool rendersThisObjectAsMirrored(Camera camera)
        {
            if (StageMirrorController.CurrentStageIsMirrored && (camera.cullingMask & (1 << gameObject.layer)) != 0)
            {
                if (camera.GetComponent<MirrorCamera>())
                {
                    return true;
                }

                CameraRigController cameraRigController;
                if (camera.TryGetComponent(out UICamera uiCamera))
                {
                    cameraRigController = uiCamera.cameraRigController;
                }
                else
                {
                    cameraRigController = camera.GetComponentInParent<CameraRigController>();
                }

                if (cameraRigController && cameraRigController.sceneCam && cameraRigController.sceneCam.GetComponent<MirrorCamera>())
                {
                    return true;
                }
            }

            return false;
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
            if (!rendersThisObjectAsMirrored(camera))
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
