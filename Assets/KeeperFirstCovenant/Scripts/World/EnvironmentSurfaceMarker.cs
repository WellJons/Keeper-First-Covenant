using UnityEngine;

namespace KeeperFirstCovenant.World
{
    public enum EnvironmentSurfaceType
    {
        Dirt = 0,
        Stone = 1,
        Grass = 2,
        Water = 3
    }

    [DisallowMultipleComponent]
    public sealed class EnvironmentSurfaceMarker : MonoBehaviour
    {
        [SerializeField] private EnvironmentSurfaceType surfaceType = EnvironmentSurfaceType.Dirt;
        [SerializeField] private bool wet;
        [SerializeField] private float footstepVolumeMultiplier = 1f;

        public EnvironmentSurfaceType SurfaceType => surfaceType;
        public bool Wet => wet;
        public float FootstepVolumeMultiplier => footstepVolumeMultiplier;

        public void Configure(
            EnvironmentSurfaceType type,
            bool isWet = false,
            float volumeMultiplier = 1f)
        {
            surfaceType = type;
            wet = isWet;
            footstepVolumeMultiplier = Mathf.Clamp(volumeMultiplier, 0f, 2f);
        }
    }
}
