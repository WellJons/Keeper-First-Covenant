using KeeperFirstCovenant.Visual;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KeeperFirstCovenant.Characters
{
    [DisallowMultipleComponent]
    public sealed class HighResExplorationController : MonoBehaviour
    {
        [SerializeField] private HighResFrameCharacter2D animator2D;
        [SerializeField] private CharacterController characterController;

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 3.15f;
        [SerializeField] private float runSpeed = 5.25f;
        [SerializeField] private float acceleration = 20f;
        [SerializeField] private float gravity = 24f;

        [Header("Control")]
        [SerializeField] private bool movementEnabled = true;

        private Vector3 _planarVelocity;
        private float _verticalVelocity;

        public bool MovementEnabled
        {
            get => movementEnabled;
            set => movementEnabled = value;
        }

        private void Awake()
        {
            if (animator2D == null)
                animator2D = GetComponentInChildren<HighResFrameCharacter2D>();

            if (characterController == null)
                characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            Vector2 input = ReadMovement();
            bool running = IsRunning();

            if (!movementEnabled)
            {
                input = Vector2.zero;
                running = false;
            }

            Vector3 worldDirection = CameraRelative(input);
            float speed = running ? runSpeed : walkSpeed;
            Vector3 desiredVelocity = worldDirection * speed;

            _planarVelocity = Vector3.MoveTowards(
                _planarVelocity,
                desiredVelocity,
                acceleration * Time.deltaTime);

            bool hasInput = worldDirection.sqrMagnitude > 0.0001f;
            bool actionLocked = animator2D != null && animator2D.IsOneShotPlaying;

            if (!actionLocked)
            {
                if (hasInput)
                {
                    animator2D?.FaceWorldDirection(worldDirection);
                    animator2D?.PlayLoop(
                        running
                            ? CharacterFrameState.Run
                            : CharacterFrameState.Walk);
                }
                else
                {
                    animator2D?.PlayLoop(CharacterFrameState.Idle);
                }
            }

            MoveCharacter(_planarVelocity);
        }

        private static Vector2 ReadMovement()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return Vector2.zero;

            Vector2 input = Vector2.zero;

            if (keyboard.wKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed) input.y -= 1f;
            if (keyboard.aKey.isPressed) input.x -= 1f;
            if (keyboard.dKey.isPressed) input.x += 1f;

            return Vector2.ClampMagnitude(input, 1f);
        }

        private static bool IsRunning()
        {
            Keyboard keyboard = Keyboard.current;
            return keyboard != null &&
                   (keyboard.leftShiftKey.isPressed ||
                    keyboard.rightShiftKey.isPressed);
        }

        private static Vector3 CameraRelative(Vector2 input)
        {
            if (input.sqrMagnitude < 0.0001f)
                return Vector3.zero;

            Camera camera = Camera.main;
            if (camera == null)
                return new Vector3(input.x, 0f, input.y).normalized;

            Vector3 forward = camera.transform.forward;
            Vector3 right = camera.transform.right;

            forward.y = 0f;
            right.y = 0f;

            if (forward.sqrMagnitude > 0.0001f)
                forward.Normalize();
            if (right.sqrMagnitude > 0.0001f)
                right.Normalize();

            return (right * input.x + forward * input.y).normalized;
        }

        private void MoveCharacter(Vector3 planarVelocity)
        {
            if (characterController == null)
            {
                transform.position += planarVelocity * Time.deltaTime;
                return;
            }

            if (characterController.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;
            else
                _verticalVelocity -= gravity * Time.deltaTime;

            Vector3 velocity = planarVelocity + Vector3.up * _verticalVelocity;
            characterController.Move(velocity * Time.deltaTime);
        }
    }
}
