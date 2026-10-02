#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;

namespace Gravivore.Presentation.Development
{
    public sealed class DevelopmentSessionSummary
    {
        private readonly Func<double> _clock;
        private readonly double _startedAt;

        public DevelopmentSessionSummary(Func<double> clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _startedAt = _clock();
        }

        public double ElapsedSeconds => Math.Max(0d, _clock() - _startedAt);
        public int OrdinaryEnemiesDefeated { get; private set; }
        public int PlayerDeaths { get; private set; }
        public int StatLevelUps { get; private set; }
        public bool EliteDefeated { get; private set; }
        public int BossAttempts { get; private set; }
        public int BossResets { get; private set; }
        public bool BossDefeated { get; private set; }

        public void RecordOrdinaryEnemyDefeated() => OrdinaryEnemiesDefeated++;
        public void RecordPlayerDeath() => PlayerDeaths++;
        public void RecordStatChange(int previousLevel, int newLevel) => StatLevelUps += Math.Max(0, newLevel - previousLevel);
        public void RecordEliteDefeated() => EliteDefeated = true;
        public void RecordBossStarted() => BossAttempts++;
        public void RecordBossReset() => BossResets++;
        public void RecordBossDefeated() => BossDefeated = true;
    }
}
#endif
