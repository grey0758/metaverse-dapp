using UnityEngine;
using UnityEngine.InputSystem;

namespace MetaverseGame.Input
{
    /// <summary>
    /// The single client-side input boundary for movement and context actions.
    /// Touch controls write into this router; the server-facing gameplay code
    /// consumes the same commands regardless of the device that produced them.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MobileInputRouter : MonoBehaviour
    {
        [SerializeField, Range(0f, 0.45f)] private float moveDeadzone = 0.12f;

        private InputAction desktopMoveAction;
        private InputAction desktopInteractAction;
        private Vector2 touchMoveInput;
        private bool touchMoveActive;
        private int pendingInteractPresses;

        public static MobileInputRouter Instance { get; private set; }

        public Vector2 MoveInput
        {
            get
            {
                if (touchMoveActive)
                {
                    return MobileInputMath.SanitizeMoveInput(
                        touchMoveInput,
                        moveDeadzone);
                }

                return MobileInputMath.SanitizeMoveInput(
                    desktopMoveAction != null
                        ? desktopMoveAction.ReadValue<Vector2>()
                        : ReadDesktopFallbackMoveInput(),
                    moveDeadzone);
            }
        }

        public bool HasActiveTouchMove => touchMoveActive;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            CreateDesktopFallbackActions();
        }

        private void OnEnable()
        {
            desktopMoveAction?.Enable();
            desktopInteractAction?.Enable();
        }

        private void OnDisable()
        {
            desktopMoveAction?.Disable();
            desktopInteractAction?.Disable();
            ResetTransientInput();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                ResetTransientInput();
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                ResetTransientInput();
            }
        }

        private void OnDestroy()
        {
            desktopMoveAction?.Dispose();
            desktopInteractAction?.Dispose();
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetTouchMoveInput(Vector2 input)
        {
            touchMoveInput = MobileInputMath.SanitizeMoveInput(input, 0f);
            touchMoveActive = true;
        }

        public void ClearTouchMoveInput()
        {
            touchMoveInput = Vector2.zero;
            touchMoveActive = false;
        }

        private void ResetTransientInput()
        {
            ClearTouchMoveInput();
            pendingInteractPresses = 0;
        }

        public void PressInteract()
        {
            pendingInteractPresses = Mathf.Min(pendingInteractPresses + 1, 4);
        }

        public bool ConsumeInteractPressed()
        {
            bool pressed = pendingInteractPresses > 0;
            if (pressed)
            {
                pendingInteractPresses--;
            }

            if (desktopInteractAction != null &&
                desktopInteractAction.WasPressedThisFrame())
            {
                pressed = true;
            }

            return pressed;
        }

        public static bool ReadDesktopFallbackInteractPressed()
        {
            if (!DesktopFallbackEnabled)
            {
                return false;
            }

            return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        }

        public static Vector2 ReadDesktopFallbackMoveInput()
        {
            if (!DesktopFallbackEnabled)
            {
                return Vector2.zero;
            }

            Keyboard keyboard = Keyboard.current;
            Vector2 input = Vector2.zero;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                {
                    input.x -= 1f;
                }
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                {
                    input.x += 1f;
                }
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                {
                    input.y -= 1f;
                }
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                {
                    input.y += 1f;
                }
            }

            if (input == Vector2.zero && Gamepad.current != null)
            {
                input = Gamepad.current.leftStick.ReadValue();
            }

            return MobileInputMath.SanitizeMoveInput(input, 0.12f);
        }

        private void CreateDesktopFallbackActions()
        {
            if (!DesktopFallbackEnabled)
            {
                return;
            }

            desktopMoveAction = new InputAction(
                "DesktopMove",
                InputActionType.Value,
                expectedControlType: "Vector2");
            AddKeyboardMoveComposite(desktopMoveAction, "w", "s", "a", "d");
            AddKeyboardMoveComposite(
                desktopMoveAction,
                "upArrow",
                "downArrow",
                "leftArrow",
                "rightArrow");
            desktopMoveAction.AddBinding("<Gamepad>/leftStick");

            desktopInteractAction = new InputAction(
                "DesktopInteract",
                InputActionType.Button,
                expectedControlType: "Button");
            desktopInteractAction.AddBinding("<Keyboard>/e");
            desktopInteractAction.AddBinding("<Gamepad>/buttonSouth");

            desktopMoveAction.Enable();
            desktopInteractAction.Enable();
        }

        private static void AddKeyboardMoveComposite(
            InputAction action,
            string up,
            string down,
            string left,
            string right)
        {
            action.AddCompositeBinding("2DVector")
                .With("Up", $"<Keyboard>/{up}")
                .With("Down", $"<Keyboard>/{down}")
                .With("Left", $"<Keyboard>/{left}")
                .With("Right", $"<Keyboard>/{right}");
        }

        private static bool DesktopFallbackEnabled
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return true;
#else
                return false;
#endif
            }
        }
    }

    public static class MobileInputMath
    {
        public static Vector2 SanitizeMoveInput(Vector2 input, float deadzone)
        {
            if (!IsFinite(input.x) || !IsFinite(input.y))
            {
                return Vector2.zero;
            }

            input = Vector2.ClampMagnitude(input, 1f);
            float magnitude = input.magnitude;
            float clampedDeadzone = Mathf.Clamp(deadzone, 0f, 0.99f);
            if (magnitude <= clampedDeadzone)
            {
                return Vector2.zero;
            }

            if (clampedDeadzone <= 0f)
            {
                return input;
            }

            float remappedMagnitude = Mathf.InverseLerp(
                clampedDeadzone,
                1f,
                magnitude);
            return input / magnitude * remappedMagnitude;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
