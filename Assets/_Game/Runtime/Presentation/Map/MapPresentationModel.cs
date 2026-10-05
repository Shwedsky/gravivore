using System;
using Gravivore.Gameplay.Encounters;
using UnityEngine;

namespace Gravivore.Presentation.Map
{
    public enum MapMarkerKind
    {
        Player,
        Ordinary,
        StrongOrdinary,
        Elite,
        Boss,
        RepairHub
    }

    public enum MapAvailabilityState
    {
        Available,
        Active,
        Cooldown,
        Inactive,
        Defeated
    }

    public enum MapRewardState
    {
        Unknown,
        Full,
        CappedFallback,
        None
    }

    public readonly struct MapMarkerSnapshot : IEquatable<MapMarkerSnapshot>
    {
        public MapMarkerSnapshot(
            string id,
            MapMarkerKind kind,
            Vector3 worldPosition,
            MapAvailabilityState availability,
            string displayName = null,
            bool visible = true,
            bool progressionLocked = false,
            float headingDegrees = 0f,
            float remainingSeconds = 0f,
            float cooldownProgress01 = -1f,
            MapRewardState rewardState = MapRewardState.Unknown,
            bool firstClearCompleted = false,
            EncounterRewardEntitlement rewardEntitlement = EncounterRewardEntitlement.FirstClear,
            int premiumRewardsRemaining = 0,
            float rewardWindowRemainingSeconds = 0f)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Marker id is required.", nameof(id));
            Id = id;
            Kind = kind;
            WorldPosition = worldPosition;
            Availability = availability;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? id : displayName;
            Visible = visible;
            ProgressionLocked = progressionLocked;
            HeadingDegrees = headingDegrees;
            RemainingSeconds = Mathf.Max(0f, remainingSeconds);
            CooldownProgress01 = cooldownProgress01 < 0f ? -1f : Mathf.Clamp01(cooldownProgress01);
            RewardState = rewardState;
            FirstClearCompleted = firstClearCompleted;
            RewardEntitlement = rewardEntitlement;
            PremiumRewardsRemaining = premiumRewardsRemaining;
            RewardWindowRemainingSeconds = rewardWindowRemainingSeconds;
        }

        public string Id { get; }
        public MapMarkerKind Kind { get; }
        public Vector3 WorldPosition { get; }
        public MapAvailabilityState Availability { get; }
        public string DisplayName { get; }
        public bool Visible { get; }
        public bool ProgressionLocked { get; }
        public float HeadingDegrees { get; }
        public float RemainingSeconds { get; }
        public float CooldownProgress01 { get; }
        public MapRewardState RewardState { get; }
        public bool FirstClearCompleted { get; }
        public EncounterRewardEntitlement RewardEntitlement { get; }
        public int PremiumRewardsRemaining { get; }
        public float RewardWindowRemainingSeconds { get; }

        public bool Equals(MapMarkerSnapshot other) =>
            Id == other.Id &&
            Kind == other.Kind &&
            WorldPosition == other.WorldPosition &&
            Availability == other.Availability &&
            DisplayName == other.DisplayName &&
            Visible == other.Visible &&
            ProgressionLocked == other.ProgressionLocked &&
            Mathf.Approximately(HeadingDegrees, other.HeadingDegrees) &&
            Mathf.Approximately(RemainingSeconds, other.RemainingSeconds) &&
            Mathf.Approximately(CooldownProgress01, other.CooldownProgress01) &&
            RewardState == other.RewardState && FirstClearCompleted == other.FirstClearCompleted &&
            RewardEntitlement == other.RewardEntitlement && PremiumRewardsRemaining == other.PremiumRewardsRemaining &&
            Mathf.Approximately(RewardWindowRemainingSeconds, other.RewardWindowRemainingSeconds);

        public override bool Equals(object obj) => obj is MapMarkerSnapshot other && Equals(other);
        public override int GetHashCode() => Id.GetHashCode();
    }

    public interface IMapMarkerSource
    {
        int Count { get; }
        MapMarkerSnapshot GetMarker(int index);
    }

    public interface IMapPoiSelectionSink
    {
        void OnMapPoiSelected(MapMarkerSnapshot marker);
        void OnMapPoiSelectionCleared();
    }
}
