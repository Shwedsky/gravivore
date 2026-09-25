using System;
using System.Collections;
using Gravivore.Core.Stats;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Presentation.Evolution;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gravivore.Tests.PlayMode
{
    public sealed class EvolutionSmokeTests
    {
        [UnityTest]
        public IEnumerator ProgressionCrossesTiers_WithExclusiveVisibleSilhouetteModules()
        {
            var rig = CreateRig();

            Assert.That(rig.View.CurrentTier, Is.EqualTo(EvolutionTier.Tier0));
            Assert.That(rig.View.GetActiveTierModuleCount(), Is.Zero);
            Assert.IsTrue(rig.View.IsAccentActive(PlayerStatType.Power));

            rig.Progression.TryGrant(Death(1));
            Assert.That(rig.View.CurrentTier, Is.EqualTo(EvolutionTier.Tier1));
            Assert.That(rig.View.GetActiveTierModuleCount(), Is.EqualTo(2));

            rig.Progression.TryGrant(Death(2));
            Assert.That(rig.View.CurrentTier, Is.EqualTo(EvolutionTier.Tier2));
            Assert.That(rig.View.GetActiveTierModuleCount(), Is.EqualTo(3));
            Assert.That(rig.View.SocketCount, Is.EqualTo(4));

            var rebuiltTier1 = new EvolutionVisualState(EvolutionTier.Tier1, PlayerStatType.Power);
            rig.View.Apply(rebuiltTier1);
            rig.View.Apply(rebuiltTier1);
            Assert.That(rig.View.CurrentTier, Is.EqualTo(EvolutionTier.Tier1));
            Assert.That(rig.View.GetActiveTierModuleCount(), Is.EqualTo(2));

            rig.Dispose();
            yield return null;
        }

        [UnityTest]
        public IEnumerator DominantAccentAndRecreatedPresenter_UseCurrentPermanentState()
        {
            var rig = CreateRig();
            rig.Stats.SetLevel(PlayerStatType.Mobility, 4);
            rig.Progression.TryGrant(Death(1));
            Assert.That(rig.View.CurrentTier, Is.EqualTo(EvolutionTier.Tier1));
            Assert.IsTrue(rig.View.IsAccentActive(PlayerStatType.Mobility));
            Assert.IsFalse(rig.View.IsAccentActive(PlayerStatType.Power));

            Object.Destroy(rig.Presenter);
            yield return null;
            rig.Presenter = rig.Player.AddComponent<PlayerEvolutionPresenter>();
            rig.Presenter.Initialize(
                rig.Progression,
                rig.Stats,
                rig.Catalog.Selection,
                rig.View);

            Assert.That(rig.Presenter.CurrentTier, Is.EqualTo(EvolutionTier.Tier1));
            Assert.That(rig.Presenter.CurrentDominantStat, Is.EqualTo(PlayerStatType.Mobility));
            Assert.That(rig.View.GetActiveTierModuleCount(), Is.EqualTo(2));
            Assert.IsTrue(rig.View.IsAccentActive(PlayerStatType.Mobility));

            rig.Dispose();
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayerDeathAndRespawn_PreserveEvolutionState()
        {
            var rig = CreateRig(withHealth: true);
            rig.Progression.TryGrant(Death(1));
            rig.Progression.TryGrant(Death(2));
            rig.Stats.SetLevel(PlayerStatType.Armor, 4);
            var expectedLevels = rig.Stats.BaseLevels;
            Assert.That(rig.View.CurrentTier, Is.EqualTo(EvolutionTier.Tier2));

            var health = rig.Player.GetComponent<PlayerHealthController>();
            var result = health.ApplyDamage(new DamageRequest(10000f, DamageType.Physical));

            Assert.IsTrue(result.WasLethal);
            Assert.That(rig.View.CurrentTier, Is.EqualTo(EvolutionTier.Tier2));
            Assert.IsTrue(rig.View.IsAccentActive(PlayerStatType.Armor));
            Assert.That(rig.Stats.BaseLevels.Armor, Is.EqualTo(expectedLevels.Armor));
            Assert.That(health.CurrentHitPoints, Is.EqualTo(health.MaximumHitPoints));

            rig.Dispose();
            yield return null;
        }

        private static RuntimeRig CreateRig(bool withHealth = false)
        {
            var componentTypes = withHealth
                ? new[]
                {
                    typeof(CharacterController),
                    typeof(PlayerHealthController),
                    typeof(PlayerEvolutionView),
                    typeof(PlayerEvolutionPresenter)
                }
                : new[] { typeof(PlayerEvolutionView), typeof(PlayerEvolutionPresenter) };
            var player = new GameObject("Evolution Smoke Player", componentTypes);
            var visualRoot = new GameObject("Player Visual Root").transform;
            visualRoot.SetParent(player.transform, false);
            var stats = CreateStats();
            var progression = new AssimilationProgressionService(
                stats,
                new ProgressionState(),
                new ProgressionConfiguration(
                    new ProgressionThresholdCurve(100f, 0f, 0f, 100f),
                    new[] { new CoreReward("enemy", PlayerStatType.Power, 1f, 15) }));
            var catalog = CreateCatalog();
            var view = player.GetComponent<PlayerEvolutionView>();
            view.Initialize(visualRoot, catalog);
            var presenter = player.GetComponent<PlayerEvolutionPresenter>();
            presenter.Initialize(progression, stats, catalog.Selection, view);
            if (withHealth)
            {
                player.GetComponent<PlayerHealthController>().Initialize(
                    player.GetComponent<CharacterController>(),
                    stats,
                    new Vector3(3f, 0f, -2f),
                    1f);
            }

            return new RuntimeRig(player, stats, progression, catalog, view, presenter);
        }

        private static EvolutionVisualCatalog CreateCatalog()
        {
            var selection = new EvolutionConfiguration(
                10,
                30,
                new[]
                {
                    PlayerStatType.Power,
                    PlayerStatType.Hull,
                    PlayerStatType.Armor,
                    PlayerStatType.Flux,
                    PlayerStatType.Mobility
                });
            var tiers = new[]
            {
                new EvolutionTierModuleSet(EvolutionTier.Tier0, Array.Empty<EvolutionModuleConfiguration>()),
                new EvolutionTierModuleSet(EvolutionTier.Tier1, new[]
                {
                    Module("tier1-left", EvolutionSocketId.Left),
                    Module("tier1-right", EvolutionSocketId.Right)
                }),
                new EvolutionTierModuleSet(EvolutionTier.Tier2, new[]
                {
                    Module("tier2-left", EvolutionSocketId.Left),
                    Module("tier2-right", EvolutionSocketId.Right),
                    Module("tier2-rear", EvolutionSocketId.Rear)
                })
            };
            var accents = new[]
            {
                new EvolutionAccentModule(PlayerStatType.Power, Module("accent-power", EvolutionSocketId.Core)),
                new EvolutionAccentModule(PlayerStatType.Hull, Module("accent-hull", EvolutionSocketId.Core)),
                new EvolutionAccentModule(PlayerStatType.Armor, Module("accent-armor", EvolutionSocketId.Core)),
                new EvolutionAccentModule(PlayerStatType.Flux, Module("accent-flux", EvolutionSocketId.Core)),
                new EvolutionAccentModule(PlayerStatType.Mobility, Module("accent-mobility", EvolutionSocketId.Rear))
            };
            return new EvolutionVisualCatalog(selection, tiers, accents);
        }

        private static EvolutionModuleConfiguration Module(string id, EvolutionSocketId socket)
        {
            return new EvolutionModuleConfiguration(
                id,
                socket,
                PrimitiveType.Cube,
                Vector3.zero,
                Vector3.zero,
                new Vector3(0.2f, 0.2f, 0.2f),
                Color.white);
        }

        private static EnemyDeathEvent Death(int seed)
        {
            return new EnemyDeathEvent(
                new EnemyLifeId(new Guid(seed, 0, 0, new byte[8])),
                "enemy",
                Vector3.zero);
        }

        private static PlayerStatsState CreateStats()
        {
            var curve = new StatCurve(10, 10f, 1f, 0f, 0.1f, 1000f);
            var configuration = new PlayerStatsConfiguration(
                curve,
                curve,
                curve,
                curve,
                curve,
                0.1f,
                100f,
                new PlayerStatLevels(1, 1, 1, 1, 1));
            return new PlayerStatsState(configuration, configuration.StartingLevels);
        }

        private sealed class RuntimeRig
        {
            public RuntimeRig(
                GameObject player,
                PlayerStatsState stats,
                AssimilationProgressionService progression,
                EvolutionVisualCatalog catalog,
                PlayerEvolutionView view,
                PlayerEvolutionPresenter presenter)
            {
                Player = player;
                Stats = stats;
                Progression = progression;
                Catalog = catalog;
                View = view;
                Presenter = presenter;
            }

            public GameObject Player { get; }
            public PlayerStatsState Stats { get; }
            public AssimilationProgressionService Progression { get; }
            public EvolutionVisualCatalog Catalog { get; }
            public PlayerEvolutionView View { get; }
            public PlayerEvolutionPresenter Presenter { get; set; }

            public void Dispose()
            {
                if (Presenter != null)
                {
                    Presenter.Shutdown();
                }

                Progression.Dispose();
                Object.Destroy(Player);
            }
        }
    }
}
