using System;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Enemies;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.AudioVfx;
using Gravivore.Presentation.Composition;
using UnityEngine;

namespace Gravivore.Presentation.Combat
{
    /// <summary>Source/travel/impact read-model. Damage remains on the authoritative commit frame.</summary>
    public sealed class AttackCausalityPresenter : MonoBehaviour
    {
        private sealed class Travel
        {
            public Vector3 Target;
            public float Remaining;
            public bool Active, Impact;
        }
        private S01SceneCompositionRoot _root;
        private OrdinaryEnemyController[] _enemies;
        private Transform[] _origins;
        private Action<DamageRequest>[] _handlers;
        private Transform _eliteOrigin, _bossOrigin;
        private Phase6BVfxPool _pool;
        private Travel[] _travel;
        private int _cursor;
        private BossTelegraphEvent _bossWarning;
        private EliteShockwaveTelegraphEvent _eliteWarning;
        public int SourceCount { get; private set; }
        public int TravelCount { get; private set; }
        public int ImpactCount { get; private set; }
        public int Capacity => _pool?.CreatedInstanceCount ?? 0;
        public Vector3 LastSource { get; private set; }
        public Vector3 LastTarget { get; private set; }
        public void Initialize(S01SceneCompositionRoot root, Phase6BVfxInstance charge, Phase6BVfxInstance travel, Phase6BVfxInstance impact)
        {
            _root = root;
            _pool = gameObject.AddComponent<Phase6BVfxPool>();
            _pool.Initialize(new[] { new Phase6BVfxPool.Binding(Phase6BVfxCue.HostileCharge, charge, 4),
                new Phase6BVfxPool.Binding(Phase6BVfxCue.HostileTravel, travel, 6), new Phase6BVfxPool.Binding(Phase6BVfxCue.HostileImpact, impact, 6) });
            _travel = new Travel[6]; for(var i=0;i<_travel.Length;i++) _travel[i]=new Travel();
            _enemies=root.EnemyPopulation.GetComponentsInChildren<OrdinaryEnemyController>(true);
            _origins=new Transform[_enemies.Length]; _handlers=new Action<DamageRequest>[_enemies.Length];
            for(var i=0;i<_enemies.Length;i++)
            {
                var index=i;_origins[i]=FindSource(_enemies[i].transform,_enemies[i].TargetPoint);
                _handlers[i]=request=>OrdinaryAttack(index);_enemies[i].AttackRequested+=_handlers[i];
            }
            _eliteOrigin=FindSource(root.MagnetarGuard.transform,root.MagnetarGuard.TargetPoint);
            _bossOrigin=FindSource(root.CustodianBoss.transform,root.CustodianBoss.TargetPoint);
            root.MagnetarGuard.BasicAttackStarted+=EliteBasicStart;root.MagnetarGuard.BasicAttackResolved+=EliteBasicResolve;
            root.CustodianBoss.BasicAttackStarted+=BossBasicStart;root.CustodianBoss.BasicAttackResolved+=BossBasicResolve;
            root.MagnetarGuard.TelegraphStarted+=EliteWarning;root.MagnetarGuard.ShockwaveResolved+=EliteSpecial;
            root.CustodianBoss.TelegraphStarted+=BossWarning;root.CustodianBoss.AttackResolved+=BossSpecial;
            root.CustodianBoss.EncounterReset+=BossReset;
        }
        private static Transform FindSource(Transform owner, Transform fallback)
        {
            var binding=owner.GetComponent<CharacterVisualBinding>();
            if(binding?.ActiveModel==null)return fallback;
            foreach(var node in binding.ActiveModel.GetComponentsInChildren<Transform>(true))
                if(node.name=="AttackOrigin")return node;
            return fallback;
        }
        private void Source(Vector3 position,float duration)
        { SourceCount++;LastSource=position;_pool.TryPlay(Phase6BVfxCue.HostileCharge,position,position,duration); }
        private void Release(Vector3 source,Vector3 target,bool impact)
        {
            LastSource=source;LastTarget=target;TravelCount++;
            _pool.TryPlay(Phase6BVfxCue.HostileTravel,source,target);
            var slot=_travel[_cursor++%_travel.Length];slot.Target=target;slot.Remaining=.14f;slot.Active=true;slot.Impact=impact;
        }
        private void OrdinaryAttack(int index)
        {
            if(!_enemies[index].IsAlive)return;
            var source=_origins[index].position;Source(source,.1f);
            Release(source,_root.PlayerObject.transform.position+Vector3.up,true);
        }
        private void EliteBasicStart(EncounterBasicAttackEvent value)=>Source(_eliteOrigin.position,value.Duration);
        private void BossBasicStart(EncounterBasicAttackEvent value)=>Source(_bossOrigin.position,value.Duration);
        private void EliteBasicResolve(EncounterBasicAttackEvent value)=>Release(_eliteOrigin.position,value.Target,value.Hit);
        private void BossBasicResolve(EncounterBasicAttackEvent value)=>Release(_bossOrigin.position,value.Target,value.Hit);
        private void EliteWarning(EliteShockwaveTelegraphEvent value){_eliteWarning=value;Source(_eliteOrigin.position,value.Duration);}
        private void BossWarning(BossTelegraphEvent value){_bossWarning=value;Source(_bossOrigin.position,value.Duration);}
        private void EliteSpecial(EliteShockwaveResolvedEvent value)
        {
            for(var i=0;i<4;i++)
                Release(_eliteOrigin.position,_eliteWarning.Origin+Quaternion.Euler(0,i*90,0)*Vector3.forward*_eliteWarning.Radius+Vector3.up*.25f,true);
        }
        private void BossSpecial(BossAttackResolvedEvent value)
        {
            var source=_bossOrigin.position;
            if(value.Attack==BossAttackType.CirclePulse)
            {
                for(var i=0;i<4;i++)Release(source,_bossWarning.Origin+Quaternion.Euler(0,i*90,0)*Vector3.forward*_bossWarning.Range+Vector3.up*.25f,true);
            }
            else if(value.Attack==BossAttackType.ConeSweep)
            {
                for(var i=-1;i<=1;i++)Release(source,_bossWarning.Origin+Quaternion.Euler(0,i*_bossWarning.HalfAngleDegrees,0)*_bossWarning.Direction*_bossWarning.Range+Vector3.up*.5f,true);
            }
            else Release(source,_bossWarning.Origin+_bossWarning.Direction*_bossWarning.Range+Vector3.up*.5f,true);
        }
        private void BossReset(BossEncounterResetEvent value)
        { _pool.StopAll();foreach(var travel in _travel)travel.Active=false; }
        private void Update()=>Tick(Time.deltaTime);
        public void Tick(float dt)
        {
            if(_travel==null)return;
            foreach(var slot in _travel)
            {
                if(!slot.Active)continue;slot.Remaining-=dt;if(slot.Remaining>0)continue;slot.Active=false;
                if(!slot.Impact)continue;ImpactCount++;_pool.TryPlay(Phase6BVfxCue.HostileImpact,slot.Target,slot.Target);
            }
        }
        private void OnDisable()
        { _pool?.StopAll();if(_travel!=null)foreach(var travel in _travel)travel.Active=false; }
        private void OnDestroy()
        {
            if(_root==null)return;
            for(var i=0;i<_enemies.Length;i++)if(_enemies[i]!=null)_enemies[i].AttackRequested-=_handlers[i];
            var elite=_root.MagnetarGuard;var boss=_root.CustodianBoss;
            if(elite!=null){elite.BasicAttackStarted-=EliteBasicStart;elite.BasicAttackResolved-=EliteBasicResolve;elite.TelegraphStarted-=EliteWarning;elite.ShockwaveResolved-=EliteSpecial;}
            if(boss!=null){boss.BasicAttackStarted-=BossBasicStart;boss.BasicAttackResolved-=BossBasicResolve;boss.TelegraphStarted-=BossWarning;boss.AttackResolved-=BossSpecial;boss.EncounterReset-=BossReset;}
        }
    }
}
