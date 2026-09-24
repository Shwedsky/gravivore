using System;
using System.Collections.Generic;

namespace Gravivore.Gameplay.Player
{
    public static class PlayerStatsCalculator
    {
        public static PlayerDerivedStats Calculate(
            PlayerStatsConfiguration configuration,
            PlayerStatLevels baseLevels,
            IReadOnlyList<IPlayerDerivedStatsModifier> modifiers = null)
        {
            configuration.ValidateLevels(baseLevels);

            var values = new PlayerDerivedStats(
                configuration.PowerCurve.Evaluate(baseLevels.Power),
                configuration.HullCurve.Evaluate(baseLevels.Hull),
                configuration.ArmorCurve.Evaluate(baseLevels.Armor),
                configuration.FluxCurve.Evaluate(baseLevels.Flux),
                configuration.MobilityCurve.Evaluate(baseLevels.Mobility));

            if (modifiers != null)
            {
                for (var i = 0; i < modifiers.Count; i++)
                {
                    var modifier = modifiers[i];
                    if (modifier == null)
                    {
                        throw new ArgumentException("Stat modifier collections cannot contain null entries.", nameof(modifiers));
                    }

                    values = modifier.Apply(in values);
                }
            }

            return configuration.ApplyHardCaps(values);
        }
    }
}
