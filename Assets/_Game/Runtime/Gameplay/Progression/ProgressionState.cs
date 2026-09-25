using System;
using System.Collections.Generic;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;

namespace Gravivore.Gameplay.Progression
{
    public sealed class ProgressionState
    {
        private const int StatCount = 5;

        private readonly float[] _statExperience = new float[StatCount];
        private readonly HashSet<string> _firstKills = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<EnemyLifeId> _processedLives = new HashSet<EnemyLifeId>();

        public long TotalAssimilationScore { get; private set; }

        public int ProcessedLifeCount => _processedLives.Count;

        public float GetStatExperience(PlayerStatType stat)
        {
            return _statExperience[GetStatIndex(stat)];
        }

        public bool HasFirstKill(string enemyId)
        {
            if (string.IsNullOrWhiteSpace(enemyId))
            {
                throw new ArgumentException("Enemy id is required.", nameof(enemyId));
            }

            return _firstKills.Contains(enemyId);
        }

        internal bool HasProcessed(EnemyLifeId lifeId)
        {
            return _processedLives.Contains(lifeId);
        }

        internal void Commit(
            EnemyLifeId lifeId,
            string enemyId,
            PlayerStatType stat,
            float statExperience,
            long totalAssimilationScore,
            bool isFirstKill)
        {
            if (!_processedLives.Add(lifeId))
            {
                throw new InvalidOperationException("Enemy life was already processed.");
            }

            _statExperience[GetStatIndex(stat)] = statExperience;
            TotalAssimilationScore = totalAssimilationScore;
            if (isFirstKill && !_firstKills.Add(enemyId))
            {
                throw new InvalidOperationException("First-kill state is inconsistent.");
            }
        }

        private static int GetStatIndex(PlayerStatType stat)
        {
            var index = (int)stat;
            if (index < 0 || index >= StatCount)
            {
                throw new ArgumentOutOfRangeException(nameof(stat));
            }

            return index;
        }
    }
}
