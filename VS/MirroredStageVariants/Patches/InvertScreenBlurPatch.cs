using HarmonyLib;
using LeTai.Asset.TranslucentImage;
using MirroredStageVariants.Components;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using UnityEngine;

namespace MirroredStageVariants.Patches
{
    static class InvertScreenBlurPatch
    {
        static ILHook TranslucentImageSource_ProgressiveBlur_Hook;

        public static void Apply()
        {
#pragma warning disable Publicizer001 // Accessing a member that was not originally public
            TranslucentImageSource_ProgressiveBlur_Hook = new ILHook(SymbolExtensions.GetMethodInfo<TranslucentImageSource>(_ => _.ProgressiveBlur(default)), TranslucentImageSource_ProgressiveBlur);
#pragma warning restore Publicizer001 // Accessing a member that was not originally public
        }

        public static void Undo()
        {
            TranslucentImageSource_ProgressiveBlur_Hook?.Undo();
        }

        delegate void orig_TranslucentImageSource_ProgressiveBlur(TranslucentImageSource self, RenderTexture sourceRt);

        static void TranslucentImageSource_ProgressiveBlur(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            // Loop around to last instruction
            c.Index--;

            if (c.TryGotoPrev(x => x.MatchCall(SymbolExtensions.GetMethodInfo(() => Graphics.Blit(default, default, default(Material), default)))))
            {
                int temporaryTextureLocalIndex = -1;
                if (c.TryGotoPrev(MoveType.After,
                                  x => x.MatchLdloc(out temporaryTextureLocalIndex) && il.Body.Variables[temporaryTextureLocalIndex].VariableType.Is(typeof(RenderTexture))))
                {
                    c.EmitDelegate((RenderTexture texture) =>
                    {
                        if (StageMirrorController.CurrentlyIsMirrored)
                        {
                            Material mirrorMaterial = Main.Instance ? Main.Instance.MirrorMaterial : null;
                            if (mirrorMaterial)
                            {
                                RenderTexture mirroredTexture = RenderTexture.GetTemporary(texture.width, texture.height, 0, texture.format);
                                mirroredTexture.filterMode = FilterMode.Bilinear;

                                Graphics.Blit(texture, mirroredTexture, mirrorMaterial);

                                RenderTexture.ReleaseTemporary(texture);
                                return mirroredTexture;
                            }
                        }

                        return texture;
                    });

                    c.Emit(OpCodes.Dup);
                    c.Emit(OpCodes.Stloc, temporaryTextureLocalIndex);
                }
                else
                {
                    Log.Error("Failed to find patch location");
                }
            }
            else
            {
                Log.Error("Failed to find Blit call");
            }
        }
    }
}
