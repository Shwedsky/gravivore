using System;
using Gravivore.Gameplay.Player;

namespace Gravivore.Gameplay.Progression
{
    public readonly struct EvolutionVisualState : IEquatable<EvolutionVisualState>
    {
        public EvolutionVisualState(EvolutionTier tier, PlayerStatType dominantStat)
        {
            Tier = tier;
            DominantStat = dominantStat;
        }

        public EvolutionTier Tier { get; }

        public PlayerStatType DominantStat { get; }

        public bool Equals(EvolutionVisualState other)
        {
            return Tier == other.Tier && DominantStat == other.DominantStat;
        }

        public override bool Equals(object obj)
        {
            return obj is EvolutionVisualState other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ((int)Tier * 397) ^ (int)DominantStat;
        }
    }

    public static class EvolutionStateSelector
    {
        public static EvolutionTier SelectTier(long totalAssimilationScore, EvolutionConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (totalAssimilationScore < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalAssimilationScore));
            }

            if (totalAssimilationScore >= configuration.Tier2Threshold)
            {
                return EvolutionTier.Tier2;
            }

            return totalAssimilationScore >= configuration.Tier1Threshold
                ? EvolutionTier.Tier1
                : EvolutionTier.Tier0;
        }

        public static PlayerStatType SelectDominantStat(
            PlayerStatLevels levels,
            EvolutionConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var dominant = configuration.GetDominancePriority(0);
            var highestLevel = levels.GetLevel(dominant);
            for (var i = 1; i < configuration.DominancePriorityCount; i++)
            {
                var candidate = configuration.GetDominancePriority(i);
                var candidateLevel = levels.GetLevel(candidate);
                if (candidateLevel > highestLevel)
                {
                    dominant = candidate;
                    highestLevel = candidateLevel;
                }
            }

            return dominant;
        }

        public static EvolutionVisualState Select(
            long totalAssimilationScore,
            PlayerStatLevels levels,
            EvolutionConfiguration configuration)
        {
            return new EvolutionVisualState(
                SelectTier(totalAssimilationScore, configuration),
                SelectDominantStat(levels, configuration));
        }
    }
}
