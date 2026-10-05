using System;
using System.Globalization;
using System.Collections.Generic;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Equipment;
using Gravivore.Gameplay.Offline;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Gameplay.Quests;
using Gravivore.Gameplay.World;
using Gravivore.Persistence.Quests;

namespace Gravivore.Persistence.Profile
{
    public sealed class ProfileRestoreContext
    {
        public ProfileRestoreContext(
            PlayerStatsConfiguration playerStats,
            ProgressionConfiguration progression,
            EquipmentCatalog equipment,
            QuestCatalog quests,
            string eliteGateId,
            string bossGateId,
            string eliteEnemyId,
            string bossId)
        {
            PlayerStats = playerStats;
            Progression = progression ?? throw new ArgumentNullException(nameof(progression));
            Equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
            Quests = quests ?? throw new ArgumentNullException(nameof(quests));
            EliteGateId = RequireId(eliteGateId, nameof(eliteGateId));
            BossGateId = RequireId(bossGateId, nameof(bossGateId));
            EliteEnemyId = RequireId(eliteEnemyId, nameof(eliteEnemyId));
            BossId = RequireId(bossId, nameof(bossId));
            EliteObjectiveId = FindEncounterObjectiveId(quests, QuestObjectiveType.EliteDefeated, EliteEnemyId);
            BossObjectiveId = FindEncounterObjectiveId(quests, QuestObjectiveType.BossDefeated, BossId);
        }

        public PlayerStatsConfiguration PlayerStats { get; }
        public ProgressionConfiguration Progression { get; }
        public EquipmentCatalog Equipment { get; }
        public QuestCatalog Quests { get; }
        public string EliteGateId { get; }
        public string BossGateId { get; }
        public string EliteEnemyId { get; }
        public string BossId { get; }
        public string EliteObjectiveId { get; }
        public string BossObjectiveId { get; }

        private static string RequireId(string value, string name) =>
            !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("A stable id is required.", name);

        private static string FindEncounterObjectiveId(QuestCatalog catalog, QuestObjectiveType type, string encounterId)
        {
            string objectiveId = null;
            for (var i = 0; i < catalog.ObjectiveCount; i++)
            {
                var objective = catalog.GetObjective(i);
                if (objective.Type != type || !string.Equals(objective.EncounterId, encounterId, StringComparison.Ordinal)) continue;
                if (objectiveId != null)
                {
                    throw new ArgumentException($"Quest catalog contains multiple {type} objectives for encounter {encounterId}.", nameof(catalog));
                }
                objectiveId = objective.Id;
            }

            return objectiveId ?? throw new ArgumentException($"Quest catalog requires a {type} objective for encounter {encounterId}.", nameof(catalog));
        }
    }

    public sealed class ProfileRuntimeState
    {
        public ProfileRuntimeState(
            Guid profileId,
            DateTime createdUtc,
            DateTime lastSeenUtc,
            PlayerStatsState playerStats,
            ProgressionState progression,
            InventoryState inventory,
            QuestState quests,
            WorldUnlockState world,
            BossCompletionState boss,
            OfflineRewardState offline,
            Chapter1RepeatableState repeatable,
            DateTime effectiveUtcFloor)
        {
            if (profileId == Guid.Empty) throw new ArgumentException("Profile id is required.", nameof(profileId));
            ProfileId = profileId;
            CreatedUtc = RequireUtc(createdUtc, nameof(createdUtc));
            LastSeenUtc = RequireUtc(lastSeenUtc, nameof(lastSeenUtc));
            PlayerStats = playerStats ?? throw new ArgumentNullException(nameof(playerStats));
            Progression = progression ?? throw new ArgumentNullException(nameof(progression));
            Inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            Quests = quests ?? throw new ArgumentNullException(nameof(quests));
            World = world ?? throw new ArgumentNullException(nameof(world));
            Boss = boss ?? throw new ArgumentNullException(nameof(boss));
            Offline = offline ?? throw new ArgumentNullException(nameof(offline));
            Repeatable = repeatable ?? throw new ArgumentNullException(nameof(repeatable));
            EffectiveUtcFloor = RequireUtc(effectiveUtcFloor, nameof(effectiveUtcFloor));
        }

        public Guid ProfileId { get; }
        public DateTime CreatedUtc { get; }
        public DateTime LastSeenUtc { get; private set; }
        public PlayerStatsState PlayerStats { get; }
        public ProgressionState Progression { get; }
        public InventoryState Inventory { get; }
        public QuestState Quests { get; }
        public WorldUnlockState World { get; }
        public BossCompletionState Boss { get; }
        public OfflineRewardState Offline { get; }
        public Chapter1RepeatableState Repeatable { get; }
        public DateTime EffectiveUtcFloor { get; private set; }

        public void SetLastSeenUtc(DateTime value) => LastSeenUtc = RequireUtc(value, nameof(value));

        public DateTime AdvanceEffectiveUtcFloor(DateTime observedUtc)
        {
            observedUtc = RequireUtc(observedUtc, nameof(observedUtc));
            if (observedUtc > EffectiveUtcFloor) EffectiveUtcFloor = observedUtc;
            return EffectiveUtcFloor;
        }

        private static DateTime RequireUtc(DateTime value, string name) =>
            value.Kind == DateTimeKind.Utc ? value : throw new ArgumentException("Timestamp must be UTC.", name);
    }

    public static class ProfileSaveMapper
    {
        public static ProfileRuntimeState CreateFresh(ProfileRestoreContext context, DateTime nowUtc, Guid? profileId = null)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (nowUtc.Kind != DateTimeKind.Utc) throw new ArgumentException("Timestamp must be UTC.", nameof(nowUtc));
            var world = new WorldUnlockState(context.EliteGateId, context.BossGateId, context.EliteEnemyId);
            return new ProfileRuntimeState(
                profileId ?? Guid.NewGuid(),
                nowUtc,
                nowUtc,
                new PlayerStatsState(context.PlayerStats, context.PlayerStats.StartingLevels),
                new ProgressionState(),
                new InventoryState(),
                new QuestState(context.Quests),
                world,
                new BossCompletionState(context.BossId),
                new OfflineRewardState(),
                new Chapter1RepeatableState(),
                nowUtc);
        }

        public static ProfileRuntimeState Restore(SaveRootDto dto, ProfileRestoreContext context, Action<string> optionalContentWarning = null)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (dto.schemaVersion != SaveSchema.CurrentVersion) throw new ArgumentException("Save schema is not current.", nameof(dto));
            if (!Guid.TryParse(dto.profileId, out var profileId) || profileId == Guid.Empty) throw new ArgumentException("Profile id must be a non-empty GUID.", nameof(dto));

            var createdUtc = ParseUtc(dto.createdUtc, nameof(dto.createdUtc));
            var lastSeenUtc = ParseUtc(dto.lastSeenUtc, nameof(dto.lastSeenUtc));
            var player = RestorePlayer(dto.player, context, optionalContentWarning, out var progression);
            var inventory = InventorySaveMapper.Restore(dto.inventory ?? throw new ArgumentException("Inventory section is required.", nameof(dto)), context.Equipment, optionalContentWarning);
            var quests = QuestSaveMapper.Restore(context.Quests, dto.quest ?? throw new ArgumentException("Quest section is required.", nameof(dto)), optionalContentWarning);
            var worldDto = dto.world ?? throw new ArgumentException("World section is required.", nameof(dto));
            var world = WorldUnlockState.Restore(
                context.EliteGateId,
                context.BossGateId,
                context.EliteEnemyId,
                new WorldUnlockSnapshot(worldDto.eliteGateUnlocked, worldDto.eliteDefeated, worldDto.bossGateUnlocked));
            var bossDto = dto.boss ?? throw new ArgumentException("Boss section is required.", nameof(dto));
            if (!string.Equals(bossDto.bossId, context.BossId, StringComparison.Ordinal)) throw new ArgumentException("Boss save id does not match the configured boss.", nameof(dto));
            if (bossDto.defeated && !world.BossGateUnlocked) throw new ArgumentException("A defeated boss requires an unlocked boss gate.", nameof(dto));

            var boss = BossCompletionState.Restore(context.BossId, new BossCompletionSnapshot(bossDto.defeated));
            ValidateEncounterQuestConsistency(quests, world, boss, context);
            var offlineDto = dto.offline ?? throw new ArgumentException("Offline section is required.", nameof(dto));
            var offline = new OfflineRewardState(offlineDto.materialBalance, offlineDto.pendingReward);
            var repeatable = RepeatableSaveMapper.Restore(dto.repeatable, out var effectiveUtcFloor);
            return new ProfileRuntimeState(
                profileId,
                createdUtc,
                lastSeenUtc,
                player,
                progression,
                inventory,
                quests,
                world,
                boss,
                offline,
                repeatable,
                effectiveUtcFloor);
        }

        public static SaveRootDto ToDto(ProfileRuntimeState state, ProfileRestoreContext context)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (context == null) throw new ArgumentNullException(nameof(context));
            var levels = state.PlayerStats.BaseLevels;
            var progression = state.Progression.ExportSnapshot();
            var world = state.World.ExportSnapshot();
            var boss = state.Boss.ExportSnapshot();
            var firstKills = new string[progression.FirstKillEnemyIds.Count];
            var firstKillIndex = 0;
            foreach (var enemyId in progression.FirstKillEnemyIds) firstKills[firstKillIndex++] = enemyId;
            return new SaveRootDto
            {
                schemaVersion = SaveSchema.CurrentVersion,
                profileId = state.ProfileId.ToString("D"),
                createdUtc = FormatUtc(state.CreatedUtc),
                lastSeenUtc = FormatUtc(state.LastSeenUtc),
                player = new PlayerProgressionSaveDto
                {
                    powerLevel = levels.Power,
                    hullLevel = levels.Hull,
                    armorLevel = levels.Armor,
                    fluxLevel = levels.Flux,
                    mobilityLevel = levels.Mobility,
                    powerExperience = progression.PowerExperience,
                    hullExperience = progression.HullExperience,
                    armorExperience = progression.ArmorExperience,
                    fluxExperience = progression.FluxExperience,
                    mobilityExperience = progression.MobilityExperience,
                    totalAssimilationScore = progression.TotalAssimilationScore,
                    firstKillEnemyIds = firstKills
                },
                inventory = InventorySaveMapper.Export(state.Inventory),
                quest = QuestSaveMapper.ToDto(state.Quests, context.Quests),
                world = new WorldSaveDto
                {
                    eliteGateUnlocked = world.EliteGateUnlocked,
                    eliteDefeated = world.EliteDefeated,
                    bossGateUnlocked = world.BossGateUnlocked
                },
                boss = new BossCompletionSaveDto { bossId = context.BossId, defeated = boss.Defeated },
                offline = new OfflineSaveDto { materialBalance = state.Offline.MaterialBalance, pendingReward = state.Offline.PendingReward },
                repeatable = RepeatableSaveMapper.ToDto(state.Repeatable, state.EffectiveUtcFloor)
            };
        }

        private static PlayerStatsState RestorePlayer(PlayerProgressionSaveDto dto, ProfileRestoreContext context, Action<string> optionalContentWarning, out ProgressionState progression)
        {
            if (dto == null) throw new ArgumentException("Player section is required.", nameof(dto));
            var levels = new PlayerStatLevels(dto.powerLevel, dto.hullLevel, dto.armorLevel, dto.fluxLevel, dto.mobilityLevel);
            var firstKills = dto.firstKillEnemyIds ?? throw new ArgumentException("First-kill ids are required.", nameof(dto));
            var knownFirstKills = new List<string>(firstKills.Length);
            var uniqueFirstKills = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < firstKills.Length; i++)
            {
                var enemyId = firstKills[i];
                if (string.IsNullOrWhiteSpace(enemyId) || !uniqueFirstKills.Add(enemyId)) throw new ArgumentException("First-kill ids must be non-empty and unique.", nameof(dto));
                if (context.Progression.TryGetReward(enemyId, out _)) knownFirstKills.Add(enemyId);
                else optionalContentWarning?.Invoke($"Dropped removed first-kill progression reference '{enemyId}'.");
            }

            var powerExperience = NormalizeExperience(ref levels, PlayerStatType.Power, dto.powerExperience, context, optionalContentWarning);
            var hullExperience = NormalizeExperience(ref levels, PlayerStatType.Hull, dto.hullExperience, context, optionalContentWarning);
            var armorExperience = NormalizeExperience(ref levels, PlayerStatType.Armor, dto.armorExperience, context, optionalContentWarning);
            var fluxExperience = NormalizeExperience(ref levels, PlayerStatType.Flux, dto.fluxExperience, context, optionalContentWarning);
            var mobilityExperience = NormalizeExperience(ref levels, PlayerStatType.Mobility, dto.mobilityExperience, context, optionalContentWarning);

            context.PlayerStats.ValidateLevels(levels);
            var snapshot = new ProgressionSnapshot(
                powerExperience,
                hullExperience,
                armorExperience,
                fluxExperience,
                mobilityExperience,
                dto.totalAssimilationScore,
                knownFirstKills);
            progression = ProgressionState.Restore(snapshot);
            return new PlayerStatsState(context.PlayerStats, levels);
        }

        private static float NormalizeExperience(ref PlayerStatLevels levels, PlayerStatType stat, float savedExperience, ProfileRestoreContext context, Action<string> optionalContentWarning)
        {
            if (float.IsNaN(savedExperience) || float.IsInfinity(savedExperience) || savedExperience < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(savedExperience), $"Invalid experience for {stat}.");
            }

            var savedLevel = levels.GetLevel(stat);
            var maximumLevel = context.PlayerStats.GetCurve(stat).MaximumLevel;
            var normalizedLevel = Math.Min(savedLevel, maximumLevel);
            var normalizedExperience = savedExperience;

            if (normalizedLevel != savedLevel) optionalContentWarning?.Invoke($"Clamped {stat} level from {savedLevel} to current maximum {maximumLevel}.");
            if (normalizedLevel >= maximumLevel)
            {
                if (normalizedExperience > 0f) optionalContentWarning?.Invoke($"Cleared residual {stat} experience at current maximum level {maximumLevel}.");
                levels = levels.WithLevel(stat, maximumLevel);
                return 0f;
            }

            while (normalizedLevel < maximumLevel)
            {
                var required = context.Progression.ThresholdCurve.Evaluate(normalizedLevel);
                if (normalizedExperience < required) break;
                normalizedExperience -= required;
                normalizedLevel++;
            }

            if (normalizedLevel >= maximumLevel) normalizedExperience = 0f;
            if (normalizedLevel != savedLevel) optionalContentWarning?.Invoke($"Normalized {stat} progression from level {savedLevel} to level {normalizedLevel} using current thresholds.");
            levels = levels.WithLevel(stat, normalizedLevel);
            return normalizedExperience;
        }

        private static void ValidateEncounterQuestConsistency(QuestState quests, WorldUnlockState world, BossCompletionState boss, ProfileRestoreContext context)
        {
            if (quests.IsObjectiveCompleted(context.EliteObjectiveId) != world.EliteDefeated)
            {
                throw new ArgumentException("Elite quest completion must match the authoritative elite defeat state.", nameof(quests));
            }
            if (quests.IsObjectiveCompleted(context.BossObjectiveId) != boss.IsDefeated)
            {
                throw new ArgumentException("Boss quest completion must match the authoritative boss completion state.", nameof(quests));
            }
        }

        private static DateTime ParseUtc(string value, string name)
        {
            if (!DateTime.TryParseExact(value, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result) || result.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Timestamp must be round-trip UTC.", name);
            }
            return result;
        }

        private static string FormatUtc(DateTime value) => value.ToString("O", CultureInfo.InvariantCulture);
    }
}
