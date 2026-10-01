using System;
using System.Collections.Generic;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.World;
using UnityEngine;

namespace Gravivore.Gameplay.Encounters
{
    public enum BossAttackType
    {
        CirclePulse = 0,
        ConeSweep = 1,
        LineCharge = 2
    }

    public readonly struct MagnetarGuardConfiguration
    {
        public MagnetarGuardConfiguration(
            string id,
            Vector3 spawnPosition,
            float maximumHitPoints,
            float armor,
            float moveSpeed,
            float attackDamage,
            float collisionRadius,
            float targetPointHeight,
            float aggroRadius,
            float attackRange,
            float shockwaveRadius,
            float telegraphDuration,
            float recoveryDuration)
        {
            RequireId(id, nameof(id));
            ValidateFinite(spawnPosition, nameof(spawnPosition));
            ValidatePositive(maximumHitPoints, nameof(maximumHitPoints));
            ValidateNonNegative(armor, nameof(armor));
            ValidatePositive(moveSpeed, nameof(moveSpeed));
            ValidatePositive(attackDamage, nameof(attackDamage));
            ValidatePositive(collisionRadius, nameof(collisionRadius));
            ValidatePositive(targetPointHeight, nameof(targetPointHeight));
            ValidatePositive(aggroRadius, nameof(aggroRadius));
            ValidatePositive(attackRange, nameof(attackRange));
            ValidatePositive(shockwaveRadius, nameof(shockwaveRadius));
            ValidatePositive(telegraphDuration, nameof(telegraphDuration));
            ValidatePositive(recoveryDuration, nameof(recoveryDuration));
            if (telegraphDuration < CustodianBossConfiguration.MinimumTelegraphDuration ||
                telegraphDuration > CustodianBossConfiguration.MaximumTelegraphDuration)
            {
                throw new ArgumentOutOfRangeException(nameof(telegraphDuration), "Elite telegraph must stay in the readable 0.8-1.2 second range.");
            }

            if (attackRange > aggroRadius) throw new ArgumentException("Elite attack range cannot exceed aggro radius.");

            Id = id;
            SpawnPosition = spawnPosition;
            MaximumHitPoints = maximumHitPoints;
            Armor = armor;
            MoveSpeed = moveSpeed;
            AttackDamage = attackDamage;
            CollisionRadius = collisionRadius;
            TargetPointHeight = targetPointHeight;
            AggroRadius = aggroRadius;
            AttackRange = attackRange;
            ShockwaveRadius = shockwaveRadius;
            TelegraphDuration = telegraphDuration;
            RecoveryDuration = recoveryDuration;
        }

        public string Id { get; }
        public Vector3 SpawnPosition { get; }
        public float MaximumHitPoints { get; }
        public float Armor { get; }
        public float MoveSpeed { get; }
        public float AttackDamage { get; }
        public float CollisionRadius { get; }
        public float TargetPointHeight { get; }
        public float AggroRadius { get; }
        public float AttackRange { get; }
        public float ShockwaveRadius { get; }
        public float TelegraphDuration { get; }
        public float RecoveryDuration { get; }
        public DisplacementClass DisplacementClass => DisplacementClass.Elite;

        private static void RequireId(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Stable id is required.", name);
        }

        private static void ValidatePositive(float value, string name)
        {
            ValidateNonNegative(value, name);
            if (value <= 0f) throw new ArgumentOutOfRangeException(name);
        }

        private static void ValidateNonNegative(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f) throw new ArgumentOutOfRangeException(name);
        }

        private static void ValidateFinite(Vector3 value, string name)
        {
            if (float.IsNaN(value.x) || float.IsInfinity(value.x) ||
                float.IsNaN(value.y) || float.IsInfinity(value.y) ||
                float.IsNaN(value.z) || float.IsInfinity(value.z))
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }
    }

    public readonly struct BossAttackConfiguration
    {
        public BossAttackConfiguration(
            BossAttackType type,
            float telegraphDuration,
            float damage,
            float range,
            float halfAngleDegrees,
            float width,
            float chargeDistance)
        {
            ValidatePositive(telegraphDuration, nameof(telegraphDuration));
            ValidatePositive(damage, nameof(damage));
            ValidatePositive(range, nameof(range));
            if (float.IsNaN(halfAngleDegrees) || float.IsInfinity(halfAngleDegrees) ||
                halfAngleDegrees < 0f || halfAngleDegrees >= 90f)
            {
                throw new ArgumentOutOfRangeException(nameof(halfAngleDegrees));
            }

            ValidateNonNegative(width, nameof(width));
            ValidateNonNegative(chargeDistance, nameof(chargeDistance));
            if (type == BossAttackType.ConeSweep && halfAngleDegrees <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(halfAngleDegrees));
            }

            if (type == BossAttackType.LineCharge && (width <= 0f || chargeDistance <= 0f))
            {
                throw new ArgumentException("Line charge requires positive width and charge distance.");
            }

            Type = type;
            TelegraphDuration = telegraphDuration;
            Damage = damage;
            Range = range;
            HalfAngleDegrees = halfAngleDegrees;
            Width = width;
            ChargeDistance = chargeDistance;
        }

        public BossAttackType Type { get; }
        public float TelegraphDuration { get; }
        public float Damage { get; }
        public float Range { get; }
        public float HalfAngleDegrees { get; }
        public float Width { get; }
        public float ChargeDistance { get; }

        private static void ValidatePositive(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f) throw new ArgumentOutOfRangeException(name);
        }

        private static void ValidateNonNegative(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f) throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class CustodianBossConfiguration
    {
        public const float MinimumTelegraphDuration = 0.8f;
        public const float MaximumTelegraphDuration = 1.2f;

        private readonly BossAttackConfiguration[] _attacks;
        private readonly BossAttackType[] _attackSequence;

        public CustodianBossConfiguration(
            string id,
            Vector3 startPosition,
            Vector3 arenaCenter,
            float arenaRadius,
            WorldBounds worldBounds,
            float maximumHitPoints,
            float armor,
            float collisionRadius,
            float targetPointHeight,
            float recoveryDuration,
            float lowHealthThreshold,
            float lowHealthCadenceMultiplier,
            IReadOnlyList<BossAttackConfiguration> attacks,
            IReadOnlyList<BossAttackType> attackSequence,
            float arenaExitResetGraceSeconds = 3f)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Stable boss id is required.", nameof(id));
            ValidateFinite(startPosition, nameof(startPosition));
            ValidateFinite(arenaCenter, nameof(arenaCenter));
            ValidatePositive(arenaRadius, nameof(arenaRadius));
            ValidatePositive(maximumHitPoints, nameof(maximumHitPoints));
            ValidateNonNegative(armor, nameof(armor));
            ValidatePositive(collisionRadius, nameof(collisionRadius));
            ValidatePositive(targetPointHeight, nameof(targetPointHeight));
            ValidatePositive(recoveryDuration, nameof(recoveryDuration));
            ValidateNonNegative(arenaExitResetGraceSeconds, nameof(arenaExitResetGraceSeconds));
            if (lowHealthThreshold <= 0f || lowHealthThreshold >= 1f || float.IsNaN(lowHealthThreshold))
            {
                throw new ArgumentOutOfRangeException(nameof(lowHealthThreshold));
            }

            if (lowHealthCadenceMultiplier <= 0f || lowHealthCadenceMultiplier >= 1f ||
                float.IsNaN(lowHealthCadenceMultiplier) || float.IsInfinity(lowHealthCadenceMultiplier))
            {
                throw new ArgumentOutOfRangeException(nameof(lowHealthCadenceMultiplier));
            }

            if (!worldBounds.ContainsCircle(arenaCenter, arenaRadius) || !worldBounds.Contains(startPosition, collisionRadius))
            {
                throw new ArgumentException("Boss arena and start position must fit inside world bounds.");
            }

            if (Vector3.Distance(startPosition, arenaCenter) > arenaRadius - collisionRadius)
            {
                throw new ArgumentException("Boss start position must be inside its arena.");
            }

            if (attacks == null || attacks.Count != 3) throw new ArgumentException("Exactly three boss attacks are required.", nameof(attacks));
            var seen = new HashSet<BossAttackType>();
            _attacks = new BossAttackConfiguration[attacks.Count];
            for (var i = 0; i < attacks.Count; i++)
            {
                var attack = attacks[i];
                if (!seen.Add(attack.Type)) throw new ArgumentException("Boss attacks must be unique.", nameof(attacks));
                if (attack.TelegraphDuration < MinimumTelegraphDuration || attack.TelegraphDuration > MaximumTelegraphDuration)
                {
                    throw new ArgumentOutOfRangeException(nameof(attacks), "Boss telegraphs must stay in the readable 0.8-1.2 second range.");
                }

                _attacks[i] = attack;
            }

            if (attackSequence == null || attackSequence.Count == 0) throw new ArgumentException("Attack sequence is required.", nameof(attackSequence));
            _attackSequence = new BossAttackType[attackSequence.Count];
            for (var i = 0; i < attackSequence.Count; i++)
            {
                if (!seen.Contains(attackSequence[i])) throw new ArgumentException("Attack sequence references an undefined attack.", nameof(attackSequence));
                _attackSequence[i] = attackSequence[i];
            }

            Id = id;
            StartPosition = startPosition;
            ArenaCenter = arenaCenter;
            ArenaRadius = arenaRadius;
            WorldBounds = worldBounds;
            MaximumHitPoints = maximumHitPoints;
            Armor = armor;
            CollisionRadius = collisionRadius;
            TargetPointHeight = targetPointHeight;
            RecoveryDuration = recoveryDuration;
            LowHealthThreshold = lowHealthThreshold;
            LowHealthCadenceMultiplier = lowHealthCadenceMultiplier;
            ArenaExitResetGraceSeconds = arenaExitResetGraceSeconds;
        }

        public string Id { get; }
        public Vector3 StartPosition { get; }
        public Vector3 ArenaCenter { get; }
        public float ArenaRadius { get; }
        public WorldBounds WorldBounds { get; }
        public float MaximumHitPoints { get; }
        public float Armor { get; }
        public float CollisionRadius { get; }
        public float TargetPointHeight { get; }
        public float RecoveryDuration { get; }
        public float LowHealthThreshold { get; }
        public float LowHealthCadenceMultiplier { get; }
        public float ArenaExitResetGraceSeconds { get; }
        public DisplacementClass DisplacementClass => DisplacementClass.Boss;
        public int AttackSequenceCount => _attackSequence.Length;

        public BossAttackType GetAttackAtSequenceIndex(int index) => _attackSequence[index % _attackSequence.Length];

        public BossAttackConfiguration GetAttack(BossAttackType type)
        {
            for (var i = 0; i < _attacks.Length; i++)
            {
                if (_attacks[i].Type == type) return _attacks[i];
            }

            throw new ArgumentOutOfRangeException(nameof(type));
        }

        private static void ValidatePositive(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f) throw new ArgumentOutOfRangeException(name);
        }

        private static void ValidateNonNegative(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f) throw new ArgumentOutOfRangeException(name);
        }

        private static void ValidateFinite(Vector3 value, string name)
        {
            if (float.IsNaN(value.x) || float.IsInfinity(value.x) ||
                float.IsNaN(value.y) || float.IsInfinity(value.y) ||
                float.IsNaN(value.z) || float.IsInfinity(value.z)) throw new ArgumentOutOfRangeException(name);
        }
    }
}
