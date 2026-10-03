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
        public void V3CharactersRespectBudgetsAndHaveNoShareAlikeOrSceneryMeshDependency()
        {
            var caps = new[] { 30, 35, 40, 25 };
            for (var index = 0; index < ArtSpikeBuilder.CharacterPaths.Length; index++)
            {
                var path = ArtSpikeBuilder.CharacterPaths[index];
                var snapshot = ArtSpikeAudit.Inspect(path);
                Assert.That(snapshot.donorFiles, Is.Empty, path);
                Assert.That(snapshot.ownedMeshFiles, Is.Not.Empty);
                Assert.That(AssetDatabase.GetDependencies(path, true).Any(p => p.Contains("/Julius/") || p.Contains("/Kenney/")), Is.False, path);
                Assert.That(snapshot.childRenderers, Is.LessThanOrEqualTo(caps[index]), path);
                Assert.That(snapshot.triangles, Is.LessThanOrEqualTo(50000), path);
                Assert.That(snapshot.uniqueTextures, Is.EqualTo(4), path);
            }
        }

        [Test]
        public void MechanicalArmorUsesPbrMapsWithCorrectLinearAndNormalImport()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(ArtSpikeBuilder.Root + "/Materials/Gravivore_ProxyArmor_PBR.mat");
            foreach (var property in new[] { "_BaseMap", "_BumpMap", "_MetallicGlossMap", "_OcclusionMap" })
            {
                var texture = material.GetTexture(property);
                Assert.That(texture, Is.Not.Null, property);
                var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));
                Assert.That(importer.maxTextureSize, Is.LessThanOrEqualTo(2048));
                Assert.That(importer.sRGBTexture, Is.EqualTo(property == "_BaseMap"));
                Assert.That(importer.GetPlatformTextureSettings("Android").format, Is.EqualTo(TextureImporterFormat.ASTC_6x6));
                if (property == "_BumpMap") Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.NormalMap));
            }
            Assert.That(material.IsKeywordEnabled("_NORMALMAP"), Is.True);
            Assert.That(material.IsKeywordEnabled("_METALLICSPECGLOSSMAP"), Is.True);
        }

        [Test]
        public void ArmConversionPreservesMetallicAndInvertsRoughnessIntoAlpha()
        {
            var packed = ArtSpikePbr.PackMetalSmoothness(new Color32(200, 50, 120, 255));
            Assert.That(packed.r, Is.EqualTo(120));
            Assert.That(packed.a, Is.EqualTo(205));
            Assert.That(ArtSpikePbr.PackMetalSmoothness(new Color32(0, 255, 255, 255)).a, Is.Zero);
            Assert.That(ArtSpikePbr.PackMetalSmoothness(new Color32(255, 0, 0, 255)).a, Is.EqualTo(255));
        }

        [Test]
        public void IdleProofMovesIndependentPivotsWithoutAddingAnimationToCharacterPrefabs()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArtSpikeBuilder.CharacterPaths[2]);
            Assert.That(prefab.GetComponentsInChildren<Animation>(true), Is.Empty);
            Assert.That(prefab.GetComponentsInChildren<Animator>(true), Is.Empty);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ArtSpikeArticulation.ClipPath);
            Assert.That(clip.length, Is.EqualTo(3));
            Assert.That(AnimationUtility.GetCurveBindings(clip).All(b => b.type == typeof(Transform)), Is.True);
            var instance = Object.Instantiate(prefab);
            try
            {
                var hips = instance.GetComponentsInChildren<Transform>(true).Where(t => t.name == "HipPivot").ToArray();
                var initial = hips.Select(t => t.localRotation).ToArray();
                clip.SampleAnimation(instance, .75f);
                Assert.That(hips.Length, Is.EqualTo(4));
                for (var i = 0; i < hips.Length; i++) Assert.That(Quaternion.Angle(initial[i], hips[i].localRotation), Is.GreaterThan(2));
            }
            finally { Object.DestroyImmediate(instance); }
        }

        [Test]
        public void RuntimePreviewDependsOnlyOnCharacterArtAndNeverBuildsTheComparisonBay()
        {
            foreach (var path in new[] { "Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity",
                "Assets/_Game/Content/Definitions/S15_VisualCatalog.asset", "Assets/_Game/Content/Definitions/S07_Evolution.asset" })
            {
                var dependencies = AssetDatabase.GetDependencies(path, true);
                Assert.That(dependencies.Any(p => p.StartsWith(ArtSpikeBuilder.Root + "/")), Is.True, path);
                Assert.That(dependencies.Any(p => p.Contains("/Prefabs/Environment/") || p == ArtSpikeBuilder.ScenePath ||
                    p.EndsWith("ArtSpike_StudioReflection.cubemap")), Is.False, path);
            }
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
