using System.Collections.Generic;
using MetaverseGame.Gameplay;
using MetaverseGame.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace MetaverseGame.Bootstrap
{
    /// <summary>
    /// Builds the mobile HUD controls at runtime so the same committed scene
    /// scales across Android and iOS aspect ratios and respects display cutouts.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MobileInputRouter))]
    public sealed class MobileTouchControls : MonoBehaviour
    {
        private static readonly Color Cyan = new(0.19f, 0.91f, 1f, 1f);
        private static readonly Color DarkPanel = new(0.018f, 0.045f, 0.085f, 0.78f);
        private static readonly Color ActionIdle = new(1f, 0.31f, 0.22f, 0.9f);
        private static readonly Color ActionPressed = new(1f, 0.7f, 0.2f, 1f);
        private static readonly Color ViewActive = new(0.19f, 0.72f, 0.82f, 0.95f);
        private static readonly Color ViewInactive = new(0.025f, 0.06f, 0.10f, 0.92f);
        private static readonly Color ViewSurface = new(0.19f, 0.91f, 1f, 0.07f);

        [SerializeField] private Vector2 referenceResolution = new(1280f, 720f);
        [SerializeField, Range(0f, 0.45f)] private float joystickDeadzone = 0.12f;

        private readonly List<Object> generatedAssets = new();
        private RectTransform safeAreaRoot;
        private GameObject canvasObject;
        private GameObject createdEventSystem;
        private Rect lastSafeArea = new(-1f, -1f, -1f, -1f);
        private int lastScreenWidth = -1;
        private int lastScreenHeight = -1;

        private void Awake()
        {
#if UNITY_SERVER
            enabled = false;
            return;
#else
            MobileInputRouter inputRouter = GetComponent<MobileInputRouter>();
            EnsureInputSystemEventSystem();
            BuildTouchCanvas(inputRouter);
            RefreshSafeArea(true);
#endif
        }

        private void Update()
        {
            RefreshSafeArea(false);
        }

        private void EnsureInputSystemEventSystem()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                createdEventSystem = new GameObject(
                    "Mobile Event System",
                    typeof(EventSystem));
                eventSystem = createdEventSystem.GetComponent<EventSystem>();
            }

            InputSystemUIInputModule inputModule =
                eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null)
            {
                inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                inputModule.AssignDefaultActions();
            }

            BaseInputModule[] modules = eventSystem.GetComponents<BaseInputModule>();
            foreach (BaseInputModule module in modules)
            {
                module.enabled = module == inputModule;
            }
        }

        private void BuildTouchCanvas(MobileInputRouter inputRouter)
        {
            canvasObject = new GameObject(
                "Mobile Touch Controls",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            GameObject safeAreaObject = CreateRectObject("Safe Area", canvasObject.transform);
            safeAreaRoot = safeAreaObject.GetComponent<RectTransform>();
            safeAreaRoot.anchorMin = Vector2.zero;
            safeAreaRoot.anchorMax = Vector2.one;
            safeAreaRoot.offsetMin = Vector2.zero;
            safeAreaRoot.offsetMax = Vector2.zero;

            Sprite circleSprite = CreateCircleSprite(128);
            CreateJoystick(inputRouter, circleSprite);
            CreateLookSurface(circleSprite);
            CreateActionButton(inputRouter, circleSprite);
            CreateCameraModeSelector();
        }

        private void CreateJoystick(MobileInputRouter inputRouter, Sprite circleSprite)
        {
            GameObject zoneObject = CreateRectObject("Floating Joystick Zone", safeAreaRoot);
            RectTransform zoneRect = zoneObject.GetComponent<RectTransform>();
            zoneRect.anchorMin = Vector2.zero;
            zoneRect.anchorMax = new Vector2(0.52f, 0.76f);
            zoneRect.offsetMin = Vector2.zero;
            zoneRect.offsetMax = Vector2.zero;

            Image captureSurface = zoneObject.AddComponent<Image>();
            captureSurface.color = new Color(0f, 0f, 0f, 0.001f);
            captureSurface.raycastTarget = true;

            GameObject joystickObject = CreateRectObject("Move Joystick", zoneRect);
            RectTransform joystickRect = joystickObject.GetComponent<RectTransform>();
            AnchorAt(joystickRect, new Vector2(0.5f, 0.5f), Vector2.zero);
            joystickRect.sizeDelta = new Vector2(220f, 220f);
            CanvasGroup visualGroup = joystickObject.AddComponent<CanvasGroup>();

            Image background = joystickObject.AddComponent<Image>();
            background.sprite = circleSprite;
            background.color = DarkPanel;
            background.raycastTarget = true;
            AddOutline(background, new Color(Cyan.r, Cyan.g, Cyan.b, 0.78f), 3f);

            GameObject ringObject = CreateRectObject("Movement Range", joystickRect);
            RectTransform ringRect = ringObject.GetComponent<RectTransform>();
            StretchCentered(ringRect, new Vector2(162f, 162f));
            Image ring = ringObject.AddComponent<Image>();
            ring.sprite = circleSprite;
            ring.color = new Color(Cyan.r, Cyan.g, Cyan.b, 0.09f);
            ring.raycastTarget = false;
            AddOutline(ring, new Color(Cyan.r, Cyan.g, Cyan.b, 0.45f), 2f);

            GameObject handleObject = CreateRectObject("Joystick Handle", joystickRect);
            RectTransform handleRect = handleObject.GetComponent<RectTransform>();
            StretchCentered(handleRect, new Vector2(86f, 86f));
            Image handle = handleObject.AddComponent<Image>();
            handle.sprite = circleSprite;
            handle.color = new Color(Cyan.r, Cyan.g, Cyan.b, 0.92f);
            handle.raycastTarget = false;
            AddOutline(handle, new Color(1f, 1f, 1f, 0.72f), 2f);

            VirtualJoystick joystick = zoneObject.AddComponent<VirtualJoystick>();
            joystick.Configure(
                inputRouter,
                joystickRect,
                handleRect,
                82f,
                110f,
                joystickDeadzone,
                visualGroup);
        }

        private void CreateActionButton(MobileInputRouter inputRouter, Sprite circleSprite)
        {
            GameObject buttonObject = CreateRectObject("Context Action Button", safeAreaRoot);
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            AnchorAt(buttonRect, new Vector2(1f, 0f), new Vector2(-140f, 146f));
            buttonRect.sizeDelta = new Vector2(172f, 172f);

            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.sprite = circleSprite;
            buttonImage.color = ActionIdle;
            buttonImage.raycastTarget = true;
            AddOutline(buttonImage, new Color(1f, 0.85f, 0.72f, 0.85f), 4f);

            MobileActionButton button = buttonObject.AddComponent<MobileActionButton>();
            button.Configure(inputRouter, buttonImage, ActionIdle, ActionPressed);

            Text buttonText = CreateText(
                "Action Text",
                buttonRect,
                "USE",
                26,
                Color.white,
                TextAnchor.MiddleCenter);
            Stretch(buttonText.rectTransform, 12f);
            buttonText.fontStyle = FontStyle.Bold;
        }

        private void CreateLookSurface(Sprite circleSprite)
        {
            GameObject zoneObject = CreateRectObject("Camera Look Zone", safeAreaRoot);
            RectTransform zoneRect = zoneObject.GetComponent<RectTransform>();
            zoneRect.anchorMin = new Vector2(0.52f, 0.34f);
            zoneRect.anchorMax = new Vector2(1f, 0.82f);
            zoneRect.offsetMin = Vector2.zero;
            zoneRect.offsetMax = Vector2.zero;

            Image captureSurface = zoneObject.AddComponent<Image>();
            captureSurface.color = new Color(0f, 0f, 0f, 0.001f);
            captureSurface.raycastTarget = true;

            FollowLocalPlayer cameraController =
                FindFirstObjectByType<FollowLocalPlayer>();
            CameraLookSurface lookSurface = zoneObject.AddComponent<CameraLookSurface>();
            lookSurface.Configure(cameraController);

            GameObject reticleObject = CreateRectObject(
                "Camera Look Reticle",
                zoneRect);
            RectTransform reticleRect = reticleObject.GetComponent<RectTransform>();
            AnchorAt(reticleRect, new Vector2(0.78f, 0.38f), Vector2.zero);
            reticleRect.sizeDelta = new Vector2(108f, 108f);
            Image reticle = reticleObject.AddComponent<Image>();
            reticle.sprite = circleSprite;
            reticle.color = ViewSurface;
            reticle.raycastTarget = false;
            AddOutline(reticle, new Color(Cyan.r, Cyan.g, Cyan.b, 0.28f), 2f);

            GameObject crosshairObject = CreateRectObject(
                "Camera Look Crosshair",
                reticleRect);
            RectTransform crosshairRect = crosshairObject.GetComponent<RectTransform>();
            Stretch(crosshairRect, 44f);
            Image crosshair = crosshairObject.AddComponent<Image>();
            crosshair.color = new Color(Cyan.r, Cyan.g, Cyan.b, 0.55f);
            crosshair.raycastTarget = false;
            AddOutline(crosshair, new Color(1f, 1f, 1f, 0.3f), 1f);
        }

        private void CreateCameraModeSelector()
        {
            GameObject panelObject = CreateRectObject(
                "Camera View Selector",
                safeAreaRoot);
            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            AnchorAt(panelRect, new Vector2(1f, 1f), new Vector2(-152f, -202f));
            panelRect.sizeDelta = new Vector2(286f, 66f);

            Image panelImage = panelObject.AddComponent<Image>();
            panelImage.color = DarkPanel;
            panelImage.raycastTarget = false;
            AddOutline(panelImage, new Color(Cyan.r, Cyan.g, Cyan.b, 0.38f), 2f);

            Text label = CreateText(
                "View Label",
                panelRect,
                "VIEW",
                16,
                new Color(0.74f, 0.84f, 0.92f, 1f),
                TextAnchor.MiddleLeft);
            AnchorAt(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(18f, 0f));
            label.rectTransform.sizeDelta = new Vector2(62f, 42f);

            CreateCameraModeButton(
                panelRect,
                "LOCK",
                FollowLocalPlayer.ViewMode.Locked,
                new Vector2(126f, 0f));
            CreateCameraModeButton(
                panelRect,
                "FREE",
                FollowLocalPlayer.ViewMode.Free,
                new Vector2(220f, 0f));
        }

        private void CreateCameraModeButton(
            RectTransform parent,
            string label,
            FollowLocalPlayer.ViewMode mode,
            Vector2 position)
        {
            GameObject buttonObject = CreateRectObject(
                $"View Mode {label}",
                parent);
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            AnchorAt(buttonRect, new Vector2(0f, 0.5f), position);
            buttonRect.sizeDelta = new Vector2(86f, 44f);

            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.raycastTarget = true;

            MobileCameraModeButton button =
                buttonObject.AddComponent<MobileCameraModeButton>();
            button.Configure(
                FindFirstObjectByType<FollowLocalPlayer>(),
                mode,
                buttonImage,
                ViewActive,
                ViewInactive);

            Text buttonText = CreateText(
                $"View Mode {label} Text",
                buttonRect,
                label,
                14,
                Color.white,
                TextAnchor.MiddleCenter);
            Stretch(buttonText.rectTransform, 2f);
        }

        private void RefreshSafeArea(bool force)
        {
            if (safeAreaRoot == null || Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            Rect safeArea = Screen.safeArea;
            if (safeArea.width <= 0f || safeArea.height <= 0f)
            {
                safeArea = new Rect(0f, 0f, Screen.width, Screen.height);
            }

            if (!force &&
                safeArea == lastSafeArea &&
                Screen.width == lastScreenWidth &&
                Screen.height == lastScreenHeight)
            {
                return;
            }

            lastSafeArea = safeArea;
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;

            safeAreaRoot.anchorMin = new Vector2(
                safeArea.xMin / Screen.width,
                safeArea.yMin / Screen.height);
            safeAreaRoot.anchorMax = new Vector2(
                safeArea.xMax / Screen.width,
                safeArea.yMax / Screen.height);
            safeAreaRoot.offsetMin = Vector2.zero;
            safeAreaRoot.offsetMax = Vector2.zero;
        }

        private Sprite CreateCircleSprite(int size)
        {
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false)
            {
                name = "Featherfall Mobile Control Circle",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };

            Color32[] pixels = new Color32[size * size];
            float center = (size - 1) * 0.5f;
            float radius = center;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(
                        new Vector2(x, y),
                        new Vector2(center, center));
                    float edge = Mathf.InverseLerp(radius, radius - 2f, distance);
                    byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(edge) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                size);
            sprite.name = "Featherfall Mobile Control Circle";
            sprite.hideFlags = HideFlags.HideAndDontSave;
            generatedAssets.Add(sprite);
            generatedAssets.Add(texture);
            return sprite;
        }

        private static GameObject CreateRectObject(string name, Transform parent)
        {
            GameObject gameObject = new(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static void AnchorAt(
            RectTransform rectTransform,
            Vector2 anchor,
            Vector2 anchoredPosition)
        {
            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchoredPosition;
        }

        private static void StretchCentered(RectTransform rectTransform, Vector2 size)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = size;
        }

        private static void Stretch(RectTransform rectTransform, float inset)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(inset, inset);
            rectTransform.offsetMax = new Vector2(-inset, -inset);
        }

        private static void AddOutline(Image image, Color color, float distance)
        {
            Outline outline = image.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(distance, -distance);
            outline.useGraphicAlpha = true;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            string value,
            int fontSize,
            Color color,
            TextAnchor alignment)
        {
            GameObject textObject = CreateRectObject(name, parent);
            Text text = textObject.AddComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            Shadow shadow = textObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
            shadow.effectDistance = new Vector2(2f, -2f);
            shadow.useGraphicAlpha = true;
            return text;
        }

        private void OnDestroy()
        {
            if (createdEventSystem != null)
            {
                Destroy(createdEventSystem);
            }
            if (canvasObject != null)
            {
                Destroy(canvasObject);
            }

            foreach (Object generatedAsset in generatedAssets)
            {
                if (generatedAsset != null)
                {
                    Destroy(generatedAsset);
                }
            }
            generatedAssets.Clear();
        }
    }
}
