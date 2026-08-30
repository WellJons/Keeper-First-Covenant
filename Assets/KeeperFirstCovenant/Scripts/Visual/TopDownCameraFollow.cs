using UnityEngine;

namespace KeeperFirstCovenant.Visual
{
    [DisallowMultipleComponent]
    public sealed class TopDownCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;

        [Header("Position")]
        [SerializeField] private Vector3 worldOffset =
            new Vector3(9.2f, 10.7f, -11.5f);

        [SerializeField] private float positionSmoothTime = 0.16f;
        [SerializeField] private float maxFollowSpeed = 60f;

        [Header("Look")]
        [SerializeField] private Vector3 lookOffset =
            new Vector3(0f, 0.65f, 0.4f);

        [SerializeField] private float rotationSharpness = 12f;

        private Vector3 _velocity;

        public Transform Target
        {
            get => target;
            set
            {
                target = value;
                Snap();
            }
        }

        public void Configure(
            Transform followTarget,
            Vector3 offset,
            Vector3 lookAtOffset)
        {
            target = followTarget;
            worldOffset = offset;
            lookOffset = lookAtOffset;
            Snap();
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            Vector3 desired =
                target.position + worldOffset;

            transform.position =
                Vector3.SmoothDamp(
                    transform.position,
                    desired,
                    ref _velocity,
                    Mathf.Max(0.01f, positionSmoothTime),
                    Mathf.Max(1f, maxFollowSpeed),
                    Time.deltaTime);

            Vector3 lookPoint =
                target.position + lookOffset;

            Vector3 direction =
                lookPoint - transform.position;

            if (direction.sqrMagnitude < 0.0001f)
                return;

            Quaternion desiredRotation =
                Quaternion.LookRotation(
                    direction.normalized,
                    Vector3.up);

            float t =
                1f -
                Mathf.Exp(
                    -Mathf.Max(0.01f, rotationSharpness) *
                    Time.deltaTime);

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    desiredRotation,
                    t);
        }

        public void Snap()
        {
            if (target == null)
                return;

            _velocity = Vector3.zero;
            transform.position =
                target.position + worldOffset;

            Vector3 direction =
                target.position +
                lookOffset -
                transform.position;

            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.rotation =
                    Quaternion.LookRotation(
                        direction.normalized,
                        Vector3.up);
            }
        }
    }
}
