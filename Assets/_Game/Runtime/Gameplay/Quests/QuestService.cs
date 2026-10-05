using System;
using System.Collections.Generic;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Progression;
using Gravivore.Gameplay.World;
using UnityEngine;

namespace Gravivore.Gameplay.Quests
{
    public sealed class QuestService : IDisposable
    {
        private readonly QuestCatalog _catalog;
        private readonly AssimilationProgressionService _progression;
        private readonly IMagnetarGuardDefeatSource _eliteDefeats;
        private readonly BossCompletionState _bossCompletion;
        private readonly Action<Exception> _observerErrorReporter;
        private bool _disposed;

        public QuestService(
            QuestCatalog catalog,
            QuestState state,
            AssimilationProgressionService progression = null,
            IMagnetarGuardDefeatSource eliteDefeats = null,
            BossCompletionState bossCompletion = null,
            Action<Exception> observerErrorReporter = null)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            State = state ?? throw new ArgumentNullException(nameof(state));
            _progression = progression;
            _eliteDefeats = eliteDefeats;
            _bossCompletion = bossCompletion;
            _observerErrorReporter = observerErrorReporter ?? Debug.LogException;
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
            return AdvanceAndPublish(objective => objective.Type == QuestObjectiveType.MovementPerformed, 1, null);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public bool CompleteRequiredObjectivesForDevelopment(EliteGateRequirement requirement)
        {
            if (requirement == null) throw new ArgumentNullException(nameof(requirement));

            var changed = false;
            for (var i = 0; i < requirement.RequiredObjectiveCount; i++)
            {
                var objectiveId = requirement.GetRequiredObjectiveId(i);
                if (!_catalog.TryGetObjective(objectiveId, out var objective))
                {
                    throw new InvalidOperationException($"Elite requirement references unknown objective '{objectiveId}'.");
                }

                if (State.IsObjectiveCompleted(objectiveId)) continue;
                changed |= AdvanceAndPublish(candidate => candidate.Id == objectiveId, objective.RequiredCount, null);
            }

            return changed;
        }

        public bool CompleteEncounterObjectiveForDevelopment(QuestObjectiveType type, string encounterId)
        {
            if (type != QuestObjectiveType.EliteDefeated && type != QuestObjectiveType.BossDefeated)
                throw new ArgumentOutOfRangeException(nameof(type));
            if (string.IsNullOrWhiteSpace(encounterId))
                throw new ArgumentException("Encounter id is required.", nameof(encounterId));

            for (var i = 0; i < _catalog.ObjectiveCount; i++)
            {
                var objective = _catalog.GetObjective(i);
                if (objective.Type != type ||
                    !string.Equals(objective.EncounterId, encounterId, StringComparison.Ordinal) ||
                    State.IsObjectiveCompleted(objective.Id)) continue;
                return AdvanceAndPublish(candidate => candidate.Id == objective.Id, objective.RequiredCount, null);
            }

            return false;
        }

        public bool ResetEncounterObjectiveForDevelopment(QuestObjectiveType type, string encounterId)
        {
            if (string.IsNullOrWhiteSpace(encounterId))
                throw new ArgumentException("Encounter id is required.", nameof(encounterId));
            for (var i = 0; i < _catalog.ObjectiveCount; i++)
            {
                var objective = _catalog.GetObjective(i);
                if (objective.Type == type &&
                    string.Equals(objective.EncounterId, encounterId, StringComparison.Ordinal))
                {
                    return State.ResetObjectiveForDevelopment(objective);
                }
            }

            return false;
        }
#endif

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
            if (!HasIncompleteRewardObjective(reward.EnemyId)) return;
            var lifeWasNew = State.TryMarkEnemyLifeProcessed(reward.LifeId);
            if (!lifeWasNew) return;

            var completedBefore = State.CompletedObjectiveCount;
            var sequenceCompletedBefore = State.Completed;
            var notifications = new List<ProgressNotification>(2);
            AdvanceMatching(
                objective => objective.Type == QuestObjectiveType.EnemyDefeated &&
                             string.Equals(objective.EnemyId, reward.EnemyId, StringComparison.Ordinal),
                1,
                null,
                notifications);
            AdvanceMatching(
                objective => objective.Type == QuestObjectiveType.AssimilationReceived &&
                             (string.IsNullOrEmpty(objective.EnemyId) ||
                              string.Equals(objective.EnemyId, reward.EnemyId, StringComparison.Ordinal)),
                1,
                null,
                notifications);

            var errors = default(List<Exception>);
            PublishNotifications(notifications, !sequenceCompletedBefore && State.Completed, ref errors);
            if (State.CompletedObjectiveCount > completedBefore)
            {
                PublishEach(ref errors, AssimilationFeedback, new QuestAssimilationFeedbackEvent(reward));
            }

            ReportObserverErrors(errors);
        }

        private void HandleEliteDefeated(MagnetarGuardDefeatedEvent defeated)
        {
            AdvanceAndPublish(
                objective => objective.Type == QuestObjectiveType.EliteDefeated &&
                             string.Equals(objective.EncounterId, defeated.EliteId, StringComparison.Ordinal),
                1,
                null);
        }

        public void RecordEliteDefeated(string eliteId) =>
            HandleEliteDefeated(new MagnetarGuardDefeatedEvent(eliteId, default));

        public void RecordBossDefeated(string bossId) =>
            HandleBossDefeated(new BossDefeatedEvent(bossId, default));

        private void HandleBossDefeated(BossDefeatedEvent defeated)
        {
            AdvanceAndPublish(
                objective => objective.Type == QuestObjectiveType.BossDefeated &&
                             string.Equals(objective.EncounterId, defeated.BossId, StringComparison.Ordinal),
                1,
                null);
        }

        private bool AdvanceAndPublish(Func<QuestObjective, bool> predicate, int amount, int? requiredOverride)
        {
            var sequenceCompletedBefore = State.Completed;
            var notifications = new List<ProgressNotification>(1);
            var any = AdvanceMatching(predicate, amount, requiredOverride, notifications);
            var errors = default(List<Exception>);
            PublishNotifications(notifications, !sequenceCompletedBefore && State.Completed, ref errors);
            ThrowIfNeeded(errors);
            return any;
        }

        private bool AdvanceMatching(
            Func<QuestObjective, bool> predicate,
            int amount,
            int? requiredOverride,
            List<ProgressNotification> notifications)
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
                    notifications.Add(new ProgressNotification(objective, progress));
                }
            }

            return any;
        }

        private void PublishNotifications(
            List<ProgressNotification> notifications,
            bool sequenceCompleted,
            ref List<Exception> errors)
        {
            for (var i = 0; i < notifications.Count; i++)
            {
                var notification = notifications[i];
                PublishEach(
                    ref errors,
                    ObjectiveProgressed,
                    new QuestObjectiveProgressedEvent(State.QuestId, notification.Objective, notification.Progress));
                if (notification.Progress.Completed)
                {
                    PublishEach(
                        ref errors,
                        ObjectiveCompleted,
                        new QuestObjectiveCompletedEvent(State.QuestId, notification.Objective));
                }
            }

            if (sequenceCompleted)
            {
                PublishEach(ref errors, SequenceCompleted, new QuestSequenceCompletedEvent(State.QuestId));
            }
        }

        private bool HasIncompleteRewardObjective(string enemyId)
        {
            for (var i = 0; i < _catalog.ObjectiveCount; i++)
            {
                var objective = _catalog.GetObjective(i);
                if (State.IsObjectiveCompleted(objective.Id)) continue;
                if (objective.Type == QuestObjectiveType.EnemyDefeated &&
                    string.Equals(objective.EnemyId, enemyId, StringComparison.Ordinal))
                {
                    return true;
                }

                if (objective.Type == QuestObjectiveType.AssimilationReceived &&
                    (string.IsNullOrEmpty(objective.EnemyId) ||
                     string.Equals(objective.EnemyId, enemyId, StringComparison.Ordinal)))
                {
                    return true;
                }
            }

            return false;
        }

        private void ReportObserverErrors(List<Exception> errors)
        {
            if (errors == null) return;
            var aggregate = new AggregateException(
                "One or more quest observers failed after authoritative reward processing completed.",
                errors);
            try
            {
                _observerErrorReporter(aggregate);
            }
            catch (Exception reporterError)
            {
                Debug.LogException(new AggregateException("Quest observer error reporting failed.", aggregate, reporterError));
            }
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

        private readonly struct ProgressNotification
        {
            public ProgressNotification(QuestObjective objective, QuestObjectiveProgress progress)
            {
                Objective = objective;
                Progress = progress;
            }

            public QuestObjective Objective { get; }
            public QuestObjectiveProgress Progress { get; }
        }
    }
}
