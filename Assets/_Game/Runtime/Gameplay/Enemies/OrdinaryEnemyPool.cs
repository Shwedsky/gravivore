using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gravivore.Gameplay.Enemies
{
    public sealed class OrdinaryEnemyPool
    {
        private readonly Stack<OrdinaryEnemyController> _available;
        private readonly HashSet<OrdinaryEnemyController> _leased;
        private readonly Transform _poolRoot;
        private readonly int _targetLayer;

        public OrdinaryEnemyPool(Transform poolRoot, int capacity, int targetLayer)
        {
            _poolRoot = poolRoot != null ? poolRoot : throw new ArgumentNullException(nameof(poolRoot));
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            if (targetLayer < 0 || targetLayer > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(targetLayer));
            }

            _targetLayer = targetLayer;
            _available = new Stack<OrdinaryEnemyController>(capacity);
            _leased = new HashSet<OrdinaryEnemyController>();
            for (var i = 0; i < capacity; i++)
            {
                var enemy = CreateEnemy(i);
                enemy.PrepareForPool(_poolRoot.position);
                _available.Push(enemy);
            }
        }

        public int Capacity => _available.Count + _leased.Count;

        public int AvailableCount => _available.Count;

        public int LeasedCount => _leased.Count;

        public OrdinaryEnemyController Acquire(
            EnemyRuntimeConfiguration configuration,
            Transform aggroTarget,
            Vector3 position,
            Action<OrdinaryEnemyController> recycleRequested)
        {
            if (_available.Count == 0)
            {
                return null;
            }

            var enemy = _available.Pop();
            if (!_leased.Add(enemy))
            {
                throw new InvalidOperationException("Enemy pool lease tracking is inconsistent.");
            }

            enemy.Activate(configuration, aggroTarget, position, recycleRequested);
            return enemy;
        }

        public void Return(OrdinaryEnemyController enemy)
        {
            if (enemy == null)
            {
                throw new ArgumentNullException(nameof(enemy));
            }

            if (!_leased.Remove(enemy))
            {
                throw new InvalidOperationException("Enemy is not leased from this pool.");
            }

            enemy.PrepareForPool(_poolRoot.position);
            _available.Push(enemy);
        }

        private OrdinaryEnemyController CreateEnemy(int index)
        {
            var enemyObject = new GameObject(
                $"Pooled Ordinary Enemy {index}",
                typeof(CharacterController),
                typeof(OrdinaryEnemyController));
            enemyObject.layer = 0;
            enemyObject.transform.SetParent(_poolRoot, false);

            var body = enemyObject.GetComponent<CharacterController>();
            body.stepOffset = 0.2f;
            body.slopeLimit = 45f;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Enemy Visual";
            visual.layer = 0;
            visual.transform.SetParent(enemyObject.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            visual.transform.localScale = new Vector3(0.7f, 0.75f, 0.7f);
            var visualCollider = visual.GetComponent<Collider>();
            visualCollider.enabled = false;
            UnityEngine.Object.Destroy(visualCollider);

            var sensorObject = new GameObject("Combat Target Sensor", typeof(SphereCollider));
            sensorObject.layer = _targetLayer;
            sensorObject.transform.SetParent(enemyObject.transform, false);
            var sensingCollider = sensorObject.GetComponent<SphereCollider>();
            sensingCollider.isTrigger = true;

            var controller = enemyObject.GetComponent<OrdinaryEnemyController>();
            controller.InitializeInfrastructure(body, sensorObject.transform, sensingCollider, _targetLayer);
            return controller;
        }
    }
}
