using System;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.World;
using Gravivore.Presentation.UI;
using UnityEngine;

namespace Gravivore.Presentation.Map
{
    /// <summary>Projects gameplay read state into map snapshots; never owns timers or rewards.</summary>
    public sealed class Chapter1WorldMarkerMapAdapter : IMapMarkerSource, IMapZoneLabelSource
    {
        private readonly Transform _player;
        private readonly EnemyPopulationController _population;
        private readonly MagnetarGuardController _elite;
        private readonly CustodianBossController _boss;
        private readonly WorldUnlockState _world;
        private readonly Chapter1WorldMarkerAuthority _authority;
        private readonly int _ordinaryCount;
        private readonly Vector3 _repairPosition;
        private readonly WorldGateConfiguration? _eliteGate, _bossGate;

        public Chapter1WorldMarkerMapAdapter(Transform player, EnemyPopulationController population,
            MagnetarGuardController elite, CustodianBossController boss, WorldUnlockState world,
            Chapter1WorldMarkerAuthority authority, int ordinaryCount, Vector3 repairPosition,
            WorldGateConfiguration? eliteGate = null, WorldGateConfiguration? bossGate = null)
        {
            _player = player != null ? player : throw new ArgumentNullException(nameof(player));
            _population = population != null ? population : throw new ArgumentNullException(nameof(population));
            _elite = elite != null ? elite : throw new ArgumentNullException(nameof(elite));
            _boss = boss != null ? boss : throw new ArgumentNullException(nameof(boss));
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _authority = authority ?? throw new ArgumentNullException(nameof(authority));
            _ordinaryCount = ordinaryCount;
            _repairPosition = repairPosition;
            _eliteGate = eliteGate; _bossGate = bossGate;
        }

        private int BaseCount => _population.SpotCount + 4;
        public int Count => BaseCount + (_eliteGate.HasValue && _bossGate.HasValue ? 2 : 0);
        public string CurrentZoneName
        {
            get
            {
                if (_bossGate.HasValue && _player.position.z >= _bossGate.Value.Position.z) return "Арена Хранителя";
                if (_eliteGate.HasValue && _player.position.z >= _eliteGate.Value.Position.z) return "Контур Магнетара";
                var best = float.MaxValue; var name = "Сектор";
                for (var i = 0; i < _ordinaryCount; i++)
                {
                    var spot = _population.GetSpot(i); var distance = (spot.Position-_player.position).sqrMagnitude;
                    if (distance < best) { best = distance; name = RussianUiText.SpotName(spot.Id); }
                }
                return name;
            }
        }
        public MapMarkerSnapshot GetMarker(int index)
        {
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            if (index >= BaseCount)
            {
                var isEliteGate = index == BaseCount;
                var gate = isEliteGate ? _eliteGate.Value : _bossGate.Value;
                var gateLocked = isEliteGate ? !_world.EliteGateUnlocked : !_world.BossGateUnlocked;
                return new MapMarkerSnapshot(gate.Id, MapMarkerKind.Gate, gate.Position,
                    gateLocked ? MapAvailabilityState.Inactive : MapAvailabilityState.Available,
                    isEliteGate ? "Шлюз элиты" : "Шлюз Хранителя", progressionLocked: gateLocked);
            }
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
            if (index == BaseCount - 1) return new MapMarkerSnapshot("repair-hub", MapMarkerKind.RepairHub,
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
