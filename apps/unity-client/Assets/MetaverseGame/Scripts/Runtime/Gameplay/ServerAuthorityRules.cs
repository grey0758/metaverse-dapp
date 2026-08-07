using UnityEngine;

namespace MetaverseGame.Gameplay
{
    public static class ServerAuthorityRules
    {
        public static bool IsNewerSequence(uint candidate, uint previous)
        {
            return unchecked((int)(candidate - previous)) > 0;
        }

        public static Vector2 SanitizeMoveInput(Vector2 input)
        {
            if (float.IsNaN(input.x) || float.IsInfinity(input.x) ||
                float.IsNaN(input.y) || float.IsInfinity(input.y))
            {
                return Vector2.zero;
            }
            return Vector2.ClampMagnitude(input, 1f);
        }

        public static bool IsWithinInteractionDistance(
            Vector3 actorPosition,
            Vector3 targetPosition,
            float maximumDistance)
        {
            if (maximumDistance < 0f)
            {
                return false;
            }
            return (targetPosition - actorPosition).sqrMagnitude <=
                maximumDistance * maximumDistance;
        }
    }
}
