using System;
using System.Collections.Generic;

namespace Gravivore.Gameplay.Progression
{
    public sealed class ProgressionConfiguration
    {
        private readonly Dictionary<string, CoreReward> _rewards;

        public ProgressionConfiguration(
            ProgressionThresholdCurve thresholdCurve,
            IReadOnlyList<CoreReward> rewards)
        {
            _ = thresholdCurve.Evaluate(1);
            if (rewards == null || rewards.Count == 0)
            {
                throw new ArgumentException("At least one core reward is required.", nameof(rewards));
            }

            ThresholdCurve = thresholdCurve;
            _rewards = new Dictionary<string, CoreReward>(rewards.Count, StringComparer.Ordinal);
            for (var i = 0; i < rewards.Count; i++)
            {
                var reward = rewards[i];
                if (!_rewards.TryAdd(reward.EnemyId, reward))
                {
                    throw new ArgumentException($"Duplicate enemy reward route: {reward.EnemyId}.", nameof(rewards));
                }
            }
        }

        public ProgressionThresholdCurve ThresholdCurve { get; }

        public int RewardCount => _rewards.Count;

        public bool TryGetReward(string enemyId, out CoreReward reward)
        {
            if (string.IsNullOrWhiteSpace(enemyId))
            {
                reward = default;
                return false;
            }

            return _rewards.TryGetValue(enemyId, out reward);
        }
    }
}
