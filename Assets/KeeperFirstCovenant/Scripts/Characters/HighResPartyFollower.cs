using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Visual;
using UnityEngine;

namespace KeeperFirstCovenant.Characters
{
    [DisallowMultipleComponent]
    public sealed class HighResPartyFollower : MonoBehaviour
    {
        [SerializeField] private Transform leader;
        [SerializeField] private HighResFrameCharacter2D animator2D;
        [SerializeField] private CharacterController controller;

        [Header("Follow")]
        [SerializeField] private float preferredDistance = 1.45f;
        [SerializeField] private float stopDistance = 1.05f;
        [SerializeField] private float walkSpeed = 3.0f;
        [SerializeField] private float runSpeed = 5.0f;
        [SerializeField] private float runDistance = 3.4f;
        [SerializeField] private float acceleration = 16f;
        [SerializeField] private float gravity = 24f;

        private Vector3 _planarVelocity;
        private float _verticalVelocity;

        public Transform Leader
        {
            get => leader;
            set => leader = value;
        }

        private void Awake()
        {
            if (animator2D == null)
                animator2D = GetComponentInChildren<HighResFrameCharacter2D>();

            if (controller == null)
                controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (leader == null || IsTacticalCombatActive())
            {
                StopAnimatedMovement();
                ApplyMovement(Vector3.zero);
                return;
            }

            Vector3 toLeader =
                leader.position - transform.position;

            toLeader.y = 0f;

            float distance = toLeader.magnitude;
            bool shouldMove = distance > stopDistance;

            if (!shouldMove)
            {
                StopAnimatedMovement();
                ApplyMovement(Vector3.zero);
                return;
            }

            Vector3 direction =
                distance > 0.001f
                    ? toLeader / distance
                    : Vector3.zero;

            bool running = distance >= runDistance;
            float speed = running ? runSpeed : walkSpeed;

            float spacingFactor =
                Mathf.InverseLerp(
                    stopDistance,
                    preferredDistance + 0.8f,
                    distance);

            Vector3 desired =
                direction *
                speed *
                Mathf.Lerp(0.35f, 1f, spacingFactor);

            _planarVelocity =
                Vector3.MoveTowards(
                    _planarVelocity,
                    desired,
                    acceleration * Time.deltaTime);

            if (animator2D != null &&
                !animator2D.IsOneShotPlaying)
            {
                animator2D.FaceWorldDirection(direction);
                animator2D.PlayLoop(
                    running
                        ? CharacterFrameState.Run
                        : CharacterFrameState.Walk);
            }

            ApplyMovement(_planarVelocity);
        }

        private void StopAnimatedMovement()
        {
            _planarVelocity =
                Vector3.MoveTowards(
                    _planarVelocity,
                    Vector3.zero,
                    acceleration * Time.deltaTime);

            if (animator2D != null &&
                !animator2D.IsOneShotPlaying)
            {
                animator2D.PlayLoop(
                    CharacterFrameState.Idle);
            }
        }

        private void ApplyMovement(Vector3 planar)
        {
            if (controller == null)
            {
                transform.position +=
                    planar * Time.deltaTime;

                return;
            }

            if (controller.isGrounded &&
                _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }
            else
            {
                _verticalVelocity -=
                    gravity * Time.deltaTime;
            }

            Vector3 velocity =
                planar +
                Vector3.up * _verticalVelocity;

            controller.Move(
                velocity * Time.deltaTime);
        }

        private static bool IsTacticalCombatActive()
        {
            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            return director != null &&
                   director.State == CombatState.Active;
        }
    }
}
