using KeeperFirstCovenant.Combat;
using UnityEngine;
using UnityEngine.Events;

namespace KeeperFirstCovenant.World
{
    public sealed class EnvironmentalDestructible : MonoBehaviour
    {
        [SerializeField, Min(1f)]
        private float maxIntegrity = 30f;

        [SerializeField]
        private ImpactTier minimumImpactTier =
            ImpactTier.Heavy;

        [SerializeField, Min(0f)]
        private float impactDamageMultiplier = 5f;

        [SerializeField]
        private bool disableCollidersWhenDestroyed = true;

        [SerializeField]
        private bool releaseRigidbodyWhenDestroyed = true;

        [SerializeField]
        private UnityEvent onDestroyed;

        private float _integrity;
        private bool _destroyed;

        public float Integrity => _integrity;
        public bool IsDestroyed => _destroyed;

        private void Awake()
        {
            _integrity = maxIntegrity;
        }

        public void ConfigurePrototype(
            float integrity,
            ImpactTier minimumTier,
            float damageMultiplier)
        {
            maxIntegrity =
                Mathf.Max(
                    1f,
                    integrity);

            minimumImpactTier =
                minimumTier;

            impactDamageMultiplier =
                Mathf.Max(
                    0f,
                    damageMultiplier);

            _integrity = maxIntegrity;
            _destroyed = false;
        }

        public void ApplyImpact(
            ImpactTier tier,
            float force,
            Vector3 impactPoint)
        {
            if (_destroyed ||
                tier < minimumImpactTier)
            {
                return;
            }

            float tierBonus =
                1f +
                (int)tier * 0.35f;

            float damage =
                Mathf.Max(
                    1f,
                    force *
                    impactDamageMultiplier *
                    tierBonus);

            _integrity -= damage;

            if (_integrity > 0f)
                return;

            DestroyObject(
                impactPoint,
                force);
        }

        private void DestroyObject(
            Vector3 impactPoint,
            float force)
        {
            if (_destroyed)
                return;

            _destroyed = true;
            _integrity = 0f;

            if (disableCollidersWhenDestroyed)
            {
                foreach (Collider collider in
                         GetComponentsInChildren<
                             Collider>(true))
                {
                    collider.enabled = false;
                }
            }

            if (releaseRigidbodyWhenDestroyed)
            {
                Rigidbody body =
                    GetComponent<Rigidbody>();

                if (body != null)
                {
                    body.isKinematic = false;

                    Vector3 direction =
                        transform.position -
                        impactPoint;

                    if (direction.sqrMagnitude <
                        0.001f)
                    {
                        direction = Vector3.up;
                    }

                    body.AddForce(
                        direction.normalized *
                        Mathf.Max(1f, force),
                        ForceMode.Impulse);
                }
            }

            onDestroyed?.Invoke();
        }

        public void DebugRestore()
        {
            _destroyed = false;
            _integrity = maxIntegrity;

            if (disableCollidersWhenDestroyed)
            {
                foreach (Collider collider in
                         GetComponentsInChildren<
                             Collider>(true))
                {
                    collider.enabled = true;
                }
            }
        }
    }
}
