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
    public sealed class VisualReplacementV3SmokeTests
    {
        private CanonicalSceneTestScope _scene;
        [UnityTearDown] public IEnumerator Cleanup(){if(_scene!=null)yield return _scene.Cleanup();}
        private IEnumerator Load()
        {
            _scene=new CanonicalSceneTestScope();yield return _scene.Load();var root=_scene.Root;
            root.EnemyPopulation.enabled=false;root.MagnetarGuard.enabled=false;root.CustodianBoss.enabled=false;
            root.PlayerObject.GetComponent<PlayerLocomotion>().enabled=false;
            root.PlayerObject.GetComponent<GravityAttackController>().enabled=false;root.PlayerHealth.enabled=false;
            foreach(var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true))enemy.enabled=false;
        }
        [UnityTest] public IEnumerator RepairPrototypeHasLayeredArtAndPreservesAuthority()
        {
            yield return Load();var root=_scene.Root;
            var layer=root.VisualEnvironment.Floor.Find("Chapter 01 Visual Replacement V3");Assert.NotNull(layer);
            Assert.That(layer.GetComponentsInChildren<Collider>(true),Is.Empty);
            Assert.That(layer.GetComponentsInChildren<MonoBehaviour>(true),Is.Empty);
            var renderers=layer.GetComponentsInChildren<Renderer>(true);Assert.That(renderers.Length,Is.GreaterThan(10));
            foreach(var r in layer.Find("Layered worn deck").GetComponentsInChildren<Renderer>())
                if(r.name.Contains("platform")){Assert.That(r.bounds.size.x,Is.GreaterThan(5.9f));Assert.That(r.bounds.size.z,Is.GreaterThan(5.9f));Assert.That(r.bounds.size.y,Is.LessThan(.4f));}
            foreach(var r in renderers)
            {
                Assert.That(r.sharedMaterials.Length,Is.EqualTo(1));Assert.NotNull(r.sharedMaterial);
                Assert.That(r.sharedMaterial.shader.name,Is.EqualTo("Universal Render Pipeline/Lit"));
                Assert.That(r.sharedMaterial.shader.isSupported,Is.True);
            }
            var controller=root.PlayerObject.GetComponent<CharacterController>();controller.enabled=false;
            root.PlayerObject.transform.position=new Vector3(0,0,-28);controller.enabled=true;Physics.SyncTransforms();yield return null;
            root.PlayerHealth.ApplyDamage(new DamageRequest(35,DamageType.Physical));root.PlayerHealth.Tick(3.1f);root.PlayerHealth.Tick(.1f);
            root.RepairHub.Manipulators.Tick(.5f);yield return null;
            Capture(root,"01_repair_hub");
            Directory.CreateDirectory("docs/visual-replacement-v3/verification");
            File.WriteAllLines("docs/visual-replacement-v3/verification/repair_renderers.txt",renderers.Select(r=>r.name+" | "+r.bounds+" | "+r.sharedMaterial.name+" | base="+r.sharedMaterial.GetTexture("_BaseMap")?.name));
            File.WriteAllLines("docs/visual-replacement-v3/verification/floor_layers.txt",root.VisualEnvironment.Floor.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled&&r.bounds.Contains(new Vector3(0,r.bounds.center.y,-30))).Select(r=>r.name+" | "+r.bounds));
            // Existing repair-exit physics must remain open with the real capsule.
            foreach(var other in root.GetComponentsInChildren<CharacterController>(true))if(other!=controller)other.enabled=false;
            controller.enabled=false;controller.transform.position=new Vector3(0,0,-27);controller.enabled=true;
            for(var i=0;i<24;i++)controller.Move(Vector3.right*.25f);
            Assert.That(controller.transform.position.x,Is.EqualTo(6).Within(.1f));
        }
        internal static void Capture(S01SceneCompositionRoot root,string name)
        {
            var camera=UnityEngine.Camera.main;camera.GetComponent<PortraitFollowCamera>().SnapToTarget();
            root.MapIntegration.MapPresenter.RefreshNow();var canvas=root.GetComponentInChildren<Canvas>();
            var mode=canvas.renderMode;var priorCamera=canvas.worldCamera;var priorTarget=camera.targetTexture;var priorActive=RenderTexture.active;
            var render=new RenderTexture(540,960,24,RenderTextureFormat.ARGBHalf);var texture=new Texture2D(540,960,TextureFormat.RGB24,false);
            try
            {
                canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
                camera.targetTexture=render;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=render;
                texture.ReadPixels(new Rect(0,0,540,960),0,0);texture.Apply();Directory.CreateDirectory("docs/visual-replacement-v3/internal");
                File.WriteAllBytes("docs/visual-replacement-v3/internal/"+name+".png",texture.EncodeToPNG());
            }
            finally
            {canvas.renderMode=mode;canvas.worldCamera=priorCamera;camera.targetTexture=priorTarget;RenderTexture.active=priorActive;render.Release();Object.Destroy(render);Object.Destroy(texture);}
        }
    }
}
