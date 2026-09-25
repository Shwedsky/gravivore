using System;
using Gravivore.Core.Stats;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Presentation.Evolution;
using NUnit.Framework;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class EvolutionTests
    {
        [TestCase(0, EvolutionTier.Tier0)]
        [TestCase(9, EvolutionTier.Tier0)]
        [TestCase(10, EvolutionTier.Tier1)]
        [TestCase(19, EvolutionTier.Tier1)]
        [TestCase(30, EvolutionTier.Tier2)]
        [TestCase(100, EvolutionTier.Tier2)]
        public void TierSelection_UsesExactConfiguredThresholds(long score, EvolutionTier expected)
        {
            Assert.That(
                EvolutionStateSelector.SelectTier(score, CreateEvolutionConfiguration()),
                Is.EqualTo(expected));
        }

        [Test]
        public void EvolutionConfiguration_RejectsInvalidThresholdsAndPriority()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new EvolutionConfiguration(0, 10, DefaultPriority()));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new EvolutionConfiguration(10, 10, DefaultPriority()));
            Assert.Throws<ArgumentException>(
                () => new EvolutionConfiguration(
                    10,
                    30,
                    new[]
                    {
                        PlayerStatType.Power,
                        PlayerStatType.Power,
                        PlayerStatType.Armor,
                        PlayerStatType.Flux,
                        PlayerStatType.Mobility
                    }));
        }

        [Test]
        public void VisualCatalog_RejectsMissingRequiredSocketConfiguration()
        {
            var tiers = new[]
            {
                new EvolutionTierModuleSet(EvolutionTier.Tier0, Array.Empty<EvolutionModuleConfiguration>()),
                new EvolutionTierModuleSet(EvolutionTier.Tier1, new[]
                {
                    Module("tier1-left", EvolutionSocketId.Left)
                }),
                new EvolutionTierModuleSet(EvolutionTier.Tier2, new[]
                {
                    Module("tier2-right", EvolutionSocketId.Right)
                })
            };
            var accents = new[]
            {
                new EvolutionAccentModule(PlayerStatType.Power, Module("power", EvolutionSocketId.Core)),
                new EvolutionAccentModule(PlayerStatType.Hull, Module("hull", EvolutionSocketId.Core)),
                new EvolutionAccentModule(PlayerStatType.Armor, Module("armor", EvolutionSocketId.Core)),
                new EvolutionAccentModule(PlayerStatType.Flux, Module("flux", EvolutionSocketId.Core)),
                new EvolutionAccentModule(PlayerStatType.Mobility, Module("mobility", EvolutionSocketId.Core))
            };

            Assert.Throws<InvalidOperationException>(
                () => new EvolutionVisualCatalog(CreateEvolutionConfiguration(), tiers, accents));
        }

        [TestCase(PlayerStatType.Power)]
        [TestCase(PlayerStatType.Hull)]
        [TestCase(PlayerStatType.Armor)]
        [TestCase(PlayerStatType.Flux)]
        [TestCase(PlayerStatType.Mobility)]
        public void DominantStatSelection_SupportsEveryPermanentStat(PlayerStatType expected)
        {
            var levels = new PlayerStatLevels(1, 1, 1, 1, 1).WithLevel(expected, 3);

            var result = EvolutionStateSelector.SelectDominantStat(
                levels,
                CreateEvolutionConfiguration());

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void DominantStatTie_UsesConfiguredStablePriority()
        {
            var configuration = new EvolutionConfiguration(
                10,
                30,
                new[]
                {
                    PlayerStatType.Mobility,
                    PlayerStatType.Armor,
                    PlayerStatType.Flux,
                    PlayerStatType.Hull,
                    PlayerStatType.Power
                });

            var result = EvolutionStateSelector.SelectDominantStat(
                new PlayerStatLevels(5, 5, 5, 5, 5),
                configuration);

            Assert.That(result, Is.EqualTo(PlayerStatType.Mobility));
        }

        [Test]
        public void DominantAccentChange_DoesNotChangeTierOrFireMilestone()
        {
            var stats = CreateStats();
            var progression = CreateProgression(stats, 1);
            var view = new RecordingView();
            var presenterObject = new GameObject("Evolution Presenter Test", typeof(PlayerEvolutionPresenter));
            var presenter = presenterObject.GetComponent<PlayerEvolutionPresenter>();
            var tierEvents = 0;
            presenter.TierChanged += _ => tierEvents++;
            presenter.Initialize(progression, stats, CreateEvolutionConfiguration(), view);

            stats.SetLevel(PlayerStatType.Mobility, 3);

            Assert.That(presenter.CurrentTier, Is.EqualTo(EvolutionTier.Tier0));
            Assert.That(presenter.CurrentDominantStat, Is.EqualTo(PlayerStatType.Mobility));
            Assert.That(view.LastState.Tier, Is.EqualTo(EvolutionTier.Tier0));
            Assert.That(tierEvents, Is.Zero);
            Object.DestroyImmediate(presenterObject);
            progression.Dispose();
        }

        [Test]
        public void Presenter_RepeatedApplyIsIdempotentAndMilestoneFiresOnce()
        {
            var stats = CreateStats();
            var progression = CreateProgression(stats, 10);
            var view = new RecordingView();
            var vfx = new RecordingVfx();
            var presenterObject = new GameObject("Evolution Idempotence Test", typeof(PlayerEvolutionPresenter));
            var presenter = presenterObject.GetComponent<PlayerEvolutionPresenter>();
            var tierEvents = 0;
            presenter.TierChanged += _ => tierEvents++;
            presenter.Initialize(progression, stats, CreateEvolutionConfiguration(), view, vfx);

            Assert.IsFalse(presenter.ApplyCurrentState());
            progression.TryGrant(Death(1));
            Assert.IsFalse(presenter.ApplyCurrentState());

            Assert.That(presenter.CurrentTier, Is.EqualTo(EvolutionTier.Tier1));
            Assert.That(view.ApplyCount, Is.EqualTo(2));
            Assert.That(tierEvents, Is.EqualTo(1));
            Assert.That(vfx.PlayCount, Is.EqualTo(1));
            Object.DestroyImmediate(presenterObject);
            progression.Dispose();
        }

        [Test]
        public void RecreatedPresenter_AppliesCurrentStateWithoutReplayingMilestone()
        {
            var stats = CreateStats();
            var progression = CreateProgression(stats, 15);
            progression.TryGrant(Death(1));
            progression.TryGrant(Death(2));
            stats.SetLevel(PlayerStatType.Armor, 4);
            var view = new RecordingView();
            var vfx = new RecordingVfx();
            var presenterObject = new GameObject("Evolution Rebuild Test", typeof(PlayerEvolutionPresenter));
            var presenter = presenterObject.GetComponent<PlayerEvolutionPresenter>();
            var tierEvents = 0;
            presenter.TierChanged += _ => tierEvents++;

            presenter.Initialize(progression, stats, CreateEvolutionConfiguration(), view, vfx);

            Assert.That(presenter.CurrentTier, Is.EqualTo(EvolutionTier.Tier2));
            Assert.That(presenter.CurrentDominantStat, Is.EqualTo(PlayerStatType.Armor));
            Assert.That(view.ApplyCount, Is.EqualTo(1));
            Assert.That(tierEvents, Is.Zero);
            Assert.That(vfx.PlayCount, Is.Zero);
            Object.DestroyImmediate(presenterObject);
            progression.Dispose();
        }

        [Test]
        public void VfxFailure_DoesNotRollbackProgressionOrAppliedTier()
        {
            var stats = CreateStats();
            var progression = CreateProgression(stats, 10);
            var view = new RecordingView();
            var presenterObject = new GameObject("Evolution VFX Failure Test", typeof(PlayerEvolutionPresenter));
            var presenter = presenterObject.GetComponent<PlayerEvolutionPresenter>();
            presenter.Initialize(
                progression,
                stats,
                CreateEvolutionConfiguration(),
                view,
                new ThrowingVfx());

            Assert.Throws<AggregateException>(() => progression.TryGrant(Death(1)));

            Assert.That(progression.State.TotalAssimilationScore, Is.EqualTo(10));
            Assert.That(presenter.CurrentTier, Is.EqualTo(EvolutionTier.Tier1));
            Assert.That(view.LastState.Tier, Is.EqualTo(EvolutionTier.Tier1));
            Object.DestroyImmediate(presenterObject);
            progression.Dispose();
        }

        private static EvolutionConfiguration CreateEvolutionConfiguration()
        {
            return new EvolutionConfiguration(10, 30, DefaultPriority());
        }

        private static PlayerStatType[] DefaultPriority()
        {
            return new[]
            {
                PlayerStatType.Power,
                PlayerStatType.Hull,
                PlayerStatType.Armor,
                PlayerStatType.Flux,
                PlayerStatType.Mobility
            };
        }

        private static AssimilationProgressionService CreateProgression(
            PlayerStatsState stats,
            long scorePerReward)
        {
            return new AssimilationProgressionService(
                stats,
                new ProgressionState(),
                new ProgressionConfiguration(
                    new ProgressionThresholdCurve(100f, 0f, 0f, 100f),
                    new[] { new CoreReward("enemy", PlayerStatType.Power, 1f, scorePerReward) }));
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
            var curve = new StatCurve(10, 1f, 1f, 0f, 0.1f, 100f);
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

        private static EvolutionModuleConfiguration Module(string id, EvolutionSocketId socket)
        {
            return new EvolutionModuleConfiguration(
                id,
                socket,
                PrimitiveType.Cube,
                Vector3.zero,
                Vector3.zero,
                Vector3.one,
                Color.white);
        }

        private sealed class RecordingView : IPlayerEvolutionView
        {
            public int ApplyCount { get; private set; }
            public EvolutionVisualState LastState { get; private set; }

            public void Apply(EvolutionVisualState state)
            {
                ApplyCount++;
                LastState = state;
            }
        }

        private sealed class RecordingVfx : IEvolutionVfxHook
        {
            public int PlayCount { get; private set; }

            public void Play(in EvolutionTierChangedEvent change)
            {
                PlayCount++;
            }
        }

        private sealed class ThrowingVfx : IEvolutionVfxHook
        {
            public void Play(in EvolutionTierChangedEvent change)
            {
                throw new InvalidOperationException("VFX failed.");
            }
        }
    }
}
