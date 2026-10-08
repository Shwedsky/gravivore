using System;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.World;
using UnityEngine;

namespace Gravivore.Presentation.World
{
    public enum WorldMarkerKind { Player, RegularSpot, Elite, Boss }
    public enum WorldMarkerStatus { Available, Locked, Respawning, Defeated, Active, Ready }

    public readonly struct WorldMarkerSnapshot
    {
        public WorldMarkerSnapshot(string id, WorldMarkerKind kind, Vector3 position, WorldMarkerStatus status,
            int liveCount = 0, int pendingRespawns = 0, float nextRespawnSeconds = 0f, int penaltySteps = 0)
        {
            Id = id;
            Kind = kind;
            Position = position;
            Status = status;
            LiveCount = liveCount;
            PendingRespawns = pendingRespawns;
            NextRespawnSeconds = nextRespawnSeconds;
            PenaltySteps = penaltySteps;
        }

        public string Id { get; }
        public WorldMarkerKind Kind { get; }
        public Vector3 Position { get; }
        public WorldMarkerStatus Status { get; }
        public int LiveCount { get; }
        public int PendingRespawns { get; }
        public float NextRespawnSeconds { get; }
        public int PenaltySteps { get; }
    }

    public interface IWorldMarkerSource
    {
        int Count { get; }
        WorldMarkerSnapshot GetMarker(int index);
    }

    // Pull snapshots on demand: no UI, subscriptions, cached gameplay state or frame allocations.
    public sealed class WorldMarkerReadModel : IWorldMarkerSource
    {
        private readonly Transform _player;
        private readonly EnemyPopulationController _population;
        private readonly MagnetarGuardController _elite;
        private readonly CustodianBossController _boss;
        private readonly WorldUnlockState _worldState;

        public WorldMarkerReadModel(Transform player, EnemyPopulationController population,
            MagnetarGuardController elite, CustodianBossController boss, WorldUnlockState worldState)
        {
            _player = player != null ? player : throw new ArgumentNullException(nameof(player));
            _population = population != null ? population : throw new ArgumentNullException(nameof(population));
            _elite = elite != null ? elite : throw new ArgumentNullException(nameof(elite));
            _boss = boss != null ? boss : throw new ArgumentNullException(nameof(boss));
            _worldState = worldState ?? throw new ArgumentNullException(nameof(worldState));
        }

        public int Count => _population.SpotCount + 3;

        public WorldMarkerSnapshot GetMarker(int index)
        {
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            if (index == 0)
                return new WorldMarkerSnapshot("player", WorldMarkerKind.Player, _player.position, WorldMarkerStatus.Available);
            if (index <= _population.SpotCount)
            {
                var spot = _population.GetSpot(index - 1);
                return new WorldMarkerSnapshot(spot.Id, WorldMarkerKind.RegularSpot, spot.Position,
                    SpotStatus(spot.Availability),
                    spot.LiveCount, spot.PendingRespawns, spot.SecondsUntilNextRespawn, spot.RespawnPenaltySteps);
            }
            if (index == _population.SpotCount + 1)
                return new WorldMarkerSnapshot(_elite.Id, WorldMarkerKind.Elite, _elite.transform.position,
                    _worldState.EliteDefeated || _elite.State == MagnetarGuardState.Dead ? WorldMarkerStatus.Defeated :
                    _elite.IsEncounterActive ? WorldMarkerStatus.Available : WorldMarkerStatus.Locked);
            return new WorldMarkerSnapshot(_boss.Id, WorldMarkerKind.Boss, _boss.transform.position,
                _boss.Completion.IsDefeated ? WorldMarkerStatus.Defeated :
                _boss.CanBeTargeted ? WorldMarkerStatus.Available : WorldMarkerStatus.Locked);
        }
        private static WorldMarkerStatus SpotStatus(SpawnSpotAvailability availability)
        {
            switch(availability)
            {
                case SpawnSpotAvailability.Cooldown:return WorldMarkerStatus.Respawning;
                case SpawnSpotAvailability.Ready:return WorldMarkerStatus.Ready;
                case SpawnSpotAvailability.Active:return WorldMarkerStatus.Active;
                case SpawnSpotAvailability.Locked:return WorldMarkerStatus.Locked;
                default:return WorldMarkerStatus.Available;
            }
        }
    }
}
