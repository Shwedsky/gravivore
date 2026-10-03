using System;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Presentation.Combat;
using Gravivore.Presentation.Evolution;
using UnityEngine;

namespace Gravivore.Presentation.Feedback
{
    [DisallowMultipleComponent]
    public sealed class S14CombatFeedbackPresenter : MonoBehaviour
    {
        private EnemyPopulationController _enemies;
        private AssimilationProgressionService _progression;
        private PlayerHealthController _playerHealth;
        private Transform _player;
        private EvolutionVfxRelay _evolution;
        private GravityLashVfxPool _lash;
        private MagnetarGuardController _elite;
        private CustodianBossController _boss;
        private S14PresentationDefinition _definition;
        private S14AudioPresenter _audio;
        private IHapticFeedback _haptics;
        private readonly Material[] _materials = new Material[6];
        private OrdinaryEnemyController[] _reactionEnemies;
        private Transform[] _reactionVisuals;
        private float[] _reactionRemaining;
        private Quaternion[] _reactionRest;
        private struct PendingHit { public bool Active, Lethal; public Vector3 Position; public float Remaining; public EnemyLifeId Life; }
        private readonly PendingHit[] _pendingHits = new PendingHit[16];
        private int _pendingCursor;

        public PooledPulseVfx EnemyHitPool { get; private set; }
        public PooledPulseVfx EnemyDeathPool { get; private set; }
        public PooledPulseVfx AssimilationPool { get; private set; }
        public PooledPulseVfx EvolutionPool { get; private set; }
        public PooledPulseVfx PlayerHitPool { get; private set; }
        public PooledPulseVfx PlayerDeathPool { get; private set; }

        public void Initialize(
            EnemyPopulationController enemies,
            AssimilationProgressionService progression,
            PlayerHealthController playerHealth,
            Transform player,
            EvolutionVfxRelay evolution,
            GravityLashVfxPool lash,
            MagnetarGuardController elite,
            CustodianBossController boss,
            S14PresentationDefinition definition,
            Material unlitMaterial,
            S14AudioPresenter audio,
            IHapticFeedback haptics)
        {
            _enemies = enemies != null ? enemies : throw new ArgumentNullException(nameof(enemies));
            _progression = progression ?? throw new ArgumentNullException(nameof(progression));
            _playerHealth = playerHealth != null ? playerHealth : throw new ArgumentNullException(nameof(playerHealth));
            _player = player != null ? player : throw new ArgumentNullException(nameof(player));
            _evolution = evolution != null ? evolution : throw new ArgumentNullException(nameof(evolution));
            _lash = lash != null ? lash : throw new ArgumentNullException(nameof(lash));
            _elite = elite != null ? elite : throw new ArgumentNullException(nameof(elite));
            _boss = boss != null ? boss : throw new ArgumentNullException(nameof(boss));
            _definition = definition != null ? definition : throw new ArgumentNullException(nameof(definition));
            _audio = audio != null ? audio : throw new ArgumentNullException(nameof(audio));
            _haptics = haptics ?? throw new ArgumentNullException(nameof(haptics));
            if (unlitMaterial == null) throw new ArgumentNullException(nameof(unlitMaterial));
            _definition.ValidateOrThrow();

            EnemyHitPool = CreatePool("Enemy Hit VFX", _definition.HitPoolSize, unlitMaterial, _definition.HitColor, 0);
            EnemyDeathPool = CreatePool("Enemy Death VFX", _definition.DeathPoolSize, unlitMaterial, _definition.DeathColor, 1);
            AssimilationPool = CreatePool("Assimilation VFX", _definition.AssimilationPoolSize, unlitMaterial, _definition.AssimilationColor, 2);
            EvolutionPool = CreatePool("Evolution VFX", _definition.EvolutionPoolSize, unlitMaterial, _definition.EvolutionColor, 3);
            PlayerHitPool = CreatePool("Player Hit VFX", 3, unlitMaterial, _definition.PlayerHitColor, 4);
            PlayerDeathPool = CreatePool("Player Death VFX", 2, unlitMaterial, _definition.PlayerHitColor, 5);
            _reactionEnemies = enemies.GetComponentsInChildren<OrdinaryEnemyController>(true);
            _reactionVisuals = new Transform[_reactionEnemies.Length];
            _reactionRemaining = new float[_reactionEnemies.Length];
            _reactionRest = new Quaternion[_reactionEnemies.Length];
            for (var i = 0; i < _reactionEnemies.Length; i++)
            {
                _reactionVisuals[i] = _reactionEnemies[i].transform.Find("Enemy Art Root");
                if (_reactionVisuals[i] != null) _reactionRest[i] = _reactionVisuals[i].localRotation;
            }

            _enemies.EnemyDamaged += HandleEnemyDamaged;
            _enemies.EnemyDied += HandleEnemyDied;
            _progression.RewardGranted += HandleRewardGranted;
            _playerHealth.Damaged += HandlePlayerDamaged;
            _playerHealth.Died += HandlePlayerDied;
            _evolution.Requested += HandleEvolution;
            _lash.CuePlayed += HandleLashCue;
            _elite.TelegraphStarted += HandleEliteTelegraph;
            _elite.ShockwaveResolved += HandleEliteImpact;
            _elite.Damaged += HandleEliteDamaged;
            _boss.TelegraphStarted += HandleBossTelegraph;
            _boss.AttackResolved += HandleBossImpact;
            _boss.Damaged += HandleBossDamaged;
        }

        public void Shutdown()
        {
            if (_enemies != null)
            {
                _enemies.EnemyDamaged -= HandleEnemyDamaged;
                _enemies.EnemyDied -= HandleEnemyDied;
            }
            if (_progression != null) _progression.RewardGranted -= HandleRewardGranted;
            if (_playerHealth != null) _playerHealth.Damaged -= HandlePlayerDamaged;
            if (_playerHealth != null) _playerHealth.Died -= HandlePlayerDied;
            if (_evolution != null) _evolution.Requested -= HandleEvolution;
            if (_lash != null) _lash.CuePlayed -= HandleLashCue;
            if (_elite != null)
            {
                _elite.TelegraphStarted -= HandleEliteTelegraph;
                _elite.ShockwaveResolved -= HandleEliteImpact;
                _elite.Damaged -= HandleEliteDamaged;
            }
            if (_boss != null)
            {
                _boss.TelegraphStarted -= HandleBossTelegraph;
                _boss.AttackResolved -= HandleBossImpact;
                _boss.Damaged -= HandleBossDamaged;
            }
            _enemies = null;
        }

        private PooledPulseVfx CreatePool(string name, int size, Material source, Color color, int materialIndex)
        {
            var material = new Material(source) { color = color, hideFlags = HideFlags.HideAndDontSave };
            _materials[materialIndex] = material;
            var poolObject = new GameObject(name, typeof(PooledPulseVfx));
            poolObject.transform.SetParent(transform, false);
            var pool = poolObject.GetComponent<PooledPulseVfx>();
            pool.Initialize(size, material, true);
            return pool;
        }

        private void HandleEnemyDamaged(EnemyDamageEvent damage)
        {
            if (damage.Result.WasLethal) return;
            for (var i = 0; i < _reactionEnemies.Length; i++)
                if (_reactionEnemies[i].LifeId.Equals(damage.LifeId)) _reactionRemaining[i] = _definition.HitDuration;
            QueueHit(damage.Position, false, damage.LifeId);
        }

        private void HandleEnemyDied(EnemyDeathEvent death)
        {
            QueueHit(death.Position, true, death.LifeId);
        }

        private void HandleRewardGranted(CoreRewardGrantedEvent reward)
        {
            TryPlayPool(
                AssimilationPool,
                reward.WorldPosition + Vector3.up,
                _definition.AssimilationDuration,
                0.25f,
                0.08f,
                _player);
            TryPlayAudio(S14AudioCue.Assimilation);
        }

        private void HandlePlayerDamaged(Gravivore.Gameplay.Combat.DamageResult damage)
        {
            TryPlayPool(PlayerHitPool, _player.position + Vector3.up, _definition.HitDuration, 0.25f, 1.1f);
            TryPlayAudio(S14AudioCue.PlayerHit);
            TryPlayHaptic(damage.WasLethal ? HapticCue.HeavyImpact : HapticCue.LightImpact);
        }

        private void HandleEvolution(EvolutionTierChangedEvent change)
        {
            TryPlayPool(EvolutionPool, _player.position + Vector3.up, _definition.EvolutionDuration, 0.4f, 2.2f);
            TryPlayAudio(S14AudioCue.Evolution);
            TryPlayHaptic(HapticCue.Evolution);
        }

        private void HandleLashCue(GravityLashCue cue, Vector3 position)
        {
            if (cue == GravityLashCue.Windup) TryPlayAudio(S14AudioCue.LashWindup);
            if (cue == GravityLashCue.Beam) TryPlayAudio(S14AudioCue.Release);
            if (cue == GravityLashCue.Impact) TryPlayAudio(S14AudioCue.LashImpact);
        }

        private void HandlePlayerDied(PlayerDeathEvent death)
        {
            TryPlayPool(PlayerDeathPool, death.Position + Vector3.up * .7f, _definition.DeathDuration, .6f, 1.8f);
            TryPlayAudio(S14AudioCue.PlayerDeath);
        }

        private void Update()
        {
            TickFeedback(Time.deltaTime);
        }

        public void TickFeedback(float deltaTime)
        {
            for (var i = 0; i < _pendingHits.Length; i++)
            {
                if (!_pendingHits[i].Active) continue;
                _pendingHits[i].Remaining -= deltaTime;
                if (_pendingHits[i].Remaining > 0) continue;
                var hit = _pendingHits[i]; _pendingHits[i].Active = false;
                if (!hit.Lethal)
                    for (var e = 0; e < _reactionEnemies.Length; e++)
                        if (_reactionEnemies[e].IsAlive && _reactionEnemies[e].LifeId.Equals(hit.Life))
                            hit.Position = _reactionEnemies[e].transform.position;
                TryPlayPool(hit.Lethal ? EnemyDeathPool : EnemyHitPool, hit.Position + Vector3.up * .7f,
                    hit.Lethal ? _definition.DeathDuration : _definition.HitDuration,
                    hit.Lethal ? .4f : .25f, hit.Lethal ? 1.5f : .8f);
                TryPlayAudio(hit.Lethal ? S14AudioCue.Death : S14AudioCue.Hit);
            }
            if (_reactionEnemies == null) return;
            for (var i = 0; i < _reactionEnemies.Length; i++)
            {
                if (_reactionRemaining[i] <= 0 || _reactionVisuals[i] == null) continue;
                _reactionRemaining[i] = _reactionEnemies[i].IsAlive ? Mathf.Max(0, _reactionRemaining[i] - deltaTime) : 0;
                var strength = _reactionRemaining[i] / _definition.HitDuration;
                _reactionVisuals[i].localRotation = _reactionRest[i] * Quaternion.Euler(0, 0, 5f * strength);
            }
        }

        private void QueueHit(Vector3 position, bool lethal, EnemyLifeId life)
        {
            // Snapshot survives immediate authoritative recycle. Cosmetic delay never retains a pooled enemy.
            _pendingHits[_pendingCursor] = new PendingHit { Active = true, Lethal = lethal, Position = position, Life = life,
                Remaining = _definition.LashWindupDuration + _definition.LashBeamDuration };
            _pendingCursor = (_pendingCursor + 1) % _pendingHits.Length;
        }

        private void HandleEliteTelegraph(EliteShockwaveTelegraphEvent value) => TryPlayAudio(S14AudioCue.Telegraph);
        private void HandleBossTelegraph(BossTelegraphEvent value) => TryPlayAudio(S14AudioCue.Telegraph);

        private void HandleEliteImpact(EliteShockwaveResolvedEvent value)
        {
            TryPlayAudio(S14AudioCue.BossImpact);
        }

        private void HandleBossImpact(BossAttackResolvedEvent value)
        {
            TryPlayAudio(S14AudioCue.BossImpact);
        }

        private void HandleEliteDamaged(Gravivore.Gameplay.Combat.DamageResult damage)
        {
            TryPlayPool(EnemyHitPool, _elite.transform.position + Vector3.up, _definition.HitDuration, 0.18f, 0.75f);
        }

        private void HandleBossDamaged(Gravivore.Gameplay.Combat.DamageResult damage)
        {
            TryPlayPool(EnemyHitPool, _boss.transform.position + Vector3.up, _definition.HitDuration, 0.22f, 0.9f);
        }

        private void TryPlayAudio(S14AudioCue cue)
        {
            try { _audio.Play(cue); }
            catch (Exception exception) { Debug.LogException(exception); }
        }

        private void TryPlayHaptic(HapticCue cue)
        {
            try { _haptics.Play(cue); }
            catch (Exception exception) { Debug.LogException(exception); }
        }

        private static void TryPlayPool(
            PooledPulseVfx pool,
            Vector3 position,
            float duration,
            float startScale,
            float endScale,
            Transform destination = null)
        {
            try { pool.Play(position, duration, startScale, endScale, destination); }
            catch (Exception exception) { Debug.LogException(exception); }
        }

        private void OnDestroy()
        {
            Shutdown();
            for (var i = 0; i < _materials.Length; i++)
                if (_materials[i] != null) Destroy(_materials[i]);
        }
    }
}
