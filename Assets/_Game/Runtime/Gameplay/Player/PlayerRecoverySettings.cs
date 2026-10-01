using System;
using UnityEngine;

namespace Gravivore.Gameplay.Player
{
    public sealed class PlayerRecoveryConfiguration
    {
        public static readonly PlayerRecoveryConfiguration Disabled = new PlayerRecoveryConfiguration(0f, 1f, 0f, 0f);

        public PlayerRecoveryConfiguration(
            float normalRegenFractionPerSecond,
            float respawnZoneMultiplier,
            float respawnZoneRadius,
            float combatExitDelaySeconds)
        {
            ValidateNonNegative(normalRegenFractionPerSecond, nameof(normalRegenFractionPerSecond));
            ValidateNonNegative(respawnZoneMultiplier, nameof(respawnZoneMultiplier));
            ValidateNonNegative(respawnZoneRadius, nameof(respawnZoneRadius));
            ValidateNonNegative(combatExitDelaySeconds, nameof(combatExitDelaySeconds));
            NormalRegenFractionPerSecond = normalRegenFractionPerSecond;
            RespawnZoneMultiplier = respawnZoneMultiplier;
            RespawnZoneRadius = respawnZoneRadius;
            CombatExitDelaySeconds = combatExitDelaySeconds;
        }

        public float NormalRegenFractionPerSecond { get; }
        public float RespawnZoneMultiplier { get; }
        public float RespawnZoneRadius { get; }
        public float CombatExitDelaySeconds { get; }

        private static void ValidateNonNegative(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }
    }

    [CreateAssetMenu(fileName = "PlayerRecoverySettings", menuName = "Gravivore/Player/Recovery Settings")]
    public sealed class PlayerRecoverySettings : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _normalRegenFractionPerSecond = 0.02f;
        [SerializeField, Min(0f)] private float _respawnZoneMultiplier = 10f;
        [SerializeField, Min(0f)] private float _respawnZoneRadius = 2.75f;
        [SerializeField, Min(0f)] private float _combatExitDelaySeconds = 3f;

        public PlayerRecoveryConfiguration Configuration => new PlayerRecoveryConfiguration(
            _normalRegenFractionPerSecond,
            _respawnZoneMultiplier,
            _respawnZoneRadius,
            _combatExitDelaySeconds);

        public void ValidateOrThrow() => _ = Configuration;
    }
}
