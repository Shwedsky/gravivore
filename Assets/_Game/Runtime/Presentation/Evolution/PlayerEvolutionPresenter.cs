using System;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using UnityEngine;

namespace Gravivore.Presentation.Evolution
{
    public readonly struct EvolutionTierChangedEvent
    {
        public EvolutionTierChangedEvent(
            EvolutionTier previousTier,
            EvolutionTier currentTier,
            long totalAssimilationScore)
        {
            PreviousTier = previousTier;
            CurrentTier = currentTier;
            TotalAssimilationScore = totalAssimilationScore;
        }

        public EvolutionTier PreviousTier { get; }
        public EvolutionTier CurrentTier { get; }
        public long TotalAssimilationScore { get; }
    }

    public interface IEvolutionVfxHook
    {
        void Play(in EvolutionTierChangedEvent change);
    }

    [DisallowMultipleComponent]
    public sealed class PlayerEvolutionPresenter : MonoBehaviour
    {
        private AssimilationProgressionService _progression;
        private PlayerStatsState _playerStats;
        private EvolutionConfiguration _configuration;
        private IPlayerEvolutionView _view;
        private IEvolutionVfxHook _vfxHook;
        private EvolutionVisualState _currentState;
        private bool _hasCurrentState;
        private bool _isInitialized;

        public event Action<EvolutionTierChangedEvent> TierChanged;

        public EvolutionTier CurrentTier => _currentState.Tier;

        public PlayerStatType CurrentDominantStat => _currentState.DominantStat;

        public void Initialize(
            AssimilationProgressionService progression,
            PlayerStatsState playerStats,
            EvolutionConfiguration configuration,
            IPlayerEvolutionView view,
            IEvolutionVfxHook vfxHook = null)
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException("Player evolution presenter is already initialized.");
            }

            _progression = progression ?? throw new ArgumentNullException(nameof(progression));
            _playerStats = playerStats ?? throw new ArgumentNullException(nameof(playerStats));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _vfxHook = vfxHook;
            _hasCurrentState = false;
            _isInitialized = true;
            try
            {
                ApplyCurrentState();
            }
            catch
            {
                _isInitialized = false;
                throw;
            }

            _progression.Dirty += HandleProgressionDirty;
            _playerStats.StatChanged += HandleStatChanged;
        }

        public bool ApplyCurrentState()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Player evolution presenter must be initialized before use.");
            }

            var nextState = EvolutionStateSelector.Select(
                _progression.State.TotalAssimilationScore,
                _playerStats.BaseLevels,
                _configuration);
            if (_hasCurrentState && _currentState.Equals(nextState))
            {
                return false;
            }

            var previousState = _currentState;
            var hadPreviousState = _hasCurrentState;
            _view.Apply(nextState);
            _currentState = nextState;
            _hasCurrentState = true;

            if (hadPreviousState && previousState.Tier != nextState.Tier)
            {
                var change = new EvolutionTierChangedEvent(
                    previousState.Tier,
                    nextState.Tier,
                    _progression.State.TotalAssimilationScore);
                TierChanged?.Invoke(change);
                _vfxHook?.Play(change);
            }

            return true;
        }

        public void Shutdown()
        {
            if (!_isInitialized)
            {
                return;
            }

            _progression.Dirty -= HandleProgressionDirty;
            _playerStats.StatChanged -= HandleStatChanged;
            _isInitialized = false;
            _hasCurrentState = false;
        }

        private void HandleProgressionDirty(ProgressionDirtyEvent change)
        {
            ApplyCurrentState();
        }

        private void HandleStatChanged(PlayerStatChange change)
        {
            ApplyCurrentState();
        }

        private void OnDestroy()
        {
            Shutdown();
        }
    }
}
