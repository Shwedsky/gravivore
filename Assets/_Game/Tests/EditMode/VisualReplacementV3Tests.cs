using System.Linq;
using Gravivore.Editor.VisualIntegration;
using Gravivore.Presentation.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class VisualReplacementV3Tests
    {
        [Test] public void ProductionContractsAndRedistributableDependenciesAreValid()=>VisualReplacementV3Audit.ValidateOrThrow();
        [Test] public void FiveRanksHaveDistinctGeometryMatchingInventoryRenders()
        {
            var definition=AssetDatabase.LoadAssetAtPath<ChapterVisualIntegrationDefinition>("Assets/_Game/Content/Definitions/Chapter01_VisualIntegration.asset");
            var prefab=definition.WeaponPrefab;var counts=new long[5];var meshes=new Mesh[5];
            for(var rank=1;rank<=5;rank++)
            {
                var group=prefab.transform.Find("Rank"+rank);Assert.NotNull(group);Assert.NotNull(group.Find("RankMuzzle"));
                var mesh=group.GetComponentInChildren<MeshFilter>(true).sharedMesh;meshes[rank-1]=mesh;counts[rank-1]=(long)mesh.GetIndexCount(0);
                Assert.That(mesh.subMeshCount,Is.EqualTo(1));Assert.That(mesh.bounds.size.magnitude,Is.GreaterThan(.9f));
                Assert.NotNull(definition.UiSkin.WeaponThumbnail(rank));
                StringAssert.Contains("M0_Thumbnail"+rank,AssetDatabase.GetAssetPath(definition.UiSkin.WeaponThumbnail(rank)));
            }
            Assert.That(meshes.Distinct().Count(),Is.EqualTo(5));Assert.That(counts.Distinct().Count(),Is.EqualTo(5));
            Assert.That(prefab.transform.Find("Rank5/RankMuzzle").localPosition.z,Is.GreaterThan(prefab.transform.Find("Rank1/RankMuzzle").localPosition.z+.5f));
        }
    }
}
