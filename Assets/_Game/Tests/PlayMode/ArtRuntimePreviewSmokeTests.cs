using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Progression;
using Gravivore.Presentation.Combat;
using Gravivore.Presentation.Evolution;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace Gravivore.Tests.PlayMode
{
    public sealed class ArtRuntimePreviewSmokeTests
    {
        [UnityTest]
        public IEnumerator CanonicalProgressionSelectsExactlyOneWholeFormAtExistingThresholds()
        {
            var scene = new CanonicalSceneTestScope();
            yield return scene.Load();
            var player = scene.Root.PlayerObject;
            var body = player.GetComponent<CharacterController>();
            var visual = player.transform.Find("Player Visual Root");
            Assert.That(body.radius, Is.EqualTo(.42f));
            Assert.That(body.height, Is.EqualTo(1.4f));
            Assert.That(body.center, Is.EqualTo(new Vector3(0, .7f, 0)));
            Assert.That(visual.GetComponentsInChildren<Collider>(true), Is.Empty);
            Assert.That(visual.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
            Assert.IsNull(visual.Find("S15 Visual [player-techno-organism]"));
            for (var score = 0; score <= 42; score++)
            {
                if (score > 0) Grant(scene);
                Assert.That(scene.Root.Progression.State.TotalAssimilationScore, Is.EqualTo(score));
                var tier = score < 20 ? 0 : score < 42 ? 1 : 2;
                Assert.That(scene.Root.EvolutionPresenter.CurrentTier, Is.EqualTo((EvolutionTier)tier));
                for (var i = 0; i < 3; i++)
                {
                    var form = visual.Find($"G0_Tier{i}_ArtSpike");
                    Assert.IsNotNull(form);
                    Assert.That(form.gameObject.activeSelf, Is.EqualTo(i == tier));
                    Assert.That(form.localScale, Is.EqualTo(Vector3.one));
                }
            }
            Assert.That(player.GetComponent<PlayerEvolutionView>().GetActiveTierModuleCount(), Is.EqualTo(1));
            Assert.That(body.radius, Is.EqualTo(.42f));
            Assert.IsNotNull(scene.Root.DevelopmentOverlay);
            yield return scene.Cleanup();
        }

        [UnityTest]
        public IEnumerator ActualCutterPopulationPreservesRootPhysicsPbrAndDeathRecycle()
        {
            var scene = new CanonicalSceneTestScope();
            yield return scene.Load();
            var cutters = Cutters(scene);
            Assert.That(cutters.Length, Is.EqualTo(4));
            foreach (var enemy in cutters)
            {
                var form = CutterForm(enemy);
                Assert.That(enemy.GetComponent<CharacterController>().radius, Is.EqualTo(.4f));
                Assert.That(enemy.MaximumHitPoints, Is.EqualTo(42));
                Assert.That(form.GetComponentsInChildren<Collider>(true), Is.Empty);
                Assert.That(form.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
                Assert.That(form.GetComponentsInChildren<Renderer>().Length, Is.EqualTo(13));
                Assert.IsTrue(form.GetComponentsInChildren<Renderer>().Any(r => r.sharedMaterial.name == "Gravivore_ProxyArmor_PBR"));
            }
            var killed = cutters[0];
            var result = killed.ApplyDamage(new DamageRequest(10000, DamageType.Gravity));
            Assert.IsTrue(result.WasLethal);
            Assert.IsFalse(killed.IsAlive);
            Assert.IsFalse(CutterForm(killed).gameObject.activeInHierarchy);
            yield return scene.Cleanup();
        }

        [UnityTest]
        public IEnumerator Tier2AndFourActualCuttersProduceRuntimeStructuralAuditAndAlignedVfx()
        {
            var scene = new CanonicalSceneTestScope();
            yield return scene.Load();
            while (scene.Root.Progression.State.TotalAssimilationScore < 42) Grant(scene);
            var player = scene.Root.PlayerObject;
            player.transform.position = new Vector3(0, 0, 35);
            var camera = Camera.main;
            Assert.IsNotNull(camera);
            camera.transform.position = player.transform.position + new Vector3(0, 14.8f, -11.2f);
            camera.transform.LookAt(player.transform.position + Vector3.up * .9f);
            camera.aspect = 9f / 16f;
            Assert.That(camera.fieldOfView, Is.EqualTo(46));
            var cutters = Cutters(scene);
            Assert.That(cutters.Length, Is.EqualTo(4));
            var forms = new[] { player.transform.Find("Player Visual Root/G0_Tier2_ArtSpike") }
                .Concat(cutters.Select(CutterForm)).ToArray();
            var planes = GeometryUtility.CalculateFrustumPlanes(camera);
            foreach (var form in forms)
                Assert.IsTrue(form.GetComponentsInChildren<Renderer>().Any(r => GeometryUtility.TestPlanesAABB(planes, r.bounds)),
                    form.name + " must be visible in the settled portrait camera audit.");
            var vfx = scene.Root.GetComponentInChildren<GravityLashVfxPool>();
            var socket = player.transform.Find("Player Visual Root/Gravity Lash Presentation Origin");
            Assert.That(socket.position.y - player.transform.position.y, Is.InRange(1f, 1.2f));
            vfx.Play(player.transform.position, cutters[0].TargetPoint.position);
            var beam = vfx.LastPlayedObject.GetComponentInChildren<LineRenderer>(true);
            Assert.That(beam.GetPosition(0), Is.EqualTo(socket.position));
            Assert.That(beam.GetPosition(1), Is.EqualTo(cutters[0].TargetPoint.position));
            Assert.That(player.transform.position, Is.EqualTo(new Vector3(0, 0, 35)));
            WriteAudit(scene, forms, cutters.Length);
            yield return scene.Cleanup();
        }

        private static void Grant(CanonicalSceneTestScope scene) =>
            Assert.IsTrue(scene.Root.Progression.TryGrant(new EnemyDeathEvent(
                new EnemyLifeId(Guid.NewGuid()), "scout-drone", Vector3.zero)));
        private static Transform CutterForm(OrdinaryEnemyController enemy) =>
            enemy.transform.Find("Enemy Art Root/S15 Visual [cutter-unit]/Cutter_ArtSpike");
        private static OrdinaryEnemyController[] Cutters(CanonicalSceneTestScope scene) =>
            scene.Root.GetComponentsInChildren<OrdinaryEnemyController>()
                .Where(e => e.IsAlive && CutterForm(e) != null && CutterForm(e).gameObject.activeInHierarchy).ToArray();

        [Serializable]
        private sealed class Snapshot
        {
            public string scene = "Chapter01_ScrapExclusion";
            public string scope = "Visible active Tier2 + four real population Cutters; excludes environment, HUD, VFX and other enemies.";
            public int cutterInstances, renderers, materialSlots, uniqueMaterials, uniqueTextures, realtimeLights, shadowLights, shadowCastingRenderers;
            public long triangles, androidAstc6x6TextureBytesIncludingMips;
            public string fps = "Not measured on device";
        }

        private static void WriteAudit(CanonicalSceneTestScope scene, Transform[] forms, int cutterCount)
        {
            var snapshot = new Snapshot { cutterInstances = cutterCount };
            var materials = new HashSet<Material>();
            var textures = new HashSet<Texture>();
            foreach (var form in forms)
            {
                foreach (var renderer in form.GetComponentsInChildren<Renderer>())
                {
                    snapshot.renderers++;
                    snapshot.materialSlots += renderer.sharedMaterials.Length;
                    if (renderer.shadowCastingMode != ShadowCastingMode.Off) snapshot.shadowCastingRenderers++;
                    foreach (var material in renderer.sharedMaterials)
                    {
                        materials.Add(material);
                        foreach (var property in material.GetTexturePropertyNames())
                        {
                            var texture = material.GetTexture(property);
                            if (texture != null) textures.Add(texture);
                        }
                    }
                }
                foreach (var filter in form.GetComponentsInChildren<MeshFilter>())
                    for (var sub = 0; sub < filter.sharedMesh.subMeshCount; sub++)
                        snapshot.triangles += filter.sharedMesh.GetIndexCount(sub) / 3;
            }
            snapshot.uniqueMaterials = materials.Count;
            snapshot.uniqueTextures = textures.Count;
            foreach (var texture in textures)
            {
                var width = texture.width; var height = texture.height;
                do
                {
                    snapshot.androidAstc6x6TextureBytesIncludingMips += ((width + 5) / 6) * ((height + 5) / 6) * 16L;
                    if (width == 1 && height == 1) break;
                    width = Math.Max(1, width / 2); height = Math.Max(1, height / 2);
                } while (true);
            }
            foreach (var light in scene.Root.GetComponentsInChildren<Light>())
            {
                if (light.lightmapBakeType == LightmapBakeType.Baked) continue;
                snapshot.realtimeLights++;
                if (light.shadows != LightShadows.None) snapshot.shadowLights++;
            }
            // Whole-form art may change within the established mobile budgets.
            Assert.That(snapshot.renderers, Is.LessThanOrEqualTo(92)); // <=40 player + four 13-renderer Cutters.
            Assert.That(snapshot.materialSlots, Is.LessThanOrEqualTo(180));
            Assert.That(snapshot.triangles, Is.InRange(1L, 50000L));
            Directory.CreateDirectory("docs/art-spike");
            File.WriteAllText("docs/art-spike/RUNTIME_PERFORMANCE.json", JsonUtility.ToJson(snapshot, true) + "\n");
        }
    }
}
