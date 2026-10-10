using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gravivore.Gameplay.Enemies;
using Gravivore.Presentation.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gravivore.Tests.PlayMode
{
    public sealed class Phase3DVisualQualitySmokeTests
    {
        private CanonicalSceneTestScope _scene;

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (_scene != null) yield return _scene.Cleanup();
            _scene = null;
        }

        [UnityTest]
        public IEnumerator CollisionProxiesStayOnAuthoritySideAndCanonicalTraversalRemainsClear()
        {
            yield return LoadControlled();
            yield return null;
            var root = _scene.Root;
            var world = root.WorldPresenter;
            Assert.That(root.VisualEnvironment.GetComponentsInChildren<Collider>(true), Is.Empty);
            Assert.That(world.EnvironmentBlockerCount, Is.EqualTo(world.Layout != null ? world.Layout.BlockerCount : 2 + root.VisualEnvironment.SliceObstacleCount));
            Assert.That(world.GameplayRoot.lossyScale, Is.EqualTo(Vector3.one));

            foreach (var regionId in new[]
            {
                "repair-hub", "shield-dump", "capacitor-field", "hauler-graveyard", "elite-arena", "boss-arena"
            })
            {
                var region = root.VisualEnvironment.GetRegion(regionId).Root;
                Assert.That(region.lossyScale, Is.EqualTo(Vector3.one), regionId + " must remain unit-scale for blocker sizing.");
                Assert.That(Quaternion.Angle(region.rotation, Quaternion.identity), Is.LessThan(.001f),
                    regionId + " rotation changed; blocker audit must be revisited.");
            }

            var blockers = new List<Collider>();
            for (var i = 0; i < world.EnvironmentBlockerCount; i++)
            {
                var blocker = world.GetEnvironmentBlocker(i);
                blockers.Add(blocker);
                Assert.IsNotNull(blocker);
                Assert.IsTrue(blocker.transform.IsChildOf(world.GameplayRoot), blocker.name);
                Assert.That(blocker.gameObject.layer, Is.EqualTo(LayerMask.NameToLayer("HardBlocker")));
                Assert.IsNull(blocker.GetComponent<Renderer>(), blocker.name);
                Assert.IsNull(blocker.GetComponent<MeshFilter>(), blocker.name);
                var authoredSize=world.Layout!=null?world.Layout.GetBlocker(i).Size:blocker.transform.lossyScale;
                Assert.That(Vector3.Distance(blocker.bounds.size, authoredSize), Is.LessThan(.001f),
                    blocker.name + " world dimensions must match the authored proxy size.");
            }

            var hub = root.VisualEnvironment.GetRegion("repair-hub").Root;
            var exit = new Bounds(hub.TransformPoint(new Vector3(0f, .75f, -2.5f)), new Vector3(2f, 1.5f, 5f));
            foreach (var blocker in blockers)
            {
                if (!blocker.name.StartsWith("Repair Hub")) continue;
                Assert.IsFalse(blocker.bounds.Intersects(exit), blocker.name + " blocks the authored south exit.");
            }

            var spawnOrigins = System.Linq.Enumerable.Range(0,5).Select(i=>root.EnemyPopulation.GetSpot(i).Position).ToArray();
            var spawnOffsets = new[]
            {
                new Vector3(-1.5f, 0f, -1.5f), new Vector3(1.5f, 0f, -1.5f),
                new Vector3(-1.5f, 0f, 1.5f), new Vector3(1.5f, 0f, 1.5f)
            };
            foreach (var origin in spawnOrigins)
            foreach (var offset in spawnOffsets)
            foreach (var blocker in blockers)
                AssertPointOutsideXZ(blocker, origin + offset, "canonical ordinary spawn");

            foreach (var blocker in blockers)
            {
                AssertPointOutsideXZ(blocker, root.MagnetarGuard.transform.position, "elite encounter centre");
                AssertPointOutsideXZ(blocker, root.CustodianBoss.transform.position, "boss encounter centre");
                AssertPointOutsideXZ(blocker, world.Configuration.EliteGate.Position, "elite gate centre");
                AssertPointOutsideXZ(blocker, world.Configuration.BossGate.Position, "boss gate centre");
            }

            var basin = new Vector3(0f, 0f, -30f);
            var zones = spawnOrigins;
            // Authored machinery introduces service turns. Connectivity with swept capsule
            // clearance replaces the prototype's requirement for empty straight diagonals.
            Physics.SyncTransforms();
            var lockedRoutes = new Chapter01ProductionSmokeTests.RouteGrid(root,basin);
            foreach(var zone in zones)Assert.IsTrue(lockedRoutes.Reaches(zone),"basin and outer-loop connectivity "+zone);
            world.EliteGate.SetLocked(false);world.BossGate.SetLocked(false);Physics.SyncTransforms();
            var unlockedRoutes = new Chapter01ProductionSmokeTests.RouteGrid(root,basin);
            Assert.IsTrue(unlockedRoutes.Reaches(root.MagnetarGuard.transform.position));
            Assert.IsTrue(unlockedRoutes.Reaches(root.CustodianBoss.transform.position));
            foreach (var blocker in blockers)
                AssertSegmentClearXZ(blocker, world.Configuration.EliteGate.Position, world.Configuration.BossArenaCenter,
                    "elite/boss traversal");
        }

        [UnityTest]
        public IEnumerator IndustrialGateVisualKeepsGameplayBarrierAuthority()
        {
            yield return LoadControlled();
            yield return null;
            var root = _scene.Root;
            var gate = root.WorldPresenter.EliteGate;
            Assert.IsTrue(gate.BlockingCollider.enabled);
            var visualRoot = root.VisualEnvironment.Structures.Find("elite-gate Visuals");
            Assert.IsNotNull(visualRoot);
            var assembly = visualRoot.Find("Containment Gate Assembly");
            Assert.IsNotNull(assembly);
            var authoredRendererCount=root.VisualEnvironment.BlueprintWorldOnly?root.VisualEnvironment.SliceGate.GetComponentsInChildren<Renderer>(true).Length:1;
            Assert.That(assembly.GetComponentsInChildren<Renderer>(true).Length, Is.EqualTo(authoredRendererCount));
            Assert.That(assembly.GetComponentsInChildren<Collider>(true), Is.Empty);
            Assert.That(assembly.GetComponentsInChildren<Light>(true), Is.Empty);
            Assert.That(assembly.GetComponentsInChildren<Renderer>(true)
                .SelectMany(r => r.sharedMaterials).Where(m => m != null).Distinct().Count(), Is.LessThanOrEqualTo(2));

            gate.SetLocked(false);
            Assert.IsFalse(gate.BlockingCollider.enabled);
            Assert.IsFalse(assembly.gameObject.activeSelf);
            Assert.IsTrue(visualRoot.Find("Left Gate Wall Visual").gameObject.activeInHierarchy);
            Assert.IsTrue(visualRoot.Find("Right Gate Wall Visual").gameObject.activeInHierarchy);

            gate.SetLocked(true);
            Assert.IsTrue(gate.BlockingCollider.enabled);
            Assert.IsTrue(assembly.gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator MechanicalMotionInitializesOnceAndStaysInsideVisualEnvironment()
        {
            yield return LoadControlled();
            var root = _scene.Root;
            root.VisualEnvironment.Initialize();
            var presenters = root.GetComponentsInChildren<Phase3DMechanicalMotionPresenter>(true);
            Assert.That(presenters, Has.Length.EqualTo(1));
            var presenter = presenters[0];
            Assert.That(presenter.ChannelCount, Is.Zero,"Retired primitive repair arms no longer run cosmetic idle channels.");

            var elitePosition = root.MagnetarGuard.transform.position;
            var bossPosition = root.CustodianBoss.transform.position;
            var hub = root.VisualEnvironment.GetRegion("repair-hub").Root;
            var arm = root.RepairHub.Manipulators.transform.Find("Authored upper manipulator 0");
            Assert.IsNotNull(arm);
            var before = arm.localRotation;
            root.PlayerHealth.ApplyDamage(new Gravivore.Gameplay.Combat.DamageRequest(50,Gravivore.Gameplay.Combat.DamageType.Physical));
            root.PlayerHealth.Tick(3.1f);root.PlayerHealth.Tick(.1f);
            Assert.IsTrue(root.RepairHub.IsRepairing);

            // A single 100 ms sample can straddle a sine turning point, where
            // Quaternion.Angle rounds the small change to zero. Observe a bounded
            // interval so valid motion is tested independently of its start phase.
            var observedAngle = 0f;
            for (var sample = 0; sample < 10 && observedAngle <= .02f; sample++)
            {
                yield return new WaitForSecondsRealtime(.1f);
                root.RepairHub.Manipulators.Tick(.1f);
                observedAngle = Mathf.Max(observedAngle, Quaternion.Angle(before, arm.localRotation));
            }

            Assert.That(observedAngle, Is.GreaterThan(.02f));
            Assert.That(root.MagnetarGuard.transform.position, Is.EqualTo(elitePosition));
            Assert.That(root.CustodianBoss.transform.position, Is.EqualTo(bossPosition));
        }

        private static void AssertPointOutsideXZ(Collider blocker, Vector3 point, string context)
        {
            var bounds = blocker.bounds;
            if(bounds.min.y>1.4f)return; // Raised service trusses clear the controller.
            var inside = point.x >= bounds.min.x && point.x <= bounds.max.x &&
                         point.z >= bounds.min.z && point.z <= bounds.max.z;
            Assert.IsFalse(inside, blocker.name + " overlaps " + context + " at " + point + ".");
        }

        private static void AssertSegmentClearXZ(Collider blocker, Vector3 from, Vector3 to, string context)
        {
            var bounds = blocker.bounds;
            if(bounds.min.y>1.4f)return;
            var min = new Vector2(bounds.min.x, bounds.min.z);
            var max = new Vector2(bounds.max.x, bounds.max.z);
            var a = new Vector2(from.x, from.z);
            var b = new Vector2(to.x, to.z);
            Assert.IsFalse(SegmentIntersectsRect(a, b, min, max),
                blocker.name + " intersects " + context + " from " + from + " to " + to + ".");
        }

        private static bool SegmentIntersectsRect(Vector2 a, Vector2 b, Vector2 min, Vector2 max)
        {
            var tMin = 0f;
            var tMax = 1f;
            var delta = b - a;
            return ClipAxis(a.x, delta.x, min.x, max.x, ref tMin, ref tMax) &&
                   ClipAxis(a.y, delta.y, min.y, max.y, ref tMin, ref tMax);
        }

        private static bool ClipAxis(float origin, float direction, float min, float max, ref float tMin, ref float tMax)
        {
            if (Mathf.Abs(direction) < 0.00001f) return origin >= min && origin <= max;
            var inverse = 1f / direction;
            var enter = (min - origin) * inverse;
            var exit = (max - origin) * inverse;
            if (enter > exit)
            {
                var swap = enter;
                enter = exit;
                exit = swap;
            }
            tMin = Mathf.Max(tMin, enter);
            tMax = Mathf.Min(tMax, exit);
            return tMin <= tMax;
        }

        private IEnumerator LoadControlled()
        {
            _scene = new CanonicalSceneTestScope();
            yield return _scene.Load();
            var root = _scene.Root;
            root.EnemyPopulation.enabled = false;
            foreach (var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true))
                enemy.enabled = false;
            root.MagnetarGuard.enabled = false;
            root.CustodianBoss.enabled = false;
        }
    }
}
