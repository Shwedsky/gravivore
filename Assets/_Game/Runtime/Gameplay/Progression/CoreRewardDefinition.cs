using Gravivore.Gameplay.Player;
using UnityEngine;

namespace Gravivore.Gameplay.Progression
{
    [CreateAssetMenu(fileName = "CoreReward", menuName = "Gravivore/Progression/Core Reward")]
    public sealed class CoreRewardDefinition : ScriptableObject
    {
        [SerializeField] private string _enemyId = "ordinary-enemy";
        [SerializeField] private PlayerStatType _stat;
        [SerializeField, Min(0.01f)] private float _statExperience = 1f;
        [SerializeField, Min(1)] private long _assimilationScore = 1;

        public CoreReward Reward => new CoreReward(_enemyId, _stat, _statExperience, _assimilationScore);

        public void ValidateOrThrow()
        {
            _ = Reward;
        }
    }
}
