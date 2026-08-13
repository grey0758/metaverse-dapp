using MetaverseGame.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MetaverseGame.Bootstrap
{
    /// <summary>
    /// One segment of the mobile locked/free camera selector.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MobileCameraModeButton : MonoBehaviour, IPointerClickHandler
    {
        private FollowLocalPlayer cameraController;
        private FollowLocalPlayer.ViewMode mode;
        private Image background;
        private Color activeColor;
        private Color inactiveColor;

        public void Configure(
            FollowLocalPlayer controller,
            FollowLocalPlayer.ViewMode targetMode,
            Image targetBackground,
            Color selectedColor,
            Color unselectedColor)
        {
            cameraController = controller;
            mode = targetMode;
            background = targetBackground;
            activeColor = selectedColor;
            inactiveColor = unselectedColor;
            RefreshVisual();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ResolveController();
            cameraController?.SetViewMode(mode);
            RefreshVisual();
            if (transform.parent == null)
            {
                return;
            }

            MobileCameraModeButton[] siblings =
                transform.parent.GetComponentsInChildren<MobileCameraModeButton>(true);
            foreach (MobileCameraModeButton sibling in siblings)
            {
                sibling.RefreshVisual();
            }
        }

        private void Update()
        {
            RefreshVisual();
        }

        private void RefreshVisual()
        {
            ResolveController();
            if (background != null && cameraController != null)
            {
                background.color = cameraController.CurrentViewMode == mode
                    ? activeColor
                    : inactiveColor;
            }
        }

        private void ResolveController()
        {
            cameraController ??= FindFirstObjectByType<FollowLocalPlayer>();
        }
    }
}
