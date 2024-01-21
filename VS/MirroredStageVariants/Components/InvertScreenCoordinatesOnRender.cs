using MirroredStageVariants.Utils;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MirroredStageVariants.Components
{
    [DisallowMultipleComponent]
    public class InvertScreenCoordinatesOnRender : MonoBehaviour
    {
        [SystemInitializer(typeof(EffectCatalog))]
        static void Init()
        {
            for (EffectIndex i = 0; i < (EffectIndex)EffectCatalog.effectCount; i++)
            {
                EffectDef effect = EffectCatalog.GetEffectDef(i);

                List<Transform> rootUIWorldSpaceObjects = [];

                foreach (Transform uiWorldSpaceObject in effect.prefab.transform.GetAllChildrenRecursive()
                                                                                .Where(t => t.gameObject.layer == LayerIndex.uiWorldSpace.intVal))
                {
                    if (!rootUIWorldSpaceObjects.Exists(uiWorldSpaceObject.IsChildOf))
                    {
                        for (int j = rootUIWorldSpaceObjects.Count - 1; j >= 0; j--)
                        {
                            if (rootUIWorldSpaceObjects[j].IsChildOf(uiWorldSpaceObject))
                            {
                                rootUIWorldSpaceObjects.RemoveAt(j);
                            }
                        }

                        rootUIWorldSpaceObjects.Add(uiWorldSpaceObject);
                    }
                }

                foreach (Transform root in rootUIWorldSpaceObjects)
                {
                    root.gameObject.AddComponent<InvertScreenCoordinatesOnRender>();

#if DEBUG
                    Log.Debug($"Added component to {Util.BuildPrefabTransformPath(effect.prefab.transform, root, false, true)} ({i})");
#endif
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
