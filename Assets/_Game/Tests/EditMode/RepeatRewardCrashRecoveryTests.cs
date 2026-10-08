using System;
using System.IO;
using Gravivore.Core.Time;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Equipment;
using Gravivore.Gameplay.Offline;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Persistence.Profile;
using NUnit.Framework;

namespace Gravivore.Tests.EditMode
{
    public sealed class RepeatRewardCrashRecoveryTests
    {
        [TestCase(1,false)] [TestCase(2,false)] [TestCase(3,false)]
        [TestCase(2,true)] [TestCase(3,true)]
        public void WeaponCopyAndRankRecoverExactlyOnceAcrossEveryRewardCheckpoint(int checkpoint,bool crash)
        {
            var original=SaveOfflineTests.CreateContext();
            var catalog=new EquipmentCatalog(new[]{new EquipmentItem(Chapter01Weapon.ItemId,"M-0",EquipmentSlot.Weapon,new EquipmentFlatModifier(12,0,0,0,0),5,4)});
            var context=new ProfileRestoreContext(original.PlayerStats,original.Progression,catalog,original.Quests,original.EliteGateId,original.BossGateId,original.EliteEnemyId,original.BossId);
            var repository=new FaultRepository();var session=Start(repository,context);
            // Test a later copy, where an accidental retry would silently raise rank twice.
            var equipment=new EquipmentService(session.State.PlayerStats,catalog,session.State.Inventory);
            equipment.GrantRankedCopy(Chapter01Weapon.ItemId);equipment.Equip(Chapter01Weapon.ItemId,EquipmentSlot.Weapon);Assert.IsTrue(session.FlushNow());
            RepeatableRewardTransactionCoordinator Transaction(ProfileSession value)
            {
                var service=new EquipmentService(value.State.PlayerStats,catalog,value.State.Inventory);
                return new RepeatableRewardTransactionCoordinator(value,new AuthoredRewardApplier(value.State.PlayerStats,value.State.Progression,context.Progression),
                    applyLoot:pending=>{if(pending.EncounterKind==RepeatableEncounterKind.Custodian)service.GrantRankedCopy(Chapter01Weapon.ItemId);});
            }
            var transaction=Transaction(session);repository.ResetFault(checkpoint);
            Assert.IsFalse(transaction.PrepareAndCommit(RepeatableEncounterKind.Custodian,true,new CoreReward("custodian-m0",PlayerStatType.Hull,2,10)));
            repository.ResetFault(0);if(crash){session=Start(repository,context);transaction=Transaction(session);}
            Assert.IsTrue(transaction.RecoverPending());Assert.IsTrue(transaction.RecoverPending());
            Assert.That(session.State.Inventory.GetRank(Chapter01Weapon.ItemId),Is.EqualTo(2));
            var final=Start(repository,context);Assert.That(final.State.Inventory.GetRank(Chapter01Weapon.ItemId),Is.EqualTo(2));
            Assert.That(final.State.Repeatable.PendingReward,Is.Null);
            Assert.Throws<InvalidOperationException>(()=>transaction.PrepareAndCommit(RepeatableEncounterKind.Custodian,true,new CoreReward("custodian-m0",PlayerStatType.Hull,2,10)));
            Assert.That(session.State.Inventory.GetRank(Chapter01Weapon.ItemId),Is.EqualTo(2));
        }
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void EveryFailedCheckpoint_CanRetryInSessionWithoutDuplicatingReward(int checkpoint)
        {
            var repository = new FaultRepository();
            var context = SaveOfflineTests.CreateContext();
            var session = Start(repository, context);
            var transaction = CreateTransaction(session, context);
            var feedbackCount=0;
            transaction.RewardApplied+=_=>feedbackCount++;
            repository.ResetFault(checkpoint);
            Assert.That(transaction.PrepareAndCommit(RepeatableEncounterKind.Magnetar, true,
                new CoreReward("magnetar-guard", PlayerStatType.Power, 2f, 10)), Is.False);
            Assert.That(session.State.Repeatable.PendingReward, Is.Not.Null);
            repository.ResetFault(0);
            Assert.That(transaction.RecoverPending(), Is.True);
            Assert.That(transaction.RecoverPending(), Is.True);
            Assert.That(session.State.Progression.TotalAssimilationScore, Is.EqualTo(10));
            Assert.That(Start(repository, context).State.Progression.TotalAssimilationScore, Is.EqualTo(10));
            Assert.That(feedbackCount,Is.EqualTo(1),"Applied reward feedback must not replay during checkpoint retries.");
        }

        [TestCase(2)]
        [TestCase(3)]
        public void CrashAfterPreparedCheckpoint_ReloadRecoversExactlyOnce(int checkpoint)
        {
            var repository = new FaultRepository();
            var context = SaveOfflineTests.CreateContext();
            var session = Start(repository, context);
            repository.ResetFault(checkpoint);
            Assert.That(CreateTransaction(session, context).PrepareAndCommit(RepeatableEncounterKind.Custodian, true,
                new CoreReward("custodian-m0", PlayerStatType.Hull, 2f, 10)), Is.False);
            repository.ResetFault(0);
            var reloaded = Start(repository, context);
            Assert.That(CreateTransaction(reloaded, context).RecoverPending(), Is.True);
            var final = Start(repository, context);
            Assert.That(final.State.Progression.TotalAssimilationScore, Is.EqualTo(10));
            Assert.That(final.State.Repeatable.PendingReward, Is.Null);
            Assert.That(final.State.Repeatable.Custodian.RewardedKillsInWindow, Is.EqualTo(1));
            Assert.That(final.State.Repeatable.Custodian.NextAvailableUtc, Is.EqualTo(FixedTime.Now.AddMinutes(30)));
        }

        [Test]
        public void FailedPreparedSave_CrashCannotGrantAnUncommittedReward()
        {
            var repository = new FaultRepository();
            var context = SaveOfflineTests.CreateContext();
            var session = Start(repository, context);
            repository.ResetFault(1);
            CreateTransaction(session, context).PrepareAndCommit(RepeatableEncounterKind.Magnetar, true,
                new CoreReward("magnetar-guard", PlayerStatType.Power, 2f, 10));
            Assert.That(session.State.Progression.TotalAssimilationScore, Is.Zero);
            repository.ResetFault(0);
            var reloaded = Start(repository, context);
            Assert.That(reloaded.State.Progression.TotalAssimilationScore, Is.Zero);
            Assert.That(reloaded.State.Repeatable.PendingReward, Is.Null);
        }

        private static RepeatableRewardTransactionCoordinator CreateTransaction(ProfileSession session, ProfileRestoreContext context) =>
            new RepeatableRewardTransactionCoordinator(session,
                new AuthoredRewardApplier(session.State.PlayerStats, session.State.Progression, context.Progression));
        private static ProfileSession Start(FaultRepository repository, ProfileRestoreContext context) =>
            ProfileSession.Start(repository, context,
                new SaveOfflineConfiguration(2, 2f, new OfflineRewardConfiguration(120d, .25d, TimeSpan.FromHours(2))),
                new FixedTime(), new Diagnostics());

        private sealed class FixedTime : ITimeProvider
        {
            public static readonly DateTime Now = new DateTime(2031, 4, 5, 12, 0, 0, DateTimeKind.Utc);
            public DateTime UtcNow => Now;
        }
        private sealed class Diagnostics : ISaveDiagnostics
        {
            public void Warning(string message, Exception exception = null) { }
            public void Error(string message, Exception exception) { Assert.That(exception, Is.TypeOf<IOException>()); }
        }
        private sealed class FaultRepository : IProfileRepository
        {
            private readonly UnityJsonSaveSerializer _serializer = new UnityJsonSaveSerializer();
            private string _disk;
            private int _writes;
            private int _failedWrite;
            public void ResetFault(int write) { _writes = 0; _failedWrite = write; }
            public ProfileLoadResult LoadOrCreate(Func<SaveRootDto> freshFactory, Action<SaveRootDto> validate)
            {
                var created = _disk == null;
                var dto = created ? freshFactory() : _serializer.Deserialize<SaveRootDto>(_disk);
                validate(dto);
                return new ProfileLoadResult(dto, created, false, false);
            }
            public void Save(SaveRootDto save, Func<SaveRootDto> freshFactory, Action<SaveRootDto> validate)
            {
                if (++_writes == _failedWrite) throw new IOException("Injected checkpoint failure");
                validate(save);
                _disk = _serializer.Serialize(save);
            }
        }
    }
}
