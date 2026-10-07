using System;
using Gravivore.Gameplay.Progression;
using Gravivore.Presentation.Combat;
using Gravivore.Presentation.Evolution;
using Gravivore.Presentation.Feedback;
using Gravivore.Presentation.Assets;
using UnityEngine;

namespace Gravivore.Presentation.Player
{
    /// <summary>Cached, replaceable visual rig. Never writes the CharacterController root.</summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(100)]
    public sealed class MechMotionPresenter : MonoBehaviour
    {
        private sealed class Joint
        {
            public readonly Transform Node;
            public readonly Vector3 Position;
            public readonly Quaternion Rotation;
            public Joint(Transform node)
            {
                Node = node != null ? node : throw new InvalidOperationException("Missing mech articulation transform.");
                Position = node.localPosition; Rotation = node.localRotation;
            }
            public void Pose(Vector3 offset, Vector3 angles)
            {
                Node.localPosition = Position + offset;
                Node.localRotation = Rotation * Quaternion.Euler(angles);
            }
        }

        private sealed class Rig
        {
            public Transform Form, Core;
            public Renderer CoreRenderer;
            public Vector3 CoreScale;
            public readonly MaterialPropertyBlock Properties = new MaterialPropertyBlock();
            public Joint Body;
            public bool HasProceduralJoints;
            public Quaternion FormRestRotation;
            public PresentationSocketSet Sockets;
            public Joint[] Hips = new Joint[2], Knees = new Joint[2], Feet = new Joint[2], Arms = new Joint[2], Elbows = new Joint[2];
        }

        private Rig[] _rigs;
        private Transform _authority, _socket;
        private Vector3 _socketRestPosition;
        private Quaternion _socketRestRotation;
        private PlayerEvolutionView _view;
        private S14PresentationDefinition _settings;
        private GravityLashVfxPool _lash;
        private S14AudioPresenter _audio;
        private Vector3 _previousPosition, _aim;
        private float _phase, _time, _attackRemaining;
        private GravityLashCue _attackPhase;
        private int _stepIndex;
        public int FootstepCueCount { get; private set; }
        private float _nextStepAudioTime;
        private Gravivore.Gameplay.Player.PlayerLocomotion _locomotion;
        private Gravivore.Gameplay.Combat.GravityAttackController _attack;
        private Quaternion _stationaryFacing;
        private bool _wasMoving;
        private float _strideDistance;
        public void ConfigureStride(float distance) => _strideDistance = Mathf.Max(.1f,distance);
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        public bool IsWalking { get; private set; }
        public Vector3 ObservedVelocity { get; private set; }
        public float GaitPhase => _phase;
        public Transform PresentationSocket => _socket;

        public void Initialize(Transform authority, PlayerEvolutionView view, Transform socket,
            S14PresentationDefinition settings, GravityLashVfxPool lash, S14AudioPresenter audio)
        {
            _authority = authority; _view = view; _socket = socket; _settings = settings;
            _lash = lash; _audio = audio; _previousPosition = authority.position;
            _locomotion = authority.GetComponent<Gravivore.Gameplay.Player.PlayerLocomotion>();
            _attack = authority.GetComponent<Gravivore.Gameplay.Combat.GravityAttackController>();
            _stationaryFacing = authority.rotation;
            _strideDistance = settings.MechStride;
            _socketRestPosition = socket.localPosition; _socketRestRotation = socket.localRotation;
            _rigs = new Rig[3];
            for (var t = 0; t < _rigs.Length; t++)
            {
                var form = view.GetTierForm((EvolutionTier)t);
                var rig = new Rig { Form = form, FormRestRotation = form.localRotation,
                    Sockets = new PresentationSocketSet(form) };
                _rigs[t] = rig;
                // Whole-form candidate swaps need not reproduce the prototype's private joint hierarchy.
                if (!HasPrototypeJoints(form)) continue;
                rig.HasProceduralJoints = true;
                rig.Body = new Joint(form.Find("01_RobotBody_CommonIdentity"));
                rig.Core = form.Find("01_RobotBody_CommonIdentity/GravityCore_Common");
                rig.CoreRenderer = rig.Core.GetComponent<Renderer>();
                rig.CoreScale = rig.Core.localScale;
                for (var i = 0; i < 2; i++)
                {
                    var side = i == 0 ? "Left" : "Right";
                    var hip = form.Find("02_TwoMechanicalLegs_Common/" + side + "Leg/HipPivot");
                    rig.Hips[i] = new Joint(hip);
                    rig.Knees[i] = new Joint(hip.Find("KneePivot"));
                    rig.Feet[i] = new Joint(hip.Find("KneePivot/FootPivot"));
                    var arm = form.Find("03_ArticulatedGravityArms_Common/" + side + "WeaponPivot");
                    rig.Arms[i] = new Joint(arm);
                    rig.Elbows[i] = new Joint(arm.Find("ElbowPivot"));
                }
                _rigs[t] = rig;
            }
            _lash.CuePlayed += OnAttackCue;
        }

        private static bool HasPrototypeJoints(Transform form)
        {
            if (form.Find("01_RobotBody_CommonIdentity/GravityCore_Common")?.GetComponent<Renderer>() == null) return false;
            foreach (var side in new[] { "Left", "Right" })
            {
                var hip = form.Find("02_TwoMechanicalLegs_Common/" + side + "Leg/HipPivot");
                var arm = form.Find("03_ArticulatedGravityArms_Common/" + side + "WeaponPivot");
                if (hip == null || hip.Find("KneePivot/FootPivot") == null ||
                    arm == null || arm.Find("ElbowPivot") == null) return false;
            }
            return true;
        }

        private void OnAttackCue(GravityLashCue cue, Vector3 destination)
        {
            _aim = destination;
            if (cue != GravityLashCue.Cancelled && (_locomotion == null || !_locomotion.HasMovementIntent) &&
                (_attack == null || _attack.ValidCurrentTarget == null))
            {
                var direction=destination-_authority.position; direction.y=0;
                if(direction.sqrMagnitude>.0001f) _stationaryFacing=Quaternion.LookRotation(direction);
            }
            _attackPhase = cue;
            _attackRemaining = cue == GravityLashCue.Cancelled ? 0f : cue == GravityLashCue.Windup ? _settings.LashWindupDuration :
                cue == GravityLashCue.Beam ? _settings.LashBeamDuration : _settings.LashImpactDuration;
            ApplyPose(0f);
        }

        private void LateUpdate() => Tick(Time.deltaTime);

        public void Tick(float dt)
        {
            if (_rigs == null || dt <= 0f) return;
            var delta = _authority.position - _previousPosition;
            _previousPosition = _authority.position;
            delta.y = 0;
            // Respawn / DEV teleport is not a giant step; authority is never corrected here.
            if (delta.sqrMagnitude > 4f) delta = Vector3.zero;
            ObservedVelocity = delta / dt;
            IsWalking = delta.sqrMagnitude > 0.000001f;
            _time += dt;
            _attackRemaining = Mathf.Max(0, _attackRemaining - dt);
            if (IsWalking)
            {
                _phase += delta.magnitude / _strideDistance * Mathf.PI * 2;
                var stepIndex = Mathf.FloorToInt(_phase / Mathf.PI);
                if (stepIndex != _stepIndex)
                {
                    _stepIndex = stepIndex;
                    if (_time >= _nextStepAudioTime)
                    {
                        _nextStepAudioTime = _time + _settings.StepMinimumInterval;
                        try { _audio.Play(S14AudioCue.Step); FootstepCueCount++; }
                        catch (Exception exception) { Debug.LogException(exception); }
                    }
                }
            }
            else _phase = 0;
            ApplyPose(dt);
        }

        private void ApplyPose(float dt)
        {
            var rig = _rigs[(int)_view.CurrentTier];
            var attacking = _attackRemaining > 0;
            var aim = _aim - _authority.position; aim.y = 0;
            var moving = _locomotion != null ? _locomotion.HasMovementIntent : IsWalking;
            if (moving || _wasMoving) _stationaryFacing = _authority.rotation;
            _wasMoving = moving;
            var target = _attack != null ? _attack.ValidCurrentTarget : null;
            if (!moving && target != null)
            {
                var targetDirection = target.TargetPoint.position - _authority.position; targetDirection.y = 0;
                if (targetDirection.sqrMagnitude > .0001f)
                    _stationaryFacing = Quaternion.RotateTowards(_stationaryFacing, Quaternion.LookRotation(targetDirection),
                        _settings.CombatFacingDegreesPerSecond * dt);
            }
            if (attacking && aim.sqrMagnitude > 0.0001f)
            {
                // Moving attacks retain their established temporary aim. At rest the live
                // selected target owns the complete cycle, including cooldown and recovery.
                if (moving) rig.Form.rotation = Quaternion.LookRotation(aim) * rig.FormRestRotation;
                else rig.Form.rotation = _stationaryFacing * rig.FormRestRotation;
            }
            else rig.Form.rotation = (moving ? _authority.rotation : _stationaryFacing) * rig.FormRestRotation;
            if (!rig.HasProceduralJoints)
            {
                if (rig.Sockets.TryGet(Gravivore.Presentation.Assets.PresentationSocket.AttackOrigin, out var origin))
                    _socket.SetPositionAndRotation(origin.position, origin.rotation);
                else
                { _socket.localPosition = _socketRestPosition; _socket.localRotation = _socketRestRotation; }
                return; // Imported geometry may remain static until its own presentation gait is integrated.
            }
            var direction = IsWalking ? rig.Form.InverseTransformDirection(ObservedVelocity).normalized : Vector3.forward;
            var idle = Mathf.Sin(_time * 1.8f) * _settings.MechIdleDegrees;
            var charge = attacking && _attackPhase == GravityLashCue.Windup;
            var release = attacking && _attackPhase == GravityLashCue.Beam;
            var recoil = release ? 5f * _attackRemaining / _settings.LashBeamDuration : 0f;
            rig.Body.Pose(new Vector3(0, IsWalking ? .012f * Mathf.Abs(Mathf.Sin(_phase)) : .003f * idle, 0),
                new Vector3(idle + recoil, 0, IsWalking ? Mathf.Sin(_phase) * 1.2f : 0));
            for (var i = 0; i < 2; i++)
            {
                var step = new MechanicalStep(_phase + i * Mathf.PI);
                var wave = IsWalking ? step.Swing : 0;
                var lift = IsWalking ? step.Lift : 0;
                var hip = wave * _settings.MechHipDegrees;
                var knee = lift * _settings.MechKneeDegrees;
                rig.Hips[i].Pose(Vector3.zero, new Vector3(hip * direction.z, 0, -hip * direction.x));
                rig.Knees[i].Pose(Vector3.zero, new Vector3(-knee, 0, 0));
                rig.Feet[i].Pose(Vector3.zero, new Vector3(-hip * direction.z + knee, 0, hip * direction.x));
                // Ground-relative foot plant with controlled swing clearance, independent of authoritative root.
                var relativeFootY = _authority.InverseTransformPoint(rig.Feet[i].Node.position).y;
                rig.Hips[i].Node.localPosition += Vector3.up * (.08f + lift * _settings.MechFootLift - relativeFootY);
                rig.Arms[i].Pose(Vector3.zero, new Vector3(charge ? -14 : release ? -9 + recoil : -wave * 7 + idle, 0, 0));
                rig.Elbows[i].Pose(Vector3.zero, new Vector3(charge ? -24 : release ? -18 : idle * 1.5f, 0, 0));
            }
            var intensity = charge ? 1.75f : release ? 2.1f : 1f + .08f * Mathf.Sin(_time * 2.2f);
            rig.Core.localScale = rig.CoreScale * (charge ? 1.08f : 1f + .015f * Mathf.Sin(_time * 2.2f));
            var color = new Color(.08f, .8f, 1f, 1f) * intensity; color.a = 1;
            rig.Properties.SetColor(BaseColorId, color); rig.Properties.SetColor(ColorId, color);
            rig.CoreRenderer.SetPropertyBlock(rig.Properties);
            // At the exposed core face, rotated with active form even for side/rear fire.
            _socket.position = rig.Core.position + rig.Form.forward * .12f;
            _socket.rotation = rig.Form.rotation;
        }

        private void OnDestroy()
        {
            if (_lash != null) _lash.CuePlayed -= OnAttackCue;
        }
    }
}
