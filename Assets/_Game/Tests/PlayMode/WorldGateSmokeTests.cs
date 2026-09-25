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
        public IEnumerator FiveZonesAreReachable_AndGateCollidersFollowMonotonicState()
        {
            var root = new GameObject("World Test Root");
            var presenter = root.AddComponent<Chapter01WorldPresenter>();
            var state = new WorldUnlockState("elite-gate", "boss-gate", "magnetar-guard");
            presenter.Initialize(CreateConfiguration(), state);

            Assert.That(presenter.ZoneCount, Is.EqualTo(5));
            Physics.SyncTransforms();
            var hardBlockers = LayerMask.GetMask("HardBlocker");
            for (var i = 0; i < presenter.ZoneCount; i++)
            {
                Assert.IsTrue(presenter.HasDirectRouteToZone(i));
                Assert.IsFalse(Physics.Linecast(
                    Vector3.up,
                    presenter.GetZoneCenter(i) + Vector3.up,
                    hardBlockers), $"Zone {i} route is blocked.");
            }

            Assert.IsTrue(presenter.EliteGate.IsLocked);
            Assert.IsTrue(presenter.EliteGate.BlockingCollider.enabled);
            Assert.IsTrue(presenter.BossGate.IsLocked);
            state.TryUnlockEliteGate();
            Assert.IsFalse(presenter.EliteGate.IsLocked);
            Assert.IsFalse(presenter.EliteGate.BlockingCollider.enabled);
            Assert.IsTrue(presenter.BossGate.IsLocked);
            state.RecordEliteDefeated("magnetar-guard");
            Assert.IsFalse(presenter.BossGate.IsLocked);
            Assert.IsFalse(presenter.BossGate.BlockingCollider.enabled);

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
    }
}
