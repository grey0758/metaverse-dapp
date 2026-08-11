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
        IPointerExitHandler,
        IPointerClickHandler
    {
        private Image image;
        private MobileInputRouter inputRouter;
        private Color idleColor;
        private Color pressedColor;

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
            ApplyVisualState(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ApplyVisualState(false);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ApplyVisualState(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            inputRouter?.PressInteract();
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
            ApplyVisualState(false);
        }
    }
}
