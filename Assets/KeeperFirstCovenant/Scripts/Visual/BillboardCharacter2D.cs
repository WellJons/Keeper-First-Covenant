using UnityEngine;

namespace KeeperFirstCovenant.Visual
{
    public sealed class BillboardCharacter2D : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private bool lockRoll = true;

        private void LateUpdate()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            if (targetCamera == null)
                return;

            Vector3 forward = targetCamera.transform.forward;
            forward.y = 0f;

            if (forward.sqrMagnitude < 0.0001f)
                return;

            Quaternion rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);

            if (lockRoll)
            {
                Vector3 euler = rotation.eulerAngles;
                rotation = Quaternion.Euler(0f, euler.y, 0f);
            }

            transform.rotation = rotation;
        }
    }
}
