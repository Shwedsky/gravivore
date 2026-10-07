using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Encounters;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.AudioVfx;
using Gravivore.Presentation.Composition;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gravivore.Presentation.Combat
{
    /// <summary>Cached cosmetic motion and short weapon cues. Gameplay roots/sensors remain authoritative.</summary>
    public sealed class VisualSliceCombatMotion : MonoBehaviour
    {
        private sealed class Actor
        {
            public CharacterVisualBinding Binding;
            public Transform Authority, Model;
            public Vector3 RestPosition, BeforePull;
            public Quaternion RestRotation;
            public EnemyLifeId Life, PendingLife;
            public float Cooldown, CueRemaining;
            public bool PendingPull, Cutter;
            public BoundedPullTrail Trail;
            public LineRenderer PullLine, AttackLine;
            public void Refresh()
            {
                if (Binding.ActiveModel == Model) return;
                if (Model != null) { Model.localPosition = RestPosition; Model.localRotation = RestRotation; }
                Model = Binding.ActiveModel; Trail = default; PendingPull = false;
                if (Model == null) return;
                RestPosition = Model.localPosition; RestRotation = Model.localRotation;
                Cutter = Model.name.StartsWith("Cutter",System.StringComparison.Ordinal);
            }
        }
        private S01SceneCompositionRoot _root;
        private GravityLashVfxPool _lash;
        private DeviceCorrectionDefinition _settings;
        private OrdinaryEnemyController[] _enemies;
        private Actor[] _actors;
        private Actor _elite;
        public int ActivePullCount { get; private set; }
        public float MaximumActivePullOffset { get; private set; }

        public void Initialize(S01SceneCompositionRoot root, GravityLashVfxPool lash, DeviceCorrectionDefinition settings)
        {
            _root = root; _lash = lash; _settings = settings;
            _enemies = root.EnemyPopulation.GetComponentsInChildren<OrdinaryEnemyController>(true);
            _actors = new Actor[_enemies.Length];
            for (var i = 0; i < _actors.Length; i++)
            {
                _actors[i] = CreateActor(_enemies[i].transform);
                _actors[i].Life = _enemies[i].LifeId; _actors[i].Cooldown = _enemies[i].AttackCooldown;
            }
            _elite = CreateActor(root.MagnetarGuard.transform);
            lash.CuePlayed += Lash; root.MagnetarGuard.ShockwaveResolved += EliteImpact;
        }
        private Actor CreateActor(Transform authority)
        {
            var actor = new Actor { Authority = authority, Binding = authority.GetComponent<CharacterVisualBinding>() };
            actor.Refresh(); actor.PullLine = Line("Pooled gravity drag",2); actor.AttackLine = Line("Pooled mechanical strike",13);
            return actor;
        }
        private LineRenderer Line(string name,int points)
        {
            var obj = new GameObject(name,typeof(LineRenderer)); obj.transform.SetParent(transform,false);
            var line = obj.GetComponent<LineRenderer>(); line.sharedMaterial = _settings.MotionStreakMaterial;
            line.useWorldSpace = true; line.positionCount = points; line.widthMultiplier = .035f;
            line.numCapVertices = 2; line.numCornerVertices = 1; line.enabled = false;
            line.shadowCastingMode = ShadowCastingMode.Off; line.receiveShadows = false;
            line.lightProbeUsage = LightProbeUsage.Off; line.reflectionProbeUsage = ReflectionProbeUsage.Off;
            return line;
        }
        private void Lash(GravityLashCue cue,Vector3 destination)
        {
            if (cue != GravityLashCue.Beam) return;
            for (var i = 0; i < _actors.Length; i++)
                if (ReferenceEquals(_lash.CurrentPresentationTarget,_enemies[i]))
                { _actors[i].PendingLife = _enemies[i].LifeId; MarkPull(_actors[i]); }
            if (ReferenceEquals(_lash.CurrentPresentationTarget,_root.MagnetarGuard)) MarkPull(_elite);
        }
        private static void MarkPull(Actor actor)
        {
            actor.Refresh(); if (actor.Model == null) return;
            actor.BeforePull = actor.Model.position; actor.PendingPull = true;
        }
        private void EliteImpact(EliteShockwaveResolvedEvent value) => _elite.CueRemaining = .24f;
        private void LateUpdate() => Tick(Time.deltaTime);
        public void Tick(float dt)
        {
            if (_actors == null) return;
            ActivePullCount = 0; MaximumActivePullOffset = 0;
            for (var i = 0; i < _actors.Length; i++)
            {
                var actor = _actors[i]; var enemy = _enemies[i]; actor.Refresh();
                if (!enemy.IsAlive || actor.Model == null) { Reset(actor); continue; }
                if (!actor.Life.Equals(enemy.LifeId))
                {
                    actor.Life = enemy.LifeId; actor.Cooldown = enemy.AttackCooldown; actor.Trail = default;
                    actor.PendingPull = actor.PendingPull && actor.PendingLife.Equals(enemy.LifeId); actor.CueRemaining = 0;
                }
                if (enemy.AttackCooldown > actor.Cooldown + .01f) actor.CueRemaining = actor.Cutter ? .22f : .13f;
                actor.Cooldown = enemy.AttackCooldown;
                Pose(actor,dt,false);
            }
            _elite.Refresh();
            if (_root.MagnetarGuard.IsAlive && _elite.Model != null) Pose(_elite,dt,true);
            else Reset(_elite);
        }
        private void Pose(Actor actor,float dt,bool elite)
        {
            if (actor.PendingPull)
            {
                actor.PendingPull = false;
                actor.Trail.Begin(actor.BeforePull - actor.Authority.position,_settings.PullDuration,_settings.PullMaximumOffset);
            }
            actor.Trail.Tick(Mathf.Max(0,dt));
            var offset = actor.Trail.Offset;
            actor.Model.localPosition = actor.RestPosition + actor.Model.parent.InverseTransformVector(offset);
            var lean = actor.Model.parent.InverseTransformDirection(offset);
            actor.Model.localRotation = actor.RestRotation * Quaternion.Euler(lean.z * 5,0,-lean.x * 5);
            actor.PullLine.enabled = actor.Trail.Active && offset.sqrMagnitude > .001f;
            if (actor.Trail.Active)
            {
                ActivePullCount++; MaximumActivePullOffset = Mathf.Max(MaximumActivePullOffset,offset.magnitude);
                actor.PullLine.startColor = new Color(.13f,.62f,.81f,.38f); actor.PullLine.endColor = new Color(.13f,.62f,.81f,0);
                actor.PullLine.SetPosition(0,actor.Model.position+Vector3.up*.65f);
                actor.PullLine.SetPosition(1,actor.Authority.position+Vector3.up*.65f);
            }
            actor.CueRemaining = Mathf.Max(0,actor.CueRemaining-Mathf.Max(0,dt));
            actor.AttackLine.enabled = actor.CueRemaining > 0;
            if (!actor.AttackLine.enabled) return;
            var duration = elite ? .24f : actor.Cutter ? .22f : .13f;
            var fraction = actor.CueRemaining/duration;
            var color = elite ? new Color(1,.55f,.10f,.6f*fraction) : new Color(1,.28f,.12f,.65f*fraction);
            actor.AttackLine.startColor = actor.AttackLine.endColor = color;
            var center = actor.Model.position+Vector3.up*(elite ? .9f : .55f);
            var aim = _root.PlayerObject.transform.position-actor.Authority.position; aim.y = 0;
            var rotation = aim.sqrMagnitude > .001f ? Quaternion.LookRotation(aim) : actor.Authority.rotation;
            actor.AttackLine.loop = elite;
            for (var i = 0; i < 13; i++)
            {
                var u = i/12f;
                Vector3 point;
                if (elite) { var angle = u*Mathf.PI*2; point = center + new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*Mathf.Lerp(.45f,1.1f,1-fraction); }
                else if (actor.Cutter) point = center + rotation * (Quaternion.Euler(0,Mathf.Lerp(-68,68,u),0)*Vector3.forward*.95f);
                else point = center + rotation * Vector3.forward * (u*.82f*(1-fraction));
                actor.AttackLine.SetPosition(i,point);
            }
        }
        private static void Reset(Actor actor)
        {
            actor.Trail = default; actor.PendingPull = false; actor.CueRemaining = 0;
            actor.PullLine.enabled = actor.AttackLine.enabled = false;
            if (actor.Model != null) { actor.Model.localPosition = actor.RestPosition; actor.Model.localRotation = actor.RestRotation; }
        }
        private void OnDestroy()
        {
            if (_lash != null) _lash.CuePlayed -= Lash;
            if (_root != null && _root.MagnetarGuard != null) _root.MagnetarGuard.ShockwaveResolved -= EliteImpact;
        }
    }
}
