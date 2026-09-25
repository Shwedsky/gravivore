using System;

namespace Gravivore.Gameplay.Combat
{
    public enum DamageType
    {
        Gravity = 0,
        Physical = 1
    }

    public readonly struct DamageRequest
    {
        public DamageRequest(float rawDamage, DamageType damageType)
        {
            if (float.IsNaN(rawDamage) || float.IsInfinity(rawDamage) || rawDamage < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(rawDamage));
            }

            RawDamage = rawDamage;
            DamageType = damageType;
        }

        public float RawDamage { get; }

        public DamageType DamageType { get; }
    }

    public readonly struct DamageResult
    {
        public DamageResult(float appliedDamage, bool wasLethal)
        {
            if (float.IsNaN(appliedDamage) || float.IsInfinity(appliedDamage) || appliedDamage < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(appliedDamage));
            }

            AppliedDamage = appliedDamage;
            WasLethal = wasLethal;
        }

        public float AppliedDamage { get; }

        public bool WasLethal { get; }
    }
}
