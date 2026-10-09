using System;
using System.Collections;
using System.IO;
using System.Linq;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.Camera;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace Gravivore.Tests.PlayMode
{
    public sealed class ConceptFidelitySmokeTests
    {
        private CanonicalSceneTestScope _scene;
        [UnityTearDown] public IEnumerator Cleanup(){if(_scene!=null)yield return _scene.Cleanup();}
        private IEnumerator Load()
        {
            _scene=new CanonicalSceneTestScope();yield return _scene.Load();var root=_scene.Root;
            root.EnemyPopulation.enabled=false;root.MagnetarGuard.enabled=false;root.CustodianBoss.enabled=false;
            root.PlayerObject.GetComponent<PlayerLocomotion>().enabled=false;root.PlayerObject.GetComponent<GravityAttackController>().enabled=false;
            root.PlayerHealth.enabled=false;
            foreach(var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true))enemy.enabled=false;
        }
        private static void Move(S01SceneCompositionRoot root,Vector3 position)
        {var body=root.PlayerObject.GetComponent<CharacterController>();body.enabled=false;body.transform.position=position;body.enabled=true;Physics.SyncTransforms();}
        [UnityTest] public IEnumerator BossHasAuthoredRigDistinctAttackClipsAndThreeLodsWithoutArtAuthority()
        {
            yield return Load();var root=_scene.Root;var boss=root.CustodianBoss;var model=boss.GetComponent<CharacterVisualBinding>().ActiveModel;
            StringAssert.StartsWith("Custodian_V3",model.name);PresentationPrefabValidation.ValidateOrThrow(model.gameObject);
            var animator=model.GetComponentInChildren<Animator>();Assert.IsFalse(animator.applyRootMotion);
            foreach(var state in new[]{"Idle","Run","Windup","Release","Special","Hit","Death"})
                Assert.IsTrue(animator.HasState(0,Animator.StringToHash(state)),state);
            var lod=model.GetComponentInChildren<LODGroup>().GetLODs();Assert.That(lod.Length,Is.EqualTo(3));
            var triangles=lod.Select(l=>(int)((SkinnedMeshRenderer)l.renderers[0]).sharedMesh.GetIndexCount(0)/3).ToArray();
            Assert.That(triangles[0],Is.LessThan(25000));Assert.That(triangles[1],Is.LessThan(triangles[0]));Assert.That(triangles[2],Is.LessThan(triangles[1]));
            Assert.That(model.GetComponentsInChildren<Collider>().Length,Is.Zero);
            var authorityPosition=boss.transform.position;var radius=boss.CollisionRadius;var hp=boss.CurrentHitPoints;
            var bones=model.GetComponentsInChildren<Transform>(true);
            var core=bones.Single(b=>b.name=="CORE");var shutter=bones.Single(b=>b.name=="L_SHUTTER");
            Sample(animator,"Idle",.55f);var idleScale=core.localScale;var idleRotation=shutter.localRotation;
            Sample(animator,"Windup",.55f);
            Assert.That(Quaternion.Angle(shutter.localRotation,idleRotation),Is.GreaterThan(5),"Imported windup must move the shell.");
            foreach(var state in new[]{"Release","Special","Hit","Death"})Sample(animator,state,.55f);
            Assert.That(core.localScale.magnitude,Is.LessThan(idleScale.magnitude*.65f),"Imported shutdown must extinguish the internal core.");
            Assert.That(boss.transform.position,Is.EqualTo(authorityPosition));Assert.That(boss.CollisionRadius,Is.EqualTo(radius));Assert.That(boss.CurrentHitPoints,Is.EqualTo(hp));
        }
        [UnityTest] public IEnumerator AuthoredRepairAndAmbientPoolsStayBoundedAndNeverHealByPresentation()
        {
            yield return Load();var root=_scene.Root;var hub=root.RepairHub;var arms=hub.Manipulators;
            Assert.That(arms.ArmCount,Is.EqualTo(2));
            foreach(var filter in arms.GetComponentsInChildren<MeshFilter>())Assert.IsNotNull(filter.sharedMesh,filter.name);
            Assert.That(arms.GetComponentsInChildren<Collider>().Length,Is.Zero);
            var count=root.GetComponentsInChildren<Transform>(true).Length;var materials=arms.GetComponentsInChildren<Renderer>().Select(r=>r.sharedMaterial).Distinct().Count();
            Assert.That(materials,Is.EqualTo(1));var atmosphere=root.GetComponentInChildren<FidelityAtmospherePresenter>();
            Assert.That(atmosphere.Capacity,Is.EqualTo(1));Assert.That(atmosphere.LightCount,Is.LessThanOrEqualTo(2));
            root.PlayerHealth.ApplyDamage(new DamageRequest(50,DamageType.Physical));root.PlayerHealth.Tick(3.1f);root.PlayerHealth.Tick(.1f);
            Assert.IsTrue(hub.IsRepairing);var hp=root.PlayerHealth.CurrentHitPoints;
            for(var i=0;i<300;i++)arms.Tick(.016f);
            Assert.That(root.PlayerHealth.CurrentHitPoints,Is.EqualTo(hp));Assert.That(arms.Engagement,Is.EqualTo(1));
            Assert.That(root.GetComponentsInChildren<Transform>(true).Length,Is.EqualTo(count));
            Capture(root,"01_repair_active");Move(root,hub.RepairPosition+Vector3.right*10);yield return null;arms.Tick(.5f);
            Assert.IsFalse(hub.IsRepairing);Assert.That(arms.Engagement,Is.Zero);
        }
        [UnityTest] public IEnumerator ContinuousHeroRouteCameraReviewAndStablePresentationInventory()
        {
            yield return Load();var root=_scene.Root;var route=root.VisualEnvironment.Floor.Find("Chapter 01 Concept Fidelity V2");Assert.NotNull(route);
            Assert.That(route.GetComponentsInChildren<Collider>(true).Length,Is.Zero);
            var materials=route.GetComponentsInChildren<Renderer>(true).Select(r=>r.sharedMaterial).Distinct().ToArray();Assert.That(materials.Length,Is.EqualTo(1));
            Assert.NotNull(route.Find("Continuous worn deck"));Assert.NotNull(route.Find("Industrial focal points"));
            foreach(var tile in route.Find("Continuous worn deck").GetComponentsInChildren<Renderer>())
            {
                Assert.That(tile.bounds.size.y,Is.LessThan(.4f),"Deck must lie flat: "+tile.name);
                Assert.That(tile.bounds.size.x,Is.GreaterThan(5.9f));Assert.That(tile.bounds.size.z,Is.GreaterThan(5.9f));
            }
            var renderers=root.GetComponentsInChildren<Renderer>(true).Length;var transforms=root.GetComponentsInChildren<Transform>(true).Length;
            foreach(var pair in new[]{("02_spawn",new Vector3(0,0,-28)),("03_capacitors",new Vector3(-20,0,-12)),("04_haulers",new Vector3(20,0,-12)),
                ("05_corridor",new Vector3(0,0,10)),("06_relay",new Vector3(-26,0,20)),("07_shield",new Vector3(26,0,20)),
                ("08_cutting",new Vector3(0,0,40)),("09_elite_approach",new Vector3(0,0,54)),("10_magnetar",new Vector3(0,0,68)),
                ("11_containment",new Vector3(0,0,84))})
            {Move(root,pair.Item2);yield return null;Capture(root,pair.Item1);}
            root.WorldUnlocks.PrepareEliteEncounterForDevelopment();root.Chapter1Encounters.Tick();
            root.MagnetarGuard.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));
            Move(root,root.WorldPresenter.Configuration.BossArenaCenter+Vector3.back*4.5f);root.CustodianBoss.Tick(0);yield return null;
            var bossAnimator=root.CustodianBoss.GetComponent<CharacterVisualBinding>().ActiveModel.GetComponentInChildren<Animator>();
            // Freeze the observer only for explicit pose captures; otherwise its
            // authoritative state restores windup while the capture is being sampled.
            root.GetComponentInChildren<Gravivore.Presentation.Player.VisualSliceAnimationBridge>().enabled=false;
            bossAnimator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            foreach(var skin in bossAnimator.GetComponentsInChildren<SkinnedMeshRenderer>())skin.updateWhenOffscreen=true;
            foreach(var pair in new[]{("12_custodian_idle","Idle"),("13_custodian_windup","Windup"),("14_custodian_release","Release"),("15_custodian_special","Special"),("16_custodian_shutdown","Death")})
            {Sample(bossAnimator,pair.Item2,.55f);yield return null;yield return null;Capture(root,pair.Item1);}
            Assert.That(root.GetComponentsInChildren<Renderer>(true).Length,Is.EqualTo(renderers));
            Assert.That(root.GetComponentsInChildren<Transform>(true).Length,Is.EqualTo(transforms));
            Directory.CreateDirectory("docs/history/visual-stages/chapter01-visual-passes/concept-fidelity-v2/verification");
            File.WriteAllText("docs/history/visual-stages/chapter01-visual-passes/concept-fidelity-v2/verification/runtime_inventory.json",JsonUtility.ToJson(new Inventory{
                transforms=transforms,renderers=renderers,sharedMaterials=MaterialCount(root),
                realtimeLights=root.GetComponentsInChildren<Light>(true).Length,heroRouteRenderers=route.GetComponentsInChildren<Renderer>(true).Length},true));
        }
        [Serializable] private sealed class Inventory{public int transforms,renderers,sharedMaterials,realtimeLights,heroRouteRenderers;}
        private static void Sample(Animator animator,string state,float normalizedTime)
        {
            // Batch-mode has no continuously rendered Game view. Explicit pose review
            // must evaluate imported curves even when normal visibility culling pauses them.
            var prior=animator.cullingMode;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            animator.Play(Animator.StringToHash(state),0,normalizedTime);animator.Update(.0001f);animator.cullingMode=prior;
        }
        private sealed class RouteInput : IMovementInput{public Vector2 Movement{get;set;}}
        private static int MaterialCount(Component owner)=>owner.GetComponentsInChildren<Renderer>(true)
            .SelectMany(r=>r.sharedMaterials).Where(m=>m!=null).Distinct().Count();
        private static int TransformCount(Component owner)=>owner.GetComponentsInChildren<Transform>(true).Length;
        private static int ActorTransformCount(S01SceneCompositionRoot root)=>
            TransformCount(root.EnemyPopulation)+TransformCount(root.PlayerObject.transform);
        private static int FidelityTransformCount(S01SceneCompositionRoot root)=>
            TransformCount(root.VisualEnvironment.Floor.Find("Chapter 01 Concept Fidelity V2"))+
            TransformCount(root.RepairHub.Manipulators)+TransformCount(root.GetComponentInChildren<FidelityAtmospherePresenter>());
        [Serializable] private sealed class SustainedEvidence
        {public double seconds,worstFrameGapMilliseconds;public int frames,completedStops,initialTransforms,finalTransforms,initialMaterials,finalMaterials,
            initialActorTransforms,finalActorTransforms,initialFidelityTransforms,finalFidelityTransforms,maximumLiveEnemies;
            public long assimilation;public string mode="Editor graphics, development gate unlock and god mode; real locomotion, combat, cooldowns and saves";}
        [UnityTest,Timeout(600000)] public IEnumerator FiveMinuteRuntimeHeroRouteUsesRealLocomotionCombatAndBoundedPresentation()
        {
            _scene=new CanonicalSceneTestScope();yield return _scene.Load();var root=_scene.Root;
            root.PlayerHealth.SetDevelopmentGodMode(true);
            root.WorldUnlocks.PrepareEliteEncounterForDevelopment();root.Chapter1Encounters.Tick();
            root.MagnetarGuard.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));
            Physics.SyncTransforms();
            var input=new RouteInput();var camera=UnityEngine.Camera.main;
            root.PlayerObject.GetComponent<PlayerLocomotion>().Initialize(input,camera.transform,root.PlayerStats,360);
            var stops=new[]{root.EnemyPopulation.GetSpot(3).Position,root.EnemyPopulation.GetSpot(4).Position,
                new Vector3(0,0,-1.25f),root.EnemyPopulation.GetSpot(0).Position,root.EnemyPopulation.GetSpot(2).Position,
                root.EnemyPopulation.GetSpot(1).Position,root.MagnetarGuard.transform.position+Vector3.back,
                root.WorldPresenter.Configuration.BossArenaCenter+Vector3.back*4.5f,root.RepairHub.RepairPosition};
            var evidence=new SustainedEvidence{initialTransforms=root.GetComponentsInChildren<Transform>(true).Length,
                initialMaterials=MaterialCount(root),initialActorTransforms=ActorTransformCount(root),initialFidelityTransforms=FidelityTransformCount(root)};
            var started=Time.realtimeSinceStartupAsDouble;var previous=started;var index=0;var point=0;var dwellUntil=0d;
            System.Collections.Generic.List<Vector3> path=null;
            var nextCapture=60d;
            while(Time.realtimeSinceStartupAsDouble-started<300)
            {
                var now=Time.realtimeSinceStartupAsDouble;evidence.frames++;
                evidence.worstFrameGapMilliseconds=Math.Max(evidence.worstFrameGapMilliseconds,(now-previous)*1000);previous=now;
                evidence.maximumLiveEnemies=Math.Max(evidence.maximumLiveEnemies,root.EnemyPopulation.LiveEnemyCount);
                if(now>=dwellUntil)
                {
                    if(path==null)
                    {
                        var grid=new Chapter01ProductionSmokeTests.RouteGrid(root,root.PlayerObject.transform.position);
                        Assert.IsTrue(grid.Reaches(stops[index]),"Unchanged collision route: "+index);
                        path=grid.Path(stops[index]);point=0;
                    }
                    while(point<path.Count&&Vector3.Distance(root.PlayerObject.transform.position,path[point])<.40f)point++;
                    if(point==path.Count)
                    {input.Movement=Vector2.zero;path=null;index=(index+1)%stops.Length;dwellUntil=now+7;evidence.completedStops++;}
                    else
                    {
                        var direction=path[point]-root.PlayerObject.transform.position;direction.y=0;direction.Normalize();
                        var forward=Vector3.ProjectOnPlane(camera.transform.forward,Vector3.up).normalized;
                        var right=Vector3.ProjectOnPlane(camera.transform.right,Vector3.up).normalized;
                        input.Movement=new Vector2(Vector3.Dot(direction,right),Vector3.Dot(direction,forward));
                    }
                }
                if(now-started>=nextCapture){Capture(root,"sustained_"+(int)nextCapture);nextCapture+=120;Assert.IsTrue(root.FlushNow());}
                yield return null;
            }
            input.Movement=Vector2.zero;evidence.seconds=Time.realtimeSinceStartupAsDouble-started;
            evidence.finalTransforms=root.GetComponentsInChildren<Transform>(true).Length;
            evidence.finalMaterials=MaterialCount(root);evidence.finalActorTransforms=ActorTransformCount(root);
            evidence.finalFidelityTransforms=FidelityTransformCount(root);
            evidence.assimilation=root.Progression.State.TotalAssimilationScore;
            // Evidence is written before assertions so a failed soak still exposes its inventories.
            Directory.CreateDirectory("docs/history/visual-stages/chapter01-visual-passes/concept-fidelity-v2/verification");
            File.WriteAllText("docs/history/visual-stages/chapter01-visual-passes/concept-fidelity-v2/verification/five_minute_runtime.json",JsonUtility.ToJson(evidence,true));
            Assert.That(evidence.completedStops,Is.GreaterThanOrEqualTo(stops.Length),"Five-minute traversal must cover the whole route.");
            Assert.That(evidence.maximumLiveEnemies,Is.LessThanOrEqualTo(root.EnemyPopulation.GlobalLiveEnemyCap));
            Assert.That(evidence.finalFidelityTransforms,Is.EqualTo(evidence.initialFidelityTransforms));
            // The accepted S15 factory lazily retains one art variant per archetype per pooled actor.
            // Account for that explicit cache, while requiring every non-actor transform to remain fixed.
            Assert.That(evidence.finalTransforms-evidence.finalActorTransforms,
                Is.EqualTo(evidence.initialTransforms-evidence.initialActorTransforms));
            Assert.That(root.EnemyPopulation.GetComponentsInChildren<OrdinaryEnemyController>(true).Length,
                Is.EqualTo(root.EnemyPopulation.GlobalLiveEnemyCap));
            Assert.That(evidence.finalMaterials,Is.LessThanOrEqualTo(evidence.initialMaterials+4));
            Assert.IsTrue(root.FlushNow());
            var diagnostics=root.GetComponentInChildren<Gravivore.Presentation.Development.ColdStartDiagnostics>();
            Assert.IsFalse(diagnostics.enabled);Assert.IsTrue(File.Exists(diagnostics.OutputPath));
            File.Copy(diagnostics.OutputPath,"docs/history/visual-stages/chapter01-visual-passes/concept-fidelity-v2/verification/cold-start-fidelity-editor.json",true);
        }
        private static void Capture(S01SceneCompositionRoot root,string name)
        {
            root.MapIntegration.MapPresenter.RefreshNow();var camera=UnityEngine.Camera.main;camera.GetComponent<PortraitFollowCamera>().SnapToTarget();
            var canvas=root.GetComponentInChildren<Canvas>();var mode=canvas.renderMode;var priorCamera=canvas.worldCamera;
            var priorTarget=camera.targetTexture;var priorActive=RenderTexture.active;
            var render=new RenderTexture(540,960,24,RenderTextureFormat.ARGBHalf);var texture=new Texture2D(540,960,TextureFormat.RGB24,false);
            try
            {
                canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
                camera.targetTexture=render;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=render;
                texture.ReadPixels(new Rect(0,0,540,960),0,0);texture.Apply();Directory.CreateDirectory("docs/history/visual-stages/chapter01-visual-passes/concept-fidelity-v2/internal");
                File.WriteAllBytes("docs/history/visual-stages/chapter01-visual-passes/concept-fidelity-v2/internal/"+name+".png",texture.EncodeToPNG());
            }
            finally
            {canvas.renderMode=mode;canvas.worldCamera=priorCamera;camera.targetTexture=priorTarget;RenderTexture.active=priorActive;render.Release();Object.Destroy(render);Object.Destroy(texture);}
        }
    }
}
