using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.Camera;
using Gravivore.Presentation.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Camera = UnityEngine.Camera;
using Object = UnityEngine.Object;

namespace Gravivore.Tests.PlayMode
{
    public sealed class Phase3CIntegrationSmokeTests
    {
        private CanonicalSceneTestScope _scene;
        private static readonly string[] Names = { "Scout_V1", "Cutter_V1", "Warden_V1", "ArcDrone_V1", "Carrier_V1" };
        private static readonly PresentationSocket[] Roles = { PresentationSocket.Core, PresentationSocket.Sensor,
            PresentationSocket.AttackOrigin,PresentationSocket.HitCenter,PresentationSocket.GroundContact,
            PresentationSocket.VfxTop,PresentationSocket.VfxRear,PresentationSocket.TelegraphOrigin };

        [UnityTearDown] public IEnumerator Cleanup() { if (_scene != null) yield return _scene.Cleanup(); _scene = null; }
        private IEnumerator LoadControlled()
        {
            _scene = new CanonicalSceneTestScope(); yield return _scene.Load();
            var root = _scene.Root;
            root.PlayerObject.GetComponent<GravityAttackController>().enabled = false;
            root.PlayerObject.GetComponent<PlayerLocomotion>().enabled = false;
            root.PlayerObject.GetComponent<MechMotionPresenter>().enabled = false;
            root.EnemyPopulation.enabled = false;
            foreach (var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true)) enemy.enabled = false;
            root.MagnetarGuard.enabled = false; root.CustodianBoss.enabled = false;
        }

        [UnityTest] public IEnumerator ActualBindingsSocketsAndCoveredEnvironmentPreserveAuthority()
        {
            yield return LoadControlled(); var root = _scene.Root; var environment = root.VisualEnvironment;
            Assert.IsFalse(environment.FallbackRoot.gameObject.activeSelf);
            Assert.That(environment.Floor.GetComponentsInChildren<Renderer>().Length, Is.GreaterThan(1));
            Assert.That(environment.GetComponentsInChildren<Collider>(true), Is.Empty);
            Assert.That(environment.GetComponentsInChildren<MonoBehaviour>(true), Has.Length.EqualTo(1));
            for (var i = 0; i < 5; i++)
            {
                var enemy = root.EnemyPopulation.GetSpot(i).GetLiveEnemy(0);
                var binding = enemy.GetComponent<CharacterVisualBinding>();
                Assert.That(binding.ActiveModel.name, Is.EqualTo(Names[i]));
                CheckSockets(binding, enemy.TargetPoint);
                Assert.That(binding.VisualRoot.GetComponentsInChildren<Collider>(true), Is.Empty);
                Assert.That(enemy.GetComponent<CharacterController>().radius, Is.EqualTo(enemy.CollisionRadius));
            }
            foreach (var actor in new[] { root.MagnetarGuard.transform, root.CustodianBoss.transform })
            {
                var binding = actor.GetComponent<CharacterVisualBinding>(); CheckSockets(binding,actor);
                Assert.That(actor.localScale, Is.EqualTo(Vector3.one));
                Assert.That(binding.VisualRoot.GetComponentsInChildren<Collider>(true), Is.Empty);
            }
            var hub = environment.GetRegion("repair-hub").Root;
            var model = hub.Find("MainPlatform/Repair_Platform_V2"); Assert.IsNotNull(model);
            Assert.That(Vector3.Distance(hub.Find("PlayerDockPoint").position, model.Find("ServicePoint").position), Is.LessThan(.001f));
            Assert.That(root.RepairHub.Manipulators.ArmCount,Is.EqualTo(2));
            Assert.That(root.RepairHub.Manipulators.transform.position,Is.EqualTo(root.RepairHub.RepairPosition));
            Assert.NotNull(root.RepairHub.Manipulators.transform.Find("Authored repair pedestal 0"));
            Assert.NotNull(root.RepairHub.Manipulators.transform.Find("Authored repair pedestal 1"));
            Assert.That(root.CustodianBoss.GetComponent<CharacterVisualBinding>().ActiveModel.Find("ConeAttackOrigin"),Is.Not.Null);
            Assert.That(root.CustodianBoss.GetComponent<CharacterVisualBinding>().ActiveModel.Find("LineAttackOrigin"),Is.Not.Null);
            Assert.That(root.CustodianBoss.GetComponent<CharacterVisualBinding>().ActiveModel.Find("CircleAttackOrigin"),Is.Not.Null);
            Assert.IsTrue(root.WorldPresenter.EliteGate.BlockingCollider.enabled);
            var barrier = environment.Structures.Find("elite-gate Visuals/Physical Gate Barrier Visual");
            Assert.IsNotNull(barrier); Assert.IsTrue(barrier.gameObject.activeInHierarchy);
            var hp = root.PlayerHealth.CurrentHitPoints;
            environment.FallbackRoot.gameObject.SetActive(true); yield return null;
            environment.FallbackRoot.gameObject.SetActive(false);
            Assert.That(root.PlayerHealth.CurrentHitPoints,Is.EqualTo(hp));
            Assert.IsTrue(root.WorldPresenter.EliteGate.BlockingCollider.enabled);
            Assert.That(root.PlayerObject.GetComponent<CharacterController>().radius,Is.EqualTo(.42f));
        }

        private static void CheckSockets(CharacterVisualBinding binding, Transform fallback)
        {
            Assert.IsNotNull(binding.ActiveModel);
            foreach (var role in Roles)
            {
                var resolved = binding.GetSocketOr(role,fallback); Assert.IsNotNull(resolved);
                if (binding.Sockets.TryGet(role,out var socket)) Assert.IsTrue(socket.IsChildOf(binding.ActiveModel));
                else Assert.AreSame(fallback,resolved);
            }
        }

        [UnityTest] public IEnumerator IntegratedPoolReuseClearsOldSocketsAndUsesNewFamilyOffsets()
        {
            yield return LoadControlled();
            var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<S15VisualCatalog>("Assets/_Game/Content/Definitions/S15_VisualCatalog.asset");
            var poolRoot = new GameObject("Phase3C pooled binding regression"); poolRoot.transform.SetParent(_scene.Root.transform,false);
            var pool = new OrdinaryEnemyPool(poolRoot.transform,1,9,TestMaterialFactory.Lit,new S15EnemyVisualFactory(catalog));
            EnemyRuntimeConfiguration Config(string id) => new EnemyRuntimeConfiguration(id,100,1,0,.4f,.9f,new EnemyBehaviorParameters(5,7,.1f,10));
            var enemy = pool.Acquire(Config("scout-drone"),_scene.Root.PlayerObject.transform,_scene.Root.PlayerHealth,Vector3.zero,pool.Return);
            var binding = enemy.GetComponent<CharacterVisualBinding>(); var life = enemy.LifeId;
            var oldSensor = binding.GetSocketOr(PresentationSocket.Sensor,enemy.TargetPoint);
            Assert.AreNotSame(enemy.TargetPoint,oldSensor);
            pool.Return(enemy); Assert.IsNull(binding.ActiveModel); Assert.IsNull(binding.Sockets);
            var reused = pool.Acquire(Config("carrier"),_scene.Root.PlayerObject.transform,_scene.Root.PlayerHealth,Vector3.one,pool.Return);
            Assert.AreSame(enemy,reused); Assert.That(reused.LifeId,Is.Not.EqualTo(life));
            Assert.That(binding.ActiveModel.name,Is.EqualTo("Carrier_V1"));
            Assert.AreSame(reused.TargetPoint,binding.GetSocketOr(PresentationSocket.Sensor,reused.TargetPoint));
            Assert.IsFalse(oldSensor.gameObject.activeInHierarchy);
            Assert.That(binding.VisualRoot.localPosition,Is.EqualTo(Vector3.zero));
            Assert.That(reused.transform.localScale,Is.EqualTo(Vector3.one));
            pool.Return(reused);
        }

        [UnityTest] public IEnumerator RuntimeCapturesAndFrustumCostSnapshot()
        {
            yield return LoadControlled(); var root = _scene.Root;
            var output = Environment.GetEnvironmentVariable("GRAVIVORE_PHASE3C_QA");
            // Binding tests always run; expensive render export is opt-in, with no ignored test.
            if (string.IsNullOrEmpty(output)) yield break;
            Directory.CreateDirectory(output);
            var camera = Camera.main; camera.GetComponent<PortraitFollowCamera>().enabled = false;
            var settings = UnityEditor.AssetDatabase.LoadAssetAtPath<CameraFollowSettings>("Assets/_Game/Content/Definitions/S01_CameraFollowSettings.asset");
            var player = root.PlayerObject.transform;
            var originalPlayer = player.position;
            var labels = new[] { "01_RepairHub", "02_CapacitorField", "03_HaulerGraveyard", "04_RelayYard", "05_ShieldDump", "06_CuttingFloor", "07_EliteApproach", "08_MagnetarGuard", "09_BossApproach", "10_CustodianM0_Arena" };
            var regions = new[] { "repair-hub", "capacitor-field", "hauler-graveyard", "relay-yard", "shield-dump", "cutting-floor", "elite-approach", "elite-arena", "boss-approach", "boss-arena" };
            var metrics = new List<Cost>();
            for (var i = 0; i < labels.Length; i++)
            {
                player.position = root.VisualEnvironment.GetRegion(regions[i]).Root.position + (i >= 7 ? Vector3.back * 3 : Vector3.zero);
                SetGameplayCamera(camera,player.position,settings); Physics.SyncTransforms();
                Capture(camera,output,labels[i]);
                if (i == 3) metrics.Add(Snapshot(root.gameObject,camera,"ordinary-combat (relay-yard; all spot populations live)"));
                if (i == 9) metrics.Add(Snapshot(root.gameObject,camera,"boss-encounter (real authored position)"));
            }
            // Supplementary family comparison uses existing pooled actors in a temporary runtime staging.
            // Scene/config spawn positions are never changed. Tests clean up the isolated temporary profile.
            var actors = Enumerable.Range(0,5).Select(i => root.EnemyPopulation.GetSpot(i).GetLiveEnemy(0)).ToArray();
            var positions = actors.Select(e => e.transform.position).ToArray();
            player.position = new Vector3(-20,0,-15);
            for (var i = 0; i < actors.Length; i++) actors[i].transform.position = player.position + new Vector3((i % 3 - 1) * 2.3f,0,2.5f + i / 3 * 2.5f);
            SetGameplayCamera(camera,player.position,settings); Physics.SyncTransforms();
            actors[0].enabled = true;
            root.PlayerObject.GetComponent<GravityAttackController>().ResetTransientState();
            root.PlayerObject.GetComponent<GravityAttackController>().Tick(0);
            actors[0].enabled = false;
            Capture(camera,output,"11_OrdinaryCombat_MultipleFamilies");
            metrics.Add(Snapshot(root.gameObject,camera,"ordinary-mixed-family comparison (five temporarily staged live actors)"));
            for (var i = 0; i < actors.Length; i++) actors[i].transform.position = positions[i];
            var elitePosition = root.MagnetarGuard.transform.position; var bossPosition = root.CustodianBoss.transform.position;
            player.position = new Vector3(0,0,24);
            for (var i = 0; i < actors.Length; i++) actors[i].transform.position = new Vector3((i - 2) * 2.5f,0,26);
            root.MagnetarGuard.transform.position = new Vector3(-4,0,31);
            root.CustodianBoss.transform.position = new Vector3(3.5f,0,32);
            camera.transform.position = new Vector3(0,22,8); camera.transform.LookAt(new Vector3(0,1,29)); camera.fieldOfView = 50;
            Capture(camera,output,"13_AllEnemyFamilies_Review",900,1200);
            for (var i = 0; i < actors.Length; i++) actors[i].transform.position = positions[i];
            root.MagnetarGuard.transform.position = elitePosition; root.CustodianBoss.transform.position = bossPosition;
            player.position = originalPlayer;
            camera.transform.position = new Vector3(0,110,-45); camera.transform.LookAt(new Vector3(0,0,30));
            camera.fieldOfView = 65; camera.farClipPlane = 300;
            Capture(camera,output,"12_WideChapterTraversal",1000,1400);
            camera.transform.position = root.CustodianBoss.transform.position + new Vector3(6,6,-8);
            camera.transform.LookAt(root.CustodianBoss.transform.position + Vector3.up * 1.4f); camera.fieldOfView = 42;
            Capture(camera,output,"14_Boss_CloseReview",900,1000);
            player.position = root.VisualEnvironment.GetRegion("repair-hub").Root.position;
            SetGameplayCamera(camera,player.position,settings); Capture(camera,output,"15_Player_In_NewWorld");
            var socketReports = new List<SocketReport>();
            foreach (var actor in actors.Select(e => e.transform).Concat(new[] { root.MagnetarGuard.transform,root.CustodianBoss.transform }))
            {
                var binding = actor.GetComponent<CharacterVisualBinding>();
                socketReports.Add(new SocketReport { actor = binding.ActiveModel.name,
                    authored = Roles.Where(role => binding.Sockets.TryGet(role,out _)).Select(role => role.ToString()).ToArray(),
                    fallback = Roles.Where(role => !binding.Sockets.TryGet(role,out _)).Select(role => role.ToString()).ToArray() });
            }
            File.WriteAllText(Path.Combine(output,"RUNTIME_REVIEW.json"),JsonUtility.ToJson(new Review {
                snapshots = metrics.ToArray(), sockets = socketReports.ToArray(),
                environmentRenderers = root.VisualEnvironment.GetComponentsInChildren<Renderer>().Count(r => r.enabled && r.gameObject.activeInHierarchy),
                ordinaryPopulation = Enumerable.Range(0,5).Sum(i => root.EnemyPopulation.GetSpot(i).LiveCount),
                cameraOffset = settings.Offset, cameraFov = settings.FieldOfView },true) + "\n");
            Assert.That(Directory.GetFiles(output,"*.ppm"),Has.Length.EqualTo(15));
        }
        private static void SetGameplayCamera(Camera camera,Vector3 player,CameraFollowSettings settings)
        { camera.transform.position = player + settings.Offset; camera.transform.LookAt(player + Vector3.up * settings.LookAtHeight); camera.fieldOfView = settings.FieldOfView; }

        [Serializable] private sealed class SocketReport { public string actor; public string[] authored,fallback; }
        [Serializable] private sealed class Review { public Cost[] snapshots; public SocketReport[] sockets; public int environmentRenderers,ordinaryPopulation; public Vector3 cameraOffset; public float cameraFov; }
        [Serializable] private sealed class Cost { public string view; public int renderers,materialSlots,uniqueMaterials,activeAudioSources,playingAudioSources,activeParticleSystems; public long triangles; }
        private static Cost Snapshot(GameObject root,Camera camera,string label)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(camera);
            var renderers = root.GetComponentsInChildren<Renderer>().Where(r => r.enabled && r.gameObject.activeInHierarchy && GeometryUtility.TestPlanesAABB(planes,r.bounds)).ToArray();
            var result = new Cost { view = label,renderers = renderers.Length,materialSlots = renderers.Sum(r => r.sharedMaterials.Length),
                uniqueMaterials = renderers.SelectMany(r => r.sharedMaterials).Where(m => m != null).Distinct().Count(),
                activeAudioSources = root.GetComponentsInChildren<AudioSource>().Count(s => s.isActiveAndEnabled),
                playingAudioSources = root.GetComponentsInChildren<AudioSource>().Count(s => s.isActiveAndEnabled && s.isPlaying),
                activeParticleSystems = root.GetComponentsInChildren<Component>().Count(c => c.GetType().FullName == "UnityEngine.ParticleSystem") };
            foreach (var renderer in renderers)
            {
                var filter = renderer.GetComponent<MeshFilter>();
                var mesh = renderer is SkinnedMeshRenderer skin ? skin.sharedMesh : filter != null ? filter.sharedMesh : null;
                if (mesh == null) continue;
                for (var i = 0; i < mesh.subMeshCount; i++) if (mesh.GetTopology(i) == MeshTopology.Triangles) result.triangles += mesh.GetIndexCount(i) / 3;
            }
            return result;
        }
        private static void Capture(Camera camera,string output,string name,int width = 540,int height = 960)
        {
            var previousTarget = camera.targetTexture; var previousActive = RenderTexture.active;
            var target = new RenderTexture(width,height,24); var pixels = new Texture2D(width,height,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0,0,width,height),0,0); pixels.Apply(false);
                var bytes = pixels.GetRawTextureData();
                using (var file = File.Create(Path.Combine(output,name + ".ppm")))
                {
                    var header = Encoding.ASCII.GetBytes("P6\n" + width + " " + height + "\n255\n"); file.Write(header,0,header.Length);
                    for (var row = height - 1; row >= 0; row--) file.Write(bytes,row * width * 3,width * 3);
                }
            }
            finally { camera.targetTexture = previousTarget; RenderTexture.active = previousActive; target.Release(); Object.Destroy(target); Object.Destroy(pixels); }
        }
    }
}
