using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.Camera;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.Map;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace Gravivore.Tests.PlayMode
{
    public sealed class Chapter01BlueprintWorldSmokeTests
    {
        private const string Output="docs/history/implementation-passes/chapter01-blueprint-world-r1";
        private CanonicalSceneTestScope _scene;
        [UnityTearDown] public IEnumerator Cleanup(){if(_scene!=null)yield return _scene.Cleanup();}
        private IEnumerator Load(bool controlled=true)
        {
            _scene=new CanonicalSceneTestScope();yield return _scene.Load();var root=_scene.Root;
            Assert.IsTrue(root.VisualEnvironment.BlueprintWorldOnly);Assert.NotNull(root.WorldPresenter.Layout);
            root.PlayerHealth.SetDevelopmentGodMode(true);
            root.PlayerObject.GetComponent<PlayerLocomotion>().enabled=false;
            if(!controlled)yield break;
            root.PlayerObject.GetComponent<GravityAttackController>().enabled=false;
            root.EnemyPopulation.enabled=false;root.MagnetarGuard.enabled=false;root.CustodianBoss.enabled=false;
            foreach(var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true))enemy.enabled=false;
        }
        [UnityTest] public IEnumerator EightSectorsAtActualProductionCamera()
        {
            yield return Load();var root=_scene.Root;
            var ids=new[]{"repair-hub","relay-yard","capacitor-field","cutting-floor","shield-dump","hauler-graveyard","elite-arena","boss-arena"};
            for(var i=0;i<ids.Length;i++)
            {
                if(i==6){root.WorldUnlocks.PrepareEliteEncounterForDevelopment();root.Chapter1Encounters.Tick();}
                if(i==7)root.MagnetarGuard.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));
                var position=root.VisualEnvironment.GetRegion(ids[i]).Root.position;
                Move(root,position+(i==6?new Vector3(-1.5f,0,-2):i==7?new Vector3(3,0,-2):Vector3.zero));
                root.EnemyPopulation.Tick(0);
                foreach(var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true))enemy.enabled=false;
                yield return null;Capture(root,$"{i+1:00}_{ids[i]}");
                if(i>=6)
                {
                    Move(root,position+(i==6?new Vector3(-8.5f,0,-.5f):new Vector3(6.5f,0,5)));
                    yield return null;Capture(root,$"{i+1:00}_{ids[i]}_facility_walk");
                }
            }
            root.MapIntegration.MapPresenter.OpenExpanded();Capture(root,"09_remapped_chapter_map");
            root.MapIntegration.MapPresenter.CloseExpanded();
        }
        [UnityTest] public IEnumerator ActualControllerTraversesEveryNewRouteAndMapUsesTheSameAuthority()
        {
            yield return Load();var root=_scene.Root;var layout=root.WorldPresenter.Layout;
            Assert.IsTrue(root.WorldPresenter.EliteGate.IsLocked);Assert.IsTrue(root.WorldPresenter.BossGate.IsLocked);
            root.WorldUnlocks.PrepareEliteEncounterForDevelopment();root.Chapter1Encounters.Tick();
            root.MagnetarGuard.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));
            Assert.IsFalse(root.WorldPresenter.EliteGate.IsLocked);Assert.IsFalse(root.WorldPresenter.BossGate.IsLocked);
            Physics.SyncTransforms();var body=root.PlayerObject.GetComponent<CharacterController>();
            foreach(var occupant in root.GetComponentsInChildren<CharacterController>(true))if(occupant!=body)occupant.enabled=false;
            Move(root,layout.GetRoutePoint(0));
            var evidence=new List<string>();
            for(var i=1;i<layout.RoutePointCount;i++)
            {
                var target=layout.GetRoutePoint(i);var reached=false;
                for(var j=0;j<400;j++)
                {
                    var delta=target-body.transform.position;delta.y=0;
                    if(delta.magnitude<.09f){reached=true;break;}
                    body.Move(Vector3.ClampMagnitude(delta,.28f)+Vector3.down*.015f);Physics.SyncTransforms();
                    Assert.That(body.transform.position.y,Is.InRange(-.15f,.3f),"Movement ground discontinuity");
                }
                Assert.IsTrue(reached,"Sticky traversal towards "+target+" from "+body.transform.position);
                evidence.Add($"route {i}: {target} CharacterController radius={body.radius} skin={body.skinWidth} PASS");
                yield return null;
            }
            var topology=root.MapIntegration.MapPresenter.Topology;
            Assert.That(topology.SurfaceCount,Is.EqualTo(layout.SurfaceCount));
            var blockers=root.WorldPresenter.GameplayRoot.GetComponentsInChildren<BoxCollider>(true).Count(c=>c.gameObject.layer==LayerMask.NameToLayer("HardBlocker"));
            Assert.That(topology.BlockerCount,Is.EqualTo(blockers));
            for(var i=0;i<topology.BlockerCount;i++)Assert.NotNull(topology.GetBlocker(i).Authority);
            Directory.CreateDirectory(Output+"/verification");File.WriteAllLines(Output+"/verification/traversal.txt",evidence);
        }
        [UnityTest, Timeout(420000)] public IEnumerator FiveMinuteCombatTraversalSoakKeepsPopulationAndPresentationBounded()
        {
            yield return Load(false);var root=_scene.Root;
            var route=new[]{new Vector3(0,0,-30),new Vector3(0,0,-10),new Vector3(0,0,10),new Vector3(-15,0,10),new Vector3(0,0,10),new Vector3(15,0,10),new Vector3(0,0,10),new Vector3(0,0,32),new Vector3(-15,0,32),new Vector3(0,0,32),new Vector3(15,0,32),new Vector3(0,0,32),new Vector3(0,0,10),new Vector3(0,0,-10)};
            var started=Time.realtimeSinceStartupAsDouble;var body=root.PlayerObject.GetComponent<CharacterController>();
            var index=1;var samples=0;double sum=0,max=0;var maxEnemies=0;var maxParticles=0;
            var gcBefore=GC.CollectionCount(0);var completed=false;var reachedWaypoints=0;
            while(Time.realtimeSinceStartupAsDouble-started<300)
            {
                var delta=route[index]-body.transform.position;delta.y=0;
                if(delta.magnitude<.3f){index=(index+1)%route.Length;reachedWaypoints++;}
                else body.Move(Vector3.ClampMagnitude(delta,4.5f*Time.unscaledDeltaTime)+Vector3.down*.02f);
                Assert.That(body.transform.position.y,Is.GreaterThan(-.15f));
                Assert.That(root.EnemyPopulation.LiveEnemyCount,Is.LessThanOrEqualTo(25));
                maxEnemies=Math.Max(maxEnemies,root.EnemyPopulation.LiveEnemyCount);
                var ms=Time.unscaledDeltaTime*1000;sum+=ms;max=Math.Max(max,ms);samples++;
                if(samples%120==0)maxParticles=Math.Max(maxParticles,root.GetComponentsInChildren<ParticleSystem>(true).Sum(p=>p.particleCount));
                yield return null;
            }
            completed=true;
            var evidence=new Soak{completed=completed,seconds=Time.realtimeSinceStartupAsDouble-started,frames=samples,
                meanEditorFrameMs=sum/samples,maxEditorFrameMs=max,maxLiveEnemies=maxEnemies,maxParticles=maxParticles,
                gen0Collections=GC.CollectionCount(0)-gcBefore,reachedWaypoints=reachedWaypoints,method="Unity Editor realtime combat/traversal observation; includes test runner and editor overhead; no Android GPU/FPS claim"};
            Directory.CreateDirectory(Output+"/verification");File.WriteAllText(Output+"/verification/five-minute-soak.json",JsonUtility.ToJson(evidence,true));
            Assert.That(reachedWaypoints,Is.GreaterThanOrEqualTo(route.Length),"Soak must complete a full ordinary-sector traversal loop.");
            Assert.IsTrue(root.FlushNow());
        }
        [Serializable] private sealed class Soak
        {public bool completed;public double seconds,meanEditorFrameMs,maxEditorFrameMs;public int frames,maxLiveEnemies,maxParticles,gen0Collections,reachedWaypoints;public string method;}
        private static void Move(S01SceneCompositionRoot root,Vector3 position)
        {var body=root.PlayerObject.GetComponent<CharacterController>();body.enabled=false;body.transform.position=position;body.enabled=true;Physics.SyncTransforms();}
        private static void Capture(S01SceneCompositionRoot root,string name)
        {
            var camera=UnityEngine.Camera.main;camera.GetComponent<PortraitFollowCamera>().SnapToTarget();root.MapIntegration.MapPresenter.RefreshNow();
            var canvas=root.GetComponentInChildren<Canvas>();var mode=canvas.renderMode;var priorCamera=canvas.worldCamera;var plane=canvas.planeDistance;
            var target=camera.targetTexture;var active=RenderTexture.active;
            var render=new RenderTexture(540,960,24,RenderTextureFormat.ARGBHalf);var texture=new Texture2D(540,960,TextureFormat.RGB24,false);
            try
            {
                canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
                camera.targetTexture=render;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=render;
                texture.ReadPixels(new Rect(0,0,540,960),0,0);texture.Apply();Directory.CreateDirectory(Output+"/internal");
                File.WriteAllBytes(Output+"/internal/"+name+".png",texture.EncodeToPNG());
                var planes=GeometryUtility.CalculateFrustumPlanes(camera);
                var visible=root.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled&&r.gameObject.activeInHierarchy&&GeometryUtility.TestPlanesAABB(planes,r.bounds)).ToArray();
                var triangles=0L;
                foreach(var renderer in visible)
                {
                    var filter=renderer.GetComponent<MeshFilter>();
                    var mesh=renderer is SkinnedMeshRenderer skin?skin.sharedMesh:filter!=null?filter.sharedMesh:null;
                    if(mesh!=null)for(var i=0;i<mesh.subMeshCount;i++)triangles+=(long)mesh.GetIndexCount(i)/3;
                }
                var inventory=new Inventory{renderers=visible.Length,materials=visible.SelectMany(r=>r.sharedMaterials).Distinct().Count(),triangles=triangles};
                File.WriteAllText(Output+"/internal/"+name+"_cost.json",JsonUtility.ToJson(inventory,true));
            }
            finally
            {canvas.renderMode=mode;canvas.worldCamera=priorCamera;canvas.planeDistance=plane;camera.targetTexture=target;RenderTexture.active=active;render.Release();Object.Destroy(render);Object.Destroy(texture);}
        }
        [Serializable] private sealed class Inventory
        {public int renderers,materials;public long triangles;public string method="Production-camera frustum AABB inventory; includes occluded bounds, excludes UI and shadow passes; no device FPS claim";}
    }
}
