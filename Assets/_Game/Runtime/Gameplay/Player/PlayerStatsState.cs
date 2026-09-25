using System;
using System.Collections.Generic;

namespace Gravivore.Gameplay.Player
{
    public sealed class PlayerStatsState : IMoveSpeedProvider
    {
        private readonly PlayerStatsConfiguration _configuration;
        private IReadOnlyList<IPlayerDerivedStatsModifier> _modifiers;
        private PlayerStatLevels _baseLevels;

        public PlayerStatsState(
            PlayerStatsConfiguration configuration,
            PlayerStatLevels baseLevels,
            IReadOnlyList<IPlayerDerivedStatsModifier> modifiers = null)
        {
            _configuration = configuration;
            _modifiers = SnapshotModifiers(modifiers);
            _baseLevels = baseLevels;
            DerivedStats = PlayerStatsCalculator.Calculate(_configuration, _baseLevels, _modifiers);
        }

        public event Action<PlayerStatChange> StatChanged;

        public event Action<PlayerDerivedStatsChange> DerivedStatsChanged;

        public PlayerStatLevels BaseLevels => _baseLevels;

        public PlayerDerivedStats DerivedStats { get; private set; }

        public float MoveSpeed => DerivedStats.MoveSpeed;

        public int GetMaximumLevel(PlayerStatType stat)
        {
            return _configuration.GetCurve(stat).MaximumLevel;
        }

        public bool SetLevel(PlayerStatType stat, int level)
        {
            _configuration.ValidateLevel(stat, level);
            var previousLevel = _baseLevels.GetLevel(stat);
            if (previousLevel == level)
            {
                return false;
            }

            var previousDerivedStats = DerivedStats;
            var nextLevels = _baseLevels.WithLevel(stat, level);
            var nextDerivedStats = PlayerStatsCalculator.Calculate(_configuration, nextLevels, _modifiers);
            _baseLevels = nextLevels;
            SetDerivedStats(nextDerivedStats, PlayerDerivedStatsChangeReason.LevelChanged);
            StatChanged?.Invoke(new PlayerStatChange(
                stat,
                previousLevel,
                level,
                previousDerivedStats,
                DerivedStats));
            return true;
        }

        public bool SetModifiers(IReadOnlyList<IPlayerDerivedStatsModifier> modifiers)
        {
            var nextModifiers = SnapshotModifiers(modifiers);
            var nextDerivedStats = PlayerStatsCalculator.Calculate(
                _configuration,
                _baseLevels,
                nextModifiers);
            _modifiers = nextModifiers;
            return SetDerivedStats(nextDerivedStats, PlayerDerivedStatsChangeReason.ModifiersChanged);
        }

        public bool RecalculateDerivedStats()
        {
            var nextDerivedStats = PlayerStatsCalculator.Calculate(
                _configuration,
                _baseLevels,
                _modifiers);
            return SetDerivedStats(nextDerivedStats, PlayerDerivedStatsChangeReason.Recalculated);
        }

        private bool SetDerivedStats(
            PlayerDerivedStats nextValues,
            PlayerDerivedStatsChangeReason reason)
        {
            var previousValues = DerivedStats;
            if (previousValues.HasSameValues(nextValues))
            {
                return false;
            }

            DerivedStats = nextValues;
            DerivedStatsChanged?.Invoke(new PlayerDerivedStatsChange(previousValues, nextValues, reason));
            return true;
        }

        private static IReadOnlyList<IPlayerDerivedStatsModifier> SnapshotModifiers(
            IReadOnlyList<IPlayerDerivedStatsModifier> modifiers)
        {
            if (modifiers == null || modifiers.Count == 0)
            {
                return Array.Empty<IPlayerDerivedStatsModifier>();
            }

            var snapshot = new IPlayerDerivedStatsModifier[modifiers.Count];
            for (var i = 0; i < modifiers.Count; i++)
            {
                snapshot[i] = modifiers[i] ??
                              throw new ArgumentException(
                                  "Stat modifier collections cannot contain null entries.",
                                  nameof(modifiers));
            }

            return snapshot;
        }
    }
}
