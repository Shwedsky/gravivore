using System;
using Gravivore.Core.Time;
using Gravivore.Gameplay.Progression;

namespace Gravivore.Gameplay.Encounters
{
    public enum RepeatableEncounterKind
    {
        Magnetar = 0,
        Custodian = 1
    }

    public enum EncounterRewardEntitlement
    {
        FirstClear = 0,
        PremiumRepeatFirstInWindow = 1,
        PremiumRepeat = 2,
        FallbackRepeat = 3
    }

    public enum PendingRewardPhase
    {
        Prepared = 0,
        Applied = 1
    }

    public readonly struct RepeatableEncounterRules
    {
        public RepeatableEncounterRules(TimeSpan cooldown, int premiumCap, TimeSpan rewardWindow)
        {
            if (cooldown <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(cooldown));
            if (premiumCap <= 0) throw new ArgumentOutOfRangeException(nameof(premiumCap));
            if (rewardWindow <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(rewardWindow));
            Cooldown = cooldown;
            PremiumCap = premiumCap;
            RewardWindow = rewardWindow;
        }

        public TimeSpan Cooldown { get; }
        public int PremiumCap { get; }
        public TimeSpan RewardWindow { get; }

        public static RepeatableEncounterRules Magnetar =>
            new RepeatableEncounterRules(TimeSpan.FromMinutes(15), 3, TimeSpan.FromHours(24));

        public static RepeatableEncounterRules Custodian =>
            new RepeatableEncounterRules(TimeSpan.FromMinutes(30), 2, TimeSpan.FromHours(24));
    }

    public sealed class RepeatableEncounterState
    {
        public RepeatableEncounterState(
            DateTime? nextAvailableUtc = null,
            DateTime? rewardWindowStartedUtc = null,
            int rewardedKillsInWindow = 0)
        {
            NextAvailableUtc = RequireOptionalUtc(nextAvailableUtc, nameof(nextAvailableUtc));
            RewardWindowStartedUtc = RequireOptionalUtc(rewardWindowStartedUtc, nameof(rewardWindowStartedUtc));
            if (rewardedKillsInWindow < 0) throw new ArgumentOutOfRangeException(nameof(rewardedKillsInWindow));
            if (!RewardWindowStartedUtc.HasValue && rewardedKillsInWindow != 0)
            {
                throw new ArgumentException("A reward count requires an anchored reward window.", nameof(rewardedKillsInWindow));
            }
            RewardedKillsInWindow = rewardedKillsInWindow;
        }

        public DateTime? NextAvailableUtc { get; private set; }
        public DateTime? RewardWindowStartedUtc { get; private set; }
        public int RewardedKillsInWindow { get; private set; }

        public bool IsAvailable(DateTime effectiveUtc) =>
            !NextAvailableUtc.HasValue || RequireUtc(effectiveUtc, nameof(effectiveUtc)) >= NextAvailableUtc.Value;

        public TimeSpan GetCooldownRemaining(DateTime effectiveUtc)
        {
            effectiveUtc = RequireUtc(effectiveUtc, nameof(effectiveUtc));
            if (!NextAvailableUtc.HasValue || effectiveUtc >= NextAvailableUtc.Value) return TimeSpan.Zero;
            return NextAvailableUtc.Value - effectiveUtc;
        }

        public RewardWindowSnapshot GetRewardWindow(DateTime effectiveUtc, in RepeatableEncounterRules rules)
        {
            effectiveUtc = RequireUtc(effectiveUtc, nameof(effectiveUtc));
            if (!RewardWindowStartedUtc.HasValue || effectiveUtc >= RewardWindowStartedUtc.Value + rules.RewardWindow)
            {
                return new RewardWindowSnapshot(
                    false,
                    0,
                    rules.PremiumCap,
                    TimeSpan.Zero,
                    true);
            }

            var remaining = rules.RewardWindow - (effectiveUtc - RewardWindowStartedUtc.Value);
            var count = Math.Min(RewardedKillsInWindow, rules.PremiumCap);
            return new RewardWindowSnapshot(
                true,
                count,
                Math.Max(0, rules.PremiumCap - count),
                remaining,
                count == 0);
        }

        public EncounterRewardEntitlement ClassifyReward(
            DateTime effectiveUtc,
            bool firstClearCompleted,
            in RepeatableEncounterRules rules)
        {
            effectiveUtc = RequireUtc(effectiveUtc, nameof(effectiveUtc));
            if (!firstClearCompleted) return EncounterRewardEntitlement.FirstClear;
            var window = GetRewardWindow(effectiveUtc, rules);
            if (!window.IsAnchored || window.IsFirstPremiumEligible)
            {
                return EncounterRewardEntitlement.PremiumRepeatFirstInWindow;
            }
            return window.RewardedKills < rules.PremiumCap
                ? EncounterRewardEntitlement.PremiumRepeat
                : EncounterRewardEntitlement.FallbackRepeat;
        }

        public void RecordDefeat(
            DateTime effectiveUtc,
            bool wasFirstClear,
            EncounterRewardEntitlement entitlement,
            in RepeatableEncounterRules rules)
        {
            effectiveUtc = RequireUtc(effectiveUtc, nameof(effectiveUtc));
            NextAvailableUtc = effectiveUtc + rules.Cooldown;
            if (wasFirstClear)
            {
                if (entitlement != EncounterRewardEntitlement.FirstClear)
                {
                    throw new ArgumentException("First clear must use first-clear reward entitlement.", nameof(entitlement));
                }
                return;
            }

            if (entitlement == EncounterRewardEntitlement.FirstClear)
            {
                throw new ArgumentException("Repeat defeat cannot use first-clear reward entitlement.", nameof(entitlement));
            }

            var expired = !RewardWindowStartedUtc.HasValue ||
                          effectiveUtc >= RewardWindowStartedUtc.Value + rules.RewardWindow;
            if (expired)
            {
                RewardWindowStartedUtc = effectiveUtc;
                RewardedKillsInWindow = 0;
            }

            if (entitlement == EncounterRewardEntitlement.PremiumRepeatFirstInWindow ||
                entitlement == EncounterRewardEntitlement.PremiumRepeat)
            {
                if (RewardedKillsInWindow >= rules.PremiumCap)
                {
                    throw new InvalidOperationException("Premium repeat cap is already exhausted.");
                }
                RewardedKillsInWindow++;
            }
        }

        private static DateTime? RequireOptionalUtc(DateTime? value, string name) =>
            value.HasValue ? RequireUtc(value.Value, name) : (DateTime?)null;

        private static DateTime RequireUtc(DateTime value, string name) =>
            value.Kind == DateTimeKind.Utc ? value : throw new ArgumentException("Timestamp must be UTC.", name);
    }

    public readonly struct RewardWindowSnapshot
    {
        public RewardWindowSnapshot(
            bool isAnchored,
            int rewardedKills,
            int premiumRewardsRemaining,
            TimeSpan remaining,
            bool firstPremiumEligible)
        {
            IsAnchored = isAnchored;
            RewardedKills = rewardedKills;
            PremiumRewardsRemaining = premiumRewardsRemaining;
            Remaining = remaining;
            IsFirstPremiumEligible = firstPremiumEligible;
        }

        public bool IsAnchored { get; }
        public int RewardedKills { get; }
        public int PremiumRewardsRemaining { get; }
        public TimeSpan Remaining { get; }
        public bool IsFirstPremiumEligible { get; }
    }

    public readonly struct RepeatableEncounterReadState
    {
        public RepeatableEncounterReadState(
            RepeatableEncounterKind kind,
            bool firstClearCompleted,
            bool available,
            TimeSpan cooldownRemaining,
            EncounterRewardEntitlement rewardEntitlement,
            int premiumCap,
            int premiumRewardsRemaining,
            TimeSpan rewardWindowRemaining,
            bool firstWindowBonusEligible)
        {
            Kind = kind;
            FirstClearCompleted = firstClearCompleted;
            Available = available;
            CooldownRemaining = cooldownRemaining;
            RewardEntitlement = rewardEntitlement;
            PremiumCap = premiumCap;
            PremiumRewardsRemaining = premiumRewardsRemaining;
            RewardWindowRemaining = rewardWindowRemaining;
            FirstWindowBonusEligible = firstWindowBonusEligible;
        }

        public RepeatableEncounterKind Kind { get; }
        public bool FirstClearCompleted { get; }
        public bool Available { get; }
        public TimeSpan CooldownRemaining { get; }
        public EncounterRewardEntitlement RewardEntitlement { get; }
        public int PremiumCap { get; }
        public int PremiumRewardsRemaining { get; }
        public TimeSpan RewardWindowRemaining { get; }
        public bool FirstWindowBonusEligible { get; }
        public bool PremiumCapped => FirstClearCompleted && PremiumRewardsRemaining == 0;
    }

    public sealed class Chapter1RepeatableState
    {
        public Chapter1RepeatableState(
            RepeatableEncounterState magnetar = null,
            RepeatableEncounterState custodian = null,
            PendingEncounterReward pendingReward = null)
        {
            Magnetar = magnetar ?? new RepeatableEncounterState();
            Custodian = custodian ?? new RepeatableEncounterState();
            PendingReward = pendingReward;
        }

        public RepeatableEncounterState Magnetar { get; }
        public RepeatableEncounterState Custodian { get; }
        public PendingEncounterReward PendingReward { get; private set; }

        public RepeatableEncounterState Get(RepeatableEncounterKind kind)
        {
            switch (kind)
            {
                case RepeatableEncounterKind.Magnetar: return Magnetar;
                case RepeatableEncounterKind.Custodian: return Custodian;
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        public void SetPending(PendingEncounterReward pending)
        {
            if (pending == null) throw new ArgumentNullException(nameof(pending));
            if (PendingReward != null) throw new InvalidOperationException("A reward transaction is already pending.");
            PendingReward = pending;
        }

        public void ClearPending(string transactionId)
        {
            if (PendingReward == null) return;
            if (!string.Equals(PendingReward.TransactionId, transactionId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Pending reward transaction id does not match.");
            }
            PendingReward = null;
        }
    }

    public sealed class PendingEncounterReward
    {
        public PendingEncounterReward(
            string transactionId,
            RepeatableEncounterKind encounterKind,
            EncounterRewardEntitlement entitlement,
            CoreReward reward,
            PendingRewardPhase phase = PendingRewardPhase.Prepared)
        {
            if (string.IsNullOrWhiteSpace(transactionId)) throw new ArgumentException("Transaction id is required.", nameof(transactionId));
            if (!Enum.IsDefined(typeof(RepeatableEncounterKind), encounterKind)) throw new ArgumentOutOfRangeException(nameof(encounterKind));
            if (!Enum.IsDefined(typeof(EncounterRewardEntitlement), entitlement)) throw new ArgumentOutOfRangeException(nameof(entitlement));
            if (!Enum.IsDefined(typeof(PendingRewardPhase), phase)) throw new ArgumentOutOfRangeException(nameof(phase));
            TransactionId = transactionId;
            EncounterKind = encounterKind;
            Entitlement = entitlement;
            Reward = reward;
            Phase = phase;
        }

        public string TransactionId { get; }
        public RepeatableEncounterKind EncounterKind { get; }
        public EncounterRewardEntitlement Entitlement { get; }
        public CoreReward Reward { get; }
        public PendingRewardPhase Phase { get; private set; }

        public void MarkApplied()
        {
            if (Phase != PendingRewardPhase.Prepared) return;
            Phase = PendingRewardPhase.Applied;
        }
    }

    public sealed class RepeatableEncounterService
    {
        private readonly Chapter1RepeatableState _state;
        private readonly ITimeProvider _time;

        public RepeatableEncounterService(Chapter1RepeatableState state, ITimeProvider time)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _time = time ?? throw new ArgumentNullException(nameof(time));
        }

        public RepeatableEncounterReadState Read(RepeatableEncounterKind kind, bool firstClearCompleted)
        {
            var now = _time.UtcNow;
            var rules = GetRules(kind);
            var state = _state.Get(kind);
            var window = state.GetRewardWindow(now, rules);
            return new RepeatableEncounterReadState(
                kind,
                firstClearCompleted,
                state.IsAvailable(now),
                state.GetCooldownRemaining(now),
                state.ClassifyReward(now, firstClearCompleted, rules),
                rules.PremiumCap,
                window.PremiumRewardsRemaining,
                window.Remaining,
                firstClearCompleted && window.IsFirstPremiumEligible);
        }

        public PendingEncounterReward PrepareDefeat(
            RepeatableEncounterKind kind,
            bool firstClearCompleted,
            CoreReward reward)
        {
            if (_state.PendingReward != null) throw new InvalidOperationException("Resolve the existing pending reward before preparing another defeat.");
            var now = _time.UtcNow;
            var rules = GetRules(kind);
            var state = _state.Get(kind);
            if (!state.IsAvailable(now)) throw new InvalidOperationException("Encounter is still on cooldown.");
            var entitlement = state.ClassifyReward(now, firstClearCompleted, rules);
            state.RecordDefeat(now, !firstClearCompleted, entitlement, rules);
            var pending = new PendingEncounterReward(
                Guid.NewGuid().ToString("N"),
                kind,
                entitlement,
                reward);
            _state.SetPending(pending);
            return pending;
        }

        public static RepeatableEncounterRules GetRules(RepeatableEncounterKind kind)
        {
            switch (kind)
            {
                case RepeatableEncounterKind.Magnetar: return RepeatableEncounterRules.Magnetar;
                case RepeatableEncounterKind.Custodian: return RepeatableEncounterRules.Custodian;
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
    }
}
