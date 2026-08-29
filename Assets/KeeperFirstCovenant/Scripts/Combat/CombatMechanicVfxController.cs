using System.Collections;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    public sealed class CombatMechanicVfxController :
        MonoBehaviour
    {
        private Camera worldCamera;
        private CameraImpactShake shake;

        private void OnEnable()
        {
            CombatActionExecutor
                .ActionPresentationRequested +=
                OnActionPresentation;

            ActiveDefenseSystem.DefenseResolved +=
                OnDefenseResolved;

            BreakGaugeComponent.Broken +=
                OnBroken;
        }

        private void OnDisable()
        {
            CombatActionExecutor
                .ActionPresentationRequested -=
                OnActionPresentation;

            ActiveDefenseSystem.DefenseResolved -=
                OnDefenseResolved;

            BreakGaugeComponent.Broken -=
                OnBroken;
        }

        private void OnActionPresentation(
            CombatPresentationRequest request)
        {
            if (!request.Result.ComboTriggered)
                return;

            float intensity =
                Mathf.Clamp(
                    1f +
                    request.Result.ComboDepth *
                    0.28f,
                    1.3f,
                    2.4f);

            Color color =
                GetDamageColor(
                    request.Action != null
                        ? request.Action.damageType
                        : DamageType.Arcane);

            SpawnBurst(
                request.ImpactPoint +
                Vector3.up * 0.35f,
                Color.Lerp(
                    color,
                    Color.white,
                    0.42f),
                34 +
                request.Result.ComboDepth * 10,
                intensity);

            StartCoroutine(
                FlashLight(
                    request.ImpactPoint,
                    color,
                    7f + intensity * 2f,
                    6f + intensity * 2.5f,
                    0.18f));

            AddShake(
                0.14f +
                request.Result.ComboDepth *
                0.035f,
                0.15f,
                31f);
        }

        private void OnDefenseResolved(
            CombatantRuntime target,
            ActiveDefenseOutcome outcome)
        {
            if (target == null)
                return;

            Color color;
            int count;
            float intensity;
            float shakeAmount;

            switch (outcome)
            {
                case ActiveDefenseOutcome
                    .PerfectParry:
                    color =
                        new Color(
                            1f,
                            0.82f,
                            0.42f,
                            1f);
                    count = 52;
                    intensity = 2.1f;
                    shakeAmount = 0.22f;
                    break;

                case ActiveDefenseOutcome
                    .PerfectDodge:
                    color =
                        new Color(
                            0.58f,
                            0.90f,
                            1f,
                            1f);
                    count = 42;
                    intensity = 1.65f;
                    shakeAmount = 0.11f;
                    break;

                case ActiveDefenseOutcome
                    .Parry:
                    color =
                        new Color(
                            0.92f,
                            0.69f,
                            0.34f,
                            1f);
                    count = 26;
                    intensity = 1.15f;
                    shakeAmount = 0.10f;
                    break;

                case ActiveDefenseOutcome
                    .Dodge:
                    color =
                        new Color(
                            0.48f,
                            0.78f,
                            1f,
                            1f);
                    count = 20;
                    intensity = 0.9f;
                    shakeAmount = 0.06f;
                    break;

                default:
                    return;
            }

            Vector3 point =
                target.transform.position +
                Vector3.up * 1.0f;

            SpawnBurst(
                point,
                color,
                count,
                intensity);

            StartCoroutine(
                FlashLight(
                    point,
                    color,
                    5f + intensity * 1.5f,
                    4f + intensity * 2f,
                    0.13f));

            AddShake(
                shakeAmount,
                0.12f,
                36f);
        }

        private void OnBroken(
            BreakGaugeComponent gauge)
        {
            if (gauge == null ||
                gauge.Owner == null)
            {
                return;
            }

            Vector3 point =
                gauge.Owner.transform.position +
                Vector3.up * 0.9f;

            Color color =
                gauge.Owner.Faction ==
                    CombatFaction.Enemy
                    ? new Color(
                        1f,
                        0.34f,
                        0.16f,
                        1f)
                    : new Color(
                        0.82f,
                        0.88f,
                        0.96f,
                        1f);

            SpawnBurst(
                point,
                color,
                64,
                2.25f);

            StartCoroutine(
                FlashLight(
                    point,
                    color,
                    8f,
                    10f,
                    0.20f));

            AddShake(
                0.27f,
                0.24f,
                28f);
        }

        private void SpawnBurst(
            Vector3 point,
            Color color,
            int count,
            float intensity)
        {
            GameObject root =
                new GameObject(
                    "CombatMechanicBurst");

            root.transform.position = point;

            ParticleSystem particles =
                root.AddComponent<
                    ParticleSystem>();

            ParticleSystem.MainModule main =
                particles.main;

            main.loop = false;
            main.duration = 0.45f;

            main.startLifetime =
                new ParticleSystem.MinMaxCurve(
                    0.25f,
                    0.75f);

            main.startSpeed =
                new ParticleSystem.MinMaxCurve(
                    2.2f * intensity,
                    4.6f * intensity);

            main.startSize =
                new ParticleSystem.MinMaxCurve(
                    0.035f,
                    0.15f * intensity);

            main.startColor =
                new ParticleSystem.MinMaxGradient(
                    color,
                    Color.Lerp(
                        color,
                        Color.white,
                        0.55f));

            main.maxParticles =
                Mathf.Clamp(
                    count + 20,
                    30,
                    150);

            ParticleSystem.EmissionModule emission =
                particles.emission;

            emission.rateOverTime = 0f;

            emission.SetBurst(
                0,
                new ParticleSystem.Burst(
                    0f,
                    (short)Mathf.Clamp(
                        count,
                        8,
                        120)));

            ParticleSystem.ShapeModule shape =
                particles.shape;

            shape.enabled = true;
            shape.shapeType =
                ParticleSystemShapeType.Sphere;

            shape.radius =
                0.22f +
                intensity * 0.12f;

            ParticleSystem.VelocityOverLifetimeModule velocity =
                particles.velocityOverLifetime;

            velocity.enabled = true;
            velocity.y =
                new ParticleSystem.MinMaxCurve(
                    -0.2f,
                    1.2f * intensity);

            ParticleSystem.ColorOverLifetimeModule colorLife =
                particles.colorOverLifetime;

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
                        0.2f),
                    new GradientColorKey(
                        color,
                        1f)
                },
                new[]
                {
                    new GradientAlphaKey(
                        1f,
                        0f),
                    new GradientAlphaKey(
                        0.8f,
                        0.45f),
                    new GradientAlphaKey(
                        0f,
                        1f)
                });

            colorLife.color =
                new ParticleSystem.MinMaxGradient(
                    gradient);

            ParticleSystemRenderer renderer =
                particles.GetComponent<
                    ParticleSystemRenderer>();

            Material material =
                CreateParticleMaterial(
                    color);

            if (material != null)
                renderer.material = material;

            particles.Play();

            Destroy(root, 1.4f);

            if (material != null)
                Destroy(material, 1.5f);
        }

        private IEnumerator FlashLight(
            Vector3 point,
            Color color,
            float range,
            float peak,
            float duration)
        {
            GameObject root =
                new GameObject(
                    "CombatMechanicLight");

            root.transform.position =
                point;

            Light light =
                root.AddComponent<Light>();

            light.type = LightType.Point;
            light.color = color;
            light.range = range;
            light.intensity = peak;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        elapsed /
                        Mathf.Max(
                            0.01f,
                            duration));

                light.intensity =
                    peak *
                    (1f - t) *
                    (1f - t);

                yield return null;
            }

            Destroy(root);
        }

        private void AddShake(
            float amplitude,
            float duration,
            float frequency)
        {
            ResolveCamera();

            shake?.AddImpulse(
                amplitude,
                duration,
                frequency);
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

        private static Color GetDamageColor(
            DamageType type)
        {
            switch (type)
            {
                case DamageType.Fire:
                    return new Color(
                        1f,
                        0.24f,
                        0.04f,
                        1f);

                case DamageType.Frost:
                    return new Color(
                        0.48f,
                        0.88f,
                        1f,
                        1f);

                case DamageType.Lightning:
                    return new Color(
                        0.24f,
                        0.70f,
                        1f,
                        1f);

                case DamageType.Arcane:
                    return new Color(
                        0.72f,
                        0.30f,
                        1f,
                        1f);

                case DamageType.Radiant:
                    return new Color(
                        1f,
                        0.84f,
                        0.38f,
                        1f);

                case DamageType.Poison:
                    return new Color(
                        0.40f,
                        0.92f,
                        0.18f,
                        1f);

                default:
                    return new Color(
                        0.88f,
                        0.90f,
                        0.94f,
                        1f);
            }
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
                shader = Shader.Find(
                    "Sprites/Default");

            if (shader == null)
                return null;

            Material material =
                new Material(shader);

            material.color = color;

            return material;
        }
    }
}
