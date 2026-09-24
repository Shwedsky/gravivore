using System;
using Gravivore.Core.Stats;

namespace Gravivore.Gameplay.Player
{
    public readonly struct PlayerStatsConfiguration
    {
        public PlayerStatsConfiguration(
            StatCurve powerCurve,
            StatCurve hullCurve,
            StatCurve armorCurve,
            StatCurve fluxCurve,
            StatCurve mobilityCurve,
            float minimumAttackInterval,
            float maximumMoveSpeed,
            PlayerStatLevels startingLevels)
        {
            ValidateCurve(powerCurve, nameof(powerCurve));
            ValidateCurve(hullCurve, nameof(hullCurve));
            ValidateCurve(armorCurve, nameof(armorCurve));
            ValidateCurve(fluxCurve, nameof(fluxCurve));
            ValidateCurve(mobilityCurve, nameof(mobilityCurve));
            ValidateDerivedCurveRanges(powerCurve, hullCurve, armorCurve, fluxCurve, mobilityCurve);
            ValidatePositiveFinite(minimumAttackInterval, nameof(minimumAttackInterval));
            ValidatePositiveFinite(maximumMoveSpeed, nameof(maximumMoveSpeed));

            PowerCurve = powerCurve;
            HullCurve = hullCurve;
            ArmorCurve = armorCurve;
            FluxCurve = fluxCurve;
            MobilityCurve = mobilityCurve;
            MinimumAttackInterval = minimumAttackInterval;
            MaximumMoveSpeed = maximumMoveSpeed;
            StartingLevels = startingLevels;

            ValidateLevels(startingLevels);
        }

        public StatCurve PowerCurve { get; }

        public StatCurve HullCurve { get; }

        public StatCurve ArmorCurve { get; }

        public StatCurve FluxCurve { get; }

        public StatCurve MobilityCurve { get; }

        public float MinimumAttackInterval { get; }

        public float MaximumMoveSpeed { get; }

        public PlayerStatLevels StartingLevels { get; }

        public StatCurve GetCurve(PlayerStatType stat)
        {
            switch (stat)
            {
                case PlayerStatType.Power:
                    return PowerCurve;
                case PlayerStatType.Hull:
                    return HullCurve;
                case PlayerStatType.Armor:
                    return ArmorCurve;
                case PlayerStatType.Flux:
                    return FluxCurve;
                case PlayerStatType.Mobility:
                    return MobilityCurve;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stat), stat, "Unknown player stat.");
            }
        }

        public void ValidateLevel(PlayerStatType stat, int level)
        {
            GetCurve(stat).Evaluate(level);
        }

        public void ValidateLevels(PlayerStatLevels levels)
        {
            PowerCurve.Evaluate(levels.Power);
            HullCurve.Evaluate(levels.Hull);
            ArmorCurve.Evaluate(levels.Armor);
            FluxCurve.Evaluate(levels.Flux);
            MobilityCurve.Evaluate(levels.Mobility);
        }

        public PlayerDerivedStats ApplyHardCaps(PlayerDerivedStats values)
        {
            return new PlayerDerivedStats(
                values.BaseDamage,
                values.MaxHp,
                values.ArmorValue,
                Math.Max(MinimumAttackInterval, values.AttackInterval),
                Math.Min(MaximumMoveSpeed, values.MoveSpeed));
        }

        private static void ValidateCurve(StatCurve curve, string parameterName)
        {
            if (curve.MaximumLevel < 1)
            {
                throw new ArgumentException("A configured stat curve is required.", parameterName);
            }
        }

        private static void ValidateDerivedCurveRanges(
            StatCurve powerCurve,
            StatCurve hullCurve,
            StatCurve armorCurve,
            StatCurve fluxCurve,
            StatCurve mobilityCurve)
        {
            if (powerCurve.MinimumValue < 0f)
            {
                throw new ArgumentException("Power curve values cannot be negative.", nameof(powerCurve));
            }

            if (hullCurve.MinimumValue <= 0f)
            {
                throw new ArgumentException("Hull curve values must be positive.", nameof(hullCurve));
            }

            if (armorCurve.MinimumValue < 0f)
            {
                throw new ArgumentException("Armor curve values cannot be negative.", nameof(armorCurve));
            }

            if (fluxCurve.MinimumValue <= 0f)
            {
                throw new ArgumentException("Flux curve values must be positive.", nameof(fluxCurve));
            }

            if (mobilityCurve.MinimumValue < 0f)
            {
                throw new ArgumentException("Mobility curve values cannot be negative.", nameof(mobilityCurve));
            }
        }

        private static void ValidatePositiveFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be finite and positive.");
            }
        }
    }
}
