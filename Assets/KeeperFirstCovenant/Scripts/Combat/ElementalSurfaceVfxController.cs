using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    public sealed class ElementalSurfaceVfxController :
        MonoBehaviour
    {
        private sealed class SurfaceVisual
        {
            public GameObject root;
            public LineRenderer outer;
            public LineRenderer inner;
            public ParticleSystem particles;
            public Material outerMaterial;
            public Material innerMaterial;
            public Material particleMaterial;
            public SurfaceType lastType;
            public float lastRadius;
        }

        private readonly Dictionary<int, SurfaceVisual>
            visuals =
                new Dictionary<int, SurfaceVisual>();

        private void Update()
        {
            ElementalSurfaceSystem system =
                ElementalSurfaceSystem.Instance;

            if (system == null)
            {
                ClearAll();
                return;
            }

            HashSet<int> alive =
                new HashSet<int>();

            foreach (SurfacePatch patch
                     in system.Patches)
            {
                if (patch == null)
                    continue;

                alive.Add(patch.id);

                if (!visuals.TryGetValue(
                        patch.id,
                        out SurfaceVisual visual))
                {
                    visual =
                        CreateVisual(patch);

                    visuals[patch.id] =
                        visual;
                }

                UpdateVisual(
                    visual,
                    patch);
            }

            foreach (int id in visuals.Keys
                         .Where(value =>
                             !alive.Contains(value))
                         .ToArray())
            {
                DestroyVisual(
                    visuals[id]);

                visuals.Remove(id);
            }
        }

        private SurfaceVisual CreateVisual(
            SurfacePatch patch)
        {
            GameObject root =
                new GameObject(
                    "SurfaceVFX_" +
                    patch.id);

            SurfaceVisual visual =
                new SurfaceVisual
                {
                    root = root
                };

            visual.outer =
                root.AddComponent<
                    LineRenderer>();

            GameObject innerObject =
                new GameObject(
                    "InnerRune");

            innerObject.transform.SetParent(
                root.transform,
                false);

            visual.inner =
                innerObject.AddComponent<
                    LineRenderer>();

            GameObject particleObject =
                new GameObject(
                    "Motes");

            particleObject.transform.SetParent(
                root.transform,
                false);

            visual.particles =
                particleObject.AddComponent<
                    ParticleSystem>();

            ConfigureLine(
                visual.outer,
                0.055f);

            ConfigureLine(
                visual.inner,
                0.022f);

            ConfigureParticles(
                visual.particles);

            return visual;
        }

        private void UpdateVisual(
            SurfaceVisual visual,
            SurfacePatch patch)
        {
            visual.root.transform.position =
                patch.center +
                Vector3.up * 0.035f;

            if (visual.lastType != patch.type)
            {
                Color color =
                    GetSurfaceColor(
                        patch.type);

                RebuildMaterials(
                    visual,
                    color);

                visual.lastType =
                    patch.type;
            }

            if (Mathf.Abs(
                    visual.lastRadius -
                    patch.radius) >
                0.01f)
            {
                DrawIrregularRing(
                    visual.outer,
                    patch.radius,
                    patch.id,
                    0.085f);

                DrawIrregularRing(
                    visual.inner,
                    patch.radius * 0.68f,
                    patch.id * 17,
                    0.06f);

                ParticleSystem.ShapeModule shape =
                    visual.particles.shape;

                shape.radius =
                    Mathf.Max(
                        0.2f,
                        patch.radius * 0.72f);

                visual.lastRadius =
                    patch.radius;
            }

            float pulse =
                0.78f +
                Mathf.Sin(
                    Time.unscaledTime *
                    3.2f +
                    patch.id) *
                0.12f;

            visual.outer.widthMultiplier =
                0.055f * pulse;

            visual.inner.widthMultiplier =
                0.022f *
                (1.15f - pulse * 0.25f);

            visual.inner.transform
                .localRotation =
                    Quaternion.Euler(
                        0f,
                        Time.unscaledTime *
                        (patch.id % 2 == 0
                            ? 11f
                            : -9f),
                        0f);
        }

        private static void ConfigureLine(
            LineRenderer line,
            float width)
        {
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 64;
            line.widthMultiplier = width;
            line.numCornerVertices = 2;
            line.numCapVertices = 2;
            line.shadowCastingMode =
                UnityEngine.Rendering
                    .ShadowCastingMode.Off;

            line.receiveShadows = false;
        }

        private static void ConfigureParticles(
            ParticleSystem particles)
        {
            ParticleSystem.MainModule main =
                particles.main;

            main.loop = true;
            main.duration = 1f;
            main.startLifetime =
                new ParticleSystem.MinMaxCurve(
                    0.8f,
                    1.7f);

            main.startSpeed =
                new ParticleSystem.MinMaxCurve(
                    0.05f,
                    0.42f);

            main.startSize =
                new ParticleSystem.MinMaxCurve(
                    0.035f,
                    0.11f);

            main.maxParticles = 70;
            main.simulationSpace =
                ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission =
                particles.emission;

            emission.rateOverTime = 18f;

            ParticleSystem.ShapeModule shape =
                particles.shape;

            shape.enabled = true;
            shape.shapeType =
                ParticleSystemShapeType.Circle;

            shape.radius = 1f;
            shape.rotation =
                new Vector3(
                    90f,
                    0f,
                    0f);

            ParticleSystem.VelocityOverLifetimeModule velocity =
                particles.velocityOverLifetime;

            velocity.enabled = true;
            velocity.y =
                new ParticleSystem.MinMaxCurve(
                    0.08f,
                    0.42f);

            particles.Play();
        }

        private void RebuildMaterials(
            SurfaceVisual visual,
            Color color)
        {
            DestroyMaterial(
                visual.outerMaterial);

            DestroyMaterial(
                visual.innerMaterial);

            DestroyMaterial(
                visual.particleMaterial);

            visual.outerMaterial =
                CreateMaterial(
                    color);

            visual.innerMaterial =
                CreateMaterial(
                    Color.Lerp(
                        color,
                        Color.white,
                        0.45f));

            visual.particleMaterial =
                CreateParticleMaterial(
                    color);

            if (visual.outerMaterial != null)
                visual.outer.material =
                    visual.outerMaterial;

            if (visual.innerMaterial != null)
                visual.inner.material =
                    visual.innerMaterial;

            ParticleSystemRenderer renderer =
                visual.particles.GetComponent<
                    ParticleSystemRenderer>();

            if (visual.particleMaterial != null)
                renderer.material =
                    visual.particleMaterial;

            ParticleSystem.MainModule main =
                visual.particles.main;

            main.startColor =
                new ParticleSystem.MinMaxGradient(
                    new Color(
                        color.r,
                        color.g,
                        color.b,
                        0.35f),
                    Color.Lerp(
                        color,
                        Color.white,
                        0.35f));
        }

        private static void DrawIrregularRing(
            LineRenderer line,
            float radius,
            int seed,
            float irregularity)
        {
            if (line == null)
                return;

            int count =
                line.positionCount;

            for (int i = 0;
                 i < count;
                 i++)
            {
                float t =
                    i /
                    (float)count;

                float angle =
                    t *
                    Mathf.PI *
                    2f;

                float noise =
                    Mathf.PerlinNoise(
                        seed * 0.173f,
                        t * 5.7f) *
                    2f -
                    1f;

                float localRadius =
                    Mathf.Max(
                        0.05f,
                        radius *
                        (1f +
                         noise *
                         irregularity));

                line.SetPosition(
                    i,
                    new Vector3(
                        Mathf.Cos(angle) *
                        localRadius,
                        0f,
                        Mathf.Sin(angle) *
                        localRadius));
            }
        }

        private static Color GetSurfaceColor(
            SurfaceType type)
        {
            switch (type)
            {
                case SurfaceType.Fire:
                    return new Color(
                        1f,
                        0.26f,
                        0.035f,
                        0.88f);

                case SurfaceType.Water:
                    return new Color(
                        0.12f,
                        0.52f,
                        0.92f,
                        0.68f);

                case SurfaceType.Ice:
                    return new Color(
                        0.45f,
                        0.88f,
                        1f,
                        0.82f);

                case SurfaceType.Poison:
                    return new Color(
                        0.38f,
                        0.92f,
                        0.20f,
                        0.78f);

                case SurfaceType.Electrified:
                    return new Color(
                        0.28f,
                        0.70f,
                        1f,
                        0.92f);

                case SurfaceType.Arcane:
                    return new Color(
                        0.68f,
                        0.25f,
                        1f,
                        0.90f);

                case SurfaceType.Steam:
                    return new Color(
                        0.78f,
                        0.84f,
                        0.90f,
                        0.52f);

                default:
                    return Color.white;
            }
        }

        private static Material CreateMaterial(
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

        private static void DestroyMaterial(
            Material material)
        {
            if (material != null)
                Destroy(material);
        }

        private static void DestroyVisual(
            SurfaceVisual visual)
        {
            if (visual == null)
                return;

            DestroyMaterial(
                visual.outerMaterial);

            DestroyMaterial(
                visual.innerMaterial);

            DestroyMaterial(
                visual.particleMaterial);

            if (visual.root != null)
                Destroy(visual.root);
        }

        private void ClearAll()
        {
            foreach (SurfaceVisual visual
                     in visuals.Values)
            {
                DestroyVisual(visual);
            }

            visuals.Clear();
        }

        private void OnDestroy()
        {
            ClearAll();
        }
    }
}
