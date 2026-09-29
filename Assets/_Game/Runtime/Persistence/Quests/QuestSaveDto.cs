using System;
using System.Collections.Generic;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Quests;

namespace Gravivore.Persistence.Quests
{
    [Serializable]
    public sealed class QuestObjectiveSaveDto
    {
        public string objectiveId;
        public int progress;
        public bool completed;
    }

    [Serializable]
    public sealed class QuestSaveDto
    {
        public string questId;
        public QuestObjectiveSaveDto[] objectives = Array.Empty<QuestObjectiveSaveDto>();
        public string[] processedEnemyLifeIds = Array.Empty<string>();
        public bool expandedObjectivesUnlocked;
        public bool completed;
    }

    public static class QuestSaveMapper
    {
        public static QuestSaveDto ToDto(QuestState state, QuestCatalog catalog)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            var snapshot = state.ExportSnapshot();
            var objectives = new QuestObjectiveSaveDto[catalog.ObjectiveCount];
            for (var i = 0; i < catalog.ObjectiveCount; i++)
            {
                var objective = catalog.GetObjective(i);
                objectives[i] = new QuestObjectiveSaveDto
                {
                    objectiveId = objective.Id,
                    progress = state.GetProgress(objective.Id),
                    completed = state.IsObjectiveCompleted(objective.Id)
                };
            }

            var lives = new string[snapshot.ProcessedEnemyLifeIds.Count];
            var index = 0;
            foreach (var lifeId in snapshot.ProcessedEnemyLifeIds)
            {
                lives[index++] = lifeId.Value.ToString("D");
            }

            return new QuestSaveDto
            {
                questId = snapshot.QuestId,
                objectives = objectives,
                processedEnemyLifeIds = lives,
                expandedObjectivesUnlocked = snapshot.ExpandedObjectivesUnlocked,
                completed = snapshot.Completed
            };
        }

        public static QuestState Restore(QuestCatalog catalog, QuestSaveDto dto)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (!string.Equals(catalog.QuestId, dto.questId, StringComparison.Ordinal))
            {
                throw new ArgumentException("Quest DTO id does not match the quest definition.", nameof(dto));
            }

            var progress = new Dictionary<string, int>(StringComparer.Ordinal);
            var completed = new List<string>();
            var objectiveIds = new HashSet<string>(StringComparer.Ordinal);
            if (dto.objectives != null)
            {
                for (var i = 0; i < dto.objectives.Length; i++)
                {
                    var entry = dto.objectives[i];
                    if (entry == null || !catalog.TryGetObjective(entry.objectiveId, out var objective))
                    {
                        throw new ArgumentException("Quest DTO contains an unknown objective id.", nameof(dto));
                    }

                    if (!objectiveIds.Add(entry.objectiveId))
                    {
                        throw new ArgumentException("Quest DTO contains a duplicate objective entry.", nameof(dto));
                    }

                    if (entry.progress < 0 || entry.progress > objective.RequiredCount)
                    {
                        throw new ArgumentOutOfRangeException(nameof(dto), $"Invalid progress for {entry.objectiveId}.");
                    }

                    if (entry.completed != (entry.progress == objective.RequiredCount))
                    {
                        throw new ArgumentException(
                            $"Quest DTO completion does not match progress for {entry.objectiveId}.",
                            nameof(dto));
                    }

                    progress[entry.objectiveId] = entry.progress;
                    if (entry.completed) completed.Add(entry.objectiveId);
                }
            }

            var lives = new List<EnemyLifeId>();
            if (dto.processedEnemyLifeIds != null)
            {
                for (var i = 0; i < dto.processedEnemyLifeIds.Length; i++)
                {
                    if (!Guid.TryParse(dto.processedEnemyLifeIds[i], out var guid) || guid == Guid.Empty)
                    {
                        throw new ArgumentException("Quest DTO contains an invalid processed enemy life id.", nameof(dto));
                    }

                    lives.Add(new EnemyLifeId(guid));
                }
            }

            return QuestState.Restore(
                catalog,
                new QuestSnapshot(
                    dto.questId,
                    progress,
                    completed,
                    lives,
                    false,
                    false));
        }
    }
}
