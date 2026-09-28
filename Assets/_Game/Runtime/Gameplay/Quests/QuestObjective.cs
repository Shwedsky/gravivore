using System;

namespace Gravivore.Gameplay.Quests
{
    public readonly struct QuestObjective
    {
        public QuestObjective(
            string id,
            string title,
            QuestObjectiveType type,
            int requiredCount,
            string enemyId,
            string spotId,
            string encounterId,
            QuestTargetType targetType,
            string targetId,
            bool expandsObjectiveUi)
        {
            Id = RequireId(id, nameof(id));
            Title = string.IsNullOrWhiteSpace(title) ? Id : title;
            Type = type;
            if (requiredCount < 1) throw new ArgumentOutOfRangeException(nameof(requiredCount));
            RequiredCount = requiredCount;
            EnemyId = NormalizeOptional(enemyId);
            SpotId = NormalizeOptional(spotId);
            EncounterId = NormalizeOptional(encounterId);
            TargetType = targetType;
            TargetId = NormalizeOptional(targetId);
            ExpandsObjectiveUi = expandsObjectiveUi;
        }

        public string Id { get; }
        public string Title { get; }
        public QuestObjectiveType Type { get; }
        public int RequiredCount { get; }
        public string EnemyId { get; }
        public string SpotId { get; }
        public string EncounterId { get; }
        public QuestTargetType TargetType { get; }
        public string TargetId { get; }
        public bool ExpandsObjectiveUi { get; }

        public bool IsIntroductorySpotObjective =>
            Type == QuestObjectiveType.EnemyDefeated &&
            !string.IsNullOrEmpty(SpotId) &&
            !string.IsNullOrEmpty(EnemyId);

        private static string RequireId(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A stable id is required.", parameterName);
            }

            return value.Trim();
        }

        private static string NormalizeOptional(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
