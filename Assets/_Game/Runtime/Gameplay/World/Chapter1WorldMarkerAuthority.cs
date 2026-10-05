using System;
using System.Collections.Generic;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Enemies;
using UnityEngine;

namespace Gravivore.Gameplay.World
{
    public enum Chapter1MarkerKind
    {
        StrongOrdinary = 0,
        Magnetar = 1,
        Custodian = 2,
        RepairHub = 3
    }

    public readonly struct Chapter1MarkerState
    {
        public Chapter1MarkerState(
            string id,
            Chapter1MarkerKind kind,
            Vector3 position,
            bool progressionFirstClearCompleted,
            bool repeatAvailable,
            TimeSpan cooldownRemaining,
            EncounterRewardEntitlement rewardEntitlement,
            int premiumRewardsRemaining,
            TimeSpan rewardWindowRemaining)
        {
            Id = !string.IsNullOrWhiteSpace(id) ? id : throw new ArgumentException("Marker id is required.", nameof(id));
            Kind = kind;
            Position = position;
            ProgressionFirstClearCompleted = progressionFirstClearCompleted;
            RepeatAvailable = repeatAvailable;
            CooldownRemaining = cooldownRemaining;
            RewardEntitlement = rewardEntitlement;
            PremiumRewardsRemaining = premiumRewardsRemaining;
            RewardWindowRemaining = rewardWindowRemaining;
        }

        public string Id { get; }
        public Chapter1MarkerKind Kind { get; }
        public Vector3 Position { get; }
        public bool ProgressionFirstClearCompleted { get; }
        public bool RepeatAvailable { get; }
        public TimeSpan CooldownRemaining { get; }
        public EncounterRewardEntitlement RewardEntitlement { get; }
        public int PremiumRewardsRemaining { get; }
        public TimeSpan RewardWindowRemaining { get; }
    }

    /// <summary>
    /// Gameplay-owned read authority for future map/presentation adapters. This class intentionally
    /// has no dependency on Presentation/Map and performs no UI work.
    /// </summary>
    public sealed class Chapter1WorldMarkerAuthority
    {
        private readonly RepeatableEncounterService _repeatable;
        private readonly IReadOnlyList<StrongOrdinarySpotDefinition> _strongSpots;

        public Chapter1WorldMarkerAuthority(
            RepeatableEncounterService repeatable,
            IReadOnlyList<StrongOrdinarySpotDefinition> strongSpots)
        {
            _repeatable = repeatable ?? throw new ArgumentNullException(nameof(repeatable));
            _strongSpots = strongSpots ?? throw new ArgumentNullException(nameof(strongSpots));
        }

        public Chapter1MarkerState ReadMagnetar(Vector3 position, bool firstClearCompleted) =>
            ToMarker("magnetar", Chapter1MarkerKind.Magnetar, position,
                _repeatable.Read(RepeatableEncounterKind.Magnetar, firstClearCompleted));

        public Chapter1MarkerState ReadCustodian(Vector3 position, bool firstClearCompleted) =>
            ToMarker("custodian", Chapter1MarkerKind.Custodian, position,
                _repeatable.Read(RepeatableEncounterKind.Custodian, firstClearCompleted));

        public IReadOnlyList<Chapter1MarkerState> ReadStrongOrdinary()
        {
            var result = new Chapter1MarkerState[_strongSpots.Count];
            for (var i = 0; i < _strongSpots.Count; i++)
            {
                var spot = _strongSpots[i];
                result[i] = new Chapter1MarkerState(
                    spot.Id,
                    Chapter1MarkerKind.StrongOrdinary,
                    spot.Position,
                    false,
                    true,
                    TimeSpan.Zero,
                    EncounterRewardEntitlement.FallbackRepeat,
                    0,
                    TimeSpan.Zero);
            }
            return result;
        }

        public Chapter1MarkerState ReadStrongOrdinary(int index)
        {
            var spot = _strongSpots[index];
            return new Chapter1MarkerState(spot.Id, Chapter1MarkerKind.StrongOrdinary, spot.Position,
                false, true, TimeSpan.Zero, EncounterRewardEntitlement.FallbackRepeat, 0, TimeSpan.Zero);
        }

        private static Chapter1MarkerState ToMarker(
            string id,
            Chapter1MarkerKind kind,
            Vector3 position,
            in RepeatableEncounterReadState state) =>
            new Chapter1MarkerState(
                id,
                kind,
                position,
                state.FirstClearCompleted,
                state.Available,
                state.CooldownRemaining,
                state.RewardEntitlement,
                state.PremiumRewardsRemaining,
                state.RewardWindowRemaining);
    }
}
