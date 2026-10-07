using System;

namespace Gravivore.Presentation.Map
{
    public enum MapMarkerGlyph
    {
        Chevron,
        Node,
        BracketedNode,
        Diamond,
        BossCore,
        RepairCross,
        Gate
    }

    public readonly struct MapMarkerVisualState
    {
        public MapMarkerVisualState(
            MapMarkerGlyph glyph,
            bool filled,
            bool lockOverlay,
            bool cooldownRing,
            bool inactiveSlash,
            bool activeBrackets,
            bool rewardPip,
            bool rewardPipBarred,
            bool completionMark)
        {
            Glyph = glyph;
            Filled = filled;
            LockOverlay = lockOverlay;
            CooldownRing = cooldownRing;
            InactiveSlash = inactiveSlash;
            ActiveBrackets = activeBrackets;
            RewardPip = rewardPip;
            RewardPipBarred = rewardPipBarred;
            CompletionMark = completionMark;
        }

        public MapMarkerGlyph Glyph { get; }
        public bool Filled { get; }
        public bool LockOverlay { get; }
        public bool CooldownRing { get; }
        public bool InactiveSlash { get; }
        public bool ActiveBrackets { get; }
        public bool RewardPip { get; }
        public bool RewardPipBarred { get; }
        public bool CompletionMark { get; }
    }

    public static class MapMarkerVisualResolver
    {
        public static MapMarkerVisualState Resolve(MapMarkerSnapshot marker, bool expanded)
        {
            var glyph = ResolveGlyph(marker.Kind);
            var locked = marker.ProgressionLocked;
            var cooldown = marker.Availability == MapAvailabilityState.Cooldown;
            var completed = !locked &&
                            ((marker.Kind == MapMarkerKind.Boss || marker.Kind == MapMarkerKind.Elite) && marker.FirstClearCompleted ||
                             marker.Kind == MapMarkerKind.Boss && marker.Availability == MapAvailabilityState.Defeated);
            var inactive = !locked && !completed &&
                           (marker.Availability == MapAvailabilityState.Inactive ||
                            marker.Availability == MapAvailabilityState.Defeated);
            var filled = !locked && !cooldown && !inactive;
            var active = marker.Availability == MapAvailabilityState.Active;
            var showReward = expanded &&
                             (marker.Kind == MapMarkerKind.Elite || marker.Kind == MapMarkerKind.Boss) &&
                             (marker.RewardState == MapRewardState.Full || marker.RewardState == MapRewardState.CappedFallback);

            return new MapMarkerVisualState(
                glyph,
                filled,
                locked,
                cooldown,
                inactive,
                active,
                showReward,
                showReward && marker.RewardState == MapRewardState.CappedFallback,
                completed);
        }

        public static MapMarkerGlyph ResolveGlyph(MapMarkerKind kind)
        {
            switch (kind)
            {
                case MapMarkerKind.Player: return MapMarkerGlyph.Chevron;
                case MapMarkerKind.Ordinary: return MapMarkerGlyph.Node;
                case MapMarkerKind.StrongOrdinary: return MapMarkerGlyph.BracketedNode;
                case MapMarkerKind.Elite: return MapMarkerGlyph.Diamond;
                case MapMarkerKind.Boss: return MapMarkerGlyph.BossCore;
                case MapMarkerKind.RepairHub: return MapMarkerGlyph.RepairCross;
                case MapMarkerKind.Gate: return MapMarkerGlyph.Gate;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported map marker kind.");
            }
        }
    }
}
