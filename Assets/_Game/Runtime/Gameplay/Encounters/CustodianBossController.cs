using System;
using Gravivore.Core.Events;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using UnityEngine;

namespace Gravivore.Gameplay.Encounters
{
    public interface IBossEncounterAccess
    {
        bool CanEngage { get; }
    }

    [DisallowMultipleComponent]
    public sealed class CustodianBossController : MonoBehaviour, ITargetable, IDamageable, IDisplaceable, IBossHealthSource
    {
        private readonly HealthState _health = new HealthState();
        private CharacterController _body;
        private Transform _targetPoint;
        private Collider _sensingCollider;
        private Transform _player;
        private PlayerHealthController _playerHealth;
        private IPullDestinationResolver _chargeResolver;
        private CustodianBossConfiguration _configuration;
        private CustodianBossStateMachine _stateMachine;
        private BossCompletionState _completion;
        private IBossEncounterAccess _encounterAccess;
        private Vector3 _telegraphOrigin;
        private Vector3 _telegraphDirection;
        private bool _initialized;
        private float _outsideArenaSeconds;
        private EncounterBasicAttackCadence _basic;
        public event Action<EncounterBasicAttackEvent> BasicAttackStarted;
        public event Action<EncounterBasicAttackEvent> BasicAttackResolved;
        public float OutsideCombatLeashSeconds => _outsideArenaSeconds;
        public float InitialAggroRadius => _configuration.InitialAggroRadius;
        public float CombatLeashRadius => _configuration.CombatLeashRadius;

        public event Action<BossTelegraphEvent> TelegraphStarted;
        public event Action<BossAttackResolvedEvent> AttackResolved;
        public event Action<BossPhaseChangedEvent> PhaseChanged;
        public event Action<BossEncounterResetEvent> EncounterReset;
        public event Action<BossEncounterStartedEvent> EncounterStarted;
        public event Action<DamageResult> Damaged;
        public event Action<BossDefeatedEvent> Defeated;
        private bool _externalDefeatAuthority;

        public Transform TargetPoint => _targetPoint;
        public string Id => _configuration.Id;
        public Transform DisplacementRoot => transform;
        // Access controls admission. A fight already in progress remains targetable
        // while availability/reward-window state changes underneath it.
        public bool CanBeTargeted => _encounterAccess != null &&
                                     IsAlive && State != CustodianBossState.Dormant &&
                                     State != CustodianBossState.Resetting &&
                                     State != CustodianBossState.Dead;
        public bool IsAlive => _initialized && _health.IsAlive;
        public DisplacementClass DisplacementClass => DisplacementClass.Boss;
        public float CollisionRadius => _configuration.CollisionRadius;
        public float CurrentHitPoints => _health.CurrentHitPoints;
        public float MaximumHitPoints => _health.MaximumHitPoints;
        public CustodianBossState State => _stateMachine?.State ?? CustodianBossState.Dormant;
        public bool IsLowHealthPhase => _stateMachine != null && _stateMachine.IsLowHealthPhase;
        public BossCompletionState Completion => _completion;

        public void Initialize(
            CharacterController body,
            Transform targetPoint,
            Collider sensingCollider,
            int targetLayer,
            CustodianBossConfiguration configuration,
            Transform player,
            PlayerHealthController playerHealth,
            IPullDestinationResolver chargeResolver,
            IBossEncounterAccess encounterAccess,
            BossCompletionState completion,
            bool externalDefeatAuthority = false)
        {
            if (_initialized) throw new InvalidOperationException("Custodian boss is already initialized.");
            _body = body != null ? body : throw new ArgumentNullException(nameof(body));
            _targetPoint = targetPoint != null ? targetPoint : throw new ArgumentNullException(nameof(targetPoint));
            _sensingCollider = sensingCollider != null ? sensingCollider : throw new ArgumentNullException(nameof(sensingCollider));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _player = player != null ? player : throw new ArgumentNullException(nameof(player));
            _playerHealth = playerHealth != null ? playerHealth : throw new ArgumentNullException(nameof(playerHealth));
            _chargeResolver = chargeResolver ?? throw new ArgumentNullException(nameof(chargeResolver));
            _encounterAccess = encounterAccess ?? throw new ArgumentNullException(nameof(encounterAccess));
            _completion = completion ?? throw new ArgumentNullException(nameof(completion));
            _externalDefeatAuthority = externalDefeatAuthority;
            ValidateSensingCollider(targetLayer);
            ConfigureBody();
            _health.Reset(configuration.MaximumHitPoints);
            _stateMachine = new CustodianBossStateMachine(configuration);
            _basic = new EncounterBasicAttackCadence(configuration.BasicAttack);
            SetPosition(configuration.StartPosition);
            _playerHealth.Died += HandlePlayerDied;
            _initialized = true;
            if (_completion.IsDefeated)
            {
                _stateMachine.MarkDead();
                _body.enabled = false;
                _sensingCollider.enabled = false;
            }
        }

        public bool IsHostileTo(CombatFaction faction) => faction == CombatFaction.Player;

        public DamageResult ApplyDamage(in DamageRequest request)
        {
            if (!CanBeTargeted) return new DamageResult(0f, false);
            var result = _health.ApplyDamage(request, _configuration.Armor);
            SafeEventDispatch.Publish(Damaged, result);
            if (!result.WasLethal) return result;
            _stateMachine.MarkDead();
            _body.enabled = false;
            _sensingCollider.enabled = false;
            try
            {
                if (!_externalDefeatAuthority) _completion.TryRecordDefeat(_configuration.Id, transform.position);
                SafeEventDispatch.Publish(Defeated, new BossDefeatedEvent(_configuration.Id, transform.position));
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            return result;
        }

        public bool TryDisplace(Vector3 destination, in DisplacementContext context)
        {
            return false;
        }

        public bool ResetForRepeat()
        {
            if (!_initialized || State != CustodianBossState.Dead || !_encounterAccess.CanEngage) return false;
            _health.Reset(_configuration.MaximumHitPoints);
            _stateMachine = new CustodianBossStateMachine(_configuration);
            _telegraphOrigin = default;
            _telegraphDirection = default;
            _outsideArenaSeconds = 0f;
            _basic?.Reset();
            SetPosition(_configuration.StartPosition);
            _sensingCollider.enabled = true;
            SafeEventDispatch.Publish(EncounterReset,
                new BossEncounterResetEvent(_configuration.Id, _configuration.StartPosition));
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (!_initialized) throw new InvalidOperationException("Custodian boss must be initialized before ticking.");
            if (State == CustodianBossState.Dead) return;
            var playerInsideAggro = BossAttackGeometry.IsInsideCircle(
                _configuration.ArenaCenter,
                _player.position,
                _configuration.InitialAggroRadius);
            var playerInsideLeash = BossAttackGeometry.IsInsideCircle(_configuration.ArenaCenter, _player.position, _configuration.CombatLeashRadius);
            if (State == CustodianBossState.Dormant)
            {
                if (!_encounterAccess.CanEngage || !playerInsideAggro || !_playerHealth.IsAlive) return;
                _stateMachine.Engage();
                SafeEventDispatch.Publish(
                    EncounterStarted,
                    new BossEncounterStartedEvent(_configuration.Id));
            }
            else if (!_playerHealth.IsAlive)
            {
                ResetEncounter();
                return;
            }

            if (!playerInsideLeash)
            {
                _outsideArenaSeconds += deltaTime;
                if (_outsideArenaSeconds >= _configuration.ArenaExitResetGraceSeconds)
                {
                    ResetEncounter();
                    return;
                }
            }
            else
            {
                _outsideArenaSeconds = 0f;
            }

            var healthFraction = _health.CurrentHitPoints / _health.MaximumHitPoints;
            var decision = _stateMachine.Tick(deltaTime, healthFraction);
            if (decision.PhaseChanged)
            {
                SafeEventDispatch.Publish(
                    PhaseChanged,
                    new BossPhaseChangedEvent(_configuration.Id, true));
            }

            var offset = _player.position - transform.position; offset.y = 0;
            var betweenSpecials = State == CustodianBossState.Recovery || State == CustodianBossState.Engaging;
            if (betweenSpecials && _configuration.PursuitSpeed > 0 && offset.magnitude > _configuration.CollisionRadius * 2)
            {
                var requested = transform.position + offset.normalized * (_configuration.PursuitSpeed * deltaTime);
                var bounds = _configuration.WorldBounds;
                requested.x = Mathf.Clamp(requested.x, bounds.MinX + _configuration.CollisionRadius, bounds.MaxX - _configuration.CollisionRadius);
                requested.z = Mathf.Clamp(requested.z, bounds.MinZ + _configuration.CollisionRadius, bounds.MaxZ - _configuration.CollisionRadius);
                var safe = _chargeResolver.Resolve(transform.position, requested, _configuration.CollisionRadius);
                _body.Move(safe - transform.position);
                transform.rotation = Quaternion.LookRotation(offset.normalized, Vector3.up);
            }
            _basic.Tick(deltaTime, offset.magnitude, betweenSpecials && _playerHealth.IsAlive);
            var basicSource = _targetPoint.position;
            var basicTarget = _player.position + Vector3.up;
            if (_basic.Began) SafeEventDispatch.Publish(BasicAttackStarted,
                new EncounterBasicAttackEvent(basicSource, basicTarget, _configuration.BasicAttack.Windup));
            if (_basic.Resolved)
            {
                if (_basic.Hit) _playerHealth.ApplyDamage(new DamageRequest(_configuration.BasicAttack.Damage, DamageType.Physical));
                SafeEventDispatch.Publish(BasicAttackResolved, new EncounterBasicAttackEvent(basicSource, basicTarget, 0, _basic.Hit));
            }
            if (decision.TelegraphBegan) BeginTelegraph(decision.Attack);
            if (decision.ResolveAttack) ResolveAttack(decision.Attack);
        }

        public bool ResetEncounter()
        {
            if (!_initialized || !_stateMachine.BeginReset()) return false;
            _health.Reset(_configuration.MaximumHitPoints);
            SetPosition(_configuration.StartPosition);
            _telegraphOrigin = default;
            _telegraphDirection = default;
            _outsideArenaSeconds = 0f;
            _basic?.Reset();
            _stateMachine.CompleteReset();
            SafeEventDispatch.Publish(
                EncounterReset,
                new BossEncounterResetEvent(_configuration.Id, _configuration.StartPosition));
            return true;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public bool ResetForDevelopment()
        {
            if (!_initialized) return false;
            _health.Reset(_configuration.MaximumHitPoints);
            _stateMachine = new CustodianBossStateMachine(_configuration);
            _telegraphOrigin = default;
            _telegraphDirection = default;
            _outsideArenaSeconds = 0f;
            _basic?.Reset();
            SetPosition(_configuration.StartPosition);
            _sensingCollider.enabled = true;
            SafeEventDispatch.Publish(
                EncounterReset,
                new BossEncounterResetEvent(_configuration.Id, _configuration.StartPosition));
            return true;
        }
#endif

        public void Shutdown()
        {
            if (_playerHealth != null) _playerHealth.Died -= HandlePlayerDied;
            _playerHealth = null;
        }

        private void Update()
        {
            if (_initialized) Tick(Time.deltaTime);
        }

        private void BeginTelegraph(BossAttackType attackType)
        {
            var attack = _configuration.GetAttack(attackType);
            _telegraphOrigin = transform.position;
            _telegraphDirection = _player.position - _telegraphOrigin;
            _telegraphDirection.y = 0f;
            if (_telegraphDirection.sqrMagnitude <= Mathf.Epsilon) _telegraphDirection = transform.forward;
            _telegraphDirection.Normalize();
            transform.rotation = Quaternion.LookRotation(_telegraphDirection, Vector3.up);
            SafeEventDispatch.Publish(
                TelegraphStarted,
                new BossTelegraphEvent(
                    attack.Type,
                    _telegraphOrigin,
                    _telegraphDirection,
                    attack.Range,
                    attack.HalfAngleDegrees,
                    attack.Width,
                    attack.TelegraphDuration));
        }

        private void ResolveAttack(BossAttackType attackType)
        {
            var attack = _configuration.GetAttack(attackType);
            var playerPosition = _player.position;
            var hit = false;
            switch (attackType)
            {
                case BossAttackType.CirclePulse:
                    hit = BossAttackGeometry.IsInsideCircle(_telegraphOrigin, playerPosition, attack.Range);
                    break;
                case BossAttackType.ConeSweep:
                    hit = BossAttackGeometry.IsInsideCone(
                        _telegraphOrigin,
                        _telegraphDirection,
                        playerPosition,
                        attack.Range,
                        attack.HalfAngleDegrees);
                    break;
                case BossAttackType.LineCharge:
                    var requestedEnd = BossAttackGeometry.ClampChargeDestination(
                        _telegraphOrigin,
                        _telegraphDirection,
                        attack.Range,
                        _configuration.WorldBounds,
                        _configuration.CollisionRadius);
                    var safeEnd = _chargeResolver.Resolve(
                        _telegraphOrigin,
                        requestedEnd,
                        _configuration.CollisionRadius);
                    hit = BossAttackGeometry.IsInsideLine(
                        _telegraphOrigin,
                        safeEnd,
                        playerPosition,
                        attack.Width * 0.5f);
                    var requestedCharge = BossAttackGeometry.ClampChargeDestination(
                        _telegraphOrigin,
                        _telegraphDirection,
                        attack.ChargeDistance,
                        _configuration.WorldBounds,
                        _configuration.CollisionRadius);
                    var safeCharge = _chargeResolver.Resolve(
                        _telegraphOrigin,
                        requestedCharge,
                        _configuration.CollisionRadius);
                    _body.Move(safeCharge - transform.position);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(attackType));
            }

            if (hit && _playerHealth.IsAlive)
            {
                _playerHealth.ApplyDamage(new DamageRequest(attack.Damage, DamageType.Physical));
                if (State == CustodianBossState.Dormant)
                {
                    return;
                }
            }

            SafeEventDispatch.Publish(AttackResolved, new BossAttackResolvedEvent(attackType, hit));
        }

        private void HandlePlayerDied(PlayerDeathEvent death)
        {
            ResetEncounter();
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

        private void SetPosition(Vector3 position)
        {
            _body.enabled = false;
            transform.position = position;
            transform.rotation = Quaternion.identity;
            _body.enabled = true;
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

        private void OnDestroy() => Shutdown();
    }
}
