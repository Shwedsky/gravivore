using System;
using System.Collections.Generic;
using Gravivore.Gameplay.Progression;

namespace Gravivore.Gameplay.World
{
    public readonly struct WorldGateUnlockedEvent
    {
        public WorldGateUnlockedEvent(string gateId) => GateId = gateId;
        public string GateId { get; }
    }

    public readonly struct EliteDefeatedEvent
    {
        public EliteDefeatedEvent(string eliteId) => EliteId = eliteId;
        public string EliteId { get; }
    }

    public readonly struct WorldUnlockSnapshot
    {
        public WorldUnlockSnapshot(bool eliteGateUnlocked, bool eliteDefeated, bool bossGateUnlocked)
        {
            if (eliteDefeated && !eliteGateUnlocked)
            {
                throw new ArgumentException("Elite defeat requires an unlocked elite gate.");
            }

            if (bossGateUnlocked != eliteDefeated)
            {
                throw new ArgumentException("Boss gate and elite defeat state must agree.");
            }

            EliteGateUnlocked = eliteGateUnlocked;
            EliteDefeated = eliteDefeated;
            BossGateUnlocked = bossGateUnlocked;
        }

        public bool EliteGateUnlocked { get; }
        public bool EliteDefeated { get; }
        public bool BossGateUnlocked { get; }
    }

    public sealed class EliteGateRequirement
    {
        private readonly string[] _requiredFirstKillEnemyIds;

        public EliteGateRequirement(IReadOnlyList<string> requiredFirstKillEnemyIds, long minimumAssimilationScore)
        {
            if (requiredFirstKillEnemyIds == null || requiredFirstKillEnemyIds.Count == 0)
            {
                throw new ArgumentException("At least one required enemy id is required.", nameof(requiredFirstKillEnemyIds));
            }

            if (minimumAssimilationScore < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumAssimilationScore));
            }

            var unique = new HashSet<string>(StringComparer.Ordinal);
            _requiredFirstKillEnemyIds = new string[requiredFirstKillEnemyIds.Count];
            for (var i = 0; i < requiredFirstKillEnemyIds.Count; i++)
            {
                var enemyId = requiredFirstKillEnemyIds[i];
                if (string.IsNullOrWhiteSpace(enemyId) || !unique.Add(enemyId))
                {
                    throw new ArgumentException("Required enemy ids must be non-empty and unique.", nameof(requiredFirstKillEnemyIds));
                }

                _requiredFirstKillEnemyIds[i] = enemyId;
            }

            MinimumAssimilationScore = minimumAssimilationScore;
        }

        public int RequiredFirstKillCount => _requiredFirstKillEnemyIds.Length;
        public long MinimumAssimilationScore { get; }

        public string GetRequiredEnemyId(int index) => _requiredFirstKillEnemyIds[index];

        public bool IsSatisfied(ProgressionState progression)
        {
            if (progression == null)
            {
                throw new ArgumentNullException(nameof(progression));
            }

            if (progression.TotalAssimilationScore < MinimumAssimilationScore)
            {
                return false;
            }

            for (var i = 0; i < _requiredFirstKillEnemyIds.Length; i++)
            {
                if (!progression.HasFirstKill(_requiredFirstKillEnemyIds[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public sealed class WorldUnlockState
    {
        private readonly string _eliteGateId;
        private readonly string _bossGateId;
        private readonly string _eliteEnemyId;

        public WorldUnlockState(string eliteGateId, string bossGateId, string eliteEnemyId)
            : this(eliteGateId, bossGateId, eliteEnemyId, default)
        {
        }

        private WorldUnlockState(
            string eliteGateId,
            string bossGateId,
            string eliteEnemyId,
            WorldUnlockSnapshot snapshot)
        {
            _eliteGateId = RequireId(eliteGateId, nameof(eliteGateId));
            _bossGateId = RequireId(bossGateId, nameof(bossGateId));
            _eliteEnemyId = RequireId(eliteEnemyId, nameof(eliteEnemyId));
            if (string.Equals(_eliteGateId, _bossGateId, StringComparison.Ordinal))
            {
                throw new ArgumentException("Gate ids must be distinct.");
            }

            EliteGateUnlocked = snapshot.EliteGateUnlocked;
            EliteDefeated = snapshot.EliteDefeated;
            BossGateUnlocked = snapshot.BossGateUnlocked;
        }

        public event Action<WorldGateUnlockedEvent> GateUnlocked;
        public event Action<EliteDefeatedEvent> EliteWasDefeated;

        public bool EliteGateUnlocked { get; private set; }
        public bool EliteDefeated { get; private set; }
        public bool BossGateUnlocked { get; private set; }

        public static WorldUnlockState Restore(
            string eliteGateId,
            string bossGateId,
            string eliteEnemyId,
            in WorldUnlockSnapshot snapshot)
        {
            return new WorldUnlockState(eliteGateId, bossGateId, eliteEnemyId, snapshot);
        }

        public bool TryUnlockEliteGate()
        {
            if (EliteGateUnlocked)
            {
                return false;
            }

            EliteGateUnlocked = true;
            GateUnlocked?.Invoke(new WorldGateUnlockedEvent(_eliteGateId));
            return true;
        }

        public bool RecordEliteDefeated(string eliteEnemyId)
        {
            if (!string.Equals(eliteEnemyId, _eliteEnemyId, StringComparison.Ordinal) ||
                !EliteGateUnlocked || EliteDefeated)
            {
                return false;
            }

            EliteDefeated = true;
            BossGateUnlocked = true;
            EliteWasDefeated?.Invoke(new EliteDefeatedEvent(_eliteEnemyId));
            GateUnlocked?.Invoke(new WorldGateUnlockedEvent(_bossGateId));
            return true;
        }

        public WorldUnlockSnapshot ExportSnapshot()
        {
            return new WorldUnlockSnapshot(EliteGateUnlocked, EliteDefeated, BossGateUnlocked);
        }

        private static string RequireId(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A stable id is required.", parameterName);
            }

            return value;
        }
    }

    public sealed class WorldUnlockService : IDisposable
    {
        private readonly AssimilationProgressionService _progression;
        private readonly EliteGateRequirement _requirement;
        private bool _disposed;

        public WorldUnlockService(
            AssimilationProgressionService progression,
            EliteGateRequirement requirement,
            WorldUnlockState state)
        {
            _progression = progression ?? throw new ArgumentNullException(nameof(progression));
            _requirement = requirement ?? throw new ArgumentNullException(nameof(requirement));
            State = state ?? throw new ArgumentNullException(nameof(state));
            _progression.RewardGranted += HandleRewardGranted;
            EvaluateEliteGate();
        }

        public WorldUnlockState State { get; }

        public bool EvaluateEliteGate()
        {
            return _requirement.IsSatisfied(_progression.State) && State.TryUnlockEliteGate();
        }

        public bool RecordEliteDefeated(string eliteEnemyId) => State.RecordEliteDefeated(eliteEnemyId);

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _progression.RewardGranted -= HandleRewardGranted;
        }

        private void HandleRewardGranted(CoreRewardGrantedEvent reward) => EvaluateEliteGate();
    }
}
