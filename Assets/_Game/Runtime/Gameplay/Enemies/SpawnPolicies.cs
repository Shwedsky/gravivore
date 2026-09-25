using System;
using System.Collections.Generic;

namespace Gravivore.Gameplay.Enemies
{
    public readonly struct SpawnPopulationPolicy
    {
        public const int MinimumAllowedPopulation = 3;
        public const int MaximumAllowedPopulation = 5;

        public SpawnPopulationPolicy(int desiredPopulation)
        {
            if (desiredPopulation < MinimumAllowedPopulation || desiredPopulation > MaximumAllowedPopulation)
            {
                throw new ArgumentOutOfRangeException(nameof(desiredPopulation));
            }

            DesiredPopulation = desiredPopulation;
        }

        public int DesiredPopulation { get; }
    }

    public sealed class SpawnPopulationState
    {
        public SpawnPopulationState(SpawnPopulationPolicy policy)
        {
            DesiredPopulation = policy.DesiredPopulation;
            PendingRespawns = DesiredPopulation;
        }

        public int DesiredPopulation { get; }

        public int LiveCount { get; private set; }

        public int PendingRespawns { get; private set; }

        public bool NeedsSpawn => PendingRespawns > 0 && LiveCount < DesiredPopulation;

        public void RegisterSpawn()
        {
            if (!NeedsSpawn)
            {
                throw new InvalidOperationException("Spawn would exceed the desired population.");
            }

            PendingRespawns--;
            LiveCount++;
        }

        public void RegisterRecycle()
        {
            if (LiveCount <= 0)
            {
                throw new InvalidOperationException("Cannot recycle an enemy from an empty population.");
            }

            LiveCount--;
            PendingRespawns++;
        }
    }

    public readonly struct RespawnDelayPolicy
    {
        public RespawnDelayPolicy(float minimumDelay, float maximumDelay)
        {
            ValidateNonNegative(minimumDelay, nameof(minimumDelay));
            ValidateNonNegative(maximumDelay, nameof(maximumDelay));
            if (maximumDelay < minimumDelay)
            {
                throw new ArgumentException("Maximum respawn delay cannot be smaller than minimum delay.");
            }

            MinimumDelay = minimumDelay;
            MaximumDelay = maximumDelay;
        }

        public float MinimumDelay { get; }

        public float MaximumDelay { get; }

        public float Sample(float unitSample)
        {
            if (float.IsNaN(unitSample) || float.IsInfinity(unitSample) || unitSample < 0f || unitSample > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(unitSample));
            }

            return MinimumDelay + ((MaximumDelay - MinimumDelay) * unitSample);
        }

        private static void ValidateNonNegative(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }

    public static class SpawnAnchorSelector
    {
        public static bool TrySelect(
            IReadOnlyList<float> distancesToPlayer,
            IReadOnlyList<bool> occupiedAnchors,
            float minimumPlayerDistance,
            int startIndex,
            out int selectedIndex)
        {
            if (distancesToPlayer == null)
            {
                throw new ArgumentNullException(nameof(distancesToPlayer));
            }

            if (occupiedAnchors == null)
            {
                throw new ArgumentNullException(nameof(occupiedAnchors));
            }

            if (distancesToPlayer.Count == 0 || distancesToPlayer.Count != occupiedAnchors.Count)
            {
                throw new ArgumentException("Anchor distance and occupancy collections must have equal non-zero counts.");
            }

            if (float.IsNaN(minimumPlayerDistance) || float.IsInfinity(minimumPlayerDistance) ||
                minimumPlayerDistance < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumPlayerDistance));
            }

            var normalizedStart = ((startIndex % distancesToPlayer.Count) + distancesToPlayer.Count) %
                                  distancesToPlayer.Count;
            for (var offset = 0; offset < distancesToPlayer.Count; offset++)
            {
                var index = (normalizedStart + offset) % distancesToPlayer.Count;
                var distance = distancesToPlayer[index];
                if (float.IsNaN(distance) || float.IsInfinity(distance) || distance < 0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(distancesToPlayer));
                }

                if (!occupiedAnchors[index] && distance >= minimumPlayerDistance)
                {
                    selectedIndex = index;
                    return true;
                }
            }

            selectedIndex = -1;
            return false;
        }
    }

    public interface ILiveEnemyCapacity
    {
        int LiveCount { get; }

        int MaximumLiveCount { get; }

        bool TryReserve();

        void Release();
    }

    public sealed class LiveEnemyCapCoordinator : ILiveEnemyCapacity
    {
        public LiveEnemyCapCoordinator(int maximumLiveCount)
        {
            if (maximumLiveCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumLiveCount));
            }

            MaximumLiveCount = maximumLiveCount;
        }

        public int LiveCount { get; private set; }

        public int MaximumLiveCount { get; }

        public bool TryReserve()
        {
            if (LiveCount >= MaximumLiveCount)
            {
                return false;
            }

            LiveCount++;
            return true;
        }

        public void Release()
        {
            if (LiveCount <= 0)
            {
                throw new InvalidOperationException("Cannot release an unreserved enemy slot.");
            }

            LiveCount--;
        }
    }

    public static class SensingColliderContract
    {
        public static void ValidateCounts(int sensingColliderCount, int otherCollidersOnTargetLayer)
        {
            if (sensingColliderCount != 1 || otherCollidersOnTargetLayer != 0)
            {
                throw new InvalidOperationException(
                    "An enemy requires exactly one dedicated sensing collider on the CombatTarget layer; body and hitbox colliders must use other layers.");
            }
        }
    }

    public interface IRandomSource
    {
        float NextUnit();
    }

    public sealed class SystemRandomSource : IRandomSource
    {
        private readonly Random _random;

        public SystemRandomSource(int seed)
        {
            _random = new Random(seed);
        }

        public float NextUnit()
        {
            return (float)_random.NextDouble();
        }
    }
}
