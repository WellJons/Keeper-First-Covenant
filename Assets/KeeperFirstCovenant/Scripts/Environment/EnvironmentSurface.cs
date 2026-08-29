using UnityEngine;

namespace KeeperFirstCovenant.Environment
{
    public enum EnvironmentSurfacePattern
    {
        FullPrimary,
        FullSecondary,
        CenterBand,
        Corner,
        Cross,
        TJunction,
        EdgeTransition
    }

    [DisallowMultipleComponent]
    public sealed class EnvironmentSurface : MonoBehaviour
    {
        [SerializeField] private EnvironmentSurfaceProfile primaryProfile;
        [SerializeField] private EnvironmentSurfaceProfile secondaryProfile;
        [SerializeField] private EnvironmentSurfacePattern pattern;
        [SerializeField] private Vector2 tileSize = new Vector2(4f, 4f);
        [SerializeField, Range(0.05f, 0.48f)] private float secondaryHalfWidth = 0.24f;

        public EnvironmentSurfaceProfile PrimaryProfile => primaryProfile;
        public EnvironmentSurfaceProfile SecondaryProfile => secondaryProfile;
        public EnvironmentSurfacePattern Pattern => pattern;

        public void Configure(
            EnvironmentSurfaceProfile primary,
            EnvironmentSurfaceProfile secondary,
            EnvironmentSurfacePattern surfacePattern,
            Vector2 size,
            float halfWidth)
        {
            primaryProfile = primary;
            secondaryProfile = secondary;
            pattern = surfacePattern;
            tileSize = new Vector2(
                Mathf.Max(0.1f, size.x),
                Mathf.Max(0.1f, size.y));
            secondaryHalfWidth = Mathf.Clamp(halfWidth, 0.05f, 0.48f);
        }

        public EnvironmentSurfaceProfile ResolveProfile(Vector3 worldPoint)
        {
            if (secondaryProfile == null)
                return primaryProfile;

            Vector3 local = transform.InverseTransformPoint(worldPoint);
            float normalizedX = local.x / tileSize.x;
            float normalizedZ = local.z / tileSize.y;
            const float junctionGate = 0.075f;

            bool useSecondary;

            switch (pattern)
            {
                case EnvironmentSurfacePattern.FullSecondary:
                    useSecondary = true;
                    break;

                case EnvironmentSurfacePattern.CenterBand:
                    useSecondary = Mathf.Abs(normalizedX) <= secondaryHalfWidth;
                    break;

                case EnvironmentSurfacePattern.Corner:
                    useSecondary =
                        (Mathf.Abs(normalizedX) <= secondaryHalfWidth && normalizedZ <= junctionGate) ||
                        (Mathf.Abs(normalizedZ) <= secondaryHalfWidth && normalizedX >= -junctionGate);
                    break;

                case EnvironmentSurfacePattern.Cross:
                    useSecondary =
                        Mathf.Abs(normalizedX) <= secondaryHalfWidth ||
                        Mathf.Abs(normalizedZ) <= secondaryHalfWidth;
                    break;

                case EnvironmentSurfacePattern.TJunction:
                    useSecondary =
                        Mathf.Abs(normalizedX) <= secondaryHalfWidth ||
                        (Mathf.Abs(normalizedZ) <= secondaryHalfWidth && normalizedX >= -junctionGate);
                    break;

                case EnvironmentSurfacePattern.EdgeTransition:
                    useSecondary = normalizedX >= 0f;
                    break;

                default:
                    useSecondary = false;
                    break;
            }

            return useSecondary ? secondaryProfile : primaryProfile;
        }

        public static bool TryResolveBelow(
            Vector3 worldPosition,
            float rayDistance,
            LayerMask layers,
            out EnvironmentSurface surface,
            out EnvironmentSurfaceProfile profile,
            out RaycastHit hit)
        {
            Vector3 origin = worldPosition + Vector3.up * 0.65f;

            if (Physics.Raycast(
                    origin,
                    Vector3.down,
                    out hit,
                    Mathf.Max(0.7f, rayDistance),
                    layers,
                    QueryTriggerInteraction.Ignore))
            {
                surface = hit.collider.GetComponentInParent<EnvironmentSurface>();
                if (surface != null)
                {
                    profile = surface.ResolveProfile(hit.point);
                    return profile != null;
                }
            }

            surface = null;
            profile = null;
            return false;
        }
    }
}
