using UnityEngine;

namespace MirroredStageVariants.Utils
{
    static class CommonShaderIDs
    {
        public static readonly int _SwapCount = Shader.PropertyToID(nameof(_SwapCount));

        public static readonly int _SwapPositions = Shader.PropertyToID(nameof(_SwapPositions));

        public static readonly int _SwapSizes = Shader.PropertyToID(nameof(_SwapSizes));

        public static readonly int _InputTex = Shader.PropertyToID(nameof(_InputTex));

        public static readonly int _OverlayTex = Shader.PropertyToID(nameof(_OverlayTex));
    }
}
