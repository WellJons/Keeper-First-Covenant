using System.Collections;
using System.Linq;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Relationships;
using UnityEngine;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public sealed class RelationshipToastController :
        MonoBehaviour
    {
        private CanvasGroup group;
        private Text title;
        private Text message;
        private Coroutine routine;

        private void Start()
        {
            Build();

            RelationshipLedger
                .Instance
                .RelationshipChanged +=
                OnRelationshipChanged;

            group.alpha = 0f;
        }

        private void OnDestroy()
        {
            RelationshipLedger ledger =
                RelationshipLedger.Current;

            if (ledger != null)
            {
                ledger.RelationshipChanged -=
                    OnRelationshipChanged;
            }
        }

        private void Build()
        {
            var canvasObject =
                new GameObject(
                    "RelationshipToastCanvas",
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

            canvas.sortingOrder = 6250;

            CanvasScaler scaler =
                canvasObject.GetComponent<CanvasScaler>();

            scaler.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;

            scaler.referenceResolution =
                new Vector2(1920f, 1080f);

            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect =
                canvasObject.GetComponent<RectTransform>();

            Image panel =
                MenuUiFactory.CreateImage(
                    "RelationshipToast",
                    canvasRect,
                    new Color(
                        MainMenuTheme.Panel.r,
                        MainMenuTheme.Panel.g,
                        MainMenuTheme.Panel.b,
                        0.95f));

            RectTransform rect =
                panel.rectTransform;

            MenuUiFactory.SetAnchoredRect(
                rect,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-44f, -52f),
                new Vector2(390f, 102f));

            KeeperUiSkin.DecorateSection(panel);

            group =
                panel.gameObject
                    .AddComponent<CanvasGroup>();

            title =
                MenuUiFactory.CreateText(
                    "Title",
                    panel.transform,
                    string.Empty,
                    17,
                    MainMenuTheme.Text,
                    TextAnchor.MiddleLeft);

            title.rectTransform.anchorMin =
                new Vector2(0f, 0.45f);

            title.rectTransform.anchorMax =
                Vector2.one;

            title.rectTransform.offsetMin =
                new Vector2(18f, 0f);

            title.rectTransform.offsetMax =
                new Vector2(-18f, -8f);

            message =
                MenuUiFactory.CreateText(
                    "Message",
                    panel.transform,
                    string.Empty,
                    13,
                    MainMenuTheme.MutedText,
                    TextAnchor.MiddleLeft);

            message.rectTransform.anchorMin =
                Vector2.zero;

            message.rectTransform.anchorMax =
                new Vector2(1f, 0.48f);

            message.rectTransform.offsetMin =
                new Vector2(18f, 6f);

            message.rectTransform.offsetMax =
                new Vector2(-18f, 0f);
        }

        private void OnRelationshipChanged(
            RelationshipChange change)
        {
            if (change.Delta == 0)
                return;

            string displayName =
                ResolveDisplayName(
                    change.CharacterId);

            title.text = displayName;

            if (change.Delta > 0)
            {
                message.text =
                    Mathf.Abs(change.Delta) >= 8
                        ? "Сильно одобряет ваш выбор."
                        : "Одобряет ваш выбор.";

                message.color =
                    new Color(
                        0.62f,
                        0.80f,
                        0.68f,
                        1f);
            }
            else
            {
                message.text =
                    Mathf.Abs(change.Delta) >= 8
                        ? "Сильно не одобряет ваш выбор."
                        : "Не одобряет ваш выбор.";

                message.color =
                    MainMenuTheme.Warm;
            }

            if (routine != null)
                StopCoroutine(routine);

            routine =
                StartCoroutine(
                    ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            group.alpha = 0f;
            float elapsed = 0f;

            while (elapsed < 0.15f)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                group.alpha =
                    Mathf.Clamp01(
                        elapsed / 0.15f);

                yield return null;
            }

            group.alpha = 1f;

            yield return
                new WaitForSecondsRealtime(
                    2.5f);

            elapsed = 0f;

            while (elapsed < 0.42f)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                group.alpha =
                    1f -
                    Mathf.Clamp01(
                        elapsed / 0.42f);

                yield return null;
            }

            group.alpha = 0f;
            routine = null;
        }

        private static string ResolveDisplayName(
            string characterId)
        {
            CombatantRuntime actor =
                Object
                    .FindObjectsByType<
                        CombatantRuntime>(
                        FindObjectsSortMode.None)
                    .FirstOrDefault(value =>
                        value != null &&
                        value.Definition != null &&
                        string.Equals(
                            value.Definition.characterId,
                            characterId,
                            System.StringComparison.Ordinal));

            if (actor?.Definition != null &&
                !string.IsNullOrWhiteSpace(
                    actor.Definition.displayName))
            {
                return actor.Definition.displayName;
            }

            return string.IsNullOrWhiteSpace(
                    characterId)
                ? "Спутник"
                : characterId;
        }
    }
}
