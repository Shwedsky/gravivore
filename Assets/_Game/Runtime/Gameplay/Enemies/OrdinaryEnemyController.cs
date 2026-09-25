using System;
using Gravivore.Gameplay.Combat;
using UnityEngine;

namespace Gravivore.Gameplay.Enemies
{
    [DisallowMultipleComponent]
    public sealed class OrdinaryEnemyController : MonoBehaviour, ITargetable, IDamageable, IDisplaceable
    {
        private readonly EnemyRuntimeState _runtimeState = new EnemyRuntimeState();
        private readonly OrdinaryEnemyStateMachine _brain = new OrdinaryEnemyStateMachine();

        private CharacterController _body;
        private Transform _targetPoint;
        private Collider _sensingCollider;
        private Transform _aggroTarget;
        private EnemyRuntimeConfiguration _configuration;
        private Action<OrdinaryEnemyController> _recycleRequested;
        private bool _isActive;
        private bool _isInitialized;

        public event Action<DamageRequest> AttackRequested;

        public Transform TargetPoint => _targetPoint;

        public Transform DisplacementRoot => transform;

        public bool CanBeTargeted => _isActive && _runtimeState.IsAlive;

        public bool IsAlive => _isActive && _runtimeState.IsAlive;

        public DisplacementClass DisplacementClass => DisplacementClass.Standard;

        public float CollisionRadius => _configuration.CollisionRadius;

        public float CurrentHitPoints => _runtimeState.CurrentHitPoints;

        public float MaximumHitPoints => _runtimeState.MaximumHitPoints;

        public OrdinaryEnemyBrainState BrainState => _brain.State;

        public float AttackCooldown => _brain.AttackCooldown;

        public Transform AggroTarget => _aggroTarget;

        public void InitializeInfrastructure(
            CharacterController body,
            Transform targetPoint,
            Collider sensingCollider,
            int targetLayer)
        {
            _body = body != null ? body : throw new ArgumentNullException(nameof(body));
            _targetPoint = targetPoint != null ? targetPoint : throw new ArgumentNullException(nameof(targetPoint));
            _sensingCollider = sensingCollider != null
                ? sensingCollider
                : throw new ArgumentNullException(nameof(sensingCollider));
            ValidateSensingColliderContract(targetLayer);
            _isInitialized = true;
        }

        public void Activate(
            EnemyRuntimeConfiguration configuration,
            Transform aggroTarget,
            Vector3 position,
            Action<OrdinaryEnemyController> recycleRequested)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Enemy infrastructure must be initialized before activation.");
            }

            _aggroTarget = aggroTarget != null ? aggroTarget : throw new ArgumentNullException(nameof(aggroTarget));
            _recycleRequested = recycleRequested ?? throw new ArgumentNullException(nameof(recycleRequested));
            _configuration = configuration;
            _runtimeState.Reset(configuration.MaximumHitPoints);
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
            _isActive = true;
            gameObject.name = $"Enemy [{configuration.Id}]";
            gameObject.SetActive(true);
        }

        public void PrepareForPool(Vector3 poolPosition)
        {
            _isActive = false;
            _brain.Reset();
            _runtimeState.MarkPooled();
            _aggroTarget = null;
            _recycleRequested = null;
            AttackRequested = null;
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
            if (!_isActive || !_runtimeState.IsAlive)
            {
                return new DamageResult(0f, false);
            }

            var appliedDamage = _runtimeState.ApplyDamage(request.RawDamage);
            var wasLethal = !_runtimeState.IsAlive;
            var result = new DamageResult(appliedDamage, wasLethal);
            if (wasLethal)
            {
                _isActive = false;
                _brain.Reset();
                _recycleRequested(this);
            }

            return result;
        }

        public bool TryDisplace(Vector3 destination, in DisplacementContext context)
        {
            if (!_isActive || !_runtimeState.IsAlive)
            {
                return false;
            }

            _body.Move(destination - transform.position);
            return true;
        }

        private void Update()
        {
            if (!_isActive || !_runtimeState.IsAlive || _aggroTarget == null)
            {
                return;
            }

            var offset = _aggroTarget.position - transform.position;
            offset.y = 0f;
            var decision = _brain.Tick(Time.deltaTime, true, offset.magnitude);
            if (decision.ShouldApproach && offset.sqrMagnitude > Mathf.Epsilon)
            {
                var direction = offset.normalized;
                _body.Move(direction * (_configuration.MoveSpeed * Time.deltaTime));
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }

            if (decision.ShouldAttack)
            {
                AttackRequested?.Invoke(new DamageRequest(_configuration.AttackDamage, DamageType.Physical));
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
