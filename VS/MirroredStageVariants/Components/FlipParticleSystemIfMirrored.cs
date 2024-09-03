using UnityEngine;

namespace MirroredStageVariants.Components
{
    [DisallowMultipleComponent]
    public class FlipParticleSystemIfMirrored : MonoBehaviour
    {
        ParticleSystemRenderer _particleRenderer;

        bool _isFlipped;

        void Awake()
        {
            _particleRenderer = GetComponent<ParticleSystemRenderer>();
        }

        void OnEnable()
        {
            updateIsFlipped();
        }

        void FixedUpdate()
        {
            updateIsFlipped();
        }

        void updateIsFlipped()
        {
            bool shouldBeFlipped = StageMirrorController.CurrentlyIsMirrored;
            setFlipped(shouldBeFlipped);
        }

        void setFlipped(bool flipped)
        {
            if (_isFlipped == flipped)
                return;

            _isFlipped = flipped;

            if (_particleRenderer)
            {
                Vector3 flip = _particleRenderer.flip;
                flip.x = _isFlipped ? 1f : 0f;
                _particleRenderer.flip = flip;
            }
        }
    }
}
