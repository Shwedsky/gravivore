using System;
using Gravivore.Gameplay.Player;

namespace Gravivore.Gameplay.Progression
{
    public readonly struct CoreReward
    {
        public CoreReward(string enemyId, PlayerStatType stat, float statExperience, long assimilationScore)
        {
            if (string.IsNullOrWhiteSpace(enemyId))
            {
                throw new ArgumentException("Enemy id is required.", nameof(enemyId));
            }

            if (!Enum.IsDefined(typeof(PlayerStatType), stat))
            {
                throw new ArgumentOutOfRangeException(nameof(stat));
            }

            if (float.IsNaN(statExperience) || float.IsInfinity(statExperience) || statExperience <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(statExperience));
            }

            if (assimilationScore <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(assimilationScore));
            }

            EnemyId = enemyId;
            Stat = stat;
            StatExperience = statExperience;
            AssimilationScore = assimilationScore;
        }

        public string EnemyId { get; }

        public PlayerStatType Stat { get; }

        public float StatExperience { get; }

        public long AssimilationScore { get; }
    }
}
