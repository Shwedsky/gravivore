using System;
using UnityEngine;

namespace Gravivore.Presentation.Combat
{
    /// <summary>Cosmetic displacement offset only. No target/collider/authority state.</summary>
    public struct BoundedPullTrail
    {
        private Vector3 _start;
        private float _remaining, _duration;
        public bool Active => _remaining > 0;
        public Vector3 Offset { get; private set; }
        public void Begin(Vector3 difference, float duration, float maximumOffset)
        {
            if (duration <= 0 || duration > .25f || maximumOffset <= 0 || float.IsNaN(duration) || float.IsInfinity(duration) ||
                float.IsNaN(maximumOffset) || float.IsInfinity(maximumOffset) || !Finite(difference))
                throw new ArgumentOutOfRangeException(nameof(duration));
            _start = Vector3.ClampMagnitude(difference,maximumOffset); _duration = _remaining = duration; Offset = _start;
        }
        public void Tick(float dt)
        {
            if (dt < 0 || float.IsNaN(dt) || float.IsInfinity(dt)) throw new ArgumentOutOfRangeException(nameof(dt));
            if (!Active) { Offset = Vector3.zero; return; }
            _remaining = Mathf.Max(0,_remaining-dt);
            var ratio = _remaining/_duration; Offset = _start * ratio * ratio;
        }
        private static bool Finite(Vector3 v) => !float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z) &&
            !float.IsInfinity(v.x) && !float.IsInfinity(v.y) && !float.IsInfinity(v.z);
    }
}
