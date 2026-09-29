using Gravivore.Gameplay.Progression;

namespace Gravivore.Gameplay.Quests
{
    public readonly struct QuestObjectiveProgressedEvent
    {
        public QuestObjectiveProgressedEvent(string questId, QuestObjective objective, QuestObjectiveProgress progress)
        {
            QuestId = questId;
            Objective = objective;
            Progress = progress;
        }

        public string QuestId { get; }
        public QuestObjective Objective { get; }
        public QuestObjectiveProgress Progress { get; }
    }

    public readonly struct QuestObjectiveCompletedEvent
    {
        public QuestObjectiveCompletedEvent(string questId, QuestObjective objective)
        {
            QuestId = questId;
            Objective = objective;
        }

        public string QuestId { get; }
        public QuestObjective Objective { get; }
    }

    public readonly struct QuestAssimilationFeedbackEvent
    {
        public QuestAssimilationFeedbackEvent(CoreRewardGrantedEvent reward)
        {
            Reward = reward;
        }

        public CoreRewardGrantedEvent Reward { get; }
    }

    public readonly struct QuestSequenceCompletedEvent
    {
        public QuestSequenceCompletedEvent(string questId) => QuestId = questId;
        public string QuestId { get; }
    }
}
