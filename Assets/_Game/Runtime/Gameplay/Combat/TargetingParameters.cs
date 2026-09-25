using System;

namespace Gravivore.Gameplay.Combat
{
    public readonly struct TargetingParameters
    {
        public TargetingParameters(
            float acquisitionRadius,
            float releaseRadius,
            float distanceWeight,
            float frontBiasWeight,
            float switchScoreAdvantage)
        {
            ValidatePositiveFinite(acquisitionRadius, nameof(acquisitionRadius));
            ValidatePositiveFinite(releaseRadius, nameof(releaseRadius));
            ValidateNonNegativeFinite(distanceWeight, nameof(distanceWeight));
            ValidateNonNegativeFinite(frontBiasWeight, nameof(frontBiasWeight));
            ValidateNonNegativeFinite(switchScoreAdvantage, nameof(switchScoreAdvantage));

            if (releaseRadius < acquisitionRadius)
            {
                throw new ArgumentException("Release radius cannot be smaller than acquisition radius.");
            }

            if (distanceWeight <= 0f && frontBiasWeight <= 0f)
            {
                throw new ArgumentException("Target scoring requires a positive distance or front-bias weight.");
            }

            AcquisitionRadius = acquisitionRadius;
            ReleaseRadius = releaseRadius;
            DistanceWeight = distanceWeight;
            FrontBiasWeight = frontBiasWeight;
            SwitchScoreAdvantage = switchScoreAdvantage;
        }

        public float AcquisitionRadius { get; }

        public float ReleaseRadius { get; }

        public float DistanceWeight { get; }

        public float FrontBiasWeight { get; }

        public float SwitchScoreAdvantage { get; }

        private static void ValidatePositiveFinite(float value, string parameterName)
        {
            ValidateNonNegativeFinite(value, parameterName);
            if (value <= 0f)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        private static void ValidateNonNegativeFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}
