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
        private RectTransform baseTransform;
        private RectTransform handle;
        private CanvasGroup visualGroup;
        private MobileInputRouter inputRouter;
        private float radius = 82f;
        private float visualRadius = 110f;
        private float edgeMargin = 12f;
        private float deadzone = 0.12f;
        private int activePointerId = NoPointer;
        private Vector2 homeCenter;
        private Vector2 activeCenter;

        public Vector2 Value { get; private set; }
        public bool IsActive => activePointerId != NoPointer;

        private void Awake()
        {
            rectTransform = transform as RectTransform;
        }

        public void Configure(
            MobileInputRouter router,
            RectTransform joystickBase,
            RectTransform handleTransform,
            float movementRadius,
            float baseVisualRadius,
            float inputDeadzone,
            CanvasGroup joystickVisualGroup)
        {
            inputRouter = router;
            baseTransform = joystickBase;
            handle = handleTransform;
            radius = Mathf.Max(1f, movementRadius);
            visualRadius = Mathf.Max(radius, baseVisualRadius);
            deadzone = Mathf.Clamp(inputDeadzone, 0f, 0.9f);
            visualGroup = joystickVisualGroup;

            Canvas.ForceUpdateCanvases();
            homeCenter = ResolveHomeCenter();
            ResetStick();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (activePointerId != NoPointer)
            {
                return;
            }

            activePointerId = eventData.pointerId;
            if (!TryGetLocalPoint(eventData, out Vector2 localPoint))
            {
                activePointerId = NoPointer;
                return;
            }

            activeCenter = MobileInputMath.ClampFloatingCenter(
                localPoint,
                rectTransform.rect,
                visualRadius,
                edgeMargin);
            if (baseTransform != null)
            {
                baseTransform.anchoredPosition = activeCenter;
            }
            SetActiveVisual(true);
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

            if (!TryGetLocalPoint(eventData, out Vector2 localPoint))
            {
                return;
            }

            Vector2 raw = MobileInputMath.NormalizeJoystickDelta(
                localPoint,
                activeCenter,
                radius);
            Value = MobileInputMath.SanitizeMoveInput(raw, deadzone);
            if (handle != null)
            {
                handle.anchoredPosition = raw * radius;
                float handleScale = Mathf.Lerp(0.92f, 1.08f, raw.magnitude);
                handle.localScale = new Vector3(handleScale, handleScale, 1f);
            }
            inputRouter?.SetTouchMoveInput(raw);
        }

        private bool TryGetLocalPoint(
            PointerEventData eventData,
            out Vector2 localPoint)
        {
            if (rectTransform == null)
            {
                rectTransform = transform as RectTransform;
            }

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint);
        }

        private Vector2 ResolveHomeCenter()
        {
            Rect bounds = rectTransform != null
                ? rectTransform.rect
                : new Rect(-320f, -220f, 640f, 440f);
            Vector2 requested = new(
                Mathf.Lerp(bounds.xMin, bounds.xMax, 0.24f),
                Mathf.Lerp(bounds.yMin, bounds.yMax, 0.34f));
            return MobileInputMath.ClampFloatingCenter(
                requested,
                bounds,
                visualRadius,
                edgeMargin);
        }

        private void SetActiveVisual(bool active)
        {
            if (visualGroup != null)
            {
                visualGroup.alpha = active ? 1f : 0.72f;
            }
        }

        private void ResetStick()
        {
            Value = Vector2.zero;
            activeCenter = homeCenter;
            if (baseTransform != null)
            {
                baseTransform.anchoredPosition = homeCenter;
            }
            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
                handle.localScale = Vector3.one;
            }
            SetActiveVisual(false);
            inputRouter?.ClearTouchMoveInput();
        }

        private void OnDisable()
        {
            activePointerId = NoPointer;
            ResetStick();
        }
    }
}
