using System;

namespace Gravivore.Core.Stats
{
    public readonly struct StatCurve
    {
        public StatCurve(
            int maximumLevel,
            float levelOneValue,
            float linearGrowthPerLevel,
            float quadraticGrowthPerLevelSquared,
            float minimumValue,
            float maximumValue)
        {
            if (maximumLevel < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumLevel));
            }

            ValidateFinite(levelOneValue, nameof(levelOneValue));
            ValidateFinite(linearGrowthPerLevel, nameof(linearGrowthPerLevel));
            ValidateFinite(quadraticGrowthPerLevelSquared, nameof(quadraticGrowthPerLevelSquared));
            ValidateFinite(minimumValue, nameof(minimumValue));
            ValidateFinite(maximumValue, nameof(maximumValue));

            if (minimumValue > maximumValue)
            {
                throw new ArgumentException("Minimum curve value cannot exceed maximum curve value.");
            }

            MaximumLevel = maximumLevel;
            LevelOneValue = levelOneValue;
            LinearGrowthPerLevel = linearGrowthPerLevel;
            QuadraticGrowthPerLevelSquared = quadraticGrowthPerLevelSquared;
            MinimumValue = minimumValue;
            MaximumValue = maximumValue;
        }

        public int MaximumLevel { get; }

        public float LevelOneValue { get; }

        public float LinearGrowthPerLevel { get; }

        public float QuadraticGrowthPerLevelSquared { get; }

        public float MinimumValue { get; }

        public float MaximumValue { get; }

        public float Evaluate(int level)
        {
            if (level < 1 || level > MaximumLevel)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            var offset = (double)level - 1d;
            var value = LevelOneValue +
                        (LinearGrowthPerLevel * offset) +
                        (QuadraticGrowthPerLevelSquared * offset * offset);
            return (float)Math.Min(MaximumValue, Math.Max(MinimumValue, value));
        }

        private static void ValidateFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentException("Curve values must be finite.", parameterName);
            }
        }
    }
}
