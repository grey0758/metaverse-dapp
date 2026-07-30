using UnityEngine;

namespace MetaverseGame.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float speed = 4f;
        [SerializeField, Min(1f)] private float turnSpeed = 12f;

        private CharacterController controller;

        public Vector2 CurrentInput { get; private set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            CurrentInput = new Vector2(
                UnityEngine.Input.GetAxisRaw("Horizontal"),
                UnityEngine.Input.GetAxisRaw("Vertical"));
            CurrentInput = Vector2.ClampMagnitude(CurrentInput, 1f);

            Vector3 direction = new(CurrentInput.x, 0f, CurrentInput.y);
            controller.SimpleMove(direction * speed);
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion target = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    target,
                    turnSpeed * Time.deltaTime);
            }
        }
    }
}
