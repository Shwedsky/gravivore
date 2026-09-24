using System;
using UnityEngine;

namespace Gravivore.Gameplay.Player
{
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "Gravivore/Player/Stats")]
    public sealed class PlayerStatsDefinition : ScriptableObject
    {
        [SerializeField] private StatCurveDefinition _powerCurve;
        [SerializeField] private StatCurveDefinition _hullCurve;
        [SerializeField] private StatCurveDefinition _armorCurve;
        [SerializeField] private StatCurveDefinition _fluxCurve;
        [SerializeField] private StatCurveDefinition _mobilityCurve;
        [SerializeField, Min(0.01f)] private float _minimumAttackInterval = 0.2f;
        [SerializeField, Min(0.01f)] private float _maximumMoveSpeed = 8.5f;
        [SerializeField, Min(1)] private int _startingPowerLevel = 1;
        [SerializeField, Min(1)] private int _startingHullLevel = 1;
        [SerializeField, Min(1)] private int _startingArmorLevel = 1;
        [SerializeField, Min(1)] private int _startingFluxLevel = 1;
        [SerializeField, Min(1)] private int _startingMobilityLevel = 1;

        public PlayerStatsConfiguration Configuration
        {
            get
            {
                ValidateReferences();
                return new PlayerStatsConfiguration(
                    _powerCurve.Curve,
                    _hullCurve.Curve,
                    _armorCurve.Curve,
                    _fluxCurve.Curve,
                    _mobilityCurve.Curve,
                    _minimumAttackInterval,
                    _maximumMoveSpeed,
                    new PlayerStatLevels(
                        _startingPowerLevel,
                        _startingHullLevel,
                        _startingArmorLevel,
                        _startingFluxLevel,
                        _startingMobilityLevel));
            }
        }

        public PlayerStatsState CreateState()
        {
            var configuration = Configuration;
            return new PlayerStatsState(configuration, configuration.StartingLevels);
        }

        public void ValidateOrThrow()
        {
            _ = Configuration;
        }

        private void ValidateReferences()
        {
            if (_powerCurve == null || _hullCurve == null || _armorCurve == null ||
                _fluxCurve == null || _mobilityCurve == null)
            {
                throw new InvalidOperationException("Player stats require all five stat curve definitions.");
            }
        }
    }
}
