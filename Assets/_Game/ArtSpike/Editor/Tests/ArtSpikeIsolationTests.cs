using System.Linq;
using Gravivore.ArtSpike.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gravivore.ArtSpike.Tests
{
    public sealed class ArtSpikeIsolationTests
    {
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void CharacterHasOnlyPresentationGeometryAndOwnedMaterials(int index)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArtSpikeBuilder.CharacterPaths[index]);
            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
            Assert.That(prefab.GetComponentsInChildren<Collider>(true), Is.Empty);
            Assert.That(prefab.GetComponentsInChildren<Rigidbody>(true), Is.Empty);
            Assert.That(prefab.GetComponentsInChildren<Light>(true), Is.Empty);
            Assert.That(prefab.GetComponentsInChildren<Camera>(true), Is.Empty);
            Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab), Is.Zero);
            foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                Assert.That(renderer.sharedMaterials, Is.Not.Empty);
                foreach (var material in renderer.sharedMaterials)
                {
                    Assert.That(material, Is.Not.Null);
                    Assert.That(AssetDatabase.GetAssetPath(material), Does.StartWith(ArtSpikeBuilder.Root + "/Materials/"));
                    Assert.That(material.shader.name, Is.EqualTo("Universal Render Pipeline/Lit"));
                }
            }
        }

        [Test]
        public void EveryTierRetainsTheSameCoreAndFourSupportSockets()
        {
            Vector3? position = null;
            Vector3? scale = null;
            for (var tier = 0; tier < 3; tier++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArtSpikeBuilder.CharacterPaths[tier]);
                var core = prefab.transform.Find("01_CoreChassis_CommonIdentity/GravityCore_Common");
                var supports = prefab.transform.Find("02_FourMechanicalSupports_Common");
                Assert.That(core, Is.Not.Null);
                Assert.That(supports.childCount, Is.EqualTo(4));
                if (position.HasValue)
                {
                    Assert.That(core.localPosition, Is.EqualTo(position.Value));
                    Assert.That(core.localScale, Is.EqualTo(scale.Value));
                }
                position = core.localPosition;
                scale = core.localScale;
                Assert.That(prefab.transform.localScale, Is.EqualTo(Vector3.one));
            }
        }

        [Test]
        public void EvolutionAddsGeometryAndChangesSilhouetteWithoutRootScaling()
        {
            var snapshots = ArtSpikeBuilder.CharacterPaths.Take(3).Select(ArtSpikeAudit.Inspect).ToArray();
            Assert.That(snapshots[1].boundsMetres.x, Is.GreaterThan(snapshots[0].boundsMetres.x * 1.10f));
            Assert.That(snapshots[2].boundsMetres.x, Is.GreaterThan(snapshots[1].boundsMetres.x * 1.10f));
            Assert.That(snapshots[1].meshInstances, Is.GreaterThan(snapshots[0].meshInstances));
            Assert.That(snapshots[2].meshInstances, Is.GreaterThan(snapshots[1].meshInstances));
        }

        [Test]
        public void ExactlyOneEnemyPrototypeHasDifferentLocomotionAndEnergyFromPlayer()
        {
            var enemies = AssetDatabase.FindAssets("t:Prefab", new[] { ArtSpikeBuilder.Root + "/Prefabs/Enemies" });
            Assert.That(enemies.Length, Is.EqualTo(1));
            var enemy = AssetDatabase.LoadAssetAtPath<GameObject>(ArtSpikeBuilder.CharacterPaths[3]);
            Assert.That(enemy.transform.Find("02_FourMechanicalSupports_Common"), Is.Null);
            Assert.That(enemy.transform.Find("02_ArticulatedCuttingArms"), Is.Not.Null);
            var snapshot = ArtSpikeAudit.Inspect(ArtSpikeBuilder.CharacterPaths[3]);
            Assert.That(snapshot.materialNames, Does.Contain("Gravivore_HostileCore"));
            Assert.That(snapshot.materialNames, Does.Not.Contain("Gravivore_PlayerCore"));
        }

        [Test]
        public void V2CharactersUseMechDonorsAndHaveNoFactoryOrGearMeshes()
        {
            foreach (var path in ArtSpikeBuilder.CharacterPaths)
            {
                var snapshot = ArtSpikeAudit.Inspect(path);
                Assert.That(snapshot.donorFiles, Has.Some.Contains("/Julius/MechSketch/"));
                Assert.That(snapshot.donorFiles.Any(p => p.Contains("/Kenney/")), Is.False, path);
                Assert.That(snapshot.childRenderers, Is.LessThan(40), path);
            }
        }

        [Test]
        public void ProductionSceneAndCatalogHaveNoArtSpikeDependenciesOrBuildEntry()
        {
            foreach (var path in new[] { "Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity",
                "Assets/_Game/Content/Definitions/S15_VisualCatalog.asset", "Assets/_Game/Content/Definitions/S07_Evolution.asset" })
                Assert.That(AssetDatabase.GetDependencies(path, true).Any(p => p.StartsWith(ArtSpikeBuilder.Root + "/")), Is.False, path);
            Assert.That(EditorBuildSettings.scenes.Any(s => s.path == ArtSpikeBuilder.ScenePath), Is.False);
        }

        [Test]
        public void ComparisonContainsFourAreasAndSettledS20CameraWithoutGameplayWiring()
        {
            var priorScene = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene = EditorSceneManager.OpenScene(ArtSpikeBuilder.ScenePath, OpenSceneMode.Single);
                var roots = scene.GetRootGameObjects();
                Assert.That(roots.Count(r => r.name.StartsWith("AREA ")), Is.EqualTo(4));
                Assert.That(roots.SelectMany(r => r.GetComponentsInChildren<Collider>(true)), Is.Empty);
                var camera = roots.Single(r => r.name == "Camera_B_S20").GetComponent<Camera>();
                var player = roots.Single(r => r.name == "B_G0_Tier1");
                var expected = ArtSpikeScene.GameplayCamera("Expected_S20", player.transform.position);
                try
                {
                    Assert.That(camera.fieldOfView, Is.EqualTo(expected.fieldOfView));
                    Assert.That(Vector3.Distance(camera.transform.position, expected.transform.position), Is.LessThan(.0001f));
                    Assert.That(Quaternion.Angle(camera.transform.rotation, expected.transform.rotation), Is.LessThan(.001f));
                    Assert.That(expected.aspect, Is.EqualTo(9f / 16f).Within(.0001f));
                    Assert.That(camera.cullingMask, Is.EqualTo(1 << 25));
                }
                finally { Object.DestroyImmediate(expected.gameObject); }
                Assert.That(roots.SelectMany(r => r.GetComponentsInChildren<Light>(true)).Count(), Is.EqualTo(1));
            }
            finally
            {
                // A fresh batch test run can start with no loaded scene to restore.
                if (priorScene.Any(s => s.isLoaded) && priorScene.Count(s => s.isActive) == 1)
                    EditorSceneManager.RestoreSceneManagerSetup(priorScene);
                else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }
    }
}
