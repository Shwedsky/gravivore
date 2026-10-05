using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gravivore.Persistence.Profile
{
    public interface ISaveSerializer
    {
        string Serialize<T>(T value);
        T Deserialize<T>(string json);
    }

    public sealed class UnityJsonSaveSerializer : ISaveSerializer
    {
        public string Serialize<T>(T value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return JsonUtility.ToJson(value, true);
        }

        public T Deserialize<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("JSON is required.", nameof(json));
            var value = JsonUtility.FromJson<T>(json);
            return value != null ? value : throw new ArgumentException("JSON produced a null save object.", nameof(json));
        }
    }

    public interface ISaveMigration
    {
        int FromVersion { get; }
        int ToVersion { get; }
        SaveRootDto Migrate(string sourceJson, ISaveSerializer serializer, SaveRootDto defaults);
    }

    [Serializable]
    internal sealed class SaveVersionHeaderDto
    {
        public int schemaVersion;
    }

    public readonly struct SaveMigrationResult
    {
        public SaveMigrationResult(SaveRootDto save, bool wasMigrated)
        {
            Save = save ?? throw new ArgumentNullException(nameof(save));
            WasMigrated = wasMigrated;
        }

        public SaveRootDto Save { get; }
        public bool WasMigrated { get; }
    }

    public sealed class SaveMigrationPipeline
    {
        private readonly int _currentVersion;
        private readonly ISaveSerializer _serializer;
        private readonly Dictionary<int, ISaveMigration> _migrations;

        public SaveMigrationPipeline(
            int currentVersion,
            ISaveSerializer serializer,
            IReadOnlyList<ISaveMigration> migrations)
        {
            if (currentVersion < 1) throw new ArgumentOutOfRangeException(nameof(currentVersion));
            _currentVersion = currentVersion;
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _migrations = new Dictionary<int, ISaveMigration>();
            if (migrations == null) throw new ArgumentNullException(nameof(migrations));
            for (var i = 0; i < migrations.Count; i++)
            {
                var migration = migrations[i] ?? throw new ArgumentException("Migration entries cannot be null.", nameof(migrations));
                if (migration.FromVersion < 0 || migration.ToVersion != migration.FromVersion + 1 ||
                    !_migrations.TryAdd(migration.FromVersion, migration))
                {
                    throw new ArgumentException("Migrations must be unique sequential steps.", nameof(migrations));
                }
            }
        }

        public SaveMigrationResult MigrateToCurrent(string json, SaveRootDto defaults)
        {
            if (defaults == null) throw new ArgumentNullException(nameof(defaults));
            var header = _serializer.Deserialize<SaveVersionHeaderDto>(json);
            if (header.schemaVersion < 0) throw new ArgumentException("Negative save schema is invalid.", nameof(json));
            if (header.schemaVersion > _currentVersion) throw new NotSupportedException("Save schema is newer than this build.");

            var version = header.schemaVersion;
            var currentJson = json;
            var migrated = false;
            while (version < _currentVersion)
            {
                if (!_migrations.TryGetValue(version, out var migration))
                {
                    throw new NotSupportedException($"Missing save migration step from schema {version}.");
                }

                var next = migration.Migrate(currentJson, _serializer, defaults);
                if (next == null || next.schemaVersion != migration.ToVersion)
                {
                    throw new InvalidOperationException($"Migration {migration.FromVersion}->{migration.ToVersion} returned an invalid schema.");
                }

                currentJson = _serializer.Serialize(next);
                version = next.schemaVersion;
                migrated = true;
            }

            var save = _serializer.Deserialize<SaveRootDto>(currentJson);
            if (save.schemaVersion != _currentVersion)
            {
                throw new InvalidOperationException("Migration pipeline did not produce the current schema.");
            }

            return new SaveMigrationResult(save, migrated);
        }
    }

    public sealed class SaveMigrationV0ToV1 : ISaveMigration
    {
        public int FromVersion => 0;
        public int ToVersion => 1;

        public SaveRootDto Migrate(string sourceJson, ISaveSerializer serializer, SaveRootDto defaults)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            if (defaults == null) throw new ArgumentNullException(nameof(defaults));
            var legacy = serializer.Deserialize<LegacySaveRootV0Dto>(sourceJson);
            return new SaveRootDto
            {
                schemaVersion = ToVersion,
                profileId = legacy.profileId,
                createdUtc = legacy.createdUtc,
                lastSeenUtc = legacy.lastSeenUtc,
                player = legacy.player,
                world = legacy.world,
                boss = legacy.boss,
                quest = legacy.quest,
                inventory = legacy.inventory,
                offline = defaults.offline
            };
        }
    }

    public sealed class SaveMigrationV1ToV2 : ISaveMigration
    {
        public int FromVersion => 1;
        public int ToVersion => 2;

        public SaveRootDto Migrate(string sourceJson, ISaveSerializer serializer, SaveRootDto defaults)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            if (defaults == null) throw new ArgumentNullException(nameof(defaults));
            var legacy = serializer.Deserialize<LegacySaveRootV1Dto>(sourceJson);
            var defaultRepeatable = defaults.repeatable ?? CreateFreshRepeatable(legacy.lastSeenUtc);

            return new SaveRootDto
            {
                schemaVersion = ToVersion,
                profileId = legacy.profileId,
                createdUtc = legacy.createdUtc,
                lastSeenUtc = legacy.lastSeenUtc,
                player = legacy.player,
                world = legacy.world,
                boss = legacy.boss,
                quest = legacy.quest,
                inventory = legacy.inventory,
                offline = legacy.offline,
                repeatable = new Chapter1RepeatableSaveDto
                {
                    // v1 had no encounter kill timestamps. A historical first clear therefore
                    // migrates with no cooldown and is immediately repeatable by contract.
                    effectiveUtcFloor = legacy.lastSeenUtc,
                    magnetar = CloneEncounter(defaultRepeatable.magnetar),
                    custodian = CloneEncounter(defaultRepeatable.custodian),
                    pendingReward = null
                }
            };
        }

        private static Chapter1RepeatableSaveDto CreateFreshRepeatable(string floorUtc)
        {
            return new Chapter1RepeatableSaveDto
            {
                effectiveUtcFloor = floorUtc,
                magnetar = new RepeatableEncounterSaveDto(),
                custodian = new RepeatableEncounterSaveDto(),
                pendingReward = null
            };
        }

        private static RepeatableEncounterSaveDto CloneEncounter(RepeatableEncounterSaveDto source)
        {
            return source == null
                ? new RepeatableEncounterSaveDto()
                : new RepeatableEncounterSaveDto
                {
                    nextAvailableUtc = source.nextAvailableUtc,
                    rewardWindowStartedUtc = source.rewardWindowStartedUtc,
                    rewardedKillsInWindow = source.rewardedKillsInWindow
                };
        }
    }
}
