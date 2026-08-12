using MetaverseGame.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MetaverseGame.Bootstrap
{
    /// <summary>
    /// Touch action surface. It deliberately only queues a context command;
    /// range, line-of-sight, cooldown, and authority remain on the server.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MobileActionButton : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerExitHandler
    {
        private const int NoPointer = int.MinValue;

        private Image image;
        private MobileInputRouter inputRouter;
        private Color idleColor;
        private Color pressedColor;
        private int activePointerId = NoPointer;

        public void Configure(
            MobileInputRouter router,
            Image targetImage,
            Color normalColor,
            Color activeColor)
        {
            inputRouter = router;
            image = targetImage;
            idleColor = normalColor;
            pressedColor = activeColor;
            ApplyVisualState(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (activePointerId != NoPointer)
            {
                return;
            }

            activePointerId = eventData.pointerId;
            ApplyVisualState(true);
            inputRouter?.PressInteract();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
            {
                return;
            }

            activePointerId = NoPointer;
            ApplyVisualState(false);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventData.pointerId == activePointerId)
            {
                activePointerId = NoPointer;
                ApplyVisualState(false);
            }
        }

        private void ApplyVisualState(bool pressed)
        {
            if (image != null)
            {
                image.color = pressed ? pressedColor : idleColor;
            }

            transform.localScale = pressed ? new Vector3(0.94f, 0.94f, 1f) : Vector3.one;
        }

        private void OnDisable()
        {
            activePointerId = NoPointer;
            ApplyVisualState(false);
        }
    }
}
