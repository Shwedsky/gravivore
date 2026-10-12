using UnityEngine;

namespace Gravivore.Gameplay.World
{
    /// <summary>Wall-only swept hull. Combat sensors and attack geometry retain their own radii.</summary>
    public sealed class ActorWallEnvelope
    {
        private readonly RaycastHit[] _hits = new RaycastHit[32];
        private readonly Collider[] _overlaps = new Collider[32];
        private float _radius;
        private int _mask;
        public float Radius => _radius;
        public void Configure(float radius, int wallMask) { _radius = Mathf.Max(0, radius); _mask = wallMask; }
        public Vector3 Resolve(Vector3 position, Vector3 step)
        {
            if (_radius <= 0 || _mask == 0) return step;
            var safe = Separate(position);
            var first = Sweep(safe, step, out var normal);
            var slide = Vector3.ProjectOnPlane(step - first, normal); slide.y = 0;
            var second = Sweep(safe + first, slide, out _);
            return Separate(safe + first + second) - position;
        }
        private Vector3 Sweep(Vector3 position, Vector3 step, out Vector3 normal)
        {
            normal = Vector3.zero; var distance = step.magnitude;
            if (distance < .00001f) return Vector3.zero;
            var count = Physics.SphereCastNonAlloc(position + Vector3.up * .55f, _radius,
                step / distance, _hits, distance + .02f, _mask, QueryTriggerInteraction.Ignore);
            var travel = distance;
            for (var i = 0; i < count; i++)
            {
                if (_hits[i].distance >= travel + .02f) continue;
                travel = Mathf.Max(0, _hits[i].distance - .02f); normal = _hits[i].normal; normal.y = 0; normal.Normalize();
            }
            return step / distance * travel;
        }
        private Vector3 Separate(Vector3 position)
        {
            for (var pass = 0; pass < 3; pass++)
            {
                var centre = position + Vector3.up * .55f;
                var count = Physics.OverlapSphereNonAlloc(centre, _radius + .02f, _overlaps, _mask, QueryTriggerInteraction.Ignore);
                if(count==0)return position;
                var corrected=false;
                for (var i = 0; i < count; i++)
                {
                    var away = centre - _overlaps[i].ClosestPoint(centre); away.y = 0;
                    var distance = away.magnitude;
                    if (distance < .0001f || distance >= _radius + .02f) continue;
                    position += away / distance * (_radius + .02f - distance); centre = position + Vector3.up * .55f;corrected=true;
                }
                if(!corrected)return position;
            }
            return position;
        }
    }
    public interface IActorWallEnvelopeSource { float WallEnvelopeRadius { get; } }
}
