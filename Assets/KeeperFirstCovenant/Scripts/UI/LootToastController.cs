using System.Collections;
using System.Linq;
using KeeperFirstCovenant.Inventory;
using KeeperFirstCovenant.World;
using UnityEngine;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public sealed class LootToastController : MonoBehaviour
    {
        private CanvasGroup group;
        private RectTransform panelRect;
        private Text title;
        private Text body;
        private Coroutine routine;

        private void Start()
        {
            Build();
            group.alpha = 0f;

            SearchableLoot.LootTransferred +=
                OnLootTransferred;
        }

        private void OnDestroy()
        {
            SearchableLoot.LootTransferred -=
                OnLootTransferred;
        }

        private void Build()
        {
            var canvasObject =
                new GameObject(
                    "LootToastCanvas",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler));

            canvasObject.transform
                .SetParent(
                    transform,
                    false);

            Canvas canvas =
                canvasObject.GetComponent<Canvas>();

            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;

            canvas.sortingOrder = 640;

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
                    "LootToast",
                    canvasRect,
                    new Color(
                        MainMenuTheme.Panel.r,
                        MainMenuTheme.Panel.g,
                        MainMenuTheme.Panel.b,
                        0.96f));

            panelRect =
                panel.rectTransform;

            MenuUiFactory.SetAnchoredRect(
                panelRect,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-42f, -126f),
                new Vector2(410f, 180f));

            KeeperUiSkin.DecorateMajorPanel(
                panel,
                true);

            group =
                panel.gameObject
                    .AddComponent<CanvasGroup>();

            title =
                MenuUiFactory.CreateText(
                    "Title",
                    panel.transform,
                    "ДОБЫЧА",
                    17,
                    MainMenuTheme.Warm,
                    TextAnchor.MiddleLeft);

            title.rectTransform.anchorMin =
                new Vector2(0f, 0.73f);

            title.rectTransform.anchorMax =
                Vector2.one;

            title.rectTransform.offsetMin =
                new Vector2(22f, 0f);

            title.rectTransform.offsetMax =
                new Vector2(-18f, -12f);

            body =
                MenuUiFactory.CreateText(
                    "Body",
                    panel.transform,
                    string.Empty,
                    14,
                    MainMenuTheme.Text,
                    TextAnchor.UpperLeft);

            body.supportRichText = true;

            body.rectTransform.anchorMin =
                Vector2.zero;

            body.rectTransform.anchorMax =
                new Vector2(1f, 0.76f);

            body.rectTransform.offsetMin =
                new Vector2(22f, 14f);

            body.rectTransform.offsetMax =
                new Vector2(-18f, 0f);
        }

        private void OnLootTransferred(
            SearchableLoot source,
            GameObject actor,
            LootTransferReport report)
        {
            if (report == null)
                return;

            string[] lines =
                report.Collected != null
                    ? report.Collected
                        .Where(stack =>
                            stack?.item != null &&
                            stack.amount > 0)
                        .Take(4)
                        .Select(FormatStack)
                        .ToArray()
                    : new string[0];

            if (lines.Length == 0 &&
                !report.HasRemaining)
            {
                title.text = "ПУСТО";
                body.text =
                    "Ничего полезного не найдено.";
            }
            else
            {
                title.text = "ДОБЫЧА";

                body.text =
                    lines.Length > 0
                        ? string.Join("\n", lines)
                        : string.Empty;

                if (report.HasRemaining)
                {
                    if (!string.IsNullOrWhiteSpace(
                            body.text))
                    {
                        body.text += "\n";
                    }

                    body.text +=
                        "<color=#B56B52>Часть добычи осталась — не хватает места.</color>";
                }
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

            while (elapsed < 0.16f)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                group.alpha =
                    Mathf.Clamp01(
                        elapsed / 0.16f);

                yield return null;
            }

            group.alpha = 1f;

            yield return
                new WaitForSecondsRealtime(
                    2.8f);

            elapsed = 0f;

            while (elapsed < 0.45f)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                group.alpha =
                    1f -
                    Mathf.Clamp01(
                        elapsed / 0.45f);

                yield return null;
            }

            group.alpha = 0f;
            routine = null;
        }

        private static string FormatStack(
            InventoryStack stack)
        {
            Color rarity =
                KeeperUiSkin.GetRarityColor(
                    stack.item.rarity);

            string hex =
                ColorUtility.ToHtmlStringRGB(
                    rarity);

            string amount =
                stack.amount > 1
                    ? "  ×" + stack.amount
                    : string.Empty;

            return
                "<color=#" +
                hex +
                ">◆</color> " +
                stack.item.displayName +
                amount;
        }
    }
}
