using MetaverseGame.Input;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MetaverseGame.Bootstrap
{
    /// <summary>
    /// One-finger virtual stick. It reports a normalized command to the shared
    /// input router and always returns to neutral when the finger is released.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VirtualJoystick : MonoBehaviour,
        IPointerDownHandler,
        IDragHandler,
        IPointerUpHandler
    {
        private const int NoPointer = int.MinValue;

        private RectTransform rectTransform;
        private RectTransform handle;
        private MobileInputRouter inputRouter;
        private float radius = 86f;
        private float deadzone = 0.12f;
        private int activePointerId = NoPointer;

        public Vector2 Value { get; private set; }

        private void Awake()
        {
            rectTransform = transform as RectTransform;
        }

        public void Configure(
            MobileInputRouter router,
            RectTransform handleTransform,
            float movementRadius,
            float inputDeadzone)
        {
            inputRouter = router;
            handle = handleTransform;
            radius = Mathf.Max(1f, movementRadius);
            deadzone = Mathf.Clamp(inputDeadzone, 0f, 0.9f);
            ResetStick();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (activePointerId != NoPointer)
            {
                return;
            }

            activePointerId = eventData.pointerId;
            UpdateFromScreenPosition(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId == activePointerId)
            {
                UpdateFromScreenPosition(eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
            {
                return;
            }

            activePointerId = NoPointer;
            ResetStick();
        }

        private void UpdateFromScreenPosition(PointerEventData eventData)
        {
            if (rectTransform == null)
            {
                rectTransform = transform as RectTransform;
            }

            Camera eventCamera = eventData.pressEventCamera;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform,
                    eventData.position,
                    eventCamera,
                    out Vector2 localPoint))
            {
                return;
            }

            Vector2 raw = Vector2.ClampMagnitude(localPoint / radius, 1f);
            Value = MobileInputMath.SanitizeMoveInput(raw, deadzone);
            if (handle != null)
            {
                handle.anchoredPosition = raw * radius;
            }
            inputRouter?.SetTouchMoveInput(raw);
        }

        private void ResetStick()
        {
            Value = Vector2.zero;
            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }
            inputRouter?.ClearTouchMoveInput();
        }

        private void OnDisable()
        {
            activePointerId = NoPointer;
            ResetStick();
        }
    }
}
