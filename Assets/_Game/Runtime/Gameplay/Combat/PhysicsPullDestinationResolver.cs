using System;
using UnityEngine;

namespace Gravivore.Gameplay.Combat
{
    public sealed class PhysicsPullDestinationResolver : IPullDestinationResolver
    {
        private readonly LayerMask _hardBlockerLayers;
        private readonly float _clearance;

        public PhysicsPullDestinationResolver(LayerMask hardBlockerLayers, float clearance)
        {
            if (float.IsNaN(clearance) || float.IsInfinity(clearance) || clearance < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(clearance));
            }

            _hardBlockerLayers = hardBlockerLayers;
            _clearance = clearance;
        }

        public Vector3 Resolve(Vector3 origin, Vector3 requestedDestination, float collisionRadius)
        {
            if (float.IsNaN(collisionRadius) || float.IsInfinity(collisionRadius) || collisionRadius < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(collisionRadius));
            }

            var displacement = requestedDestination - origin;
            var requestedDistance = displacement.magnitude;
            if (requestedDistance <= Mathf.Epsilon)
            {
                return origin;
            }

            var hasBlocker = Physics.SphereCast(
                origin,
                collisionRadius,
                displacement / requestedDistance,
                out var hit,
                requestedDistance,
                _hardBlockerLayers,
                QueryTriggerInteraction.Ignore);
            var allowedDistance = SafePullMath.CalculateAllowedTravelDistance(
                requestedDistance,
                hasBlocker,
                hasBlocker ? hit.distance : 0f,
                _clearance);
            return origin + ((displacement / requestedDistance) * allowedDistance);
        }
    }
}
