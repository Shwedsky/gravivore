using System;
using Gravivore.Presentation.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class BoundedPullTrailTests
    {
        [TestCase(.2f)] [TestCase(1.2f)] [TestCase(8f)]
        public void CosmeticPullConvergesMonotonicallyWithinTimeAndDistanceCaps(float displacement)
        {
            var trail = new BoundedPullTrail(); trail.Begin(new Vector3(displacement,0,displacement),.18f,1.35f);
            var last = trail.Offset.magnitude; Assert.That(last,Is.LessThanOrEqualTo(1.35001f));
            for (var i = 0; i < 10; i++)
            {
                trail.Tick(.02f); Assert.That(trail.Offset.magnitude,Is.LessThanOrEqualTo(last)); last = trail.Offset.magnitude;
            }
            Assert.IsFalse(trail.Active); Assert.That(trail.Offset,Is.EqualTo(Vector3.zero));
        }
        [Test] public void InvalidOrLongDesynchronizationIsRejected()
        {
            var trail = new BoundedPullTrail();
            Assert.Throws<ArgumentOutOfRangeException>(() => trail.Begin(Vector3.one,.3f,1));
            Assert.Throws<ArgumentOutOfRangeException>(() => trail.Begin(Vector3.one,float.NaN,1));
            Assert.Throws<ArgumentOutOfRangeException>(() => trail.Begin(Vector3.one,.18f,float.PositiveInfinity));
            Assert.Throws<ArgumentOutOfRangeException>(() => trail.Begin(new Vector3(float.NaN,0,0),.18f,1));
            Assert.Throws<ArgumentOutOfRangeException>(() => trail.Tick(float.NaN));
        }
    }
}
