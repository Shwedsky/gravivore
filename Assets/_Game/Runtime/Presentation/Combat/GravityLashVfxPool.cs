using System;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Presentation.AudioVfx;
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

    /// <summary>
    /// Compatibility facade used by the production composition root and mech pose presenter.
    /// The old procedural beam/pulse implementation has been replaced by the merged Phase6B
    /// bounded Audio/VFX pools. Gameplay still owns the exact release/damage commit frame.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GravityLashVfxPool : MonoBehaviour, IPrechargedGravityLashVfx
    {
        private sealed class Sequence
        {
            public GravityLashCue Phase;
            public float Remaining;
            public Vector3 Destination;
            public ITargetable Target;
            public EnemyLifeId Life;
            public bool Active;
            public Phase6BVfxInstance Effect;
        }

        private Sequence[] _sequences;
        private Sequence _charging;
        private Phase6BAudioPlayer _audio;
        private Phase6BVfxPool _vfx;
        private Transform _presentationOrigin;
        private float _beamDuration;
        private float _windupDuration;
        private float _impactDuration;
        private int _reuseCursor;
        private bool _isInitialized;
        private Action<GravityLashCue, Vector3>[] _cueObservers = Array.Empty<Action<GravityLashCue, Vector3>>();

        public event Action<GravityLashCue, Vector3> CuePlayed
        {
            add
            {
                var list = new System.Collections.Generic.List<Action<GravityLashCue, Vector3>>(_cueObservers);
                list.Add(value);
                _cueObservers = list.ToArray();
            }
            remove
            {
                var list = new System.Collections.Generic.List<Action<GravityLashCue, Vector3>>(_cueObservers);
                list.Remove(value);
                _cueObservers = list.ToArray();
            }
        }

        public int Capacity => _sequences != null ? _sequences.Length : 0;
        public int ActiveCount { get; private set; }
        public GameObject LastPlayedObject { get; private set; }
        public float ChargeDuration => _windupDuration;
        public bool UsesPhase6BProductionPack => _isInitialized && _audio != null && _vfx != null;
        public int Phase6BCreatedVfxCount => _vfx != null ? _vfx.CreatedInstanceCount : 0;
        public Phase6BAudioPlayer Audio => _audio;
        public Phase6BVfxPool Vfx => _vfx;

        public void Initialize(GravityAttackSettings settings, Material unlitMaterial)
        {
            Initialize(settings, unlitMaterial, null);
        }

        public void Initialize(
            GravityAttackSettings settings,
            Material unlitMaterial,
            S14PresentationDefinition presentation,
            Transform presentationOrigin = null,
            Phase6BProductionDefinition productionDefinition = null)
        {
            if (_isInitialized) throw new InvalidOperationException("Gravity lash presentation is already initialized.");
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (unlitMaterial == null) throw new ArgumentNullException(nameof(unlitMaterial));
            settings.ValidateOrThrow();

            var definition = productionDefinition != null ? productionDefinition : Phase6BProductionDefinition.LoadRequired();
            _presentationOrigin = presentationOrigin;
            _windupDuration = presentation != null ? presentation.LashWindupDuration : 0f;
            _beamDuration = presentation != null ? presentation.LashBeamDuration : settings.VfxDuration * 0.45f;
            _impactDuration = presentation != null ? presentation.LashImpactDuration : settings.VfxDuration;

            var audioObject = new GameObject("Phase6B Production Audio", typeof(Phase6BAudioPlayer));
            audioObject.transform.SetParent(transform, false);
            _audio = audioObject.GetComponent<Phase6BAudioPlayer>();
            _audio.Initialize(definition.AudioBank, 6);

            var vfxObject = new GameObject("Phase6B Production VFX", typeof(Phase6BVfxPool));
            vfxObject.transform.SetParent(transform, false);
            _vfx = vfxObject.GetComponent<Phase6BVfxPool>();
            var bindings = definition.CreateCombatBindings();
            for (var i = 0; i < bindings.Length; i++)
                if (bindings[i].Cue <= Phase6BVfxCue.PlayerImpact)
                    bindings[i] = new Phase6BVfxPool.Binding(bindings[i].Cue, bindings[i].Prefab,
                        Mathf.Min(bindings[i].Capacity, settings.VfxPoolSize));
            _vfx.Initialize(bindings);

            _sequences = new Sequence[settings.VfxPoolSize];
            for (var i = 0; i < _sequences.Length; i++) _sequences[i] = new Sequence();
            LastPlayedObject = vfxObject;
            _isInitialized = true;
        }

        public void BeginCharge(Vector3 origin, Vector3 destination, ITargetable target, float remainingUntilCommit)
        {
            EnsureInitialized();
            if (remainingUntilCommit <= 0f || float.IsNaN(remainingUntilCommit) || float.IsInfinity(remainingUntilCommit))
                throw new ArgumentOutOfRangeException(nameof(remainingUntilCommit));

            CancelCharge();
            var sequence = FindAvailable();
            sequence.Active = true;
            sequence.Phase = GravityLashCue.Windup;
            sequence.Remaining = remainingUntilCommit;
            SetTarget(sequence, destination, target);
            _charging = sequence;
            ActiveCount++;

            PublishCue(GravityLashCue.Windup, destination);
            var resolvedOrigin = ResolveOrigin(origin);
            _audio.TryPlay(Phase6BAudioCue.GravityLashCharge, resolvedOrigin);
            _vfx.TryPlay(Phase6BVfxCue.PlayerCharge, resolvedOrigin, destination, remainingUntilCommit);
            sequence.Effect = _vfx.LastPlayedInstance;
            LastPlayedObject = sequence.Effect.gameObject;
        }

        public void CancelCharge()
        {
            if (_charging == null) return;
            var destination = _charging.Destination;
            _vfx?.StopCue(Phase6BVfxCue.PlayerCharge);
            ResetSequence(_charging);
            _charging = null;
            ActiveCount = Mathf.Max(0, ActiveCount - 1);
            PublishCue(GravityLashCue.Cancelled, destination);
        }

        public void Play(Vector3 origin, Vector3 destination)
        {
            Play(origin, destination, null);
        }

        public void Play(Vector3 origin, Vector3 destination, ITargetable presentationTarget)
        {
            EnsureInitialized();
            if (_charging != null && (!ReferenceEquals(_charging.Target, presentationTarget) ||
                                      _charging.Target != null && !IsTargetLive(_charging)))
                CancelCharge();

            var sequence = _charging ?? FindAvailable();
            if (_charging == null)
            {
                sequence.Active = true;
                ActiveCount++;
            }
            _charging = null;
            _vfx.StopCue(Phase6BVfxCue.PlayerCharge);
            SetTarget(sequence, destination, presentationTarget);
            sequence.Phase = GravityLashCue.Beam;
            sequence.Remaining = _beamDuration;

            PublishCue(GravityLashCue.Beam, destination);
            var resolvedOrigin = ResolveOrigin(origin);
            _audio.TryPlay(Phase6BAudioCue.GravityLashRelease, resolvedOrigin);
            _vfx.TryPlay(Phase6BVfxCue.PlayerReleaseFlash, resolvedOrigin, destination);
            _vfx.TryPlay(Phase6BVfxCue.GravityLashTravel, resolvedOrigin, destination, _beamDuration);
            sequence.Effect = _vfx.LastPlayedInstance;
            LastPlayedObject = sequence.Effect.gameObject;
        }

        public void PlayEnemyHit(Vector3 position, int variationSeed)
        {
            EnsureInitialized();
            _audio.TryPlay(Phase6BAudioCue.EnemyMechanicalHit, position, variationSeed);
            var origin = position + Vector3.up * 0.7f;
            var direction = ResolveOrigin(transform.position) - position;
            direction.y = 0.25f;
            _vfx.TryPlay(Phase6BVfxCue.HostileImpact, origin, origin + direction.normalized);
        }

        public void PlayEnemyShutdown(Vector3 position, int variationSeed)
        {
            EnsureInitialized();
            _audio.TryPlay(Phase6BAudioCue.EnemyShutdown, position, variationSeed);
            _vfx.TryPlay(Phase6BVfxCue.MechanicalKillBurst, position + Vector3.up * 0.7f, position);
        }

        public void Tick(float deltaTime)
        {
            if (!_isInitialized) return;
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime));

            for (var i = 0; i < _sequences.Length; i++)
            {
                var sequence = _sequences[i];
                if (!sequence.Active) continue;

                if (sequence.Phase == GravityLashCue.Windup)
                {
                    sequence.Remaining = Mathf.Max(0f, sequence.Remaining - deltaTime);
                    if (sequence.Remaining <= 0f) sequence.Effect?.StopImmediate();
                    if (sequence.Target != null && !IsTargetLive(sequence) && ReferenceEquals(sequence, _charging))
                        CancelCharge();
                    continue;
                }

                if (IsTargetLive(sequence)) sequence.Destination = sequence.Target.TargetPoint.position;
                else sequence.Target = null;

                sequence.Remaining = Mathf.Max(0f, sequence.Remaining - deltaTime);
                if (sequence.Remaining > 0f) continue;

                if (sequence.Phase == GravityLashCue.Beam)
                {
                    sequence.Effect?.StopImmediate();
                    sequence.Phase = GravityLashCue.Impact;
                    sequence.Remaining = _impactDuration;
                    _audio.TryPlay(Phase6BAudioCue.GravityLashImpact, sequence.Destination);
                    _vfx.TryPlay(Phase6BVfxCue.PlayerImpact, sequence.Destination, sequence.Destination);
                    sequence.Effect = _vfx.LastPlayedInstance;
                    PublishCue(GravityLashCue.Impact, sequence.Destination);
                    continue;
                }

                ResetSequence(sequence);
                ActiveCount = Mathf.Max(0, ActiveCount - 1);
            }
        }

        private void Update() => Tick(Time.deltaTime);
        private void OnDisable()
        {
            CancelCharge();
            if (_sequences != null)
                for (var i = 0; i < _sequences.Length; i++) ResetSequence(_sequences[i]);
            ActiveCount = 0;
            _audio?.StopAll();
            _vfx?.StopAll();
        }

        private Sequence FindAvailable()
        {
            for (var i = 0; i < _sequences.Length; i++)
            {
                var index = (_reuseCursor + i) % _sequences.Length;
                if (_sequences[index].Active) continue;
                _reuseCursor = (index + 1) % _sequences.Length;
                return _sequences[index];
            }

            var reused = _sequences[_reuseCursor];
            _reuseCursor = (_reuseCursor + 1) % _sequences.Length;
            if (ReferenceEquals(reused, _charging))
            {
                CancelCharge();
            }
            else
            {
                ResetSequence(reused);
                ActiveCount = Mathf.Max(0, ActiveCount - 1);
            }
            return reused;
        }

        private void SetTarget(Sequence sequence, Vector3 destination, ITargetable target)
        {
            sequence.Destination = destination;
            sequence.Target = target;
            sequence.Life = target is OrdinaryEnemyController enemy ? enemy.LifeId : default;
        }

        private static bool IsTargetLive(Sequence sequence)
        {
            var target = sequence.Target;
            return target != null &&
                   (!(target is MonoBehaviour owner) || owner != null && owner.isActiveAndEnabled) &&
                   target.TargetPoint != null && target.CanBeTargeted &&
                   (!(target is IDamageable damageable) || damageable.IsAlive) &&
                   (!(target is OrdinaryEnemyController enemy) || enemy.LifeId.Equals(sequence.Life));
        }

        private Vector3 ResolveOrigin(Vector3 fallback) =>
            _presentationOrigin != null ? _presentationOrigin.position : fallback;

        private static void ResetSequence(Sequence sequence)
        {
            sequence.Effect?.StopImmediate();
            sequence.Effect = null;
            sequence.Active = false;
            sequence.Remaining = 0f;
            sequence.Target = null;
            sequence.Life = default;
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized) throw new InvalidOperationException("Gravity lash presentation must be initialized before use.");
        }

        private void PublishCue(GravityLashCue cue, Vector3 position)
        {
            var handlers = _cueObservers;
            for (var i = 0; i < handlers.Length; i++)
            {
                try { handlers[i](cue, position); }
                catch (Exception exception) { Debug.LogException(exception); }
            }
        }

        private void OnDestroy()
        {
            _audio?.StopAll();
            _vfx?.StopAll();
        }
    }
}
