using System;
using System.Collections.Generic;
using Gravivore.Gameplay.World;
using UnityEngine;

namespace Gravivore.Gameplay.Enemies
{
    public enum StrongOrdinaryRegion
    {
        Elite = 0,
        Boss = 1
    }

    public readonly struct StrongOrdinarySpotDefinition
    {
        public StrongOrdinarySpotDefinition(
            string id,
            StrongOrdinaryRegion region,
            Vector3 position,
            float healthMultiplier,
            float damageMultiplier,
            float rewardMultiplier,
            float baseRespawnSeconds,
            AdaptiveRespawnPolicy pressurePolicy)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Spot id is required.", nameof(id));
            if (!Enum.IsDefined(typeof(StrongOrdinaryRegion), region)) throw new ArgumentOutOfRangeException(nameof(region));
            if (!IsFinite(position)) throw new ArgumentOutOfRangeException(nameof(position));
            if (!IsFinitePositive(healthMultiplier) || !IsFinitePositive(damageMultiplier) ||
                !IsFinitePositive(rewardMultiplier) || !IsFinitePositive(baseRespawnSeconds))
                throw new ArgumentOutOfRangeException(nameof(healthMultiplier));
            ValidateTier(region, healthMultiplier, damageMultiplier, rewardMultiplier, baseRespawnSeconds);
            Id = id;
            Region = region;
            Position = position;
            HealthMultiplier = healthMultiplier;
            DamageMultiplier = damageMultiplier;
            RewardMultiplier = rewardMultiplier;
            BaseRespawnSeconds = baseRespawnSeconds;
            PressurePolicy = pressurePolicy;
        }

        public string Id { get; }
        public StrongOrdinaryRegion Region { get; }
        public Vector3 Position { get; }
        public float HealthMultiplier { get; }
        public float DamageMultiplier { get; }
        public float RewardMultiplier { get; }
        public float BaseRespawnSeconds { get; }
        public AdaptiveRespawnPolicy PressurePolicy { get; }

        public SpawnSpotRuntimeConfiguration CreateSpawnConfiguration(in SpawnSpotRuntimeConfiguration ordinary)
        {
            var source = ordinary.Enemy;
            var enemy = new EnemyRuntimeConfiguration(source.Id, source.MaximumHitPoints * HealthMultiplier,
                source.MoveSpeed, source.AttackDamage * DamageMultiplier, source.CollisionRadius,
                source.TargetPointHeight, source.Behavior);
            return new SpawnSpotRuntimeConfiguration(Id, enemy, Position, ordinary.AnchorOffsets,
                SpawnPopulationPolicy.MinimumAllowedPopulation, ordinary.MinimumPlayerDistance,
                BaseRespawnSeconds, BaseRespawnSeconds, PressurePolicy, RewardMultiplier);
        }

        private static void ValidateTier(StrongOrdinaryRegion region, float hp, float damage, float reward, float respawn)
        {
            if (region == StrongOrdinaryRegion.Elite)
            {
                if (hp < 1.25f || hp > 1.4f || damage < 1.15f || damage > 1.25f ||
                    reward < 1.5f || reward > 2f || respawn < 12f || respawn > 16f)
                    throw new ArgumentException("Elite-side strong spot is outside the authored Phase 4 range.");
            }
            else if (hp < 1.5f || hp > 1.75f || damage < 1.3f || damage > 1.5f ||
                     reward < 2f || reward > 3f || respawn < 14f || respawn > 20f)
            {
                throw new ArgumentException("Boss-side strong spot is outside the authored Phase 4 range.");
            }
        }

        private static bool IsFinite(Vector3 value) =>
            !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
            !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
            !float.IsNaN(value.z) && !float.IsInfinity(value.z);
        private static bool IsFinitePositive(float value) => !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
    }

    public sealed class StrongOrdinarySpotRuntime
    {
        private readonly AdaptiveRespawnState _pressure;

        public StrongOrdinarySpotRuntime(in StrongOrdinarySpotDefinition definition)
        {
            Definition = definition;
            _pressure = new AdaptiveRespawnState(definition.PressurePolicy);
        }

        public StrongOrdinarySpotDefinition Definition { get; }
        public int KillPressure => _pressure.KillPressure;
        public float RespawnDelaySeconds => Definition.BaseRespawnSeconds + _pressure.AdditionalDelay;

        public void RegisterKill(double elapsedTime) => _pressure.RegisterKill(elapsedTime);
        public void AdvanceTo(double elapsedTime) => _pressure.AdvanceTo(elapsedTime);
    }

    public static class Chapter1StrongOrdinarySpotCatalog
    {
        public static IReadOnlyList<StrongOrdinarySpotDefinition> Create(
            Chapter01WorldConfiguration world,
            IReadOnlyCollection<Vector3> existingOrdinaryPositions = null)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            var eliteBase = world.EliteGate.Position;
            var bossBase = world.BossArenaCenter;
            var pressure = new AdaptiveRespawnPolicy(3, 2f, 3, 35f, 20f);
            var spots = new[]
            {
                new StrongOrdinarySpotDefinition("strong-elite-a", StrongOrdinaryRegion.Elite,
                    ClampInside(world.Bounds, eliteBase + new Vector3(-6f, 0f, -5f)), 1.3f, 1.2f, 1.75f, 14f, pressure),
                new StrongOrdinarySpotDefinition("strong-elite-b", StrongOrdinaryRegion.Elite,
                    ClampInside(world.Bounds, eliteBase + new Vector3(7f, 0f, 4f)), 1.4f, 1.25f, 2f, 16f, pressure),
                new StrongOrdinarySpotDefinition("strong-boss-a", StrongOrdinaryRegion.Boss,
                    ClampInside(world.Bounds, bossBase + new Vector3(-world.BossArenaRadius * 0.55f, 0f, 0f)), 1.55f, 1.35f, 2.25f, 16f, pressure),
                new StrongOrdinarySpotDefinition("strong-boss-b", StrongOrdinaryRegion.Boss,
                    ClampInside(world.Bounds, bossBase + new Vector3(world.BossArenaRadius * 0.45f, 0f, -world.BossArenaRadius * 0.35f)), 1.7f, 1.45f, 2.75f, 19f, pressure)
            };
            ValidateDistinct(spots, existingOrdinaryPositions);
            return spots;
        }

        private static Vector3 ClampInside(WorldBounds bounds, Vector3 value)
        {
            const float clearance = 1f;
            return new Vector3(
                Mathf.Clamp(value.x, bounds.MinX + clearance, bounds.MaxX - clearance),
                value.y,
                Mathf.Clamp(value.z, bounds.MinZ + clearance, bounds.MaxZ - clearance));
        }

        private static void ValidateDistinct(IReadOnlyList<StrongOrdinarySpotDefinition> spots, IReadOnlyCollection<Vector3> existing)
        {
            for (var i = 0; i < spots.Count; i++)
            {
                for (var j = i + 1; j < spots.Count; j++)
                    if (SameXZ(spots[i].Position, spots[j].Position)) throw new InvalidOperationException("Strong ordinary spot coordinates must be distinct.");
                if (existing == null) continue;
                foreach (var position in existing)
                    if (SameXZ(spots[i].Position, position)) throw new InvalidOperationException($"Strong ordinary spot {spots[i].Id} duplicates an existing ordinary spot coordinate.");
            }
        }

        private static bool SameXZ(Vector3 a, Vector3 b) => Mathf.Abs(a.x - b.x) < 0.01f && Mathf.Abs(a.z - b.z) < 0.01f;
    }
}
