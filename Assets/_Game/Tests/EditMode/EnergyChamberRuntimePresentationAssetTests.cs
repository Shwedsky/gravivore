using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class EnergyChamberRuntimePresentationAssetTests
    {
        private const string Prefab="Assets/_Game/Content/Chapter01Architecture/EnergyChamber/Prefabs/EnergyChamber_Production.prefab";
        [Test] public void IdentityAssembliesSurviveAutomaticLodAndCostStillFallsAtRange()
        {
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var group=asset.GetComponent<LODGroup>();var lods=group.GetLODs();
            Assert.That(lods.Select(l=>l.screenRelativeTransitionHeight),Is.EqualTo(new[]{.26f,.13f,.035f}));
            Assert.That(group.size,Is.EqualTo(11.480001f).Within(.0001f));
            var critical=new[]{"EC_ChamberPolish.fbx","EC_NodePolish.fbx","Prop_Rail_Round_Big.fbx","Platform_Stairs_4.fbx","EC_VerticalDistribution.fbx"};
            foreach(var r in lods[0].renderers.Where(r=>critical.Contains(Path.GetFileName(AssetDatabase.GetAssetPath(r.GetComponent<MeshFilter>().sharedMesh)))))
                for(var level=1;level<3;level++)Assert.That(lods[level].renderers.Contains(r),Is.True,r.name+" lost at LOD"+level);
            var costs=lods.Select(l=>l.renderers.Sum(r=>Enumerable.Range(0,r.GetComponent<MeshFilter>().sharedMesh.subMeshCount).Sum(s=>(int)r.GetComponent<MeshFilter>().sharedMesh.GetIndexCount(s)/3))).ToArray();
            Assert.That(costs[0],Is.EqualTo(279694));Assert.That(costs[1],Is.LessThan(costs[0]));Assert.That(costs[2],Is.LessThan(costs[1]));
        }
    }
}
