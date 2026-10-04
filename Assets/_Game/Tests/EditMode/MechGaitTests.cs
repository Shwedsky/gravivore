using Gravivore.Presentation.Player;
using NUnit.Framework;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class MechGaitTests
    {
        [TestCase(.4f)]
        [TestCase(1.7f)]
        [TestCase(4.8f)]
        public void AlternatingLegsHaveOppositeSwingAndOnlyOneFootInLift(float phase)
        {
            var left = new MechanicalStep(phase);
            var right = new MechanicalStep(phase + Mathf.PI);
            Assert.That(left.Swing, Is.EqualTo(-right.Swing).Within(.00001f));
            Assert.That(left.Lift * right.Lift, Is.EqualTo(0).Within(.00001f));
            Assert.That(left.Lift + right.Lift, Is.InRange(0f, 1f));
        }

        [Test]
        public void CompleteStrideReturnsToSamePlantPhaseWithoutAccumulatedOffsets()
        {
            var first = new MechanicalStep(.8f);
            var next = new MechanicalStep(.8f + 2 * Mathf.PI);
            Assert.That(next.Swing, Is.EqualTo(first.Swing).Within(.00001f));
            Assert.That(next.Lift, Is.EqualTo(first.Lift).Within(.00001f));
        }
    }
}
