using System;
using UnityEngine;

namespace Gravivore.Gameplay.Progression
{
    [CreateAssetMenu(fileName = "PlayerProgression", menuName = "Gravivore/Progression/Player Progression")]
    public sealed class PlayerProgressionDefinition : ScriptableObject
    {
        [SerializeField] private ProgressionThresholdDefinition _thresholds;
        [SerializeField] private CoreRewardDefinition[] _coreRewards = Array.Empty<CoreRewardDefinition>();

        public ProgressionConfiguration Configuration
        {
            get
            {
                if (_thresholds == null)
                {
                    throw new InvalidOperationException("A progression threshold definition is required.");
                }

                if (_coreRewards == null || _coreRewards.Length == 0)
                {
                    throw new InvalidOperationException("At least one core reward definition is required.");
                }

                var rewards = new CoreReward[_coreRewards.Length];
                for (var i = 0; i < _coreRewards.Length; i++)
                {
                    if (_coreRewards[i] == null)
                    {
                        throw new InvalidOperationException($"Core reward definition {i} is not assigned.");
                    }

                    rewards[i] = _coreRewards[i].Reward;
                }

                return new ProgressionConfiguration(_thresholds.Curve, rewards);
            }
        }

        public void ValidateOrThrow()
        {
            _ = Configuration;
        }
    }
}
