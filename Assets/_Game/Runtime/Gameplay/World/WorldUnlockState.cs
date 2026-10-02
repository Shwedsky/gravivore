using System;
using System.Collections.Generic;
using Gravivore.Gameplay.Quests;
using Gravivore.Gameplay.Progression;
using UnityEngine;

namespace Gravivore.Gameplay.World
{
    public interface IEliteDefeatRecorder
    {
        bool RecordEliteDefeated(string eliteEnemyId);
    }

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
        private readonly string[] _requiredObjectiveIds;

        public EliteGateRequirement(IReadOnlyList<string> requiredObjectiveIds, long minimumAssimilationScore)
        {
            if (requiredObjectiveIds == null || requiredObjectiveIds.Count == 0)
            {
                throw new ArgumentException("At least one required objective id is required.", nameof(requiredObjectiveIds));
            }

            if (minimumAssimilationScore < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumAssimilationScore));
            }

            var unique = new HashSet<string>(StringComparer.Ordinal);
            _requiredObjectiveIds = new string[requiredObjectiveIds.Count];
            for (var i = 0; i < requiredObjectiveIds.Count; i++)
            {
                var objectiveId = requiredObjectiveIds[i];
                if (string.IsNullOrWhiteSpace(objectiveId) || !unique.Add(objectiveId))
                {
                    throw new ArgumentException("Required objective ids must be non-empty and unique.", nameof(requiredObjectiveIds));
                }

                _requiredObjectiveIds[i] = objectiveId;
            }

            MinimumAssimilationScore = minimumAssimilationScore;
        }

        public int RequiredObjectiveCount => _requiredObjectiveIds.Length;
        public long MinimumAssimilationScore { get; }

        public string GetRequiredObjectiveId(int index) => _requiredObjectiveIds[index];

        public bool IsSatisfied(ProgressionState progression, QuestState quests)
        {
            if (progression == null)
            {
                throw new ArgumentNullException(nameof(progression));
            }
            if (quests == null)
            {
                throw new ArgumentNullException(nameof(quests));
            }

            if (progression.TotalAssimilationScore < MinimumAssimilationScore)
            {
                return false;
            }

            for (var i = 0; i < _requiredObjectiveIds.Length; i++)
            {
                if (!quests.IsObjectiveCompleted(_requiredObjectiveIds[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public sealed class WorldUnlockState : Gravivore.Gameplay.Encounters.IBossEncounterAccess
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
        public bool CanEngage => BossGateUnlocked;

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
            Publish(GateUnlocked, new WorldGateUnlockedEvent(_eliteGateId));
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
            Publish(EliteWasDefeated, new EliteDefeatedEvent(_eliteEnemyId));
            Publish(GateUnlocked, new WorldGateUnlockedEvent(_bossGateId));
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

    public sealed class WorldUnlockService : IDisposable, IEliteDefeatRecorder
    {
        private readonly AssimilationProgressionService _progression;
        private readonly QuestService _quests;
        private readonly EliteGateRequirement _requirement;
        private bool _disposed;

        public WorldUnlockService(
            AssimilationProgressionService progression,
            QuestService quests,
            EliteGateRequirement requirement,
            WorldUnlockState state)
        {
            _progression = progression ?? throw new ArgumentNullException(nameof(progression));
            _quests = quests ?? throw new ArgumentNullException(nameof(quests));
            _requirement = requirement ?? throw new ArgumentNullException(nameof(requirement));
            State = state ?? throw new ArgumentNullException(nameof(state));
            _progression.RewardGranted += HandleRewardGranted;
            _quests.ObjectiveCompleted += HandleObjectiveCompleted;
            EvaluateEliteGate();
        }

        public WorldUnlockState State { get; }

        public bool EvaluateEliteGate()
        {
            return _requirement.IsSatisfied(_progression.State, _quests.State) && State.TryUnlockEliteGate();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public bool PrepareEliteEncounterForDevelopment()
        {
            var changed = _progression.State.EnsureMinimumAssimilationScoreForDevelopment(
                _requirement.MinimumAssimilationScore);
            changed |= _quests.CompleteRequiredObjectivesForDevelopment(_requirement);
            changed |= EvaluateEliteGate();
            return changed;
        }
#endif

        public bool RecordEliteDefeated(string eliteEnemyId) => State.RecordEliteDefeated(eliteEnemyId);

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _progression.RewardGranted -= HandleRewardGranted;
            _quests.ObjectiveCompleted -= HandleObjectiveCompleted;
        }

        private void HandleRewardGranted(CoreRewardGrantedEvent reward) => EvaluateEliteGate();
        private void HandleObjectiveCompleted(QuestObjectiveCompletedEvent completed) => EvaluateEliteGate();
    }
}
