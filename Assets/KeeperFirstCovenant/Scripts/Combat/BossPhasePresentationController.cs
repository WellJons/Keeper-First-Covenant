using System.Collections;
using KeeperFirstCovenant.UI;
using UnityEngine;
using UnityEngine.UI;

namespace KeeperFirstCovenant.Combat
{
    public sealed class BossPhasePresentationController :
        MonoBehaviour
    {
        private CanvasGroup group;
        private Text phaseText;
        private Text nameText;
        private Coroutine routine;
        private Camera worldCamera;
        private CameraImpactShake shake;

        private void OnEnable()
        {
            BossPhaseController.PhaseChanged +=
                OnPhaseChanged;
        }

        private void OnDisable()
        {
            BossPhaseController.PhaseChanged -=
                OnPhaseChanged;
        }

        private void Start()
        {
            Build();
            group.alpha = 0f;
            ResolveCamera();
        }

        private void OnPhaseChanged(
            BossPhaseEvent value)
        {
            if (value.Boss == null ||
                value.Step == null)
            {
                return;
            }

            Color color =
                value.Step.phaseColor.a <= 0.001f
                    ? MainMenuTheme.Warm
                    : value.Step.phaseColor;

            phaseText.text =
                "ФАЗА " +
                ToRoman(
                    value.PhaseNumber);

            phaseText.color = color;

            nameText.text =
                string.IsNullOrWhiteSpace(
                    value.Step.phaseName)
                    ? "ИЗМЕНЕНИЕ БОЯ"
                    : value.Step.phaseName;

            SpawnPhaseBurst(
                value.Boss.transform.position +
                Vector3.up * 0.5f,
                color);

            StartCoroutine(
                PhaseLight(
                    value.Boss.transform.position,
                    color));

            ResolveCamera();

            shake?.AddImpulse(
                0.34f,
                0.32f,
                25f);

            if (routine != null)
                StopCoroutine(routine);

            routine =
                StartCoroutine(
                    ShowRoutine());
        }

        private void Build()
        {
            GameObject canvasObject =
                new GameObject(
                    "BossPhaseCanvas",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler));

            canvasObject.transform.SetParent(
                transform,
                false);

            Canvas canvas =
                canvasObject.GetComponent<Canvas>();

            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;

            canvas.sortingOrder = 6200;

            CanvasScaler scaler =
                canvasObject.GetComponent<
                    CanvasScaler>();

            scaler.uiScaleMode =
                CanvasScaler.ScaleMode
                    .ScaleWithScreenSize;

            scaler.referenceResolution =
                new Vector2(1920f, 1080f);

            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect =
                canvasObject.GetComponent<
                    RectTransform>();

            Image panel =
                MenuUiFactory.CreateImage(
                    "BossPhasePanel",
                    canvasRect,
                    new Color(
                        0.006f,
                        0.009f,
                        0.014f,
                        0.92f));

            RectTransform rect =
                panel.rectTransform;

            MenuUiFactory.SetAnchoredRect(
                rect,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 255f),
                new Vector2(560f, 112f));

            KeeperUiSkin.DecorateMajorPanel(
                panel,
                true);

            group =
                panel.gameObject
                    .AddComponent<CanvasGroup>();

            phaseText =
                MenuUiFactory.CreateText(
                    "Phase",
                    panel.transform,
                    string.Empty,
                    15,
                    MainMenuTheme.Warm,
                    TextAnchor.MiddleCenter);

            phaseText.rectTransform.anchorMin =
                new Vector2(0f, 0.58f);

            phaseText.rectTransform.anchorMax =
                Vector2.one;

            phaseText.rectTransform.offsetMin =
                new Vector2(18f, 0f);

            phaseText.rectTransform.offsetMax =
                new Vector2(-18f, -8f);

            nameText =
                MenuUiFactory.CreateText(
                    "Name",
                    panel.transform,
                    string.Empty,
                    27,
                    MainMenuTheme.Text,
                    TextAnchor.UpperCenter);

            nameText.rectTransform.anchorMin =
                Vector2.zero;

            nameText.rectTransform.anchorMax =
                new Vector2(1f, 0.64f);

            nameText.rectTransform.offsetMin =
                new Vector2(18f, 10f);

            nameText.rectTransform.offsetMax =
                new Vector2(-18f, 0f);
        }

        private IEnumerator ShowRoutine()
        {
            group.alpha = 0f;

            RectTransform rect =
                group.GetComponent<
                    RectTransform>();

            rect.localScale =
                Vector3.one * 1.12f;

            float elapsed = 0f;

            while (elapsed < 0.16f)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        elapsed / 0.16f);

                group.alpha = t;

                rect.localScale =
                    Vector3.one *
                    Mathf.Lerp(
                        1.12f,
                        1f,
                        t);

                yield return null;
            }

            group.alpha = 1f;
            rect.localScale = Vector3.one;

            yield return
                new WaitForSecondsRealtime(
                    1.15f);

            elapsed = 0f;

            while (elapsed < 0.38f)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                group.alpha =
                    1f -
                    Mathf.Clamp01(
                        elapsed / 0.38f);

                yield return null;
            }

            group.alpha = 0f;
            routine = null;
        }

        private void SpawnPhaseBurst(
            Vector3 point,
            Color color)
        {
            GameObject root =
                new GameObject(
                    "BossPhaseBurst");

            root.transform.position =
                point;

            ParticleSystem particles =
                root.AddComponent<
                    ParticleSystem>();

            ParticleSystem.MainModule main =
                particles.main;

            main.loop = false;
            main.duration = 0.65f;

            main.startLifetime =
                new ParticleSystem.MinMaxCurve(
                    0.45f,
                    1.0f);

            main.startSpeed =
                new ParticleSystem.MinMaxCurve(
                    3.5f,
                    7.5f);

            main.startSize =
                new ParticleSystem.MinMaxCurve(
                    0.07f,
                    0.23f);

            main.startColor =
                new ParticleSystem.MinMaxGradient(
                    color,
                    Color.Lerp(
                        color,
                        Color.white,
                        0.6f));

            main.maxParticles = 110;

            ParticleSystem.EmissionModule emission =
                particles.emission;

            emission.rateOverTime = 0f;

            emission.SetBurst(
                0,
                new ParticleSystem.Burst(
                    0f,
                    (short)72));

            ParticleSystem.ShapeModule shape =
                particles.shape;

            shape.enabled = true;
            shape.shapeType =
                ParticleSystemShapeType.Circle;

            shape.radius = 1.25f;
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
                    0.5f,
                    2.0f);

            ParticleSystemRenderer renderer =
                particles.GetComponent<
                    ParticleSystemRenderer>();

            Material material =
                CreateParticleMaterial(
                    color);

            if (material != null)
                renderer.material = material;

            particles.Play();

            Destroy(root, 1.8f);

            if (material != null)
                Destroy(material, 1.9f);
        }

        private IEnumerator PhaseLight(
            Vector3 point,
            Color color)
        {
            GameObject root =
                new GameObject(
                    "BossPhaseLight");

            root.transform.position =
                point +
                Vector3.up * 1.0f;

            Light light =
                root.AddComponent<Light>();

            light.type = LightType.Point;
            light.color = color;
            light.range = 11f;
            light.intensity = 16f;

            float elapsed = 0f;
            const float duration = 0.42f;

            while (elapsed < duration)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        elapsed / duration);

                light.intensity =
                    16f *
                    (1f - t) *
                    (1f - t);

                yield return null;
            }

            Destroy(root);
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

        private static string ToRoman(
            int value)
        {
            switch (value)
            {
                case 1: return "I";
                case 2: return "II";
                case 3: return "III";
                case 4: return "IV";
                case 5: return "V";
                default:
                    return value.ToString();
            }
        }
    }
}
