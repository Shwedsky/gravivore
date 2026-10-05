using System;
using System.Globalization;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;

namespace Gravivore.Persistence.Profile
{
    public static class RepeatableSaveMapper
    {
        public static Chapter1RepeatableState Restore(Chapter1RepeatableSaveDto dto, out DateTime effectiveUtcFloor)
        {
            if (dto == null) throw new ArgumentException("Repeatable Chapter 1 section is required.", nameof(dto));
            effectiveUtcFloor = ParseRequiredUtc(dto.effectiveUtcFloor, nameof(dto.effectiveUtcFloor));
            var magnetar = RestoreEncounter(dto.magnetar, RepeatableEncounterKind.Magnetar);
            var custodian = RestoreEncounter(dto.custodian, RepeatableEncounterKind.Custodian);
            var pending = RestorePending(dto.pendingReward);
            return new Chapter1RepeatableState(magnetar, custodian, pending);
        }

        public static Chapter1RepeatableSaveDto ToDto(Chapter1RepeatableState state, DateTime effectiveUtcFloor)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            RequireUtc(effectiveUtcFloor, nameof(effectiveUtcFloor));
            return new Chapter1RepeatableSaveDto
            {
                effectiveUtcFloor = FormatUtc(effectiveUtcFloor),
                magnetar = ToDto(state.Magnetar),
                custodian = ToDto(state.Custodian),
                pendingReward = ToDto(state.PendingReward)
            };
        }

        private static RepeatableEncounterState RestoreEncounter(RepeatableEncounterSaveDto dto, RepeatableEncounterKind kind)
        {
            if (dto == null) throw new ArgumentException($"Repeatable {kind} state is required.", nameof(dto));
            var rules = RepeatableEncounterService.GetRules(kind);
            if (dto.rewardedKillsInWindow < 0 || dto.rewardedKillsInWindow > rules.PremiumCap)
                throw new ArgumentOutOfRangeException(nameof(dto), $"Repeatable {kind} premium count is invalid.");
            var nextAvailable = ParseOptionalUtc(dto.nextAvailableUtc, nameof(dto.nextAvailableUtc));
            var windowStarted = ParseOptionalUtc(dto.rewardWindowStartedUtc, nameof(dto.rewardWindowStartedUtc));
            if (!windowStarted.HasValue && dto.rewardedKillsInWindow != 0)
                throw new ArgumentException($"Repeatable {kind} premium count requires a reward window.", nameof(dto));
            return new RepeatableEncounterState(nextAvailable, windowStarted, dto.rewardedKillsInWindow);
        }

        private static PendingEncounterReward RestorePending(PendingEncounterRewardSaveDto dto)
        {
            if (dto == null) return null;
            if (string.IsNullOrWhiteSpace(dto.transactionId)) throw new ArgumentException("Pending reward transaction id is required.", nameof(dto));
            if (!Enum.IsDefined(typeof(RepeatableEncounterKind), dto.encounterKind) ||
                !Enum.IsDefined(typeof(EncounterRewardEntitlement), dto.entitlement) ||
                !Enum.IsDefined(typeof(PlayerStatType), dto.stat) ||
                !Enum.IsDefined(typeof(PendingRewardPhase), dto.phase))
                throw new ArgumentException("Pending reward transaction contains an unknown enum identifier.", nameof(dto));
            var reward = new CoreReward(dto.enemyId, (PlayerStatType)dto.stat, dto.statExperience, dto.assimilationScore);
            return new PendingEncounterReward(dto.transactionId, (RepeatableEncounterKind)dto.encounterKind,
                (EncounterRewardEntitlement)dto.entitlement, reward, (PendingRewardPhase)dto.phase);
        }

        private static RepeatableEncounterSaveDto ToDto(RepeatableEncounterState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            return new RepeatableEncounterSaveDto
            {
                nextAvailableUtc = state.NextAvailableUtc.HasValue ? FormatUtc(state.NextAvailableUtc.Value) : null,
                rewardWindowStartedUtc = state.RewardWindowStartedUtc.HasValue ? FormatUtc(state.RewardWindowStartedUtc.Value) : null,
                rewardedKillsInWindow = state.RewardedKillsInWindow
            };
        }

        private static PendingEncounterRewardSaveDto ToDto(PendingEncounterReward pending)
        {
            if (pending == null) return null;
            return new PendingEncounterRewardSaveDto
            {
                transactionId = pending.TransactionId,
                encounterKind = (int)pending.EncounterKind,
                entitlement = (int)pending.Entitlement,
                enemyId = pending.Reward.EnemyId,
                stat = (int)pending.Reward.Stat,
                statExperience = pending.Reward.StatExperience,
                assimilationScore = pending.Reward.AssimilationScore,
                phase = (int)pending.Phase
            };
        }

        private static DateTime ParseRequiredUtc(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Timestamp is required.", name);
            return ParseUtc(value, name);
        }

        private static DateTime? ParseOptionalUtc(string value, string name) => string.IsNullOrWhiteSpace(value) ? (DateTime?)null : ParseUtc(value, name);
        private static DateTime ParseUtc(string value, string name)
        {
            if (!DateTime.TryParseExact(value, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result) || result.Kind != DateTimeKind.Utc)
                throw new ArgumentException("Timestamp must be round-trip UTC.", name);
            return result;
        }
        private static void RequireUtc(DateTime value, string name)
        {
            if (value.Kind != DateTimeKind.Utc) throw new ArgumentException("Timestamp must be UTC.", name);
        }
        private static string FormatUtc(DateTime value) => value.ToString("O", CultureInfo.InvariantCulture);
    }
}
