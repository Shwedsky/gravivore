using System;

namespace Gravivore.Gameplay.Progression
{
    public readonly struct ProgressionThresholdCurve
    {
        public ProgressionThresholdCurve(
            float levelOneCost,
            float linearGrowthPerLevel,
            float quadraticGrowthPerLevelSquared,
            float maximumCost)
        {
            ValidateFinitePositive(levelOneCost, nameof(levelOneCost));
            ValidateFiniteNonNegative(linearGrowthPerLevel, nameof(linearGrowthPerLevel));
            ValidateFiniteNonNegative(quadraticGrowthPerLevelSquared, nameof(quadraticGrowthPerLevelSquared));
            ValidateFinitePositive(maximumCost, nameof(maximumCost));
            if (maximumCost < levelOneCost)
            {
                throw new ArgumentException("Maximum cost cannot be lower than the level-one cost.");
            }

            LevelOneCost = levelOneCost;
            LinearGrowthPerLevel = linearGrowthPerLevel;
            QuadraticGrowthPerLevelSquared = quadraticGrowthPerLevelSquared;
            MaximumCost = maximumCost;
        }

        public float LevelOneCost { get; }

        public float LinearGrowthPerLevel { get; }

        public float QuadraticGrowthPerLevelSquared { get; }

        public float MaximumCost { get; }

        public float Evaluate(int currentLevel)
        {
            if (currentLevel < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(currentLevel));
            }

            var offset = (double)currentLevel - 1d;
            var cost = LevelOneCost +
                       (LinearGrowthPerLevel * offset) +
                       (QuadraticGrowthPerLevelSquared * offset * offset);
            return (float)Math.Min(MaximumCost, cost);
        }

        private static void ValidateFinitePositive(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }

        private static void ValidateFiniteNonNegative(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }
    }
}
