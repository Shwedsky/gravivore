using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gravivore.Presentation.World
{
    /// <summary>
    /// One bounded presentation update for a few Phase 3D environment mechanisms.
    /// It never moves authority roots and owns no combat, navigation, timing, or state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Phase3DMechanicalMotionPresenter : MonoBehaviour
    {
        private readonly List<MotionChannel> _channels = new List<MotionChannel>(12);
        private bool _initialized;

        public int ChannelCount => _channels.Count;

        public void Initialize(Transform visualEnvironmentRoot)
        {
            if (_initialized) throw new InvalidOperationException("Phase 3D motion is already initialized.");
            if (visualEnvironmentRoot == null) throw new ArgumentNullException(nameof(visualEnvironmentRoot));

            var transforms = visualEnvironmentRoot.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
            {
                var target = transforms[i];
                switch (target.name)
                {
                    case "ServiceArm_L_Forearm":
                        Add(target, MotionKind.Hinge, 2.2f, .55f);
                        break;
                    case "ServiceArm_R_Forearm":
                        Add(target, MotionKind.Hinge, -2.2f, .55f);
                        break;
                    case "RearServiceArm_A":
                        Add(target, MotionKind.Hinge, 1.4f, .4f);
                        break;
                    case "RearServiceArm_B":
                        Add(target, MotionKind.Hinge, -1.4f, .4f);
                        break;
                    default:
                        if (target.name.StartsWith("EnergyIndicator_", StringComparison.Ordinal))
                            Add(target, MotionKind.Pulse, .035f, i * .31f);
                        else if (target.name.StartsWith("HeatIndicator_", StringComparison.Ordinal))
                            Add(target, MotionKind.Pulse, .025f, i * .31f);
                        break;
                }
            }

            _initialized = true;
        }

        private void Update()
        {
            if (!_initialized) return;
            var t = Time.unscaledTime;
            for (var i = 0; i < _channels.Count; i++)
            {
                var channel = _channels[i];
                if (channel.Transform == null) continue;
                switch (channel.Kind)
                {
                    case MotionKind.Hinge:
                        channel.Transform.localRotation =
                            channel.BaseRotation * Quaternion.AngleAxis(
                                Mathf.Sin(t * .65f + channel.Phase) * channel.Amount,
                                Vector3.forward);
                        break;
                    case MotionKind.Pulse:
                        channel.Transform.localScale =
                            channel.BaseScale * (1f + Mathf.Sin(t * 1.4f + channel.Phase) * channel.Amount);
                        break;
                }
            }
        }

        private void Add(Transform target, MotionKind kind, float amount, float phase)
        {
            _channels.Add(new MotionChannel(target, kind, amount, phase));
        }

        private enum MotionKind
        {
            Hinge,
            Pulse
        }

        private sealed class MotionChannel
        {
            public MotionChannel(Transform transform, MotionKind kind, float amount, float phase)
            {
                Transform = transform;
                Kind = kind;
                Amount = amount;
                Phase = phase;
                BaseRotation = transform.localRotation;
                BaseScale = transform.localScale;
            }

            public Transform Transform { get; }
            public MotionKind Kind { get; }
            public float Amount { get; }
            public float Phase { get; }
            public Quaternion BaseRotation { get; }
            public Vector3 BaseScale { get; }
        }
    }
}
