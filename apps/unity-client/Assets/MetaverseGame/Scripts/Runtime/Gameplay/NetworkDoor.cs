using Unity.Netcode;
using UnityEngine;

namespace MetaverseGame.Gameplay
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class NetworkDoor : NetworkBehaviour
    {
        [SerializeField, Range(0f, 180f)] private float openAngle = 90f;
        [SerializeField, Min(0f)] private float cooldownSeconds = 0.5f;

        private readonly NetworkVariable<bool> isOpen = new(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private Quaternion closedRotation;
        private double nextToggleAt;

        public Vector3 InteractionPoint => transform.position;

        private void Awake()
        {
            closedRotation = transform.rotation;
        }

        public override void OnNetworkSpawn()
        {
            isOpen.OnValueChanged += OnOpenChanged;
            ApplyState(isOpen.Value);
        }

        public override void OnNetworkDespawn()
        {
            isOpen.OnValueChanged -= OnOpenChanged;
        }

        public bool TryToggleOnServer()
        {
            if (!IsServer || NetworkManager.ServerTime.Time < nextToggleAt)
            {
                return false;
            }

            nextToggleAt = NetworkManager.ServerTime.Time + cooldownSeconds;
            isOpen.Value = !isOpen.Value;
            return true;
        }

        private void OnOpenChanged(bool previous, bool current)
        {
            ApplyState(current);
        }

        private void ApplyState(bool open)
        {
            transform.rotation = open
                ? closedRotation * Quaternion.Euler(0f, openAngle, 0f)
                : closedRotation;
        }
    }
}
