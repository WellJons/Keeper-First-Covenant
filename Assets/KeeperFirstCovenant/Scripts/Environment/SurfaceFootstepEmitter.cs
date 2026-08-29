using UnityEngine;

namespace KeeperFirstCovenant.Environment
{
    [DisallowMultipleComponent]
    public sealed class SurfaceFootstepEmitter : MonoBehaviour
    {
        [SerializeField] private LayerMask surfaceLayers = ~0;
        [SerializeField, Min(0.7f)] private float rayDistance = 2.2f;
        [SerializeField, Min(0.001f)] private float minimumMovement = 0.015f;
        [SerializeField] private Material particleMaterial;

        private ParticleSystem particles;
        private Vector3 previousPosition;
        private float accumulatedDistance;
        private bool initialized;

        public EnvironmentSurfaceProfile CurrentProfile { get; private set; }

        public void Configure(Material stepParticleMaterial, LayerMask layers)
        {
            particleMaterial = stepParticleMaterial;
            surfaceLayers = layers;

            if (particles != null)
                particles.GetComponent<ParticleSystemRenderer>().sharedMaterial = particleMaterial;
        }

        private void OnEnable()
        {
            previousPosition = transform.position;
            accumulatedDistance = 0f;
            initialized = true;
            EnsureParticleSystem();
        }

        private void LateUpdate()
        {
            if (!initialized)
            {
                previousPosition = transform.position;
                initialized = true;
                return;
            }

            Vector3 current = transform.position;
            Vector3 delta = current - previousPosition;
            delta.y = 0f;
            previousPosition = current;

            float distance = delta.magnitude;
            if (distance < minimumMovement)
                return;

            accumulatedDistance += distance;

            if (!EnvironmentSurface.TryResolveBelow(
                    current,
                    rayDistance,
                    surfaceLayers,
                    out EnvironmentSurface _,
                    out EnvironmentSurfaceProfile profile,
                    out RaycastHit hit))
            {
                CurrentProfile = null;
                return;
            }

            CurrentProfile = profile;

            float stepDistance = Mathf.Max(0.1f, profile.StepDistance);
            while (accumulatedDistance >= stepDistance)
            {
                accumulatedDistance -= stepDistance;
                EmitStep(profile, hit.point, delta.normalized);
            }
        }

        private void EmitStep(
            EnvironmentSurfaceProfile profile,
            Vector3 point,
            Vector3 movementDirection)
        {
            EnsureParticleSystem();
            if (particles == null)
                return;

            int count = Random.Range(
                profile.MinimumStepParticles,
                profile.MaximumStepParticles + 1);

            for (int i = 0; i < count; i++)
            {
                float size = Random.Range(
                    profile.StepParticleSize.x,
                    profile.StepParticleSize.y);
                float speed = Random.Range(
                    profile.StepParticleSpeed.x,
                    profile.StepParticleSpeed.y);

                Vector2 spread = Random.insideUnitCircle * 0.12f;
                Vector3 position = point + new Vector3(spread.x, 0.025f, spread.y);
                Vector3 lateral = Vector3.Cross(Vector3.up, movementDirection) * Random.Range(-0.45f, 0.45f);
                Vector3 velocity =
                    Vector3.up * speed +
                    movementDirection * Random.Range(-0.08f, 0.16f) +
                    lateral;

                ParticleSystem.EmitParams emit = new ParticleSystem.EmitParams
                {
                    position = position,
                    velocity = velocity,
                    startColor = profile.StepParticleColor,
                    startSize = size,
                    startLifetime = Random.Range(0.28f, 0.52f)
                };

                particles.Emit(emit, 1);
            }
        }

        private void EnsureParticleSystem()
        {
            if (particles != null)
                return;

            Transform existing = transform.Find("SurfaceStepParticles");
            GameObject particleObject;

            if (existing != null)
            {
                particleObject = existing.gameObject;
            }
            else
            {
                particleObject = new GameObject("SurfaceStepParticles");
                particleObject.transform.SetParent(transform, false);
            }

            particles = particleObject.GetComponent<ParticleSystem>();
            if (particles == null)
                particles = particleObject.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 0.65f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = 0.4f;
            main.startSpeed = 0.3f;
            main.startSize = 0.045f;
            main.gravityModifier = 0.24f;
            main.maxParticles = 96;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = false;

            ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
            color.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.9f, 0f),
                    new GradientAlphaKey(0.45f, 0.58f),
                    new GradientAlphaKey(0f, 1f)
                });
            color.color = new ParticleSystem.MinMaxGradient(gradient);

            ParticleSystemRenderer renderer =
                particleObject.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = particleMaterial;
            renderer.sortingOrder = 12;
        }
    }
}
