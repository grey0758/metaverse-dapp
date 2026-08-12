using MetaverseGame.Input;
using MetaverseGame.Bootstrap;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

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

        [Test]
        public void FloatingJoystickCenterStaysInsideItsTouchZone()
        {
            Rect bounds = new(-320f, -220f, 640f, 440f);

            Vector2 lowerLeft = MobileInputMath.ClampFloatingCenter(
                new Vector2(-900f, -900f),
                bounds,
                100f,
                20f);
            Vector2 upperRight = MobileInputMath.ClampFloatingCenter(
                new Vector2(900f, 900f),
                bounds,
                100f,
                20f);

            Assert.That(lowerLeft, Is.EqualTo(new Vector2(-200f, -100f)));
            Assert.That(upperRight, Is.EqualTo(new Vector2(200f, 100f)));
        }

        [Test]
        public void FloatingJoystickNormalizesAndClampsPointerTravel()
        {
            Assert.That(
                MobileInputMath.NormalizeJoystickDelta(
                    new Vector2(55f, 40f),
                    new Vector2(15f, 40f),
                    80f),
                Is.EqualTo(new Vector2(0.5f, 0f)));
            Assert.That(
                MobileInputMath.NormalizeJoystickDelta(
                    new Vector2(300f, 0f),
                    Vector2.zero,
                    80f).magnitude,
                Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void ActionButtonQueuesOnPressWithoutDuplicatePointers()
        {
            GameObject routerObject = new("MobileInputRouter");
            GameObject buttonObject = new("MobileActionButton");
            MobileInputRouter router = routerObject.AddComponent<MobileInputRouter>();
            MobileActionButton button = buttonObject.AddComponent<MobileActionButton>();
            try
            {
                button.Configure(router, null, Color.white, Color.yellow);
                PointerEventData primary = new(null) { pointerId = 11 };
                PointerEventData secondary = new(null) { pointerId = 12 };

                button.OnPointerDown(primary);
                button.OnPointerDown(secondary);
                Assert.That(router.ConsumeInteractPressed(), Is.True);
                Assert.That(router.ConsumeInteractPressed(), Is.False);

                button.OnPointerUp(primary);
                button.OnPointerDown(secondary);
                Assert.That(router.ConsumeInteractPressed(), Is.True);
                Assert.That(router.ConsumeInteractPressed(), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(buttonObject);
                Object.DestroyImmediate(routerObject);
            }
        }
    }
}
