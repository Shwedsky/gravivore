using System;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Equipment;
using Gravivore.Gameplay.Offline;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Gameplay.Quests;
using Gravivore.Gameplay.World;

namespace Gravivore.Persistence.Profile
{
    public sealed class SaveCoordinator : IDisposable
    {
        private readonly ProfileSession _session;
        private readonly PlayerStatsState _playerStats;
        private readonly AssimilationProgressionService _progression;
        private readonly EquipmentService _equipment;
        private readonly QuestService _quests;
        private readonly WorldUnlockState _world;
        private readonly BossCompletionState _boss;
        private readonly OfflineRewardState _offline;
        private readonly float _autosaveDelaySeconds;
        private float _remainingDelay;
        private bool _disposed;

        public SaveCoordinator(
            ProfileSession session,
            PlayerStatsState playerStats,
            AssimilationProgressionService progression,
            EquipmentService equipment,
            QuestService quests,
            WorldUnlockState world,
            BossCompletionState boss,
            OfflineRewardState offline,
            float autosaveDelaySeconds)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _playerStats = playerStats ?? throw new ArgumentNullException(nameof(playerStats));
            _progression = progression ?? throw new ArgumentNullException(nameof(progression));
            _equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
            _quests = quests ?? throw new ArgumentNullException(nameof(quests));
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _boss = boss ?? throw new ArgumentNullException(nameof(boss));
            _offline = offline ?? throw new ArgumentNullException(nameof(offline));
            if (float.IsNaN(autosaveDelaySeconds) || float.IsInfinity(autosaveDelaySeconds) || autosaveDelaySeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(autosaveDelaySeconds));
            }

            _autosaveDelaySeconds = autosaveDelaySeconds;
            _playerStats.StatChanged += HandleStatChanged;
            _progression.Dirty += HandleProgressionDirty;
            _equipment.InventoryChanged += HandleInventoryChanged;
            _quests.ObjectiveProgressed += HandleQuestProgressed;
            _world.GateUnlocked += HandleGateUnlocked;
            _world.EliteWasDefeated += HandleEliteDefeated;
            _boss.Defeated += HandleBossDefeated;
            _offline.Changed += MarkDirty;
            if (!session.StartupCheckpointSucceeded && !session.PersistenceSuspended) MarkDirty();
        }

        public bool IsDirty { get; private set; }

        public void Tick(float deltaTime)
        {
            if (_disposed || _session.PersistenceSuspended || !IsDirty) return;
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            _remainingDelay -= deltaTime;
            if (_remainingDelay <= 0f) FlushNow();
        }

        public bool FlushNow()
        {
            if (_disposed) return false;
            var saved = _session.FlushNow();
            IsDirty = !saved;
            if (!saved) _remainingDelay = _autosaveDelaySeconds;
            return saved;
        }

        public OfflineReturnSummary ProcessResume()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(SaveCoordinator));
            var summary = _session.ProcessResume();
            FlushNow();
            return summary;
        }

        public void MarkDirty()
        {
            if (_disposed || _session.PersistenceSuspended || IsDirty) return;
            IsDirty = true;
            _remainingDelay = _autosaveDelaySeconds;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _playerStats.StatChanged -= HandleStatChanged;
            _progression.Dirty -= HandleProgressionDirty;
            _equipment.InventoryChanged -= HandleInventoryChanged;
            _quests.ObjectiveProgressed -= HandleQuestProgressed;
            _world.GateUnlocked -= HandleGateUnlocked;
            _world.EliteWasDefeated -= HandleEliteDefeated;
            _boss.Defeated -= HandleBossDefeated;
            _offline.Changed -= MarkDirty;
        }

        private void HandleStatChanged(PlayerStatChange _) => MarkDirty();
        private void HandleProgressionDirty(ProgressionDirtyEvent _) => MarkDirty();
        private void HandleInventoryChanged(InventoryChangedEvent _) => MarkDirty();
        private void HandleQuestProgressed(QuestObjectiveProgressedEvent _) => MarkDirty();
        private void HandleGateUnlocked(WorldGateUnlockedEvent _) => MarkDirty();
        private void HandleEliteDefeated(EliteDefeatedEvent _) => MarkDirty();
        private void HandleBossDefeated(BossDefeatedEvent _) => MarkDirty();
    }
}
