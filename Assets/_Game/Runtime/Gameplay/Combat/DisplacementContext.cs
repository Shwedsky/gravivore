using UnityEngine;

namespace Gravivore.Gameplay.Combat
{
    public readonly struct DisplacementContext
    {
        public DisplacementContext(Vector3 sourcePosition, float requestedDistance, float resolvedDistance)
        {
            SourcePosition = sourcePosition;
            RequestedDistance = requestedDistance;
            ResolvedDistance = resolvedDistance;
        }

        public Vector3 SourcePosition { get; }

        public float RequestedDistance { get; }

        public float ResolvedDistance { get; }
    }
}
