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
        Impact
    }

    [DisallowMultipleComponent]
    public sealed class GravityLashVfxPool : MonoBehaviour, ITrackedGravityLashVfx
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
            public ITargetable Target;
            public EnemyLifeId Life;
        }

        private Sequence[] _sequences;
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
            if (_presentationOrigin != null) origin = _presentationOrigin.position;
            var sequence = FindAvailable();
            if (!IsActive(sequence)) ActiveCount++;
            ResetVisuals(sequence);
            sequence.Origin = origin;
            sequence.Destination = destination;
            sequence.Target = presentationTarget;
            if (presentationTarget is OrdinaryEnemyController enemy) sequence.Life = enemy.LifeId;
            sequence.Phase = _windupDuration > 0 ? GravityLashCue.Windup : GravityLashCue.Beam;
            sequence.Remaining = _windupDuration > 0 ? _windupDuration : _beamDuration;
            sequence.Beam.SetPosition(0, origin);
            sequence.Beam.SetPosition(1, destination);
            sequence.BeamObject.SetActive(_windupDuration <= 0);
            sequence.Charge.transform.position = origin;
            sequence.Charge.transform.localScale = Vector3.one * .45f;
            sequence.Charge.SetActive(_windupDuration > 0);
            LastPlayedObject = sequence.BeamObject;
            PublishCue(sequence.Phase, destination);
            // Observers aim the active form before sampling its live socket.
            if (_presentationOrigin != null) sequence.Origin = _presentationOrigin.position;
            sequence.Charge.transform.position = sequence.Origin;
        }

        public void Tick(float deltaTime)
        {
            if (!_isInitialized) return;
            for (var i = 0; i < _sequences.Length; i++)
            {
                var sequence = _sequences[i];
                if (!IsActive(sequence)) continue;
                sequence.Remaining -= deltaTime;
                if (sequence.Target != null && sequence.Target.CanBeTargeted && sequence.Target.TargetPoint != null &&
                    (!(sequence.Target is OrdinaryEnemyController trackedEnemy) || trackedEnemy.LifeId.Equals(sequence.Life)))
                    sequence.Destination = sequence.Target.TargetPoint.position;
                else sequence.Target = null;
                if (sequence.Phase == GravityLashCue.Windup)
                {
                    if (_presentationOrigin != null) sequence.Origin = _presentationOrigin.position;
                    sequence.Charge.transform.position = sequence.Origin;
                    sequence.Charge.transform.localScale = Vector3.one * Mathf.Lerp(.45f, .8f, 1 - sequence.Remaining / _windupDuration);
                }
                if (sequence.Phase == GravityLashCue.Beam && _presentationOrigin != null)
                    sequence.Beam.SetPosition(0, _presentationOrigin.position);
                if (sequence.Phase == GravityLashCue.Beam) sequence.Beam.SetPosition(1, sequence.Destination);
                if (sequence.Remaining > 0f) continue;
                Advance(sequence);
            }
        }

        private void Update() => Tick(Time.deltaTime);

        private void Advance(Sequence sequence)
        {
            if (sequence.Phase == GravityLashCue.Windup)
            {
                sequence.Charge.SetActive(false);
                sequence.Phase = GravityLashCue.Beam;
                sequence.Remaining = _beamDuration;
                PublishCue(GravityLashCue.Beam, sequence.Destination);
                sequence.Origin = _presentationOrigin != null ? _presentationOrigin.position : sequence.Origin;
                sequence.Beam.SetPosition(0, sequence.Origin);
                sequence.Beam.SetPosition(1, sequence.Destination);
                sequence.BeamObject.SetActive(true);
                return;
            }
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
            ResetVisuals(reused);
            ActiveCount--;
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
