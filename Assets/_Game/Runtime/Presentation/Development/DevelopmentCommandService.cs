#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Quests;
using Gravivore.Gameplay.World;

namespace Gravivore.Presentation.Development
{
    public sealed class DevelopmentCommandService
    {
        private readonly PlayerStatsState _stats;
        private readonly QuestService _quests;
        private readonly WorldUnlockState _world;
        private readonly MagnetarGuardController _elite;
        private readonly BossCompletionState _completion;
        private readonly CustodianBossController _boss;
        private readonly PlayerHealthController _health;
        private readonly string _eliteId;
        private readonly string _bossId;
        private readonly Action _markDirty;
        private readonly Func<bool> _resetProfile;
        private readonly Action _restartRuntime;

        public DevelopmentCommandService(
            PlayerStatsState stats,
            QuestService quests,
            WorldUnlockState world,
            MagnetarGuardController elite,
            BossCompletionState completion,
            CustodianBossController boss,
            PlayerHealthController health,
            string eliteId,
            string bossId,
            Action markDirty,
            Func<bool> resetProfile,
            Action restartRuntime)
        {
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            _quests = quests ?? throw new ArgumentNullException(nameof(quests));
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _elite = elite;
            _completion = completion ?? throw new ArgumentNullException(nameof(completion));
            _boss = boss;
            _health = health ?? throw new ArgumentNullException(nameof(health));
            _eliteId = !string.IsNullOrWhiteSpace(eliteId) ? eliteId : throw new ArgumentException("Elite id is required.", nameof(eliteId));
            _bossId = !string.IsNullOrWhiteSpace(bossId) ? bossId : throw new ArgumentException("Boss id is required.", nameof(bossId));
            _markDirty = markDirty ?? throw new ArgumentNullException(nameof(markDirty));
            _resetProfile = resetProfile ?? throw new ArgumentNullException(nameof(resetProfile));
            _restartRuntime = restartRuntime ?? throw new ArgumentNullException(nameof(restartRuntime));
        }

        public bool GodModeEnabled => _health.DevelopmentGodMode;

        public bool GrantStat(PlayerStatType stat, int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            var current = _stats.BaseLevels.GetLevel(stat);
            var maximum = _stats.GetMaximumLevel(stat);
            var changed = _stats.SetLevel(stat, Math.Min(maximum, checked(current + amount)));
            if (changed) _markDirty();
            return changed;
        }

        public bool GrantAllStats(int amount)
        {
            var changed = false;
            for (var i = 0; i < 5; i++) changed |= GrantStat((PlayerStatType)i, amount);
            return changed;
        }

        public bool UnlockElite()
        {
            var changed = _world.TryUnlockEliteGate();
            if (changed) _markDirty();
            return changed;
        }

        public bool UnlockBoss()
        {
            var changed = UnlockElite();
            if (!_world.EliteDefeated && _elite != null)
            {
                _elite.ActivateEncounter();
                var result = _elite.ApplyDamage(new DamageRequest(_elite.MaximumHitPoints + 100000f, DamageType.Gravity));
                changed |= result.WasLethal;
            }
            else
            {
                changed |= _quests.CompleteEncounterObjectiveForDevelopment(QuestObjectiveType.EliteDefeated, _eliteId);
                changed |= _world.RecordEliteDefeated(_eliteId);
            }
            if (changed) _markDirty();
            return changed;
        }

        public bool ResetBoss()
        {
            var changed = _quests.ResetEncounterObjectiveForDevelopment(QuestObjectiveType.BossDefeated, _bossId);
            changed |= _completion.ResetForDevelopment();
            if (_boss != null) changed |= _boss.ResetForDevelopment();
            if (changed) _markDirty();
            return changed;
        }

        public bool ToggleGodMode()
        {
            _health.SetDevelopmentGodMode(!_health.DevelopmentGodMode);
            return _health.DevelopmentGodMode;
        }

        public bool ResetProfile()
        {
            if (!_resetProfile()) return false;
            _restartRuntime();
            return true;
        }
    }
}
#endif
