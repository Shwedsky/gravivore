using Gravivore.Core.Stats;
using UnityEngine;

namespace Gravivore.Gameplay.Player
{
    [CreateAssetMenu(fileName = "StatCurve", menuName = "Gravivore/Player/Stat Curve")]
    public sealed class StatCurveDefinition : ScriptableObject
    {
        [SerializeField, Min(1)] private int _maximumLevel = 1000;
        [SerializeField] private float _levelOneValue = 1f;
        [SerializeField] private float _linearGrowthPerLevel = 1f;
        [SerializeField] private float _quadraticGrowthPerLevelSquared;
        [SerializeField] private float _minimumValue;
        [SerializeField] private float _maximumValue = 1000000f;

        public StatCurve Curve => new StatCurve(
            _maximumLevel,
            _levelOneValue,
            _linearGrowthPerLevel,
            _quadraticGrowthPerLevelSquared,
            _minimumValue,
            _maximumValue);

        public void ValidateOrThrow()
        {
            _ = Curve;
        }
    }
}
