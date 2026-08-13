using Unity.Collections;
using MetaverseGame.Input;
using Unity.Netcode;
using UnityEngine;

namespace MetaverseGame.Gameplay
{
    [RequireComponent(typeof(NetworkObject), typeof(CharacterController))]
    public sealed class NetworkPlayerController : NetworkBehaviour
    {
        private static readonly Vector3[] SpawnPositions =
        {
            new(-6.35f, 1f, -6.05f),
            new(6.35f, 1f, -6.05f),
            new(-6.35f, 1f, -2.2f),
            new(6.35f, 1f, -2.2f),
            new(-6.35f, 1f, 4f),
            new(6.35f, 1f, 4f),
        };

        [SerializeField, Min(0.1f)] private float speed = 4f;
        [SerializeField, Min(1f)] private float turnSpeed = 12f;
        [SerializeField, Min(1f)] private float inputRate = 30f;
        [SerializeField, Min(0.1f)] private float interactionDistance = 2.4f;
        [SerializeField, Min(0.05f)] private float inputTimeout = 0.25f;

        private readonly NetworkVariable<FixedString32Bytes> privateRole = new(
            default,
            NetworkVariableReadPermission.Owner,
            NetworkVariableWritePermission.Server);

        private CharacterController controller;
        private Vector2 serverInput;
        private uint localMoveSequence;
        private uint localInteractionSequence;
        private uint lastMoveSequence;
        private uint lastInteractionSequence;
        private float lastInputAt;
        private float nextInputAt;
        private MobileInputRouter inputRouter;
        private FollowLocalPlayer cameraController;

        public string PrivateRole => privateRole.Value.ToString();

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            inputRouter = FindFirstObjectByType<MobileInputRouter>();
            cameraController = FindFirstObjectByType<FollowLocalPlayer>();
        }

        public override void OnNetworkSpawn()
        {
            controller.enabled = false;
            if (IsServer)
            {
                if (MatchSessionRegistry.Instance != null &&
                    MatchSessionRegistry.Instance.TryGetSession(
                        OwnerClientId,
                        out SessionRecord session))
                {
                    transform.position = ResolveSpawnPosition(session.SpawnIndex);
                    privateRole.Value = new FixedString32Bytes(session.Role);
                }
                else
                {
                    transform.position = ResolveSpawnPosition(
                        (int)(OwnerClientId % (ulong)SpawnPositions.Length));
                    privateRole.Value = new FixedString32Bytes(
                        OwnerClientId == 1 ? "duck" : "goose");
                }
            }
            controller.enabled = IsServer;
            if (IsOwner && privateRole.Value.Length > 0)
            {
                Debug.Log($"Private role assigned: {privateRole.Value}");
            }
        }

        private void Update()
        {
            if (!IsSpawned || !IsOwner)
            {
                return;
            }

            if (inputRouter == null)
            {
                inputRouter = FindFirstObjectByType<MobileInputRouter>();
            }
            if (cameraController == null)
            {
                cameraController = FindFirstObjectByType<FollowLocalPlayer>();
            }

            bool interactionPressed = inputRouter != null
                ? inputRouter.ConsumeInteractPressed()
                : MobileInputRouter.ReadDesktopFallbackInteractPressed();
            if (interactionPressed)
            {
                RequestInteraction();
            }

            if (Time.unscaledTime < nextInputAt)
            {
                return;
            }

            nextInputAt = Time.unscaledTime + 1f / inputRate;
            Vector2 input = inputRouter != null
                ? inputRouter.MoveInput
                : MobileInputRouter.ReadDesktopFallbackMoveInput();
            if (cameraController != null)
            {
                input = cameraController.ConvertMoveInput(input);
            }
            SubmitMoveInputRpc(input, ++localMoveSequence);
        }

        private void FixedUpdate()
        {
            if (!IsSpawned || !IsServer || !controller.enabled)
            {
                return;
            }

            if (Time.unscaledTime - lastInputAt > inputTimeout)
            {
                serverInput = Vector2.zero;
            }

            Vector3 direction = new(serverInput.x, 0f, serverInput.y);
            controller.Move(direction * (speed * Time.fixedDeltaTime));
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion target = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    target,
                    turnSpeed * Time.fixedDeltaTime);
            }
        }

        [Rpc(
            SendTo.Server,
            Delivery = RpcDelivery.Unreliable,
            InvokePermission = RpcInvokePermission.Owner)]
        private void SubmitMoveInputRpc(
            Vector2 input,
            uint sequence,
            RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId ||
                !ServerAuthorityRules.IsNewerSequence(sequence, lastMoveSequence))
            {
                return;
            }

            lastMoveSequence = sequence;
            serverInput = ServerAuthorityRules.SanitizeMoveInput(input);
            lastInputAt = Time.unscaledTime;
        }

        public void RequestInteraction()
        {
            if (!IsSpawned || !IsOwner)
            {
                return;
            }

            NetworkDoor closest = null;
            float closestDistanceSquared = interactionDistance * interactionDistance;
            NetworkDoor[] doors = FindObjectsByType<NetworkDoor>(FindObjectsSortMode.None);
            foreach (NetworkDoor door in doors)
            {
                if (!door.IsSpawned)
                {
                    continue;
                }
                float distanceSquared = (door.InteractionPoint - transform.position).sqrMagnitude;
                if (distanceSquared <= closestDistanceSquared)
                {
                    closest = door;
                    closestDistanceSquared = distanceSquared;
                }
            }

            if (closest != null)
            {
                RequestDoorToggleRpc(
                    closest.NetworkObject,
                    ++localInteractionSequence);
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void RequestDoorToggleRpc(
            NetworkObjectReference targetReference,
            uint sequence,
            RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId ||
                !ServerAuthorityRules.IsNewerSequence(sequence, lastInteractionSequence) ||
                !targetReference.TryGet(out NetworkObject target, NetworkManager) ||
                !target.TryGetComponent(out NetworkDoor door) ||
                !ServerAuthorityRules.IsWithinInteractionDistance(
                    transform.position,
                    door.InteractionPoint,
                    interactionDistance) ||
                !HasLineOfSight(door))
            {
                return;
            }

            lastInteractionSequence = sequence;
            door.TryToggleOnServer();
        }

        private bool HasLineOfSight(NetworkDoor door)
        {
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 offset = door.InteractionPoint - origin;
            RaycastHit[] hits = Physics.RaycastAll(
                origin,
                offset.normalized,
                offset.magnitude,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            foreach (RaycastHit hit in hits)
            {
                Transform hitTransform = hit.collider.transform;
                if (hitTransform.IsChildOf(transform) ||
                    hitTransform == transform ||
                    hitTransform.IsChildOf(door.transform) ||
                    hitTransform == door.transform)
                {
                    continue;
                }
                return false;
            }
            return true;
        }

        public static Vector3 ResolveSpawnPosition(int spawnIndex)
        {
            return SpawnPositions[
                Mathf.Abs(spawnIndex) % SpawnPositions.Length];
        }
    }
}
