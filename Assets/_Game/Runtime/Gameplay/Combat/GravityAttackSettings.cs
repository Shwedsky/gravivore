using System;
using UnityEngine;

namespace Gravivore.Gameplay.Combat
{
    [CreateAssetMenu(fileName = "GravityAttackSettings", menuName = "Gravivore/Combat/Gravity Attack Settings")]
    public sealed class GravityAttackSettings : ScriptableObject
    {
        [Header("Targeting")]
        [SerializeField, Min(0.1f)] private float _acquisitionRadius = 5f;
        [SerializeField, Min(0.1f)] private float _releaseRadius = 6f;
        [SerializeField, Min(0f)] private float _distanceWeight = 1f;
        [SerializeField, Min(0f)] private float _frontBiasWeight = 0.35f;
        [SerializeField, Min(0f)] private float _switchScoreAdvantage = 0.15f;
        [SerializeField, Min(0.01f)] private float _targetScanInterval = 0.1f;
        [SerializeField, Min(1)] private int _targetColliderCapacity = 32;
        [SerializeField] private LayerMask _targetLayers = ~0;
        [SerializeField] private LayerMask _hardBlockerLayers = 256;

        [Header("Gravity Pull")]
        [SerializeField, Min(0f)] private float _pullStopDistance = 1.2f;
        [SerializeField, Range(0f, 1f)] private float _elitePullFraction = 0.35f;
        [SerializeField, Min(0f)] private float _blockerClearance = 0.05f;

        [Header("Placeholder VFX")]
        [SerializeField, Min(1)] private int _vfxPoolSize = 4;
        [SerializeField, Min(0.01f)] private float _vfxDuration = 0.12f;
        [SerializeField, Min(0.001f)] private float _vfxWidth = 0.08f;
        [SerializeField] private Color _vfxColor = new Color(0.2f, 0.95f, 0.85f, 0.9f);

        public TargetingParameters Targeting => new TargetingParameters(
            _acquisitionRadius,
            _releaseRadius,
            _distanceWeight,
            _frontBiasWeight,
            _switchScoreAdvantage);

        public DisplacementPolicy Displacement => new DisplacementPolicy(_elitePullFraction);

        public float TargetScanInterval => _targetScanInterval;

        public int TargetColliderCapacity => _targetColliderCapacity;

        public LayerMask TargetLayers => _targetLayers;

        public LayerMask HardBlockerLayers => _hardBlockerLayers;

        public float PullStopDistance => _pullStopDistance;

        public float BlockerClearance => _blockerClearance;

        public int VfxPoolSize => _vfxPoolSize;

        public float VfxDuration => _vfxDuration;

        public float VfxWidth => _vfxWidth;

        public Color VfxColor => _vfxColor;

        public void ValidateOrThrow()
        {
            _ = Targeting;
            _ = Displacement;
            ValidatePositiveFinite(_targetScanInterval, nameof(_targetScanInterval));
            ValidateNonNegativeFinite(_pullStopDistance, nameof(_pullStopDistance));
            ValidateNonNegativeFinite(_blockerClearance, nameof(_blockerClearance));
            ValidatePositiveFinite(_vfxDuration, nameof(_vfxDuration));
            ValidatePositiveFinite(_vfxWidth, nameof(_vfxWidth));

            if (_targetColliderCapacity < 1 || _vfxPoolSize < 1)
            {
                throw new InvalidOperationException("Target capacity and VFX pool size must be positive.");
            }
        }

        private static void ValidatePositiveFinite(float value, string fieldName)
        {
            ValidateNonNegativeFinite(value, fieldName);
            if (value <= 0f)
            {
                throw new InvalidOperationException($"{fieldName} must be positive.");
            }
        }

        private static void ValidateNonNegativeFinite(float value, string fieldName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            {
                throw new InvalidOperationException($"{fieldName} must be finite and non-negative.");
            }
        }
    }
}
