using System.Collections.Generic;
using System.Linq;
using KeeperFirstCovenant.UI;
using UnityEngine;
using UnityEngine.UI;

namespace KeeperFirstCovenant.Combat
{
    public sealed class ChargedActionTelegraphController :
        MonoBehaviour
    {
        private sealed class TelegraphVisual
        {
            public CombatantRuntime owner;
            public CombatantRuntime target;
            public CombatActionDefinition action;
            public Vector3 fixedPoint;
            public GameObject root;
            public LineRenderer outer;
            public LineRenderer inner;
            public Light light;
            public Material outerMaterial;
            public Material innerMaterial;
            public float radius;
        }

        private readonly Dictionary<
            CombatantRuntime,
            TelegraphVisual>
            visuals =
                new Dictionary<
                    CombatantRuntime,
                    TelegraphVisual>();

        private CanvasGroup notificationGroup;
        private Text notificationTitle;
        private Text notificationBody;
        private float notificationUntil;

        private void OnEnable()
        {
            ChargedActionComponent.ChargeStarted +=
                OnChargeStarted;

            ChargedActionComponent.ChargeUpdated +=
                OnChargeUpdated;

            ChargedActionComponent.ChargeReleased +=
                OnChargeReleased;

            ChargedActionComponent.ChargeCancelled +=
                OnChargeCancelled;
        }

        private void OnDisable()
        {
            ChargedActionComponent.ChargeStarted -=
                OnChargeStarted;

            ChargedActionComponent.ChargeUpdated -=
                OnChargeUpdated;

            ChargedActionComponent.ChargeReleased -=
                OnChargeReleased;

            ChargedActionComponent.ChargeCancelled -=
                OnChargeCancelled;

            ClearAll();
        }

        private void Start()
        {
            BuildNotification();
        }

        private void Update()
        {
            foreach (TelegraphVisual visual
                     in visuals.Values
                         .ToArray())
            {
                if (visual == null ||
                    visual.owner == null ||
                    visual.action == null ||
                    visual.root == null)
                {
                    continue;
                }

                Vector3 point =
                    visual.action.targetKind ==
                        TargetKind.Ground
                        ? visual.fixedPoint
                        : visual.target != null
                            ? visual.target
                                .transform.position
                            : visual.fixedPoint;

                visual.root.transform.position =
                    point +
                    Vector3.up * 0.045f;

                float pulse =
                    1f +
                    Mathf.Sin(
                        Time.unscaledTime *
                        5.5f) *
                    0.055f;

                DrawRing(
                    visual.outer,
                    visual.radius * pulse,
                    72);

                visual.inner.transform
                    .localRotation =
                        Quaternion.Euler(
                            0f,
                            Time.unscaledTime *
                            34f,
                            0f);

                float innerRadius =
                    visual.radius *
                    (0.66f +
                     Mathf.Sin(
                         Time.unscaledTime *
                         3.7f) *
                     0.025f);

                DrawRing(
                    visual.inner,
                    innerRadius,
                    56);

                if (visual.light != null)
                {
                    visual.light.intensity =
                        1.4f +
                        Mathf.Sin(
                            Time.unscaledTime *
                            6.2f) *
                        0.45f;
                }
            }

            if (notificationGroup != null)
            {
                float remaining =
                    notificationUntil -
                    Time.unscaledTime;

                if (remaining <= 0f)
                {
                    notificationGroup.alpha =
                        Mathf.MoveTowards(
                            notificationGroup.alpha,
                            0f,
                            Time.unscaledDeltaTime *
                            5f);
                }
                else
                {
                    notificationGroup.alpha =
                        Mathf.MoveTowards(
                            notificationGroup.alpha,
                            1f,
                            Time.unscaledDeltaTime *
                            8f);
                }
            }
        }

        private void OnChargeStarted(
            ChargedActionEvent value)
        {
            if (value.Owner == null ||
                value.Action == null)
            {
                return;
            }

            RemoveVisual(
                value.Owner,
                false);

            Color color =
                GetDamageColor(
                    value.Action.damageType);

            float radius =
                value.Action
                    .telegraphRadiusOverride >
                    0.01f
                    ? value.Action
                        .telegraphRadiusOverride
                    : value.Action.areaRadius >
                      0.01f
                        ? value.Action.areaRadius
                        : 0.85f;

            GameObject root =
                new GameObject(
                    "ChargedActionTelegraph_" +
                    value.Action.actionId);

            LineRenderer outer =
                root.AddComponent<
                    LineRenderer>();

            Material outerMaterial =
                CreateMaterial(color);

            ConfigureLine(
                outer,
                outerMaterial,
                0.075f,
                72);

            GameObject innerObject =
                new GameObject(
                    "InnerWarning");

            innerObject.transform.SetParent(
                root.transform,
                false);

            LineRenderer inner =
                innerObject.AddComponent<
                    LineRenderer>();

            Material innerMaterial =
                CreateMaterial(
                    Color.Lerp(
                        color,
                        Color.white,
                        0.58f));

            ConfigureLine(
                inner,
                innerMaterial,
                0.026f,
                56);

            GameObject lightObject =
                new GameObject(
                    "ChargeLight");

            lightObject.transform.SetParent(
                root.transform,
                false);

            lightObject.transform.localPosition =
                Vector3.up * 0.45f;

            Light light =
                lightObject.AddComponent<Light>();

            light.type = LightType.Point;
            light.color = color;
            light.range =
                Mathf.Max(
                    3.5f,
                    radius * 1.7f);

            light.intensity = 1.5f;

            var visual =
                new TelegraphVisual
                {
                    owner = value.Owner,
                    target = value.Target,
                    action = value.Action,
                    fixedPoint = value.Point,
                    root = root,
                    outer = outer,
                    inner = inner,
                    light = light,
                    outerMaterial =
                        outerMaterial,
                    innerMaterial =
                        innerMaterial,
                    radius = radius
                };

            visuals[value.Owner] =
                visual;

            ShowNotification(
                value.Action.displayName,
                value.Action
                    .interruptWindUpOnBreak
                    ? "ОПАСНЫЙ ПРИЁМ   •   СЛОМ СОРВЁТ ЗАРЯД"
                    : "ОПАСНЫЙ ПРИЁМ   •   ПОКИНЬТЕ ЗОНУ",
                color,
                2.4f);
        }

        private void OnChargeUpdated(
            ChargedActionEvent value)
        {
            if (value.Owner == null ||
                value.Action == null)
            {
                return;
            }

            ShowNotification(
                value.Action.displayName,
                "ЗАРЯД ПРОДОЛЖАЕТСЯ   •   " +
                Mathf.Max(
                    1,
                    value.TurnsRemaining) +
                " ХОД",
                GetDamageColor(
                    value.Action.damageType),
                1.8f);
        }

        private void OnChargeReleased(
            ChargedActionEvent value)
        {
            if (value.Action != null)
            {
                ShowNotification(
                    value.Action.displayName,
                    "УДАР ВЫПУЩЕН",
                    GetDamageColor(
                        value.Action.damageType),
                    0.8f);
            }

            RemoveVisual(
                value.Owner,
                true);
        }

        private void OnChargeCancelled(
            ChargedActionEvent value)
        {
            ShowNotification(
                "ЗАРЯД СОРВАН",
                value.Action != null
                    ? value.Action.displayName
                    : string.Empty,
                Color.white,
                1.25f);

            RemoveVisual(
                value.Owner,
                false);
        }

        private void BuildNotification()
        {
            GameObject canvasObject =
                new GameObject(
                    "ChargedActionWarningCanvas",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler));

            canvasObject.transform.SetParent(
                transform,
                false);

            Canvas canvas =
                canvasObject.GetComponent<
                    Canvas>();

            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;

            canvas.sortingOrder = 6080;

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
                    "ChargeWarning",
                    canvasRect,
                    new Color(
                        0.012f,
                        0.016f,
                        0.022f,
                        0.94f));

            RectTransform rect =
                panel.rectTransform;

            MenuUiFactory.SetAnchoredRect(
                rect,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -92f),
                new Vector2(620f, 92f));

            KeeperUiSkin.DecorateMajorPanel(
                panel,
                false);

            notificationGroup =
                panel.gameObject
                    .AddComponent<CanvasGroup>();

            notificationTitle =
                MenuUiFactory.CreateText(
                    "Title",
                    panel.transform,
                    string.Empty,
                    21,
                    MainMenuTheme.Text,
                    TextAnchor.MiddleCenter);

            notificationTitle.rectTransform.anchorMin =
                new Vector2(0f, 0.44f);

            notificationTitle.rectTransform.anchorMax =
                Vector2.one;

            notificationTitle.rectTransform.offsetMin =
                new Vector2(20f, 0f);

            notificationTitle.rectTransform.offsetMax =
                new Vector2(-20f, -7f);

            notificationBody =
                MenuUiFactory.CreateText(
                    "Body",
                    panel.transform,
                    string.Empty,
                    12,
                    MainMenuTheme.Warm,
                    TextAnchor.UpperCenter);

            notificationBody.rectTransform.anchorMin =
                Vector2.zero;

            notificationBody.rectTransform.anchorMax =
                new Vector2(1f, 0.48f);

            notificationBody.rectTransform.offsetMin =
                new Vector2(20f, 8f);

            notificationBody.rectTransform.offsetMax =
                new Vector2(-20f, 0f);

            notificationGroup.alpha = 0f;
        }

        private void ShowNotification(
            string title,
            string body,
            Color color,
            float duration)
        {
            if (notificationGroup == null)
                return;

            notificationTitle.text =
                string.IsNullOrWhiteSpace(title)
                    ? "ОПАСНЫЙ ПРИЁМ"
                    : title;

            notificationTitle.color = color;
            notificationBody.text = body;

            notificationUntil =
                Time.unscaledTime +
                Mathf.Max(
                    0.2f,
                    duration);
        }

        private void RemoveVisual(
            CombatantRuntime owner,
            bool released)
        {
            if (owner == null)
                return;

            if (!visuals.TryGetValue(
                    owner,
                    out TelegraphVisual visual))
            {
                return;
            }

            visuals.Remove(owner);

            if (visual.light != null &&
                released)
            {
                visual.light.intensity = 5f;
            }

            DestroyVisual(visual);
        }

        private static void ConfigureLine(
            LineRenderer line,
            Material material,
            float width,
            int points)
        {
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = points;
            line.widthMultiplier = width;
            line.numCornerVertices = 2;
            line.numCapVertices = 2;
            line.shadowCastingMode =
                UnityEngine.Rendering
                    .ShadowCastingMode.Off;

            line.receiveShadows = false;

            if (material != null)
                line.material = material;
        }

        private static void DrawRing(
            LineRenderer line,
            float radius,
            int points)
        {
            if (line == null)
                return;

            if (line.positionCount != points)
                line.positionCount = points;

            for (int i = 0;
                 i < points;
                 i++)
            {
                float angle =
                    i /
                    (float)points *
                    Mathf.PI *
                    2f;

                line.SetPosition(
                    i,
                    new Vector3(
                        Mathf.Cos(angle) *
                        radius,
                        0f,
                        Mathf.Sin(angle) *
                        radius));
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

        private static Color GetDamageColor(
            DamageType type)
        {
            switch (type)
            {
                case DamageType.Fire:
                    return new Color(
                        1f,
                        0.24f,
                        0.035f,
                        1f);

                case DamageType.Frost:
                    return new Color(
                        0.46f,
                        0.88f,
                        1f,
                        1f);

                case DamageType.Lightning:
                    return new Color(
                        0.24f,
                        0.68f,
                        1f,
                        1f);

                case DamageType.Arcane:
                    return new Color(
                        0.70f,
                        0.30f,
                        1f,
                        1f);

                case DamageType.Poison:
                    return new Color(
                        0.42f,
                        0.92f,
                        0.18f,
                        1f);

                case DamageType.Radiant:
                    return new Color(
                        1f,
                        0.82f,
                        0.35f,
                        1f);

                default:
                    return new Color(
                        1f,
                        0.34f,
                        0.20f,
                        1f);
            }
        }

        private static void DestroyVisual(
            TelegraphVisual visual)
        {
            if (visual == null)
                return;

            if (visual.outerMaterial != null)
                Destroy(
                    visual.outerMaterial);

            if (visual.innerMaterial != null)
                Destroy(
                    visual.innerMaterial);

            if (visual.root != null)
                Destroy(
                    visual.root);
        }

        private void ClearAll()
        {
            foreach (TelegraphVisual visual
                     in visuals.Values)
            {
                DestroyVisual(visual);
            }

            visuals.Clear();
        }
    }
}
