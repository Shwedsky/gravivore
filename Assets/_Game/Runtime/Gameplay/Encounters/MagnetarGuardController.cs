using System;
using Gravivore.Core.Events;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using UnityEngine;

namespace Gravivore.Gameplay.Encounters
{
    [DisallowMultipleComponent]
    public sealed class MagnetarGuardController : MonoBehaviour, ITargetable, IDamageable, IDisplaceable,
        IMagnetarGuardDefeatSource, IMagnetarGuardActivationTarget
    {
        private readonly HealthState _health = new HealthState();
        private readonly MagnetarGuardStateMachine _brain = new MagnetarGuardStateMachine();
        private CharacterController _body;
        private Transform _targetPoint;
        private Collider _sensingCollider;
        private Transform _player;
        private IDamageable _playerDamageable;
        private MagnetarGuardConfiguration _configuration;
        private EliteShockwaveSnapshot _activeShockwave;
        private bool _hasActiveShockwave;
        private bool _encounterActive;
        private bool _initialized;
        private EncounterBasicAttackCadence _basic;
        public event Action<EncounterBasicAttackEvent> BasicAttackStarted;
        public event Action<EncounterBasicAttackEvent> BasicAttackResolved;

        public event Action<MagnetarGuardActivatedEvent> Activated;
        public event Action<DamageResult> Damaged;
        public event Action<EliteShockwaveTelegraphEvent> TelegraphStarted;
        public event Action<EliteShockwaveResolvedEvent> ShockwaveResolved;
        public event Action<EliteShockwaveCancelledEvent> ShockwaveCancelled;
        public event Action<MagnetarGuardDefeatedEvent> Defeated;

        public Transform TargetPoint => _targetPoint;
        public string Id => _configuration.Id;
        public Transform DisplacementRoot => transform;
        public bool CanBeTargeted => IsAlive;
        public bool IsAlive => _initialized && _encounterActive && _health.IsAlive;
        public bool IsEncounterActive => _initialized && _encounterActive;
        public DisplacementClass DisplacementClass => DisplacementClass.Elite;
        public float CollisionRadius => _configuration.CollisionRadius;
        public float CurrentHitPoints => _health.CurrentHitPoints;
        public float MaximumHitPoints => _health.MaximumHitPoints;
        public MagnetarGuardState State => _brain.State;

        public void Initialize(
            CharacterController body,
            Transform targetPoint,
            Collider sensingCollider,
            int targetLayer,
            in MagnetarGuardConfiguration configuration,
            Transform player,
            IDamageable playerDamageable)
        {
            if (_initialized) throw new InvalidOperationException("Magnetar Guard is already initialized.");
            _body = body != null ? body : throw new ArgumentNullException(nameof(body));
            _targetPoint = targetPoint != null ? targetPoint : throw new ArgumentNullException(nameof(targetPoint));
            _sensingCollider = sensingCollider != null ? sensingCollider : throw new ArgumentNullException(nameof(sensingCollider));
            _player = player != null ? player : throw new ArgumentNullException(nameof(player));
            _playerDamageable = playerDamageable ?? throw new ArgumentNullException(nameof(playerDamageable));
            _configuration = configuration;
            ValidateSensingCollider(targetLayer);
            ConfigureBody();
            _health.Reset(configuration.MaximumHitPoints);
            _brain.Configure(configuration);
            _basic = new EncounterBasicAttackCadence(configuration.BasicAttack);
            _body.enabled = false;
            transform.position = configuration.SpawnPosition;
            transform.rotation = Quaternion.identity;
            _sensingCollider.enabled = false;
            _initialized = true;
        }

        public bool ActivateEncounter()
        {
            if (!_initialized) throw new InvalidOperationException("Magnetar Guard must be initialized before activation.");
            if (_encounterActive || !_health.IsAlive) return false;
            _encounterActive = true;
            _brain.Reset();
            _basic?.Reset();
            _body.enabled = true;
            _sensingCollider.enabled = true;
            SafeEventDispatch.Publish(Activated, new MagnetarGuardActivatedEvent(_configuration.Id));
            return true;
        }

        public bool IsHostileTo(CombatFaction faction) => faction == CombatFaction.Player;

        public bool ResetForRepeat()
        {
            if (!_initialized || IsAlive || State != MagnetarGuardState.Dead) return false;
            CancelActiveShockwave();
            _encounterActive = false;
            _body.enabled = false;
            _sensingCollider.enabled = false;
            transform.position = _configuration.SpawnPosition;
            transform.rotation = Quaternion.identity;
            _health.Reset(_configuration.MaximumHitPoints);
            _brain.Reset();
            _basic?.Reset();
            return true;
        }

        public DamageResult ApplyDamage(in DamageRequest request)
        {
            if (!IsAlive) return new DamageResult(0f, false);
            var result = _health.ApplyDamage(request, _configuration.Armor);
            SafeEventDispatch.Publish(Damaged, result);
            if (!result.WasLethal) return result;
            _brain.MarkDead();
            CancelActiveShockwave();
            _sensingCollider.enabled = false;
            _body.enabled = false;
            SafeEventDispatch.Publish(
                Defeated,
                new MagnetarGuardDefeatedEvent(_configuration.Id, transform.position));
            return result;
        }

        public bool TryDisplace(Vector3 destination, in DisplacementContext context)
        {
            if (!CanBeTargeted) return false;
            _body.Move(destination - transform.position);
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (!_initialized) throw new InvalidOperationException("Magnetar Guard must be initialized before ticking.");
            if (!IsEncounterActive || !IsAlive) return;
            var offset = _player.position - transform.position;
            offset.y = 0f;
            var decision = _brain.Tick(deltaTime, offset.magnitude);
            if (decision.ShouldApproach && offset.sqrMagnitude > Mathf.Epsilon)
            {
                var direction = offset.normalized;
                _body.Move(direction * (_configuration.MoveSpeed * deltaTime));
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }

            _basic.Tick(deltaTime, offset.magnitude, _playerDamageable.IsAlive &&
                (State == MagnetarGuardState.Approach || State == MagnetarGuardState.Recovery));
            var source = _targetPoint.position;
            var target = _player.position + Vector3.up;
            if (_basic.Began) SafeEventDispatch.Publish(BasicAttackStarted,
                new EncounterBasicAttackEvent(source, target, _configuration.BasicAttack.Windup));
            if (_basic.Resolved)
            {
                var hit = _basic.Hit;
                if (hit) _playerDamageable.ApplyDamage(new DamageRequest(_configuration.BasicAttack.Damage, DamageType.Physical));
                SafeEventDispatch.Publish(BasicAttackResolved, new EncounterBasicAttackEvent(source, target, 0, hit));
            }

            if (decision.TelegraphBegan)
            {
                _activeShockwave = new EliteShockwaveSnapshot(
                    transform.position,
                    _configuration.ShockwaveRadius,
                    _configuration.AttackDamage);
                _hasActiveShockwave = true;
                SafeEventDispatch.Publish(
                    TelegraphStarted,
                    new EliteShockwaveTelegraphEvent(
                        _activeShockwave.Origin,
                        _activeShockwave.Radius,
                        _configuration.TelegraphDuration));
            }

            if (decision.ResolveShockwave)
            {
                if (!_hasActiveShockwave) return;
                var shockwave = _activeShockwave;
                _hasActiveShockwave = false;
                var hit = _playerDamageable.IsAlive && BossAttackGeometry.IsInsideCircle(
                    shockwave.Origin,
                    _player.position,
                    shockwave.Radius);
                if (hit)
                {
                    _playerDamageable.ApplyDamage(new DamageRequest(shockwave.Damage, DamageType.Physical));
                }

                SafeEventDispatch.Publish(ShockwaveResolved, new EliteShockwaveResolvedEvent(hit));
            }
        }

        private void Update()
        {
            if (_initialized) Tick(Time.deltaTime);
        }

        private void ConfigureBody()
        {
            _targetPoint.localPosition = new Vector3(0f, _configuration.TargetPointHeight, 0f);
            _sensingCollider.transform.localPosition = _targetPoint.localPosition;
            if (_sensingCollider is SphereCollider sphere) sphere.radius = _configuration.CollisionRadius;
            _body.radius = _configuration.CollisionRadius;
            _body.height = Mathf.Max(_configuration.TargetPointHeight * 1.8f, _configuration.CollisionRadius * 2f);
            _body.center = new Vector3(0f, _body.height * 0.5f, 0f);
        }

        private void ValidateSensingCollider(int targetLayer)
        {
            var colliders = GetComponentsInChildren<Collider>(true);
            var sensing = 0;
            var other = 0;
            for (var i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] == _sensingCollider && colliders[i].gameObject.layer == targetLayer) sensing++;
                else if (colliders[i].gameObject.layer == targetLayer) other++;
            }

            SensingColliderContract.ValidateCounts(sensing, other);
        }

        private void CancelActiveShockwave()
        {
            if (!_hasActiveShockwave) return;
            var shockwave = _activeShockwave;
            _hasActiveShockwave = false;
            SafeEventDispatch.Publish(
                ShockwaveCancelled,
                new EliteShockwaveCancelledEvent(shockwave.Origin, shockwave.Radius));
        }

        private readonly struct EliteShockwaveSnapshot
        {
            public EliteShockwaveSnapshot(Vector3 origin, float radius, float damage)
            {
                Origin = origin;
                Radius = radius;
                Damage = damage;
            }

            public Vector3 Origin { get; }
            public float Radius { get; }
            public float Damage { get; }
        }
    }

}
