using System;
using Gravivore.Core.Stats;
using Gravivore.Gameplay.Player;
using NUnit.Framework;

namespace Gravivore.Tests.EditMode
{
    public sealed class PlayerStatsTests
    {
        [TestCase(PlayerStatType.Power, 3, 16f)]
        [TestCase(PlayerStatType.Hull, 3, 124f)]
        [TestCase(PlayerStatType.Armor, 3, 11f)]
        [TestCase(PlayerStatType.Flux, 3, 0.8f)]
        [TestCase(PlayerStatType.Mobility, 3, 5f)]
        public void StatCurve_EvaluatesConfiguredValue(PlayerStatType stat, int level, float expected)
        {
            var configuration = CreateConfiguration();

            var result = configuration.GetCurve(stat).Evaluate(level);

            Assert.That(result, Is.EqualTo(expected).Within(0.0001f));
        }

        [Test]
        public void Calculate_MapsAllLevelsToDerivedValues()
        {
            var configuration = CreateConfiguration();
            var levels = new PlayerStatLevels(3, 2, 4, 5, 5);

            var result = PlayerStatsCalculator.Calculate(configuration, levels);

            Assert.That(result.BaseDamage, Is.EqualTo(16f).Within(0.0001f));
            Assert.That(result.MaxHp, Is.EqualTo(111f).Within(0.0001f));
            Assert.That(result.ArmorValue, Is.EqualTo(14f).Within(0.0001f));
            Assert.That(result.AttackInterval, Is.EqualTo(0.6f).Within(0.0001f));
            Assert.That(result.MoveSpeed, Is.EqualTo(6f).Within(0.0001f));
        }

        [Test]
        public void Calculate_HighFlux_RespectsMinimumAttackInterval()
        {
            var result = PlayerStatsCalculator.Calculate(
                CreateConfiguration(),
                new PlayerStatLevels(1, 1, 1, 10, 1));

            Assert.That(result.AttackInterval, Is.EqualTo(0.4f).Within(0.0001f));
        }

        [Test]
        public void Calculate_HighMobility_RespectsMaximumMoveSpeed()
        {
            var result = PlayerStatsCalculator.Calculate(
                CreateConfiguration(),
                new PlayerStatLevels(1, 1, 1, 1, 10));

            Assert.That(result.MoveSpeed, Is.EqualTo(6f).Within(0.0001f));
        }

        [Test]
        public void InvalidLevelsAndCurveConfiguration_Throw()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new StatCurve(0, 1f, 1f, 0f, 0f, 10f));
            Assert.Throws<ArgumentException>(
                () => new StatCurve(10, float.NaN, 1f, 0f, 0f, 10f));
            Assert.Throws<ArgumentException>(
                () => new StatCurve(10, 1f, 1f, 0f, 10f, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new PlayerStatLevels(0, 1, 1, 1, 1));

            var state = new PlayerStatsState(CreateConfiguration(), new PlayerStatLevels(1, 1, 1, 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => state.SetLevel(PlayerStatType.Power, 11));
            Assert.That(state.BaseLevels.Power, Is.EqualTo(1));
        }

        [Test]
        public void SetLevel_RecalculatesDeterministicallyAndRaisesTypedEvent()
        {
            var configuration = CreateConfiguration();
            var state = new PlayerStatsState(configuration, configuration.StartingLevels);
            var eventCount = 0;
            var lastChange = default(PlayerStatChange);
            state.StatChanged += change =>
            {
                eventCount++;
                lastChange = change;
            };

            var changed = state.SetLevel(PlayerStatType.Power, 3);
            var expected = PlayerStatsCalculator.Calculate(
                configuration,
                new PlayerStatLevels(3, 1, 1, 1, 1));

            Assert.IsTrue(changed);
            Assert.That(eventCount, Is.EqualTo(1));
            Assert.That(lastChange.Stat, Is.EqualTo(PlayerStatType.Power));
            Assert.That(lastChange.PreviousLevel, Is.EqualTo(1));
            Assert.That(lastChange.CurrentLevel, Is.EqualTo(3));
            Assert.That(state.DerivedStats.BaseDamage, Is.EqualTo(expected.BaseDamage).Within(0.0001f));
            Assert.That(lastChange.CurrentDerivedStats.BaseDamage, Is.EqualTo(expected.BaseDamage).Within(0.0001f));

            Assert.IsFalse(state.SetLevel(PlayerStatType.Power, 3));
            Assert.That(eventCount, Is.EqualTo(1));
        }

        [Test]
        public void Modifiers_ChangeDerivedValuesWithoutMutatingBaseLevelsOrHardCaps()
        {
            var configuration = CreateConfiguration();
            var levels = new PlayerStatLevels(2, 3, 4, 5, 6);
            var state = new PlayerStatsState(
                configuration,
                levels,
                new IPlayerDerivedStatsModifier[] { new TestModifier() });

            Assert.That(state.BaseLevels.Power, Is.EqualTo(2));
            Assert.That(state.BaseLevels.Hull, Is.EqualTo(3));
            Assert.That(state.BaseLevels.Armor, Is.EqualTo(4));
            Assert.That(state.BaseLevels.Flux, Is.EqualTo(5));
            Assert.That(state.BaseLevels.Mobility, Is.EqualTo(6));
            Assert.That(state.DerivedStats.BaseDamage, Is.EqualTo(25.5f).Within(0.0001f));
            Assert.That(state.DerivedStats.AttackInterval, Is.EqualTo(0.4f).Within(0.0001f));
            Assert.That(state.DerivedStats.MoveSpeed, Is.EqualTo(6f).Within(0.0001f));
        }

        private static PlayerStatsConfiguration CreateConfiguration()
        {
            return new PlayerStatsConfiguration(
                new StatCurve(10, 10f, 2f, 0.5f, 0f, 1000f),
                new StatCurve(10, 100f, 10f, 1f, 1f, 10000f),
                new StatCurve(10, 5f, 3f, 0f, 0f, 1000f),
                new StatCurve(10, 1f, -0.1f, 0f, 0.05f, 10f),
                new StatCurve(10, 4f, 0.5f, 0f, 0f, 20f),
                0.4f,
                6f,
                new PlayerStatLevels(1, 1, 1, 1, 1));
        }

        private sealed class TestModifier : IPlayerDerivedStatsModifier
        {
            public PlayerDerivedStats Apply(in PlayerDerivedStats currentValues)
            {
                return new PlayerDerivedStats(
                    currentValues.BaseDamage + 13f,
                    currentValues.MaxHp,
                    currentValues.ArmorValue,
                    0.05f,
                    100f);
            }
        }
    }
}
