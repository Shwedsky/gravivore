using System;

namespace Gravivore.Gameplay.Combat
{
    public static class DamageResolver
    {
        private const float ArmorScale = 100f;

        public static float ResolvePhysicalDamage(float rawDamage, float armor)
        {
            ValidateFiniteNonNegative(rawDamage, nameof(rawDamage));
            ValidateFiniteNonNegative(armor, nameof(armor));
            return Math.Max(1f, rawDamage * ArmorScale / (ArmorScale + armor));
        }

        private static void ValidateFiniteNonNegative(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}
