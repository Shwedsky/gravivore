using System;
using Gravivore.Gameplay.World;
using UnityEngine;

namespace Gravivore.Gameplay.Encounters
{
    public static class BossAttackGeometry
    {
        public static bool IsInsideCircle(Vector3 center, Vector3 point, float radius)
        {
            ValidatePositive(radius, nameof(radius));
            return Flatten(point - center).sqrMagnitude <= radius * radius;
        }

        public static bool IsInsideCone(
            Vector3 origin,
            Vector3 forward,
            Vector3 point,
            float range,
            float halfAngleDegrees)
        {
            ValidatePositive(range, nameof(range));
            if (float.IsNaN(halfAngleDegrees) || float.IsInfinity(halfAngleDegrees) ||
                halfAngleDegrees <= 0f || halfAngleDegrees >= 90f)
            {
                throw new ArgumentOutOfRangeException(nameof(halfAngleDegrees));
            }

            var flatForward = Flatten(forward);
            if (flatForward.sqrMagnitude <= Mathf.Epsilon) throw new ArgumentException("Cone direction is required.", nameof(forward));
            var offset = Flatten(point - origin);
            if (offset.sqrMagnitude > range * range) return false;
            if (offset.sqrMagnitude <= Mathf.Epsilon) return true;
            return Vector3.Dot(flatForward.normalized, offset.normalized) >= Mathf.Cos(halfAngleDegrees * Mathf.Deg2Rad);
        }

        public static bool IsInsideLine(
            Vector3 start,
            Vector3 end,
            Vector3 point,
            float halfWidth)
        {
            ValidatePositive(halfWidth, nameof(halfWidth));
            var segment = Flatten(end - start);
            var lengthSquared = segment.sqrMagnitude;
            if (lengthSquared <= Mathf.Epsilon) return false;
            var fromStart = Flatten(point - start);
            var projection = Vector3.Dot(fromStart, segment) / lengthSquared;
            if (projection < 0f || projection > 1f) return false;
            var closest = Flatten(start) + segment * projection;
            return (Flatten(point) - closest).sqrMagnitude <= halfWidth * halfWidth;
        }

        public static Vector3 ClampChargeDestination(
            Vector3 origin,
            Vector3 direction,
            float requestedDistance,
            in WorldBounds bounds,
            float clearance)
        {
            ValidatePositive(requestedDistance, nameof(requestedDistance));
            if (float.IsNaN(clearance) || float.IsInfinity(clearance) || clearance < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(clearance));
            }

            var flatDirection = Flatten(direction);
            if (flatDirection.sqrMagnitude <= Mathf.Epsilon) return origin;
            flatDirection.Normalize();
            var maximumDistance = requestedDistance;
            if (flatDirection.x > Mathf.Epsilon)
            {
                maximumDistance = Mathf.Min(maximumDistance, (bounds.MaxX - clearance - origin.x) / flatDirection.x);
            }
            else if (flatDirection.x < -Mathf.Epsilon)
            {
                maximumDistance = Mathf.Min(maximumDistance, (bounds.MinX + clearance - origin.x) / flatDirection.x);
            }

            if (flatDirection.z > Mathf.Epsilon)
            {
                maximumDistance = Mathf.Min(maximumDistance, (bounds.MaxZ - clearance - origin.z) / flatDirection.z);
            }
            else if (flatDirection.z < -Mathf.Epsilon)
            {
                maximumDistance = Mathf.Min(maximumDistance, (bounds.MinZ + clearance - origin.z) / flatDirection.z);
            }

            return origin + flatDirection * Mathf.Max(0f, maximumDistance);
        }

        private static Vector3 Flatten(Vector3 value)
        {
            value.y = 0f;
            return value;
        }

        private static void ValidatePositive(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f) throw new ArgumentOutOfRangeException(name);
        }
    }
}
