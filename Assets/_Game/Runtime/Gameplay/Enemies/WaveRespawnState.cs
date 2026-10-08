using System;

namespace Gravivore.Gameplay.Enemies
{
    public enum SpawnSpotAvailability { Available, Active, Cooldown, Ready, Locked }

    /// <summary>One clock per ordinary wave. Later kills never extend the first-kill deadline.</summary>
    public sealed class WaveRespawnState
    {
        public const float MinimumCooldownSeconds = 120f;
        private readonly double _cooldown;
        private double _elapsed, _eligibleAt;
        public WaveRespawnState(float cooldownSeconds)
        {
            if (float.IsNaN(cooldownSeconds) || float.IsInfinity(cooldownSeconds) || cooldownSeconds < MinimumCooldownSeconds)
                throw new ArgumentOutOfRangeException(nameof(cooldownSeconds));
            _cooldown = cooldownSeconds;
        }
        public bool HasMissingMembers { get; private set; }
        public float RemainingSeconds => HasMissingMembers ? (float)Math.Max(0d, _eligibleAt - _elapsed) : 0f;
        public void RegisterKill()
        {
            if (HasMissingMembers) return;
            HasMissingMembers = true;
            _eligibleAt = _elapsed + _cooldown;
        }
        public void Tick(float deltaTime)
        {
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            _elapsed += deltaTime;
        }
        public bool CanRestore(bool playerOutside, bool inCombat, bool capacityAvailable) =>
            HasMissingMembers && RemainingSeconds <= 0f && playerOutside && !inCombat && capacityAvailable;
        public void CompleteRestore() => HasMissingMembers = false;
        public SpawnSpotAvailability Read(bool inCombat) => HasMissingMembers
            ? RemainingSeconds > 0f ? SpawnSpotAvailability.Cooldown : SpawnSpotAvailability.Ready
            : inCombat ? SpawnSpotAvailability.Active : SpawnSpotAvailability.Available;
    }
}
