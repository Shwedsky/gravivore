using System;
using Gravivore.Gameplay.Quests;

namespace Gravivore.Gameplay.Offline
{
    public static class OfflineRewardEligibility
    {
        public static bool IsUnlocked(QuestState quests)
        {
            return quests != null
                ? quests.ExpandedObjectivesUnlocked
                : throw new ArgumentNullException(nameof(quests));
        }
    }

    public readonly struct OfflineRewardConfiguration
    {
        public OfflineRewardConfiguration(
            double activeBaselineUnitsPerHour,
            double efficiency,
            TimeSpan maximumEligibleDuration)
        {
            if (double.IsNaN(activeBaselineUnitsPerHour) ||
                double.IsInfinity(activeBaselineUnitsPerHour) ||
                activeBaselineUnitsPerHour < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(activeBaselineUnitsPerHour));
            }

            if (double.IsNaN(efficiency) || double.IsInfinity(efficiency) || efficiency <= 0d || efficiency > 1d)
            {
                throw new ArgumentOutOfRangeException(nameof(efficiency));
            }

            if (maximumEligibleDuration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumEligibleDuration));
            }

            ActiveBaselineUnitsPerHour = activeBaselineUnitsPerHour;
            Efficiency = efficiency;
            MaximumEligibleDuration = maximumEligibleDuration;
        }

        public double ActiveBaselineUnitsPerHour { get; }
        public double Efficiency { get; }
        public TimeSpan MaximumEligibleDuration { get; }
    }

    public enum OfflineClockAnomaly
    {
        None = 0,
        NonPositiveElapsed = 1
    }

    public readonly struct OfflineReturnSummary
    {
        public OfflineReturnSummary(
            TimeSpan realElapsed,
            TimeSpan eligibleDuration,
            long earnedAmount,
            long totalPendingAmount,
            bool wasCapped,
            OfflineClockAnomaly clockAnomaly)
        {
            RealElapsed = realElapsed;
            EligibleDuration = eligibleDuration;
            EarnedAmount = earnedAmount;
            TotalPendingAmount = totalPendingAmount;
            WasCapped = wasCapped;
            ClockAnomaly = clockAnomaly;
        }

        public TimeSpan RealElapsed { get; }
        public TimeSpan EligibleDuration { get; }
        public long EarnedAmount { get; }
        public long TotalPendingAmount { get; }
        public bool WasCapped { get; }
        public OfflineClockAnomaly ClockAnomaly { get; }
    }

    public sealed class OfflineRewardState
    {
        public OfflineRewardState(long materialBalance = 0, long pendingReward = 0)
        {
            if (materialBalance < 0) throw new ArgumentOutOfRangeException(nameof(materialBalance));
            if (pendingReward < 0) throw new ArgumentOutOfRangeException(nameof(pendingReward));
            MaterialBalance = materialBalance;
            PendingReward = pendingReward;
        }

        public event Action Changed;

        public long MaterialBalance { get; private set; }
        public long PendingReward { get; private set; }

        internal void AddPending(long amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (amount == 0) return;
            PendingReward = checked(PendingReward + amount);
            Changed?.Invoke();
        }

        internal bool ClaimPending()
        {
            if (PendingReward == 0) return false;
            var nextBalance = checked(MaterialBalance + PendingReward);
            MaterialBalance = nextBalance;
            PendingReward = 0;
            Changed?.Invoke();
            return true;
        }
    }

    public sealed class OfflineRewardService
    {
        private readonly OfflineRewardConfiguration _configuration;

        public OfflineRewardService(OfflineRewardConfiguration configuration, OfflineRewardState state)
        {
            _configuration = configuration;
            State = state ?? throw new ArgumentNullException(nameof(state));
        }

        public OfflineRewardState State { get; }

        public OfflineReturnSummary Accrue(DateTime lastSeenUtc, DateTime nowUtc)
        {
            if (lastSeenUtc.Kind != DateTimeKind.Utc) throw new ArgumentException("lastSeenUtc must be UTC.", nameof(lastSeenUtc));
            if (nowUtc.Kind != DateTimeKind.Utc) throw new ArgumentException("nowUtc must be UTC.", nameof(nowUtc));

            var deltaTicks = nowUtc.Ticks - lastSeenUtc.Ticks;
            if (deltaTicks <= 0)
            {
                return new OfflineReturnSummary(
                    TimeSpan.FromTicks(deltaTicks),
                    TimeSpan.Zero,
                    0,
                    State.PendingReward,
                    false,
                    OfflineClockAnomaly.NonPositiveElapsed);
            }

            var realElapsed = TimeSpan.FromTicks(deltaTicks);
            var wasCapped = deltaTicks > _configuration.MaximumEligibleDuration.Ticks;
            var eligible = wasCapped ? _configuration.MaximumEligibleDuration : realElapsed;
            var rawReward = _configuration.ActiveBaselineUnitsPerHour *
                            _configuration.Efficiency *
                            eligible.TotalHours;
            var earned = checked((long)Math.Round(rawReward, MidpointRounding.AwayFromZero));
            State.AddPending(earned);
            return new OfflineReturnSummary(
                realElapsed,
                eligible,
                earned,
                State.PendingReward,
                wasCapped,
                OfflineClockAnomaly.None);
        }

        public bool ClaimPendingReward() => State.ClaimPending();
    }
}
