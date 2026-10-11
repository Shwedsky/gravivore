using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.Camera;
using Gravivore.Presentation.Combat;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Profiling;
using Object=UnityEngine.Object;

namespace Gravivore.Tests.PlayMode
{
    public sealed class Chapter01ActorIntegrationSmokeTests
    {
        private const string Output="docs/history/implementation-passes/chapter01-r2-actors-v2-integration";
        private CanonicalSceneTestScope _scene;
        private static readonly string[] Names={"Scout","Cutter","Warden","ArcDrone","Carrier"};
        [UnityTearDown] public IEnumerator Cleanup(){if(_scene!=null)yield return _scene.Cleanup();}
        private IEnumerator Load()
        {
            _scene=new CanonicalSceneTestScope();yield return _scene.Load();var root=_scene.Root;
            root.PlayerHealth.SetDevelopmentGodMode(true);
            root.PlayerObject.GetComponent<PlayerLocomotion>().enabled=false;
            root.PlayerObject.GetComponent<GravityAttackController>().enabled=false;
            root.EnemyPopulation.enabled=false;root.MagnetarGuard.enabled=false;root.CustodianBoss.enabled=false;
            foreach(var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true))enemy.enabled=false;
        }
        [UnityTest] public IEnumerator EveryOrdinaryRoleAttacksDiesAndRespawnsWithFreshAnimationAndSockets()
        {
            yield return Load();var root=_scene.Root;
            var causality=root.GetComponent<AttackCausalityPresenter>();var bridge=root.GetComponent<VisualSliceAnimationBridge>();
            var lives=new HashSet<EnemyLifeId>();var records=new List<string>();
            for(var i=0;i<5;i++)
            {
                var spot=root.EnemyPopulation.GetSpot(i);var enemy=spot.GetLiveEnemy(0);
                var binding=enemy.GetComponent<CharacterVisualBinding>();Assert.That(binding.ActiveModel.name,Is.EqualTo(Names[i]+"_V2"));
                Move(root,enemy.transform.position+Vector3.forward*.8f);
                var sourceCount=causality.SourceCount;
                for(var frame=0;frame<35;frame++){enemy.Tick(.05f);bridge.Tick(.05f);yield return null;}
                Assert.That(causality.SourceCount,Is.GreaterThan(sourceCount),Names[i]+" actual AttackRequested");
                Assert.That(Vector3.Distance(causality.LastSource,binding.GetSocketOr(PresentationSocket.AttackOrigin,enemy.TargetPoint).position),Is.LessThan(2f));
                records.Add(Names[i]+" attack and source PASS");
                while(spot.LiveCount>0)
                {
                    enemy=spot.GetLiveEnemy(0);lives.Add(enemy.LifeId);
                    enemy.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));
                }
                bridge.Tick();Assert.That(bridge.ActiveShutdownCount,Is.GreaterThan(0));yield return null;
            }
            Move(root,new Vector3(0,0,-30));
            for(var tick=0;tick<12;tick++)root.EnemyPopulation.Tick(60);
            causality.Tick(0);
            for(var i=0;i<5;i++)
            {
                var spot=root.EnemyPopulation.GetSpot(i);Assert.That(spot.LiveCount,Is.EqualTo(4));
                var enemy=spot.GetLiveEnemy(0);Assert.IsFalse(lives.Contains(enemy.LifeId));
                Move(root,enemy.transform.position+Vector3.forward*.8f);causality.Tick(0);
                var before=causality.SourceCount;
                for(var frame=0;frame<35;frame++){enemy.Tick(.05f);bridge.Tick(.05f);yield return null;}
                Assert.That(causality.SourceCount,Is.GreaterThan(before),Names[i]+" source must survive recycle");
                var animator=enemy.GetComponent<CharacterVisualBinding>().ActiveModel.GetComponentInChildren<Animator>();
                Assert.IsFalse(animator.GetCurrentAnimatorStateInfo(0).IsName("Death"));
                records.Add(Names[i]+" respawn animation/source PASS");
            }
            Assert.IsTrue(root.FlushNow());
            Directory.CreateDirectory(Output+"/verification");File.WriteAllLines(Output+"/verification/death-respawn.txt",records);
        }
        [UnityTest,Timeout(180000)] public IEnumerator ActualPortraitGalleryAndAnimatedWorldClearance()
        {
            yield return Load();var root=_scene.Root;var records=new List<Clearance>();
            var blockers=root.WorldPresenter.GameplayRoot.GetComponentsInChildren<BoxCollider>(true)
                .Where(c=>c.gameObject.layer==LayerMask.NameToLayer("HardBlocker")).ToArray();
            for(var i=0;i<5;i++)
            {
                var enemy=root.EnemyPopulation.GetSpot(i).GetLiveEnemy(0);
                var binding=enemy.GetComponent<CharacterVisualBinding>();var original=enemy.transform.position;
                Move(root,original+Vector3.back*3);yield return null;
                Capture(root,Names[i]+"_normal");
                var profile=Profile(binding);var animator=binding.ActiveModel.GetComponentInChildren<Animator>();animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                for(var heading=0;heading<4;heading++)
                {
                    enemy.transform.rotation=Quaternion.Euler(0,heading*90,0);
                    animator.Play(profile.Idle,0,.25f);animator.Update(0);Capture(root,Names[i]+"_heading_"+heading);
                    foreach(var state in new[]{profile.Idle,profile.Move,profile.Attack,"Hit","Death",profile.Block}.Distinct())
                        for(var frame=0;frame<4;frame++)
                        {
                            animator.Play(state,0,frame*.25f);animator.Update(0);
                            records.Add(CheckMesh(Names[i],state,"spawn-turn",binding,blockers));
                        }
                }
                enemy.transform.rotation=Quaternion.identity;
                animator.Play(profile.Attack,0,.5f);animator.Update(0);Capture(root,Names[i]+"_attack");
                if(Names[i]=="Warden"){animator.Play(profile.Block,0,.5f);animator.Update(0);Capture(root,"Warden_Block");}
                Move(root,original+Vector3.back*.8f);
                for(var frame=0;frame<30;frame++){enemy.Tick(.05f);root.GetComponent<VisualSliceAnimationBridge>().Tick(.05f);yield return null;}
                Capture(root,Names[i]+"_combat");
                var nearest=blockers.Where(b=>b.enabled).OrderBy(b=>(b.ClosestPoint(original+Vector3.up*.5f)-original-Vector3.up*.5f).sqrMagnitude).First();
                var contact=nearest.ClosestPoint(original+Vector3.up*.5f);var away=original+Vector3.up*.5f-contact;away.y=0;away.Normalize();
                var wallStart=contact+away*(enemy.CollisionRadius+.12f);wallStart.y=original.y;
                var body=enemy.GetComponent<CharacterController>();body.enabled=false;enemy.transform.position=wallStart;body.enabled=true;
                Move(root,contact-away*2);Physics.SyncTransforms();
                for(var frame=0;frame<32;frame++){enemy.Tick(.05f);root.GetComponent<VisualSliceAnimationBridge>().Tick(.05f);yield return null;}
                Move(root,enemy.transform.position+Vector3.back*3);yield return null;Capture(root,Names[i]+"_near_wall");
                records.Add(CheckMesh(Names[i],profile.Move,"wall-pursuit",binding,blockers));
                body.enabled=false;enemy.transform.position=original;body.enabled=true;Move(root,original+Vector3.back*3);
                enemy.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));yield return null;Capture(root,Names[i]+"_death");
            }
            root.WorldUnlocks.PrepareEliteEncounterForDevelopment();root.Chapter1Encounters.Tick();
            Move(root,root.MagnetarGuard.transform.position+Vector3.back*4);root.MagnetarGuard.Tick(.01f);
            yield return null;Capture(root,"Magnetar_normal");
            var eliteBinding=root.MagnetarGuard.GetComponent<CharacterVisualBinding>();
            SampleEncounter(root,"Magnetar",eliteBinding,blockers,records,new[]{"Idle","Run","Attack","Charge","Vent","Hit","Death"});
            root.MagnetarGuard.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));
            yield return null;Capture(root,"Magnetar_death");
            Move(root,root.CustodianBoss.transform.position+Vector3.back*4);root.CustodianBoss.Tick(.01f);
            yield return null;Capture(root,"Custodian_normal");
            var bossBinding=root.CustodianBoss.GetComponent<CharacterVisualBinding>();
            SampleEncounter(root,"Custodian",bossBinding,blockers,records,new[]{"Idle","Run","Attack","Windup","Telegraph","AttackLine","AttackCircle","AttackCone","Hit","Death"});
            root.CustodianBoss.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));
            yield return null;Capture(root,"Custodian_death");
            Directory.CreateDirectory(Output+"/verification");
            File.WriteAllText(Output+"/verification/animated-clearance.json",JsonUtility.ToJson(new ClearanceReport{samples=records.ToArray()},true));
            // Record all diagnostics before failing; a failed envelope gate is never reported as acceptance.
            Assert.IsFalse(records.Any(r=>r.penetratingVertices>0),"Animated presentation intersects R2 blockers; see clearance evidence.");
        }
        [UnityTest,Timeout(240000)] public IEnumerator AllOrdinaryVisualsTraverseRoutesAndEncounterStructuresClearArenaWalls()
        {
            yield return Load();var root=_scene.Root;var layout=root.WorldPresenter.Layout;
            root.WorldUnlocks.PrepareEliteEncounterForDevelopment();root.Chapter1Encounters.Tick();
            root.MagnetarGuard.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));
            var blockers=root.WorldPresenter.GameplayRoot.GetComponentsInChildren<BoxCollider>(true)
                .Where(c=>c.gameObject.layer==LayerMask.NameToLayer("HardBlocker")).ToArray();
            var motion=root.GetComponent<VisualSliceCombatMotion>();var bridge=root.GetComponent<VisualSliceAnimationBridge>();
            var records=new List<Clearance>();var traversal=new List<string>();
            foreach(var controller in root.GetComponentsInChildren<CharacterController>(true))controller.enabled=false;
            for(var actor=0;actor<5;actor++)
            {
                var enemy=root.EnemyPopulation.GetSpot(actor).GetLiveEnemy(0);
                var body=enemy.GetComponent<CharacterController>();body.transform.position=layout.GetRoutePoint(0);body.enabled=true;
                var binding=enemy.GetComponent<CharacterVisualBinding>();var animator=binding.ActiveModel.GetComponentInChildren<Animator>();animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                for(var route=1;route<layout.RoutePointCount;route++)
                {
                    var target=layout.GetRoutePoint(route);var reached=false;
                    for(var step=0;step<1000;step++)
                    {
                        var delta=target-body.transform.position;delta.y=0;
                        if(delta.magnitude<.12f){reached=true;break;}
                        root.PlayerObject.transform.position=body.transform.position+delta.normalized*2.5f;
                        Physics.SyncTransforms();enemy.Tick(.04f);
                        var authority=body.transform.position;motion.Tick(.04f);bridge.Tick(.04f);animator.Update(.04f);
                        Assert.That(body.transform.position,Is.EqualTo(authority),"Visual correction must never move collision authority");
                        if(step%120==0)yield return null;
                    }
                    Assert.IsTrue(reached,Names[actor]+" traversal segment "+route+" from "+body.transform.position+" towards "+target);
                    records.Add(CheckMesh(Names[actor],"Move","route-"+route,binding,blockers));
                    traversal.Add(Names[actor]+" route "+route+" PASS; radius="+enemy.CollisionRadius);
                }
                body.enabled=false;
            }
            SampleArenaEdges(root,"Magnetar",root.MagnetarGuard.transform,root.MagnetarGuard.CollisionRadius,
                new[]{"Idle","Run","Charge","Vent","Hit","Death"},motion,blockers,records);
            SampleArenaEdges(root,"Custodian",root.CustodianBoss.transform,root.CustodianBoss.CollisionRadius,
                new[]{"Idle","Telegraph","AttackLine","AttackCircle","AttackCone","Windup","Death"},motion,blockers,records);
            Directory.CreateDirectory(Output+"/verification");File.WriteAllLines(Output+"/verification/actor-traversal.txt",traversal);
            File.WriteAllText(Output+"/verification/route-arena-clearance.json",JsonUtility.ToJson(new ClearanceReport{samples=records.ToArray()},true));
            Assert.IsFalse(records.Any(r=>r.penetratingVertices>0),"Route/arena animated mesh envelope gate failed");
        }
        private static void SampleArenaEdges(S01SceneCompositionRoot root,string name,Transform actor,float radius,string[] states,
            VisualSliceCombatMotion motion,BoxCollider[] blockers,List<Clearance> records)
        {
            var binding=actor.GetComponent<CharacterVisualBinding>();var start=actor.position;
            var nearest=blockers.Where(b=>b.enabled).OrderBy(b=>(b.ClosestPoint(start+Vector3.up*.55f)-start-Vector3.up*.55f).sqrMagnitude).Take(4).ToArray();
            var wallIndex=0;
            foreach(var wall in nearest)
            {
                var contact=wall.ClosestPoint(start+Vector3.up*.55f);var away=start+Vector3.up*.55f-contact;away.y=0;away.Normalize();
                var wallPosition=contact+away*(radius+.15f);wallPosition.y=start.y;
                // A near face of one wall can be inside a different reactor proxy.
                // Choose a reachable controller centre, never place the fixture through collision.
                var tangent=Vector3.Cross(away,Vector3.up);
                var valid=false;
                foreach(var lateral in new[]{0f,8f,-8f})
                {
                    var candidate=wallPosition+tangent*lateral;
                    if(Physics.CheckCapsule(candidate+Vector3.up*radius,candidate+Vector3.up*1.7f,radius,
                        LayerMask.GetMask("HardBlocker"),QueryTriggerInteraction.Ignore))continue;
                    actor.position=candidate;valid=true;break;
                }
                Assert.IsTrue(valid,"No reachable near-wall fixture for "+wall.name);
                for(var heading=0;heading<4;heading++)
                {
                    actor.rotation=Quaternion.Euler(0,heading*90,0);Physics.SyncTransforms();motion.Tick(2);
                    var animator=binding.ActiveModel.GetComponentInChildren<Animator>();animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                    foreach(var state in states)
                    {
                        animator.Play(state,0,.5f);animator.Update(0);records.Add(CheckMesh(name,state,"arena-wall-"+wallIndex+"-heading-"+heading,binding,blockers));
                        if(wallIndex==0&&heading==2&&state==states[2])
                        {root.PlayerObject.transform.position=actor.position+Vector3.back*4;Capture(root,name+"_near_wall");}
                    }
                }
                wallIndex++;
            }
            actor.position=start;actor.rotation=Quaternion.identity;motion.Tick(2);
        }
        [UnityTest] public IEnumerator LethalGravityReleaseKeepsTheChargedActorHitSocket()
        {
            yield return Load();var root=_scene.Root;var enemy=root.EnemyPopulation.GetSpot(3).GetLiveEnemy(0);
            enemy.enabled=true;var binding=enemy.GetComponent<CharacterVisualBinding>();
            var hit=binding.GetSocketOr(PresentationSocket.HitCenter,enemy.TargetPoint).position;
            var lash=root.GetComponentInChildren<GravityLashVfxPool>();var release=Vector3.zero;var impact=Vector3.zero;
            lash.CuePlayed+=(cue,point)=>{if(cue==GravityLashCue.Beam)release=point;if(cue==GravityLashCue.Impact)impact=point;};
            lash.BeginCharge(root.PlayerObject.transform.position,enemy.TargetPoint.position,enemy,.15f);
            enemy.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));
            lash.Play(root.PlayerObject.transform.position,enemy.TargetPoint.position,enemy);lash.Tick(1f);
            Assert.That(Vector3.Distance(release,hit),Is.LessThan(.001f));
            Assert.That(Vector3.Distance(impact,hit),Is.LessThan(.001f));
            Assert.IsFalse(enemy.IsAlive);
        }
        [UnityTest] public IEnumerator EncounterEventsDriveChargeVentAndEveryBossPhaseAtAuthoritativeTiming()
        {
            yield return Load();var root=_scene.Root;var bridge=root.GetComponent<VisualSliceAnimationBridge>();
            root.WorldUnlocks.PrepareEliteEncounterForDevelopment();root.Chapter1Encounters.Tick();
            var elite=root.MagnetarGuard;Move(root,elite.transform.position+Vector3.back*2);
            elite.Tick(0);bridge.Tick();yield return null;
            var eliteAnimator=elite.GetComponent<CharacterVisualBinding>().ActiveModel.GetComponentInChildren<Animator>();eliteAnimator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            elite.Tick(0);bridge.Tick();eliteAnimator.Update(.1f);
            Assert.IsTrue(eliteAnimator.GetCurrentAnimatorStateInfo(0).IsName("Charge"));
            Capture(root,"Magnetar_event_charge");elite.Tick(2);bridge.Tick();eliteAnimator.Update(.1f);
            Assert.IsTrue(eliteAnimator.GetCurrentAnimatorStateInfo(0).IsName("Vent"));Capture(root,"Magnetar_event_vent");
            elite.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));bridge.Tick();yield return null;
            var boss=root.CustodianBoss;Move(root,boss.transform.position+Vector3.back*4.5f);boss.Tick(0);
            var animator=boss.GetComponent<CharacterVisualBinding>().ActiveModel.GetComponentInChildren<Animator>();animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            var states=new[]{"AttackCircle","AttackCone","AttackLine"};
            for(var i=0;i<3;i++)
            {
                bridge.Tick();animator.Update(.1f);Assert.IsTrue(animator.GetCurrentAnimatorStateInfo(0).IsName("Telegraph"));
                Capture(root,"Custodian_event_telegraph_"+i);
                var hp=root.PlayerHealth.CurrentHitPoints;boss.Tick(.25f);
                Assert.That(root.PlayerHealth.CurrentHitPoints,Is.EqualTo(hp),"Animation cannot move the damage commit frame");
                boss.Tick(2);bridge.Tick();animator.Update(.1f);
                Assert.IsTrue(animator.GetCurrentAnimatorStateInfo(0).IsName(states[i]),states[i]);Capture(root,"Custodian_event_"+states[i]);
                boss.Tick(0);boss.Tick(2);if(i<2)boss.Tick(0);
            }
            boss.ResetEncounter();bridge.Tick();animator.Update(.1f);Assert.IsFalse(animator.GetCurrentAnimatorStateInfo(0).IsName("Death"));
            var completions=0;root.BossCompletion.Defeated+=_=>completions++;
            Move(root,boss.transform.position+Vector3.back*4.5f);boss.Tick(0);boss.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));
            bridge.Tick();animator.Update(.1f);Assert.IsTrue(animator.GetCurrentAnimatorStateInfo(0).IsName("Death"));
            Assert.That(completions,Is.EqualTo(1));Assert.IsTrue(root.FlushNow());
            File.WriteAllText(Output+"/verification/encounter-animation.txt","Magnetar Charge/Vent and Custodian Telegraph/AttackCircle/AttackCone/AttackLine/reset/death/exactly-once completion PASS");
        }
        [UnityTest] public IEnumerator RealLethalGravityAttackKeepsDamageTextAtTheAuthoredAnchor()
        {
            yield return Load();var root=_scene.Root;var enemy=root.EnemyPopulation.GetSpot(0).GetLiveEnemy(0);
            enemy.enabled=true;
            var body=enemy.GetComponent<CharacterController>();body.enabled=false;
            enemy.transform.position=root.PlayerObject.transform.position+Vector3.forward*2;body.enabled=true;Physics.SyncTransforms();
            var expected=enemy.GetComponent<CharacterVisualBinding>().GetSocketOr(PresentationSocket.DamageNumberAnchor,enemy.TargetPoint).position;
            enemy.ApplyDamage(new DamageRequest(enemy.CurrentHitPoints-1,DamageType.Gravity));
            var attack=root.PlayerObject.GetComponent<GravityAttackController>();var feedback=root.CombatReadability.DamageFeedbackCount;
            attack.ResetTransientState();attack.Tick(0);attack.Tick(root.PlayerStats.DerivedStats.AttackInterval+.01f);
            Assert.IsFalse(enemy.IsAlive,"The actual gravity controller must kill and immediately recycle the target");
            Assert.That(root.CombatReadability.DamageFeedbackCount,Is.GreaterThan(feedback));
            Assert.That(Vector3.Distance(root.CombatReadability.LastDamageAnchor,expected),Is.LessThan(.001f));
        }
        [UnityTest,Timeout(60000)] public IEnumerator ActualPlayableEditorPerformanceObservation()
        {
            yield return Load();var root=_scene.Root;
            root.EnemyPopulation.enabled=true;
            foreach(var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true))enemy.enabled=true;
            root.PlayerObject.GetComponent<GravityAttackController>().enabled=true;
            Move(root,root.EnemyPopulation.GetSpot(2).Position+Vector3.back*4);
            for(var frame=0;frame<60;frame++)yield return null;
            using var gc=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame",1);
            using var cpu=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Main Thread",1);
            var frames=240;double frameMs=0,cpuMs=0;long gcBytes=0,maxGc=0;int maxEnemies=0,maxVfx=0;
            for(var frame=0;frame<frames;frame++)
            {
                frameMs+=Time.unscaledDeltaTime*1000;
                if(cpu.Valid)cpuMs+=cpu.LastValue/1000000.0;
                if(gc.Valid){gcBytes+=gc.LastValue;maxGc=Math.Max(maxGc,gc.LastValue);}
                maxEnemies=Math.Max(maxEnemies,root.EnemyPopulation.LiveEnemyCount);
                maxVfx=Math.Max(maxVfx,root.Phase6BCombat.Vfx.ActiveCount);
                yield return null;
            }
            var renderers=root.GetComponentsInChildren<Renderer>(true).Where(r=>r.gameObject.activeInHierarchy&&r.enabled).ToArray();
            var skins=renderers.OfType<SkinnedMeshRenderer>().ToArray();
            var lights=root.GetComponentsInChildren<Light>(true).Where(l=>l.enabled).ToArray();
            var maxLights=skins.Length==0?0:skins.Max(s=>lights.Count(l=>l.type==LightType.Directional||Vector3.Distance(l.transform.position,s.bounds.center)<l.range+s.bounds.extents.magnitude));
            var cost=new Performance{frames=frames,meanEditorFrameMs=frameMs/frames,editorFps=1000/(frameMs/frames),meanMainThreadMs=cpuMs/frames,
                recorderGcValid=gc.Valid,recorderCpuValid=cpu.Valid,totalEditorGcBytes=gcBytes,maxEditorGcBytes=maxGc,peakEnemyPopulation=maxEnemies,peakVfx=maxVfx,
                activeRendererObjects=renderers.Length,activeSkinObjects=skins.Length,materials=renderers.SelectMany(r=>r.sharedMaterials).Distinct().Count(),
                inventoryTriangles=renderers.Sum(r=>{var m=r is SkinnedMeshRenderer s?s.sharedMesh:r.TryGetComponent<MeshFilter>(out var filter)?filter.sharedMesh:null;return m==null?0L:(long)m.GetIndexCount(0)/3;}),
                lights=lights.Length,maxPotentialLightsAtSkin=maxLights};
            Directory.CreateDirectory(Output+"/verification");File.WriteAllText(Output+"/verification/editor-performance.json",JsonUtility.ToJson(cost,true));
            Assert.That(maxEnemies,Is.LessThanOrEqualTo(25));
        }
        [Serializable] private sealed class Performance
        {
            public string method="Actual playable Chapter01 Editor observation; CPU/GC include Unity Editor and test runner. Renderer/triangle inventory counts all enabled LOD objects, not submitted GPU draws. Potential lights are bounding-sphere candidates, not measured per-pixel lights. No Android performance acceptance.";
            public int frames,peakEnemyPopulation,peakVfx,activeRendererObjects,activeSkinObjects,materials,lights,maxPotentialLightsAtSkin;
            public double meanEditorFrameMs,editorFps,meanMainThreadMs;
            public bool recorderGcValid,recorderCpuValid;
            public long totalEditorGcBytes,maxEditorGcBytes,inventoryTriangles;
        }
        private static HostileMotionProfile Profile(CharacterVisualBinding binding)
        {
            var catalog=Resources.FindObjectsOfTypeAll<S15VisualCatalog>().First(c=>c.HostilePresentation!=null);
            return catalog.HostilePresentation.Get(binding.ActiveModel.GetComponentInChildren<Animator>().runtimeAnimatorController);
        }
        private static void SampleEncounter(S01SceneCompositionRoot root,string name,CharacterVisualBinding binding,BoxCollider[] blockers,List<Clearance> records,string[] states)
        {
            var animator=binding.ActiveModel.GetComponentInChildren<Animator>();animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            foreach(var state in states)
            {
                animator.Play(state,0,.5f);animator.Update(0);Capture(root,name+"_"+state);
                records.Add(CheckMesh(name,state,"arena-center",binding,blockers));
            }
        }
        [Serializable] private sealed class ClearanceReport{public Clearance[] samples;}
        [Serializable] private sealed class Clearance
        {public string actor,state,context;public int penetratingVertices;public float minimumY;public string blocker;public Vector3 visualPosition;}
        private static Clearance CheckMesh(string actor,string state,string context,CharacterVisualBinding binding,BoxCollider[] blockers)
        {
            var skin=binding.ActiveModel.GetComponentsInChildren<SkinnedMeshRenderer>(true).OrderBy(s=>s.name).First();
            var mesh=new Mesh();skin.BakeMesh(mesh);var vertices=mesh.vertices;
            var candidates=blockers.Where(b=>b.enabled&&b.bounds.Intersects(skin.bounds)).ToArray();
            var result=new Clearance{actor=actor,state=state,context=context,minimumY=float.MaxValue,visualPosition=binding.ActiveModel.position};
            foreach(var vertex in vertices)
            {
                var world=skin.transform.TransformPoint(vertex);result.minimumY=Mathf.Min(result.minimumY,world.y);
                foreach(var blocker in candidates)
                    if(blocker.bounds.Contains(world)&&(blocker.ClosestPoint(world)-world).sqrMagnitude<.00000001f)
                    {result.penetratingVertices++;result.blocker=blocker.name;break;}
            }
            Object.Destroy(mesh);return result;
        }
        private static void Move(S01SceneCompositionRoot root,Vector3 position)
        {var body=root.PlayerObject.GetComponent<CharacterController>();body.enabled=false;body.transform.position=position;body.enabled=true;Physics.SyncTransforms();}
        private static void Capture(S01SceneCompositionRoot root,string name)
        {
            var camera=UnityEngine.Camera.main;camera.GetComponent<PortraitFollowCamera>().SnapToTarget();
            var canvases=root.GetComponentsInChildren<Canvas>(true);var enabled=canvases.Select(c=>c.enabled).ToArray();
            var prior=camera.targetTexture;var active=RenderTexture.active;
            var render=new RenderTexture(540,960,24,RenderTextureFormat.ARGBHalf);var texture=new Texture2D(540,960,TextureFormat.RGB24,false);
            try
            {
                foreach(var canvas in canvases)canvas.enabled=false;
                camera.targetTexture=render;camera.Render();RenderTexture.active=render;
                texture.ReadPixels(new Rect(0,0,540,960),0,0);texture.Apply();
                Directory.CreateDirectory(Output+"/gallery");File.WriteAllBytes(Output+"/gallery/"+name+".png",texture.EncodeToPNG());
            }
            finally
            {for(var i=0;i<canvases.Length;i++)canvases[i].enabled=enabled[i];camera.targetTexture=prior;RenderTexture.active=active;render.Release();Object.Destroy(render);Object.Destroy(texture);}
        }
    }
}
