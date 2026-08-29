using System.Linq;
using KeeperFirstCovenant.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public sealed class BossCombatHud : MonoBehaviour
    {
        private CanvasGroup group;
        private Text bossName;
        private Text phaseText;
        private Text intentText;
        private Text hpText;
        private RectTransform healthFill;
        private Image healthFillImage;
        private RectTransform breakFill;
        private Image breakFillImage;

        private CombatantRuntime boss;
        private BossPhaseController phases;
        private BreakGaugeComponent breakGauge;
        private ChargedActionComponent charge;

        private float nextLookup;

        private void Start()
        {
            Build();
            SetVisible(false);
        }

        private void Update()
        {
            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            if (director == null ||
                director.State != CombatState.Active)
            {
                boss = null;
                SetVisible(false);
                return;
            }

            if (boss == null ||
                !boss.IsAlive ||
                Time.unscaledTime >= nextLookup)
            {
                nextLookup =
                    Time.unscaledTime + 0.45f;

                ResolveBoss(director);
            }

            if (boss == null ||
                !boss.IsAlive ||
                boss.Definition == null)
            {
                SetVisible(false);
                return;
            }

            Refresh();
            SetVisible(true);
        }

        private void ResolveBoss(
            TurnCombatDirector director)
        {
            CombatantRuntime[] combatants =
                FindObjectsByType<
                    CombatantRuntime>(
                    FindObjectsSortMode.None);

            boss =
                combatants
                    .Where(value =>
                        value != null &&
                        value.IsAlive &&
                        value.Faction ==
                            CombatFaction.Enemy &&
                        director.IsParticipant(value) &&
                        value.GetComponent<
                            BossPhaseController>() !=
                            null)
                    .OrderByDescending(value =>
                        value.Definition != null
                            ? value.Definition.maxHealth
                            : 0)
                    .FirstOrDefault();

            phases =
                boss != null
                    ? boss.GetComponent<
                        BossPhaseController>()
                    : null;

            breakGauge =
                boss != null
                    ? boss.GetComponent<
                        BreakGaugeComponent>()
                    : null;

            charge =
                boss != null
                    ? boss.GetComponent<
                        ChargedActionComponent>()
                    : null;
        }

        private void Refresh()
        {
            bossName.text =
                boss.Definition.displayName;

            phaseText.text =
                phases != null
                    ? "ФАЗА " +
                      ToRoman(
                          phases.CurrentPhaseNumber)
                    : string.Empty;

            float health =
                Mathf.Clamp01(
                    boss.CurrentHealth /
                    (float)Mathf.Max(
                        1,
                        boss.Definition.maxHealth));

            healthFill.anchorMax =
                new Vector2(
                    health,
                    1f);

            hpText.text =
                boss.CurrentHealth +
                " / " +
                boss.Definition.maxHealth;

            healthFillImage.color =
                health <= 0.32f
                    ? MainMenuTheme.Danger
                    : health <= 0.68f
                        ? MainMenuTheme.Warm
                        : new Color(
                            0.72f,
                            0.78f,
                            0.82f,
                            1f);

            if (breakGauge != null)
            {
                breakFill.anchorMax =
                    new Vector2(
                        breakGauge.Normalized,
                        1f);

                breakFillImage.color =
                    breakGauge.IsBroken
                        ? MainMenuTheme.Danger
                        : breakGauge.Normalized >=
                            0.72f
                            ? MainMenuTheme.Warm
                            : MainMenuTheme.Silver;
            }
            else
            {
                breakFill.anchorMax =
                    Vector2.zero;
            }

            if (charge != null &&
                charge.HasCharge &&
                charge.Action != null)
            {
                intentText.text =
                    "ГОТОВИТ: " +
                    charge.Action.displayName;

                intentText.color =
                    MainMenuTheme.Warm;
            }
            else if (breakGauge != null &&
                     breakGauge.IsBroken)
            {
                intentText.text =
                    "СТОЙКА СЛОМАНА";

                intentText.color =
                    MainMenuTheme.Danger;
            }
            else
            {
                intentText.text =
                    "Стойкость " +
                    (breakGauge != null
                        ? breakGauge.Stability +
                          "/" +
                          breakGauge.MaxStability
                        : "—");

                intentText.color =
                    MainMenuTheme.MutedText;
            }
        }

        private void Build()
        {
            GameObject canvasObject =
                new GameObject(
                    "BossCombatCanvas",
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

            canvas.sortingOrder = 570;

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
                    "BossPanel",
                    canvasRect,
                    new Color(
                        MainMenuTheme.Panel.r,
                        MainMenuTheme.Panel.g,
                        MainMenuTheme.Panel.b,
                        0.93f));

            RectTransform rect =
                panel.rectTransform;

            MenuUiFactory.SetAnchoredRect(
                rect,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -30f),
                new Vector2(760f, 112f));

            KeeperUiSkin.DecorateMajorPanel(
                panel,
                true);

            group =
                panel.gameObject
                    .AddComponent<CanvasGroup>();

            bossName =
                MenuUiFactory.CreateText(
                    "BossName",
                    panel.transform,
                    string.Empty,
                    21,
                    MainMenuTheme.Text,
                    TextAnchor.MiddleLeft);

            bossName.rectTransform.anchorMin =
                new Vector2(0f, 0.68f);

            bossName.rectTransform.anchorMax =
                new Vector2(0.72f, 1f);

            bossName.rectTransform.offsetMin =
                new Vector2(20f, 0f);

            bossName.rectTransform.offsetMax =
                new Vector2(0f, -6f);

            phaseText =
                MenuUiFactory.CreateText(
                    "Phase",
                    panel.transform,
                    string.Empty,
                    12,
                    MainMenuTheme.Silver,
                    TextAnchor.MiddleRight);

            phaseText.rectTransform.anchorMin =
                new Vector2(0.68f, 0.68f);

            phaseText.rectTransform.anchorMax =
                Vector2.one;

            phaseText.rectTransform.offsetMin =
                Vector2.zero;

            phaseText.rectTransform.offsetMax =
                new Vector2(-20f, -6f);

            Image healthTrack =
                MenuUiFactory.CreateImage(
                    "HealthTrack",
                    panel.transform,
                    new Color(
                        0.05f,
                        0.055f,
                        0.06f,
                        0.98f));

            RectTransform hpTrackRect =
                healthTrack.rectTransform;

            hpTrackRect.anchorMin =
                new Vector2(0f, 0.43f);

            hpTrackRect.anchorMax =
                new Vector2(1f, 0.60f);

            hpTrackRect.offsetMin =
                new Vector2(20f, 0f);

            hpTrackRect.offsetMax =
                new Vector2(-20f, 0f);

            Image hpFill =
                MenuUiFactory.CreateImage(
                    "HealthFill",
                    healthTrack.transform,
                    MainMenuTheme.Silver);

            healthFill =
                hpFill.rectTransform;

            healthFill.anchorMin =
                Vector2.zero;

            healthFill.anchorMax =
                new Vector2(1f, 1f);

            healthFill.offsetMin =
                Vector2.zero;

            healthFill.offsetMax =
                Vector2.zero;

            healthFillImage = hpFill;

            hpText =
                MenuUiFactory.CreateText(
                    "HealthText",
                    healthTrack.transform,
                    string.Empty,
                    11,
                    MainMenuTheme.Text,
                    TextAnchor.MiddleCenter);

            MenuUiFactory.Stretch(
                hpText.rectTransform);

            Image breakTrack =
                MenuUiFactory.CreateImage(
                    "BreakTrack",
                    panel.transform,
                    new Color(
                        0.05f,
                        0.055f,
                        0.06f,
                        0.94f));

            RectTransform breakTrackRect =
                breakTrack.rectTransform;

            breakTrackRect.anchorMin =
                new Vector2(0f, 0.29f);

            breakTrackRect.anchorMax =
                new Vector2(1f, 0.37f);

            breakTrackRect.offsetMin =
                new Vector2(20f, 0f);

            breakTrackRect.offsetMax =
                new Vector2(-20f, 0f);

            Image breakFillImageObject =
                MenuUiFactory.CreateImage(
                    "BreakFill",
                    breakTrack.transform,
                    MainMenuTheme.Silver);

            breakFill =
                breakFillImageObject.rectTransform;

            breakFill.anchorMin =
                Vector2.zero;

            breakFill.anchorMax =
                Vector2.zero;

            breakFill.offsetMin =
                Vector2.zero;

            breakFill.offsetMax =
                Vector2.zero;

            breakFillImage =
                breakFillImageObject;

            intentText =
                MenuUiFactory.CreateText(
                    "Intent",
                    panel.transform,
                    string.Empty,
                    12,
                    MainMenuTheme.MutedText,
                    TextAnchor.MiddleCenter);

            intentText.rectTransform.anchorMin =
                Vector2.zero;

            intentText.rectTransform.anchorMax =
                new Vector2(1f, 0.25f);

            intentText.rectTransform.offsetMin =
                new Vector2(20f, 3f);

            intentText.rectTransform.offsetMax =
                new Vector2(-20f, 0f);
        }

        private void SetVisible(
            bool visible)
        {
            if (group == null)
                return;

            group.alpha =
                visible ? 1f : 0f;

            group.interactable = false;
            group.blocksRaycasts = false;
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
                default:
                    return value.ToString();
            }
        }
    }
}
