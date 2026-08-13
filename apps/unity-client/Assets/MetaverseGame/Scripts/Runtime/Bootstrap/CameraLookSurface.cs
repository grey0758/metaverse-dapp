using MetaverseGame.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MetaverseGame.Bootstrap
{
    /// <summary>
    /// Owns one right-side touch pointer for free camera orbit input.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CameraLookSurface : MonoBehaviour,
        IPointerDownHandler,
        IDragHandler,
        IPointerUpHandler,
        ICancelHandler
    {
        private const int NoPointer = int.MinValue;

        private FollowLocalPlayer cameraController;
        private int activePointerId = NoPointer;

        public bool IsActive => activePointerId != NoPointer;

        public void Configure(FollowLocalPlayer controller)
        {
            cameraController = controller;
            activePointerId = NoPointer;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            ResolveController();
            if (activePointerId != NoPointer ||
                cameraController == null ||
                !cameraController.IsFreeView)
            {
                return;
            }

            activePointerId = eventData.pointerId;
            eventData.useDragThreshold = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId == activePointerId)
            {
                cameraController?.ApplyLookDelta(eventData.delta);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ReleasePointer(eventData.pointerId);
        }

        public void OnCancel(BaseEventData eventData)
        {
            activePointerId = NoPointer;
        }

        private void ReleasePointer(int pointerId)
        {
            if (pointerId == activePointerId)
            {
                activePointerId = NoPointer;
            }
        }

        private void OnDisable()
        {
            activePointerId = NoPointer;
        }

        private void ResolveController()
        {
            cameraController ??= FindFirstObjectByType<FollowLocalPlayer>();
        }
    }
}
