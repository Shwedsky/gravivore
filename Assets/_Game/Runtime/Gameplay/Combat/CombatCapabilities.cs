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

    // Optional presentation capability. Play releases immediately at the gameplay commit.
    public interface ITrackedGravityLashVfx : IGravityLashVfx
    {
        void Play(Vector3 origin, Vector3 destination, ITargetable presentationTarget);
    }

    // Cosmetic preparation only. Neither expiry nor completion may commit an attack.
    public interface IPrechargedGravityLashVfx : ITrackedGravityLashVfx
    {
        float ChargeDuration { get; }

        void BeginCharge(Vector3 origin, Vector3 destination, ITargetable target, float remainingUntilCommit);

        void CancelCharge();
    }

    public interface IPullDestinationResolver
    {
        Vector3 Resolve(Vector3 origin, Vector3 requestedDestination, float collisionRadius);
    }
}
