using System;

namespace Gravivore.Gameplay.Combat
{
    public enum DisplacementClass
    {
        Standard = 0,
        Elite = 1,
        Boss = 2
    }

    public readonly struct DisplacementPolicy
    {
        public DisplacementPolicy(float elitePullFraction)
        {
            if (float.IsNaN(elitePullFraction) || float.IsInfinity(elitePullFraction) ||
                elitePullFraction < 0f || elitePullFraction > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(elitePullFraction));
            }

            ElitePullFraction = elitePullFraction;
        }

        public float ElitePullFraction { get; }

        public float GetPullFraction(DisplacementClass displacementClass)
        {
            switch (displacementClass)
            {
                case DisplacementClass.Standard:
                    return 1f;
                case DisplacementClass.Elite:
                    return ElitePullFraction;
                case DisplacementClass.Boss:
                    return 0f;
                default:
                    throw new ArgumentOutOfRangeException(nameof(displacementClass));
            }
        }

        public float CalculatePullDistance(
            float distanceToSource,
            float stopDistance,
            DisplacementClass displacementClass)
        {
            ValidateNonNegative(distanceToSource, nameof(distanceToSource));
            ValidateNonNegative(stopDistance, nameof(stopDistance));
            var fullPullDistance = Math.Max(0f, distanceToSource - stopDistance);
            return fullPullDistance * GetPullFraction(displacementClass);
        }

        private static void ValidateNonNegative(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}
