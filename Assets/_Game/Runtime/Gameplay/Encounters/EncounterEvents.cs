using System;
using Gravivore.Core.Events;
using UnityEngine;

namespace Gravivore.Gameplay.Encounters
{
    public interface IBossHealthSource
    {
        event Action<BossEncounterStartedEvent> EncounterStarted;
        event Action<Gravivore.Gameplay.Combat.DamageResult> Damaged;
        event Action<BossEncounterResetEvent> EncounterReset;

        float CurrentHitPoints { get; }
        float MaximumHitPoints { get; }
    }

    public readonly struct BossEncounterStartedEvent
    {
        public BossEncounterStartedEvent(string bossId) => BossId = bossId;

        public string BossId { get; }
    }

    public interface IMagnetarGuardDefeatSource
    {
        event Action<MagnetarGuardDefeatedEvent> Defeated;
    }

    public interface IMagnetarGuardActivationTarget
    {
        bool IsEncounterActive { get; }

        bool ActivateEncounter();
    }

    public readonly struct MagnetarGuardActivatedEvent
    {
        public MagnetarGuardActivatedEvent(string eliteId) => EliteId = eliteId;
        public string EliteId { get; }
    }

    public readonly struct MagnetarGuardDefeatedEvent
    {
        public MagnetarGuardDefeatedEvent(string eliteId, Vector3 position)
        {
            EliteId = eliteId;
            Position = position;
        }

        public string EliteId { get; }
        public Vector3 Position { get; }
    }

    public readonly struct EliteShockwaveTelegraphEvent
    {
        public EliteShockwaveTelegraphEvent(Vector3 origin, float radius, float duration)
        {
            Origin = origin;
            Radius = radius;
            Duration = duration;
        }

        public Vector3 Origin { get; }
        public float Radius { get; }
        public float Duration { get; }
    }

    public readonly struct EliteShockwaveResolvedEvent
    {
        public EliteShockwaveResolvedEvent(bool playerWasHit) => PlayerWasHit = playerWasHit;
        public bool PlayerWasHit { get; }
    }

    public readonly struct EliteShockwaveCancelledEvent
    {
        public EliteShockwaveCancelledEvent(Vector3 origin, float radius)
        {
            Origin = origin;
            Radius = radius;
        }

        public Vector3 Origin { get; }
        public float Radius { get; }
    }

    public readonly struct BossTelegraphEvent
    {
        public BossTelegraphEvent(
            BossAttackType attack,
            Vector3 origin,
            Vector3 direction,
            float range,
            float halfAngleDegrees,
            float width,
            float duration)
        {
            Attack = attack;
            Origin = origin;
            Direction = direction;
            Range = range;
            HalfAngleDegrees = halfAngleDegrees;
            Width = width;
            Duration = duration;
        }

        public BossAttackType Attack { get; }
        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
        public float Range { get; }
        public float HalfAngleDegrees { get; }
        public float Width { get; }
        public float Duration { get; }
    }

    public readonly struct BossAttackResolvedEvent
    {
        public BossAttackResolvedEvent(BossAttackType attack, bool playerWasHit)
        {
            Attack = attack;
            PlayerWasHit = playerWasHit;
        }

        public BossAttackType Attack { get; }
        public bool PlayerWasHit { get; }
    }

    public readonly struct BossPhaseChangedEvent
    {
        public BossPhaseChangedEvent(string bossId, bool isLowHealthPhase)
        {
            BossId = bossId;
            IsLowHealthPhase = isLowHealthPhase;
        }

        public string BossId { get; }
        public bool IsLowHealthPhase { get; }
    }

    public readonly struct BossEncounterResetEvent
    {
        public BossEncounterResetEvent(string bossId, Vector3 startPosition)
        {
            BossId = bossId;
            StartPosition = startPosition;
        }

        public string BossId { get; }
        public Vector3 StartPosition { get; }
    }

    public readonly struct BossDefeatedEvent
    {
        public BossDefeatedEvent(string bossId, Vector3 position)
        {
            BossId = bossId;
            Position = position;
        }

        public string BossId { get; }
        public Vector3 Position { get; }
    }

    public readonly struct BossCompletionSnapshot
    {
        public BossCompletionSnapshot(bool defeated) => Defeated = defeated;
        public bool Defeated { get; }
    }

    public sealed class BossCompletionState
    {
        private readonly string _bossId;

        public BossCompletionState(string bossId)
        {
            if (string.IsNullOrWhiteSpace(bossId)) throw new ArgumentException("Boss id is required.", nameof(bossId));
            _bossId = bossId;
        }

        private BossCompletionState(string bossId, in BossCompletionSnapshot snapshot) : this(bossId)
        {
            IsDefeated = snapshot.Defeated;
        }

        public event Action<BossDefeatedEvent> Defeated;

        public bool IsDefeated { get; private set; }

        public bool TryRecordDefeat(string bossId, Vector3 position)
        {
            if (IsDefeated || !string.Equals(_bossId, bossId, StringComparison.Ordinal)) return false;
            IsDefeated = true;
            SafeEventDispatch.Publish(Defeated, new BossDefeatedEvent(_bossId, position));
            return true;
        }

        public BossCompletionSnapshot ExportSnapshot() => new BossCompletionSnapshot(IsDefeated);

        public static BossCompletionState Restore(string bossId, in BossCompletionSnapshot snapshot)
        {
            return new BossCompletionState(bossId, snapshot);
        }
    }
}
