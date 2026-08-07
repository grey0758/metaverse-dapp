using Unity.Netcode;
using UnityEngine;

namespace MetaverseGame.Gameplay
{
    public sealed class FollowLocalPlayer : MonoBehaviour
    {
        [SerializeField] private Vector3 offset = new(0f, 9f, -8f);
        [SerializeField, Min(0.1f)] private float smoothing = 8f;

        private void LateUpdate()
        {
            NetworkObject player = NetworkManager.Singleton?.LocalClient?.PlayerObject;
            if (player == null)
            {
                return;
            }

            Vector3 target = player.transform.position;
            transform.position = Vector3.Lerp(
                transform.position,
                target + offset,
                1f - Mathf.Exp(-smoothing * Time.deltaTime));
            transform.LookAt(target + Vector3.up * 0.75f);
        }
    }
}
