using UnityEngine;

namespace KeeperFirstCovenant.Environment
{
    public enum EnvironmentSurfaceKind
    {
        MeadowGrass,
        WoodlandDirt,
        NaturalStone,
        PackedDirtRoad,
        OldCobblestone
    }

    [CreateAssetMenu(
        menuName = "Keeper First Covenant/Environment/Surface Profile",
        fileName = "EnvironmentSurfaceProfile")]
    public sealed class EnvironmentSurfaceProfile : ScriptableObject
    {
        [SerializeField] private string stableId = "surface";
        [SerializeField] private EnvironmentSurfaceKind kind;
        [SerializeField, Min(0.1f)] private float movementMultiplier = 1f;
        [SerializeField, Range(0f, 1f)] private float staticFriction = 0.7f;
        [SerializeField, Range(0f, 1f)] private float dynamicFriction = 0.55f;
        [SerializeField, Min(0.1f)] private float stepDistance = 0.78f;
        [SerializeField] private Color stepParticleColor = Color.white;
        [SerializeField, Min(0)] private int minimumStepParticles = 3;
        [SerializeField, Min(1)] private int maximumStepParticles = 6;
        [SerializeField] private Vector2 stepParticleSize = new Vector2(0.025f, 0.065f);
        [SerializeField] private Vector2 stepParticleSpeed = new Vector2(0.15f, 0.42f);

        public string StableId => stableId;
        public EnvironmentSurfaceKind Kind => kind;
        public float MovementMultiplier => movementMultiplier;
        public float StaticFriction => staticFriction;
        public float DynamicFriction => dynamicFriction;
        public float StepDistance => stepDistance;
        public Color StepParticleColor => stepParticleColor;
        public int MinimumStepParticles => minimumStepParticles;
        public int MaximumStepParticles => Mathf.Max(minimumStepParticles, maximumStepParticles);
        public Vector2 StepParticleSize => stepParticleSize;
        public Vector2 StepParticleSpeed => stepParticleSpeed;

        public void Configure(
            string id,
            EnvironmentSurfaceKind surfaceKind,
            float speedMultiplier,
            float staticFrictionValue,
            float dynamicFrictionValue,
            float distancePerStep,
            Color particleColor,
            int minParticles,
            int maxParticles,
            Vector2 particleSize,
            Vector2 particleSpeed)
        {
            stableId = id;
            kind = surfaceKind;
            movementMultiplier = Mathf.Max(0.1f, speedMultiplier);
            staticFriction = Mathf.Clamp01(staticFrictionValue);
            dynamicFriction = Mathf.Clamp01(dynamicFrictionValue);
            stepDistance = Mathf.Max(0.1f, distancePerStep);
            stepParticleColor = particleColor;
            minimumStepParticles = Mathf.Max(0, minParticles);
            maximumStepParticles = Mathf.Max(minimumStepParticles, maxParticles);
            stepParticleSize = particleSize;
            stepParticleSpeed = particleSpeed;
        }
    }
}
