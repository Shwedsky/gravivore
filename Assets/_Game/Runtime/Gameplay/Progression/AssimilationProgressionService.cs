using System;
using System.Collections.Generic;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;

namespace Gravivore.Gameplay.Progression
{
    public sealed class AssimilationProgressionService : IDisposable
    {
        private readonly PlayerStatsState _playerStats;
        private readonly ProgressionConfiguration _configuration;
        private readonly EnemyPopulationController _deathSource;
        private bool _isDisposed;

        public AssimilationProgressionService(
            PlayerStatsState playerStats,
            ProgressionState state,
            ProgressionConfiguration configuration,
            EnemyPopulationController deathSource = null)
        {
            _playerStats = playerStats ?? throw new ArgumentNullException(nameof(playerStats));
            State = state ?? throw new ArgumentNullException(nameof(state));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _deathSource = deathSource;
            if (_deathSource != null)
            {
                _deathSource.EnemyDied += HandleEnemyDied;
            }
        }

        public event Action<CoreRewardGrantedEvent> RewardGranted;
        public event Action<StatExperienceChangedEvent> StatExperienceChanged;
        public event Action<FirstEnemyKillEvent> FirstKill;
        public event Action<ProgressionDirtyEvent> Dirty;

        public ProgressionState State { get; }

        public bool TryGrant(in EnemyDeathEvent death)
        {
            ThrowIfDisposed();
            if (!death.LifeId.IsValid)
            {
                return false;
            }

            if (State.HasProcessed(death.LifeId))
            {
                return false;
            }

            if (!_configuration.TryGetReward(death.EnemyId, out var reward))
            {
                return false;
            }

            var previousLevel = _playerStats.BaseLevels.GetLevel(reward.Stat);
            var maximumLevel = _playerStats.GetMaximumLevel(reward.Stat);
            var previousExperience = State.GetStatExperience(reward.Stat);
            var nextExperience = previousExperience + reward.StatExperience;
            if (float.IsInfinity(nextExperience))
            {
                throw new OverflowException("Stat experience overflowed.");
            }

            var nextLevel = previousLevel;
            while (nextLevel < maximumLevel)
            {
                var required = _configuration.ThresholdCurve.Evaluate(nextLevel);
                if (nextExperience < required)
                {
                    break;
                }

                nextExperience -= required;
                nextLevel++;
            }

            if (nextLevel == maximumLevel)
            {
                nextExperience = 0f;
            }

            var nextTotal = checked(State.TotalAssimilationScore + reward.AssimilationScore);
            var isFirstKill = !State.HasFirstKill(death.EnemyId);

            State.Commit(
                death.LifeId,
                death.EnemyId,
                reward.Stat,
                nextExperience,
                nextTotal,
                isFirstKill);
            Exception statObserverError = null;
            if (nextLevel != previousLevel)
            {
                try
                {
                    _playerStats.SetLevel(reward.Stat, nextLevel);
                }
                catch (Exception exception)
                {
                    statObserverError = exception;
                }
            }

            PublishEvents(
                death,
                reward,
                previousExperience,
                nextExperience,
                previousLevel,
                nextLevel,
                isFirstKill,
                nextTotal,
                statObserverError);
            return true;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            if (_deathSource != null)
            {
                _deathSource.EnemyDied -= HandleEnemyDied;
            }

            _isDisposed = true;
        }

        private void HandleEnemyDied(EnemyDeathEvent death)
        {
            TryGrant(death);
        }

        private void PublishEvents(
            in EnemyDeathEvent death,
            in CoreReward reward,
            float previousExperience,
            float nextExperience,
            int previousLevel,
            int nextLevel,
            bool isFirstKill,
            long nextTotal,
            Exception precedingError)
        {
            List<Exception> errors = null;
            if (precedingError != null)
            {
                AddError(ref errors, precedingError);
            }

            if (previousExperience != nextExperience || previousLevel != nextLevel)
            {
                try
                {
                    StatExperienceChanged?.Invoke(new StatExperienceChangedEvent(
                        reward.Stat,
                        previousExperience,
                        nextExperience,
                        previousLevel,
                        nextLevel));
                }
                catch (Exception exception)
                {
                    AddError(ref errors, exception);
                }
            }

            try
            {
                RewardGranted?.Invoke(new CoreRewardGrantedEvent(
                    death.LifeId,
                    death.EnemyId,
                    death.Position,
                    reward.Stat,
                    reward.StatExperience,
                    reward.AssimilationScore,
                    previousLevel,
                    nextLevel,
                    nextExperience,
                    isFirstKill));
            }
            catch (Exception exception)
            {
                AddError(ref errors, exception);
            }

            if (isFirstKill)
            {
                try
                {
                    FirstKill?.Invoke(new FirstEnemyKillEvent(death.EnemyId, reward.Stat, death.Position));
                }
                catch (Exception exception)
                {
                    AddError(ref errors, exception);
                }
            }

            try
            {
                Dirty?.Invoke(new ProgressionDirtyEvent(death.LifeId, nextTotal));
            }
            catch (Exception exception)
            {
                AddError(ref errors, exception);
            }

            if (errors != null)
            {
                throw new AggregateException("One or more progression observers failed after the reward was committed.", errors);
            }
        }

        private static void AddError(ref List<Exception> errors, Exception exception)
        {
            if (errors == null)
            {
                errors = new List<Exception>(1);
            }

            errors.Add(exception);
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AssimilationProgressionService));
            }
        }
    }
}
