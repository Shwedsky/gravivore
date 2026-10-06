using System;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.Combat;
using Gravivore.Presentation.Composition;
using UnityEngine;

namespace Gravivore.Presentation.Player
{
    /// <summary>Observes movement/health/attack events. Never writes gameplay transforms or timing.</summary>
    public sealed class VisualSliceAnimationBridge : MonoBehaviour
    {
        private static readonly int Idle = Animator.StringToHash("Idle"), Run = Animator.StringToHash("Run"),
            Attack = Animator.StringToHash("Attack"), Hit = Animator.StringToHash("Hit"), Death = Animator.StringToHash("Death");
        private sealed class Actor
        {
            public CharacterVisualBinding Binding;
            public Transform Model;
            public Animator Animator;
            public int State;
            public float ReactionUntil, Hp, Cooldown;
            public EnemyLifeId Life;
            public Vector3 Position;
            public void Refresh()
            {
                if (Model == Binding.ActiveModel) return;
                Model = Binding.ActiveModel;
                Animator = Model != null ? Model.GetComponentInChildren<Animator>(true) : null;
                State = 0;
            }
            public void Pose(int state)
            {
                Refresh();
                if (Animator == null || !Animator.gameObject.activeInHierarchy || State == state) return;
                Animator.CrossFadeInFixedTime(state, .065f, 0, 0);
                State = state;
            }
        }
        private sealed class Corpse
        {
            public string Id;
            public GameObject Object;
            public Animator Animator;
            public float Until;
        }
        private S01SceneCompositionRoot _root;
        private GravityLashVfxPool _lash;
        private Actor _player, _elite;
        private Actor[] _actors;
        private OrdinaryEnemyController[] _enemies;
        private Corpse[] _corpses;
        private Transform _corpseRoot;
        private float _playerDeathUntil, _playerAttackUntil, _playerHitUntil, _eliteAttackUntil, _eliteHitUntil;
        public string PlayerState { get; private set; }
        public int ActiveShutdownCount { get; private set; }

        public void Initialize(S01SceneCompositionRoot root, GravityLashVfxPool lash, S15VisualCatalog catalog)
        {
            _root = root; _lash = lash;
            _player = Make(root.PlayerObject.transform); _elite = Make(root.MagnetarGuard.transform);
            _enemies = root.EnemyPopulation.GetComponentsInChildren<OrdinaryEnemyController>(true);
            _actors = new Actor[_enemies.Length];
            for (var i = 0; i < _actors.Length; i++) _actors[i] = Make(_enemies[i].transform);
            _corpseRoot = new GameObject("Visual Slice Pooled Shutdowns").transform;
            _corpseRoot.SetParent(transform, false);
            // Death meshes outlive immediate authoritative recycle, without delaying rewards or respawns.
            _corpses = new Corpse[8];
            for (var i = 0; i < _corpses.Length; i++)
            {
                var id = i < 4 ? "scout-drone" : "cutter-unit";
                catalog.TryGetEnemy(id, out var recipe);
                if (recipe?.PresentationPrefab == null) continue;
                var obj = Instantiate(recipe.PresentationPrefab, _corpseRoot, false);
                var animator = obj.GetComponentInChildren<Animator>(true);
                if (animator == null) { Destroy(obj); continue; }
                obj.name = id + " pooled shutdown"; obj.SetActive(false);
                _corpses[i] = new Corpse { Id = id, Object = obj, Animator = animator };
            }
            lash.CuePlayed += Lash;
            root.PlayerHealth.Damaged += PlayerDamaged; root.PlayerHealth.Died += PlayerDied;
            root.EnemyPopulation.EnemyDied += EnemyDied;
            root.MagnetarGuard.TelegraphStarted += EliteAttack; root.MagnetarGuard.Damaged += EliteDamaged;
            root.MagnetarGuard.Activated += EliteActivated;
        }
        private static Actor Make(Transform authority) => new Actor
        { Binding = authority.GetComponent<CharacterVisualBinding>(), Position = authority.position };
        private void Lash(GravityLashCue cue, Vector3 position)
        {
            if (cue != GravityLashCue.Windup) return;
            _playerAttackUntil = Time.time + .65f;
            _player.State = 0; _player.Pose(Attack);
        }
        private void PlayerDamaged(DamageResult result) { if (!result.WasLethal) _playerHitUntil = Time.time + .28f; }
        private void PlayerDied(PlayerDeathEvent value) { _playerDeathUntil = Time.time + .85f; _player.State = 0; _player.Pose(Death); }
        private void EliteAttack(EliteShockwaveTelegraphEvent value) { _eliteAttackUntil = Time.time + value.Duration; _elite.State = 0; }
        private void EliteDamaged(DamageResult value) { if (!value.WasLethal) _eliteHitUntil = Time.time + .28f; }
        private void EliteActivated(MagnetarGuardActivatedEvent value) { _elite.State = 0; _elite.Pose(Idle); }
        private void EnemyDied(EnemyDeathEvent value)
        {
            for (var i = 0; i < _corpses.Length; i++)
            {
                var slot = _corpses[i];
                if (slot == null || slot.Id != value.EnemyId || slot.Until > Time.time) continue;
                var rotation = Quaternion.identity;
                for (var e = 0; e < _enemies.Length; e++)
                    if (_enemies[e].LifeId.Equals(value.LifeId)) { rotation = _enemies[e].transform.rotation; break; }
                slot.Object.transform.SetPositionAndRotation(value.Position, rotation);
                slot.Object.SetActive(true); slot.Animator.Rebind(); slot.Animator.Play(Death, 0, 0);
                slot.Until = Time.time + .95f; return;
            }
        }
        private void LateUpdate() => Tick();
        public void Tick()
        {
            if (_root == null) return;
            var position = _root.PlayerObject.transform.position;
            var moving = (position - _player.Position).sqrMagnitude > .000001f;
            var state = Time.time < _playerDeathUntil ? Death : Time.time < _playerHitUntil ? Hit :
                Time.time < _playerAttackUntil ? Attack : moving ? Run : Idle;
            _player.Pose(state); _player.Position = position;
            PlayerState = state == Death ? "Death" : state == Hit ? "Hit" : state == Attack ? "Attack" : state == Run ? "Run" : "Idle";
            for (var i = 0; i < _actors.Length; i++)
            {
                var actor = _actors[i]; var enemy = _enemies[i];
                if (!enemy.IsAlive || actor.Binding == null) continue;
                actor.Refresh(); position = enemy.transform.position;
                if (!actor.Life.Equals(enemy.LifeId))
                {
                    actor.Life = enemy.LifeId; actor.Hp = enemy.CurrentHitPoints; actor.Cooldown = enemy.AttackCooldown;
                    actor.ReactionUntil = 0; actor.State = 0; actor.Position = position;
                }
                if (enemy.CurrentHitPoints < actor.Hp) actor.ReactionUntil = Time.time + .23f;
                if (enemy.AttackCooldown > actor.Cooldown + .01f) { actor.ReactionUntil = Time.time + .52f; actor.State = 0; }
                state = Time.time < actor.ReactionUntil ? (enemy.CurrentHitPoints < actor.Hp || actor.State == Hit ? Hit : Attack) :
                    (position - actor.Position).sqrMagnitude > .000001f ? Run : Idle;
                actor.Pose(state); actor.Position = position; actor.Hp = enemy.CurrentHitPoints; actor.Cooldown = enemy.AttackCooldown;
            }
            _elite.Pose(_root.MagnetarGuard.State == MagnetarGuardState.Dead ? Death : Time.time < _eliteHitUntil ? Hit :
                Time.time < _eliteAttackUntil ? Attack : Idle);
            ActiveShutdownCount = 0;
            for (var i = 0; i < _corpses.Length; i++)
            {
                var slot = _corpses[i]; if (slot == null) continue;
                if (slot.Until <= Time.time) slot.Object.SetActive(false);
                else ActiveShutdownCount++;
            }
        }
        private void OnDestroy()
        {
            if (_lash != null) _lash.CuePlayed -= Lash;
            if (_root == null) return;
            if (_root.PlayerHealth != null) { _root.PlayerHealth.Damaged -= PlayerDamaged; _root.PlayerHealth.Died -= PlayerDied; }
            if (_root.EnemyPopulation != null) _root.EnemyPopulation.EnemyDied -= EnemyDied;
            if (_root.MagnetarGuard != null)
            {
                _root.MagnetarGuard.TelegraphStarted -= EliteAttack; _root.MagnetarGuard.Damaged -= EliteDamaged;
                _root.MagnetarGuard.Activated -= EliteActivated;
            }
        }
    }
}
