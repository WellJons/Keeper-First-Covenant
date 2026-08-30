using UnityEngine;

namespace KeeperFirstCovenant.World
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class WorldPhysicsNoiseEmitter :
        MonoBehaviour
    {
        [SerializeField, Min(0.1f)]
        private float minimumImpactSpeed = 1.4f;

        [SerializeField, Min(0f)]
        private float baseNoiseRadius = 2.2f;

        [SerializeField, Min(0f)]
        private float radiusPerImpactSpeed = 1.15f;

        [SerializeField, Min(0.5f)]
        private float maximumNoiseRadius = 12f;

        [SerializeField, Min(0.02f)]
        private float cooldown = 0.18f;

        [SerializeField, Range(0.1f, 3f)]
        private float intensityMultiplier = 1f;

        private Rigidbody body;
        private float nextAllowedNoise;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(
            Collision collision)
        {
            if (body == null ||
                body.isKinematic ||
                Time.unscaledTime <
                    nextAllowedNoise)
            {
                return;
            }

            float speed =
                collision.relativeVelocity
                    .magnitude;

            if (speed < minimumImpactSpeed)
                return;

            nextAllowedNoise =
                Time.unscaledTime +
                cooldown;

            float radius =
                Mathf.Clamp(
                    baseNoiseRadius +
                    (speed -
                     minimumImpactSpeed) *
                    radiusPerImpactSpeed,
                    0f,
                    maximumNoiseRadius);

            Vector3 point =
                collision.contactCount > 0
                    ? collision
                        .GetContact(0)
                        .point
                    : transform.position;

            float intensity =
                Mathf.Clamp(
                    speed /
                    Mathf.Max(
                        1f,
                        minimumImpactSpeed * 2f) *
                    intensityMultiplier,
                    0.35f,
                    2.5f);

            WorldNoiseSystem.Emit(
                point,
                radius,
                gameObject,
                intensity);
        }
    }
}
