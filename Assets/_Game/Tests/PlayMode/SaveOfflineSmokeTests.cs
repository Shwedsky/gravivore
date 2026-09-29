using System;
using System.Collections;
using System.IO;
using Gravivore.Core.Stats;
using Gravivore.Core.Time;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Equipment;
using Gravivore.Presentation.Composition;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Gravivore.Tests.PlayMode
{
    public sealed class SaveOfflineSmokeTests
    {
        [UnityTest]
        public IEnumerator CanonicalChapter_SaveReloadRestoresAuthoritativeGraphAtConstruction()
        {
            var directory = Path.Combine(
                Path.GetTempPath(),
                "gravivore-s12-playmode-" + Guid.NewGuid().ToString("N"));
            var time = new FakeTimeProvider(new DateTime(2031, 4, 5, 12, 0, 0, DateTimeKind.Utc));
            S01SceneCompositionRoot first = null;
            yield return LoadCanonical(directory, time, value => first = value);

            var profileId = first.ProfileId;
            var baseDamageBeforeEquipment = first.PlayerStats.DerivedStats.BaseDamage;
            var item = first.EquipmentCatalog.GetAt(0);
            Assert.IsTrue(first.Equipment.GrantEquipment(item.Id));
            Assert.IsTrue(first.Equipment.Equip(item.Id, item.Slot));
            first.Quests.RecordMovementPerformed();

            var enemyIds = new[]
            {
                "scout-drone", "cutter-unit", "warden", "arc-drone", "carrier"
            };
            var seed = 1;
            for (var i = 0; i < enemyIds.Length; i++)
            {
                Assert.IsTrue(first.Progression.TryGrant(Death(seed++, enemyIds[i])));
            }

            while (first.Progression.State.TotalAssimilationScore < 25)
            {
                Assert.IsTrue(first.Progression.TryGrant(Death(seed++, "scout-drone")));
            }

            Assert.IsTrue(first.WorldUnlocks.State.EliteGateUnlocked);
            Assert.IsTrue(first.MagnetarGuard.IsEncounterActive);
            Assert.IsTrue(first.MagnetarGuard.ApplyDamage(
                new DamageRequest(10000f, DamageType.Gravity)).WasLethal);
            Assert.IsTrue(first.WorldUnlocks.State.EliteDefeated);
            first.PlayerObject.transform.position = new Vector3(0f, 0f, 27f);
            first.CustodianBoss.Tick(0f);
            Assert.IsTrue(first.CustodianBoss.CanBeTargeted);
            Assert.IsTrue(first.CustodianBoss.ApplyDamage(
                new DamageRequest(10000f, DamageType.Gravity)).WasLethal);
            Assert.IsTrue(first.BossCompletion.IsDefeated);

            var levels = first.PlayerStats.BaseLevels;
            var progression = first.Progression.State.ExportSnapshot();
            var equippedDamage = first.PlayerStats.DerivedStats.BaseDamage;
            var evolutionTier = first.EvolutionPresenter.CurrentTier;
            Assert.That(equippedDamage, Is.GreaterThan(baseDamageBeforeEquipment));
            Assert.IsTrue(first.Quests.State.Completed);
            Assert.That(first.OfflineRewards.State.PendingReward, Is.Zero);
            Assert.IsTrue(first.FlushNow());
            UnityEngine.Object.Destroy(first.gameObject);
            yield return null;
            time.UtcNow = time.UtcNow.AddMinutes(30);

            S01SceneCompositionRoot restored = null;
            yield return LoadCanonical(directory, time, value => restored = value);

            Assert.That(restored.ProfileId, Is.EqualTo(profileId));
            Assert.That(restored.PlayerStats.BaseLevels, Is.EqualTo(levels));
            Assert.That(restored.Progression.State.TotalAssimilationScore,
                Is.EqualTo(progression.TotalAssimilationScore));
            Assert.That(restored.Progression.State.GetStatExperience(PlayerStatType.Mobility),
                Is.EqualTo(progression.MobilityExperience));
            Assert.IsTrue(restored.Progression.State.HasFirstKill("scout-drone"));
            Assert.IsTrue(restored.Inventory.HasItem(item.Id));
            Assert.IsTrue(restored.Inventory.TryGetEquipped(item.Slot, out var restoredItemId));
            Assert.That(restoredItemId, Is.EqualTo(item.Id));
            Assert.That(restored.PlayerStats.DerivedStats.BaseDamage, Is.EqualTo(equippedDamage));
            Assert.That(restored.EvolutionPresenter.CurrentTier, Is.EqualTo(evolutionTier));
            Assert.IsTrue(restored.Quests.State.Completed);
            Assert.IsTrue(restored.WorldUnlocks.State.EliteGateUnlocked);
            Assert.IsTrue(restored.WorldUnlocks.State.EliteDefeated);
            Assert.IsTrue(restored.WorldUnlocks.State.BossGateUnlocked);
            Assert.IsTrue(restored.BossCompletion.IsDefeated);
            Assert.IsFalse(restored.MagnetarGuard.IsEncounterActive);
            Assert.That(restored.CustodianBoss.State, Is.EqualTo(
                Gravivore.Gameplay.Encounters.CustodianBossState.Dead));
            Assert.IsFalse(restored.CustodianBoss.CanBeTargeted);
            Assert.That(restored.OfflineRewards.State.PendingReward, Is.EqualTo(15));
            Assert.That(restored.OfflineReturnSummary.EarnedAmount, Is.EqualTo(15));
            Assert.That(restored.OfflineReturnSummary.EligibleDuration, Is.EqualTo(TimeSpan.FromMinutes(30)));

            UnityEngine.Object.Destroy(restored.gameObject);
            yield return null;
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
            LogAssert.NoUnexpectedReceived();
        }

        private static IEnumerator LoadCanonical(
            string directory,
            ITimeProvider time,
            Action<S01SceneCompositionRoot> capture)
        {
            S01SceneCompositionRoot composition = null;
            void Configure(Scene scene, LoadSceneMode mode)
            {
                if (!string.Equals(scene.name, "Chapter01_ScrapExclusion", StringComparison.Ordinal)) return;
                var roots = scene.GetRootGameObjects();
                for (var i = 0; i < roots.Length; i++)
                {
                    if (!roots[i].TryGetComponent(out composition)) continue;
                    composition.ConfigurePersistence(directory, time);
                    return;
                }
            }

            SceneManager.sceneLoaded += Configure;
            var operation = SceneManager.LoadSceneAsync("Chapter01_ScrapExclusion", LoadSceneMode.Single);
            Assert.IsNotNull(operation);
            yield return operation;
            SceneManager.sceneLoaded -= Configure;
            yield return null;
            Assert.IsNotNull(composition);
            capture(composition);
        }

        private static EnemyDeathEvent Death(int seed, string enemyId)
        {
            return new EnemyDeathEvent(
                new EnemyLifeId(new Guid(seed, 0, 0, new byte[8])),
                enemyId,
                Vector3.zero);
        }

        private sealed class FakeTimeProvider : ITimeProvider
        {
            public FakeTimeProvider(DateTime utcNow) => UtcNow = utcNow;
            public DateTime UtcNow { get; set; }
        }
    }
}
