#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Offline;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Gameplay.Quests;

namespace Gravivore.Presentation.Development
{
    public sealed class DevelopmentTelemetryObserver : IDisposable
    {
        private readonly DevelopmentTelemetryRecorder _recorder;
        private readonly DevelopmentSessionSummary _summary;
        private readonly PlayerStatsState _stats;
        private readonly AssimilationProgressionService _progression;
        private readonly QuestService _quests;
        private readonly EnemyPopulationController _population;
        private readonly MagnetarGuardController _elite;
        private readonly CustodianBossController _boss;
        private readonly BossCompletionState _completion;
        private readonly PlayerHealthController _health;
        private readonly OfflineRewardState _offline;
        private long _pending;
        private long _balance;
        private bool _ended;

        public DevelopmentTelemetryObserver(
            DevelopmentTelemetryRecorder recorder,
            DevelopmentSessionSummary summary,
            PlayerStatsState stats,
            AssimilationProgressionService progression,
            QuestService quests,
            EnemyPopulationController population,
            MagnetarGuardController elite,
            CustodianBossController boss,
            BossCompletionState completion,
            PlayerHealthController health,
            OfflineRewardState offline)
        {
            _recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));
            _summary = summary ?? throw new ArgumentNullException(nameof(summary));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            _progression = progression ?? throw new ArgumentNullException(nameof(progression));
            _quests = quests ?? throw new ArgumentNullException(nameof(quests));
            _population = population ?? throw new ArgumentNullException(nameof(population));
            _elite = elite ?? throw new ArgumentNullException(nameof(elite));
            _boss = boss ?? throw new ArgumentNullException(nameof(boss));
            _completion = completion ?? throw new ArgumentNullException(nameof(completion));
            _health = health ?? throw new ArgumentNullException(nameof(health));
            _offline = offline ?? throw new ArgumentNullException(nameof(offline));
            _pending = offline.PendingReward;
            _balance = offline.MaterialBalance;

            _stats.StatChanged += HandleStatChanged;
            _progression.RewardGranted += HandleRewardGranted;
            _quests.ObjectiveCompleted += HandleObjectiveCompleted;
            _population.EnemyDied += HandleEnemyDied;
            _elite.Activated += HandleEliteStarted;
            _elite.Defeated += HandleEliteDefeated;
            _boss.EncounterStarted += HandleBossStarted;
            _boss.EncounterReset += HandleBossReset;
            _completion.Defeated += HandleBossDefeated;
            _health.Died += HandlePlayerDied;
            _offline.Changed += HandleOfflineChanged;
            _recorder.TryRecord("session_started", new EmptyPayload());
        }

        public DevelopmentSessionSummary Summary => _summary;
        public void RecordPause(bool paused) => _recorder.TryRecord(paused ? "app_paused" : "app_resumed", new LifecyclePayload(paused));

        public void RecordStartupOfflineReward(OfflineReturnSummary summary)
        {
            if (summary.EarnedAmount > 0)
                _recorder.TryRecord("offline_reward_generated", new OfflineRewardPayload(summary.EarnedAmount, _offline.PendingReward, _offline.MaterialBalance));
        }

        public void EndSession()
        {
            if (_ended) return;
            _ended = true;
            _recorder.TryRecord("session_ended", new EmptyPayload());
        }

        public void Dispose()
        {
            EndSession();
            _stats.StatChanged -= HandleStatChanged;
            _progression.RewardGranted -= HandleRewardGranted;
            _quests.ObjectiveCompleted -= HandleObjectiveCompleted;
            _population.EnemyDied -= HandleEnemyDied;
            _elite.Activated -= HandleEliteStarted;
            _elite.Defeated -= HandleEliteDefeated;
            _boss.EncounterStarted -= HandleBossStarted;
            _boss.EncounterReset -= HandleBossReset;
            _completion.Defeated -= HandleBossDefeated;
            _health.Died -= HandlePlayerDied;
            _offline.Changed -= HandleOfflineChanged;
        }

        private void HandleStatChanged(PlayerStatChange change)
        {
            _summary.RecordStatChange(change.PreviousLevel, change.CurrentLevel);
            _recorder.TryRecord("stat_level_changed", new StatLevelChangedPayload(change.Stat.ToString(), change.PreviousLevel, change.CurrentLevel));
        }

        private void HandleRewardGranted(CoreRewardGrantedEvent reward) =>
            _recorder.TryRecord("assimilation_reward_granted", new AssimilationRewardPayload(reward.EnemyId, reward.Stat.ToString(), _progression.State.TotalAssimilationScore));

        private void HandleObjectiveCompleted(QuestObjectiveCompletedEvent completed) =>
            _recorder.TryRecord("objective_completed", new ObjectiveCompletedPayload(completed.QuestId, completed.Objective.Id));

        private void HandleEnemyDied(EnemyDeathEvent _) => _summary.RecordOrdinaryEnemyDefeated();
        private void HandleEliteStarted(MagnetarGuardActivatedEvent value) => _recorder.TryRecord("elite_encounter_started", new EncounterPayload(value.EliteId));
        private void HandleEliteDefeated(MagnetarGuardDefeatedEvent value) { _summary.RecordEliteDefeated(); _recorder.TryRecord("elite_defeated", new EncounterPayload(value.EliteId)); }
        private void HandleBossStarted(BossEncounterStartedEvent value) { _summary.RecordBossStarted(); _recorder.TryRecord("boss_encounter_started", new EncounterPayload(value.BossId)); }
        private void HandleBossReset(BossEncounterResetEvent value) { _summary.RecordBossReset(); _recorder.TryRecord("boss_encounter_reset", new EncounterPayload(value.BossId)); }
        private void HandleBossDefeated(BossDefeatedEvent value) { _summary.RecordBossDefeated(); _recorder.TryRecord("boss_defeated", new EncounterPayload(value.BossId)); }

        private void HandlePlayerDied(PlayerDeathEvent _)
        {
            _summary.RecordPlayerDeath();
            var levels = _stats.BaseLevels;
            _recorder.TryRecord("player_died", new PlayerDeathPayload(
                _progression.State.TotalAssimilationScore,
                levels.Power, levels.Hull, levels.Armor, levels.Flux, levels.Mobility));
        }

        private void HandleOfflineChanged()
        {
            var pending = _offline.PendingReward;
            var balance = _offline.MaterialBalance;
            if (pending > _pending)
                _recorder.TryRecord("offline_reward_generated", new OfflineRewardPayload(pending - _pending, pending, balance));
            else if (pending < _pending && balance > _balance)
                _recorder.TryRecord("offline_reward_claimed", new OfflineRewardPayload(balance - _balance, pending, balance));
            _pending = pending;
            _balance = balance;
        }
    }
}
#endif
