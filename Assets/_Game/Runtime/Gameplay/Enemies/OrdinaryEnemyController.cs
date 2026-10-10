using System;
using Gravivore.Core.Events;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Player;
using UnityEngine;

namespace Gravivore.Gameplay.Enemies
{
    public readonly struct EnemyDeathEvent
    {
        public EnemyDeathEvent(EnemyLifeId lifeId, string enemyId, Vector3 position, float rewardMultiplier = 1f)
        {
            if (!lifeId.IsValid)
            {
                throw new ArgumentException("A valid enemy life id is required.", nameof(lifeId));
            }

            if (string.IsNullOrWhiteSpace(enemyId))
            {
                throw new ArgumentException("Enemy id is required.", nameof(enemyId));
            }

            LifeId = lifeId;
            EnemyId = enemyId;
            Position = position;
            if (float.IsNaN(rewardMultiplier) || float.IsInfinity(rewardMultiplier) || rewardMultiplier <= 0f)
                throw new ArgumentOutOfRangeException(nameof(rewardMultiplier));
            RewardMultiplier = rewardMultiplier;
        }

        public EnemyLifeId LifeId { get; }

        public string EnemyId { get; }

        public Vector3 Position { get; }
        public float RewardMultiplier { get; }
    }

    public readonly struct EnemyDamageEvent
    {
        public EnemyDamageEvent(EnemyLifeId lifeId, Vector3 position, DamageResult result)
        {
            LifeId = lifeId;
            Position = position;
            Result = result;
        }

        public EnemyLifeId LifeId { get; }
        public Vector3 Position { get; }
        public DamageResult Result { get; }
    }

    [DisallowMultipleComponent]
    public sealed class OrdinaryEnemyController : MonoBehaviour, ITargetable, IDamageable, IDisplaceable
    {
        private readonly HealthState _health = new HealthState();
        private readonly OrdinaryEnemyStateMachine _brain = new OrdinaryEnemyStateMachine();

        private CharacterController _body;
        private Transform _targetPoint;
        private Collider _sensingCollider;
        private Transform _aggroTarget;
        private IDamageable _attackTarget;
        private EnemyRuntimeConfiguration _configuration;
        private EnemyLifeId _lifeId;
        private Action<OrdinaryEnemyController> _recycleRequested;
        private IEnemyVisualState _visualState;
        private PlayerStatsState _playerStats;
        private bool _isActive;
        private bool _isInitialized;
        private AmbientPatrolParameters _ambientSettings;
        private AmbientPatrolState _ambient;
        private uint _ambientSeed;
        public Vector3 AmbientHome => _ambient.Home;
        public float AmbientRadius => _ambientSettings.Radius;
        public void ConfigureAmbientMotion(in AmbientPatrolParameters settings, uint seed)
        { _ambientSettings=settings; _ambientSeed=seed; }

        public event Action<DamageRequest> AttackRequested;

        public event Action<EnemyDamageEvent> Damaged;

        public event Action<EnemyDeathEvent> Died;

        public Transform TargetPoint => _targetPoint;
        public EnemyLifeId LifeId => _lifeId;

        public Transform DisplacementRoot => transform;

        public bool CanBeTargeted => _isActive && _health.IsAlive;

        public bool IsAlive => _isActive && _health.IsAlive;

        public DisplacementClass DisplacementClass => DisplacementClass.Standard;

        public float CollisionRadius => _configuration.CollisionRadius;

        public float CurrentHitPoints => _health.CurrentHitPoints;

        public float MaximumHitPoints => _health.MaximumHitPoints;

        public OrdinaryEnemyBrainState BrainState => _brain.State;

        public float AttackCooldown => _brain.AttackCooldown;

        public Transform AggroTarget => _aggroTarget;

        public void InitializeInfrastructure(
            CharacterController body,
            Transform targetPoint,
            Collider sensingCollider,
            int targetLayer,
            IEnemyVisualState visualState = null)
        {
            _body = body != null ? body : throw new ArgumentNullException(nameof(body));
            _targetPoint = targetPoint != null ? targetPoint : throw new ArgumentNullException(nameof(targetPoint));
            _sensingCollider = sensingCollider != null
                ? sensingCollider
                : throw new ArgumentNullException(nameof(sensingCollider));
            ValidateSensingColliderContract(targetLayer);
            _visualState = visualState;
            _isInitialized = true;
        }

        public void Activate(
            EnemyRuntimeConfiguration configuration,
            EnemyLifeId lifeId,
            Transform aggroTarget,
            IDamageable attackTarget,
            Vector3 position,
            Action<OrdinaryEnemyController> recycleRequested,
            PlayerStatsState playerStats = null)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Enemy infrastructure must be initialized before activation.");
            }

            if (!lifeId.IsValid)
            {
                throw new ArgumentException("A valid enemy life id is required.", nameof(lifeId));
            }

            _lifeId = lifeId;
            _aggroTarget = aggroTarget != null ? aggroTarget : throw new ArgumentNullException(nameof(aggroTarget));
            _attackTarget = attackTarget ?? throw new ArgumentNullException(nameof(attackTarget));
            _recycleRequested = recycleRequested ?? throw new ArgumentNullException(nameof(recycleRequested));
            _configuration = configuration;
            _playerStats = playerStats;
            _health.Reset(configuration.MaximumHitPoints);
            _brain.Configure(configuration.Behavior);
            _targetPoint.localPosition = new Vector3(0f, configuration.TargetPointHeight, 0f);
            _sensingCollider.transform.localPosition = _targetPoint.localPosition;
            if (_sensingCollider is SphereCollider sensingSphere)
            {
                sensingSphere.radius = configuration.CollisionRadius;
            }

            _body.radius = configuration.CollisionRadius;
            _body.height = Mathf.Max(configuration.TargetPointHeight * 1.8f, configuration.CollisionRadius * 2f);
            _body.center = new Vector3(0f, _body.height * 0.5f, 0f);
            _body.enabled = false;
            transform.position = position;
            transform.rotation = Quaternion.identity;
            _body.enabled = true;
            _ambient.Reset(position,++_ambientSeed,_ambientSettings);
            _visualState?.Apply(configuration.Id);
            _isActive = true;
            gameObject.name = $"Enemy [{configuration.Id}]";
            gameObject.SetActive(true);
        }

        public void PrepareForPool(Vector3 poolPosition)
        {
            _isActive = false;
            _brain.Reset();
            _health.MarkInactive();
            _aggroTarget = null;
            _attackTarget = null;
            _playerStats = null;
            _recycleRequested = null;
            _lifeId = default;
            _ambient=default;
            AttackRequested = null;
            Damaged = null;
            Died = null;
            _visualState?.Reset();
            _body.enabled = false;
            transform.position = poolPosition;
            transform.rotation = Quaternion.identity;
            gameObject.SetActive(false);
        }

        public bool IsHostileTo(CombatFaction faction)
        {
            return faction == CombatFaction.Player;
        }

        public DamageResult ApplyDamage(in DamageRequest request)
        {
            if (!_isActive || !_health.IsAlive)
            {
                return new DamageResult(0f, false);
            }

            _brain.Engage();
            var result = _health.ApplyDamage(request, 0f);
            SafeEventDispatch.Publish(Damaged, new EnemyDamageEvent(_lifeId, transform.position, result));
            if (result.WasLethal)
            {
                _isActive = false;
                _brain.Reset();
                var recycleRequested = _recycleRequested;
                try
                {
                    SafeEventDispatch.Publish(
                        Died,
                        new EnemyDeathEvent(_lifeId, _configuration.Id, transform.position));
                }
                finally
                {
                    recycleRequested(this);
                }
            }

            return result;
        }

        public bool TryDisplace(Vector3 destination, in DisplacementContext context)
        {
            if (!_isActive || !_health.IsAlive)
            {
                return false;
            }

            _body.Move(destination - transform.position);
            return true;
        }

        private void Update() => Tick(Time.deltaTime);
        public void Tick(float deltaTime)
        {
            if (!_isActive || !_health.IsAlive || _aggroTarget == null || _attackTarget == null)
            {
                return;
            }

            var offset = _aggroTarget.position - transform.position;
            offset.y = 0f;
            var proactiveAggroRadius = _playerStats != null
                ? OrdinaryEnemyAggressionPolicy.ResolveProactiveAggroRadius(
                    _playerStats.DerivedStats,
                    _configuration)
                : _configuration.Behavior.AggroRadius;
            var decision = _brain.Tick(deltaTime, true, offset.magnitude, proactiveAggroRadius);
            if (decision.State==OrdinaryEnemyBrainState.Idle)
            {
                var ambientStep=_ambient.Step(transform.position,deltaTime,_ambientSettings);
                if (ambientStep.sqrMagnitude>0)
                {
                    _body.Move(ambientStep);
                    transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(ambientStep),_ambientSettings.TurnSpeed*deltaTime);
                }
            }
            if (decision.ShouldApproach && offset.sqrMagnitude > Mathf.Epsilon)
            {
                var direction = offset.normalized;
                _body.Move(direction * (_configuration.MoveSpeed * deltaTime));
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }

            if (decision.ShouldAttack)
            {
                var request = new DamageRequest(_configuration.AttackDamage, DamageType.Physical);
                AttackRequested?.Invoke(request);
                if (_isActive && _attackTarget.IsAlive)
                {
                    _attackTarget.ApplyDamage(request);
                }
            }
        }

        private void ValidateSensingColliderContract(int targetLayer)
        {
            var colliders = GetComponentsInChildren<Collider>(true);
            var sensingColliderCount = 0;
            var otherCollidersOnTargetLayer = 0;
            for (var i = 0; i < colliders.Length; i++)
            {
                var collider = colliders[i];
                if (collider == _sensingCollider && collider.gameObject.layer == targetLayer)
                {
                    sensingColliderCount++;
                }
                else if (collider.gameObject.layer == targetLayer)
                {
                    otherCollidersOnTargetLayer++;
                }
            }

            SensingColliderContract.ValidateCounts(sensingColliderCount, otherCollidersOnTargetLayer);
        }
    }
}
