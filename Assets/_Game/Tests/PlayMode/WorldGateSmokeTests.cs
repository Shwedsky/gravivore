using System.Collections;
using Gravivore.Core.Stats;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.World;
using Gravivore.Presentation.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gravivore.Tests.PlayMode
{
    public sealed class WorldGateSmokeTests
    {
        [UnityTest]
        public IEnumerator LockedGatesAndWorldBoundary_BlockBypassUntilAuthoritativeUnlocks()
        {
            var root = new GameObject("World Test Root");
            var presenter = root.AddComponent<Chapter01WorldPresenter>();
            var state = new WorldUnlockState("elite-gate", "boss-gate", "magnetar-guard");
            presenter.Initialize(CreateConfiguration(), state);

            Physics.SyncTransforms();
            var hardBlockers = LayerMask.GetMask("HardBlocker");
            var walker = CreateWalker(new Vector3(0f, 0f, 14f));

            Assert.IsTrue(presenter.EliteGate.IsLocked);
            Assert.IsTrue(presenter.EliteGate.BlockingCollider.enabled);
            Assert.IsTrue(presenter.BossGate.IsLocked);
            Move(walker, Vector3.forward * 3f);
            Assert.That(walker.transform.position.z, Is.LessThan(15f), "Locked elite gate allowed a direct passage.");

            ResetWalker(walker, new Vector3(19f, 0f, 14f));
            Move(walker, Vector3.right * 4f);
            Assert.That(
                walker.transform.position.x,
                Is.LessThan(presenter.Bounds.MaxX),
                "CharacterController escaped past the east world boundary.");

            state.TryUnlockEliteGate();
            Assert.IsFalse(presenter.EliteGate.IsLocked);
            Assert.IsFalse(presenter.EliteGate.BlockingCollider.enabled);
            Assert.IsTrue(presenter.BossGate.IsLocked);
            Assert.IsTrue(Physics.Linecast(
                new Vector3(10f, 1f, 14f),
                new Vector3(10f, 1f, 16f),
                hardBlockers), "Elite flank wall was disabled with the central opening.");
            ResetWalker(walker, new Vector3(0f, 0f, 14f));
            Move(walker, Vector3.forward * 3f);
            Assert.That(walker.transform.position.z, Is.GreaterThan(15.5f), "Unlocked elite opening remained blocked.");

            ResetWalker(walker, new Vector3(0f, 0f, 20f));
            Move(walker, Vector3.forward * 3f);
            Assert.That(walker.transform.position.z, Is.LessThan(21f), "Boss gate opened before elite defeat.");

            state.RecordEliteDefeated("magnetar-guard");
            Assert.IsFalse(presenter.BossGate.IsLocked);
            Assert.IsFalse(presenter.BossGate.BlockingCollider.enabled);
            ResetWalker(walker, new Vector3(0f, 0f, 20f));
            Move(walker, Vector3.forward * 3f);
            Assert.That(walker.transform.position.z, Is.GreaterThan(21.5f), "Unlocked boss opening remained blocked.");

            Object.Destroy(walker.gameObject);
            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator OrdinaryZonesAndSpawnAnchors_StayInsideUnobstructedPlayableBounds()
        {
            var root = new GameObject("World Bounds Test Root");
            var presenter = root.AddComponent<Chapter01WorldPresenter>();
            presenter.Initialize(
                CreateConfiguration(),
                new WorldUnlockState("elite-gate", "boss-gate", "magnetar-guard"));
            Physics.SyncTransforms();

            Assert.That(presenter.PerimeterColliderCount, Is.EqualTo(4));
            var hardBlockers = LayerMask.GetMask("HardBlocker");
            var offsets = new[]
            {
                new Vector3(-1.5f, 0f, -1.5f),
                new Vector3(1.5f, 0f, -1.5f),
                new Vector3(-1.5f, 0f, 1.5f),
                new Vector3(1.5f, 0f, 1.5f)
            };
            for (var i = 0; i < presenter.ZoneCount; i++)
            {
                var center = presenter.GetZoneCenter(i);
                Assert.IsTrue(presenter.Bounds.Contains(center));
                Assert.IsFalse(Physics.Linecast(Vector3.up, center + Vector3.up, hardBlockers));
                for (var anchorIndex = 0; anchorIndex < offsets.Length; anchorIndex++)
                {
                    var anchor = center + offsets[anchorIndex];
                    Assert.IsTrue(presenter.Bounds.Contains(anchor, 0.1f));
                    Assert.IsFalse(Physics.CheckSphere(anchor + Vector3.up, 0.1f, hardBlockers));
                }
            }

            for (var i = 0; i < presenter.PerimeterColliderCount; i++)
            {
                var boundary = presenter.GetPerimeterCollider(i);
                Assert.IsTrue(boundary.enabled);
                Assert.That(boundary.gameObject.layer, Is.EqualTo(LayerMask.NameToLayer("HardBlocker")));
            }

            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RestoredSnapshot_AppliesImmediatelyWithoutMilestoneReplay()
        {
            var snapshot = new WorldUnlockSnapshot(true, true, true);
            var state = WorldUnlockState.Restore("elite-gate", "boss-gate", "magnetar-guard", snapshot);
            var eventCount = 0;
            state.GateUnlocked += _ => eventCount++;
            var root = new GameObject("Restored World Test Root");
            var presenter = root.AddComponent<Chapter01WorldPresenter>();
            presenter.Initialize(CreateConfiguration(), state);

            Assert.IsFalse(presenter.EliteGate.IsLocked);
            Assert.IsFalse(presenter.BossGate.IsLocked);
            Assert.That(eventCount, Is.Zero);

            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator UnlockedWorldState_SurvivesPlayerDeathAndRespawn()
        {
            var state = new WorldUnlockState("elite-gate", "boss-gate", "magnetar-guard");
            state.TryUnlockEliteGate();
            var player = new GameObject("Player", typeof(CharacterController), typeof(PlayerHealthController));
            var stats = CreateStats();
            var health = player.GetComponent<PlayerHealthController>();
            health.Initialize(player.GetComponent<CharacterController>(), stats, Vector3.zero, 0f);

            var result = health.ApplyDamage(new DamageRequest(10000f, DamageType.Physical));

            Assert.IsTrue(result.WasLethal);
            Assert.IsTrue(state.EliteGateUnlocked);
            Assert.IsFalse(state.BossGateUnlocked);
            Object.Destroy(player);
            yield return null;
        }

        private static Chapter01WorldConfiguration CreateConfiguration()
        {
            var zones = new[]
            {
                Zone("relay-yard", -8f, 6f, -11f, 6f, Color.cyan),
                Zone("cutting-floor", 0f, 10f, 0f, 13.5f, Color.red),
                Zone("shield-dump", 8f, 6f, 11f, 6f, Color.green),
                Zone("capacitor-field", -7f, -7f, -10f, -9f, Color.yellow),
                Zone("hauler-graveyard", 7f, -7f, 10f, -9f, Color.magenta)
            };
            return new Chapter01WorldConfiguration(
                "chapter01-scrap-exclusion",
                Vector3.zero,
                new Vector3(0f, 0f, 8f),
                new Vector2(40f, 48f),
                new WorldBoundaryConfiguration(0.6f, 2.5f),
                zones,
                new WorldGateConfiguration("elite-gate", new Vector3(0f, 0f, 15f), new Vector3(5f, 2.5f, 0.6f)),
                new WorldGateConfiguration("boss-gate", new Vector3(0f, 0f, 21f), new Vector3(5f, 2.5f, 0.6f)),
                new Vector3(0f, 0f, 27f),
                5f,
                "magnetar-guard",
                new EliteGateRequirement(new[] { "a", "b", "c", "d", "e" }, 5));
        }

        private static WorldZoneConfiguration Zone(
            string id, float x, float z, float landmarkX, float landmarkZ, Color color)
        {
            return new WorldZoneConfiguration(
                id,
                new Vector3(x, 0f, z),
                new Vector3(landmarkX, 0f, landmarkZ),
                color);
        }

        private static PlayerStatsState CreateStats()
        {
            var configuration = new PlayerStatsConfiguration(
                new StatCurve(10, 10f, 1f, 0f, 0f, 1000f),
                new StatCurve(10, 100f, 10f, 0f, 1f, 10000f),
                new StatCurve(10, 5f, 1f, 0f, 0f, 1000f),
                new StatCurve(10, 1f, -0.05f, 0f, 0.2f, 10f),
                new StatCurve(10, 4f, 0.25f, 0f, 0f, 20f),
                0.2f,
                20f,
                new PlayerStatLevels(1, 1, 1, 1, 1));
            return new PlayerStatsState(configuration, configuration.StartingLevels);
        }

        private static CharacterController CreateWalker(Vector3 position)
        {
            var walkerObject = new GameObject("Boundary Test Walker", typeof(CharacterController));
            var walker = walkerObject.GetComponent<CharacterController>();
            walker.radius = 0.35f;
            walker.height = 1.8f;
            walker.center = new Vector3(0f, 0.9f, 0f);
            walker.skinWidth = 0.03f;
            walkerObject.transform.position = position;
            return walker;
        }

        private static void ResetWalker(CharacterController walker, Vector3 position)
        {
            walker.enabled = false;
            walker.transform.position = position;
            walker.enabled = true;
            Physics.SyncTransforms();
        }

        private static void Move(CharacterController walker, Vector3 displacement)
        {
            const int steps = 12;
            var step = displacement / steps;
            for (var i = 0; i < steps; i++) walker.Move(step);
            Physics.SyncTransforms();
        }
    }
}
