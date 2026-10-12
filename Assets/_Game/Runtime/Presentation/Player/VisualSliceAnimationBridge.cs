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
    [DefaultExecutionOrder(200)]
    public sealed class VisualSliceAnimationBridge : MonoBehaviour
    {
        private static readonly int Idle = Animator.StringToHash("Idle"), Run = Animator.StringToHash("Run"),
            Attack = Animator.StringToHash("Attack"), Hit = Animator.StringToHash("Hit"), Death = Animator.StringToHash("Death"),
            Windup = Animator.StringToHash("Windup"), Release = Animator.StringToHash("Release"), Special = Animator.StringToHash("Special");
        private sealed class Actor
        {
            public HostilePresentationDefinition Profiles;
            public HostileMotionProfile Profile;
            public int IdlePose = Idle, MovePose = Run, AttackPose = Attack, HitPose = Hit, BlockPose = Hit;
            public int WindupPose = Windup, ReleasePose = Release, LinePose, CirclePose, ConePose;
            public float WindupSeconds = 1;
            public float BasicWindupSeconds = 1;
            public int SupportLayer=-1;
            public float GaitPhase;
            private static readonly int GaitParameter=UnityEngine.Animator.StringToHash("IndustrialGaitPhase");
            public CharacterVisualBinding Binding;
            public Transform Model;
            public Animator Animator;
            public int State;
            public float ReactionUntil, Hp, Cooldown;
            public EnemyLifeId Life;
            public Vector3 Position;
            public float RunSeconds = .8f;
            public Quaternion AnimatorRestRotation;
            public FootContact[] Feet;
            public void Refresh()
            {
                if (Model == Binding.ActiveModel) return;
                Model = Binding.ActiveModel;
                Animator = Model != null ? Model.GetComponentInChildren<Animator>(true) : null;
                if (Animator != null)
                {
                    Profile = Profiles?.Get(Animator.runtimeAnimatorController);
                    IdlePose = Profile != null ? Profile.State(Profile.Idle) : Idle;
                    MovePose = Profile != null ? Profile.State(Profile.Move) : Run;
                    AttackPose = Profile != null ? Profile.State(Profile.Attack) : Attack;
                    HitPose = Profile != null ? Profile.State(Profile.Hit) : Hit;
                    BlockPose = Profile != null ? Profile.State(Profile.Block) : Hit;
                    WindupPose = Profile != null ? Profile.State(Profile.Windup) : Windup;
                    ReleasePose = Profile != null ? Profile.State(Profile.Release) : Release;
                    LinePose = Profile != null ? Profile.State(Profile.Line) : Release;
                    CirclePose = Profile != null ? Profile.State(Profile.Circle) : Special;
                    ConePose = Profile != null ? Profile.State(Profile.Cone) : Release;
                    Animator.applyRootMotion = false;
                    SupportLayer=Animator.GetLayerIndex("Industrial moving supports");GaitPhase=0;
                    AnimatorRestRotation = Animator.transform.localRotation;
                    foreach (var clip in Animator.runtimeAnimatorController.animationClips)
                    {
                        if (clip.name == (Profile?.Move ?? "Run")) RunSeconds = clip.length;
                        if (clip.name == (Profile?.Windup ?? "Windup")) WindupSeconds = clip.length;
                        if (clip.name == "Windup") BasicWindupSeconds = clip.length;
                    }
                    var bones=Animator.GetComponentsInChildren<Transform>(true);
                    Transform Bone(string name) { foreach(var bone in bones) if(bone.name==name) return bone; return null; }
                    Feet = new[] { new FootContact(Bone("L_HIP"),Bone("L_KNEE"),Bone("L_ANKLE")),
                        new FootContact(Bone("R_HIP"),Bone("R_KNEE"),Bone("R_ANKLE")) };
                }
                State = 0;
            }
            public void Pose(int state)
            {
                Refresh();
                if (Animator == null || !Animator.gameObject.activeInHierarchy || State == state) return;
                if(Profile!=null && state==AttackPose && Profile.AttackStartNormalizedTime>0)
                    Animator.Play(state,0,Profile.AttackStartNormalizedTime);
                else Animator.CrossFadeInFixedTime(state, .065f, 0, 0);
                State = state;
            }
            public void ResetLife()
            {
                Refresh(); State = 0; ReactionUntil = 0;
                if (Animator != null) { Animator.Rebind(); Animator.Update(0); Animator.speed = 1; }
                GaitPhase=0;
            }
            public void Locomotion(float distance, float dt, bool moving)
            {
                if (Animator == null || Profile == null) return;
                Animator.speed = moving && dt > 0 ? Mathf.Clamp(distance / dt * RunSeconds / Profile.RunCycleDistance, .45f, 2.5f) : 1;
            }
            public void MovingSupports(float distance,float cycle,float dt,bool moving)
            {
                if(Animator==null||SupportLayer<0)return;
                if(moving)GaitPhase=Mathf.Repeat(GaitPhase+distance/Mathf.Max(.1f,cycle),1);
                Animator.SetFloat(GaitParameter,GaitPhase);
                Animator.SetLayerWeight(SupportLayer,Mathf.MoveTowards(Animator.GetLayerWeight(SupportLayer),moving?1:0,Mathf.Max(0,dt)*16));
            }
        }
        private sealed class FootContact
        {
            private readonly Transform _hip,_knee,_ankle;
            private bool _planted;
            private Vector3 _anchor;
            public FootContact(Transform hip,Transform knee,Transform ankle) { _hip=hip; _knee=knee; _ankle=ankle; }
            public float Apply(float phase,float maximum,bool running)
            {
                if(_ankle==null || _hip==null || _knee==null) return 0;
                var stance=running && Mathf.Sin(phase*Mathf.PI*2)<=0;
                if(!stance) { _planted=false; return 0; }
                if(!_planted) { _anchor=_ankle.position; _planted=true; }
                var offset=_anchor-_ankle.position; offset.y=0;
                var envelope=Mathf.Clamp01(-Mathf.Sin(phase*Mathf.PI*2)*3);
                offset=Vector3.ClampMagnitude(offset,maximum)*envelope;
                var a=_hip.position; var b=_knee.position; var c=_ankle.position;
                var target=c+offset; var l1=Vector3.Distance(a,b); var l2=Vector3.Distance(b,c);
                if(l1<.001f || l2<.001f) return 0;
                var distance=Mathf.Clamp(Vector3.Distance(a,target),Mathf.Abs(l1-l2)+.001f,l1+l2-.001f);
                var direction=(target-a).normalized; target=a+direction*distance;
                var pole=Vector3.ProjectOnPlane(b-a,direction).normalized;
                if(pole.sqrMagnitude<.01f) return 0;
                var along=(l1*l1-l2*l2+distance*distance)/(2*distance);
                var kneeTarget=a+direction*along+pole*Mathf.Sqrt(Mathf.Max(0,l1*l1-along*along));
                var ankleRotation=_ankle.rotation;
                _hip.rotation=Quaternion.FromToRotation(b-a,kneeTarget-a)*_hip.rotation;
                _knee.rotation=Quaternion.FromToRotation(_ankle.position-_knee.position,target-_knee.position)*_knee.rotation;
                _ankle.rotation=ankleRotation;
                return offset.magnitude;
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
        private Actor _player, _elite, _boss;
        private Actor[] _actors;
        private OrdinaryEnemyController[] _enemies;
        private Corpse[] _corpses;
        private Transform _corpseRoot;
        private float _playerDeathUntil, _playerAttackUntil, _playerHitUntil, _eliteAttackUntil, _eliteHitUntil;
        private float _bossReleaseUntil, _bossBasicWindupUntil, _bossBasicReleaseUntil, _bossWindupDuration=1, _bossBasicWindupDuration=1;
        private BossAttackType _bossAttack;
        private bool _eliteSpecial;
        private float _eliteReleaseUntil;
        private HostilePresentationDefinition _hostileProfiles;
        public string PlayerState { get; private set; }
        public int ActiveShutdownCount { get; private set; }
        private Gravivore.Presentation.Combat.PostDevicePresentationDefinition _locomotionSettings;
        private float _visualSpeed;
        public float PlayerRunPlaybackRate { get; private set; } = 1;
        public float MaximumFootContactOffset { get; private set; }
        public void ConfigureLocomotion(Gravivore.Presentation.Combat.PostDevicePresentationDefinition settings) => _locomotionSettings=settings;

        public void Initialize(S01SceneCompositionRoot root, GravityLashVfxPool lash, S15VisualCatalog catalog)
        {
            _root = root; _lash = lash; _hostileProfiles = catalog.HostilePresentation;
            _player = Make(root.PlayerObject.transform); _elite = Make(root.MagnetarGuard.transform);
            _boss = Make(root.CustodianBoss.transform);
            _enemies = root.EnemyPopulation.GetComponentsInChildren<OrdinaryEnemyController>(true);
            _actors = new Actor[_enemies.Length];
            for (var i = 0; i < _actors.Length; i++) _actors[i] = Make(_enemies[i].transform);
            _corpseRoot = new GameObject("Visual Slice Pooled Shutdowns").transform;
            _corpseRoot.SetParent(transform, false);
            // Death meshes outlive immediate authoritative recycle, without delaying rewards or respawns.
            var ids = new[] { "scout-drone", "cutter-unit", "warden", "arc-drone", "carrier" };
            _corpses = new Corpse[ids.Length * 4];
            for (var i = 0; i < _corpses.Length; i++)
            {
                var id = ids[i / 4];
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
            root.CustodianBoss.EncounterReset += BossReset;
            root.CustodianBoss.TelegraphStarted += BossWindup;
            root.CustodianBoss.AttackResolved += BossRelease;
            root.CustodianBoss.BasicAttackStarted += BossBasicWindup;
            root.CustodianBoss.BasicAttackResolved += BossBasicRelease;
            root.MagnetarGuard.BasicAttackStarted += EliteBasic;
            root.MagnetarGuard.ShockwaveResolved += EliteRelease;
            root.MagnetarGuard.ShockwaveCancelled += EliteCancelled;
        }
        private Actor Make(Transform authority) => new Actor
        { Binding = authority.GetComponent<CharacterVisualBinding>(), Position = authority.position, Profiles = _hostileProfiles };
        private void Lash(GravityLashCue cue, Vector3 position)
        {
            if (cue == GravityLashCue.Cancelled) { _playerAttackUntil = 0; return; }
            if (cue != GravityLashCue.Windup) return;
            _playerAttackUntil = Time.time + .65f;
            _player.State = 0; _player.Pose(Attack);
        }
        private void PlayerDamaged(DamageResult result) { if (!result.WasLethal) _playerHitUntil = Time.time + .28f; }
        private void PlayerDied(PlayerDeathEvent value) { _playerDeathUntil = Time.time + .85f; _player.State = 0; _player.Pose(Death);if(_player.SupportLayer>=0)_player.Animator.SetLayerWeight(_player.SupportLayer,0); }
        private void EliteAttack(EliteShockwaveTelegraphEvent value)
        { _eliteSpecial = true; _eliteAttackUntil = Time.time + value.Duration; _elite.State = 0; _elite.Refresh();
          if (_elite.Animator != null) _elite.Animator.speed = _elite.WindupSeconds / Mathf.Max(.01f, value.Duration); }
        private void EliteBasic(EncounterBasicAttackEvent value) { _eliteSpecial=false;_eliteAttackUntil=Time.time+value.Duration+.25f;_elite.State=0; }
        private void EliteRelease(EliteShockwaveResolvedEvent value) { _eliteAttackUntil=0;_eliteReleaseUntil=Time.time+.5f;_elite.State=0; }
        private void EliteCancelled(EliteShockwaveCancelledEvent value) { _eliteAttackUntil=0;_eliteReleaseUntil=0;_eliteSpecial=false; }
        private void BossBasicWindup(EncounterBasicAttackEvent value)
        { _bossBasicWindupDuration=Mathf.Max(.01f,value.Duration);_bossBasicWindupUntil=Time.time+value.Duration;_bossBasicReleaseUntil=0;_boss.State=0; }
        private void BossBasicRelease(EncounterBasicAttackEvent value)
        { _bossBasicWindupUntil=0;_bossBasicReleaseUntil=Time.time+.35f;_bossReleaseUntil=_bossBasicReleaseUntil;_bossAttack=BossAttackType.LineCharge;_boss.State=0; }
        private void EliteDamaged(DamageResult value) { if (!value.WasLethal) _eliteHitUntil = Time.time + .28f; }
        private void EliteActivated(MagnetarGuardActivatedEvent value) { _elite.ResetLife(); _eliteReleaseUntil=0;_eliteAttackUntil=0;_elite.Pose(_elite.IdlePose); }
        private void BossReset(BossEncounterResetEvent value)
        { _boss.ResetLife(); _bossReleaseUntil=0; _bossBasicWindupUntil=0;_bossBasicReleaseUntil=0;_boss.Pose(_boss.IdlePose); }
        private void BossWindup(BossTelegraphEvent value)
        { _bossAttack=value.Attack;_bossWindupDuration=Mathf.Max(.01f,value.Duration);_bossReleaseUntil=0;_bossBasicReleaseUntil=0;_bossBasicWindupUntil=0;_boss.State=0; }
        private void BossRelease(BossAttackResolvedEvent value)
        { _bossAttack=value.Attack;_bossReleaseUntil=Time.time+.5f;_bossBasicReleaseUntil=0;_boss.State=0; }
        private void EnemyDied(EnemyDeathEvent value)
        {
            for (var i = 0; i < _corpses.Length; i++)
            {
                var slot = _corpses[i];
                if (slot == null || slot.Id != value.EnemyId || slot.Until > Time.time) continue;
                var rotation = Quaternion.identity;var corpsePosition=value.Position;
                for (var e = 0; e < _enemies.Length; e++)
                    if (_enemies[e].LifeId.Equals(value.LifeId))
                    {
                        var model=_enemies[e].GetComponent<CharacterVisualBinding>().ActiveModel;
                        rotation=model!=null?model.rotation:_enemies[e].transform.rotation;
                        corpsePosition=model!=null?model.position:value.Position;break;
                    }
                slot.Object.transform.SetPositionAndRotation(corpsePosition, rotation);
                slot.Object.SetActive(true); slot.Animator.Rebind(); slot.Animator.Play(Death, 0, 0);
                slot.Until = Time.time + .95f; return;
            }
        }
        private void LateUpdate() => Tick();
        public void Tick() => Tick(Time.deltaTime);
        public void Tick(float deltaTime)
        {
            if (_root == null) return;
            var position = _root.PlayerObject.transform.position;
            var delta=position-_player.Position; delta.y=0;
            if(delta.sqrMagnitude>4) delta=Vector3.zero;
            var moving = delta.sqrMagnitude > .000001f;
            var state = Time.time < _playerDeathUntil ? Death : Time.time < _playerHitUntil ? Hit :
                Time.time < _playerAttackUntil ? Attack : moving ? Run : Idle;
            _player.Pose(state); _player.Position = position;
            if(_locomotionSettings!=null && _player.Animator!=null)
            {
                var dt=Mathf.Max(.0001f,deltaTime);
                var velocity=delta/dt;
                _visualSpeed=Mathf.MoveTowards(_visualSpeed,velocity.magnitude, _root.PlayerStats.MoveSpeed*dt/_locomotionSettings.LocomotionBlendSeconds);
                PlayerRunPlaybackRate=state==Run ? Mathf.Clamp(_visualSpeed*_player.RunSeconds/_locomotionSettings.RunCycleDistance,.25f,3f) : 1;
                _player.Animator.speed=PlayerRunPlaybackRate;
                _player.MovingSupports(delta.magnitude,_locomotionSettings.RunCycleDistance,deltaTime,moving&&state!=Death);
                var lean=state==Run?_player.Model.InverseTransformDirection(velocity)/Mathf.Max(1,_root.PlayerStats.MoveSpeed):Vector3.zero;
                _player.Animator.transform.localRotation=Quaternion.Euler(Mathf.Clamp(lean.z*2,-2,2),0,Mathf.Clamp(-lean.x*2,-2,2))*_player.AnimatorRestRotation;
                var animation=_player.Animator.GetCurrentAnimatorStateInfo(0);
                MaximumFootContactOffset=0;
                for(var i=0;i<_player.Feet.Length;i++) MaximumFootContactOffset=Mathf.Max(MaximumFootContactOffset,
                    _player.Feet[i].Apply((_player.SupportLayer>=0?_player.GaitPhase:animation.normalizedTime)+i*.5f,_locomotionSettings.FootContactCorrection,moving&&state!=Death&&(_player.SupportLayer>=0||state==Run&&animation.shortNameHash==Run)));
            }
            PlayerState = state == Death ? "Death" : state == Hit ? "Hit" : state == Attack ? "Attack" : state == Run ? "Run" : "Idle";
            for (var i = 0; i < _actors.Length; i++)
            {
                var actor = _actors[i]; var enemy = _enemies[i];
                if (!enemy.IsAlive || actor.Binding == null) continue;
                actor.Refresh(); position = enemy.transform.position;
                if (!actor.Life.Equals(enemy.LifeId))
                {
                    actor.Life = enemy.LifeId; actor.Hp = enemy.CurrentHitPoints; actor.Cooldown = enemy.AttackCooldown;
                    actor.ResetLife(); actor.Position = position;
                }
                if (enemy.CurrentHitPoints < actor.Hp) actor.ReactionUntil = Time.time + .23f;
                if (enemy.AttackCooldown > actor.Cooldown + .01f) { actor.ReactionUntil = Time.time + (actor.Profile?.AttackRecovery ?? .52f); actor.State = 0; }
                var actorDistance = Vector3.Distance(position, actor.Position);
                state = Time.time < actor.ReactionUntil ? (enemy.CurrentHitPoints < actor.Hp || actor.State == actor.HitPose ? actor.HitPose : actor.AttackPose) :
                    actorDistance > .001f ? actor.MovePose :
                    actor.BlockPose != Hit && enemy.BrainState == OrdinaryEnemyBrainState.Attack ? actor.BlockPose : actor.IdlePose;
                actor.Locomotion(actorDistance, deltaTime, state == actor.MovePose);
                actor.Pose(state); actor.Position = position; actor.Hp = enemy.CurrentHitPoints; actor.Cooldown = enemy.AttackCooldown;
            }
            _elite.Refresh();
            var eliteDistance=Vector3.Distance(_root.MagnetarGuard.transform.position,_elite.Position);
            var elitePose=_root.MagnetarGuard.State == MagnetarGuardState.Dead ? Death : Time.time < _eliteAttackUntil ?
                (_eliteSpecial && _elite.Profile != null ? _elite.WindupPose : _elite.AttackPose) :
                Time.time < _eliteReleaseUntil && _elite.Profile != null ? _elite.ReleasePose :
                Time.time < _eliteHitUntil ? _elite.HitPose : eliteDistance>.001f?_elite.MovePose:_elite.IdlePose;
            _elite.Pose(elitePose);
            if (_elite.Animator != null && Time.time >= _eliteAttackUntil) _elite.Animator.speed=1;
            if(elitePose==_elite.MovePose)_elite.Locomotion(eliteDistance,deltaTime,true);
            _elite.MovingSupports(eliteDistance,_elite.Profile?.RunCycleDistance??1.4f,deltaTime,eliteDistance>.001f&&elitePose!=Death);
            _elite.Position=_root.MagnetarGuard.transform.position;
            var boss = _root.CustodianBoss;
            if (boss.CurrentHitPoints < _boss.Hp) _boss.ReactionUntil = Time.time + .23f;
            _boss.Refresh();
            var authoredBoss=_boss.Animator!=null && _boss.Animator.HasState(0,Windup);
            var movingBoss=(boss.transform.position-_boss.Position).sqrMagnitude>.000001f;
            var pose=CustodianPresentationSelector.Select(boss.State,_bossAttack,Time.time<_boss.ReactionUntil,Time.time<_bossReleaseUntil,movingBoss);
            var bossState=pose==CustodianPresentationPose.Death?Death:pose==CustodianPresentationPose.Hit?Hit:
                pose==CustodianPresentationPose.Windup?(authoredBoss?Windup:Attack):
                pose==CustodianPresentationPose.Special?(authoredBoss?Special:Attack):
                pose==CustodianPresentationPose.Release?(authoredBoss?Release:Attack):pose==CustodianPresentationPose.Run?Run:Idle;
            if(Time.time<_bossBasicWindupUntil && bossState!=Death && bossState!=Hit)bossState=authoredBoss?Windup:Attack;
            if (_boss.Profile != null)
            {
                var phasePose = _bossAttack == BossAttackType.LineCharge ? _boss.LinePose :
                    _bossAttack == BossAttackType.CirclePulse ? _boss.CirclePose : _boss.ConePose;
                bossState = pose == CustodianPresentationPose.Death ? Death :
                    pose == CustodianPresentationPose.Windup ? _boss.WindupPose :
                    Time.time < _bossBasicWindupUntil ? Windup :
                    Time.time < _bossBasicReleaseUntil ? _boss.AttackPose :
                    pose == CustodianPresentationPose.Release || pose == CustodianPresentationPose.Special ? phasePose :
                    pose == CustodianPresentationPose.Hit ? _boss.HitPose :
                    pose == CustodianPresentationPose.Run ? _boss.MovePose : _boss.IdlePose;
            }
            var bossDistance=Vector3.Distance(boss.transform.position,_boss.Position);
            _boss.Pose(bossState); _boss.Hp=boss.CurrentHitPoints; _boss.Position=boss.transform.position;
            if(_boss.Animator!=null)_boss.Animator.speed=authoredBoss&&pose==CustodianPresentationPose.Windup?_boss.WindupSeconds/_bossWindupDuration:1;
            if(bossState==_boss.MovePose)_boss.Locomotion(bossDistance,deltaTime,true);
            _boss.MovingSupports(bossDistance,_boss.Profile?.RunCycleDistance??1.4f,deltaTime,movingBoss&&bossState!=Death);
            if(_boss.Profile!=null && _boss.Animator!=null && Time.time<_bossBasicWindupUntil && bossState==Windup)
                _boss.Animator.speed=_boss.BasicWindupSeconds/_bossBasicWindupDuration;
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
            if (_root.CustodianBoss != null) _root.CustodianBoss.EncounterReset -= BossReset;
            if (_root.CustodianBoss != null)
            {
                _root.CustodianBoss.TelegraphStarted -= BossWindup;
                _root.CustodianBoss.AttackResolved -= BossRelease;
                _root.CustodianBoss.BasicAttackStarted -= BossBasicWindup;
                _root.CustodianBoss.BasicAttackResolved -= BossBasicRelease;
            }
            if (_root.MagnetarGuard != null)
            {
                _root.MagnetarGuard.TelegraphStarted -= EliteAttack; _root.MagnetarGuard.Damaged -= EliteDamaged;
                _root.MagnetarGuard.Activated -= EliteActivated;
                _root.MagnetarGuard.BasicAttackStarted -= EliteBasic;
                _root.MagnetarGuard.ShockwaveResolved -= EliteRelease;
                _root.MagnetarGuard.ShockwaveCancelled -= EliteCancelled;
            }
        }
    }
}
