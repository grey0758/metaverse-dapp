using MetaverseGame.Input;
using NUnit.Framework;
using UnityEngine;

namespace MetaverseGame.Tests
{
    public sealed class MobileInputTests
    {
        [Test]
        public void AppliesJoystickDeadzoneAndRemapsTheActiveRange()
        {
            Assert.That(
                MobileInputMath.SanitizeMoveInput(new Vector2(0.05f, 0f), 0.12f),
                Is.EqualTo(Vector2.zero));

            Vector2 remapped = MobileInputMath.SanitizeMoveInput(
                new Vector2(0.56f, 0f),
                0.12f);
            Assert.That(remapped.x, Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void ClampsDiagonalTouchInputToUnitMagnitude()
        {
            Vector2 input = MobileInputMath.SanitizeMoveInput(
                new Vector2(3f, 4f),
                0.12f);

            Assert.That(input.magnitude, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void RejectsNonFiniteTouchInput()
        {
            Assert.That(
                MobileInputMath.SanitizeMoveInput(
                    new Vector2(float.PositiveInfinity, 0f),
                    0.12f),
                Is.EqualTo(Vector2.zero));
            Assert.That(
                MobileInputMath.SanitizeMoveInput(
                    new Vector2(float.NaN, 0f),
                    0.12f),
                Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void TouchRouterQueuesEachContextActionOnce()
        {
            GameObject gameObject = new("MobileInputRouter");
            MobileInputRouter router = gameObject.AddComponent<MobileInputRouter>();
            try
            {
                router.PressInteract();
                Assert.That(router.ConsumeInteractPressed(), Is.True);
                Assert.That(router.ConsumeInteractPressed(), Is.False);

                router.SetTouchMoveInput(new Vector2(0f, 1f));
                Assert.That(router.MoveInput.y, Is.EqualTo(1f).Within(0.0001f));
                router.ClearTouchMoveInput();
                Assert.That(router.HasActiveTouchMove, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
