using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gravivore.Gameplay.Player
{
    public sealed class PlayerStatsState : IMoveSpeedProvider
    {
        private readonly PlayerStatsConfiguration _configuration;
        private readonly IReadOnlyList<IPlayerDerivedStatsModifier>[] _modifierSources;
        private IReadOnlyList<IPlayerDerivedStatsModifier> _modifiers;
        private PlayerStatLevels _baseLevels;

        public PlayerStatsState(
            PlayerStatsConfiguration configuration,
            PlayerStatLevels baseLevels,
            IReadOnlyList<IPlayerDerivedStatsModifier> modifiers = null)
        {
            _configuration = configuration;
            _modifierSources = new IReadOnlyList<IPlayerDerivedStatsModifier>[2];
            _modifierSources[(int)PlayerStatsModifierSource.External] = SnapshotModifiers(modifiers);
            _modifierSources[(int)PlayerStatsModifierSource.Equipment] = Array.Empty<IPlayerDerivedStatsModifier>();
            _modifiers = CombineModifiers();
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
            Publish(StatChanged, new PlayerStatChange(
                stat,
                previousLevel,
                level,
                previousDerivedStats,
                DerivedStats));
            return true;
        }

        public bool SetModifiers(IReadOnlyList<IPlayerDerivedStatsModifier> modifiers)
        {
            return SetModifiers(PlayerStatsModifierSource.External, modifiers);
        }

        public bool SetModifiers(
            PlayerStatsModifierSource source,
            IReadOnlyList<IPlayerDerivedStatsModifier> modifiers)
        {
            ValidateSource(source);
            var nextModifiers = SnapshotModifiers(modifiers);
            var combinedModifiers = CombineModifiers(source, nextModifiers);
            var nextDerivedStats = PlayerStatsCalculator.Calculate(
                _configuration,
                _baseLevels,
                combinedModifiers);
            _modifierSources[(int)source] = nextModifiers;
            _modifiers = combinedModifiers;
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
            Publish(DerivedStatsChanged, new PlayerDerivedStatsChange(previousValues, nextValues, reason));
            return true;
        }

        private static void Publish<T>(Action<T> handlers, T value)
        {
            if (handlers == null) return;
            var invocationList = handlers.GetInvocationList();
            for (var i = 0; i < invocationList.Length; i++)
            {
                try
                {
                    ((Action<T>)invocationList[i])(value);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
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

        private IReadOnlyList<IPlayerDerivedStatsModifier> CombineModifiers()
        {
            return CombineModifiers(
                PlayerStatsModifierSource.External,
                _modifierSources[(int)PlayerStatsModifierSource.External]);
        }

        private IReadOnlyList<IPlayerDerivedStatsModifier> CombineModifiers(
            PlayerStatsModifierSource overrideSource,
            IReadOnlyList<IPlayerDerivedStatsModifier> overrideModifiers)
        {
            var count = 0;
            for (var sourceIndex = 0; sourceIndex < _modifierSources.Length; sourceIndex++)
            {
                count += sourceIndex == (int)overrideSource
                    ? overrideModifiers.Count
                    : _modifierSources[sourceIndex].Count;
            }

            if (count == 0) return Array.Empty<IPlayerDerivedStatsModifier>();
            var combined = new IPlayerDerivedStatsModifier[count];
            var destinationIndex = 0;
            for (var sourceIndex = 0; sourceIndex < _modifierSources.Length; sourceIndex++)
            {
                var source = sourceIndex == (int)overrideSource
                    ? overrideModifiers
                    : _modifierSources[sourceIndex];
                for (var modifierIndex = 0; modifierIndex < source.Count; modifierIndex++)
                {
                    combined[destinationIndex++] = source[modifierIndex];
                }
            }

            return combined;
        }

        private static void ValidateSource(PlayerStatsModifierSource source)
        {
            if (!Enum.IsDefined(typeof(PlayerStatsModifierSource), source))
            {
                throw new ArgumentOutOfRangeException(nameof(source));
            }
        }
    }
}
