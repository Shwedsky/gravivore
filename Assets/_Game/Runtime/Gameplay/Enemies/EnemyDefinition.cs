using System;
using UnityEngine;

namespace Gravivore.Gameplay.Enemies
{
    public readonly struct EnemyRuntimeConfiguration
    {
        public EnemyRuntimeConfiguration(
            string id,
            float maximumHitPoints,
            float moveSpeed,
            float attackDamage,
            float collisionRadius,
            float targetPointHeight,
            EnemyBehaviorParameters behavior)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Enemy id is required.", nameof(id));
            }

            ValidatePositive(maximumHitPoints, nameof(maximumHitPoints));
            ValidatePositive(moveSpeed, nameof(moveSpeed));
            ValidateNonNegative(attackDamage, nameof(attackDamage));
            ValidatePositive(collisionRadius, nameof(collisionRadius));
            ValidatePositive(targetPointHeight, nameof(targetPointHeight));

            Id = id;
            MaximumHitPoints = maximumHitPoints;
            MoveSpeed = moveSpeed;
            AttackDamage = attackDamage;
            CollisionRadius = collisionRadius;
            TargetPointHeight = targetPointHeight;
            Behavior = behavior;
        }

        public string Id { get; }

        public float MaximumHitPoints { get; }

        public float MoveSpeed { get; }

        public float AttackDamage { get; }

        public float CollisionRadius { get; }

        public float TargetPointHeight { get; }

        public EnemyBehaviorParameters Behavior { get; }

        private static void ValidatePositive(float value, string parameterName)
        {
            ValidateNonNegative(value, parameterName);
            if (value <= 0f)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        private static void ValidateNonNegative(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }

    [CreateAssetMenu(fileName = "EnemyDefinition", menuName = "Gravivore/Enemies/Enemy Definition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [SerializeField] private string _id = "ordinary-enemy";
        [SerializeField, Min(1f)] private float _maximumHitPoints = 40f;
        [SerializeField, Min(0.1f)] private float _moveSpeed = 2.5f;
        [SerializeField, Min(0f)] private float _attackDamage = 5f;
        [SerializeField, Min(0.05f)] private float _collisionRadius = 0.4f;
        [SerializeField, Min(0.1f)] private float _targetPointHeight = 0.9f;
        [SerializeField, Min(0.1f)] private float _aggroRadius = 5f;
        [SerializeField, Min(0.1f)] private float _aggroReleaseRadius = 7f;
        [SerializeField, Min(0.1f)] private float _attackRange = 1.1f;
        [SerializeField, Min(0.05f)] private float _attackInterval = 1.2f;

        public string Id => _id;

        public EnemyRuntimeConfiguration CreateRuntimeConfiguration()
        {
            return new EnemyRuntimeConfiguration(
                _id,
                _maximumHitPoints,
                _moveSpeed,
                _attackDamage,
                _collisionRadius,
                _targetPointHeight,
                new EnemyBehaviorParameters(
                    _aggroRadius,
                    _aggroReleaseRadius,
                    _attackRange,
                    _attackInterval));
        }

        public void ValidateOrThrow()
        {
            _ = CreateRuntimeConfiguration();
        }
    }
}
