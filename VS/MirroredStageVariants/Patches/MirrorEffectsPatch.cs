using MirroredStageVariants.Components;
using RoR2;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MirroredStageVariants.Patches
{
    static class MirrorEffectsPatch
    {
        [SystemInitializer(typeof(EffectCatalog))]
        static void Init()
        {
            List<TMP_Text> textLabelsBuffer = [];

            for (EffectIndex i = 0; i < (EffectIndex)EffectCatalog.effectCount; i++)
            {
                EffectDef effect = EffectCatalog.GetEffectDef(i);
                if (effect is null)
                    continue;

                GameObject effectPrefab = effect.prefab;
                if (!effectPrefab)
                    continue;

                textLabelsBuffer.Clear();
                effectPrefab.GetComponentsInChildren(textLabelsBuffer);

                foreach (TMP_Text textLabel in textLabelsBuffer)
                {
                    if (textLabel.gameObject.layer == LayerIndex.uiWorldSpace.intVal)
                    {
                        Log.Debug($"UI Worldspace text: {Util.BuildPrefabTransformPath(effect.prefab.transform, textLabel.transform, false, true)} ({i})");

                        if (!textLabel.GetComponent<MirrorTextLabelIfMirrored>())
                        {
                            textLabel.gameObject.AddComponent<MirrorTextLabelIfMirrored>();
                        }
                    }
                }
            }
        }
    }
}
