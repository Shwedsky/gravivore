using System;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Progression;
using Gravivore.Gameplay.Quests;
using Gravivore.Gameplay.World;

namespace Gravivore.Persistence.Profile
{
    /// <summary>Production encounter lifecycle; persistence and gameplay own every transition.</summary>
    public sealed class Chapter1EncounterRuntime : IDisposable, IBossEncounterAccess
    {
        private readonly ProfileSession _session;
        private readonly RepeatableEncounterRewardLadder _ladder;
        private MagnetarGuardController _elite;
        private CustodianBossController _boss;
        private QuestService _quests;

        public Chapter1EncounterRuntime(ProfileSession session, ProgressionConfiguration progression,
            CoreReward eliteUnit, CoreReward bossUnit)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _ladder = new RepeatableEncounterRewardLadder(eliteUnit, bossUnit);
            Transactions = new RepeatableRewardTransactionCoordinator(session,
                new AuthoredRewardApplier(session.State.PlayerStats, session.State.Progression, progression),
                CompleteFirstClear);
            session.State.World.GateUnlocked += HandleGateUnlocked;
        }

        public RepeatableRewardTransactionCoordinator Transactions { get; }
        public RepeatableEncounterService Encounters => Transactions.Encounters;
        public CoreReward PreviewReward(RepeatableEncounterKind kind) => _ladder.Build(kind,
            Encounters.Read(kind, kind == RepeatableEncounterKind.Magnetar
                ? _session.State.World.EliteDefeated : _session.State.Boss.IsDefeated).RewardEntitlement);
        public bool CanEngage => _session.State.World.BossGateUnlocked &&
            _session.State.Repeatable.PendingReward == null &&
            Encounters.Read(RepeatableEncounterKind.Custodian, _session.State.Boss.IsDefeated).Available;

        public void Attach(MagnetarGuardController elite, CustodianBossController boss, QuestService quests)
        {
            _elite = elite ?? throw new ArgumentNullException(nameof(elite));
            _boss = boss ?? throw new ArgumentNullException(nameof(boss));
            _quests = quests ?? throw new ArgumentNullException(nameof(quests));
            _elite.Defeated += HandleEliteDefeated;
            _boss.Defeated += HandleBossDefeated;
            Transactions.RecoverPending();
            Tick();
        }

        public void Tick()
        {
            if (_elite == null || _boss == null) return;
            if (_session.State.Repeatable.PendingReward != null)
            {
                // Retry on a gameplay tick after a storage failure. No new encounter may start.
                if (!Transactions.RecoverPending()) return;
            }
            if (_session.State.World.EliteGateUnlocked &&
                Encounters.Read(RepeatableEncounterKind.Magnetar, _session.State.World.EliteDefeated).Available)
            {
                if (_elite.State == MagnetarGuardState.Dead) _elite.ResetForRepeat();
                _elite.ActivateEncounter();
            }
            if (CanEngage && _boss.State == CustodianBossState.Dead) _boss.ResetForRepeat();
        }

        private void HandleEliteDefeated(MagnetarGuardDefeatedEvent value) =>
            Commit(RepeatableEncounterKind.Magnetar, _session.State.World.EliteDefeated, value.EliteId);
        private void HandleGateUnlocked(WorldGateUnlockedEvent value)
        {
            if (_elite != null && _session.State.Repeatable.PendingReward == null &&
                _session.State.World.EliteGateUnlocked &&
                Encounters.Read(RepeatableEncounterKind.Magnetar, _session.State.World.EliteDefeated).Available)
                _elite.ActivateEncounter();
        }
        private void HandleBossDefeated(BossDefeatedEvent value) =>
            Commit(RepeatableEncounterKind.Custodian, _session.State.Boss.IsDefeated, value.BossId);

        private void Commit(RepeatableEncounterKind kind, bool previouslyCleared, string enemyId)
        {
            var package = _ladder.Build(kind, Encounters.Read(kind, previouslyCleared).RewardEntitlement);
            Transactions.PrepareAndCommit(kind, previouslyCleared,
                new CoreReward(enemyId, package.Stat, package.StatExperience, package.AssimilationScore));
        }

        private void CompleteFirstClear(PendingEncounterReward pending)
        {
            if (pending.Entitlement != EncounterRewardEntitlement.FirstClear) return;
            if (pending.EncounterKind == RepeatableEncounterKind.Magnetar)
            {
                try { _quests.RecordEliteDefeated(pending.Reward.EnemyId); }
                catch (Exception exception) { UnityEngine.Debug.LogException(exception); }
                _session.State.World.RecordEliteDefeated(pending.Reward.EnemyId);
            }
            else
            {
                try { _quests.RecordBossDefeated(pending.Reward.EnemyId); }
                catch (Exception exception) { UnityEngine.Debug.LogException(exception); }
                _session.State.Boss.TryRecordDefeat(pending.Reward.EnemyId,
                    _boss != null ? _boss.transform.position : default);
            }
        }

        public void Dispose()
        {
            _session.State.World.GateUnlocked -= HandleGateUnlocked;
            if (_elite != null) _elite.Defeated -= HandleEliteDefeated;
            if (_boss != null) _boss.Defeated -= HandleBossDefeated;
            _elite = null;
            _boss = null;
        }
    }
}
