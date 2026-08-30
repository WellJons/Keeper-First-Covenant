using System.Collections;
using KeeperFirstCovenant.World;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    public sealed class ElementalReactionVfxController :
        MonoBehaviour
    {
        private Camera worldCamera;
        private CameraImpactShake shake;

        private void OnEnable()
        {
            ElementalSurfaceSystem.ReactionTriggered +=
                OnReactionTriggered;
        }

        private void OnDisable()
        {
            ElementalSurfaceSystem.ReactionTriggered -=
                OnReactionTriggered;
        }

        private void Start()
        {
            ResolveCamera();
        }

        private void OnReactionTriggered(
            ElementalReactionEvent reaction)
        {
            ResolveCamera();

            Color color =
                GetReactionColor(
                    reaction.Kind);

            float intensity =
                GetIntensity(
                    reaction.Kind);

            SpawnBurst(
                reaction.Point +
                Vector3.up * 0.28f,
                color,
                reaction.Radius,
                intensity);

            StartCoroutine(
                FlashLight(
                    reaction.Point,
                    color,
                    reaction.Radius,
                    intensity));

            if (shake != null)
            {
                shake.AddImpulse(
                    0.12f +
                    intensity * 0.09f,
                    0.12f +
                    intensity * 0.05f,
                    24f + intensity * 8f);
            }

            WorldNoiseSystem.Emit(
                reaction.Point,
                Mathf.Max(
                    5f,
                    reaction.Radius * 2.4f),
                reaction.Source,
                0.9f + intensity * 0.25f);
        }

        private void ResolveCamera()
        {
            if (worldCamera == null)
                worldCamera = Camera.main;

            if (worldCamera == null)
                return;

            shake =
                worldCamera.GetComponent<
                    CameraImpactShake>();

            if (shake == null)
            {
                shake =
                    worldCamera.gameObject
                        .AddComponent<
                            CameraImpactShake>();
            }
        }

        private void SpawnBurst(
            Vector3 point,
            Color color,
            float radius,
            float intensity)
        {
            GameObject root =
                new GameObject(
                    "ElementalReactionVFX");

            root.transform.position = point;

            ParticleSystem core =
                root.AddComponent<
                    ParticleSystem>();

            ParticleSystem.MainModule main =
                core.main;

            main.loop = false;
            main.duration = 0.65f;
            main.startLifetime =
                new ParticleSystem.MinMaxCurve(
                    0.32f,
                    0.72f);

            main.startSpeed =
                new ParticleSystem.MinMaxCurve(
                    2.8f + intensity,
                    5.4f + intensity * 2f);

            main.startSize =
                new ParticleSystem.MinMaxCurve(
                    0.08f,
                    0.22f +
                    intensity * 0.05f);

            main.startColor =
                new ParticleSystem.MinMaxGradient(
                    color,
                    Color.Lerp(
                        color,
                        Color.white,
                        0.55f));

            main.maxParticles =
                Mathf.RoundToInt(
                    48f +
                    intensity * 20f);

            ParticleSystem.EmissionModule emission =
                core.emission;

            emission.rateOverTime = 0f;

            emission.SetBurst(
                0,
                new ParticleSystem.Burst(
                    0f,
                    (short)Mathf.RoundToInt(
                        28f +
                        intensity * 16f)));

            ParticleSystem.ShapeModule shape =
                core.shape;

            shape.enabled = true;
            shape.shapeType =
                ParticleSystemShapeType.Sphere;

            shape.radius =
                Mathf.Clamp(
                    radius * 0.18f,
                    0.18f,
                    0.75f);

            ParticleSystem.ColorOverLifetimeModule colorLife =
                core.colorOverLifetime;

            colorLife.enabled = true;

            Gradient gradient =
                new Gradient();

            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(
                        Color.white,
                        0f),
                    new GradientColorKey(
                        color,
                        0.22f),
                    new GradientColorKey(
                        Color.Lerp(
                            color,
                            Color.black,
                            0.25f),
                        1f)
                },
                new[]
                {
                    new GradientAlphaKey(
                        1f,
                        0f),
                    new GradientAlphaKey(
                        0.85f,
                        0.4f),
                    new GradientAlphaKey(
                        0f,
                        1f)
                });

            colorLife.color =
                new ParticleSystem.MinMaxGradient(
                    gradient);

            ParticleSystemRenderer renderer =
                core.GetComponent<
                    ParticleSystemRenderer>();

            Material material =
                CreateParticleMaterial(
                    color);

            if (material != null)
                renderer.material = material;

            core.Play();

            SpawnRing(
                point,
                color,
                radius,
                intensity);

            Destroy(
                root,
                1.4f);

            if (material != null)
                Destroy(
                    material,
                    1.5f);
        }

        private void SpawnRing(
            Vector3 point,
            Color color,
            float radius,
            float intensity)
        {
            GameObject ringObject =
                new GameObject(
                    "ElementalReactionRing");

            ringObject.transform.position =
                point + Vector3.up * 0.05f;

            ParticleSystem ring =
                ringObject.AddComponent<
                    ParticleSystem>();

            ParticleSystem.MainModule main =
                ring.main;

            main.loop = false;
            main.duration = 0.45f;
            main.startLifetime =
                new ParticleSystem.MinMaxCurve(
                    0.28f,
                    0.48f);

            main.startSpeed =
                3.2f +
                intensity * 1.4f;

            main.startSize =
                0.07f +
                intensity * 0.018f;

            main.startColor =
                new ParticleSystem.MinMaxGradient(
                    color);

            ParticleSystem.EmissionModule emission =
                ring.emission;

            emission.rateOverTime = 0f;

            emission.SetBurst(
                0,
                new ParticleSystem.Burst(
                    0f,
                    (short)Mathf.RoundToInt(
                        34f +
                        intensity * 8f)));

            ParticleSystem.ShapeModule shape =
                ring.shape;

            shape.enabled = true;
            shape.shapeType =
                ParticleSystemShapeType.Circle;

            shape.radius =
                Mathf.Clamp(
                    radius * 0.25f,
                    0.2f,
                    0.8f);

            shape.rotation =
                new Vector3(
                    90f,
                    0f,
                    0f);

            ParticleSystemRenderer renderer =
                ring.GetComponent<
                    ParticleSystemRenderer>();

            Material material =
                CreateParticleMaterial(
                    color);

            if (material != null)
                renderer.material = material;

            ring.Play();

            Destroy(
                ringObject,
                1.1f);

            if (material != null)
                Destroy(
                    material,
                    1.2f);
        }

        private IEnumerator FlashLight(
            Vector3 point,
            Color color,
            float radius,
            float intensity)
        {
            GameObject lightObject =
                new GameObject(
                    "ElementalReactionLight");

            lightObject.transform.position =
                point + Vector3.up * 0.55f;

            Light light =
                lightObject.AddComponent<Light>();

            light.type = LightType.Point;
            light.color = color;
            light.range =
                Mathf.Max(
                    4f,
                    radius * 2.2f);

            float peak =
                4.5f +
                intensity * 2.4f;

            light.intensity = peak;

            float duration =
                0.16f +
                intensity * 0.035f;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        elapsed / duration);

                light.intensity =
                    peak *
                    (1f - t) *
                    (1f - t);

                yield return null;
            }

            Destroy(lightObject);
        }

        private static Material
            CreateParticleMaterial(
                Color color)
        {
            Shader shader =
                Shader.Find(
                    "Universal Render Pipeline/Particles/Unlit");

            if (shader == null)
            {
                shader =
                    Shader.Find(
                        "Particles/Standard Unlit");
            }

            if (shader == null)
            {
                shader =
                    Shader.Find(
                        "Sprites/Default");
            }

            if (shader == null)
                return null;

            Material material =
                new Material(shader);

            material.color = color;

            return material;
        }

        private static Color GetReactionColor(
            ElementalReactionKind kind)
        {
            switch (kind)
            {
                case ElementalReactionKind
                    .ConductiveSurge:
                    return new Color(
                        0.22f,
                        0.72f,
                        1f,
                        1f);

                case ElementalReactionKind
                    .FlashFreeze:
                    return new Color(
                        0.58f,
                        0.90f,
                        1f,
                        1f);

                case ElementalReactionKind
                    .ThermalShock:
                    return new Color(
                        1f,
                        0.66f,
                        0.30f,
                        1f);

                case ElementalReactionKind
                    .Combustion:
                    return new Color(
                        1f,
                        0.28f,
                        0.06f,
                        1f);

                case ElementalReactionKind
                    .ArcaneResonance:
                    return new Color(
                        0.72f,
                        0.34f,
                        1f,
                        1f);

                default:
                    return Color.white;
            }
        }

        private static float GetIntensity(
            ElementalReactionKind kind)
        {
            switch (kind)
            {
                case ElementalReactionKind
                    .Combustion:
                    return 2.4f;

                case ElementalReactionKind
                    .ConductiveSurge:
                case ElementalReactionKind
                    .ArcaneResonance:
                    return 1.8f;

                case ElementalReactionKind
                    .ThermalShock:
                    return 1.45f;

                case ElementalReactionKind
                    .FlashFreeze:
                    return 1.2f;

                default:
                    return 1f;
            }
        }
    }
}
