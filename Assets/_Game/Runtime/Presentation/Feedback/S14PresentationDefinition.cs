using System;
using UnityEngine;

namespace Gravivore.Presentation.Feedback
{
    [CreateAssetMenu(fileName = "S14_Presentation", menuName = "Gravivore/S14 Presentation")]
    public sealed class S14PresentationDefinition : ScriptableObject
    {
        [Header("Pools")]
        [SerializeField, Min(1)] private int _hitPoolSize = 8;
        [SerializeField, Min(1)] private int _deathPoolSize = 6;
        [SerializeField, Min(1)] private int _assimilationPoolSize = 8;
        [SerializeField, Min(1)] private int _evolutionPoolSize = 2;

        [Header("Timing")]
        [SerializeField, Min(0.01f)] private float _hitDuration = 0.12f;
        [SerializeField, Min(0.01f)] private float _deathDuration = 0.3f;
        [SerializeField, Min(0.01f)] private float _assimilationDuration = 0.45f;
        [SerializeField, Min(0.01f)] private float _evolutionDuration = 0.55f;
        [SerializeField, Min(0.01f)] private float _lashWindupDuration = 0.15f;
        [SerializeField, Min(0.01f)] private float _lashBeamDuration = 0.08f;
        [SerializeField, Min(0.01f)] private float _lashImpactDuration = 0.12f;

        [Header("Colors")]
        [SerializeField] private Color _hitColor = new Color(1f, 0.85f, 0.35f, 1f);
        [SerializeField] private Color _deathColor = new Color(1f, 0.25f, 0.18f, 1f);
        [SerializeField] private Color _assimilationColor = new Color(0.2f, 0.95f, 0.85f, 1f);
        [SerializeField] private Color _evolutionColor = new Color(0.55f, 0.9f, 1f, 1f);
        [SerializeField] private Color _playerHitColor = new Color(1f, 0.35f, 0.25f, 1f);

        [Header("Generated project audio")]
        [SerializeField] private AudioClip _lashWindupClip;
        [SerializeField] private AudioClip _lashImpactClip;
        [SerializeField] private AudioClip _hitClip;
        [SerializeField] private AudioClip _deathClip;
        [SerializeField] private AudioClip _assimilationClip;
        [SerializeField] private AudioClip _evolutionClip;
        [SerializeField] private AudioClip _telegraphClip;
        [SerializeField] private AudioClip _bossImpactClip;

        [Header("Mech presentation only")]
        [SerializeField, Min(0.1f)] private float _mechStride = 1.15f;
        [SerializeField] private float _mechHipDegrees = 27f;
        [SerializeField] private float _mechKneeDegrees = 42f;
        [SerializeField] private float _mechFootLift = 0.09f;
        [SerializeField] private float _mechIdleDegrees = 0.6f;
        [SerializeField, Min(0.1f)] private float _stepMinimumInterval = 0.28f;
        [SerializeField] private AudioClip _stepClip;
        [SerializeField] private AudioClip _releaseClip;
        [SerializeField] private AudioClip _playerHitClip;
        [SerializeField] private AudioClip _playerDeathClip;

        public float MechStride => _mechStride;
        public float MechHipDegrees => _mechHipDegrees;
        public float MechKneeDegrees => _mechKneeDegrees;
        public float MechFootLift => _mechFootLift;
        public float MechIdleDegrees => _mechIdleDegrees;
        public float StepMinimumInterval => _stepMinimumInterval;
        public AudioClip StepClip => _stepClip != null ? _stepClip : _hitClip;
        public AudioClip ReleaseClip => _releaseClip != null ? _releaseClip : _lashImpactClip;
        public AudioClip PlayerHitClip => _playerHitClip != null ? _playerHitClip : _hitClip;
        public AudioClip PlayerDeathClip => _playerDeathClip != null ? _playerDeathClip : _deathClip;

        public int HitPoolSize => _hitPoolSize;
        public int DeathPoolSize => _deathPoolSize;
        public int AssimilationPoolSize => _assimilationPoolSize;
        public int EvolutionPoolSize => _evolutionPoolSize;
        public float HitDuration => _hitDuration;
        public float DeathDuration => _deathDuration;
        public float AssimilationDuration => _assimilationDuration;
        public float EvolutionDuration => _evolutionDuration;
        public float LashWindupDuration => _lashWindupDuration;
        public float LashBeamDuration => _lashBeamDuration;
        public float LashImpactDuration => _lashImpactDuration;
        public Color HitColor => _hitColor;
        public Color DeathColor => _deathColor;
        public Color AssimilationColor => _assimilationColor;
        public Color EvolutionColor => _evolutionColor;
        public Color PlayerHitColor => _playerHitColor;
        public AudioClip LashWindupClip => _lashWindupClip;
        public AudioClip LashImpactClip => _lashImpactClip;
        public AudioClip HitClip => _hitClip;
        public AudioClip DeathClip => _deathClip;
        public AudioClip AssimilationClip => _assimilationClip;
        public AudioClip EvolutionClip => _evolutionClip;
        public AudioClip TelegraphClip => _telegraphClip;
        public AudioClip BossImpactClip => _bossImpactClip;

        public void ValidateOrThrow()
        {
            if (_hitPoolSize < 1 || _deathPoolSize < 1 || _assimilationPoolSize < 1 || _evolutionPoolSize < 1)
                throw new InvalidOperationException("S14 presentation pool sizes must be positive.");
            ValidateDuration(_hitDuration, nameof(_hitDuration));
            ValidateDuration(_deathDuration, nameof(_deathDuration));
            ValidateDuration(_assimilationDuration, nameof(_assimilationDuration));
            ValidateDuration(_evolutionDuration, nameof(_evolutionDuration));
            ValidateDuration(_lashWindupDuration, nameof(_lashWindupDuration));
            ValidateDuration(_lashBeamDuration, nameof(_lashBeamDuration));
            ValidateDuration(_lashImpactDuration, nameof(_lashImpactDuration));
            if (_lashWindupClip == null || _lashImpactClip == null || _hitClip == null || _deathClip == null ||
                _assimilationClip == null || _evolutionClip == null || _telegraphClip == null || _bossImpactClip == null)
                throw new InvalidOperationException("S14 presentation requires every generated audio cue.");
        }

        private static void ValidateDuration(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
                throw new InvalidOperationException($"{name} must be finite and positive.");
        }
    }
}
