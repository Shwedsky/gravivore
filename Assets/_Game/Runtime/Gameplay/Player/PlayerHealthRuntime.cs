using System;
using Gravivore.Gameplay.Combat;
using UnityEngine;

namespace Gravivore.Gameplay.Player
{
    public sealed class PlayerHealthRuntime
    {
        private readonly PlayerStatsState _stats;
        private readonly HealthState _health = new HealthState();
        private readonly float _postRespawnInvulnerabilitySeconds;
        private readonly PlayerRecoveryConfiguration _recovery;
        private float _invulnerabilityRemaining;
        private float _combatRemaining;

        public PlayerHealthRuntime(PlayerStatsState stats, float postRespawnInvulnerabilitySeconds)
            : this(stats, postRespawnInvulnerabilitySeconds, PlayerRecoveryConfiguration.Disabled)
        {
        }

        public PlayerHealthRuntime(
            PlayerStatsState stats,
            float postRespawnInvulnerabilitySeconds,
            PlayerRecoveryConfiguration recovery)
        {
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            if (float.IsNaN(postRespawnInvulnerabilitySeconds) ||
                float.IsInfinity(postRespawnInvulnerabilitySeconds) ||
                postRespawnInvulnerabilitySeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(postRespawnInvulnerabilitySeconds));
            }

            _postRespawnInvulnerabilitySeconds = postRespawnInvulnerabilitySeconds;
            _recovery = recovery ?? throw new ArgumentNullException(nameof(recovery));
            _health.Reset(_stats.DerivedStats.MaxHp);
        }

        public float CurrentHitPoints => _health.CurrentHitPoints;

        public float MaximumHitPoints => _health.MaximumHitPoints;

        public float Armor => _stats.DerivedStats.ArmorValue;

        public bool IsAlive => _health.IsAlive;

        public bool IsInvulnerable => _invulnerabilityRemaining > 0f;

        public float CombatSecondsRemaining => _combatRemaining;

        public DamageResult ApplyDamage(in DamageRequest request)
        {
            var result = IsInvulnerable
                ? new DamageResult(0f, false)
                : _health.ApplyDamage(request, Armor);
            if (result.AppliedDamage > 0f) RecordCombatActivity();
            return result;
        }

        public void RecordDamageDealt(float appliedDamage)
        {
            if (float.IsNaN(appliedDamage) || float.IsInfinity(appliedDamage) || appliedDamage < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(appliedDamage));
            }

            if (appliedDamage > 0f) RecordCombatActivity();
        }

        public void SynchronizeDerivedStats()
        {
            _health.SetMaximum(_stats.DerivedStats.MaxHp);
        }

        public void Respawn()
        {
            _health.Reset(_stats.DerivedStats.MaxHp);
            _invulnerabilityRemaining = _postRespawnInvulnerabilitySeconds;
            _combatRemaining = 0f;
        }

        public void Tick(float deltaTime)
        {
            Tick(deltaTime, Vector3.positiveInfinity, Vector3.zero);
        }

        public float Tick(float deltaTime, Vector3 playerPosition, Vector3 respawnPosition)
        {
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            _invulnerabilityRemaining = Math.Max(0f, _invulnerabilityRemaining - deltaTime);
            var recoveryDelta = deltaTime;
            if (_combatRemaining > 0f)
            {
                recoveryDelta = Math.Max(0f, deltaTime - _combatRemaining);
                _combatRemaining = Math.Max(0f, _combatRemaining - deltaTime);
            }

            if (!_health.IsAlive || recoveryDelta <= 0f || _recovery.NormalRegenFractionPerSecond <= 0f)
            {
                return 0f;
            }

            var planarOffset = playerPosition - respawnPosition;
            planarOffset.y = 0f;
            var multiplier = planarOffset.sqrMagnitude <= _recovery.RespawnZoneRadius * _recovery.RespawnZoneRadius
                ? _recovery.RespawnZoneMultiplier
                : 1f;
            return _health.Heal(_health.MaximumHitPoints * _recovery.NormalRegenFractionPerSecond * multiplier * recoveryDelta);
        }

        private void RecordCombatActivity()
        {
            _combatRemaining = _recovery.CombatExitDelaySeconds;
        }
    }
}
