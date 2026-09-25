using System;
using UnityEngine;

namespace Gravivore.Gameplay.Combat
{
    public readonly struct CombatTarget : IEquatable<CombatTarget>
    {
        public CombatTarget(
            MonoBehaviour owner,
            ITargetable targetable,
            IDamageable damageable,
            IDisplaceable displaceable)
        {
            Owner = owner;
            Targetable = targetable ?? throw new ArgumentNullException(nameof(targetable));
            Damageable = damageable ?? throw new ArgumentNullException(nameof(damageable));
            Displaceable = displaceable;
        }

        public MonoBehaviour Owner { get; }

        public ITargetable Targetable { get; }

        public IDamageable Damageable { get; }

        public IDisplaceable Displaceable { get; }

        public bool IsValidFor(CombatFaction sourceFaction)
        {
            return Owner != null && Owner.isActiveAndEnabled &&
                   Targetable.TargetPoint != null && Targetable.CanBeTargeted &&
                   Targetable.IsHostileTo(sourceFaction) && Damageable.IsAlive;
        }

        public bool Equals(CombatTarget other)
        {
            return ReferenceEquals(Targetable, other.Targetable);
        }

        public override bool Equals(object obj)
        {
            return obj is CombatTarget other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Targetable != null ? Targetable.GetHashCode() : 0;
        }
    }
}
