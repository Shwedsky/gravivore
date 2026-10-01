using System;
using Gravivore.Gameplay.Combat;
using UnityEngine;

namespace Gravivore.Gameplay.Player
{
    public readonly struct PlayerDeathEvent
    {
        public PlayerDeathEvent(Vector3 position)
        {
            Position = position;
        }

        public Vector3 Position { get; }
    }

    public readonly struct PlayerRespawnEvent
    {
        public PlayerRespawnEvent(Vector3 position, float restoredHitPoints)
        {
            Position = position;
            RestoredHitPoints = restoredHitPoints;
        }

        public Vector3 Position { get; }

        public float RestoredHitPoints { get; }
    }

    public interface IPlayerCombatActivity
    {
        void RecordDamageDealt(float appliedDamage);
    }

    [DisallowMultipleComponent]
    public sealed class PlayerHealthController : MonoBehaviour, IDamageable, IPlayerCombatActivity
    {
        private CharacterController _body;
        private PlayerStatsState _stats;
        private PlayerHealthRuntime _runtime;
        private Vector3 _respawnPosition;
        private bool _respawnInProgress;
        private bool _isInitialized;

        public event Action<DamageResult> Damaged;

        public event Action<PlayerDeathEvent> Died;

        public event Action<PlayerRespawnEvent> Respawned;

        public event Action<float> Healed;

        public float CurrentHitPoints => _runtime != null ? _runtime.CurrentHitPoints : 0f;

        public float MaximumHitPoints => _runtime != null ? _runtime.MaximumHitPoints : 0f;

        public float Armor => _runtime != null ? _runtime.Armor : 0f;

        public bool IsAlive => _isInitialized && _runtime.IsAlive;

        public bool IsInvulnerable => _isInitialized && _runtime.IsInvulnerable;

        public float CombatSecondsRemaining => _runtime != null ? _runtime.CombatSecondsRemaining : 0f;

        public void Initialize(
            CharacterController body,
            PlayerStatsState stats,
            Vector3 respawnPosition,
            float postRespawnInvulnerabilitySeconds)
        {
            Initialize(body, stats, respawnPosition, postRespawnInvulnerabilitySeconds, PlayerRecoveryConfiguration.Disabled);
        }

        public void Initialize(
            CharacterController body,
            PlayerStatsState stats,
            Vector3 respawnPosition,
            float postRespawnInvulnerabilitySeconds,
            PlayerRecoveryConfiguration recovery)
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException("Player health is already initialized.");
            }

            _body = body != null ? body : throw new ArgumentNullException(nameof(body));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            _respawnPosition = respawnPosition;
            _runtime = new PlayerHealthRuntime(stats, postRespawnInvulnerabilitySeconds, recovery);
            _stats.DerivedStatsChanged += HandleDerivedStatsChanged;
            _isInitialized = true;
        }

        public DamageResult ApplyDamage(in DamageRequest request)
        {
            EnsureInitialized();
            var result = _runtime.ApplyDamage(request);
            if (result.AppliedDamage > 0f)
            {
                Publish(Damaged, result);
            }

            if (result.WasLethal && !_respawnInProgress)
            {
                RespawnAfterDeath();
            }

            return result;
        }

        public void Tick(float deltaTime)
        {
            EnsureInitialized();
            var healed = _runtime.Tick(deltaTime, transform.position, _respawnPosition);
            if (healed > 0f) Publish(Healed, healed);
        }

        public void RecordDamageDealt(float appliedDamage)
        {
            EnsureInitialized();
            _runtime.RecordDamageDealt(appliedDamage);
        }

        private void Update()
        {
            if (_isInitialized)
            {
                Tick(Time.deltaTime);
            }
        }

        private void RespawnAfterDeath()
        {
            _respawnInProgress = true;
            try
            {
                Publish(Died, new PlayerDeathEvent(transform.position));
            }
            finally
            {
                try
                {
                    _body.enabled = false;
                    transform.position = _respawnPosition;
                    _body.enabled = true;
                    _runtime.Respawn();
                    Publish(Respawned, new PlayerRespawnEvent(_respawnPosition, _runtime.CurrentHitPoints));
                }
                finally
                {
                    _respawnInProgress = false;
                }
            }
        }

        private void HandleDerivedStatsChanged(PlayerDerivedStatsChange change)
        {
            _runtime.SynchronizeDerivedStats();
        }

        private void OnDestroy()
        {
            if (_stats != null)
            {
                _stats.DerivedStatsChanged -= HandleDerivedStatsChanged;
            }
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Player health must be initialized before use.");
            }
        }

        private static void Publish<T>(Action<T> handlers, T value)
        {
            if (handlers == null) return;
            var invocationList = handlers.GetInvocationList();
            for (var i = 0; i < invocationList.Length; i++)
            {
                try
                {
                    ((Action<T>)invocationList[i])(value);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }
    }
}
