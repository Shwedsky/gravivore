using System.Collections;
using System.IO;
using System.Linq;
using Gravivore.Presentation.AudioVfx;
using Gravivore.Presentation.Combat;
using Gravivore.Presentation.Development;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Gravivore.Tests.PlayMode
{
    public sealed class RenderingHotfixSmokeTests
    {
        private CanonicalSceneTestScope _scene;
        [UnityTearDown]public IEnumerator Cleanup(){if(_scene!=null)yield return _scene.Cleanup();}
        [UnityTest]public IEnumerator CanonicalBootMaterialsAndV3BoundsAreValidWithoutFullscreenMeshAtSpawn()
        {
            _scene=new CanonicalSceneTestScope();yield return _scene.Load();var root=_scene.Root;
            var diagnostics=root.GetComponent<RenderingBootDiagnostics>();Assert.NotNull(diagnostics);Assert.IsTrue(diagnostics.HasLogged);Assert.That(diagnostics.MaterialCount,Is.GreaterThan(10));
            var prior=diagnostics.MaterialCount;diagnostics.Initialize(root.transform,Camera.main);Assert.That(diagnostics.MaterialCount,Is.EqualTo(prior));
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))foreach(var material in renderer.sharedMaterials)
            {Assert.NotNull(material,renderer.name);Assert.NotNull(material.shader,renderer.name);Assert.AreNotEqual("Hidden/InternalErrorShader",material.shader.name);Assert.IsTrue(material.shader.isSupported,renderer.name);}
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true).Where(r=>r.name.StartsWith("Custodian_V3")||r.name.StartsWith("Emitter_M0")||r.name.Contains("Broken_Edge_V3")||r.name.Contains("Service_Trench_V3")||r.name.Contains("Collapsed_Hull_V3")))
            {Assert.That(renderer.bounds.extents.magnitude,Is.LessThan(40),renderer.name);Assert.IsFalse(renderer.bounds.Contains(Camera.main.transform.position),renderer.name);}
        }
        [UnityTest]public IEnumerator HostileChargeTravelAndImpactPoolConstructsFullyInactiveWithNoDefaultLineEndpoints()
        {
            _scene=new CanonicalSceneTestScope();yield return _scene.Load();
            var owner=new GameObject("isolated render pool");var pool=owner.AddComponent<Phase6BVfxPool>();
            try{
                var production=_scene.Root.GetComponentInChildren<AttackCausalityPresenter>();
                var prototypes=production.GetComponent<Phase6BVfxPool>().GetComponentsInChildren<Phase6BVfxInstance>(true);
                var charge=prototypes.First(p=>p.Cue==Phase6BVfxCue.HostileCharge);var travel=prototypes.First(p=>p.Cue==Phase6BVfxCue.HostileTravel);var impact=prototypes.First(p=>p.Cue==Phase6BVfxCue.HostileImpact);
                pool.Initialize(new[]{new Phase6BVfxPool.Binding(charge.Cue,charge,4),new Phase6BVfxPool.Binding(travel.Cue,travel,6),new Phase6BVfxPool.Binding(impact.Cue,impact,6)});
                Assert.That(pool.CreatedInstanceCount,Is.EqualTo(16));Assert.That(pool.ActiveCount,Is.Zero);
                foreach(var instance in owner.GetComponentsInChildren<Phase6BVfxInstance>(true)){Assert.IsFalse(instance.gameObject.activeSelf);Assert.IsFalse(instance.IsPlaying);}
                foreach(var line in owner.GetComponentsInChildren<LineRenderer>(true))Assert.IsFalse(line.gameObject.activeInHierarchy);
                foreach(var particles in owner.GetComponentsInChildren<ParticleSystem>(true)){Assert.IsFalse(particles.isPlaying);Assert.That(particles.particleCount,Is.Zero);}
            }finally{Object.Destroy(owner);}
        }
        [UnityTest]public IEnumerator InvalidShaderOnTransparentFullscreenUiReproducesMagentaAndRestoringRealUiShaderRecoversFrame()
        {
            _scene=new CanonicalSceneTestScope();yield return _scene.Load();var root=_scene.Root;var canvas=root.GetComponentInChildren<Canvas>();var camera=Camera.main;
            var priorMode=canvas.renderMode;var priorCamera=canvas.worldCamera;var priorTarget=camera.targetTexture;var priorActive=RenderTexture.active;
            var panel=new GameObject("transparent fullscreen reproduction",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));panel.transform.SetParent(canvas.transform,false);
            var rect=(RectTransform)panel.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            panel.layer=canvas.gameObject.layer;
            var image=panel.GetComponent<Image>();image.color=new Color(1,1,1,.01f);image.raycastTarget=false;image.canvasRenderer.cullTransparentMesh=false;
            var error=new Material(Shader.Find("Hidden/InternalErrorShader"));var target=new RenderTexture(270,480,24,RenderTextureFormat.ARGB32);var pixels=new Texture2D(270,480,TextureFormat.RGB24,false);
            try{
                canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;camera.targetTexture=target;
                image.material=error;yield return null;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,270,480),0,0);pixels.Apply();
                Directory.CreateDirectory("docs/render-hotfix/internal");File.WriteAllBytes("docs/render-hotfix/internal/ui_error_reproduction_editor.png",pixels.EncodeToPNG());
                Assert.That(MagentaFraction(pixels),Is.GreaterThan(.85f),"Error shader must cover the camera despite nearly transparent UI color.");
                image.material=null;Canvas.ForceUpdateCanvases();camera.Render();pixels.ReadPixels(new Rect(0,0,270,480),0,0);pixels.Apply();
                Assert.That(MagentaFraction(pixels),Is.LessThan(.01f));Assert.IsTrue(image.material.shader.isSupported);Assert.That(image.material.shader.name,Is.EqualTo("UI/Default"));
                File.WriteAllBytes("docs/render-hotfix/internal/ui_restored_editor.png",pixels.EncodeToPNG());
            }finally{canvas.renderMode=priorMode;canvas.worldCamera=priorCamera;camera.targetTexture=priorTarget;RenderTexture.active=priorActive;target.Release();Object.Destroy(target);Object.Destroy(pixels);Object.Destroy(error);Object.Destroy(panel);}
        }
        private static float MagentaFraction(Texture2D image){var colors=image.GetPixels32();return colors.Count(c=>c.r>180&&c.b>180&&c.g<90)/(float)colors.Length;}
    }
}
