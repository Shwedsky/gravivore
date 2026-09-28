using System;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using UnityEngine;

namespace Gravivore.Gameplay.Encounters
{
    [DisallowMultipleComponent]
    public sealed class MagnetarGuardController : MonoBehaviour, ITargetable, IDamageable, IDisplaceable,
        IMagnetarGuardDefeatSource
    {
        private readonly HealthState _health = new HealthState();
        private readonly MagnetarGuardStateMachine _brain = new MagnetarGuardStateMachine();
        private CharacterController _body;
        private Transform _targetPoint;
        private Collider _sensingCollider;
        private Transform _player;
        private IDamageable _playerDamageable;
        private MagnetarGuardConfiguration _configuration;
        private bool _initialized;

        public event Action<EliteShockwaveTelegraphEvent> TelegraphStarted;
        public event Action<EliteShockwaveResolvedEvent> ShockwaveResolved;
        public event Action<MagnetarGuardDefeatedEvent> Defeated;

        public Transform TargetPoint => _targetPoint;
        public Transform DisplacementRoot => transform;
        public bool CanBeTargeted => _initialized && _health.IsAlive;
        public bool IsAlive => _initialized && _health.IsAlive;
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
            _body.enabled = false;
            transform.position = configuration.SpawnPosition;
            transform.rotation = Quaternion.identity;
            _body.enabled = true;
            _initialized = true;
        }

        public bool IsHostileTo(CombatFaction faction) => faction == CombatFaction.Player;

        public DamageResult ApplyDamage(in DamageRequest request)
        {
            if (!IsAlive) return new DamageResult(0f, false);
            var result = _health.ApplyDamage(request, _configuration.Armor);
            if (!result.WasLethal) return result;
            _brain.MarkDead();
            EncounterEventDispatch.Publish(
                Defeated,
                new MagnetarGuardDefeatedEvent(_configuration.Id, transform.position));
            return result;
        }

        public bool TryDisplace(Vector3 destination, in DisplacementContext context)
        {
            if (!IsAlive) return false;
            _body.Move(destination - transform.position);
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (!_initialized) throw new InvalidOperationException("Magnetar Guard must be initialized before ticking.");
            if (!IsAlive) return;
            var offset = _player.position - transform.position;
            offset.y = 0f;
            var decision = _brain.Tick(deltaTime, offset.magnitude);
            if (decision.ShouldApproach && offset.sqrMagnitude > Mathf.Epsilon)
            {
                var direction = offset.normalized;
                _body.Move(direction * (_configuration.MoveSpeed * deltaTime));
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }

            if (decision.TelegraphBegan)
            {
                EncounterEventDispatch.Publish(
                    TelegraphStarted,
                    new EliteShockwaveTelegraphEvent(
                        transform.position,
                        _configuration.ShockwaveRadius,
                        _configuration.TelegraphDuration));
            }

            if (decision.ResolveShockwave)
            {
                var hit = _playerDamageable.IsAlive && BossAttackGeometry.IsInsideCircle(
                    transform.position,
                    _player.position,
                    _configuration.ShockwaveRadius);
                if (hit)
                {
                    _playerDamageable.ApplyDamage(new DamageRequest(_configuration.AttackDamage, DamageType.Physical));
                }

                EncounterEventDispatch.Publish(ShockwaveResolved, new EliteShockwaveResolvedEvent(hit));
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
    }

    internal static class EncounterEventDispatch
    {
        public static void Publish<T>(Action<T> handlers, T value)
        {
            if (handlers == null) return;
            var invocationList = handlers.GetInvocationList();
            for (var i = 0; i < invocationList.Length; i++)
            {
                try
                {
                    ((Action<T>)invocationList[i])(value);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }
    }
}
