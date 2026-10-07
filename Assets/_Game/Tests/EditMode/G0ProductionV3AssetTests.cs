using System.Linq;
using Gravivore.Editor.VisualIntegration;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class G0ProductionV3AssetTests
    {
        [Test]
        public void ProductionDependenciesIncludeApprovedV3ModelAndExcludeReviewScene()
        {
            var deps=AssetDatabase.GetDependencies("Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity",true);
            Assert.Contains(G0ProductionV3Review.Model, deps);
            Assert.IsFalse(deps.Contains(G0ProductionV3Review.ScenePath));
            Assert.IsFalse(EditorBuildSettings.scenes.Any(s=>s.path==G0ProductionV3Review.ScenePath));
        }

        [Test]
        public void ImportedSkinHasCompleteVertexChannelsAndStrictLodReductions()
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(G0ProductionV3Review.Prefab);Assert.IsNotNull(prefab);
            Assert.IsEmpty(prefab.GetComponentsInChildren<Collider>(true));
            Assert.IsEmpty(prefab.GetComponentsInChildren<MonoBehaviour>(true));
            var lod=prefab.GetComponent<LODGroup>();Assert.IsNotNull(lod);Assert.That(lod.GetLODs().Length,Is.EqualTo(3));
            var skins=prefab.GetComponentsInChildren<SkinnedMeshRenderer>().OrderBy(r=>r.name).ToArray();
            Assert.That(skins.Length,Is.EqualTo(3));var previous=int.MaxValue;
            foreach(var skin in skins)
            {
                var mesh=skin.sharedMesh;var count=(int)mesh.GetIndexCount(0)/3;
                Assert.That(count,Is.LessThan(previous));previous=count;
                Assert.That(mesh.subMeshCount,Is.EqualTo(1));Assert.That(skin.sharedMaterials.Length,Is.EqualTo(1));
                Assert.That(mesh.uv.Length,Is.EqualTo(mesh.vertexCount));
                Assert.That(mesh.normals.Length,Is.EqualTo(mesh.vertexCount));Assert.That(mesh.tangents.Length,Is.EqualTo(mesh.vertexCount));
                Assert.IsTrue(mesh.uv.All(u=>u.x>=-.00001f&&u.x<=1.00001f&&u.y>=-.00001f&&u.y<=1.00001f));
                Assert.That(skin.bones.Length,Is.EqualTo(18));
                Assert.IsTrue(mesh.boneWeights.All(w=>Mathf.Abs(w.weight0-1)<.0001f&&w.weight1==0&&w.weight2==0&&w.weight3==0));
                Assert.IsTrue(skin.sharedMaterials.All(m=>m!=null&&AssetDatabase.Contains(m)));
            }
            var importer=(ModelImporter)AssetImporter.GetAtPath(G0ProductionV3Review.Model);
            Assert.That(importer.globalScale,Is.EqualTo(1));Assert.IsTrue(importer.importAnimation);
            Assert.That(importer.animationType,Is.EqualTo(ModelImporterAnimationType.Generic));
        }

        [Test]
        public void VisualClipsRemainInPlaceAndLoopWithoutTransformJumps()
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(G0ProductionV3Review.Prefab);
            var instance=Object.Instantiate(prefab);
            try
            {
                var animator=instance.GetComponent<Animator>();animator.enabled=true;
                var clips=AssetDatabase.LoadAllAssetsAtPath(G0ProductionV3Review.Model).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();
                Assert.That(clips.Select(c=>c.name),Is.EquivalentTo(G0ProductionV3Review.ClipNames));
                var root=instance.GetComponentsInChildren<Transform>().Single(t=>t.name=="ROOT");var origin=root.localPosition;
                foreach(var clip in clips)
                {
                    for(var i=0;i<=8;i++){Sample(animator,clip.name,i/8f);Assert.That(Vector3.Distance(root.localPosition,origin),Is.LessThan(.0001f),clip.name);}
                    Assert.That(clip.isLooping,Is.EqualTo(clip.name=="Idle"||clip.name=="Run"));
                    if(!clip.isLooping)continue;
                    Sample(animator,clip.name,0);var transforms=instance.GetComponentsInChildren<Transform>();
                    var positions=transforms.Select(t=>t.localPosition).ToArray();var rotations=transforms.Select(t=>t.localRotation).ToArray();
                    Sample(animator,clip.name,1);
                    for(var i=0;i<transforms.Length;i++)
                    {
                        Assert.That(Vector3.Distance(positions[i],transforms[i].localPosition),Is.LessThan(.001f),clip.name+" "+transforms[i].name);
                        Assert.That(Quaternion.Angle(rotations[i],transforms[i].localRotation),Is.LessThan(.1f),clip.name+" "+transforms[i].name);
                    }
                }
            }
            finally {Object.DestroyImmediate(instance);}
        }

        private static void Sample(Animator animator,string state,float normalizedTime)
        {
            animator.Rebind();animator.Update(0);animator.Play(state,0,normalizedTime);animator.Update(0);
        }

        [Test]
        public void SavedReviewUsesFrozenCameraAndPresentationFit()
        {
            var scene=EditorSceneManager.OpenScene(G0ProductionV3Review.ScenePath);
            var roots=scene.GetRootGameObjects();var camera=roots.SelectMany(r=>r.GetComponentsInChildren<Camera>()).Single();
            Assert.That(camera.fieldOfView,Is.EqualTo(46));Assert.That(camera.transform.position,Is.EqualTo(new Vector3(0,14.8f,45.8f)));
            var hero=roots.Single(r=>r.name=="G-0 V3 PRESENTATION ONLY");Assert.That(hero.transform.localScale,Is.EqualTo(Vector3.one*G0ProductionV3Review.PresentationFit));
            Assert.IsEmpty(roots.SelectMany(r=>r.GetComponentsInChildren<Collider>(true)));
            Assert.IsFalse(hero.GetComponent<Animator>().applyRootMotion);
        }
    }
}
