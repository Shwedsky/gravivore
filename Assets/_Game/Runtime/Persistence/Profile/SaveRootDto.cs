using System;
using Gravivore.Persistence.Quests;

namespace Gravivore.Persistence.Profile
{
    public static class SaveSchema
    {
        public const int CurrentVersion = 3;
    }

    [Serializable]
    public sealed class SaveRootDto
    {
        public int schemaVersion;
        public string profileId;
        public string createdUtc;
        public string lastSeenUtc;
        public PlayerProgressionSaveDto player;
        public WorldSaveDto world;
        public BossCompletionSaveDto boss;
        public QuestSaveDto quest;
        public InventorySaveDto inventory;
        public OfflineSaveDto offline;
        public Chapter1RepeatableSaveDto repeatable;
    }

    [Serializable]
    public sealed class PlayerProgressionSaveDto
    {
        public int powerLevel;
        public int hullLevel;
        public int armorLevel;
        public int fluxLevel;
        public int mobilityLevel;
        public float powerExperience;
        public float hullExperience;
        public float armorExperience;
        public float fluxExperience;
        public float mobilityExperience;
        public long totalAssimilationScore;
        public string[] firstKillEnemyIds = Array.Empty<string>();
    }

    [Serializable]
    public sealed class WorldSaveDto
    {
        public bool eliteGateUnlocked;
        public bool eliteDefeated;
        public bool bossGateUnlocked;
    }

    [Serializable]
    public sealed class BossCompletionSaveDto
    {
        public string bossId;
        public bool defeated;
    }

    [Serializable]
    public sealed class OfflineSaveDto
    {
        public long materialBalance;
        public long pendingReward;
    }

    [Serializable]
    public sealed class Chapter1RepeatableSaveDto
    {
        public string effectiveUtcFloor;
        public RepeatableEncounterSaveDto magnetar;
        public RepeatableEncounterSaveDto custodian;
        public PendingEncounterRewardSaveDto pendingReward;
    }

    [Serializable]
    public sealed class RepeatableEncounterSaveDto
    {
        public string nextAvailableUtc;
        public string rewardWindowStartedUtc;
        public int rewardedKillsInWindow;
    }

    [Serializable]
    public sealed class PendingEncounterRewardSaveDto
    {
        public string transactionId;
        public int encounterKind;
        public int entitlement;
        public string enemyId;
        public int stat;
        public float statExperience;
        public long assimilationScore;
        public int phase;
    }

    [Serializable]
    public sealed class LegacySaveRootV0Dto
    {
        public int schemaVersion;
        public string profileId;
        public string createdUtc;
        public string lastSeenUtc;
        public PlayerProgressionSaveDto player;
        public WorldSaveDto world;
        public BossCompletionSaveDto boss;
        public QuestSaveDto quest;
        public InventorySaveDto inventory;
    }

    [Serializable]
    public sealed class LegacySaveRootV1Dto
    {
        public int schemaVersion;
        public string profileId;
        public string createdUtc;
        public string lastSeenUtc;
        public PlayerProgressionSaveDto player;
        public WorldSaveDto world;
        public BossCompletionSaveDto boss;
        public QuestSaveDto quest;
        public InventorySaveDto inventory;
        public OfflineSaveDto offline;
    }
}
