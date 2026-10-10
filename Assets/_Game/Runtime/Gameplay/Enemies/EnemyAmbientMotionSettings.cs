using System;
using UnityEngine;

namespace Gravivore.Gameplay.Enemies
{
    public readonly struct AmbientPatrolParameters
    {
        public readonly float Radius, Speed, MinimumPause, MaximumPause, TravelSeconds, TurnSpeed;
        public bool Enabled => Radius > 0;
        public AmbientPatrolParameters(float radius, float speed, float minimumPause, float maximumPause, float travelSeconds, float turnSpeed)
        {
            foreach (var value in new[] { radius, speed, minimumPause, maximumPause, travelSeconds, turnSpeed })
                if (!float.IsFinite(value) || value <= 0) throw new ArgumentOutOfRangeException(nameof(radius));
            if (maximumPause < minimumPause) throw new ArgumentException("Pause interval is reversed.");
            Radius=radius; Speed=speed; MinimumPause=minimumPause; MaximumPause=maximumPause; TravelSeconds=travelSeconds; TurnSpeed=turnSpeed;
        }
    }

    /// <summary>Deterministic, allocation-free short patrols around the leased spawn anchor.</summary>
    public struct AmbientPatrolState
    {
        private uint _random;
        private Vector3 _home, _destination;
        private float _remaining;
        private bool _travelling;
        public Vector3 Home => _home;
        public void Reset(Vector3 home, uint seed, in AmbientPatrolParameters settings)
        {
            _home=home; _destination=home; _random=seed==0?1:seed; _travelling=false;
            _remaining=settings.Enabled ? Next()*settings.MaximumPause : 0;
        }
        private float Next()
        {
            _random^=_random<<13; _random^=_random>>17; _random^=_random<<5;
            return (_random & 0x00ffffff)/16777216f;
        }
        public Vector3 Step(Vector3 position, float deltaTime, in AmbientPatrolParameters settings)
        {
            if (!float.IsFinite(deltaTime) || deltaTime < 0) throw new ArgumentOutOfRangeException(nameof(deltaTime));
            if (!settings.Enabled || deltaTime==0) return Vector3.zero;
            _remaining-=deltaTime;
            if (_remaining<=0)
            {
                _travelling=!_travelling;
                _remaining=_travelling?settings.TravelSeconds:Mathf.Lerp(settings.MinimumPause,settings.MaximumPause,Next());
                if (_travelling)
                {
                    var angle=Next()*Mathf.PI*2;
                    var radius=settings.Radius*Mathf.Sqrt(Next());
                    _destination=_home+new Vector3(Mathf.Cos(angle)*radius,0,Mathf.Sin(angle)*radius);
                }
            }
            // Combat may end outside the patrol envelope. Return through the real capsule,
            // without teleporting or broadening ambient movement responsibility.
            var fromHome=position-_home; fromHome.y=0;
            var returning=fromHome.sqrMagnitude>settings.Radius*settings.Radius;
            if (!_travelling && !returning) return Vector3.zero;
            var delta=(returning?_home:_destination)-position; delta.y=0;
            if (delta.sqrMagnitude<.0025f) { _travelling=false; return Vector3.zero; }
            return Vector3.ClampMagnitude(delta,settings.Speed*deltaTime);
        }
    }

    [CreateAssetMenu(menuName="Gravivore/Enemies/Ambient Motion")]
    public sealed class EnemyAmbientMotionSettings : ScriptableObject
    {
        [SerializeField,Min(.1f)] private float _radius=.9f;
        [SerializeField,Min(.1f)] private float _speed=.5f;
        [SerializeField,Min(.1f)] private float _minimumPause=1.4f, _maximumPause=3.6f;
        [SerializeField,Min(.1f)] private float _travelSeconds=2.4f, _turnSpeed=95;
        public AmbientPatrolParameters Parameters => new AmbientPatrolParameters(_radius,_speed,_minimumPause,_maximumPause,_travelSeconds,_turnSpeed);
        public void ValidateOrThrow() => _=Parameters;
    }
}
