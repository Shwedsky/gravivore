using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.Camera;
using Gravivore.Presentation.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;
using Unity.Profiling;
using Object=UnityEngine.Object;

namespace Gravivore.Tests.PlayMode
{
    public sealed class ConnectedConceptSectorsSmokeTests
    {
        private const string Output="docs/history/implementation-passes/connected-concept-sectors";
        private CanonicalSceneTestScope _scene;
        [UnityTearDown] public IEnumerator Cleanup(){if(_scene!=null)yield return _scene.Cleanup();}
        private IEnumerator Load()
        {
            _scene=new CanonicalSceneTestScope();yield return _scene.Load();
            Assert.That(_scene.Root.VisualEnvironment.GetRegion("capacitor-field").Root.GetComponentsInChildren<MeshRenderer>().Any(r=>r.name.StartsWith("CF_")),Is.True);
        }
        private SpawnSpotRuntime Spot(string id) => Enumerable.Range(0,_scene.Root.EnemyPopulation.SpotCount).Select(_scene.Root.EnemyPopulation.GetSpot).Single(s=>s.Id==id);
        private static readonly Vector3[] Positions={new Vector3(-8,0,10),new Vector3(-15,0,10),new Vector3(-15,0,12.9f),new Vector3(-18.5f,0,8),new Vector3(-9,0,10),new Vector3(8,0,10),new Vector3(15,0,10),new Vector3(15,0,12.8f),new Vector3(18.2f,0,8),new Vector3(9,0,10),new Vector3(-5.8f,0,10),new Vector3(0,0,10),new Vector3(5.8f,0,10)};
        private static readonly string[] Names={"01_entry","02_combat","03_installation_approach","04_edge","05_return_exit","06_cutting_entry","07_cutting_combat","08_cutting_approach","09_cutting_edge","10_cutting_exit","11_transfer_west","12_transfer_junction","13_transfer_east"};

        [UnityTest] public IEnumerator BothSectorsAndTransferAtOneProductionCameraWithActualPostProcessing()
        {
            yield return Load();var root=_scene.Root;root.PlayerHealth.SetDevelopmentGodMode(true);
            var locomotion=root.PlayerObject.GetComponent<PlayerLocomotion>();locomotion.enabled=false;
            for(var i=0;i<Positions.Length;i++)
            {
                Move(Positions[i]);yield return new WaitForSeconds(.25f);
                var camera=UnityEngine.Camera.main;camera.GetComponent<PortraitFollowCamera>().SnapToTarget();
                Capture(Names[i],true);Capture(Names[i],false);
            }
            Move(Positions[1]);yield return null;
            var additional=UnityEngine.Camera.main.GetUniversalAdditionalCameraData();
            Assert.IsTrue(additional.renderPostProcessing);Assert.IsTrue(UnityEngine.Camera.main.allowHDR);
            var stack=VolumeManager.instance.stack;
            Assert.That(stack.GetComponent<Bloom>().intensity.value,Is.EqualTo(.12f).Within(.001f));
            Assert.That(stack.GetComponent<Tonemapping>().mode.value,Is.EqualTo(TonemappingMode.ACES));
            var on=Capture("postprocess_on",false);
            try {additional.renderPostProcessing=false;var off=Capture("postprocess_off",false);
                Assert.IsFalse(on.SequenceEqual(off),"Post-processing resources must change rendered pixels, not only configuration.");}
            finally {additional.renderPostProcessing=true;}
        }
        [UnityTest] public IEnumerator ActualPlayerLocomotionTraversesBothInstallationsAndAllTransferPortals()
        {
            yield return Load();var root=_scene.Root;root.PlayerHealth.SetDevelopmentGodMode(true);
            root.PlayerObject.GetComponent<GravityAttackController>().enabled=false;
            var locomotion=root.PlayerObject.GetComponent<PlayerLocomotion>();locomotion.enabled=false;
            var input=new RouteInput();locomotion.Initialize(input,UnityEngine.Camera.main.transform,root.PlayerStats,540);
            var body=root.PlayerObject.GetComponent<CharacterController>();
            foreach(var occupant in root.GetComponentsInChildren<CharacterController>(true))if(occupant!=body)occupant.enabled=false;
            foreach(var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true))enemy.enabled=false;
            var route=new[]{new Vector3(-15,0,10),new Vector3(-15,0,12.9f),new Vector3(-15,0,10),new Vector3(-8,0,10),new Vector3(-4,0,10),new Vector3(0,0,10),new Vector3(4,0,10),new Vector3(8,0,10),new Vector3(15,0,10),new Vector3(15,0,12.8f),new Vector3(18.2f,0,8),new Vector3(15,0,10),new Vector3(8,0,10),new Vector3(0,0,10),new Vector3(-8,0,10),new Vector3(-15,0,10)};
            Move(route[0]);var records=new List<string>();
            for(var i=1;i<route.Length;i++)
            {
                for(var step=0;step<350;step++)
                {
                    var delta=route[i]-body.transform.position;delta.y=0;if(delta.magnitude<.12f)break;
                    Steer(input,delta);locomotion.Step(.035f);Physics.SyncTransforms();
                    Assert.That(body.transform.position.y,Is.InRange(-.10f,.15f));
                    if(step%20==0)yield return null;
                }
                Assert.That(Vector3.Distance(body.transform.position,route[i]),Is.LessThan(.2f),"Sticky controller route "+i);
                records.Add("segment "+i+" PASS / actual PlayerLocomotion + CharacterController / "+route[i]);
            }
            input.Movement=Vector2.zero;
            var topology=root.MapIntegration.MapPresenter.Topology;
            var blockerCount=root.WorldPresenter.GameplayRoot.GetComponentsInChildren<BoxCollider>(true).Count(c=>c.gameObject.layer==LayerMask.NameToLayer("HardBlocker"));
            Assert.That(topology.BlockerCount,Is.EqualTo(blockerCount));
            Directory.CreateDirectory(Output+"/verification");File.WriteAllLines(Output+"/verification/sector-traversal.txt",records);
        }
        [UnityTest] public IEnumerator ArcAndCutterAttacksRewardsDeathRespawnRepairAndSaveRemainFunctional()
        {
            yield return Load();var root=_scene.Root;root.PlayerObject.GetComponent<PlayerLocomotion>().enabled=false;
            foreach(var spotId in new[]{"capacitor-field","cutting-floor"})
            {
            var spot=Spot(spotId);root.PlayerObject.GetComponent<GravityAttackController>().enabled=false;
            var enemy=spot.GetLiveEnemy(0);Move(enemy.transform.position+Vector3.back);
            var causality=root.GetComponent<AttackCausalityPresenter>();var attacks=causality.SourceCount;
            root.PlayerHealth.SetDevelopmentGodMode(true);
            for(var i=0;i<90;i++){enemy.Tick(.04f);yield return null;}
            Assert.That(causality.SourceCount,Is.GreaterThan(attacks));
            var score=root.Progression.State.TotalAssimilationScore;
            var prior=new HashSet<EnemyLifeId>();
            root.EnemyPopulation.enabled=false;
            while(spot.LiveCount>0){enemy=spot.GetLiveEnemy(0);prior.Add(enemy.LifeId);enemy.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));}
            Assert.That(root.Progression.State.TotalAssimilationScore,Is.GreaterThan(score));
            Move(new Vector3(0,0,-30));for(var i=0;i<12;i++)root.EnemyPopulation.Tick(60);
            Assert.That(spot.LiveCount,Is.EqualTo(4));Assert.IsFalse(prior.Contains(spot.GetLiveEnemy(0).LifeId));
            root.PlayerHealth.SetDevelopmentGodMode(false);Move(Positions[1]);
            root.PlayerHealth.Tick(3);Assert.IsTrue(root.PlayerHealth.ApplyDamage(new DamageRequest(100000,DamageType.Gravity)).WasLethal);
            Assert.That(Vector3.Distance(root.PlayerObject.transform.position,root.RepairHub.RepairPosition),Is.LessThan(.1f));
            root.PlayerHealth.Tick(3);root.PlayerHealth.ApplyDamage(new DamageRequest(25,DamageType.Gravity));
            var hp=root.PlayerHealth.CurrentHitPoints;root.PlayerHealth.Tick(30);root.PlayerHealth.Tick(30);
            Assert.That(root.PlayerHealth.CurrentHitPoints,Is.GreaterThan(hp));Assert.IsTrue(root.FlushNow());
            Directory.CreateDirectory(Output+"/verification");File.AppendAllText(Output+"/verification/functional-flow.txt",
                ""+spotId+" actual attack/source; rewards; whole-wave death/respawn with new life IDs; player death/recovery; repair; save flush PASS. Damage injection and advanced cooldown time are test fixtures; production balance untouched.\n");
            }
        }
        [UnityTest,Timeout(180000)] public IEnumerator UninterruptedRecordedMovementAndCombatAtTheProductionCamera()
        {
            if(Environment.GetEnvironmentVariable("GRAVIVORE_CONNECTED_VIDEO")!="1")Assert.Ignore("Opt-in recording; run with GRAVIVORE_CONNECTED_VIDEO=1.");
            yield return Load();var root=_scene.Root;root.PlayerHealth.SetDevelopmentGodMode(true);
            var locomotion=root.PlayerObject.GetComponent<PlayerLocomotion>();locomotion.enabled=false;
            var input=new RouteInput();locomotion.Initialize(input,UnityEngine.Camera.main.transform,root.PlayerStats,540);
            var route=new[]{new Vector3(-15,0,10),new Vector3(-15,0,12.9f),new Vector3(-15,0,10),new Vector3(-8,0,10),new Vector3(-4,0,10),new Vector3(0,0,10),new Vector3(4,0,10),new Vector3(8,0,10),new Vector3(15,0,10),new Vector3(15,0,12.8f),new Vector3(18.2f,0,8),new Vector3(15,0,10),new Vector3(8,0,10),new Vector3(0,0,10),new Vector3(-8,0,10),new Vector3(-15,0,10)};
            Move(route[0]);var index=1;var frame=0;var started=Time.time;var wait=0f;var nextCapture=0f;
            var attacks=root.GetComponent<AttackCausalityPresenter>().SourceCount;
            var deaths=0;foreach(var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true))enemy.Died+=_=>deaths++;
            Directory.CreateDirectory(Output+"/video-frames");
            var maxEnemies=0;
            while(Time.time-started<80 && (index<route.Length || wait>0))
            {
                var delta=index<route.Length?route[index]-root.PlayerObject.transform.position:Vector3.zero;delta.y=0;
                if(wait>0){wait-=Time.deltaTime;input.Movement=Vector2.zero;}
                else if(delta.magnitude<.2f){if(index==2||index==8)wait=12;index++;input.Movement=Vector2.zero;}
                else {Steer(input,delta);locomotion.Step(Mathf.Min(Time.deltaTime,.10f));}
                maxEnemies=Math.Max(maxEnemies,root.EnemyPopulation.LiveEnemyCount);
                if(Time.time-started>=nextCapture){Capture("frame_"+(frame++).ToString("D5"),true,true);nextCapture+=.1f;}
                yield return null;
            }
            input.Movement=Vector2.zero;
            var actualAttacks=root.GetComponent<AttackCausalityPresenter>().SourceCount-attacks;
            Assert.That(index,Is.GreaterThanOrEqualTo(route.Length));Assert.That(actualAttacks,Is.GreaterThan(0));
            File.WriteAllText(Output+"/verification/video.json",JsonUtility.ToJson(new Video{frames=frame,seconds=Time.time-started,attacks=actualAttacks,deaths=deaths,maxEnemies=maxEnemies,
                method="Uncut production-camera Editor run; real locomotion, AI, attacks and balance. Isolated test save; player god mode only to retain one uninterrupted recording. Silent video, 10 capture samples per game second. No Android FPS claim."},true));
        }
        [UnityTest] public IEnumerator MeasuredEditorCombatInBothFunctionalPlacesWithoutReadback()
        {
            yield return Load();var root=_scene.Root;root.PlayerHealth.SetDevelopmentGodMode(true);
            foreach(var region in new[]{"capacitor-field","cutting-floor"})
            {
            Move(region=="capacitor-field"?Positions[1]:Positions[6]);
            root.PlayerObject.GetComponent<PlayerLocomotion>().enabled=false;
            using(var cpu=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Main Thread"))
            using(var gc=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame"))
            {
                double sum=0,max=0,cpuTotal=0;long gcTotal=0,gcMax=0;var live=0;
                for(var i=0;i<360;i++)
                {yield return null;var ms=Time.unscaledDeltaTime*1000d;sum+=ms;max=Math.Max(max,ms);cpuTotal+=cpu.LastValue/1000000d;
                    gcTotal+=gc.LastValue;gcMax=Math.Max(gcMax,gc.LastValue);live=Math.Max(live,root.EnemyPopulation.LiveEnemyCount);}
                var renderers=root.VisualEnvironment.GetRegion(region).Root.GetComponentsInChildren<MeshRenderer>();
                long triangles=0;foreach(var r in renderers){var mesh=r.GetComponent<MeshFilter>().sharedMesh;for(var j=0;j<mesh.subMeshCount;j++)triangles+=(long)mesh.GetIndexCount(j)/3;}
                Directory.CreateDirectory(Output+"/verification");File.WriteAllText(Output+"/verification/performance-"+region+"-editor.json",JsonUtility.ToJson(new Performance
                    {frames=360,meanFrameMs=sum/360,maxFrameMs=max,meanMainThreadMs=cpuTotal/360,gcBytes=gcTotal,gcMaxFrameBytes=gcMax,cpuValid=cpu.Valid,gcValid=gc.Valid,
                    maxLiveEnemies=live,sectorRenderers=renderers.Length,sectorTriangles=triangles,
                    method="Unity Editor batch play-mode 360-frame combat observation, no screenshot readback. Includes Editor/test overhead; inventory counts source objects, not submitted draws. Does not establish Android FPS, thermal behaviour or zero-allocation runtime."},true));
            }
            }
        }
        private void Move(Vector3 position)
        {var body=_scene.Root.PlayerObject.GetComponent<CharacterController>();body.enabled=false;body.transform.position=position;body.enabled=true;Physics.SyncTransforms();UnityEngine.Camera.main.GetComponent<PortraitFollowCamera>().SnapToTarget();}
        private static void Steer(RouteInput input,Vector3 delta)
        {var camera=UnityEngine.Camera.main.transform;var f=Vector3.ProjectOnPlane(camera.forward,Vector3.up).normalized;var r=Vector3.ProjectOnPlane(camera.right,Vector3.up).normalized;
            var direction=delta.normalized;input.Movement=new Vector2(Vector3.Dot(direction,r),Vector3.Dot(direction,f));}
        private byte[] Capture(string name,bool ui,bool video=false)
        {
            var root=_scene.Root;var camera=UnityEngine.Camera.main;root.MapIntegration.MapPresenter.RefreshNow();
            var canvases=root.GetComponentsInChildren<Canvas>(true);var enabled=canvases.Select(c=>c.enabled).ToArray();
            var canvas=root.GetComponentInChildren<Canvas>();var mode=canvas.renderMode;var priorCamera=canvas.worldCamera;var plane=canvas.planeDistance;
            var target=camera.targetTexture;var active=RenderTexture.active;
            // Match the SDR Android backbuffer. Reading an ARGBHalf linear
            // target straight into an sRGB PNG falsely darkens Linear captures.
            var render=new RenderTexture(540,960,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);var texture=new Texture2D(540,960,TextureFormat.RGB24,false);
            try
            {
                if(!ui)foreach(var item in canvases)item.enabled=false;
                canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
                camera.targetTexture=render;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=render;
                texture.ReadPixels(new Rect(0,0,540,960),0,0);texture.Apply();var bytes=texture.EncodeToPNG();
                var folder=Output+(video?"/video-frames":ui?"/gameplay-ui":"/gameplay-no-ui");Directory.CreateDirectory(folder);File.WriteAllBytes(folder+"/"+name+".png",bytes);return bytes;
            }
            finally
            {for(var i=0;i<canvases.Length;i++)canvases[i].enabled=enabled[i];canvas.renderMode=mode;canvas.worldCamera=priorCamera;canvas.planeDistance=plane;
                camera.targetTexture=target;RenderTexture.active=active;render.Release();Object.Destroy(render);Object.Destroy(texture);}
        }
        private sealed class RouteInput:IMovementInput {public Vector2 Movement{get;set;}}
        [Serializable] private sealed class Video {public int frames,attacks,deaths,maxEnemies;public float seconds;public string method;}
        [Serializable] private sealed class Performance {public int frames,maxLiveEnemies,sectorRenderers;public long gcBytes,gcMaxFrameBytes,sectorTriangles;public double meanFrameMs,maxFrameMs,meanMainThreadMs;public bool cpuValid,gcValid;public string method;}
    }
}
