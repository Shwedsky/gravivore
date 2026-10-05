using System;
using Gravivore.Core.Time;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Persistence.Profile;
using NUnit.Framework;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class Chapter1RepeatableRuntimeV2Tests
    {
        private static readonly DateTime T0 = new DateTime(2026, 10, 5, 9, 0, 0, DateTimeKind.Utc);

        [Test]
        public void Migration_V1ToV2_PreservesHistoricalFirstClearsAndMakesRepeatsImmediatelyAvailable()
        {
            var serializer = new UnityJsonSaveSerializer();
            var legacy = new LegacySaveRootV1Dto
            {
                schemaVersion = 1,
                profileId = Guid.NewGuid().ToString("D"),
                createdUtc = T0.AddDays(-3).ToString("O"),
                lastSeenUtc = T0.ToString("O"),
                world = new WorldSaveDto { eliteGateUnlocked = true, eliteDefeated = true, bossGateUnlocked = true },
                boss = new BossCompletionSaveDto { bossId = "custodian", defeated = true },
                offline = new OfflineSaveDto()
            };
            var defaults = new SaveRootDto
            {
                schemaVersion = SaveSchema.CurrentVersion,
                repeatable = new Chapter1RepeatableSaveDto
                {
                    effectiveUtcFloor = T0.ToString("O"),
                    magnetar = new RepeatableEncounterSaveDto(),
                    custodian = new RepeatableEncounterSaveDto()
                }
            };
            var migration = new SaveMigrationV1ToV2();
            var migrated = migration.Migrate(serializer.Serialize(legacy), serializer, defaults);

            Assert.That(migrated.schemaVersion, Is.EqualTo(2));
            Assert.That(migrated.world.eliteDefeated, Is.True);
            Assert.That(migrated.boss.defeated, Is.True);
            Assert.That(migrated.repeatable.magnetar.nextAvailableUtc, Is.Null.Or.Empty);
            Assert.That(migrated.repeatable.custodian.nextAvailableUtc, Is.Null.Or.Empty);
            Assert.That(migrated.repeatable.effectiveUtcFloor, Is.EqualTo(legacy.lastSeenUtc));
        }

        [Test]
        public void MigrationPipeline_AutoRegistersSequentialV1ToV2Step()
        {
            var serializer = new UnityJsonSaveSerializer();
            var legacy = new LegacySaveRootV1Dto
            {
                schemaVersion = 1,
                profileId = Guid.NewGuid().ToString("D"),
                createdUtc = T0.ToString("O"),
                lastSeenUtc = T0.ToString("O"),
                offline = new OfflineSaveDto()
            };
            var defaults = new SaveRootDto
            {
                schemaVersion = 2,
                repeatable = new Chapter1RepeatableSaveDto
                {
                    effectiveUtcFloor = T0.ToString("O"),
                    magnetar = new RepeatableEncounterSaveDto(),
                    custodian = new RepeatableEncounterSaveDto()
                }
            };
            var pipeline = new SaveMigrationPipeline(2, serializer, Array.Empty<ISaveMigration>());
            var result = pipeline.MigrateToCurrent(serializer.Serialize(legacy), defaults);
            Assert.That(result.WasMigrated, Is.True);
            Assert.That(result.Save.schemaVersion, Is.EqualTo(2));
        }

        [Test]
        public void RepeatableSave_CorruptPremiumCountIsRejected()
        {
            var dto = new Chapter1RepeatableSaveDto
            {
                effectiveUtcFloor = T0.ToString("O"),
                magnetar = new RepeatableEncounterSaveDto
                {
                    rewardWindowStartedUtc = T0.ToString("O"),
                    rewardedKillsInWindow = 4
                },
                custodian = new RepeatableEncounterSaveDto()
            };
            Assert.Throws<ArgumentOutOfRangeException>(() => RepeatableSaveMapper.Restore(dto, out _));
        }

        [Test]
        public void EffectiveUtcFloor_BackwardClockCannotMoveGameplayTimeBackward()
        {
            var floor = T0.AddMinutes(10);
            Assert.That(ProfileEffectiveUtcTimeProvider.ClampToPersistedFloor(floor, T0), Is.EqualTo(floor));
            Assert.That(ProfileEffectiveUtcTimeProvider.ClampToPersistedFloor(floor, T0.AddMinutes(20)), Is.EqualTo(T0.AddMinutes(20)));
        }

        [Test]
        public void Magnetar_FirstClearDoesNotConsumePremium_ThenCapFallsBack()
        {
            var rules = RepeatableEncounterRules.Magnetar;
            var state = new RepeatableEncounterState();
            state.RecordDefeat(T0, true, EncounterRewardEntitlement.FirstClear, rules);
            Assert.That(state.RewardedKillsInWindow, Is.Zero);
            Assert.That(state.RewardWindowStartedUtc, Is.Null);
            Assert.That(state.IsAvailable(T0.AddMinutes(14).AddSeconds(59)), Is.False);
            Assert.That(state.IsAvailable(T0.AddMinutes(15)), Is.True);

            var t = T0.AddMinutes(15);
            for (var i = 0; i < 3; i++)
            {
                var entitlement = state.ClassifyReward(t, true, rules);
                Assert.That(entitlement, Is.EqualTo(i == 0
                    ? EncounterRewardEntitlement.PremiumRepeatFirstInWindow
                    : EncounterRewardEntitlement.PremiumRepeat));
                state.RecordDefeat(t, false, entitlement, rules);
                t = state.NextAvailableUtc.Value;
            }
            Assert.That(state.ClassifyReward(t, true, rules), Is.EqualTo(EncounterRewardEntitlement.FallbackRepeat));
            state.RecordDefeat(t, false, EncounterRewardEntitlement.FallbackRepeat, rules);
            Assert.That(state.RewardedKillsInWindow, Is.EqualTo(3));
        }

        [Test]
        public void Custodian_FirstClearThenRepeatsNeverReclassifyAsFirstClear_AndCapIsTwo()
        {
            var rules = RepeatableEncounterRules.Custodian;
            var state = new RepeatableEncounterState();
            Assert.That(state.ClassifyReward(T0, false, rules), Is.EqualTo(EncounterRewardEntitlement.FirstClear));
            state.RecordDefeat(T0, true, EncounterRewardEntitlement.FirstClear, rules);
            var t = T0.AddMinutes(30);
            for (var i = 0; i < 2; i++)
            {
                var entitlement = state.ClassifyReward(t, true, rules);
                Assert.That(entitlement, Is.Not.EqualTo(EncounterRewardEntitlement.FirstClear));
                state.RecordDefeat(t, false, entitlement, rules);
                t = state.NextAvailableUtc.Value;
            }
            Assert.That(state.ClassifyReward(t, true, rules), Is.EqualTo(EncounterRewardEntitlement.FallbackRepeat));
        }

        [Test]
        public void RewardWindow_RollsAtAnchoredTwentyFourHours()
        {
            var rules = RepeatableEncounterRules.Magnetar;
            var state = new RepeatableEncounterState();
            state.RecordDefeat(T0, false, EncounterRewardEntitlement.PremiumRepeatFirstInWindow, rules);
            Assert.That(state.GetRewardWindow(T0.AddHours(23).AddMinutes(59), rules).RewardedKills, Is.EqualTo(1));
            var rolled = state.GetRewardWindow(T0.AddHours(24), rules);
            Assert.That(rolled.IsAnchored, Is.False);
            Assert.That(state.ClassifyReward(T0.AddHours(24), true, rules), Is.EqualTo(EncounterRewardEntitlement.PremiumRepeatFirstInWindow));
        }

        [Test]
        public void RepeatableSave_RoundTripPreservesCooldownWindowAndPendingTransaction()
        {
            var magnetar = new RepeatableEncounterState(T0.AddMinutes(15), T0, 2);
            var pending = new PendingEncounterReward(
                "tx-1",
                RepeatableEncounterKind.Magnetar,
                EncounterRewardEntitlement.PremiumRepeat,
                new CoreReward("magnetar", PlayerStatType.Power, 10f, 20),
                PendingRewardPhase.Applied);
            var original = new Chapter1RepeatableState(magnetar, new RepeatableEncounterState(), pending);
            var dto = RepeatableSaveMapper.ToDto(original, T0.AddMinutes(3));
            var restored = RepeatableSaveMapper.Restore(dto, out var floor);
            Assert.That(restored.Magnetar.NextAvailableUtc, Is.EqualTo(T0.AddMinutes(15)));
            Assert.That(restored.Magnetar.RewardedKillsInWindow, Is.EqualTo(2));
            Assert.That(restored.PendingReward.TransactionId, Is.EqualTo("tx-1"));
            Assert.That(restored.PendingReward.Phase, Is.EqualTo(PendingRewardPhase.Applied));
            Assert.That(floor, Is.EqualTo(T0.AddMinutes(3)));
        }

        [Test]
        public void StrongSpots_HaveIndependentPressureAndOrderedRewards()
        {
            var policy = new AdaptiveRespawnPolicy(1, 2f, 3, 30f, 10f);
            var elite = new StrongOrdinarySpotRuntime(new StrongOrdinarySpotDefinition(
                "elite", StrongOrdinaryRegion.Elite, Vector3.zero, 1.3f, 1.2f, 1.75f, 14f, policy));
            var boss = new StrongOrdinarySpotRuntime(new StrongOrdinarySpotDefinition(
                "boss", StrongOrdinaryRegion.Boss, Vector3.right, 1.7f, 1.45f, 2.75f, 19f, policy));
            elite.RegisterKill(0d);
            Assert.That(elite.KillPressure, Is.EqualTo(1));
            Assert.That(elite.RespawnDelaySeconds, Is.EqualTo(16f));
            Assert.That(boss.KillPressure, Is.Zero);
            Assert.That(boss.RespawnDelaySeconds, Is.EqualTo(19f));
            Assert.That(boss.Definition.RewardMultiplier, Is.GreaterThan(elite.Definition.RewardMultiplier));
        }

        [Test]
        public void PendingReward_PhasesAreIdempotentAndClearByTransactionId()
        {
            var pending = new PendingEncounterReward(
                "tx", RepeatableEncounterKind.Custodian, EncounterRewardEntitlement.PremiumRepeat,
                new CoreReward("custodian", PlayerStatType.Hull, 5f, 9));
            var state = new Chapter1RepeatableState(pendingReward: pending);
            pending.MarkApplied();
            pending.MarkApplied();
            Assert.That(pending.Phase, Is.EqualTo(PendingRewardPhase.Applied));
            Assert.Throws<InvalidOperationException>(() => state.ClearPending("wrong"));
            state.ClearPending("tx");
            Assert.That(state.PendingReward, Is.Null);
        }

        private sealed class FakeTimeProvider : ITimeProvider
        {
            public FakeTimeProvider(DateTime utcNow) => UtcNow = utcNow;
            public DateTime UtcNow { get; set; }
        }
    }
}
