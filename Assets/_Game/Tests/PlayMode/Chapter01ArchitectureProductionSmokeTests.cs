using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.Camera;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Camera = UnityEngine.Camera;
using Object = UnityEngine.Object;

namespace Gravivore.Tests.PlayMode
{
    public sealed class Chapter01ArchitectureProductionSmokeTests
    {
        private const string Output = "docs/history/implementation-passes/chapter01-architecture-production-v1";
        private static readonly Vector3 Center = new Vector3(11, .08f, 50.65f);
        private CanonicalSceneTestScope _scene;
        private GameObject _installation;
        private CharacterController _walker;
        [UnityTearDown] public IEnumerator Cleanup() { if (_scene != null) yield return _scene.Cleanup(); }
        private IEnumerator Load()
        {
            _scene = new CanonicalSceneTestScope(); yield return _scene.Load();
            var root = _scene.Root;
            root.PlayerHealth.SetDevelopmentGodMode(true);
            root.PlayerObject.GetComponent<PlayerLocomotion>().enabled = false;
            root.PlayerObject.GetComponent<GravityAttackController>().enabled = false;
            root.EnemyPopulation.enabled = false; root.MagnetarGuard.enabled = false; root.CustodianBoss.enabled = false;
            foreach (var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true)) enemy.enabled = false;
            _installation = SceneManager.GetActiveScene().GetRootGameObjects().Single(g => g.name == "Energy Chamber / production");
            _walker = root.PlayerObject.GetComponent<CharacterController>();
        }
        [UnityTest] public IEnumerator ProductionLandmarkIsWiredWithoutNewGameplayOwnership()
        {
            yield return Load();
            Assert.That(Vector3.Distance(_installation.transform.position, Center), Is.LessThan(.001f));
            Assert.That(_installation.transform.localScale, Is.EqualTo(Vector3.one));
            Assert.That(_installation.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
            Assert.That(_installation.GetComponentsInChildren<Camera>(true), Is.Empty);
            Assert.That(_installation.GetComponentsInChildren<Light>(true), Is.Empty);
            var collisionRoot = _scene.Root.WorldPresenter.GameplayRoot.Find("Architectural collision");
            Assert.That(collisionRoot, Is.Not.Null);
            var colliders = collisionRoot.GetComponentsInChildren<BoxCollider>(true);
            Assert.That(colliders.Length, Is.EqualTo(25));
            Assert.That(colliders.All(c => c.gameObject.layer == LayerMask.NameToLayer("HardBlocker")), Is.True);
            var topology = _scene.Root.MapIntegration.MapPresenter.Topology;
            Assert.That(Enumerable.Range(0, topology.BlockerCount).Select(i => topology.GetBlocker(i).Authority).Intersect(colliders).Count(), Is.EqualTo(25));
            Assert.That(_installation.GetComponent<LODGroup>().GetLODs()[0].renderers.Length, Is.EqualTo(45));
            Assert.That(_walker.radius, Is.EqualTo(.42f).Within(.001f));
            Assert.That(_walker.height, Is.EqualTo(1.4f).Within(.001f));
        }
        [UnityTest] public IEnumerator ActualG0CannotCrossSolidMachineryAndCanTraverseAroundIt()
        {
            yield return Load();
            var records = new List<TraversalRecord>();
            for (var i = 0; i < 8; i++)
            {
                if (i == 4) continue; // South edge is the existing closed platform rim, not a traversable approach.
                var direction = Quaternion.Euler(0, i * 45, 0) * Vector3.forward;
                Teleport(Center + direction * 6.8f);
                for (var step = 0; step < 70; step++) _walker.Move(-direction * .1f + Vector3.down * .05f);
                var radius = FlatDistance(_walker.transform.position, Center);
                Assert.That(radius, Is.GreaterThan(2.55f), "G-0 crossed solid footprint on approach " + i);
                Assert.That(radius, Is.LessThan(6.6f), "Unexpected external obstruction on approach " + i);
                records.Add(new TraversalRecord { name = "blocked approach " + i, position = _walker.transform.position, clearance = radius });
            }
            // Two opposing diagonal service pockets do not contain the stairs.
            foreach (var direction in new[] {new Vector3(1, 0, -1).normalized, new Vector3(-1, 0, 1).normalized})
            {
                Teleport(Center + direction * 6.8f);
                for (var step = 0; step < 17; step++) _walker.Move(-direction * .1f + Vector3.down * .05f);
                Assert.That(FlatDistance(_walker.transform.position, Center + direction * 5.1f), Is.LessThan(.15f));
                records.Add(new TraversalRecord { name = "open service pocket", position = _walker.transform.position, clearance = FlatDistance(_walker.transform.position, Center) });
            }
            const float ringRadius = 6.5f;
            // The south rim faces the existing platform boundary, rather than a
            // service lane. Validate the available west/north/east perimeter.
            Teleport(Center + Vector3.left * ringRadius);
            for (var i = 1; i <= 48; i++)
            {
                var point = Center + Quaternion.Euler(0, -90 + i * 180f / 48, 0) * Vector3.forward * ringRadius;
                var delta = point - _walker.transform.position; delta.y = -.05f;
                _walker.Move(delta);
                Assert.That(FlatDistance(_walker.transform.position, point), Is.LessThan(.20f), "Route blocked at segment " + i);
            }
            records.Add(new TraversalRecord { name = "48-segment accessible west/north/east perimeter", position = _walker.transform.position, clearance = ringRadius });
            Directory.CreateDirectory(Output + "/verification");
            File.WriteAllText(Output + "/verification/g0-traversal.json", JsonUtility.ToJson(new TraversalRecords { items = records.ToArray() }, true));
            yield return null;
        }
        [UnityTest] public IEnumerator CaptureActualChapterWithLiveG0AndUnchangedGameplayLens()
        {
            yield return Load();
            var camera = _scene.Root.GetComponentsInChildren<Camera>(true).First(c => c.CompareTag("MainCamera"));
            var follow = camera.GetComponent<PortraitFollowCamera>();
            if (follow != null) follow.enabled = false;
            var offsets = new[] { new Vector3(3, 0, -10), new Vector3(3, 0, -5.3f), new Vector3(-3.8f, 0, -5.3f), new Vector3(3, 0, -14), new Vector3(-6.4f, 0, 2), new Vector3(3, 0, -5.3f) };
            var names = new[] {"01_gameplay_approach", "02_G0_beside_installation", "03_close_gameplay", "04_wider_chapter_context", "05_collision_navigation", "06_normal_gameplay_emission"};
            var records = new List<CameraRecord>();
            for (var i = 0; i < offsets.Length; i++)
            {
                var anchor = Center + offsets[i]; Teleport(anchor);
                camera.transform.position = anchor + new Vector3(3.4f, 19.8f, -13.6f);
                camera.transform.rotation = Quaternion.LookRotation(anchor + Vector3.up * 3.2f - camera.transform.position, Vector3.up);
                camera.fieldOfView = 48; camera.aspect = 9f / 16;
                yield return null; yield return null;
                var frustum = GeometryUtility.CalculateFrustumPlanes(camera);
                var visible = _installation.GetComponentsInChildren<Renderer>().Where(r => GeometryUtility.TestPlanesAABB(frustum, r.bounds)).ToArray();
                Save(camera, names[i]);
                records.Add(new CameraRecord { name = names[i], player = _walker.transform.position, camera = camera.transform.position,
                    visibleRenderersUpperBound = visible.Length, visibleTrianglesUpperBound = visible.Sum(r => TriangleCount(r.GetComponent<MeshFilter>().sharedMesh)),
                    visibleMaterialSlotsUpperBound = visible.Sum(r => r.sharedMaterials.Length) });
            }
            File.WriteAllText(Output + "/verification/gameplay-camera.json", JsonUtility.ToJson(new CameraRecords { items = records.ToArray() }, true));
        }
        [UnityTest] public IEnumerator ActualLocomotionEnvelopeTraversesTheNorthernServicePassage()
        {
            yield return Load();
            foreach (var body in _scene.Root.GetComponentsInChildren<CharacterController>(true)) if (body != _walker) body.enabled = false;
            var locomotion = _walker.GetComponent<PlayerLocomotion>();
            Assert.That(locomotion.WallEnvelopeRadius, Is.EqualTo(1.2f).Within(.001f));
            var camera = _scene.Root.GetComponentsInChildren<Camera>().First(c => c.CompareTag("MainCamera"));
            var input = new ControlledMovementInput();
            locomotion.Initialize(input, camera.transform, new PlayerMovementParameters(4.5f, 720));
            Teleport(new Vector3(0, .08f, 54));
            var route = new[] {new Vector3(4,.08f,58),new Vector3(18,.08f,58),new Vector3(17,.08f,54),new Vector3(18,.08f,58),new Vector3(4,.08f,58),new Vector3(0,.08f,54)};
            var records = new List<TraversalRecord>();
            foreach (var target in route)
            {
                for (var step = 0; step < 500 && FlatDistance(_walker.transform.position, target) > .10f; step++)
                {
                    var delta = target - _walker.transform.position; delta.y = 0;
                    var direction = delta.normalized;
                    input.Movement = new Vector2(Vector3.Dot(direction, Vector3.ProjectOnPlane(camera.transform.right,Vector3.up).normalized), Vector3.Dot(direction,Vector3.ProjectOnPlane(camera.transform.forward,Vector3.up).normalized));
                    locomotion.Step(Mathf.Min(.05f, delta.magnitude / 4.5f));
                }
                Assert.That(FlatDistance(_walker.transform.position, target), Is.LessThan(.15f), "Actual locomotion stuck towards " + target + " at " + _walker.transform.position);
                records.Add(new TraversalRecord { name="actual locomotion service route", position=_walker.transform.position, clearance=locomotion.WallEnvelopeRadius });
            }
            input.Movement = Vector2.zero;
            File.WriteAllText(Output + "/verification/g0-locomotion-envelope.json", JsonUtility.ToJson(new TraversalRecords {items=records.ToArray()},true));
            yield return null;
        }
        private void Teleport(Vector3 p) { _walker.enabled = false; _walker.transform.position = p; _walker.enabled = true; Physics.SyncTransforms(); }
        private static float FlatDistance(Vector3 a, Vector3 b) { a.y = b.y = 0; return Vector3.Distance(a, b); }
        private static int TriangleCount(Mesh mesh) => Enumerable.Range(0, mesh.subMeshCount).Sum(s => (int)mesh.GetIndexCount(s) / 3);
        private static void Save(Camera camera, string name)
        {
            Directory.CreateDirectory(Output + "/screenshots");
            var old = camera.targetTexture; var active = RenderTexture.active;
            var target = new RenderTexture(1080, 1920, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var pixels = new Texture2D(1080, 1920, TextureFormat.RGB24, false);
            try { camera.targetTexture = target; for (var i = 0; i < 3; i++) camera.Render(); RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, 1080, 1920), 0, 0); pixels.Apply(); File.WriteAllBytes(Output + "/screenshots/" + name + ".png", pixels.EncodeToPNG()); }
            finally { camera.targetTexture = old; RenderTexture.active = active; Object.DestroyImmediate(target); Object.DestroyImmediate(pixels); }
        }
        [Serializable] private sealed class TraversalRecord { public string name; public Vector3 position; public float clearance; }
        [Serializable] private sealed class TraversalRecords { public TraversalRecord[] items; }
        [Serializable] private sealed class CameraRecord { public string name; public Vector3 player, camera; public int visibleRenderersUpperBound, visibleTrianglesUpperBound, visibleMaterialSlotsUpperBound; }
        [Serializable] private sealed class CameraRecords { public CameraRecord[] items; }
        private sealed class ControlledMovementInput : IMovementInput { public Vector2 Movement {get;set;} }
    }
}
