using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gravivore.Gameplay.Combat
{
    public interface ITargetSensor
    {
        IReadOnlyList<TargetCandidate<CombatTarget>> Collect(
            Transform source,
            CombatFaction sourceFaction,
            TargetingParameters parameters);

        bool HasLineOfSight(Vector3 origin, Vector3 destination);
    }

    // Authoring contract: each entity contributes exactly one dedicated sensing collider to
    // the configured target layer. Body and hitbox colliders must use other layers so the
    // fixed non-alloc buffer represents entities rather than an arbitrary collider count.
    public sealed class PhysicsTargetSensor : ITargetSensor
    {
        private readonly Collider[] _colliderBuffer;
        private readonly List<MonoBehaviour> _componentBuffer = new List<MonoBehaviour>(8);
        private readonly List<TargetCandidate<CombatTarget>> _candidates;
        private readonly LayerMask _targetLayers;
        private readonly LayerMask _hardBlockerLayers;

        public PhysicsTargetSensor(
            int colliderCapacity,
            LayerMask targetLayers,
            LayerMask hardBlockerLayers)
        {
            if (colliderCapacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(colliderCapacity));
            }

            _colliderBuffer = new Collider[colliderCapacity];
            _candidates = new List<TargetCandidate<CombatTarget>>(colliderCapacity);
            _targetLayers = targetLayers;
            _hardBlockerLayers = hardBlockerLayers;
        }

        public IReadOnlyList<TargetCandidate<CombatTarget>> Collect(
            Transform source,
            CombatFaction sourceFaction,
            TargetingParameters parameters)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            _candidates.Clear();
            var count = Physics.OverlapSphereNonAlloc(
                source.position,
                parameters.ReleaseRadius,
                _colliderBuffer,
                _targetLayers,
                QueryTriggerInteraction.Collide);

            for (var i = 0; i < count; i++)
            {
                if (!TryResolveTarget(_colliderBuffer[i], out var target) || Contains(target))
                {
                    continue;
                }

                var targetPoint = target.Targetable.TargetPoint;
                var valid = target.IsValidFor(sourceFaction);
                var distance = valid ? Vector3.Distance(source.position, targetPoint.position) : float.PositiveInfinity;
                var direction = valid ? targetPoint.position - source.position : Vector3.zero;
                var frontAlignment = direction.sqrMagnitude > Mathf.Epsilon
                    ? Vector3.Dot(source.forward.normalized, direction.normalized)
                    : 1f;
                valid = valid && HasLineOfSight(source.position, targetPoint.position);
                _candidates.Add(new TargetCandidate<CombatTarget>(target, distance, frontAlignment, valid));
            }

            return _candidates;
        }

        public bool HasLineOfSight(Vector3 origin, Vector3 destination)
        {
            return !Physics.Linecast(
                origin,
                destination,
                _hardBlockerLayers,
                QueryTriggerInteraction.Ignore);
        }

        private bool TryResolveTarget(Collider candidateCollider, out CombatTarget target)
        {
            _componentBuffer.Clear();
            candidateCollider.GetComponentsInParent(false, _componentBuffer);

            ITargetable targetable = null;
            IDamageable damageable = null;
            IDisplaceable displaceable = null;
            MonoBehaviour owner = null;

            for (var i = 0; i < _componentBuffer.Count; i++)
            {
                var component = _componentBuffer[i];
                if (targetable == null && component is ITargetable foundTargetable)
                {
                    targetable = foundTargetable;
                    owner = component;
                }

                if (damageable == null && component is IDamageable foundDamageable)
                {
                    damageable = foundDamageable;
                }

                if (displaceable == null && component is IDisplaceable foundDisplaceable)
                {
                    displaceable = foundDisplaceable;
                }
            }

            if (targetable == null || damageable == null)
            {
                target = default;
                return false;
            }

            target = new CombatTarget(owner, targetable, damageable, displaceable);
            return true;
        }

        private bool Contains(CombatTarget target)
        {
            for (var i = 0; i < _candidates.Count; i++)
            {
                if (_candidates[i].Target.Equals(target))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
