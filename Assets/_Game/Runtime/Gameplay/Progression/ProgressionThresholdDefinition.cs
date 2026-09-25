using UnityEngine;

namespace Gravivore.Gameplay.Progression
{
    [CreateAssetMenu(fileName = "ProgressionThreshold", menuName = "Gravivore/Progression/Threshold Curve")]
    public sealed class ProgressionThresholdDefinition : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float _levelOneCost = 3f;
        [SerializeField, Min(0f)] private float _linearGrowthPerLevel = 1f;
        [SerializeField, Min(0f)] private float _quadraticGrowthPerLevelSquared;
        [SerializeField, Min(0.01f)] private float _maximumCost = 1000000f;

        public ProgressionThresholdCurve Curve => new ProgressionThresholdCurve(
            _levelOneCost,
            _linearGrowthPerLevel,
            _quadraticGrowthPerLevelSquared,
            _maximumCost);

        public void ValidateOrThrow()
        {
            _ = Curve;
        }
    }
}
