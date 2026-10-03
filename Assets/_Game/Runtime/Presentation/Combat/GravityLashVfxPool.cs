using System;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Presentation.Feedback;
using UnityEngine;

namespace Gravivore.Presentation.Combat
{
    public enum GravityLashCue
    {
        Windup,
        Beam,
        Impact,
        Cancelled
    }

    [DisallowMultipleComponent]
    public sealed class GravityLashVfxPool : MonoBehaviour, IPrechargedGravityLashVfx
    {
        private sealed class Sequence
        {
            public GameObject BeamObject;
            public LineRenderer Beam;
            public GameObject Impact;
            public GameObject Charge;
            public Vector3 Origin;
            public Vector3 Destination;
            public GravityLashCue Phase;
            public float Remaining;
            public float ChargeDuration;
            public ITargetable Target;
            public EnemyLifeId Life;
        }

        private Sequence[] _sequences;
        private Sequence _charging;
        private Transform _presentationOrigin;
        private Material[] _materials;
        private float _beamDuration;
        private float _windupDuration;
        private Mesh _pulseMesh;
        private Action<GravityLashCue, Vector3>[] _cueObservers = Array.Empty<Action<GravityLashCue, Vector3>>();
        private float _impactDuration;
        private int _reuseCursor;
        private bool _isInitialized;

        // Copy observer lists only when wiring changes, never at each attack.
        public event Action<GravityLashCue, Vector3> CuePlayed
        {
            add { var list = new System.Collections.Generic.List<Action<GravityLashCue, Vector3>>(_cueObservers); list.Add(value); _cueObservers = list.ToArray(); }
            remove { var list = new System.Collections.Generic.List<Action<GravityLashCue, Vector3>>(_cueObservers); list.Remove(value); _cueObservers = list.ToArray(); }
        }

        public int Capacity => _sequences != null ? _sequences.Length : 0;
        public int ActiveCount { get; private set; }
        public GameObject LastPlayedObject { get; private set; }
        public float ChargeDuration => _windupDuration;

        public void BeginCharge(Vector3 origin, Vector3 destination, ITargetable target, float remainingUntilCommit)
        {
            if (!_isInitialized) throw new InvalidOperationException("GravityLashVfxPool must be initialized before use.");
            if (remainingUntilCommit <= 0f || float.IsNaN(remainingUntilCommit) || float.IsInfinity(remainingUntilCommit))
                throw new ArgumentOutOfRangeException(nameof(remainingUntilCommit));
            CancelCharge();
            var sequence = FindAvailable();
            ActiveCount++;
            _charging = sequence;
            SetTarget(sequence, origin, destination, target);
            sequence.Phase = GravityLashCue.Windup;
            sequence.ChargeDuration = remainingUntilCommit;
            sequence.Remaining = remainingUntilCommit;
            sequence.Charge.transform.localScale = Vector3.one * .45f;
            sequence.Charge.SetActive(true);
            LastPlayedObject = sequence.BeamObject;
            PublishCue(GravityLashCue.Windup, destination);
            sequence.Origin = _presentationOrigin != null ? _presentationOrigin.position : origin;
            sequence.Charge.transform.position = sequence.Origin;
        }

        public void CancelCharge()
        {
            if (_charging == null) return;
            var destination = _charging.Destination;
            ResetVisuals(_charging);
            _charging = null;
            ActiveCount--;
            PublishCue(GravityLashCue.Cancelled, destination);
        }

        public void Initialize(GravityAttackSettings settings, Material unlitMaterial)
        {
            Initialize(settings, unlitMaterial, null);
        }

        public void Initialize(
            GravityAttackSettings settings,
            Material unlitMaterial,
            S14PresentationDefinition presentation,
            Transform presentationOrigin = null)
        {
            if (_isInitialized) throw new InvalidOperationException("Gravity lash VFX pool is already initialized.");
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (unlitMaterial == null) throw new ArgumentNullException(nameof(unlitMaterial));
            settings.ValidateOrThrow();
            _presentationOrigin = presentationOrigin;
            _windupDuration = presentation != null ? presentation.LashWindupDuration : 0f;
            _pulseMesh = IndustrialPulseMesh.Create();
            _beamDuration = presentation != null ? presentation.LashBeamDuration : settings.VfxDuration * 0.45f;
            _impactDuration = presentation != null ? presentation.LashImpactDuration : settings.VfxDuration;
            _materials = new[]
            {
                CreateMaterial(unlitMaterial, settings.VfxColor, "Beam"),
                CreateMaterial(unlitMaterial, new Color(1f, 0.85f, 0.3f, 1f), "Impact")
            };
            _sequences = new Sequence[settings.VfxPoolSize];
            for (var i = 0; i < _sequences.Length; i++) _sequences[i] = CreateSequence(i, settings);
            _isInitialized = true;
        }

        public void Play(Vector3 origin, Vector3 destination)
        {
            Play(origin, destination, null);
        }

        public void Play(Vector3 origin, Vector3 destination, ITargetable presentationTarget)
        {
            if (!_isInitialized) throw new InvalidOperationException("GravityLashVfxPool must be initialized before use.");
            // Only the gameplay Play call releases. A charge timer can never produce a beam/impact.
            if (_charging != null && (!ReferenceEquals(_charging.Target, presentationTarget) ||
                _charging.Target != null && !IsTargetLive(_charging)))
                CancelCharge();
            var sequence = _charging ?? FindAvailable();
            if (_charging == null) ActiveCount++;
            _charging = null;
            ResetVisuals(sequence);
            SetTarget(sequence, origin, destination, presentationTarget);
            sequence.Phase = GravityLashCue.Beam;
            sequence.Remaining = _beamDuration;
            LastPlayedObject = sequence.BeamObject;
            PublishCue(GravityLashCue.Beam, destination);
            // Observers aim the active form before sampling its live socket.
            if (_presentationOrigin != null) sequence.Origin = _presentationOrigin.position;
            sequence.Beam.SetPosition(0, sequence.Origin);
            sequence.Beam.SetPosition(1, destination);
            sequence.BeamObject.SetActive(true);
        }

        private void SetTarget(Sequence sequence, Vector3 origin, Vector3 destination, ITargetable target)
        {
            sequence.Origin = _presentationOrigin != null ? _presentationOrigin.position : origin;
            sequence.Destination = destination;
            sequence.Target = target;
            sequence.Life = target is OrdinaryEnemyController enemy ? enemy.LifeId : default;
        }

        private static bool IsTargetLive(Sequence sequence)
        {
            var target = sequence.Target;
            return target != null && (!(target is MonoBehaviour owner) || owner != null && owner.isActiveAndEnabled) &&
                target.TargetPoint != null && target.CanBeTargeted &&
                (!(target is IDamageable damageable) || damageable.IsAlive) &&
                (!(target is OrdinaryEnemyController enemy) || enemy.LifeId.Equals(sequence.Life));
        }

        public void Tick(float deltaTime)
        {
            if (!_isInitialized) return;
            for (var i = 0; i < _sequences.Length; i++)
            {
                var sequence = _sequences[i];
                if (!IsActive(sequence)) continue;
                sequence.Remaining = Mathf.Max(0f, sequence.Remaining - deltaTime);
                if (IsTargetLive(sequence))
                    sequence.Destination = sequence.Target.TargetPoint.position;
                else if (sequence.Phase == GravityLashCue.Windup && sequence.Target != null)
                {
                    CancelCharge();
                    continue;
                }
                else sequence.Target = null;
                if (sequence.Phase == GravityLashCue.Windup)
                {
                    if (_presentationOrigin != null) sequence.Origin = _presentationOrigin.position;
                    sequence.Charge.transform.position = sequence.Origin;
                    sequence.Charge.transform.localScale = Vector3.one * Mathf.Lerp(.45f, .8f, 1 - sequence.Remaining / sequence.ChargeDuration);
                    continue; // Hold a completed cosmetic charge until gameplay releases or cancels it.
                }
                if (sequence.Phase == GravityLashCue.Beam && _presentationOrigin != null)
                    sequence.Beam.SetPosition(0, _presentationOrigin.position);
                if (sequence.Phase == GravityLashCue.Beam) sequence.Beam.SetPosition(1, sequence.Destination);
                if (sequence.Remaining > 0f) continue;
                Advance(sequence);
            }
        }

        private void Update() => Tick(Time.deltaTime);
        private void OnDisable() => CancelCharge();

        private void Advance(Sequence sequence)
        {
            if (sequence.Phase == GravityLashCue.Beam)
            {
                sequence.BeamObject.SetActive(false);
                sequence.Phase = GravityLashCue.Impact;
                sequence.Remaining = _impactDuration;
                sequence.Impact.transform.position = sequence.Destination;
                sequence.Impact.transform.localScale = Vector3.one * 1.2f;
                sequence.Impact.SetActive(true);
                PublishCue(GravityLashCue.Impact, sequence.Destination);
                return;
            }

            ResetVisuals(sequence);
            ActiveCount--;
        }

        private Sequence FindAvailable()
        {
            for (var i = 0; i < _sequences.Length; i++)
            {
                var index = (_reuseCursor + i) % _sequences.Length;
                if (IsActive(_sequences[index])) continue;
                _reuseCursor = (index + 1) % _sequences.Length;
                return _sequences[index];
            }

            var reused = _sequences[_reuseCursor];
            _reuseCursor = (_reuseCursor + 1) % _sequences.Length;
            if (ReferenceEquals(reused, _charging)) CancelCharge();
            else { ResetVisuals(reused); ActiveCount--; }
            return reused;
        }

        private Sequence CreateSequence(int index, GravityAttackSettings settings)
        {
            var beamObject = new GameObject($"Gravity Lash Beam {index}", typeof(LineRenderer));
            beamObject.transform.SetParent(transform, false);
            var line = beamObject.GetComponent<LineRenderer>();
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.widthMultiplier = settings.VfxWidth;
            line.numCapVertices = 2;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.sharedMaterial = _materials[0];
            line.startColor = settings.VfxColor;
            line.endColor = new Color(settings.VfxColor.r, settings.VfxColor.g, settings.VfxColor.b, 0f);
            var impact = CreatePulse($"Gravity Lash Impact {index}", _materials[1]);
            var charge = CreatePulse($"Gravity Lash Charge {index}", _materials[0]);
            beamObject.SetActive(false);
            return new Sequence { BeamObject = beamObject, Beam = line, Impact = impact, Charge = charge };
        }

        private GameObject CreatePulse(string name, Material material)
        {
            var pulse = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            pulse.name = name;
            pulse.transform.SetParent(transform, false);
            pulse.GetComponent<MeshFilter>().sharedMesh = _pulseMesh;
            pulse.GetComponent<Renderer>().sharedMaterial = material;
            pulse.SetActive(false);
            return pulse;
        }

        private static bool IsActive(Sequence sequence) =>
            sequence.BeamObject.activeSelf || sequence.Impact.activeSelf || sequence.Charge.activeSelf;

        private static void ResetVisuals(Sequence sequence)
        {
            sequence.BeamObject.SetActive(false);
            sequence.Impact.SetActive(false);
            sequence.Charge.SetActive(false);
            sequence.Remaining = 0f;
            sequence.Target = null;
        }

        private void PublishCue(GravityLashCue cue, Vector3 position)
        {
            var handlers = _cueObservers;
            for (var i = 0; i < handlers.Length; i++)
            {
                try
                {
                    handlers[i](cue, position);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        private void OnDestroy()
        {
            if (_materials == null) return;
            if (_pulseMesh != null) Destroy(_pulseMesh);
            for (var i = 0; i < _materials.Length; i++) if (_materials[i] != null) Destroy(_materials[i]);
        }

        private static Material CreateMaterial(Material source, Color color, string role)
        {
            return new Material(source)
            {
                name = $"Gravity Lash {role} Material",
                color = color,
                hideFlags = HideFlags.HideAndDontSave
            };
        }
    }
}
