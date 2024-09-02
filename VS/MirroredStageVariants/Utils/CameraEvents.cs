using System;
using UnityEngine;

namespace MirroredStageVariants.Utils
{
    public static class CameraEvents
    {
        public static event Camera.CameraCallback OnPreCull;
        public static event Camera.CameraCallback OnPreRender;
        public static event Camera.CameraCallback OnPostRender;

        static CameraEvents()
        {
            Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(cam => OnPreCull?.Invoke(cam)));
            Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(cam => OnPreRender?.Invoke(cam)));
            Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(cam => OnPostRender?.Invoke(cam)));
        }
    }
}
