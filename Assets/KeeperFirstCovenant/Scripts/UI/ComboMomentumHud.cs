using System.Collections;
using KeeperFirstCovenant.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public sealed class ComboMomentumHud : MonoBehaviour
    {
        private CanvasGroup group;
        private Text chainText;
        private Text actionText;
        private Image accent;
        private Coroutine routine;

        private void Start()
        {
            Build();

            CombatActionStateComponent.ComboResolved +=
                OnComboResolved;

            group.alpha = 0f;
        }

        private void OnDestroy()
        {
            CombatActionStateComponent.ComboResolved -=
                OnComboResolved;
        }

        private void Build()
        {
            GameObject canvasObject =
                new GameObject(
                    "ComboMomentumCanvas",
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

            canvas.sortingOrder = 6100;

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
                    "ComboPanel",
                    canvasRect,
                    new Color(
                        MainMenuTheme.Panel.r,
                        MainMenuTheme.Panel.g,
                        MainMenuTheme.Panel.b,
                        0.90f));

            RectTransform rect =
                panel.rectTransform;

            MenuUiFactory.SetAnchoredRect(
                rect,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 205f),
                new Vector2(380f, 92f));

            KeeperUiSkin.DecorateMajorPanel(
                panel,
                false);

            group =
                panel.gameObject
                    .AddComponent<CanvasGroup>();

            accent =
                MenuUiFactory.CreateImage(
                    "Accent",
                    panel.transform,
                    MainMenuTheme.Warm);

            RectTransform accentRect =
                accent.rectTransform;

            accentRect.anchorMin =
                new Vector2(0f, 0f);

            accentRect.anchorMax =
                new Vector2(0f, 1f);

            accentRect.offsetMin =
                new Vector2(0f, 7f);

            accentRect.offsetMax =
                new Vector2(3f, -7f);

            chainText =
                MenuUiFactory.CreateText(
                    "Chain",
                    panel.transform,
                    string.Empty,
                    24,
                    MainMenuTheme.Warm,
                    TextAnchor.MiddleCenter);

            chainText.rectTransform.anchorMin =
                new Vector2(0f, 0.43f);

            chainText.rectTransform.anchorMax =
                Vector2.one;

            chainText.rectTransform.offsetMin =
                new Vector2(18f, 0f);

            chainText.rectTransform.offsetMax =
                new Vector2(-18f, -4f);

            actionText =
                MenuUiFactory.CreateText(
                    "Action",
                    panel.transform,
                    string.Empty,
                    13,
                    MainMenuTheme.MutedText,
                    TextAnchor.UpperCenter);

            actionText.rectTransform.anchorMin =
                Vector2.zero;

            actionText.rectTransform.anchorMax =
                new Vector2(1f, 0.46f);

            actionText.rectTransform.offsetMin =
                new Vector2(18f, 7f);

            actionText.rectTransform.offsetMax =
                new Vector2(-18f, 0f);
        }

        private void OnComboResolved(
            CombatantRuntime actor,
            CombatActionDefinition action,
            ComboExecutionContext context)
        {
            if (actor == null ||
                action == null ||
                !context.Matched)
            {
                return;
            }

            chainText.text =
                "СВЯЗКА  ×" +
                Mathf.Max(
                    2,
                    context.Depth);

            actionText.text =
                action.displayName +
                (context.BreakBonus > 0
                    ? "   •   усиленный слом"
                    : string.Empty);

            Color color =
                context.Depth >= 3
                    ? new Color(
                        1f,
                        0.82f,
                        0.38f,
                        1f)
                    : MainMenuTheme.Warm;

            chainText.color = color;
            accent.color = color;

            if (routine != null)
                StopCoroutine(routine);

            routine =
                StartCoroutine(
                    ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            group.alpha = 0f;

            RectTransform rect =
                group.GetComponent<
                    RectTransform>();

            Vector3 original =
                Vector3.one;

            rect.localScale =
                original * 0.82f;

            float elapsed = 0f;

            while (elapsed < 0.11f)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        elapsed / 0.11f);

                group.alpha = t;

                rect.localScale =
                    original *
                    Mathf.Lerp(
                        0.82f,
                        1.05f,
                        t);

                yield return null;
            }

            elapsed = 0f;

            while (elapsed < 0.09f)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        elapsed / 0.09f);

                rect.localScale =
                    original *
                    Mathf.Lerp(
                        1.05f,
                        1f,
                        t);

                yield return null;
            }

            rect.localScale = original;
            group.alpha = 1f;

            yield return
                new WaitForSecondsRealtime(
                    0.75f);

            elapsed = 0f;

            while (elapsed < 0.25f)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                group.alpha =
                    1f -
                    Mathf.Clamp01(
                        elapsed / 0.25f);

                yield return null;
            }

            group.alpha = 0f;
            routine = null;
        }
    }
}
