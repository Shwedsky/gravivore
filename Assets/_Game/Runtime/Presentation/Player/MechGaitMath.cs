using UnityEngine;

namespace Gravivore.Presentation.Player
{
    public readonly struct MechanicalStep
    {
        public MechanicalStep(float phase) { Swing = Mathf.Sin(phase); Lift = Mathf.Max(0, Swing); }
        public float Swing { get; }
        public float Lift { get; }
    }
}
