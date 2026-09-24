using Gravivore.Core.Input;
using NUnit.Framework;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class MovementInputMathTests
    {
        [Test]
        public void NormalizeDrag_ZeroRadius_ReturnsZero()
        {
            Assert.AreEqual(Vector2.zero, MovementInputMath.NormalizeDrag(Vector2.zero, Vector2.one, 0f));
        }

        [Test]
        public void NormalizeDrag_ClampsMagnitudeAndPreservesDirection()
        {
            var result = MovementInputMath.NormalizeDrag(Vector2.zero, new Vector2(300f, 400f), 100f);

            Assert.That(result.magnitude, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(result.x, Is.EqualTo(0.6f).Within(0.0001f));
            Assert.That(result.y, Is.EqualTo(0.8f).Within(0.0001f));
        }

        [Test]
        public void ApplyRadialDeadZone_InsideDeadZone_ReturnsZero()
        {
            var result = MovementInputMath.ApplyRadialDeadZone(new Vector2(0.05f, 0f), 0.1f);

            Assert.AreEqual(Vector2.zero, result);
        }

        [Test]
        public void ApplyRadialDeadZone_RescalesRemainingAnalogRange()
        {
            var result = MovementInputMath.ApplyRadialDeadZone(new Vector2(0.55f, 0f), 0.1f);

            Assert.That(result.x, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(result.y, Is.Zero.Within(0.0001f));
        }

        [Test]
        public void ApplyRadialDeadZone_OversizedInput_IsNormalized()
        {
            var result = MovementInputMath.ApplyRadialDeadZone(new Vector2(4f, 3f), 0.2f);

            Assert.That(result.magnitude, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(result.x, Is.EqualTo(0.8f).Within(0.0001f));
            Assert.That(result.y, Is.EqualTo(0.6f).Within(0.0001f));
        }
    }
}
