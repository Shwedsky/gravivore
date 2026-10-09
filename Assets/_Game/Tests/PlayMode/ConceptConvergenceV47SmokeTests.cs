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
    public sealed class ConceptConvergenceV47SmokeTests
    {
        private CanonicalSceneTestScope _scene;
        private string _phase;
        [UnityTearDown] public IEnumerator Cleanup(){if(_scene!=null)yield return _scene.Cleanup();}
        [UnityTest] public IEnumerator ProductionAndCloseSurfaceReview()
        {
            _scene=new CanonicalSceneTestScope();yield return _scene.Load();var root=_scene.Root;
            _phase=System.Environment.GetEnvironmentVariable("GRAVIVORE_V47_CAPTURE_PHASE")??(root.VisualEnvironment.Floor.Find("Chapter 01 Concept Convergence V47")==null?"before":"after");
            root.PlayerHealth.SetDevelopmentGodMode(true);root.PlayerObject.GetComponent<PlayerLocomotion>().enabled=false;
            root.PlayerObject.GetComponent<GravityAttackController>().enabled=false;
            root.EnemyPopulation.enabled=false;root.MagnetarGuard.enabled=false;root.CustodianBoss.enabled=false;
            foreach(var e in root.GetComponentsInChildren<OrdinaryEnemyController>(true))e.enabled=false;
            Assert.That(root.WorldPresenter.Bounds.Size.x,Is.EqualTo(56));Assert.That(root.WorldPresenter.Bounds.Size.y,Is.EqualTo(118));
            Capture(root,"01_repair_hub");
            for(var i=0;i<5;i++)
            {var spot=root.EnemyPopulation.GetSpot(i);Move(root,spot.Position);yield return null;Capture(root,"sector_"+spot.Id);Close(root,"dock_"+spot.Id,spot.Position+new Vector3(0,2.0f,3.4f),11f);var enemy=spot.GetLiveEnemy(0);Assert.NotNull(enemy);Close(root,"actor_"+spot.Id,enemy.transform.position+Vector3.up*.8f,4.2f);}
            Move(root,new Vector3(0,0,-1.25f));yield return null;Capture(root,"04_service_corridor");Close(root,"corridor_machine",new Vector3(-3.7f,1.2f,2),5);
            root.WorldUnlocks.PrepareEliteEncounterForDevelopment();root.Chapter1Encounters.Tick();
            Move(root,root.MagnetarGuard.transform.position+Vector3.back*5);yield return null;Capture(root,"05_magnetar_encounter");Close(root,"magnetar_surface",root.MagnetarGuard.transform.position+Vector3.up*.8f,4.7f);
            root.MagnetarGuard.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));
            Move(root,root.WorldPresenter.Configuration.BossArenaCenter+Vector3.back*4.5f);root.CustodianBoss.Tick(0);yield return null;Capture(root,"06_custodian_arena");Close(root,"custodian_surface",root.CustodianBoss.transform.position+Vector3.up*.9f,6.4f);
            if(_phase=="after")
            {
                var layer=root.VisualEnvironment.Floor.Find("Chapter 01 Concept Convergence V47");
                Assert.IsEmpty(layer.GetComponentsInChildren<Collider>(true));Assert.IsEmpty(layer.GetComponentsInChildren<MonoBehaviour>(true));Assert.IsEmpty(layer.GetComponentsInChildren<Light>(true));
                foreach(var family in new[]{"DeploymentBay","FabricationBay","InductionStation"})
                {
                    var t=layer.GetComponentsInChildren<Transform>().FirstOrDefault(n=>n.name.StartsWith("V47 "+family+" / ")&&n.GetComponentsInChildren<Renderer>().Any(r=>r.enabled));Assert.NotNull(t,family);
                    var rs=t.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);
                    Close(root,"hero_"+family,b.center,Mathf.Max(b.size.x,b.size.y,b.size.z)*1.8f);
                }
            }
        }
        private static void Move(S01SceneCompositionRoot root,Vector3 p)
        {var b=root.PlayerObject.GetComponent<CharacterController>();b.enabled=false;b.transform.position=p;b.enabled=true;Physics.SyncTransforms();}
        private void Capture(S01SceneCompositionRoot root,string name)
        {var c=UnityEngine.Camera.main;c.GetComponent<PortraitFollowCamera>().SnapToTarget();root.MapIntegration.MapPresenter.RefreshNow();Save(root,c,name,540,960);}
        private void Close(S01SceneCompositionRoot root,string name,Vector3 target,float distance)
        {
            var c=UnityEngine.Camera.main;var p=c.transform.position;var q=c.transform.rotation;var fov=c.fieldOfView;var follow=c.GetComponent<PortraitFollowCamera>();var enabled=follow.enabled;
            var canvases=root.GetComponentsInChildren<Canvas>(true);var states=canvases.Select(x=>x.enabled).ToArray();
            try{follow.enabled=false;foreach(var x in canvases)x.enabled=false;c.transform.position=target+new Vector3(.45f,.65f,-1).normalized*distance;c.transform.LookAt(target);c.fieldOfView=40;Save(root,c,name,960,960);}
            finally{c.transform.SetPositionAndRotation(p,q);c.fieldOfView=fov;follow.enabled=enabled;for(var i=0;i<canvases.Length;i++)canvases[i].enabled=states[i];}
        }
        private void Save(S01SceneCompositionRoot root,UnityEngine.Camera camera,string name,int width,int height)
        {
            var canvas=root.GetComponentInChildren<Canvas>();var mode=canvas.renderMode;var priorCamera=canvas.worldCamera;var plane=canvas.planeDistance;var priorTarget=camera.targetTexture;var priorActive=RenderTexture.active;
            var render=new RenderTexture(width,height,24,RenderTextureFormat.ARGBHalf);var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
            try{canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;camera.targetTexture=render;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=render;texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();var dir="docs/history/implementation-passes/chapter01-visual-replacement-v3/v47/internal/"+_phase;Directory.CreateDirectory(dir);File.WriteAllBytes(dir+"/"+name+".png",texture.EncodeToPNG());}
            finally{canvas.renderMode=mode;canvas.worldCamera=priorCamera;canvas.planeDistance=plane;camera.targetTexture=priorTarget;RenderTexture.active=priorActive;render.Release();Object.Destroy(render);Object.Destroy(texture);}
        }
    }
}

