using System;
using System.Collections.Generic;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;

namespace Gravivore.Gameplay.Progression
{
    public readonly struct ProgressionSnapshot
    {
        public ProgressionSnapshot(
            float powerExperience,
            float hullExperience,
            float armorExperience,
            float fluxExperience,
            float mobilityExperience,
            long totalAssimilationScore,
            IReadOnlyCollection<string> firstKillEnemyIds)
        {
            PowerExperience = powerExperience;
            HullExperience = hullExperience;
            ArmorExperience = armorExperience;
            FluxExperience = fluxExperience;
            MobilityExperience = mobilityExperience;
            TotalAssimilationScore = totalAssimilationScore;
            FirstKillEnemyIds = firstKillEnemyIds;
        }

        public float PowerExperience { get; }
        public float HullExperience { get; }
        public float ArmorExperience { get; }
        public float FluxExperience { get; }
        public float MobilityExperience { get; }
        public long TotalAssimilationScore { get; }
        public IReadOnlyCollection<string> FirstKillEnemyIds { get; }

        public float GetStatExperience(PlayerStatType stat)
        {
            switch (stat)
            {
                case PlayerStatType.Power: return PowerExperience;
                case PlayerStatType.Hull: return HullExperience;
                case PlayerStatType.Armor: return ArmorExperience;
                case PlayerStatType.Flux: return FluxExperience;
                case PlayerStatType.Mobility: return MobilityExperience;
                default: throw new ArgumentOutOfRangeException(nameof(stat));
            }
        }
    }

    public sealed class ProgressionState
    {
        private const int StatCount = 5;

        private readonly float[] _statExperience = new float[StatCount];
        private readonly HashSet<string> _firstKills = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<EnemyLifeId> _processedLives = new HashSet<EnemyLifeId>();

        public long TotalAssimilationScore { get; private set; }

        public int ProcessedLifeCount => _processedLives.Count;

        public static ProgressionState Restore(in ProgressionSnapshot snapshot)
        {
            if (snapshot.TotalAssimilationScore < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(snapshot), "Total assimilation cannot be negative.");
            }

            var state = new ProgressionState { TotalAssimilationScore = snapshot.TotalAssimilationScore };
            for (var i = 0; i < StatCount; i++)
            {
                var stat = (PlayerStatType)i;
                var experience = snapshot.GetStatExperience(stat);
                if (float.IsNaN(experience) || float.IsInfinity(experience) || experience < 0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(snapshot), $"Invalid experience for {stat}.");
                }

                state._statExperience[i] = experience;
            }

            if (snapshot.FirstKillEnemyIds != null)
            {
                foreach (var enemyId in snapshot.FirstKillEnemyIds)
                {
                    if (string.IsNullOrWhiteSpace(enemyId) || !state._firstKills.Add(enemyId))
                    {
                        throw new ArgumentException("First-kill ids must be non-empty and unique.", nameof(snapshot));
                    }
                }
            }

            return state;
        }

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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        internal bool EnsureMinimumAssimilationScoreForDevelopment(long minimumScore)
        {
            if (minimumScore < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumScore));
            }

            if (TotalAssimilationScore >= minimumScore)
            {
                return false;
            }

            TotalAssimilationScore = minimumScore;
            return true;
        }
#endif

        public ProgressionSnapshot ExportSnapshot()
        {
            var firstKills = new List<string>(_firstKills);
            firstKills.Sort(StringComparer.Ordinal);
            return new ProgressionSnapshot(
                GetStatExperience(PlayerStatType.Power),
                GetStatExperience(PlayerStatType.Hull),
                GetStatExperience(PlayerStatType.Armor),
                GetStatExperience(PlayerStatType.Flux),
                GetStatExperience(PlayerStatType.Mobility),
                TotalAssimilationScore,
                firstKills);
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
