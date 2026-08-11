using System.Collections.Generic;
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
            CreateActionButton(inputRouter, circleSprite);
        }

        private void CreateJoystick(MobileInputRouter inputRouter, Sprite circleSprite)
        {
            GameObject joystickObject = CreateRectObject("Move Joystick", safeAreaRoot);
            RectTransform joystickRect = joystickObject.GetComponent<RectTransform>();
            AnchorAt(joystickRect, new Vector2(0f, 0f), new Vector2(150f, 148f));
            joystickRect.sizeDelta = new Vector2(220f, 220f);

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

            VirtualJoystick joystick = joystickObject.AddComponent<VirtualJoystick>();
            joystick.Configure(inputRouter, handleRect, 82f, joystickDeadzone);

            Text label = CreateText(
                "Move Label",
                joystickRect,
                "MOVE",
                18,
                Color.white,
                TextAnchor.MiddleCenter);
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0.5f, 1f);
            labelRect.anchorMax = new Vector2(0.5f, 1f);
            labelRect.pivot = new Vector2(0.5f, 0f);
            labelRect.anchoredPosition = new Vector2(0f, 10f);
            labelRect.sizeDelta = new Vector2(180f, 30f);
        }

        private void CreateActionButton(MobileInputRouter inputRouter, Sprite circleSprite)
        {
            GameObject buttonObject = CreateRectObject("Context Action Button", safeAreaRoot);
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            AnchorAt(buttonRect, new Vector2(1f, 0f), new Vector2(-140f, 146f));
            buttonRect.sizeDelta = new Vector2(158f, 158f);

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
                "USE\nDOOR",
                22,
                Color.white,
                TextAnchor.MiddleCenter);
            Stretch(buttonText.rectTransform, 12f);
            buttonText.fontStyle = FontStyle.Bold;

            Text label = CreateText(
                "Action Label",
                buttonRect,
                "ACTION",
                18,
                Color.white,
                TextAnchor.MiddleCenter);
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0.5f, 1f);
            labelRect.anchorMax = new Vector2(0.5f, 1f);
            labelRect.pivot = new Vector2(0.5f, 0f);
            labelRect.anchoredPosition = new Vector2(0f, 10f);
            labelRect.sizeDelta = new Vector2(180f, 30f);
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
