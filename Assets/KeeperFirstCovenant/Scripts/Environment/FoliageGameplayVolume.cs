using System.Collections.Generic;
using UnityEngine;

namespace KeeperFirstCovenant.Environment
{
    [DisallowMultipleComponent]
    public sealed class FoliageGameplayVolume : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float concealment;
        [SerializeField, Range(0.1f, 1f)] private float movementMultiplier = 1f;
        [SerializeField, Range(0f, 1f)] private float flammability = 0.8f;

        private readonly HashSet<Transform> occupants = new HashSet<Transform>();

        public float Concealment => concealment;
        public float MovementMultiplier => movementMultiplier;
        public float Flammability => flammability;
        public IReadOnlyCollection<Transform> Occupants => occupants;

        public void Configure(
            float concealmentAmount,
            float speedMultiplier,
            float fireSusceptibility)
        {
            concealment = Mathf.Clamp01(concealmentAmount);
            movementMultiplier = Mathf.Clamp(speedMultiplier, 0.1f, 1f);
            flammability = Mathf.Clamp01(fireSusceptibility);
        }

        public bool Contains(Transform actor)
        {
            if (actor == null)
                return false;

            return occupants.Contains(actor.root);
        }

        private void OnTriggerEnter(Collider other)
        {
            Transform actor = ResolveActor(other);
            if (actor != null)
                occupants.Add(actor);
        }

        private void OnTriggerExit(Collider other)
        {
            Transform actor = ResolveActor(other);
            if (actor != null)
                occupants.Remove(actor);
        }

        private static Transform ResolveActor(Collider other)
        {
            if (other == null)
                return null;

            CharacterController controller = other.GetComponentInParent<CharacterController>();
            if (controller != null)
                return controller.transform.root;

            Rigidbody body = other.attachedRigidbody;
            return body != null ? body.transform.root : null;
        }
    }
}
