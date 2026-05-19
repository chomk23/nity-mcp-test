using UnityEngine;
using UnityEngine.InputSystem;

namespace ForTheCompany.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class RealtimePlayerController : MonoBehaviour
    {
        public float walkSpeed = 5f;
        public float runMultiplier = 1.8f;
        public float gravity = -20f;

        private CharacterController controller;
        private float verticalVelocity;

        public bool IsRunning { get; private set; }
        public Vector3 LastInputDirection { get; private set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            var kb = Keyboard.current;
            Vector2 input = Vector2.zero;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) input.y += 1;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) input.y -= 1;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) input.x -= 1;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1;
            }

            if (input.sqrMagnitude > 1f) input.Normalize();

            IsRunning = kb != null && (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed) && input.sqrMagnitude > 0.01f;
            float speed = walkSpeed * (IsRunning ? runMultiplier : 1f);

            Vector3 horizontal = new Vector3(input.x, 0f, input.y) * speed;
            LastInputDirection = horizontal.sqrMagnitude > 0.01f ? horizontal.normalized : LastInputDirection;

            if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -1f;
            verticalVelocity += gravity * Time.deltaTime;

            Vector3 motion = horizontal + Vector3.up * verticalVelocity;
            controller.Move(motion * Time.deltaTime);

            if (horizontal.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(horizontal);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 12f * Time.deltaTime);
            }
        }
    }
}
