using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class Chapter01ArchitectureProductionTests
    {
        private const string Prefab = "Assets/_Game/Content/Chapter01Architecture/EnergyChamber/Prefabs/EnergyChamber_Production.prefab";
        [Test] public void RuntimeClosureHasNoProofScenesCamerasOrEditorHelpers()
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            Assert.That(asset, Is.Not.Null);
            Assert.That(AssetDatabase.GetDependencies(Prefab, true).Any(p => p.StartsWith("Assets/") && (p.Contains("/ArtProof/") || p.Contains("/Editor/") || p.EndsWith(".unity"))), Is.False);
            Assert.That(asset.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
            Assert.That(asset.GetComponentsInChildren<Camera>(true), Is.Empty);
            Assert.That(asset.GetComponentsInChildren<MeshCollider>(true), Is.Empty);
            var collision = asset.GetComponentsInChildren<Collider>(true);
            Assert.That(collision.Length, Is.EqualTo(25));
            Assert.That(collision.All(c => c is BoxCollider && !c.isTrigger && c.enabled), Is.True);
        }
        [Test] public void DistanceLevelsReduceCostWithoutChangingAcceptedNearPresentation()
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var lods = asset.GetComponent<LODGroup>().GetLODs();
            Assert.That(lods.Length, Is.EqualTo(3));
            Assert.That(lods[0].renderers.Length, Is.EqualTo(45));
            var costs = lods.Select(l => l.renderers.Sum(r => r.GetComponent<MeshFilter>().sharedMesh.triangles.Length / 3)).ToArray();
            Assert.That(costs[0], Is.EqualTo(279694));
            Assert.That(costs[1], Is.LessThan(costs[0]));
            Assert.That(costs[2], Is.LessThan(costs[1]));
            for (var i = 1; i < lods.Length; i++) Assert.That(lods[i].renderers.All(r => lods[0].renderers.Contains(r)), Is.True);
            Assert.That(lods[2].renderers.Count(r => AssetDatabase.GetAssetPath(r.GetComponent<MeshFilter>().sharedMesh).EndsWith("EC_VerticalDistribution.fbx")), Is.EqualTo(4));
        }
    }
}
