using UnityEngine;

namespace Gravivore.Core.Input
{
    public static class MovementInputMath
    {
        public static Vector2 NormalizeDrag(Vector2 origin, Vector2 current, float radius)
        {
            if (radius <= 0f)
            {
                return Vector2.zero;
            }

            return Vector2.ClampMagnitude((current - origin) / radius, 1f);
        }

        public static Vector2 ApplyRadialDeadZone(Vector2 input, float deadZone)
        {
            var clampedInput = Vector2.ClampMagnitude(input, 1f);
            var clampedDeadZone = Mathf.Clamp01(deadZone);
            var magnitude = clampedInput.magnitude;

            if (magnitude <= clampedDeadZone || clampedDeadZone >= 1f)
            {
                return Vector2.zero;
            }

            var scaledMagnitude = (magnitude - clampedDeadZone) / (1f - clampedDeadZone);
            return clampedInput.normalized * scaledMagnitude;
        }
    }
}
