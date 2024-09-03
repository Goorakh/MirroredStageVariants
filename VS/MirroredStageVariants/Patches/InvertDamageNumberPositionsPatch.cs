using MirroredStageVariants.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MirroredStageVariants.Patches
{
    static class InvertDamageNumberPositionsPatch
    {
        static readonly List<MonoBehaviour> _addedComponents = [];

        public static void Apply()
        {
            AsyncOperationHandle<GameObject> damageNumberManagerLoad = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Core/DamageNumberManager.prefab");
            damageNumberManagerLoad.Completed += handle =>
            {
                GameObject damageNumberManager = handle.Result;

                FlipParticleSystemIfMirrored flipParticleSystemComponent = damageNumberManager.AddComponent<FlipParticleSystemIfMirrored>();

                _addedComponents.Add(flipParticleSystemComponent);
            };

            AsyncOperationHandle<GameObject> critGlassesVoidExecuteEffectLoad = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/CritGlassesVoid/CritGlassesVoidExecuteEffect.prefab");
            critGlassesVoidExecuteEffectLoad.Completed += handle =>
            {
                GameObject effectPrefab = handle.Result;

                Transform fakeDamageNumbersTransform = effectPrefab.transform.Find("FakeDamageNumbers");
                if (fakeDamageNumbersTransform)
                {
                    FlipParticleSystemIfMirrored flipParticleSystemComponent = fakeDamageNumbersTransform.gameObject.AddComponent<FlipParticleSystemIfMirrored>();

                    _addedComponents.Add(flipParticleSystemComponent);
                }
                else
                {
                    Log.Error("Failed to find fake damage number emitter on CritGlassesVoidExecuteEffect");
                }
            };
        }

        public static void Undo()
        {
            foreach (MonoBehaviour component in _addedComponents)
            {
                GameObject.Destroy(component);
            }

            _addedComponents.Clear();
        }
    }
}
