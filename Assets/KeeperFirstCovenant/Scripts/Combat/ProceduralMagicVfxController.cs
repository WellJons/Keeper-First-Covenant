using System.Collections;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    public sealed class ProceduralMagicVfxController :
        MonoBehaviour
    {
        private void OnEnable()
        {
            CombatActionExecutor.ActionPresentationRequested +=
                OnActionPresentation;
        }

        private void OnDisable()
        {
            CombatActionExecutor.ActionPresentationRequested -=
                OnActionPresentation;
        }

        private void OnActionPresentation(
            CombatPresentationRequest request)
        {
            CombatActionDefinition action =
                request.Action;

            if (action == null ||
                !ShouldRenderFallback(action))
            {
                return;
            }

            Color color =
                GetDamageColor(
                    action.damageType);

            StartCoroutine(
                BeamRoutine(
                    request.Origin +
                    Vector3.up * 1.0f,
                    request.ImpactPoint +
                    Vector3.up * 0.35f,
                    color,
                    action.areaRadius));

            SpawnBurst(
                request.Origin +
                Vector3.up * 0.9f,
                color,
                0.55f,
                16);

            SpawnBurst(
                request.ImpactPoint +
                Vector3.up * 0.28f,
                Color.Lerp(
                    color,
                    Color.white,
                    0.24f),
                Mathf.Max(
                    0.8f,
                    action.areaRadius),
                action.areaRadius > 0.1f
                    ? 34
                    : 24);

            StartCoroutine(
                ImpactLight(
                    request.ImpactPoint,
                    color,
                    action.areaRadius));
        }

        private static bool ShouldRenderFallback(
            CombatActionDefinition action)
        {
            bool magical =
                action.category ==
                    CombatActionCategory.Spell ||
                action.category ==
                    CombatActionCategory.Control ||
                action.category ==
                    CombatActionCategory.Unique ||
                (action.damageType !=
                     DamageType.Physical &&
                 action.damageType !=
                     DamageType.Bleeding);

            if (!magical)
                return false;

            CombatPresentationProfile profile =
                action.presentationProfile;

            if (profile == null)
                return true;

            return
                profile.castVfxPrefab == null &&
                profile.impactVfxPrefab == null;
        }

        private IEnumerator BeamRoutine(
            Vector3 origin,
            Vector3 impact,
            Color color,
            float areaRadius)
        {
            GameObject beamObject =
                new GameObject(
                    "ProceduralMagicBeam");

            LineRenderer line =
                beamObject.AddComponent<
                    LineRenderer>();

            Material material =
                CreateLineMaterial(color);

            line.useWorldSpace = true;
            line.positionCount = 2;
            line.widthMultiplier =
                areaRadius > 0.1f
                    ? 0.11f
                    : 0.075f;

            line.numCapVertices = 4;
            line.numCornerVertices = 4;
            line.shadowCastingMode =
                UnityEngine.Rendering
                    .ShadowCastingMode.Off;

            line.receiveShadows = false;

            if (material != null)
                line.material = material;

            line.startColor =
                Color.Lerp(
                    color,
                    Color.white,
                    0.55f);

            line.endColor = color;

            float distance =
                Vector3.Distance(
                    origin,
                    impact);

            float duration =
                Mathf.Clamp(
                    distance / 48f,
                    0.08f,
                    0.22f);

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        elapsed / duration);

                float eased =
                    1f -
                    Mathf.Pow(
                        1f - t,
                        2f);

                Vector3 head =
                    Vector3.Lerp(
                        origin,
                        impact,
                        eased);

                Vector3 direction =
                    impact - origin;

                Vector3 side =
                    Vector3.Cross(
                        direction.normalized,
                        Vector3.up);

                float wave =
                    Mathf.Sin(
                        t *
                        Mathf.PI *
                        5f) *
                    0.05f *
                    Mathf.Clamp(
                        distance,
                        0f,
                        8f);

                head += side * wave;

                line.SetPosition(
                    0,
                    origin);

                line.SetPosition(
                    1,
                    head);

                yield return null;
            }

            line.SetPosition(1, impact);

            float fade = 0f;

            while (fade < 0.12f)
            {
                fade +=
                    Time.unscaledDeltaTime;

                float alpha =
                    1f -
                    Mathf.Clamp01(
                        fade / 0.12f);

                Color start =
                    line.startColor;

                Color end =
                    line.endColor;

                start.a = alpha;
                end.a = alpha;

                line.startColor = start;
                line.endColor = end;

                yield return null;
            }

            Destroy(beamObject);

            if (material != null)
                Destroy(material);
        }

        private void SpawnBurst(
            Vector3 point,
            Color color,
            float radius,
            int count)
        {
            GameObject root =
                new GameObject(
                    "ProceduralMagicBurst");

            root.transform.position =
                point;

            ParticleSystem particles =
                root.AddComponent<
                    ParticleSystem>();

            ParticleSystem.MainModule main =
                particles.main;

            main.loop = false;
            main.duration = 0.45f;

            main.startLifetime =
                new ParticleSystem.MinMaxCurve(
                    0.28f,
                    0.72f);

            main.startSpeed =
                new ParticleSystem.MinMaxCurve(
                    1.6f,
                    4.4f);

            main.startSize =
                new ParticleSystem.MinMaxCurve(
                    0.035f,
                    0.16f);

            main.startColor =
                new ParticleSystem.MinMaxGradient(
                    color,
                    Color.Lerp(
                        color,
                        Color.white,
                        0.45f));

            main.maxParticles =
                Mathf.Max(
                    24,
                    count + 10);

            ParticleSystem.EmissionModule emission =
                particles.emission;

            emission.rateOverTime = 0f;

            emission.SetBurst(
                0,
                new ParticleSystem.Burst(
                    0f,
                    (short)Mathf.Clamp(
                        count,
                        4,
                        120)));

            ParticleSystem.ShapeModule shape =
                particles.shape;

            shape.enabled = true;
            shape.shapeType =
                ParticleSystemShapeType.Sphere;

            shape.radius =
                Mathf.Clamp(
                    radius * 0.22f,
                    0.12f,
                    0.75f);

            ParticleSystemRenderer renderer =
                particles.GetComponent<
                    ParticleSystemRenderer>();

            Material material =
                CreateParticleMaterial(color);

            if (material != null)
                renderer.material = material;

            particles.Play();

            Destroy(root, 1.3f);

            if (material != null)
                Destroy(material, 1.4f);
        }

        private IEnumerator ImpactLight(
            Vector3 point,
            Color color,
            float areaRadius)
        {
            GameObject lightObject =
                new GameObject(
                    "ProceduralMagicImpactLight");

            lightObject.transform.position =
                point + Vector3.up * 0.45f;

            Light light =
                lightObject.AddComponent<
                    Light>();

            light.type = LightType.Point;
            light.color = color;

            light.range =
                Mathf.Max(
                    4.5f,
                    areaRadius * 2.1f);

            float peak =
                areaRadius > 0.1f
                    ? 6.8f
                    : 5.0f;

            light.intensity = peak;

            float elapsed = 0f;
            const float duration = 0.18f;

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

        private static Color GetDamageColor(
            DamageType type)
        {
            switch (type)
            {
                case DamageType.Fire:
                    return new Color(
                        1f,
                        0.22f,
                        0.035f,
                        1f);

                case DamageType.Frost:
                    return new Color(
                        0.42f,
                        0.86f,
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
                        0.67f,
                        0.28f,
                        1f,
                        1f);

                case DamageType.Radiant:
                    return new Color(
                        1f,
                        0.82f,
                        0.34f,
                        1f);

                case DamageType.Poison:
                    return new Color(
                        0.34f,
                        0.92f,
                        0.18f,
                        1f);

                default:
                    return new Color(
                        0.82f,
                        0.88f,
                        0.96f,
                        1f);
            }
        }

        private static Material CreateLineMaterial(
            Color color)
        {
            Shader shader =
                Shader.Find(
                    "Sprites/Default");

            if (shader == null)
                return null;

            Material material =
                new Material(shader);

            material.color = color;
            return material;
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
    }
}
