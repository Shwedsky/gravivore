using System;
using Gravivore.Gameplay.Player;

namespace Gravivore.Gameplay.Progression
{
    public sealed class AuthoredRewardApplier
    {
        private readonly PlayerStatsState _playerStats;
        private readonly ProgressionState _state;
        private readonly ProgressionConfiguration _configuration;

        public AuthoredRewardApplier(PlayerStatsState playerStats, ProgressionState state, ProgressionConfiguration configuration)
        {
            _playerStats = playerStats ?? throw new ArgumentNullException(nameof(playerStats));
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void Apply(in CoreReward reward)
        {
            var previousLevel = _playerStats.BaseLevels.GetLevel(reward.Stat);
            var maximumLevel = _playerStats.GetMaximumLevel(reward.Stat);
            var nextExperience = _state.GetStatExperience(reward.Stat) + reward.StatExperience;
            if (float.IsInfinity(nextExperience)) throw new OverflowException("Stat experience overflowed.");

            var nextLevel = previousLevel;
            while (nextLevel < maximumLevel)
            {
                var required = _configuration.ThresholdCurve.Evaluate(nextLevel);
                if (nextExperience < required) break;
                nextExperience -= required;
                nextLevel++;
            }

            if (nextLevel == maximumLevel) nextExperience = 0f;
            var nextTotal = checked(_state.TotalAssimilationScore + reward.AssimilationScore);
            _state.CommitAuthoredReward(reward.Stat, nextExperience, nextTotal);
            if (nextLevel != previousLevel)
            {
                // Stats commit before notifying observers. A failing observer cannot turn a
                // committed reward into a Prepared transaction that would grant it again.
                try { _playerStats.SetLevel(reward.Stat, nextLevel); }
                catch (Exception exception) { UnityEngine.Debug.LogException(exception); }
            }
        }
    }
}
