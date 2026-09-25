using UnityEngine;

namespace Gravivore.Gameplay.Combat
{
    public enum CombatFaction
    {
        Player = 0,
        Hostile = 1,
        Neutral = 2
    }

    public interface ITargetable
    {
        Transform TargetPoint { get; }

        bool CanBeTargeted { get; }

        bool IsHostileTo(CombatFaction faction);
    }

    public interface IDamageable
    {
        bool IsAlive { get; }

        DamageResult ApplyDamage(in DamageRequest request);
    }

    public interface IDisplaceable
    {
        Transform DisplacementRoot { get; }

        DisplacementClass DisplacementClass { get; }

        float CollisionRadius { get; }

        bool TryDisplace(Vector3 destination, in DisplacementContext context);
    }

    public interface IGravityLashVfx
    {
        void Play(Vector3 origin, Vector3 destination);
    }

    public interface IPullDestinationResolver
    {
        Vector3 Resolve(Vector3 origin, Vector3 requestedDestination, float collisionRadius);
    }
}
