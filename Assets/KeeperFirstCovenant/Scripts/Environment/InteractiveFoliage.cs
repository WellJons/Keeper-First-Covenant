using System.Collections.Generic;
using UnityEngine;

namespace KeeperFirstCovenant.Environment
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class InteractiveFoliage : MonoBehaviour
    {
        private static readonly int PhaseId = Shader.PropertyToID("_Phase");
        private static readonly int BendVectorId = Shader.PropertyToID("_BendVector");
        private static readonly int BendStrengthId = Shader.PropertyToID("_BendStrength");

        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField, Min(0.1f)] private float responseSpeed = 8f;
        [SerializeField, Min(0.1f)] private float recoverySpeed = 3.5f;
        [SerializeField, Range(0f, 1f)] private float maximumBend = 0.52f;
        [SerializeField, Range(0f, 0.25f)] private float tintVariation = 0.055f;
        [SerializeField, Range(0f, 0.2f)] private float scaleVariation = 0.08f;

        private readonly List<Transform> interactors = new List<Transform>();
        private MaterialPropertyBlock propertyBlock;
        private Vector2 currentBend;
        private float currentStrength;
        private Vector3 initialScale;

        public void Configure(
            SpriteRenderer renderer,
            float response,
            float recovery,
            float bend,
            float tint,
            float scale)
        {
            targetRenderer = renderer;
            responseSpeed = Mathf.Max(0.1f, response);
            recoverySpeed = Mathf.Max(0.1f, recovery);
            maximumBend = Mathf.Clamp01(bend);
            tintVariation = Mathf.Clamp(tint, 0f, 0.25f);
            scaleVariation = Mathf.Clamp(scale, 0f, 0.2f);
        }

        private void Awake()
        {
            if (targetRenderer == null)
                targetRenderer = GetComponentInChildren<SpriteRenderer>(true);

            propertyBlock = new MaterialPropertyBlock();
            initialScale = transform.localScale;

            int seed = GetInstanceID();
            float phase = Mathf.Abs(seed * 0.6180339887f) % 6.283185f;
            float scale = 1f + HashSigned(seed + 11) * scaleVariation;
            float tint = 1f + HashSigned(seed + 37) * tintVariation;

            transform.localScale = initialScale * scale;

            if (targetRenderer != null)
            {
                targetRenderer.color = new Color(tint, tint, tint, 1f);
                targetRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat(PhaseId, phase);
                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void Update()
        {
            if (targetRenderer == null)
                return;

            RemoveMissingInteractors();
            Transform nearest = FindNearestInteractor();

            Vector2 targetBend = Vector2.zero;
            float targetStrength = 0f;

            if (nearest != null)
            {
                Vector3 away = transform.position - nearest.position;
                away.y = 0f;

                if (away.sqrMagnitude > 0.0001f)
                {
                    Transform visual = targetRenderer.transform;
                    Vector3 localAway = visual.InverseTransformDirection(away.normalized);
                    targetBend = new Vector2(localAway.x, Mathf.Abs(localAway.z));
                    targetStrength = maximumBend;
                }
            }

            float speed = nearest != null ? responseSpeed : recoverySpeed;
            currentBend = Vector2.Lerp(
                currentBend,
                targetBend,
                1f - Mathf.Exp(-speed * Time.deltaTime));
            currentStrength = Mathf.Lerp(
                currentStrength,
                targetStrength,
                1f - Mathf.Exp(-speed * Time.deltaTime));

            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetVector(
                BendVectorId,
                new Vector4(currentBend.x, currentBend.y, 0f, 0f));
            propertyBlock.SetFloat(BendStrengthId, currentStrength);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }

        private void OnTriggerEnter(Collider other)
        {
            Transform actor = ResolveActor(other);
            if (actor != null && !interactors.Contains(actor))
                interactors.Add(actor);
        }

        private void OnTriggerExit(Collider other)
        {
            Transform actor = ResolveActor(other);
            if (actor != null)
                interactors.Remove(actor);
        }

        private Transform FindNearestInteractor()
        {
            Transform nearest = null;
            float nearestDistance = float.MaxValue;

            for (int i = 0; i < interactors.Count; i++)
            {
                Transform candidate = interactors[i];
                if (candidate == null)
                    continue;

                float distance = (candidate.position - transform.position).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = candidate;
                }
            }

            return nearest;
        }

        private void RemoveMissingInteractors()
        {
            for (int i = interactors.Count - 1; i >= 0; i--)
            {
                if (interactors[i] == null)
                    interactors.RemoveAt(i);
            }
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

        private static float HashSigned(int value)
        {
            uint x = (uint)value;
            x ^= x >> 16;
            x *= 0x7feb352d;
            x ^= x >> 15;
            x *= 0x846ca68b;
            x ^= x >> 16;
            return (x / (float)uint.MaxValue) * 2f - 1f;
        }
    }
}
