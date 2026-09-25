using System;

namespace Gravivore.Gameplay.Enemies
{
    public enum OrdinaryEnemyBrainState
    {
        Idle = 0,
        Approach = 1,
        Attack = 2
    }

    public readonly struct EnemyBehaviorParameters
    {
        public EnemyBehaviorParameters(
            float aggroRadius,
            float aggroReleaseRadius,
            float attackRange,
            float attackInterval)
        {
            ValidatePositive(aggroRadius, nameof(aggroRadius));
            ValidatePositive(aggroReleaseRadius, nameof(aggroReleaseRadius));
            ValidatePositive(attackRange, nameof(attackRange));
            ValidatePositive(attackInterval, nameof(attackInterval));

            if (aggroReleaseRadius < aggroRadius)
            {
                throw new ArgumentException("Aggro release radius cannot be smaller than aggro radius.");
            }

            if (attackRange > aggroRadius)
            {
                throw new ArgumentException("Attack range cannot exceed aggro radius.");
            }

            AggroRadius = aggroRadius;
            AggroReleaseRadius = aggroReleaseRadius;
            AttackRange = attackRange;
            AttackInterval = attackInterval;
        }

        public float AggroRadius { get; }

        public float AggroReleaseRadius { get; }

        public float AttackRange { get; }

        public float AttackInterval { get; }

        private static void ValidatePositive(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }

    public readonly struct EnemyBrainDecision
    {
        public EnemyBrainDecision(OrdinaryEnemyBrainState state, bool shouldApproach, bool shouldAttack)
        {
            State = state;
            ShouldApproach = shouldApproach;
            ShouldAttack = shouldAttack;
        }

        public OrdinaryEnemyBrainState State { get; }

        public bool ShouldApproach { get; }

        public bool ShouldAttack { get; }
    }

    public sealed class OrdinaryEnemyStateMachine
    {
        private EnemyBehaviorParameters _parameters;
        private float _attackCooldown;
        private bool _isConfigured;
        private bool _isEngaged;

        public OrdinaryEnemyBrainState State { get; private set; } = OrdinaryEnemyBrainState.Idle;

        public float AttackCooldown => _attackCooldown;

        public void Configure(EnemyBehaviorParameters parameters)
        {
            _parameters = parameters;
            _isConfigured = true;
            Reset();
        }

        public EnemyBrainDecision Tick(float deltaTime, bool hasValidTarget, float distanceToTarget)
        {
            if (!_isConfigured)
            {
                throw new InvalidOperationException("Enemy state machine must be configured before ticking.");
            }

            ValidateNonNegative(deltaTime, nameof(deltaTime));
            ValidateNonNegative(distanceToTarget, nameof(distanceToTarget));

            if (!hasValidTarget || (_isEngaged && distanceToTarget > _parameters.AggroReleaseRadius))
            {
                Reset();
                return new EnemyBrainDecision(State, false, false);
            }

            if (!_isEngaged)
            {
                if (distanceToTarget > _parameters.AggroRadius)
                {
                    return new EnemyBrainDecision(State, false, false);
                }

                _isEngaged = true;
            }

            _attackCooldown = Math.Max(0f, _attackCooldown - deltaTime);
            if (distanceToTarget > _parameters.AttackRange)
            {
                State = OrdinaryEnemyBrainState.Approach;
                return new EnemyBrainDecision(State, true, false);
            }

            State = OrdinaryEnemyBrainState.Attack;
            if (_attackCooldown > 0f)
            {
                return new EnemyBrainDecision(State, false, false);
            }

            _attackCooldown = _parameters.AttackInterval;
            return new EnemyBrainDecision(State, false, true);
        }

        public void Reset()
        {
            State = OrdinaryEnemyBrainState.Idle;
            _attackCooldown = 0f;
            _isEngaged = false;
        }

        private static void ValidateNonNegative(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }

}
