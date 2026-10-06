using System.Linq;
using Gravivore.Editor.VisualIntegration;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class G0V21ReviewIsolationTests
    {
        [Test]
        public void ReviewModelIsExcludedFromProductionDependenciesAndBuildScenes()
        {
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(G0V21ScaleReview.Model);
            Assert.IsNotNull(model);
            Assert.IsEmpty(model.GetComponentsInChildren<Collider>(true));
            Assert.IsEmpty(model.GetComponentsInChildren<MonoBehaviour>(true));
            Assert.IsEmpty(model.GetComponentsInChildren<Animator>(true));
            var dependencies=AssetDatabase.GetDependencies("Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity",true);
            Assert.IsFalse(dependencies.Contains(G0V21ScaleReview.Model));
            Assert.IsFalse(EditorBuildSettings.scenes.Any(s=>s.path==G0V21ScaleReview.ScenePath));
            var importer=(ModelImporter)AssetImporter.GetAtPath(G0V21ScaleReview.Model);
            Assert.That(importer.globalScale,Is.EqualTo(1));
            Assert.IsFalse(importer.importAnimation);
        }

        [Test]
        public void SavedReviewHasPersistentMaterialsAndNoGameplayAuthority()
        {
            var scene=EditorSceneManager.OpenScene(G0V21ScaleReview.ScenePath);
            var roots=scene.GetRootGameObjects();
            Assert.IsEmpty(roots.SelectMany(r=>r.GetComponentsInChildren<Collider>(true)));
            Assert.IsEmpty(roots.SelectMany(r=>r.GetComponentsInChildren<MonoBehaviour>(true)).Where(b=>b.GetType().Name!="UniversalAdditionalCameraData"));
            foreach(var renderer in roots.SelectMany(r=>r.GetComponentsInChildren<Renderer>(true)))
                foreach(var material in renderer.sharedMaterials)
                    Assert.IsTrue(material!=null&&AssetDatabase.Contains(material),renderer.name+" material must survive scene reopening");
            var camera=roots.SelectMany(r=>r.GetComponentsInChildren<Camera>()).Single();
            Assert.That(camera.fieldOfView,Is.EqualTo(46));
            Assert.That(camera.transform.position,Is.EqualTo(new Vector3(0,14.8f,45.8f)));
        }
    }
}
