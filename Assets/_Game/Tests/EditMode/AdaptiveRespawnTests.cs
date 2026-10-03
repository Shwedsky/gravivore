using System;
using Gravivore.Gameplay.Enemies;
using NUnit.Framework;

namespace Gravivore.Tests.EditMode
{
    public sealed class AdaptiveRespawnTests
    {
        private static AdaptiveRespawnState CreateState() =>
            new AdaptiveRespawnState(new AdaptiveRespawnPolicy(4, 8f, 4, 60f, 30f));

        [Test]
        public void RepeatedKills_RaiseDelayInStepsAndSaturate()
        {
            var state = CreateState();
            for (var kills = 1; kills <= 100; kills++)
            {
                state.RegisterKill(kills);
                Assert.That(state.PenaltySteps, Is.EqualTo(Math.Min(kills / 4, 4)));
                Assert.That(state.AdditionalDelay, Is.EqualTo(Math.Min(kills / 4, 4) * 8f));
                Assert.That(state.KillPressure, Is.LessThanOrEqualTo(16));
            }
        }

        [Test]
        public void IdleGraceAndRecovery_RestoreOneStepPerInterval()
        {
            var state = CreateState();
            for (var i = 0; i < 16; i++) state.RegisterKill(0d);
            state.AdvanceTo(89.999d);
            Assert.That(state.PenaltySteps, Is.EqualTo(4));
            state.AdvanceTo(90d);
            Assert.That(state.PenaltySteps, Is.EqualTo(3));
            state.AdvanceTo(120d);
            Assert.That(state.PenaltySteps, Is.EqualTo(2));
            state.AdvanceTo(150d);
            Assert.That(state.PenaltySteps, Is.EqualTo(1));
            state.AdvanceTo(180d);
            Assert.That(state.KillPressure, Is.Zero);
            Assert.That(state.AdditionalDelay, Is.Zero);
        }

        [Test]
        public void NewKill_RestartsGraceEvenAtCap()
        {
            var state = CreateState();
            for (var i = 0; i < 16; i++) state.RegisterKill(0d);
            state.RegisterKill(89d);
            state.AdvanceTo(178.999d);
            Assert.That(state.PenaltySteps, Is.EqualTo(4));
            state.AdvanceTo(179d);
            Assert.That(state.PenaltySteps, Is.EqualTo(3));
        }

        [Test]
        public void LongIdleAndPartialPressure_DoNotCarryOldChainIntoNewVisit()
        {
            var state = CreateState();
            for (var i = 0; i < 3; i++) state.RegisterKill(0d);
            state.RegisterKill(500d);
            Assert.That(state.KillPressure, Is.EqualTo(1));
            Assert.That(state.AdditionalDelay, Is.Zero);
        }

        [Test]
        public void Recovery_IsIndependentOfTickPartition()
        {
            var coarse = CreateState();
            var fine = CreateState();
            for (var i = 0; i < 13; i++) { coarse.RegisterKill(0d); fine.RegisterKill(0d); }
            coarse.AdvanceTo(149d);
            for (var t = 1; t <= 149; t++) fine.AdvanceTo(t);
            Assert.That(fine.KillPressure, Is.EqualTo(coarse.KillPressure));
            Assert.That(fine.AdditionalDelay, Is.EqualTo(coarse.AdditionalDelay));
            coarse.RegisterKill(150d);
            fine.RegisterKill(150d);
            Assert.That(fine.KillPressure, Is.EqualTo(coarse.KillPressure));
        }

        [Test]
        public void SeparateSpots_HaveIndependentPressure()
        {
            var farmed = CreateState();
            var untouched = CreateState();
            for (var i = 0; i < 8; i++) farmed.RegisterKill(i);
            untouched.AdvanceTo(8d);
            Assert.That(farmed.AdditionalDelay, Is.EqualTo(16f));
            Assert.That(untouched.AdditionalDelay, Is.Zero);
        }

        [Test]
        public void LegacyPolicy_LeavesRespawnBaselineUnchanged()
        {
            var state = new AdaptiveRespawnState(default);
            for (var i = 0; i < 100; i++) state.RegisterKill(i);
            Assert.That(state.KillPressure, Is.Zero);
            Assert.That(state.AdditionalDelay, Is.Zero);
        }

        [Test]
        public void InvalidPolicy_RejectsBadCountsAndRecoveryInterval()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AdaptiveRespawnPolicy(0, 8, 4, 60, 30));
            Assert.Throws<ArgumentOutOfRangeException>(() => new AdaptiveRespawnPolicy(4, 8, -1, 60, 30));
            Assert.Throws<ArgumentOutOfRangeException>(() => new AdaptiveRespawnPolicy(int.MaxValue, 8, 4, 60, 30));
            Assert.Throws<ArgumentOutOfRangeException>(() => new AdaptiveRespawnPolicy(4, 8, 4, 60, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new AdaptiveRespawnPolicy(4, float.MaxValue, 4, 60, 30));
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void InvalidSeconds_AreRejected(float value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AdaptiveRespawnPolicy(4, value, 4, 60, 30));
            Assert.Throws<ArgumentOutOfRangeException>(() => new AdaptiveRespawnPolicy(4, 8, 4, value, 30));
            Assert.Throws<ArgumentOutOfRangeException>(() => new AdaptiveRespawnPolicy(4, 8, 4, 60, value));
        }

        [Test]
        public void InvalidOrReversedTime_IsRejectedWithoutChangingPressure()
        {
            var state = CreateState();
            state.RegisterKill(10d);
            Assert.Throws<ArgumentOutOfRangeException>(() => state.AdvanceTo(9d));
            Assert.Throws<ArgumentOutOfRangeException>(() => state.RegisterKill(double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => state.AdvanceTo(double.PositiveInfinity));
            Assert.That(state.KillPressure, Is.EqualTo(1));
        }
    }
}
