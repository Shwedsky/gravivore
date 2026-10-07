using System;
using Gravivore.Gameplay.Enemies;
using NUnit.Framework;

namespace Gravivore.Tests.EditMode
{
    public sealed class WaveRespawnTests
    {
        [Test] public void FirstKillOwnsDeadlineAndLaterKillsDoNotResetIt()
        {
            var wave = new WaveRespawnState(120f);
            wave.Tick(30f); wave.RegisterKill();
            Assert.That(wave.RemainingSeconds, Is.EqualTo(120f));
            wave.Tick(42f); wave.RegisterKill(); wave.RegisterKill();
            Assert.That(wave.RemainingSeconds, Is.EqualTo(78f));
            wave.Tick(77.5f);
            Assert.IsFalse(wave.CanRestore(true, false, true));
            wave.Tick(.5f);
            Assert.IsTrue(wave.CanRestore(true, false, true));
        }
        [Test] public void EligibilityWaitsForDistanceCombatAndEntireGroupCapacity()
        {
            var wave = new WaveRespawnState(120f); wave.RegisterKill(); wave.Tick(120f);
            Assert.That(wave.Read(false), Is.EqualTo(SpawnSpotAvailability.Ready));
            Assert.IsFalse(wave.CanRestore(false, false, true));
            Assert.IsFalse(wave.CanRestore(true, true, true));
            Assert.IsFalse(wave.CanRestore(true, false, false));
            Assert.IsTrue(wave.CanRestore(true, false, true));
            wave.CompleteRestore();
            Assert.That(wave.Read(false), Is.EqualTo(SpawnSpotAvailability.Available));
            Assert.That(wave.Read(true), Is.EqualTo(SpawnSpotAvailability.Active));
            wave.RegisterKill(); Assert.That(wave.RemainingSeconds, Is.EqualTo(120f));
        }
        [TestCase(119f)] [TestCase(float.NaN)] [TestCase(float.PositiveInfinity)]
        public void InvalidCooldownCannotUndercutMinimum(float seconds) =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new WaveRespawnState(seconds));
    }
}
