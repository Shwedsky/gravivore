using System;
using Gravivore.Gameplay.Player;

namespace Gravivore.Gameplay.Progression
{
    public sealed class EvolutionConfiguration
    {
        private const int StatCount = 5;
        private readonly PlayerStatType[] _dominancePriority;

        public EvolutionConfiguration(
            long tier1Threshold,
            long tier2Threshold,
            PlayerStatType[] dominancePriority)
        {
            if (tier1Threshold <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tier1Threshold));
            }

            if (tier2Threshold <= tier1Threshold)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(tier2Threshold),
                    "Tier thresholds must be strictly increasing.");
            }

            if (dominancePriority == null || dominancePriority.Length != StatCount)
            {
                throw new ArgumentException("Dominance priority must contain all five stats.", nameof(dominancePriority));
            }

            var seen = new bool[StatCount];
            _dominancePriority = new PlayerStatType[StatCount];
            for (var i = 0; i < dominancePriority.Length; i++)
            {
                var statIndex = (int)dominancePriority[i];
                if (statIndex < 0 || statIndex >= StatCount || seen[statIndex])
                {
                    throw new ArgumentException(
                        "Dominance priority must contain each player stat exactly once.",
                        nameof(dominancePriority));
                }

                seen[statIndex] = true;
                _dominancePriority[i] = dominancePriority[i];
            }

            Tier1Threshold = tier1Threshold;
            Tier2Threshold = tier2Threshold;
        }

        public long Tier1Threshold { get; }

        public long Tier2Threshold { get; }

        public int DominancePriorityCount => _dominancePriority.Length;

        public PlayerStatType GetDominancePriority(int index)
        {
            if (index < 0 || index >= _dominancePriority.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _dominancePriority[index];
        }
    }
}
