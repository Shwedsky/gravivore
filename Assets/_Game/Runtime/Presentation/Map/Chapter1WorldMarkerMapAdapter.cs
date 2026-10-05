using System;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.World;
using Gravivore.Presentation.UI;
using UnityEngine;

namespace Gravivore.Presentation.Map
{
    /// <summary>Projects gameplay read state into map snapshots; never owns timers or rewards.</summary>
    public sealed class Chapter1WorldMarkerMapAdapter : IMapMarkerSource
    {
        private readonly Transform _player;
        private readonly EnemyPopulationController _population;
        private readonly MagnetarGuardController _elite;
        private readonly CustodianBossController _boss;
        private readonly WorldUnlockState _world;
        private readonly Chapter1WorldMarkerAuthority _authority;
        private readonly int _ordinaryCount;
        private readonly Vector3 _repairPosition;

        public Chapter1WorldMarkerMapAdapter(Transform player, EnemyPopulationController population,
            MagnetarGuardController elite, CustodianBossController boss, WorldUnlockState world,
            Chapter1WorldMarkerAuthority authority, int ordinaryCount, Vector3 repairPosition)
        {
            _player = player != null ? player : throw new ArgumentNullException(nameof(player));
            _population = population != null ? population : throw new ArgumentNullException(nameof(population));
            _elite = elite != null ? elite : throw new ArgumentNullException(nameof(elite));
            _boss = boss != null ? boss : throw new ArgumentNullException(nameof(boss));
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _authority = authority ?? throw new ArgumentNullException(nameof(authority));
            _ordinaryCount = ordinaryCount;
            _repairPosition = repairPosition;
        }

        public int Count => _population.SpotCount + 4;
        public MapMarkerSnapshot GetMarker(int index)
        {
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            if (index == 0) return new MapMarkerSnapshot("player", MapMarkerKind.Player, _player.position,
                MapAvailabilityState.Available, "Игрок", headingDegrees: _player.eulerAngles.y);
            if (index <= _population.SpotCount)
            {
                var spot = _population.GetSpot(index - 1);
                var strong = index > _ordinaryCount;
                var position = strong ? _authority.ReadStrongOrdinary(index - _ordinaryCount - 1).Position : spot.Position;
                return new MapMarkerSnapshot(spot.Id, strong ? MapMarkerKind.StrongOrdinary : MapMarkerKind.Ordinary,
                    position, spot.LiveCount > 0 || strong && !spot.IsActive ? MapAvailabilityState.Available : MapAvailabilityState.Cooldown,
                    strong ? "Усиленная зона" : RussianUiText.SpotName(spot.Id), remainingSeconds: spot.SecondsUntilNextRespawn);
            }
            if (index == Count - 1) return new MapMarkerSnapshot("repair-hub", MapMarkerKind.RepairHub,
                _repairPosition, MapAvailabilityState.Available, "Ремонтный узел");
            var elite = index == _population.SpotCount + 1;
            var state = elite ? _authority.ReadMagnetar(_elite.transform.position, _world.EliteDefeated)
                : _authority.ReadCustodian(_boss.transform.position, _boss.Completion.IsDefeated);
            var locked = elite ? !_world.EliteGateUnlocked : !_world.BossGateUnlocked;
            var active = elite ? _elite.IsAlive && _elite.IsEncounterActive
                : _boss.CanBeTargeted;
            var availability = locked ? MapAvailabilityState.Inactive : !state.RepeatAvailable
                ? MapAvailabilityState.Cooldown : active ? MapAvailabilityState.Active : MapAvailabilityState.Available;
            var reward = state.RewardEntitlement == EncounterRewardEntitlement.FallbackRepeat
                ? MapRewardState.CappedFallback : MapRewardState.Full;
            return new MapMarkerSnapshot(state.Id, elite ? MapMarkerKind.Elite : MapMarkerKind.Boss,
                state.Position, availability, elite ? "Магнетар" : "Хранитель", progressionLocked: locked,
                remainingSeconds: (float)state.CooldownRemaining.TotalSeconds, rewardState: reward,
                firstClearCompleted: state.ProgressionFirstClearCompleted,
                rewardEntitlement: state.RewardEntitlement, premiumRewardsRemaining: state.PremiumRewardsRemaining,
                rewardWindowRemainingSeconds: (float)state.RewardWindowRemaining.TotalSeconds);
        }
    }
}
