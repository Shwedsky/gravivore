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

        [Test]
        public void SetModifiers_ReplacesModifiersAndPreservesBaseLevelsAndCaps()
        {
            var levels = new PlayerStatLevels(2, 3, 4, 5, 6);
            var state = new PlayerStatsState(CreateConfiguration(), levels);
            var eventCount = 0;
            var lastChange = default(PlayerDerivedStatsChange);
            state.DerivedStatsChanged += change =>
            {
                eventCount++;
                lastChange = change;
            };

            Assert.IsTrue(state.SetModifiers(new IPlayerDerivedStatsModifier[]
            {
                new ConfigurableModifier(5f, 0.05f, 100f)
            }));
            Assert.That(state.DerivedStats.BaseDamage, Is.EqualTo(17.5f).Within(0.0001f));
            Assert.That(state.DerivedStats.AttackInterval, Is.EqualTo(0.4f).Within(0.0001f));
            Assert.That(state.DerivedStats.MoveSpeed, Is.EqualTo(6f).Within(0.0001f));

            Assert.IsTrue(state.SetModifiers(new IPlayerDerivedStatsModifier[]
            {
                new ConfigurableModifier(2f, 0.7f, 5f)
            }));
            Assert.That(state.DerivedStats.BaseDamage, Is.EqualTo(14.5f).Within(0.0001f));
            Assert.That(state.DerivedStats.AttackInterval, Is.EqualTo(0.7f).Within(0.0001f));
            Assert.That(state.DerivedStats.MoveSpeed, Is.EqualTo(5f).Within(0.0001f));
            AssertLevelsEqual(levels, state.BaseLevels);
            Assert.That(eventCount, Is.EqualTo(2));
            Assert.That(lastChange.Reason, Is.EqualTo(PlayerDerivedStatsChangeReason.ModifiersChanged));
            Assert.That(lastChange.PreviousValues.BaseDamage, Is.EqualTo(17.5f).Within(0.0001f));
            Assert.That(lastChange.CurrentValues.BaseDamage, Is.EqualTo(14.5f).Within(0.0001f));
        }

        [Test]
        public void SetModifiers_NullRemovesModifiersAndOnlyRaisesForActualChange()
        {
            var configuration = CreateConfiguration();
            var levels = new PlayerStatLevels(2, 2, 2, 2, 2);
            var state = new PlayerStatsState(
                configuration,
                levels,
                new IPlayerDerivedStatsModifier[] { new ConfigurableModifier(5f, 0.7f, 5f) });
            var eventCount = 0;
            state.DerivedStatsChanged += _ => eventCount++;

            Assert.IsTrue(state.SetModifiers(null));
            var expected = PlayerStatsCalculator.Calculate(configuration, levels);
            Assert.That(state.DerivedStats.BaseDamage, Is.EqualTo(expected.BaseDamage).Within(0.0001f));
            Assert.That(state.DerivedStats.AttackInterval, Is.EqualTo(expected.AttackInterval).Within(0.0001f));
            Assert.That(state.DerivedStats.MoveSpeed, Is.EqualTo(expected.MoveSpeed).Within(0.0001f));
            AssertLevelsEqual(levels, state.BaseLevels);
            Assert.That(eventCount, Is.EqualTo(1));

            Assert.IsFalse(state.SetModifiers(Array.Empty<IPlayerDerivedStatsModifier>()));
            Assert.That(eventCount, Is.EqualTo(1));
        }

        [Test]
        public void RecalculateDerivedStats_UsesCurrentModifiersAndOnlyRaisesForActualChange()
        {
            var levels = new PlayerStatLevels(3, 2, 1, 1, 1);
            var modifier = new MutableDamageModifier(1f);
            var state = new PlayerStatsState(
                CreateConfiguration(),
                levels,
                new IPlayerDerivedStatsModifier[] { modifier });
            var eventCount = 0;
            var lastChange = default(PlayerDerivedStatsChange);
            state.DerivedStatsChanged += change =>
            {
                eventCount++;
                lastChange = change;
            };

            Assert.IsFalse(state.RecalculateDerivedStats());
            Assert.That(eventCount, Is.Zero);

            modifier.DamageBonus = 4f;
            Assert.IsTrue(state.RecalculateDerivedStats());
            Assert.That(state.DerivedStats.BaseDamage, Is.EqualTo(20f).Within(0.0001f));
            AssertLevelsEqual(levels, state.BaseLevels);
            Assert.That(eventCount, Is.EqualTo(1));
            Assert.That(lastChange.Reason, Is.EqualTo(PlayerDerivedStatsChangeReason.Recalculated));

            Assert.IsFalse(state.RecalculateDerivedStats());
            Assert.That(eventCount, Is.EqualTo(1));
        }

        [Test]
        public void SetLevel_DerivedEventOnlyRaisesWhenCappedValuesActuallyChange()
        {
            var state = new PlayerStatsState(
                CreateConfiguration(),
                new PlayerStatLevels(1, 1, 1, 1, 1));
            var eventCount = 0;
            var lastChange = default(PlayerDerivedStatsChange);
            state.DerivedStatsChanged += change =>
            {
                eventCount++;
                lastChange = change;
            };

            Assert.IsTrue(state.SetLevel(PlayerStatType.Mobility, 5));
            Assert.That(eventCount, Is.EqualTo(1));
            Assert.That(lastChange.Reason, Is.EqualTo(PlayerDerivedStatsChangeReason.LevelChanged));
            Assert.That(state.DerivedStats.MoveSpeed, Is.EqualTo(6f).Within(0.0001f));

            Assert.IsTrue(state.SetLevel(PlayerStatType.Mobility, 6));
            Assert.That(state.BaseLevels.Mobility, Is.EqualTo(6));
            Assert.That(state.DerivedStats.MoveSpeed, Is.EqualTo(6f).Within(0.0001f));
            Assert.That(eventCount, Is.EqualTo(1));
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

        private static void AssertLevelsEqual(PlayerStatLevels expected, PlayerStatLevels actual)
        {
            Assert.That(actual.Power, Is.EqualTo(expected.Power));
            Assert.That(actual.Hull, Is.EqualTo(expected.Hull));
            Assert.That(actual.Armor, Is.EqualTo(expected.Armor));
            Assert.That(actual.Flux, Is.EqualTo(expected.Flux));
            Assert.That(actual.Mobility, Is.EqualTo(expected.Mobility));
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

        private sealed class ConfigurableModifier : IPlayerDerivedStatsModifier
        {
            private readonly float _damageBonus;
            private readonly float _attackInterval;
            private readonly float _moveSpeed;

            public ConfigurableModifier(float damageBonus, float attackInterval, float moveSpeed)
            {
                _damageBonus = damageBonus;
                _attackInterval = attackInterval;
                _moveSpeed = moveSpeed;
            }

            public PlayerDerivedStats Apply(in PlayerDerivedStats currentValues)
            {
                return new PlayerDerivedStats(
                    currentValues.BaseDamage + _damageBonus,
                    currentValues.MaxHp,
                    currentValues.ArmorValue,
                    _attackInterval,
                    _moveSpeed);
            }
        }

        private sealed class MutableDamageModifier : IPlayerDerivedStatsModifier
        {
            public MutableDamageModifier(float damageBonus)
            {
                DamageBonus = damageBonus;
            }

            public float DamageBonus { get; set; }

            public PlayerDerivedStats Apply(in PlayerDerivedStats currentValues)
            {
                return new PlayerDerivedStats(
                    currentValues.BaseDamage + DamageBonus,
                    currentValues.MaxHp,
                    currentValues.ArmorValue,
                    currentValues.AttackInterval,
                    currentValues.MoveSpeed);
            }
        }
    }
}
