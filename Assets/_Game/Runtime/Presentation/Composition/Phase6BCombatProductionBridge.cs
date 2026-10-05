using System;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Encounters;
using Gravivore.Presentation.AudioVfx;
using Gravivore.Presentation.Combat;
using UnityEngine;

namespace Gravivore.Presentation.Composition
{
    /// <summary>Observes authoritative events. Does not schedule attacks, damage, or rewards.</summary>
    [DisallowMultipleComponent]
    public sealed class Phase6BCombatProductionBridge : MonoBehaviour
    {
        private S01SceneCompositionRoot _root;
        private GravityLashVfxPool _presentation;
        private float _groundEffectVisualLift;
        public Phase6BVfxCue LastTelegraphCue { get; private set; }
        public float LastTelegraphDuration { get; private set; }
        public int EnemyHitCount { get; private set; }
        public int EnemyDeathCount { get; private set; }
        public int EliteAttackCount { get; private set; }
        public int BossTelegraphCount { get; private set; }
        public Phase6BVfxPool Vfx => _presentation.Vfx;

        public void Initialize(S01SceneCompositionRoot root, GravityLashVfxPool presentation, Phase6BProductionDefinition definition)
        {
            _root = root != null ? root : throw new ArgumentNullException(nameof(root));
            _presentation = presentation != null ? presentation : throw new ArgumentNullException(nameof(presentation));
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            _groundEffectVisualLift = definition.GroundEffectVisualLift;
            if (!presentation.UsesPhase6BProductionPack) throw new InvalidOperationException("Phase6B pack required.");
            root.EnemyPopulation.EnemyDamaged += EnemyDamaged;
            root.EnemyPopulation.EnemyDied += EnemyDied;
            root.MagnetarGuard.TelegraphStarted += EliteTelegraph;
            root.MagnetarGuard.ShockwaveResolved += EliteResolved;
            root.MagnetarGuard.ShockwaveCancelled += EliteCancelled;
            root.MagnetarGuard.Damaged += EliteDamaged;
            root.MagnetarGuard.Defeated += EliteDefeated;
            root.CustodianBoss.TelegraphStarted += BossTelegraph;
            root.CustodianBoss.AttackResolved += BossResolved;
            root.CustodianBoss.EncounterReset += BossReset;
            root.CustodianBoss.Damaged += BossDamaged;
            root.CustodianBoss.Defeated += BossDefeated;
        }

        private void EnemyDamaged(EnemyDamageEvent value)
        {
            if (value.Result.WasLethal) return;
            EnemyHitCount++;
            _presentation.PlayEnemyHit(value.Position, value.LifeId.GetHashCode());
        }
        private void EnemyDied(EnemyDeathEvent value)
        {
            EnemyDeathCount++;
            _presentation.PlayEnemyShutdown(value.Position, value.LifeId.GetHashCode());
        }
        private void EliteTelegraph(EliteShockwaveTelegraphEvent value)
        {
            EliteAttackCount++;
            _presentation.Audio.TryPlay(Phase6BAudioCue.MagnetarSignature, value.Origin);
            Vfx.TryPlayTelegraph(Phase6BVfxCue.HostileTelegraphBase, value.Origin + Vector3.up * _groundEffectVisualLift, Vector3.forward,
                value.Radius, 0f, 0f, value.Duration);
        }
        private void EliteResolved(EliteShockwaveResolvedEvent value)
        {
            Vfx.StopCue(Phase6BVfxCue.HostileTelegraphBase);
            var position = _root.MagnetarGuard.transform.position;
            Vfx.TryPlay(Phase6BVfxCue.HostileImpact, position, position);
        }
        private void EliteCancelled(EliteShockwaveCancelledEvent value) => Vfx.StopCue(Phase6BVfxCue.HostileTelegraphBase);
        private void EliteDamaged(DamageResult value)
        {
            if (!value.WasLethal) _presentation.PlayEnemyHit(_root.MagnetarGuard.transform.position, 0);
        }
        private void EliteDefeated(MagnetarGuardDefeatedEvent value)
        {
            Vfx.StopCue(Phase6BVfxCue.HostileTelegraphBase);
            _presentation.PlayEnemyShutdown(value.Position, 0);
        }
        private void BossTelegraph(BossTelegraphEvent value)
        {
            StopBossTelegraphs();
            BossTelegraphCount++;
            Phase6BAudioCue audio;
            switch (value.Attack)
            {
                case BossAttackType.ConeSweep: LastTelegraphCue = Phase6BVfxCue.BossConeTelegraph; audio = Phase6BAudioCue.CustodianCone; break;
                case BossAttackType.LineCharge: LastTelegraphCue = Phase6BVfxCue.BossLineTelegraph; audio = Phase6BAudioCue.CustodianLine; break;
                case BossAttackType.CirclePulse: LastTelegraphCue = Phase6BVfxCue.BossCircleTelegraph; audio = Phase6BAudioCue.CustodianCircle; break;
                default: throw new ArgumentOutOfRangeException(nameof(value));
            }
            LastTelegraphDuration = value.Duration;
            _presentation.Audio.TryPlay(audio, value.Origin);
            Vfx.TryPlayTelegraph(LastTelegraphCue, value.Origin + Vector3.up * _groundEffectVisualLift, value.Direction,
                value.Range, value.Width, value.HalfAngleDegrees, value.Duration);
        }
        private void BossResolved(BossAttackResolvedEvent value)
        {
            StopBossTelegraphs();
            var position = _root.CustodianBoss.transform.position;
            Vfx.TryPlay(Phase6BVfxCue.HostileImpact, position, position);
        }
        private void BossReset(BossEncounterResetEvent value) => StopBossTelegraphs();
        private void BossDamaged(DamageResult value)
        {
            if (!value.WasLethal) _presentation.PlayEnemyHit(_root.CustodianBoss.transform.position, 1);
        }
        private void BossDefeated(BossDefeatedEvent value)
        {
            StopBossTelegraphs();
            _presentation.PlayEnemyShutdown(value.Position, 1);
        }
        private void StopBossTelegraphs()
        {
            Vfx.StopCue(Phase6BVfxCue.BossConeTelegraph);
            Vfx.StopCue(Phase6BVfxCue.BossLineTelegraph);
            Vfx.StopCue(Phase6BVfxCue.BossCircleTelegraph);
        }
        private void OnDestroy()
        {
            if (_root == null) return;
            var enemies = _root.EnemyPopulation;
            if (enemies != null) { enemies.EnemyDamaged -= EnemyDamaged; enemies.EnemyDied -= EnemyDied; }
            var elite = _root.MagnetarGuard;
            if (elite != null)
            {
                elite.TelegraphStarted -= EliteTelegraph; elite.ShockwaveResolved -= EliteResolved;
                elite.ShockwaveCancelled -= EliteCancelled; elite.Damaged -= EliteDamaged; elite.Defeated -= EliteDefeated;
            }
            var boss = _root.CustodianBoss;
            if (boss != null)
            {
                boss.TelegraphStarted -= BossTelegraph; boss.AttackResolved -= BossResolved; boss.EncounterReset -= BossReset;
                boss.Damaged -= BossDamaged; boss.Defeated -= BossDefeated;
            }
        }
    }
}
