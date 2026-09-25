using System;
using Gravivore.Gameplay.Combat;

namespace Gravivore.Gameplay.Player
{
    public sealed class PlayerHealthRuntime
    {
        private readonly PlayerStatsState _stats;
        private readonly HealthState _health = new HealthState();
        private readonly float _postRespawnInvulnerabilitySeconds;
        private float _invulnerabilityRemaining;

        public PlayerHealthRuntime(PlayerStatsState stats, float postRespawnInvulnerabilitySeconds)
        {
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            if (float.IsNaN(postRespawnInvulnerabilitySeconds) ||
                float.IsInfinity(postRespawnInvulnerabilitySeconds) ||
                postRespawnInvulnerabilitySeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(postRespawnInvulnerabilitySeconds));
            }

            _postRespawnInvulnerabilitySeconds = postRespawnInvulnerabilitySeconds;
            _health.Reset(_stats.DerivedStats.MaxHp);
        }

        public float CurrentHitPoints => _health.CurrentHitPoints;

        public float MaximumHitPoints => _health.MaximumHitPoints;

        public float Armor => _stats.DerivedStats.ArmorValue;

        public bool IsAlive => _health.IsAlive;

        public bool IsInvulnerable => _invulnerabilityRemaining > 0f;

        public DamageResult ApplyDamage(in DamageRequest request)
        {
            return IsInvulnerable
                ? new DamageResult(0f, false)
                : _health.ApplyDamage(request, Armor);
        }

        public void SynchronizeDerivedStats()
        {
            _health.SetMaximum(_stats.DerivedStats.MaxHp);
        }

        public void Respawn()
        {
            _health.Reset(_stats.DerivedStats.MaxHp);
            _invulnerabilityRemaining = _postRespawnInvulnerabilitySeconds;
        }

        public void Tick(float deltaTime)
        {
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            _invulnerabilityRemaining = Math.Max(0f, _invulnerabilityRemaining - deltaTime);
        }
    }
}
