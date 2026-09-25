using System;

namespace Gravivore.Gameplay.Combat
{
    public static class SafePullMath
    {
        public static float CalculateAllowedTravelDistance(
            float requestedDistance,
            bool hasBlocker,
            float blockerDistance,
            float clearance)
        {
            ValidateNonNegative(requestedDistance, nameof(requestedDistance));
            ValidateNonNegative(clearance, nameof(clearance));

            if (!hasBlocker)
            {
                return requestedDistance;
            }

            ValidateNonNegative(blockerDistance, nameof(blockerDistance));
            return Math.Min(requestedDistance, Math.Max(0f, blockerDistance - clearance));
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
