using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using UnityEngine;

namespace Gravivore.Gameplay.Progression
{
    public readonly struct CoreRewardGrantedEvent
    {
        public CoreRewardGrantedEvent(
            EnemyLifeId lifeId,
            string enemyId,
            Vector3 worldPosition,
            PlayerStatType stat,
            float grantedExperience,
            long grantedAssimilationScore,
            int previousLevel,
            int currentLevel,
            float remainingExperience,
            bool isFirstKill)
        {
            LifeId = lifeId;
            EnemyId = enemyId;
            WorldPosition = worldPosition;
            Stat = stat;
            GrantedExperience = grantedExperience;
            GrantedAssimilationScore = grantedAssimilationScore;
            PreviousLevel = previousLevel;
            CurrentLevel = currentLevel;
            RemainingExperience = remainingExperience;
            IsFirstKill = isFirstKill;
        }

        public EnemyLifeId LifeId { get; }
        public string EnemyId { get; }
        public Vector3 WorldPosition { get; }
        public PlayerStatType Stat { get; }
        public float GrantedExperience { get; }
        public long GrantedAssimilationScore { get; }
        public int PreviousLevel { get; }
        public int CurrentLevel { get; }
        public int LevelsGained => CurrentLevel - PreviousLevel;
        public float RemainingExperience { get; }
        public bool IsFirstKill { get; }
    }

    public readonly struct StatExperienceChangedEvent
    {
        public StatExperienceChangedEvent(
            PlayerStatType stat,
            float previousExperience,
            float currentExperience,
            int previousLevel,
            int currentLevel)
        {
            Stat = stat;
            PreviousExperience = previousExperience;
            CurrentExperience = currentExperience;
            PreviousLevel = previousLevel;
            CurrentLevel = currentLevel;
        }

        public PlayerStatType Stat { get; }
        public float PreviousExperience { get; }
        public float CurrentExperience { get; }
        public int PreviousLevel { get; }
        public int CurrentLevel { get; }
    }

    public readonly struct FirstEnemyKillEvent
    {
        public FirstEnemyKillEvent(string enemyId, PlayerStatType rewardedStat, Vector3 worldPosition)
        {
            EnemyId = enemyId;
            RewardedStat = rewardedStat;
            WorldPosition = worldPosition;
        }

        public string EnemyId { get; }
        public PlayerStatType RewardedStat { get; }
        public Vector3 WorldPosition { get; }
    }

    public readonly struct ProgressionDirtyEvent
    {
        public ProgressionDirtyEvent(EnemyLifeId sourceLifeId, long totalAssimilationScore)
        {
            SourceLifeId = sourceLifeId;
            TotalAssimilationScore = totalAssimilationScore;
        }

        public EnemyLifeId SourceLifeId { get; }
        public long TotalAssimilationScore { get; }
    }
}
