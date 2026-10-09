using System;
using System.Collections;
using System.IO;
using System.Linq;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.Camera;
using Gravivore.Presentation.Composition;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace Gravivore.Tests.PlayMode
{
    public sealed class ConceptCorrectiveV45SmokeTests
    {
        private CanonicalSceneTestScope _scene;
        private const string Output="docs/history/implementation-passes/chapter01-visual-replacement-v3/v45";
        [UnityTearDown] public IEnumerator Cleanup(){if(_scene!=null)yield return _scene.Cleanup();}
        private IEnumerator Load()
        {
            _scene=new CanonicalSceneTestScope();yield return _scene.Load();var root=_scene.Root;
            root.PlayerHealth.SetDevelopmentGodMode(true);root.PlayerObject.GetComponent<PlayerLocomotion>().enabled=false;
            root.PlayerObject.GetComponent<GravityAttackController>().enabled=false;
            root.EnemyPopulation.enabled=false;root.MagnetarGuard.enabled=false;root.CustodianBoss.enabled=false;
            foreach(var e in root.EnemyPopulation.GetComponentsInChildren<OrdinaryEnemyController>(true))e.enabled=false;
        }
        private static void Move(S01SceneCompositionRoot root,Vector3 p)
        {var b=root.PlayerObject.GetComponent<CharacterController>();b.enabled=false;b.transform.position=p;b.enabled=true;Physics.SyncTransforms();}
        [UnityTest] public IEnumerator ProductionCameraSectorReviewAndBoundedIdleLife()
        {
            yield return Load();var root=_scene.Root;var env=root.VisualEnvironment;
            var layer=env.BlueprintWorldOnly?env.Floor:env.Floor.Find("Chapter 01 Active Industrial Facility V45");Assert.NotNull(layer);
            Assert.IsEmpty(layer.GetComponentsInChildren<Collider>(true));Assert.IsEmpty(layer.GetComponentsInChildren<MonoBehaviour>(true));
            Assert.That(layer.GetComponentsInChildren<Light>(true).Length,Is.Zero,"Reuse the existing two local lights.");
            Assert.That(root.WorldPresenter.Bounds.Size.x,Is.EqualTo(root.WorldPresenter.Configuration.GroundSize.x));
            if(!env.BlueprintWorldOnly)Assert.That(root.WorldPresenter.Configuration.EliteGate.Position.z-root.RepairHub.RepairPosition.z,Is.EqualTo(68));
            else Assert.IsNull(env.Floor.Find("Chapter 01 Active Industrial Facility V45"));
            Assert.That(root.WorldPresenter.Configuration.BossGate.Position.z-root.WorldPresenter.Configuration.EliteGate.Position.z,Is.EqualTo(20));
            var indices=new[]{3,4,0,2,1};
            Capture(root,"01_repair_hub");
            foreach(var i in indices)
            {var spot=root.EnemyPopulation.GetSpot(i);Move(root,spot.Position);yield return null;Capture(root,"sector_"+spot.Id);
                if(env.BlueprintWorldOnly)Assert.IsNotEmpty(env.GetRegion(spot.Id).Root.GetComponentsInChildren<Renderer>());
                else Assert.NotNull(layer.Find("Serviced enemy dock "+spot.Id));}
            Move(root,root.EnemyPopulation.GetSpot(0).Position);yield return null;Capture(root,"02_ordinary_sector");
            Move(root,root.EnemyPopulation.GetSpot(1).Position);yield return null;Capture(root,"03_strong_ordinary");
            Move(root,new Vector3(0,0,-1.25f));yield return null;Capture(root,"04_service_corridor");FloorCoverage(root,"04_service_corridor");
            root.WorldUnlocks.PrepareEliteEncounterForDevelopment();root.Chapter1Encounters.Tick();
            Move(root,root.MagnetarGuard.transform.position+Vector3.back*5);yield return null;Capture(root,"05_magnetar_encounter");
            root.MagnetarGuard.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));
            Move(root,root.WorldPresenter.Configuration.BossArenaCenter+Vector3.back*4.5f);root.CustodianBoss.Tick(0);yield return null;Capture(root,"06_custodian_arena");
            var map=root.MapIntegration.MapPresenter;map.OpenExpanded();map.RefreshNow();Capture(root,"07_full_map");map.CloseExpanded();
            // Actual runtime controllers, no visual-only fake drift. Keep the player far enough
            // away to test idle motion independently of the accepted aggression policy.
            Move(root,root.RepairHub.RepairPosition);var spot0=root.EnemyPopulation.GetSpot(0);var enemy=spot0.GetLiveEnemy(0);Assert.NotNull(enemy);
            var start=enemy.transform.position;var count=root.EnemyPopulation.GetComponentsInChildren<Transform>(true).Length;
            var hp=enemy.CurrentHitPoints;var moved=false;var paused=false;
            for(var i=0;i<900;i++)
            {
                var before=enemy.transform.position;enemy.Tick(.016f);
                Assert.That(enemy.BrainState,Is.EqualTo(OrdinaryEnemyBrainState.Idle));
                Assert.That(Vector3.Distance(enemy.transform.position,enemy.AmbientHome),Is.LessThanOrEqualTo(enemy.AmbientRadius+.05f));
                if(Vector3.Distance(before,enemy.transform.position)>.001f)moved=true;else paused=true;
                if(i==0||i==450||i==899)
                {Move(root,spot0.Position+Vector3.back*7);Capture(root,"idle_"+i);Move(root,root.RepairHub.RepairPosition);}
            }
            Assert.IsTrue(moved);Assert.IsTrue(paused);Assert.That(enemy.CurrentHitPoints,Is.EqualTo(hp));Assert.That(root.EnemyPopulation.GetComponentsInChildren<Transform>(true).Length,Is.EqualTo(count));
            Directory.CreateDirectory(Output+"/verification");
            File.WriteAllText(Output+"/verification/idle_life.json",JsonUtility.ToJson(new IdleEvidence{validated=true,seconds=14.4f,start=start,end=enemy.transform.position,radius=enemy.AmbientRadius,observedMovement=moved,observedPauses=paused,transformsBefore=count,transformsAfter=count},true));
        }
        [Serializable] private sealed class IdleEvidence{public bool validated,observedMovement,observedPauses;public float seconds,radius;public Vector3 start,end;public int transformsBefore,transformsAfter;}
        private static void FloorCoverage(S01SceneCompositionRoot root,string name)
        {
            var camera=UnityEngine.Camera.main;var priorTarget=camera.targetTexture;var priorActive=RenderTexture.active;var background=camera.backgroundColor;
            var rs=root.GetComponentsInChildren<Renderer>(true);var originals=rs.Select(r=>r.sharedMaterials).ToArray();var canvases=root.GetComponentsInChildren<Canvas>(true);var canvasStates=canvases.Select(c=>c.enabled).ToArray();
            var white=new Material(Shader.Find("Universal Render Pipeline/Unlit"));white.SetColor("_BaseColor",Color.white);
            var black=new Material(white);black.SetColor("_BaseColor",Color.black);var render=new RenderTexture(270,480,24);var texture=new Texture2D(270,480,TextureFormat.RGB24,false);
            try
            {
                for(var i=0;i<rs.Length;i++)
                {var r=rs[i];var floor=r.transform.IsChildOf(root.VisualEnvironment.Floor)&&r.bounds.size.y<.4f&&Mathf.Max(r.bounds.size.x,r.bounds.size.z)>1.5f;r.sharedMaterials=Enumerable.Repeat(floor?white:black,originals[i].Length).ToArray();}
                foreach(var c in canvases)c.enabled=false;camera.backgroundColor=Color.black;camera.targetTexture=render;camera.Render();RenderTexture.active=render;texture.ReadPixels(new Rect(0,0,270,480),0,0);texture.Apply();
                var pixels=texture.GetPixels32();var fraction=(float)pixels.Count(p=>p.r>180&&p.g>180&&p.b>180)/pixels.Length;
                Directory.CreateDirectory(Output+"/verification");File.WriteAllText(Output+"/verification/"+name+"_floor_coverage.json","{\"visibleFlatDeckFraction\":"+fraction.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+",\"method\":\"Production camera semantic material mask; UI hidden; all flat deck counts as floor, including worn and marked panels\"}");
                File.WriteAllBytes(Output+"/internal/after/"+name+"_floor_mask.png",texture.EncodeToPNG());
            }
            finally
            {for(var i=0;i<rs.Length;i++)rs[i].sharedMaterials=originals[i];for(var i=0;i<canvases.Length;i++)canvases[i].enabled=canvasStates[i];camera.backgroundColor=background;camera.targetTexture=priorTarget;RenderTexture.active=priorActive;render.Release();Object.Destroy(render);Object.Destroy(texture);Object.Destroy(white);Object.Destroy(black);}
        }
        internal static void Capture(S01SceneCompositionRoot root,string name)
        {
            var camera=UnityEngine.Camera.main;camera.GetComponent<PortraitFollowCamera>().SnapToTarget();root.MapIntegration.MapPresenter.RefreshNow();
            var canvas=root.GetComponentInChildren<Canvas>();var mode=canvas.renderMode;var priorCamera=canvas.worldCamera;var plane=canvas.planeDistance;var priorTarget=camera.targetTexture;var priorActive=RenderTexture.active;
            var render=new RenderTexture(540,960,24,RenderTextureFormat.ARGBHalf);var texture=new Texture2D(540,960,TextureFormat.RGB24,false);
            try
            {
                canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;camera.targetTexture=render;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=render;
                texture.ReadPixels(new Rect(0,0,540,960),0,0);texture.Apply();Directory.CreateDirectory(Output+"/internal/after");File.WriteAllBytes(Output+"/internal/after/"+name+".png",texture.EncodeToPNG());
            }
            finally{canvas.renderMode=mode;canvas.worldCamera=priorCamera;canvas.planeDistance=plane;camera.targetTexture=priorTarget;RenderTexture.active=priorActive;render.Release();Object.Destroy(render);Object.Destroy(texture);}
        }
    }
}
