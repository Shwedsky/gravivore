using UnityEngine;

namespace Gravivore.Gameplay.Combat
{
    // Immutable position survives immediate lethal-target pool return.
    public readonly struct PlayerAttackResolvedEvent
    {
        public PlayerAttackResolvedEvent(Vector3 position, DamageResult result)
        { Position = position; Result = result; }
        public Vector3 Position { get; }
        public DamageResult Result { get; }
    }
}
