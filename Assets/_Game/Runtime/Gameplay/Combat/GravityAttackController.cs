using System;
using Gravivore.Gameplay.Player;
using UnityEngine;

namespace Gravivore.Gameplay.Combat
{
    [DisallowMultipleComponent]
    public sealed class GravityAttackController : MonoBehaviour
    {
        private readonly AttackCadenceTimer _cadence = new AttackCadenceTimer();

        private Transform _attackOrigin;
        private PlayerStatsState _playerStats;
        private GravityAttackSettings _settings;
        private ITargetSensor _targetSensor;
        private IPullDestinationResolver _pullDestinationResolver;
        private IGravityLashVfx _lashVfx;
        private IPlayerCombatActivity _combatActivity;
        private CombatTarget _currentTarget;
        private ITargetable _chargingTarget;
        private float _scanRemaining;
        private bool _hasCurrentTarget;
        private bool _isInitialized;

        public bool HasCurrentTarget => _hasCurrentTarget;

        public void ResetTransientState()
        {
            ClearCurrentTarget();
            _scanRemaining = 0f;
            _cadence.Reset();
        }

        public void Initialize(
            Transform attackOrigin,
            PlayerStatsState playerStats,
            GravityAttackSettings settings,
            ITargetSensor targetSensor,
            IPullDestinationResolver pullDestinationResolver,
            IGravityLashVfx lashVfx,
            IPlayerCombatActivity combatActivity = null)
        {
            _attackOrigin = attackOrigin != null
                ? attackOrigin
                : throw new ArgumentNullException(nameof(attackOrigin));
            _playerStats = playerStats ?? throw new ArgumentNullException(nameof(playerStats));
            _settings = settings != null ? settings : throw new ArgumentNullException(nameof(settings));
            _targetSensor = targetSensor ?? throw new ArgumentNullException(nameof(targetSensor));
            _pullDestinationResolver = pullDestinationResolver ??
                                       throw new ArgumentNullException(nameof(pullDestinationResolver));
            _lashVfx = lashVfx ?? throw new ArgumentNullException(nameof(lashVfx));
            _combatActivity = combatActivity;
            _settings.ValidateOrThrow();
            _scanRemaining = 0f;
            _isInitialized = true;
        }

        private void Update()
        {
            if (_isInitialized)
            {
                Tick(Time.deltaTime);
            }
        }

        private void OnDisable() => CancelPresentationCharge();

        public void Tick(float deltaTime)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("GravityAttackController must be initialized before ticking.");
            }

            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            if (_hasCurrentTarget && !IsCurrentTargetValid())
            {
                ClearCurrentTarget();
            }

            _scanRemaining -= deltaTime;
            if (_scanRemaining <= 0f)
            {
                RefreshTarget();
                _scanRemaining = _settings.TargetScanInterval;
            }

            var hasValidTarget = _hasCurrentTarget && IsCurrentTargetValid();
            if (!hasValidTarget || !ReferenceEquals(_chargingTarget, _currentTarget.Targetable))
                CancelPresentationCharge();
            if (_cadence.Advance(
                    deltaTime,
                    hasValidTarget,
                    _playerStats.DerivedStats.AttackInterval))
            {
                ExecuteAttack();
            }
            else if (hasValidTarget)
            {
                TryBeginPresentationCharge();
            }
        }

        private void TryBeginPresentationCharge()
        {
            if (_chargingTarget != null || !(_lashVfx is IPrechargedGravityLashVfx prepared)) return;
            try
            {
                var remaining = _cadence.RemainingCooldown;
                if (remaining <= 0f || remaining > prepared.ChargeDuration) return;
                _chargingTarget = _currentTarget.Targetable;
                prepared.BeginCharge(_attackOrigin.position, _chargingTarget.TargetPoint.position,
                    _chargingTarget, remaining);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                CancelPresentationCharge();
            }
        }

        private void CancelPresentationCharge()
        {
            if (_chargingTarget == null) return;
            _chargingTarget = null;
            try { if (_lashVfx is IPrechargedGravityLashVfx prepared) prepared.CancelCharge(); }
            catch (Exception exception) { Debug.LogException(exception); }
        }

        private void RefreshTarget()
        {
            var candidates = _targetSensor.Collect(
                _attackOrigin,
                CombatFaction.Player,
                _settings.Targeting);
            _hasCurrentTarget = TargetSelector.TrySelect(
                candidates,
                _hasCurrentTarget,
                _currentTarget,
                _settings.Targeting,
                out _currentTarget);
        }

        private bool IsCurrentTargetValid()
        {
            if (!_currentTarget.IsValidFor(CombatFaction.Player))
            {
                return false;
            }

            var targetPoint = _currentTarget.Targetable.TargetPoint.position;
            return Vector3.Distance(_attackOrigin.position, targetPoint) <= _settings.Targeting.ReleaseRadius &&
                   _targetSensor.HasLineOfSight(_attackOrigin.position, targetPoint);
        }

        private void ExecuteAttack()
        {
            if (!IsCurrentTargetValid())
            {
                ClearCurrentTarget();
                return;
            }

            var origin = _attackOrigin.position;
            var targetPoint = _currentTarget.Targetable.TargetPoint.position;
            try
            {
                if (_lashVfx is ITrackedGravityLashVfx tracked)
                    tracked.Play(origin, targetPoint, _currentTarget.Targetable);
                else _lashVfx.Play(origin, targetPoint);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                CancelPresentationCharge();
            }
            _chargingTarget = null;
            var damageResult = _currentTarget.Damageable.ApplyDamage(new DamageRequest(
                _playerStats.DerivedStats.BaseDamage,
                DamageType.Gravity));
            if (damageResult.AppliedDamage > 0f)
            {
                _combatActivity?.RecordDamageDealt(damageResult.AppliedDamage);
            }

            if (damageResult.WasLethal || !_currentTarget.Damageable.IsAlive)
            {
                ClearCurrentTarget();
                return;
            }

            if (_currentTarget.Displaceable == null)
            {
                return;
            }

            ApplyDisplacement(origin, _currentTarget.Displaceable);
        }

        private void ApplyDisplacement(
            Vector3 sourcePosition,
            IDisplaceable displaceable)
        {
            var displacementRoot = displaceable.DisplacementRoot;
            if (displacementRoot == null)
            {
                return;
            }

            var displacementPosition = displacementRoot.position;
            var distanceToSource = Vector3.Distance(displacementPosition, sourcePosition);
            var requestedDistance = _settings.Displacement.CalculatePullDistance(
                distanceToSource,
                _settings.PullStopDistance,
                displaceable.DisplacementClass);
            if (requestedDistance <= Mathf.Epsilon)
            {
                return;
            }

            var pullDirection = (sourcePosition - displacementPosition).normalized;
            var requestedDestination = displacementPosition + (pullDirection * requestedDistance);
            var resolvedDestination = _pullDestinationResolver.Resolve(
                displacementPosition,
                requestedDestination,
                displaceable.CollisionRadius);
            var resolvedDistance = Vector3.Distance(displacementPosition, resolvedDestination);
            displaceable.TryDisplace(
                resolvedDestination,
                new DisplacementContext(sourcePosition, requestedDistance, resolvedDistance));
        }

        private void ClearCurrentTarget()
        {
            CancelPresentationCharge();
            _hasCurrentTarget = false;
            _currentTarget = default;
        }
    }
}
