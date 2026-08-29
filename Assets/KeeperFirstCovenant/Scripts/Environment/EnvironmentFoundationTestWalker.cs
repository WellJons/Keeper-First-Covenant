using UnityEngine;
using UnityEngine.InputSystem;

namespace KeeperFirstCovenant.Environment
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class EnvironmentFoundationTestWalker : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float movementSpeed = 3.6f;
        [SerializeField, Min(0.1f)] private float acceleration = 18f;
        [SerializeField] private Transform visual;

        private CharacterController controller;
        private Vector3 velocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            Vector2 input = Vector2.zero;

            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                    input.y += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                    input.y -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                    input.x += 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                    input.x -= 1f;
            }

            input = Vector2.ClampMagnitude(input, 1f);
            Vector3 desired = new Vector3(input.x, 0f, input.y) * movementSpeed;
            velocity = Vector3.MoveTowards(
                velocity,
                desired,
                acceleration * Time.deltaTime);

            EnvironmentSurfaceProfile profile = ResolveCurrentSurface();
            float multiplier = profile != null ? profile.MovementMultiplier : 1f;
            controller.Move(velocity * (multiplier * Time.deltaTime));

            if (visual != null && velocity.sqrMagnitude > 0.01f)
            {
                Quaternion target = Quaternion.LookRotation(velocity.normalized, Vector3.up);
                visual.rotation = Quaternion.Slerp(
                    visual.rotation,
                    target,
                    1f - Mathf.Exp(-12f * Time.deltaTime));
            }
        }

        public void Configure(Transform visualTransform)
        {
            visual = visualTransform;
        }

        private EnvironmentSurfaceProfile ResolveCurrentSurface()
        {
            if (EnvironmentSurface.TryResolveBelow(
                    transform.position,
                    2.2f,
                    ~0,
                    out EnvironmentSurface _,
                    out EnvironmentSurfaceProfile profile,
                    out RaycastHit _))
            {
                return profile;
            }

            return null;
        }
    }
}
