using System;
using Gravivore.Gameplay.Progression;

namespace Gravivore.Gameplay.Encounters
{
    public readonly struct RepeatRewardScaleSet
    {
        public RepeatRewardScaleSet(
            float firstClearScale,
            float premiumRepeatScale,
            float fallbackRepeatScale,
            float firstPremiumWindowBonusScale)
        {
            ValidateScale(firstClearScale, nameof(firstClearScale));
            ValidateScale(premiumRepeatScale, nameof(premiumRepeatScale));
            ValidateScale(fallbackRepeatScale, nameof(fallbackRepeatScale));
            ValidateScale(firstPremiumWindowBonusScale, nameof(firstPremiumWindowBonusScale));
            if (firstPremiumWindowBonusScale < 1f)
                throw new ArgumentOutOfRangeException(nameof(firstPremiumWindowBonusScale));
            if (!(fallbackRepeatScale < premiumRepeatScale && premiumRepeatScale < firstClearScale))
                throw new ArgumentException("Reward scales must preserve fallback < premium < first-clear ordering.");

            FirstClearScale = firstClearScale;
            PremiumRepeatScale = premiumRepeatScale;
            FallbackRepeatScale = fallbackRepeatScale;
            FirstPremiumWindowBonusScale = firstPremiumWindowBonusScale;
        }

        public float FirstClearScale { get; }
        public float PremiumRepeatScale { get; }
        public float FallbackRepeatScale { get; }
        public float FirstPremiumWindowBonusScale { get; }

        private static void ValidateScale(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    /// <summary>
    /// Converts an authored 1R baseline package into the initial Phase 4 encounter bands.
    /// Exact economy numbers remain data-driven: these defaults use the midpoint of the merged
    /// reward-ladder test bands and may be replaced by authored scale sets without domain changes.
    /// </summary>
    public sealed class RepeatableEncounterRewardLadder
    {
        public static readonly RepeatRewardScaleSet DefaultMagnetarScales =
            new RepeatRewardScaleSet(7f, 5f, 1.5f, 1.25f);

        public static readonly RepeatRewardScaleSet DefaultCustodianScales =
            new RepeatRewardScaleSet(12f, 8.5f, 2.5f, 1.25f);

        private readonly CoreReward _magnetarUnitReward;
        private readonly CoreReward _custodianUnitReward;
        private readonly RepeatRewardScaleSet _magnetarScales;
        private readonly RepeatRewardScaleSet _custodianScales;

        public RepeatableEncounterRewardLadder(
            CoreReward magnetarUnitReward,
            CoreReward custodianUnitReward,
            RepeatRewardScaleSet? magnetarScales = null,
            RepeatRewardScaleSet? custodianScales = null)
        {
            _magnetarUnitReward = magnetarUnitReward;
            _custodianUnitReward = custodianUnitReward;
            _magnetarScales = magnetarScales ?? DefaultMagnetarScales;
            _custodianScales = custodianScales ?? DefaultCustodianScales;
        }

        public CoreReward Build(RepeatableEncounterKind kind, EncounterRewardEntitlement entitlement)
        {
            CoreReward unit;
            RepeatRewardScaleSet scales;
            switch (kind)
            {
                case RepeatableEncounterKind.Magnetar:
                    unit = _magnetarUnitReward;
                    scales = _magnetarScales;
                    break;
                case RepeatableEncounterKind.Custodian:
                    unit = _custodianUnitReward;
                    scales = _custodianScales;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }

            var scale = GetScale(scales, entitlement);
            return Scale(unit, scale);
        }

        private static float GetScale(in RepeatRewardScaleSet scales, EncounterRewardEntitlement entitlement)
        {
            switch (entitlement)
            {
                case EncounterRewardEntitlement.FirstClear:
                    return scales.FirstClearScale;
                case EncounterRewardEntitlement.PremiumRepeatFirstInWindow:
                    return scales.PremiumRepeatScale * scales.FirstPremiumWindowBonusScale;
                case EncounterRewardEntitlement.PremiumRepeat:
                    return scales.PremiumRepeatScale;
                case EncounterRewardEntitlement.FallbackRepeat:
                    return scales.FallbackRepeatScale;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entitlement));
            }
        }

        private static CoreReward Scale(in CoreReward unit, float scale)
        {
            var statExperience = unit.StatExperience * scale;
            if (float.IsNaN(statExperience) || float.IsInfinity(statExperience) || statExperience <= 0f)
                throw new OverflowException("Scaled stat experience is invalid.");

            var assimilation = checked((long)Math.Round(
                unit.AssimilationScore * (double)scale,
                MidpointRounding.AwayFromZero));
            if (assimilation <= 0) assimilation = 1;
            return new CoreReward(unit.EnemyId, unit.Stat, statExperience, assimilation);
        }
    }
}
