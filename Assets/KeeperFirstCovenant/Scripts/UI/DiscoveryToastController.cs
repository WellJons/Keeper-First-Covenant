using System.Collections;
using KeeperFirstCovenant.Discoveries;
using UnityEngine;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public sealed class DiscoveryToastController : MonoBehaviour
    {
        private CanvasGroup group;
        private Text header;
        private Text title;
        private Text detail;
        private Coroutine routine;

        private void Start()
        {
            Build();

            DiscoveryJournal.Instance.Discovered +=
                OnDiscovered;

            group.alpha = 0f;
        }

        private void OnDestroy()
        {
            DiscoveryJournal journal =
                DiscoveryJournal.Current;

            if (journal != null)
            {
                journal.Discovered -=
                    OnDiscovered;
            }
        }

        private void Build()
        {
            var canvasObject =
                new GameObject(
                    "DiscoveryToastCanvas",
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

            canvas.sortingOrder = 655;

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
                    "DiscoveryToast",
                    canvasRect,
                    new Color(
                        MainMenuTheme.Panel.r,
                        MainMenuTheme.Panel.g,
                        MainMenuTheme.Panel.b,
                        0.97f));

            RectTransform panelRect =
                panel.rectTransform;

            MenuUiFactory.SetAnchoredRect(
                panelRect,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -268f),
                new Vector2(590f, 142f));

            KeeperUiSkin.DecorateMajorPanel(
                panel,
                false);

            group =
                panel.gameObject
                    .AddComponent<CanvasGroup>();

            header =
                MenuUiFactory.CreateText(
                    "Header",
                    panel.transform,
                    "ОТКРЫТИЕ",
                    13,
                    MainMenuTheme.Silver,
                    TextAnchor.MiddleCenter);

            header.rectTransform.anchorMin =
                new Vector2(0f, 0.73f);

            header.rectTransform.anchorMax =
                Vector2.one;

            header.rectTransform.offsetMin =
                new Vector2(20f, 0f);

            header.rectTransform.offsetMax =
                new Vector2(-20f, -8f);

            title =
                MenuUiFactory.CreateText(
                    "Title",
                    panel.transform,
                    string.Empty,
                    21,
                    MainMenuTheme.Text,
                    TextAnchor.MiddleCenter);

            title.rectTransform.anchorMin =
                new Vector2(0f, 0.39f);

            title.rectTransform.anchorMax =
                new Vector2(1f, 0.76f);

            title.rectTransform.offsetMin =
                new Vector2(24f, 0f);

            title.rectTransform.offsetMax =
                new Vector2(-24f, 0f);

            detail =
                MenuUiFactory.CreateText(
                    "Detail",
                    panel.transform,
                    string.Empty,
                    13,
                    MainMenuTheme.MutedText,
                    TextAnchor.UpperCenter);

            detail.rectTransform.anchorMin =
                Vector2.zero;

            detail.rectTransform.anchorMax =
                new Vector2(1f, 0.42f);

            detail.rectTransform.offsetMin =
                new Vector2(24f, 10f);

            detail.rectTransform.offsetMax =
                new Vector2(-24f, 0f);
        }

        private void OnDiscovered(
            DiscoveryEntryState entry)
        {
            if (entry == null)
                return;

            header.text =
                CategoryName(entry.category);

            title.text =
                entry.title;

            detail.text =
                string.IsNullOrWhiteSpace(
                    entry.locationName)
                    ? "Запись добавлена в журнал."
                    : entry.locationName +
                      "   •   запись добавлена в журнал";

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

            while (elapsed < 0.18f)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                group.alpha =
                    Mathf.Clamp01(
                        elapsed / 0.18f);

                yield return null;
            }

            group.alpha = 1f;

            yield return
                new WaitForSecondsRealtime(
                    3.0f);

            elapsed = 0f;

            while (elapsed < 0.5f)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                group.alpha =
                    1f -
                    Mathf.Clamp01(
                        elapsed / 0.5f);

                yield return null;
            }

            group.alpha = 0f;
            routine = null;
        }

        private static string CategoryName(
            DiscoveryCategory category)
        {
            switch (category)
            {
                case DiscoveryCategory.Location:
                    return "НОВОЕ МЕСТО";
                case DiscoveryCategory.Lore:
                    return "НОВАЯ ЗАПИСЬ";
                case DiscoveryCategory.Person:
                    return "НОВЫЕ СВЕДЕНИЯ";
                case DiscoveryCategory.Faction:
                    return "НОВАЯ ФРАКЦИЯ";
                case DiscoveryCategory.Creature:
                    return "НОВОЕ СУЩЕСТВО";
                case DiscoveryCategory.Magic:
                    return "МАГИЧЕСКОЕ ЯВЛЕНИЕ";
                case DiscoveryCategory.Clue:
                    return "НАЙДЕНА УЛИКА";
                default:
                    return "ОТКРЫТИЕ";
            }
        }
    }
}
