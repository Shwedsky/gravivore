using Gravivore.Gameplay.Combat;
using NUnit.Framework;

namespace Gravivore.Tests.EditMode
{
    public sealed class TargetingCombatTests
    {
        [Test]
        public void CalculateScore_PrefersCloserTarget()
        {
            var parameters = CreateTargetingParameters(frontBiasWeight: 0f);

            var nearScore = TargetSelector.CalculateScore(2f, 0f, parameters);
            var farScore = TargetSelector.CalculateScore(4f, 0f, parameters);

            Assert.That(nearScore, Is.LessThan(farScore));
        }

        [Test]
        public void CalculateScore_FrontBiasPrefersTargetAheadAtEqualDistance()
        {
            var parameters = CreateTargetingParameters(frontBiasWeight: 1f);

            var frontScore = TargetSelector.CalculateScore(3f, 1f, parameters);
            var rearScore = TargetSelector.CalculateScore(3f, -1f, parameters);

            Assert.That(frontScore, Is.LessThan(rearScore));
        }

        [Test]
        public void TrySelect_KeepsCurrentTargetWhenChallengerImprovementIsBelowHysteresis()
        {
            var candidates = new[]
            {
                new TargetCandidate<string>("current", 3f, 1f, true),
                new TargetCandidate<string>("challenger", 2.5f, 1f, true)
            };

            var found = TargetSelector.TrySelect(
                candidates,
                true,
                "current",
                CreateTargetingParameters(switchAdvantage: 0.15f),
                out var selected);

            Assert.IsTrue(found);
            Assert.That(selected, Is.EqualTo("current"));
        }

        [Test]
        public void TrySelect_SwitchesWhenChallengerClearlyBeatsHysteresis()
        {
            var candidates = new[]
            {
                new TargetCandidate<string>("current", 4f, 1f, true),
                new TargetCandidate<string>("challenger", 1f, 1f, true)
            };

            var found = TargetSelector.TrySelect(
                candidates,
                true,
                "current",
                CreateTargetingParameters(switchAdvantage: 0.15f),
                out var selected);

            Assert.IsTrue(found);
            Assert.That(selected, Is.EqualTo("challenger"));
        }

        [Test]
        public void TrySelect_CurrentTargetUsesReleaseRadiusButNewTargetUsesAcquisitionRadius()
        {
            var candidates = new[]
            {
                new TargetCandidate<string>("current", 5.5f, 1f, true),
                new TargetCandidate<string>("outside", 5.2f, 1f, true)
            };
            var parameters = CreateTargetingParameters();

            Assert.IsTrue(TargetSelector.TrySelect(
                candidates,
                true,
                "current",
                parameters,
                out var retained));
            Assert.That(retained, Is.EqualTo("current"));

            Assert.IsFalse(TargetSelector.TrySelect(
                candidates,
                false,
                null,
                parameters,
                out _));
        }

        [Test]
        public void TrySelect_ReleasesCurrentOutsideReleaseRadiusAndAcquiresValidCandidate()
        {
            var candidates = new[]
            {
                new TargetCandidate<string>("current", 6.1f, 1f, true),
                new TargetCandidate<string>("replacement", 4f, 1f, true)
            };

            var found = TargetSelector.TrySelect(
                candidates,
                true,
                "current",
                CreateTargetingParameters(),
                out var selected);

            Assert.IsTrue(found);
            Assert.That(selected, Is.EqualTo("replacement"));
        }

        [Test]
        public void TrySelect_RejectsInvalidOrDeadCandidate()
        {
            var candidates = new[]
            {
                new TargetCandidate<string>("dead", 1f, 1f, false),
                new TargetCandidate<string>("alive", 3f, 0f, true)
            };

            var found = TargetSelector.TrySelect(
                candidates,
                false,
                null,
                CreateTargetingParameters(),
                out var selected);

            Assert.IsTrue(found);
            Assert.That(selected, Is.EqualTo("alive"));
        }

        [Test]
        public void AttackCadence_AttacksImmediatelyThenWaitsForInterval()
        {
            var cadence = new AttackCadenceTimer();

            Assert.IsTrue(cadence.Advance(0f, true, 1f));
            Assert.IsFalse(cadence.Advance(0.4f, true, 1f));
            Assert.IsFalse(cadence.Advance(0.59f, true, 1f));
            Assert.IsTrue(cadence.Advance(0.02f, true, 1f));
        }

        [Test]
        public void AttackCadence_NoTargetResetsCooldownForImmediateFutureAttack()
        {
            var cadence = new AttackCadenceTimer();

            Assert.IsTrue(cadence.Advance(0f, true, 1f));
            Assert.IsFalse(cadence.Advance(0.2f, false, 1f));
            Assert.That(cadence.RemainingCooldown, Is.Zero);
            Assert.IsTrue(cadence.Advance(0f, true, 1f));
        }

        [Test]
        public void DisplacementPolicy_StandardReceivesFullPull()
        {
            var policy = new DisplacementPolicy(0.35f);

            var distance = policy.CalculatePullDistance(5f, 1f, DisplacementClass.Standard);

            Assert.That(distance, Is.EqualTo(4f).Within(0.0001f));
        }

        [Test]
        public void DisplacementPolicy_EliteReceivesConfiguredReducedPull()
        {
            var policy = new DisplacementPolicy(0.35f);

            var distance = policy.CalculatePullDistance(5f, 1f, DisplacementClass.Elite);

            Assert.That(distance, Is.EqualTo(1.4f).Within(0.0001f));
        }

        [Test]
        public void DisplacementPolicy_BossIsImmune()
        {
            var policy = new DisplacementPolicy(0.35f);

            var distance = policy.CalculatePullDistance(5f, 1f, DisplacementClass.Boss);

            Assert.That(distance, Is.Zero);
        }

        [Test]
        public void SafePullMath_StopsBeforeHardBlocker()
        {
            var allowed = SafePullMath.CalculateAllowedTravelDistance(5f, true, 3f, 0.2f);

            Assert.That(allowed, Is.EqualTo(2.8f).Within(0.0001f));
        }

        [Test]
        public void SafePullMath_WithoutBlockerAllowsFullRequestedTravel()
        {
            var allowed = SafePullMath.CalculateAllowedTravelDistance(5f, false, 0f, 0.2f);

            Assert.That(allowed, Is.EqualTo(5f).Within(0.0001f));
        }

        [Test]
        public void SafePullMath_BlockerInsideClearancePreventsMovement()
        {
            var allowed = SafePullMath.CalculateAllowedTravelDistance(5f, true, 0.1f, 0.2f);

            Assert.That(allowed, Is.Zero);
        }

        private static TargetingParameters CreateTargetingParameters(
            float frontBiasWeight = 0.35f,
            float switchAdvantage = 0.15f)
        {
            return new TargetingParameters(
                5f,
                6f,
                1f,
                frontBiasWeight,
                switchAdvantage);
        }
    }
}
