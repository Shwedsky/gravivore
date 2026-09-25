using System;
using Gravivore.Core.Stats;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Player;
using NUnit.Framework;

namespace Gravivore.Tests.EditMode
{
    public sealed class CombatHealthTests
    {
        [Test]
        public void PhysicalDamage_UsesDocumentedArmorFormula()
        {
            Assert.That(DamageResolver.ResolvePhysicalDamage(25f, 0f), Is.EqualTo(25f));
            Assert.That(DamageResolver.ResolvePhysicalDamage(100f, 100f), Is.EqualTo(50f));
            Assert.That(DamageResolver.ResolvePhysicalDamage(0.1f, 1000f), Is.EqualTo(1f));
        }

        [Test]
        public void PhysicalDamage_RejectsInvalidInputs()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DamageResolver.ResolvePhysicalDamage(-1f, 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => DamageResolver.ResolvePhysicalDamage(float.NaN, 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => DamageResolver.ResolvePhysicalDamage(1f, float.PositiveInfinity));
            Assert.Throws<ArgumentOutOfRangeException>(() => DamageResolver.ResolvePhysicalDamage(1f, -1f));
        }

        [Test]
        public void Health_ClampsToZeroAndSignalsDeathOnceForRepeatedLethalDamage()
        {
            var health = new HealthState();
            health.Reset(10f);
            var firstSubscriberCalls = 0;
            var secondSubscriberCalls = 0;
            health.Died += () => firstSubscriberCalls++;
            health.Died += () => secondSubscriberCalls++;

            var lethal = health.ApplyDamage(new DamageRequest(100f, DamageType.Physical), 0f);
            var repeated = health.ApplyDamage(new DamageRequest(100f, DamageType.Physical), 0f);

            Assert.That(lethal.AppliedDamage, Is.EqualTo(10f));
            Assert.IsTrue(lethal.WasLethal);
            Assert.That(health.CurrentHitPoints, Is.Zero);
            Assert.That(firstSubscriberCalls, Is.EqualTo(1));
            Assert.That(secondSubscriberCalls, Is.EqualTo(1));
            Assert.That(repeated.AppliedDamage, Is.Zero);
            Assert.IsFalse(repeated.WasLethal);
            Assert.Throws<ArgumentOutOfRangeException>(
                () => health.ApplyDamage(new DamageRequest(1f, DamageType.Physical), float.NaN));
        }

        [Test]
        public void Health_ResetAndHealRestoreMaximumAndStartNewLife()
        {
            var health = new HealthState();
            health.Reset(20f);
            health.ApplyDamage(new DamageRequest(7f, DamageType.Physical), 0f);
            health.HealToFull();
            Assert.That(health.CurrentHitPoints, Is.EqualTo(20f));

            health.ApplyDamage(new DamageRequest(100f, DamageType.Physical), 0f);
            health.Reset(35f);
            Assert.That(health.CurrentHitPoints, Is.EqualTo(35f));
            Assert.That(health.MaximumHitPoints, Is.EqualTo(35f));
            Assert.IsTrue(health.IsAlive);
        }

        [Test]
        public void PlayerHealth_UsesDerivedStatsAndRespawnPreservesBaseLevels()
        {
            var configuration = CreateStatsConfiguration();
            var levels = new PlayerStatLevels(2, 3, 4, 2, 2);
            var stats = new PlayerStatsState(configuration, levels);
            var health = new PlayerHealthRuntime(stats, 1.5f);

            Assert.That(health.MaximumHitPoints, Is.EqualTo(stats.DerivedStats.MaxHp));
            Assert.That(health.Armor, Is.EqualTo(stats.DerivedStats.ArmorValue));

            var result = health.ApplyDamage(new DamageRequest(10000f, DamageType.Physical));
            Assert.IsTrue(result.WasLethal);
            health.Respawn();

            Assert.That(health.CurrentHitPoints, Is.EqualTo(stats.DerivedStats.MaxHp));
            Assert.That(stats.BaseLevels.Power, Is.EqualTo(levels.Power));
            Assert.That(stats.BaseLevels.Hull, Is.EqualTo(levels.Hull));
            Assert.That(stats.BaseLevels.Armor, Is.EqualTo(levels.Armor));
            Assert.That(stats.BaseLevels.Flux, Is.EqualTo(levels.Flux));
            Assert.That(stats.BaseLevels.Mobility, Is.EqualTo(levels.Mobility));
        }

        [Test]
        public void PlayerHealth_InvulnerabilityBlocksLossUntilWindowExpires()
        {
            var stats = new PlayerStatsState(
                CreateStatsConfiguration(),
                new PlayerStatLevels(1, 1, 1, 1, 1));
            var health = new PlayerHealthRuntime(stats, 1f);
            health.ApplyDamage(new DamageRequest(10000f, DamageType.Physical));
            health.Respawn();

            var blocked = health.ApplyDamage(new DamageRequest(10f, DamageType.Physical));
            Assert.That(blocked.AppliedDamage, Is.Zero);
            Assert.That(health.CurrentHitPoints, Is.EqualTo(health.MaximumHitPoints));

            health.Tick(1f);
            var applied = health.ApplyDamage(new DamageRequest(10f, DamageType.Physical));
            Assert.That(applied.AppliedDamage, Is.GreaterThan(0f));
            Assert.That(health.CurrentHitPoints, Is.LessThan(health.MaximumHitPoints));
        }

        private static PlayerStatsConfiguration CreateStatsConfiguration()
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
    }
}
