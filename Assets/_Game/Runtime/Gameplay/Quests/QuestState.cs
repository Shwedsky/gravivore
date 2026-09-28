using System;
using System.Collections.Generic;
using Gravivore.Gameplay.Enemies;

namespace Gravivore.Gameplay.Quests
{
    public readonly struct QuestObjectiveProgress
    {
        public QuestObjectiveProgress(string objectiveId, int progress, int required, bool completed)
        {
            ObjectiveId = objectiveId;
            Progress = progress;
            Required = required;
            Completed = completed;
        }

        public string ObjectiveId { get; }
        public int Progress { get; }
        public int Required { get; }
        public bool Completed { get; }
    }

    public readonly struct QuestSnapshot
    {
        public QuestSnapshot(
            string questId,
            IReadOnlyDictionary<string, int> objectiveProgress,
            IReadOnlyCollection<string> completedObjectiveIds,
            IReadOnlyCollection<EnemyLifeId> processedEnemyLifeIds,
            bool expandedObjectivesUnlocked,
            bool completed)
        {
            QuestId = questId;
            ObjectiveProgress = objectiveProgress;
            CompletedObjectiveIds = completedObjectiveIds;
            ProcessedEnemyLifeIds = processedEnemyLifeIds;
            ExpandedObjectivesUnlocked = expandedObjectivesUnlocked;
            Completed = completed;
        }

        public string QuestId { get; }
        public IReadOnlyDictionary<string, int> ObjectiveProgress { get; }
        public IReadOnlyCollection<string> CompletedObjectiveIds { get; }
        public IReadOnlyCollection<EnemyLifeId> ProcessedEnemyLifeIds { get; }
        public bool ExpandedObjectivesUnlocked { get; }
        public bool Completed { get; }
    }

    public sealed class QuestState
    {
        private readonly QuestCatalog _catalog;
        private readonly Dictionary<string, int> _progress = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly HashSet<string> _completed = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<EnemyLifeId> _processedEnemyLives = new HashSet<EnemyLifeId>();

        public QuestState(QuestCatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        private QuestState(QuestCatalog catalog, in QuestSnapshot snapshot) : this(catalog)
        {
            if (!string.Equals(snapshot.QuestId, catalog.QuestId, StringComparison.Ordinal))
            {
                throw new ArgumentException("Snapshot quest id does not match the quest definition.", nameof(snapshot));
            }

            if (snapshot.ObjectiveProgress != null)
            {
                foreach (var pair in snapshot.ObjectiveProgress)
                {
                    if (!_catalog.TryGetObjective(pair.Key, out var objective))
                    {
                        throw new ArgumentException($"Unknown objective id in snapshot: {pair.Key}.", nameof(snapshot));
                    }

                    if (pair.Value < 0 || pair.Value > objective.RequiredCount)
                    {
                        throw new ArgumentOutOfRangeException(nameof(snapshot), $"Invalid progress for objective {pair.Key}.");
                    }

                    if (pair.Value > 0) _progress[pair.Key] = pair.Value;
                }
            }

            if (snapshot.CompletedObjectiveIds != null)
            {
                foreach (var objectiveId in snapshot.CompletedObjectiveIds)
                {
                    if (!_catalog.TryGetObjective(objectiveId, out var objective))
                    {
                        throw new ArgumentException($"Unknown completed objective id in snapshot: {objectiveId}.", nameof(snapshot));
                    }

                    _completed.Add(objectiveId);
                    _progress[objectiveId] = objective.RequiredCount;
                }
            }

            if (snapshot.ProcessedEnemyLifeIds != null)
            {
                foreach (var lifeId in snapshot.ProcessedEnemyLifeIds)
                {
                    if (!lifeId.IsValid)
                    {
                        throw new ArgumentException("Snapshot contains an invalid enemy life id.", nameof(snapshot));
                    }

                    _processedEnemyLives.Add(lifeId);
                }
            }

            for (var i = 0; i < _catalog.ObjectiveCount; i++)
            {
                var objective = _catalog.GetObjective(i);
                if (!_completed.Contains(objective.Id)) continue;
                if (objective.ExpandsObjectiveUi) ExpandedObjectivesUnlocked = true;
                if (objective.Type == QuestObjectiveType.BossDefeated) Completed = true;
            }
        }

        public string QuestId => _catalog.QuestId;
        public bool ExpandedObjectivesUnlocked { get; private set; }
        public bool Completed { get; private set; }
        public int CompletedObjectiveCount => _completed.Count;

        public static QuestState Restore(QuestCatalog catalog, in QuestSnapshot snapshot)
        {
            return new QuestState(catalog, snapshot);
        }

        public bool IsObjectiveCompleted(string objectiveId) => _completed.Contains(objectiveId);

        public int GetProgress(string objectiveId)
        {
            return _progress.TryGetValue(objectiveId, out var progress) ? progress : 0;
        }

        public bool HasProcessedEnemyLife(EnemyLifeId lifeId) => _processedEnemyLives.Contains(lifeId);

        internal bool TryMarkEnemyLifeProcessed(EnemyLifeId lifeId)
        {
            return lifeId.IsValid && _processedEnemyLives.Add(lifeId);
        }

        internal bool TryAdvance(QuestObjective objective, int amount, out QuestObjectiveProgress progress)
        {
            if (_completed.Contains(objective.Id))
            {
                progress = new QuestObjectiveProgress(objective.Id, objective.RequiredCount, objective.RequiredCount, true);
                return false;
            }

            var current = GetProgress(objective.Id);
            var next = Math.Min(objective.RequiredCount, checked(current + amount));
            if (next == current)
            {
                progress = new QuestObjectiveProgress(objective.Id, current, objective.RequiredCount, false);
                return false;
            }

            _progress[objective.Id] = next;
            var completed = next >= objective.RequiredCount;
            if (completed)
            {
                _completed.Add(objective.Id);
                if (objective.ExpandsObjectiveUi)
                {
                    ExpandedObjectivesUnlocked = true;
                }

                if (objective.Type == QuestObjectiveType.BossDefeated)
                {
                    Completed = true;
                }
            }

            progress = new QuestObjectiveProgress(objective.Id, next, objective.RequiredCount, completed);
            return true;
        }

        public QuestSnapshot ExportSnapshot()
        {
            return new QuestSnapshot(
                QuestId,
                new Dictionary<string, int>(_progress, StringComparer.Ordinal),
                new List<string>(_completed),
                new List<EnemyLifeId>(_processedEnemyLives),
                ExpandedObjectivesUnlocked,
                Completed);
        }
    }
}
