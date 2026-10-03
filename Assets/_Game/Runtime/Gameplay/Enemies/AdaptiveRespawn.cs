using System;

namespace Gravivore.Gameplay.Enemies
{
    public readonly struct AdaptiveRespawnPolicy
    {
        public AdaptiveRespawnPolicy(int killsPerStep, float delayPerStep, int maximumSteps,
            float idleGraceSeconds, float recoveryStepSeconds)
        {
            if (killsPerStep < 1) throw new ArgumentOutOfRangeException(nameof(killsPerStep));
            if (maximumSteps < 0 || (long)killsPerStep * maximumSteps > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(maximumSteps));
            ValidateSeconds(delayPerStep, nameof(delayPerStep), false);
            ValidateSeconds(idleGraceSeconds, nameof(idleGraceSeconds), false);
            ValidateSeconds(recoveryStepSeconds, nameof(recoveryStepSeconds), true);
            if (double.IsInfinity((double)delayPerStep * maximumSteps) ||
                (double)delayPerStep * maximumSteps > float.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(delayPerStep));
            KillsPerStep = killsPerStep;
            DelayPerStep = delayPerStep;
            MaximumSteps = maximumSteps;
            IdleGraceSeconds = idleGraceSeconds;
            RecoveryStepSeconds = recoveryStepSeconds;
        }

        // default(AdaptiveRespawnPolicy) keeps legacy/test configurations at baseline.
        public bool IsEnabled => MaximumSteps > 0 && DelayPerStep > 0f;
        public int KillsPerStep { get; }
        public float DelayPerStep { get; }
        public int MaximumSteps { get; }
        public float IdleGraceSeconds { get; }
        public float RecoveryStepSeconds { get; }

        private static void ValidateSeconds(float value, string name, bool positive)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f || (positive && value == 0f))
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AdaptiveRespawnState
    {
        private readonly AdaptiveRespawnPolicy _policy;
        private double _elapsedTime;
        private double _nextRecoveryTime;

        public AdaptiveRespawnState(AdaptiveRespawnPolicy policy) => _policy = policy;

        public int KillPressure { get; private set; }
        public int PenaltySteps => _policy.IsEnabled ? KillPressure / _policy.KillsPerStep : 0;
        public float AdditionalDelay => PenaltySteps * _policy.DelayPerStep;

        public void RegisterKill(double elapsedTime)
        {
            AdvanceTo(elapsedTime);
            if (!_policy.IsEnabled) return;
            var cap = _policy.KillsPerStep * _policy.MaximumSteps;
            if (KillPressure < cap) KillPressure++;
            _nextRecoveryTime = elapsedTime + _policy.IdleGraceSeconds + _policy.RecoveryStepSeconds;
        }

        public void AdvanceTo(double elapsedTime)
        {
            if (double.IsNaN(elapsedTime) || double.IsInfinity(elapsedTime) || elapsedTime < _elapsedTime)
                throw new ArgumentOutOfRangeException(nameof(elapsedTime));
            _elapsedTime = elapsedTime;
            if (!_policy.IsEnabled || KillPressure == 0 || elapsedTime < _nextRecoveryTime) return;

            var recoveries = Math.Floor((elapsedTime - _nextRecoveryTime) / _policy.RecoveryStepSeconds) + 1d;
            var remainingRecoveries = (KillPressure + (long)_policy.KillsPerStep - 1) / _policy.KillsPerStep;
            if (recoveries >= remainingRecoveries)
            {
                KillPressure = 0;
            }
            else
            {
                KillPressure -= (int)recoveries * _policy.KillsPerStep;
                _nextRecoveryTime += recoveries * _policy.RecoveryStepSeconds;
            }
        }
    }
}
