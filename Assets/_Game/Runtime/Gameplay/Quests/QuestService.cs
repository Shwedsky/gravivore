using System;
using System.Collections.Generic;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Progression;

namespace Gravivore.Gameplay.Quests
{
    public sealed class QuestService : IDisposable
    {
        private readonly QuestCatalog _catalog;
        private readonly AssimilationProgressionService _progression;
        private readonly IMagnetarGuardDefeatSource _eliteDefeats;
        private readonly BossCompletionState _bossCompletion;
        private bool _disposed;

        public QuestService(
            QuestCatalog catalog,
            QuestState state,
            AssimilationProgressionService progression = null,
            IMagnetarGuardDefeatSource eliteDefeats = null,
            BossCompletionState bossCompletion = null)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            State = state ?? throw new ArgumentNullException(nameof(state));
            _progression = progression;
            _eliteDefeats = eliteDefeats;
            _bossCompletion = bossCompletion;
            if (_progression != null) _progression.RewardGranted += HandleRewardGranted;
            if (_eliteDefeats != null) _eliteDefeats.Defeated += HandleEliteDefeated;
            if (_bossCompletion != null) _bossCompletion.Defeated += HandleBossDefeated;
        }

        public event Action<QuestObjectiveProgressedEvent> ObjectiveProgressed;
        public event Action<QuestObjectiveCompletedEvent> ObjectiveCompleted;
        public event Action<QuestAssimilationFeedbackEvent> AssimilationFeedback;
        public event Action<QuestSequenceCompletedEvent> SequenceCompleted;

        public QuestState State { get; }

        public QuestObjective? ActiveObjective
        {
            get
            {
                if (State.Completed) return null;
                for (var i = 0; i < _catalog.ObjectiveCount; i++)
                {
                    var objective = _catalog.GetObjective(i);
                    if (!State.IsObjectiveCompleted(objective.Id)) return objective;
                }

                return null;
            }
        }

        public QuestObjectiveProgress GetProgress(string objectiveId)
        {
            if (!_catalog.TryGetObjective(objectiveId, out var objective))
            {
                throw new ArgumentException("Unknown objective id.", nameof(objectiveId));
            }

            return new QuestObjectiveProgress(
                objective.Id,
                State.GetProgress(objective.Id),
                objective.RequiredCount,
                State.IsObjectiveCompleted(objective.Id));
        }

        public bool RecordMovementPerformed()
        {
            return AdvanceMatching(objective => objective.Type == QuestObjectiveType.MovementPerformed, 1, null);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (_progression != null) _progression.RewardGranted -= HandleRewardGranted;
            if (_eliteDefeats != null) _eliteDefeats.Defeated -= HandleEliteDefeated;
            if (_bossCompletion != null) _bossCompletion.Defeated -= HandleBossDefeated;
        }

        private void HandleRewardGranted(CoreRewardGrantedEvent reward)
        {
            var lifeWasNew = State.TryMarkEnemyLifeProcessed(reward.LifeId);
            if (!lifeWasNew) return;

            var completedBefore = State.CompletedObjectiveCount;
            AdvanceMatching(
                objective => objective.Type == QuestObjectiveType.EnemyDefeated &&
                             string.Equals(objective.EnemyId, reward.EnemyId, StringComparison.Ordinal),
                1,
                null);
            AdvanceMatching(
                objective => objective.Type == QuestObjectiveType.AssimilationReceived &&
                             (string.IsNullOrEmpty(objective.EnemyId) ||
                              string.Equals(objective.EnemyId, reward.EnemyId, StringComparison.Ordinal)),
                1,
                null);
            if (State.CompletedObjectiveCount > completedBefore)
            {
                PublishFeedback(reward);
            }
        }

        private void HandleEliteDefeated(MagnetarGuardDefeatedEvent defeated)
        {
            AdvanceMatching(
                objective => objective.Type == QuestObjectiveType.EliteDefeated &&
                             string.Equals(objective.EncounterId, defeated.EliteId, StringComparison.Ordinal),
                1,
                null);
        }

        private void HandleBossDefeated(BossDefeatedEvent defeated)
        {
            AdvanceMatching(
                objective => objective.Type == QuestObjectiveType.BossDefeated &&
                             string.Equals(objective.EncounterId, defeated.BossId, StringComparison.Ordinal),
                1,
                null);
        }

        private bool AdvanceMatching(Func<QuestObjective, bool> predicate, int amount, int? requiredOverride)
        {
            var any = false;
            for (var i = 0; i < _catalog.ObjectiveCount; i++)
            {
                var objective = _catalog.GetObjective(i);
                if (State.IsObjectiveCompleted(objective.Id) || !predicate(objective)) continue;
                var advanceAmount = amount;
                if (requiredOverride.HasValue)
                {
                    advanceAmount = Math.Min(requiredOverride.Value, State.GetProgress(objective.Id) + amount) -
                                    State.GetProgress(objective.Id);
                    if (advanceAmount <= 0) continue;
                }

                if (State.TryAdvance(objective, advanceAmount, out var progress))
                {
                    any = true;
                    PublishProgress(objective, progress);
                }
            }

            return any;
        }

        private void PublishProgress(QuestObjective objective, QuestObjectiveProgress progress)
        {
            var errors = default(List<Exception>);
            PublishEach(ref errors, ObjectiveProgressed, new QuestObjectiveProgressedEvent(State.QuestId, objective, progress));
            if (progress.Completed)
            {
                PublishEach(ref errors, ObjectiveCompleted, new QuestObjectiveCompletedEvent(State.QuestId, objective));
                if (State.Completed)
                {
                    PublishEach(ref errors, SequenceCompleted, new QuestSequenceCompletedEvent(State.QuestId));
                }
            }

            ThrowIfNeeded(errors);
        }

        private void PublishFeedback(CoreRewardGrantedEvent reward)
        {
            var errors = default(List<Exception>);
            PublishEach(ref errors, AssimilationFeedback, new QuestAssimilationFeedbackEvent(reward));
            ThrowIfNeeded(errors);
        }

        private static void PublishEach<T>(ref List<Exception> errors, Action<T> handlers, T value)
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
                    if (errors == null) errors = new List<Exception>(1);
                    errors.Add(exception);
                }
            }
        }

        private static void ThrowIfNeeded(List<Exception> errors)
        {
            if (errors != null)
            {
                throw new AggregateException("One or more quest observers failed after quest state was committed.", errors);
            }
        }
    }
}
