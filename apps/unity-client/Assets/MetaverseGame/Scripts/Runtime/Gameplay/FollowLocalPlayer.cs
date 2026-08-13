using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MetaverseGame.Gameplay
{
    /// <summary>
    /// Mobile third-person orbit camera. Locked view follows the player's
    /// facing direction; free view keeps an independent orbit controlled by
    /// the right-side look surface.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class FollowLocalPlayer : MonoBehaviour
    {
        public enum ViewMode
        {
            Locked,
            Free,
        }

        private const float PlayerEyeTargetHeight = 1.02f;
        private const float DefaultDistance = 4.6f;
        private const float DefaultPitch = 17f;
        private const float DefaultLookSensitivity = 0.16f;
        private const float DefaultMinDistance = 1.35f;
        private const float DefaultMaxDistance = 6.2f;

        [SerializeField] private ViewMode viewMode = ViewMode.Locked;
        [SerializeField, Min(0.1f)] private float distance = DefaultDistance;
        [SerializeField, Range(-10f, 55f)] private float pitch = DefaultPitch;
        [SerializeField, Range(-180f, 180f)] private float lockedYawOffset;
        [SerializeField, Min(0.1f)] private float smoothing = 10f;
        [SerializeField, Min(0.1f)] private float rotationSmoothing = 14f;
        [SerializeField, Min(0.01f)] private float lookSensitivity = DefaultLookSensitivity;
        [SerializeField, Range(-30f, 80f)] private float minPitch = -8f;
        [SerializeField, Range(-10f, 89f)] private float maxPitch = 48f;
        [SerializeField, Min(0.1f)] private float minDistance = DefaultMinDistance;
        [SerializeField, Min(0.1f)] private float maxDistance = DefaultMaxDistance;
        [SerializeField, Min(0.01f)] private float collisionRadius = 0.22f;
        [SerializeField, Min(0.01f)] private float collisionPadding = 0.12f;
        [SerializeField] private LayerMask collisionMask = ~0;

        private readonly RaycastHit[] collisionHits = new RaycastHit[24];
        private Transform trackedPlayer;
        private float freeYaw;
        private float currentYaw;
        private float currentPitch;
        private bool orbitInitialized;

        public ViewMode CurrentViewMode => viewMode;
        public bool IsFreeView => viewMode == ViewMode.Free;
        public float CurrentYaw => currentYaw;
        public float CurrentPitch => currentPitch;
        public float Distance => distance;

        private void Awake()
        {
            distance = ClampDistance(distance, minDistance, maxDistance);
            pitch = ClampPitch(pitch, minPitch, maxPitch);
            currentPitch = pitch;
            if (collisionMask.value == 0)
            {
                collisionMask = ~0;
            }
        }

        private void LateUpdate()
        {
            NetworkObject playerObject = NetworkManager.Singleton?.LocalClient?.PlayerObject;
            if (playerObject == null)
            {
                return;
            }

            Transform player = playerObject.transform;
            if (trackedPlayer != player)
            {
                trackedPlayer = player;
                orbitInitialized = false;
            }

            float lockedYaw = player.eulerAngles.y + lockedYawOffset;
            if (!orbitInitialized)
            {
                currentYaw = lockedYaw;
                freeYaw = lockedYaw;
                currentPitch = pitch;
                orbitInitialized = true;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ReadDesktopLookFallback();
#endif

            if (viewMode == ViewMode.Locked)
            {
                currentYaw = Mathf.LerpAngle(
                    currentYaw,
                    lockedYaw,
                    1f - Mathf.Exp(-rotationSmoothing * Time.deltaTime));
                freeYaw = currentYaw;
                currentPitch = Mathf.Lerp(
                    currentPitch,
                    pitch,
                    1f - Mathf.Exp(-rotationSmoothing * Time.deltaTime));
            }
            else
            {
                currentYaw = Mathf.LerpAngle(
                    currentYaw,
                    freeYaw,
                    1f - Mathf.Exp(-rotationSmoothing * Time.deltaTime));
            }

            Vector3 target = player.position + Vector3.up * PlayerEyeTargetHeight;
            Vector3 desiredPosition = CalculateOrbitPosition(
                target,
                currentYaw,
                currentPitch,
                distance);
            desiredPosition = ResolveCameraCollision(target, desiredPosition, player);

            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                1f - Mathf.Exp(-smoothing * Time.deltaTime));

            Quaternion desiredRotation = Quaternion.LookRotation(
                target - transform.position,
                Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                1f - Mathf.Exp(-rotationSmoothing * Time.deltaTime));
        }

        public void SetViewMode(ViewMode mode)
        {
            if (viewMode == mode)
            {
                return;
            }

            if (mode == ViewMode.Free)
            {
                freeYaw = currentYaw;
            }
            else
            {
                freeYaw = currentYaw;
            }

            viewMode = mode;
        }

        public void ToggleViewMode()
        {
            SetViewMode(viewMode == ViewMode.Locked ? ViewMode.Free : ViewMode.Locked);
        }

        public void ApplyLookDelta(Vector2 screenDelta)
        {
            if (viewMode != ViewMode.Free ||
                !IsFinite(screenDelta.x) ||
                !IsFinite(screenDelta.y))
            {
                return;
            }

            freeYaw += screenDelta.x * lookSensitivity;
            currentPitch = ClampPitch(
                currentPitch - screenDelta.y * lookSensitivity,
                minPitch,
                maxPitch);
        }

        public Vector2 ConvertMoveInput(Vector2 input)
        {
            return ConvertMoveInput(input, currentYaw);
        }

        public static Vector2 ConvertMoveInput(Vector2 input, float yaw)
        {
            if (!IsFinite(input.x) || !IsFinite(input.y) || !IsFinite(yaw))
            {
                return Vector2.zero;
            }

            Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);
            Vector3 forward = yawRotation * Vector3.forward;
            Vector3 right = yawRotation * Vector3.right;
            Vector3 direction = right * input.x + forward * input.y;
            direction.y = 0f;
            if (direction.sqrMagnitude > 1f)
            {
                direction.Normalize();
            }

            return new Vector2(direction.x, direction.z);
        }

        public static float ClampPitch(float value, float lowerBound, float upperBound)
        {
            float min = Mathf.Min(lowerBound, upperBound);
            float max = Mathf.Max(lowerBound, upperBound);
            return Mathf.Clamp(value, min, max);
        }

        public static float ClampDistance(
            float value,
            float lowerBound = DefaultMinDistance,
            float upperBound = DefaultMaxDistance)
        {
            float min = Mathf.Max(0.1f, Mathf.Min(lowerBound, upperBound));
            float max = Mathf.Max(min, Mathf.Max(lowerBound, upperBound));
            return Mathf.Clamp(value, min, max);
        }

        public static Vector3 CalculateOrbitPosition(
            Vector3 target,
            float yaw,
            float pitch,
            float orbitDistance)
        {
            Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
            return target + orbitRotation * (Vector3.back * Mathf.Max(0.1f, orbitDistance));
        }

        private Vector3 ResolveCameraCollision(
            Vector3 target,
            Vector3 desiredPosition,
            Transform player)
        {
            Vector3 offset = desiredPosition - target;
            float desiredDistance = offset.magnitude;
            if (desiredDistance <= 0.01f)
            {
                return desiredPosition;
            }

            Vector3 direction = offset / desiredDistance;
            int hitCount = Physics.SphereCastNonAlloc(
                target,
                collisionRadius,
                direction,
                collisionHits,
                desiredDistance,
                collisionMask,
                QueryTriggerInteraction.Ignore);
            float resolvedDistance = desiredDistance;
            for (int index = 0; index < hitCount; index++)
            {
                Collider collider = collisionHits[index].collider;
                if (collider == null ||
                    collider.transform == player ||
                    collider.transform.IsChildOf(player))
                {
                    continue;
                }

                resolvedDistance = Mathf.Min(
                    resolvedDistance,
                    collisionHits[index].distance - collisionPadding);
            }

            resolvedDistance = Mathf.Max(
                Mathf.Min(0.35f, desiredDistance),
                resolvedDistance);
            return target + direction * resolvedDistance;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void ReadDesktopLookFallback()
        {
            if (Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame)
            {
                ToggleViewMode();
            }

            if (viewMode == ViewMode.Free &&
                Mouse.current != null &&
                Mouse.current.rightButton.isPressed)
            {
                ApplyLookDelta(Mouse.current.delta.ReadValue());
            }
        }
#endif

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
