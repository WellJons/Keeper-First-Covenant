using KeeperFirstCovenant.Visual;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KeeperFirstCovenant.Characters
{
    [DisallowMultipleComponent]
    public sealed class EdwardExplorationController : MonoBehaviour
    {
        [SerializeField] private float walkSpeed = 3.2f;
        [SerializeField] private float runSpeed = 5.4f;
        [SerializeField] private float acceleration = 18f;
        [SerializeField] private float gravity = 24f;
        [SerializeField] private bool movementEnabled = true;

        [SerializeField] private PaperDollCharacterVisual visual;
        [SerializeField] private PaperDollMotionAnimator motion;
        [SerializeField] private CharacterController characterController;

        private Vector3 _velocity;
        private Vector3 _planarVelocity;

        public bool MovementEnabled
        {
            get => movementEnabled;
            set
            {
                movementEnabled = value;
                if (!value)
                {
                    _planarVelocity = Vector3.zero;
                    motion?.SetLocomotion(false, false, 0f);
                }
            }
        }

        private void Awake()
        {
            if (visual == null)
                visual = GetComponentInChildren<PaperDollCharacterVisual>();
            if (motion == null)
                motion = GetComponentInChildren<PaperDollMotionAnimator>();
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (!movementEnabled || motion != null && motion.IsDead)
            {
                _planarVelocity = Vector3.MoveTowards(_planarVelocity, Vector3.zero, acceleration * Time.deltaTime);
                motion?.SetLocomotion(false, false, 0f);
                ApplyMovement(Vector3.zero, false);
                return;
            }

            Keyboard keyboard = Keyboard.current;
            Vector2 input = Vector2.zero;
            bool running = false;

            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed) input.y += 1f;
                if (keyboard.sKey.isPressed) input.y -= 1f;
                if (keyboard.dKey.isPressed) input.x += 1f;
                if (keyboard.aKey.isPressed) input.x -= 1f;
                running = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            }

            input = Vector2.ClampMagnitude(input, 1f);
            Vector3 desiredDirection = CameraRelativeDirection(input);
            float speed = running ? runSpeed : walkSpeed;
            Vector3 desiredVelocity = desiredDirection * speed;
            _planarVelocity = Vector3.MoveTowards(_planarVelocity, desiredVelocity, acceleration * Time.deltaTime);

            bool moving = desiredDirection.sqrMagnitude > 0.001f;
            if (moving)
                visual?.FaceWorldDirection(desiredDirection);

            motion?.SetLocomotion(moving, running, speed > 0f ? _planarVelocity.magnitude / speed : 0f);
            ApplyMovement(_planarVelocity, moving);
        }

        private Vector3 CameraRelativeDirection(Vector2 input)
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
            forward.Normalize();
            right.Normalize();
            return (right * input.x + forward * input.y).normalized;
        }

        private void ApplyMovement(Vector3 planar, bool moving)
        {
            if (characterController != null)
            {
                if (characterController.isGrounded && _velocity.y < 0f)
                    _velocity.y = -2f;
                else
                    _velocity.y -= gravity * Time.deltaTime;

                Vector3 frameMotion = planar + Vector3.up * _velocity.y;
                characterController.Move(frameMotion * Time.deltaTime);
            }
            else if (moving)
            {
                transform.position += planar * Time.deltaTime;
            }
        }
    }
}
