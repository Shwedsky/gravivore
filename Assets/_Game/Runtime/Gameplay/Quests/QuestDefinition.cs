using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gravivore.Gameplay.Quests
{
    [Serializable]
    public sealed class QuestObjectiveDefinition
    {
        [SerializeField] private string _id;
        [SerializeField] private string _title;
        [SerializeField] private QuestObjectiveType _type;
        [SerializeField, Min(1)] private int _requiredCount = 1;
        [SerializeField] private string _enemyId;
        [SerializeField] private string _spotId;
        [SerializeField] private string _encounterId;
        [SerializeField] private QuestTargetType _targetType;
        [SerializeField] private string _targetId;
        [SerializeField] private bool _expandsObjectiveUi;

        public QuestObjective Objective => new QuestObjective(
            _id,
            _title,
            _type,
            _requiredCount,
            _enemyId,
            _spotId,
            _encounterId,
            _targetType,
            _targetId,
            _expandsObjectiveUi);
    }

    public sealed class QuestCatalog
    {
        private readonly QuestObjective[] _objectives;
        private readonly Dictionary<string, int> _indexByObjectiveId;

        public QuestCatalog(string questId, IReadOnlyList<QuestObjective> objectives)
        {
            if (string.IsNullOrWhiteSpace(questId)) throw new ArgumentException("Quest id is required.", nameof(questId));
            if (objectives == null || objectives.Count == 0) throw new ArgumentException("At least one objective is required.", nameof(objectives));
            QuestId = questId.Trim();
            _objectives = new QuestObjective[objectives.Count];
            _indexByObjectiveId = new Dictionary<string, int>(objectives.Count, StringComparer.Ordinal);
            for (var i = 0; i < objectives.Count; i++)
            {
                var objective = objectives[i];
                if (_indexByObjectiveId.ContainsKey(objective.Id))
                {
                    throw new ArgumentException($"Duplicate objective id: {objective.Id}.", nameof(objectives));
                }

                _indexByObjectiveId.Add(objective.Id, i);
                _objectives[i] = objective;
            }
        }

        public string QuestId { get; }
        public int ObjectiveCount => _objectives.Length;
        public QuestObjective GetObjective(int index) => _objectives[index];
        public bool TryGetObjective(string objectiveId, out QuestObjective objective)
        {
            if (!string.IsNullOrWhiteSpace(objectiveId) && _indexByObjectiveId.TryGetValue(objectiveId, out var index))
            {
                objective = _objectives[index];
                return true;
            }

            objective = default;
            return false;
        }

        public bool ContainsObjective(string objectiveId) => _indexByObjectiveId.ContainsKey(objectiveId);
    }

    [CreateAssetMenu(fileName = "QuestDefinition", menuName = "Gravivore/Quests/Quest Definition")]
    public sealed class QuestDefinition : ScriptableObject
    {
        [SerializeField] private string _questId = "chapter01-onboarding";
        [SerializeField] private QuestObjectiveDefinition[] _objectives = Array.Empty<QuestObjectiveDefinition>();

        public QuestCatalog Catalog => CreateCatalog();

        public QuestCatalog CreateCatalog()
        {
            if (_objectives == null) throw new InvalidOperationException("Quest objectives are required.");
            var objectives = new QuestObjective[_objectives.Length];
            for (var i = 0; i < objectives.Length; i++)
            {
                if (_objectives[i] == null) throw new InvalidOperationException($"Quest objective {i} is missing.");
                objectives[i] = _objectives[i].Objective;
            }

            return new QuestCatalog(_questId, objectives);
        }

        public void ValidateOrThrow() => _ = CreateCatalog();
    }
}
