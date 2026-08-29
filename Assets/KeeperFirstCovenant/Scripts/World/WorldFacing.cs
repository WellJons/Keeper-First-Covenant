using UnityEngine;

namespace KeeperFirstCovenant.World
{
    public sealed class WorldFacing : MonoBehaviour
    {
        [SerializeField]
        private Vector3 forward =
            Vector3.forward;

        public Vector3 Forward
        {
            get
            {
                Vector3 flat = forward;
                flat.y = 0f;

                return flat.sqrMagnitude > 0.001f
                    ? flat.normalized
                    : Vector3.forward;
            }
        }

        private void Awake()
        {
            if (forward.sqrMagnitude <= 0.001f)
                forward = transform.forward;
        }

        public void FaceDirection(
            Vector3 direction)
        {
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.001f)
                return;

            forward = direction.normalized;
        }

        public void FacePoint(
            Vector3 point)
        {
            FaceDirection(
                point - transform.position);
        }
    }
}
