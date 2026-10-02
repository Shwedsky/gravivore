using System;
using Gravivore.Gameplay.Player;

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
            : this(
                aggroRadius,
                aggroReleaseRadius,
                attackRange,
                attackInterval,
                new OrdinaryEnemyAggressionParameters(15f, 30f, 0.55f, 0.2f))
        {
        }

        public EnemyBehaviorParameters(
            float aggroRadius,
            float aggroReleaseRadius,
            float attackRange,
            float attackInterval,
            OrdinaryEnemyAggressionParameters aggression)
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
            Aggression = aggression;
        }

        public float AggroRadius { get; }

        public float AggroReleaseRadius { get; }

        public float AttackRange { get; }

        public float AttackInterval { get; }

        public OrdinaryEnemyAggressionParameters Aggression { get; }

        private static void ValidatePositive(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }

    public readonly struct OrdinaryEnemyAggressionParameters
    {
        public OrdinaryEnemyAggressionParameters(
            float moderatePlayerStrengthRatio,
            float massivePlayerStrengthRatio,
            float moderateAggroMultiplier,
            float massiveAggroMultiplier)
        {
            if (float.IsNaN(moderatePlayerStrengthRatio) || float.IsInfinity(moderatePlayerStrengthRatio) ||
                float.IsNaN(massivePlayerStrengthRatio) || float.IsInfinity(massivePlayerStrengthRatio) ||
                moderatePlayerStrengthRatio <= 1f || massivePlayerStrengthRatio <= moderatePlayerStrengthRatio)
                throw new ArgumentOutOfRangeException(nameof(massivePlayerStrengthRatio));
            if (float.IsNaN(moderateAggroMultiplier) || float.IsInfinity(moderateAggroMultiplier) ||
                moderateAggroMultiplier <= 0f || moderateAggroMultiplier >= 1f)
                throw new ArgumentOutOfRangeException(nameof(moderateAggroMultiplier));
            if (float.IsNaN(massiveAggroMultiplier) || float.IsInfinity(massiveAggroMultiplier) ||
                massiveAggroMultiplier <= 0f || massiveAggroMultiplier >= moderateAggroMultiplier)
                throw new ArgumentOutOfRangeException(nameof(massiveAggroMultiplier));

            ModeratePlayerStrengthRatio = moderatePlayerStrengthRatio;
            MassivePlayerStrengthRatio = massivePlayerStrengthRatio;
            ModerateAggroMultiplier = moderateAggroMultiplier;
            MassiveAggroMultiplier = massiveAggroMultiplier;
        }

        public float ModeratePlayerStrengthRatio { get; }
        public float MassivePlayerStrengthRatio { get; }
        public float ModerateAggroMultiplier { get; }
        public float MassiveAggroMultiplier { get; }
    }

    public static class OrdinaryEnemyAggressionPolicy
    {
        public static float ResolveProactiveAggroRadius(
            in PlayerDerivedStats player,
            in EnemyRuntimeConfiguration enemy)
        {
            var playerOutput = player.BaseDamage / Math.Max(player.AttackInterval, 0.01f);
            var playerDurability = player.MaxHp * (1f + Math.Max(0f, player.ArmorValue) / 100f);
            var enemyOutput = enemy.AttackDamage / Math.Max(enemy.Behavior.AttackInterval, 0.01f);
            var enemyStrength = Math.Max(0.01f, enemyOutput * enemy.MaximumHitPoints);
            var ratio = playerOutput * playerDurability / enemyStrength;
            var scaling = enemy.Behavior.Aggression;

            if (ratio >= scaling.MassivePlayerStrengthRatio)
                return Math.Max(enemy.Behavior.AttackRange, enemy.Behavior.AggroRadius * scaling.MassiveAggroMultiplier);
            if (ratio >= scaling.ModeratePlayerStrengthRatio)
                return Math.Max(enemy.Behavior.AttackRange, enemy.Behavior.AggroRadius * scaling.ModerateAggroMultiplier);
            return enemy.Behavior.AggroRadius;
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
            return Tick(deltaTime, hasValidTarget, distanceToTarget, _parameters.AggroRadius);
        }

        public EnemyBrainDecision Tick(
            float deltaTime,
            bool hasValidTarget,
            float distanceToTarget,
            float proactiveAggroRadius)
        {
            if (!_isConfigured)
            {
                throw new InvalidOperationException("Enemy state machine must be configured before ticking.");
            }

            ValidateNonNegative(deltaTime, nameof(deltaTime));
            ValidateNonNegative(distanceToTarget, nameof(distanceToTarget));
            ValidateNonNegative(proactiveAggroRadius, nameof(proactiveAggroRadius));

            if (!hasValidTarget || (_isEngaged && distanceToTarget > _parameters.AggroReleaseRadius))
            {
                Reset();
                return new EnemyBrainDecision(State, false, false);
            }

            if (!_isEngaged)
            {
                if (distanceToTarget > proactiveAggroRadius)
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

        public void Engage()
        {
            if (!_isConfigured) throw new InvalidOperationException("Enemy state machine must be configured before engagement.");
            _isEngaged = true;
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
