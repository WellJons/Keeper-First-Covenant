using System;
using System.Linq;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    public enum CoverQuality
    {
        None,
        Half,
        Full
    }

    public readonly struct LineOfSightResult
    {
        public readonly bool HasLineOfSight;
        public readonly CoverQuality Cover;
        public readonly int VisibleSamples;

        public LineOfSightResult(bool hasLineOfSight, CoverQuality cover, int visibleSamples)
        {
            HasLineOfSight = hasLineOfSight;
            Cover = cover;
            VisibleSamples = visibleSamples;
        }
    }

    [DefaultExecutionOrder(-450)]
    public sealed class TacticalLineOfSight : MonoBehaviour
    {
        public static TacticalLineOfSight Instance { get; private set; }

        [SerializeField] private LayerMask obstructionMask = ~0;
        [SerializeField, Min(0.1f)] private float eyeHeight = 1.25f;
        [SerializeField, Min(0.1f)] private float targetCenterHeight = 0.95f;
        [SerializeField, Min(0.05f)] private float targetSideOffset = 0.32f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public LineOfSightResult Evaluate(
            CombatantRuntime actor,
            CombatantRuntime target)
        {
            if (actor == null || target == null)
                return new LineOfSightResult(false, CoverQuality.Full, 0);

            Vector3 origin = actor.transform.position + Vector3.up * eyeHeight;
            Vector3 targetCenter =
                target.transform.position + Vector3.up * targetCenterHeight;

            Vector3 horizontal =
                target.transform.position - actor.transform.position;
            horizontal.y = 0f;

            Vector3 right = horizontal.sqrMagnitude > 0.001f
                ? Vector3.Cross(Vector3.up, horizontal.normalized)
                : Vector3.right;

            Vector3[] samples =
            {
                targetCenter,
                targetCenter + right * targetSideOffset,
                targetCenter - right * targetSideOffset
            };

            int visible = 0;
            foreach (Vector3 sample in samples)
            {
                if (IsVisible(origin, sample, actor.transform, target.transform))
                    visible++;
            }

            if (visible <= 0)
                return new LineOfSightResult(false, CoverQuality.Full, 0);

            CoverQuality cover = visible == 3
                ? CoverQuality.None
                : visible == 2
                    ? CoverQuality.Half
                    : CoverQuality.Full;

            return new LineOfSightResult(true, cover, visible);
        }

        public bool HasLineOfSightToPoint(
            CombatantRuntime actor,
            Vector3 point)
        {
            if (actor == null)
                return false;

            Vector3 origin = actor.transform.position + Vector3.up * eyeHeight;
            Vector3 destination = point + Vector3.up * 0.15f;

            return IsVisible(origin, destination, actor.transform, null);
        }

        private bool IsVisible(
            Vector3 origin,
            Vector3 destination,
            Transform actorRoot,
            Transform targetRoot)
        {
            Vector3 delta = destination - origin;
            float distance = delta.magnitude;

            if (distance <= 0.01f)
                return true;

            RaycastHit[] hits = Physics.RaycastAll(
                origin,
                delta / distance,
                distance,
                obstructionMask,
                QueryTriggerInteraction.Ignore);

            foreach (RaycastHit hit in hits.OrderBy(x => x.distance))
            {
                Transform hitTransform = hit.collider.transform;

                if (actorRoot != null && IsPartOf(hitTransform, actorRoot))
                    continue;

                if (targetRoot != null && IsPartOf(hitTransform, targetRoot))
                    continue;

                return false;
            }

            return true;
        }

        private static bool IsPartOf(Transform candidate, Transform root)
        {
            if (candidate == null || root == null)
                return false;

            return candidate == root || candidate.IsChildOf(root);
        }
    }
}
