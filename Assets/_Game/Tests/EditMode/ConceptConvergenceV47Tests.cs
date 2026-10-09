using System.IO;
using System.Linq;
using Gravivore.Editor.VisualIntegration;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class ConceptConvergenceV47Tests
    {
        [Test] public void AllActorSkinsRetainPbrAndValidPresentationBindings()=>ConceptConvergenceV47Builder.AuditActors();
        [Test] public void ProductionFacilitiesPreserveCollisionAndRuntimeSourceAuthority()=>ConceptConvergenceV47Builder.Audit();
        [TestCase("DeploymentBay")][TestCase("FabricationBay")][TestCase("InductionStation")]
        public void FacilitiesRemainRichPresentationWithoutGameplayAuthority(string family)
        {
            var obj=AssetDatabase.LoadAssetAtPath<GameObject>(ConceptConvergenceV47Builder.Root+"/Prefabs/V47_"+family+".prefab");Assert.NotNull(obj);
            Assert.IsEmpty(obj.GetComponentsInChildren<Collider>(true));Assert.IsEmpty(obj.GetComponentsInChildren<MonoBehaviour>(true));Assert.IsEmpty(obj.GetComponentsInChildren<Light>(true));
            var r=obj.GetComponentInChildren<Renderer>();Assert.That(r.bounds.size.x,Is.GreaterThan(7));Assert.That(r.bounds.size.y,Is.GreaterThan(4));
            Assert.That(r.sharedMaterial.GetTexture("_BumpMap"),Is.Not.Null);Assert.That(r.sharedMaterial.GetTexture("_OcclusionMap"),Is.Not.Null);
        }
        [TestCase("VisualSlice","Scout_V1",15)]
        [TestCase("VisualSlice","Cutter_V1",13)]
        [TestCase("Chapter01Production","Warden_V1",11)]
        [TestCase("Chapter01Production","ArcDrone_V1",7)]
        [TestCase("Chapter01Production","Carrier_V1",9)]
        [TestCase("VisualSlice","Magnetar_V1",17)]
        [TestCase("Chapter01V3","Custodian_V3",14)]
        public void AuthoredActorsKeepRigidLodsBonesAndExistingAttackAnimation(string folder,string name,int bones)
        {
            Assert.IsTrue(File.Exists("art/concept-convergence-v47/"+name+".blend"));
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Content/"+folder+"/Prefabs/"+name+".prefab");
            var animator=prefab.GetComponent<Animator>();Assert.NotNull(animator);Assert.IsFalse(animator.applyRootMotion);
            CollectionAssert.Contains(animator.runtimeAnimatorController.animationClips.Select(c=>c.name).ToArray(),"Attack");
            var lods=prefab.GetComponent<LODGroup>().GetLODs();Assert.That(lods.Length,Is.EqualTo(3));
            long last=long.MaxValue;
            foreach(var lod in lods)
            {
                var r=(SkinnedMeshRenderer)lod.renderers.Single();Assert.That(r.bones.Length,Is.EqualTo(bones));
                Assert.IsTrue(r.bones.All(b=>b!=null));Assert.That(r.sharedMesh.subMeshCount,Is.EqualTo(1));
                var count=(long)r.sharedMesh.GetIndexCount(0)/3;Assert.That(count,Is.GreaterThan(0).And.LessThan(last));last=count;
                Assert.That(r.sharedMaterial.name,Is.EqualTo(name=="Magnetar_V1"?"V46_HeroAmber":"V46_HeroRed"));
            }
        }
    }
}
