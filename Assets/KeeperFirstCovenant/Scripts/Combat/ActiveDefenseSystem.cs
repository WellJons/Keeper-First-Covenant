using System;
using System.Collections;
using KeeperFirstCovenant.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace KeeperFirstCovenant.Combat
{
    public sealed class ActiveDefenseResolution
    {
        public ActiveDefenseOutcome Outcome =
            ActiveDefenseOutcome.None;
    }

    public sealed class ActiveDefenseSystem :
        MonoBehaviour
    {
        [SerializeField, Min(0.35f)]
        private float telegraphDuration = 0.85f;

        [SerializeField, Range(0.25f, 0.9f)]
        private float idealTiming = 0.68f;

        [SerializeField, Range(0.02f, 0.25f)]
        private float perfectDodgeWindow = 0.075f;

        [SerializeField, Range(0.05f, 0.4f)]
        private float dodgeWindow = 0.22f;

        [SerializeField, Range(0.01f, 0.2f)]
        private float perfectParryWindow = 0.05f;

        [SerializeField, Range(0.03f, 0.3f)]
        private float parryWindow = 0.12f;

        public static ActiveDefenseSystem Instance
        {
            get;
            private set;
        }

        public static event Action<
            CombatantRuntime,
            ActiveDefenseOutcome>
            DefenseResolved;

        private GameObject promptRoot;
        private Text promptText;

        private void Awake()
        {
            if (Instance != null &&
                Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            BuildPrompt();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public bool CanReact(
            CombatantRuntime target,
            CombatActionDefinition action)
        {
            if (target == null ||
                action == null ||
                !target.IsAlive ||
                target.ReactionsRemaining <= 0)
            {
                return false;
            }

            if (target.Faction !=
                    CombatFaction.Player &&
                target.Faction !=
                    CombatFaction.Ally)
            {
                return false;
            }

            return
                action.targetKind ==
                    TargetKind.Enemy ||
                action.targetKind ==
                    TargetKind.AnyCombatant;
        }

        public IEnumerator ResolveIncomingAttack(
            CombatantRuntime attacker,
            CombatantRuntime target,
            CombatActionDefinition action,
            ActiveDefenseResolution resolution)
        {
            if (resolution == null)
                yield break;

            resolution.Outcome =
                ActiveDefenseOutcome.None;

            if (!CanReact(target, action))
                yield break;

            bool canParry =
                action.category ==
                    CombatActionCategory.Melee;

            GameObject telegraph =
                CreateTelegraph(
                    target,
                    canParry,
                    out LineRenderer outer,
                    out LineRenderer perfectRing,
                    out Material outerMaterial,
                    out Material perfectMaterial);

            ShowPrompt(canParry);

            float elapsed = 0f;
            bool resolved = false;

            while (elapsed < telegraphDuration &&
                   target != null &&
                   target.IsAlive &&
                   !resolved)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                float normalized =
                    Mathf.Clamp01(
                        elapsed /
                        telegraphDuration);

                UpdateTelegraph(
                    telegraph,
                    target,
                    outer,
                    perfectRing,
                    normalized);

                Keyboard keyboard =
                    Keyboard.current;

                bool dodgePressed =
                    keyboard != null &&
                    keyboard.spaceKey
                        .wasPressedThisFrame;

                bool parryPressed =
                    canParry &&
                    keyboard != null &&
                    keyboard.fKey
                        .wasPressedThisFrame;

                if (!dodgePressed &&
                    !parryPressed)
                {
                    yield return null;
                    continue;
                }

                if (!target.TrySpendReaction())
                {
                    break;
                }

                float error =
                    Mathf.Abs(
                        normalized -
                        idealTiming);

                if (parryPressed)
                {
                    resolution.Outcome =
                        error <=
                            perfectParryWindow
                            ? ActiveDefenseOutcome
                                .PerfectParry
                            : error <= parryWindow
                                ? ActiveDefenseOutcome
                                    .Parry
                                : ActiveDefenseOutcome
                                    .Failed;
                }
                else
                {
                    resolution.Outcome =
                        error <=
                            perfectDodgeWindow
                            ? ActiveDefenseOutcome
                                .PerfectDodge
                            : error <= dodgeWindow
                                ? ActiveDefenseOutcome
                                    .Dodge
                                : ActiveDefenseOutcome
                                    .Failed;
                }

                resolved = true;

                Color resultColor =
                    IsPerfect(
                        resolution.Outcome)
                        ? Color.white
                        : IsSuccessful(
                            resolution.Outcome)
                            ? new Color(
                                0.90f,
                                0.64f,
                                0.25f,
                                1f)
                            : new Color(
                                0.75f,
                                0.18f,
                                0.14f,
                                1f);

                if (outer != null)
                {
                    outer.startColor =
                        resultColor;

                    outer.endColor =
                        resultColor;
                }

                yield return
                    new WaitForSecondsRealtime(
                        0.09f);
            }

            HidePrompt();

            if (telegraph != null)
                Destroy(telegraph);

            if (outerMaterial != null)
                Destroy(outerMaterial);

            if (perfectMaterial != null)
                Destroy(perfectMaterial);

            DefenseResolved?.Invoke(
                target,
                resolution.Outcome);
        }

        private GameObject CreateTelegraph(
            CombatantRuntime target,
            bool canParry,
            out LineRenderer outer,
            out LineRenderer perfectRing,
            out Material outerMaterial,
            out Material perfectMaterial)
        {
            GameObject root =
                new GameObject(
                    "ActiveDefenseTelegraph");

            root.transform.position =
                target.transform.position +
                Vector3.up * 0.06f;

            outer =
                root.AddComponent<
                    LineRenderer>();

            outerMaterial =
                CreateLineMaterial(
                    new Color(
                        0.72f,
                        0.80f,
                        0.88f,
                        0.95f));

            ConfigureRing(
                outer,
                outerMaterial,
                0.045f);

            GameObject perfectObject =
                new GameObject(
                    "PerfectTimingRing");

            perfectObject.transform.SetParent(
                root.transform,
                false);

            perfectRing =
                perfectObject.AddComponent<
                    LineRenderer>();

            perfectMaterial =
                CreateLineMaterial(
                    canParry
                        ? new Color(
                            1f,
                            0.58f,
                            0.18f,
                            0.82f)
                        : new Color(
                            0.58f,
                            0.86f,
                            1f,
                            0.78f));

            ConfigureRing(
                perfectRing,
                perfectMaterial,
                0.025f);

            DrawRing(
                perfectRing,
                0.48f);

            DrawRing(
                outer,
                1.55f);

            return root;
        }

        private void UpdateTelegraph(
            GameObject telegraph,
            CombatantRuntime target,
            LineRenderer outer,
            LineRenderer perfectRing,
            float normalized)
        {
            if (telegraph == null ||
                target == null)
            {
                return;
            }

            telegraph.transform.position =
                target.transform.position +
                Vector3.up * 0.06f;

            float radius =
                Mathf.Lerp(
                    1.55f,
                    0.22f,
                    normalized);

            DrawRing(
                outer,
                radius);

            float error =
                Mathf.Abs(
                    normalized -
                    idealTiming);

            Color color =
                error <= perfectDodgeWindow
                    ? Color.white
                    : error <= dodgeWindow
                        ? new Color(
                            0.95f,
                            0.68f,
                            0.28f,
                            1f)
                        : new Color(
                            0.68f,
                            0.78f,
                            0.88f,
                            0.9f);

            outer.startColor = color;
            outer.endColor = color;

            if (perfectRing != null)
            {
                float pulse =
                    0.72f +
                    Mathf.Sin(
                        Time.unscaledTime *
                        18f) * 0.06f;

                perfectRing.widthMultiplier =
                    0.025f * pulse;
            }
        }

        private static void ConfigureRing(
            LineRenderer line,
            Material material,
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

            if (material != null)
                line.material = material;
        }

        private static void DrawRing(
            LineRenderer line,
            float radius)
        {
            if (line == null)
                return;

            int count =
                line.positionCount;

            for (int i = 0;
                 i < count;
                 i++)
            {
                float angle =
                    i /
                    (float)count *
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

        private void BuildPrompt()
        {
            GameObject canvasObject =
                new GameObject(
                    "ActiveDefenseCanvas",
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

            canvas.sortingOrder = 6150;

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
                    "DefensePrompt",
                    canvasRect,
                    new Color(
                        0.012f,
                        0.016f,
                        0.021f,
                        0.90f));

            promptRoot = panel.gameObject;

            RectTransform rect =
                panel.rectTransform;

            MenuUiFactory.SetAnchoredRect(
                rect,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 102f),
                new Vector2(520f, 52f));

            KeeperUiSkin.DecorateSection(
                panel);

            promptText =
                MenuUiFactory.CreateText(
                    "Prompt",
                    panel.transform,
                    string.Empty,
                    14,
                    MainMenuTheme.Text,
                    TextAnchor.MiddleCenter);

            MenuUiFactory.Stretch(
                promptText.rectTransform,
                10f,
                5f,
                10f,
                5f);

            promptRoot.SetActive(false);
        }

        private void ShowPrompt(
            bool canParry)
        {
            if (promptRoot == null)
                return;

            promptText.text =
                canParry
                    ? "SPACE — УКЛОНЕНИЕ     •     F — ПАРИРОВАНИЕ"
                    : "SPACE — УКЛОНЕНИЕ";

            promptRoot.SetActive(true);
        }

        private void HidePrompt()
        {
            if (promptRoot != null)
                promptRoot.SetActive(false);
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

        private static bool IsSuccessful(
            ActiveDefenseOutcome outcome)
        {
            return
                outcome ==
                    ActiveDefenseOutcome.Dodge ||
                outcome ==
                    ActiveDefenseOutcome.PerfectDodge ||
                outcome ==
                    ActiveDefenseOutcome.Parry ||
                outcome ==
                    ActiveDefenseOutcome.PerfectParry;
        }

        private static bool IsPerfect(
            ActiveDefenseOutcome outcome)
        {
            return
                outcome ==
                    ActiveDefenseOutcome.PerfectDodge ||
                outcome ==
                    ActiveDefenseOutcome.PerfectParry;
        }
    }
}
