using System.Linq;
using KeeperFirstCovenant.Quests;
using UnityEngine;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public sealed class QuestTrackerHud : MonoBehaviour
    {
        private CanvasGroup group;
        private Text title;
        private Text objectives;

        private void Start()
        {
            Build();

            QuestJournal.Instance.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (QuestJournal.Current != null)
                QuestJournal.Current.Changed -= Refresh;
        }

        private void Build()
        {
            var canvasObject = new GameObject(
                "QuestTrackerCanvas",
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

            canvas.sortingOrder = 250;

            CanvasScaler scaler =
                canvasObject.GetComponent<CanvasScaler>();

            scaler.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;

            scaler.referenceResolution =
                new Vector2(1920f, 1080f);

            RectTransform canvasRect =
                canvasObject.GetComponent<RectTransform>();

            Image panel = MenuUiFactory.CreateImage(
                "QuestTracker",
                canvasRect,
                new Color(0.02f, 0.025f, 0.03f, 0.66f));

            KeeperUiSkin.DecorateSection(
                panel);

            RectTransform panelRect =
                panel.rectTransform;

            MenuUiFactory.SetAnchoredRect(
                panelRect,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-52f, -54f),
                new Vector2(410f, 220f));

            group =
                panel.gameObject.AddComponent<CanvasGroup>();

            Image accent = MenuUiFactory.CreateImage(
                "Accent",
                panel.transform,
                MainMenuTheme.SilverDim);

            RectTransform accentRect =
                accent.rectTransform;

            accentRect.anchorMin =
                new Vector2(0f, 0f);
            accentRect.anchorMax =
                new Vector2(0f, 1f);
            accentRect.pivot =
                new Vector2(0f, 0.5f);
            accentRect.sizeDelta =
                new Vector2(2f, 0f);
            accentRect.anchoredPosition =
                Vector2.zero;

            title = MenuUiFactory.CreateText(
                "Title",
                panel.transform,
                string.Empty,
                18,
                MainMenuTheme.Text,
                TextAnchor.UpperLeft);

            title.rectTransform.anchorMin =
                new Vector2(0f, 0.70f);
            title.rectTransform.anchorMax =
                Vector2.one;
            title.rectTransform.offsetMin =
                new Vector2(20f, 0f);
            title.rectTransform.offsetMax =
                new Vector2(-18f, -16f);

            objectives = MenuUiFactory.CreateText(
                "Objectives",
                panel.transform,
                string.Empty,
                14,
                MainMenuTheme.MutedText,
                TextAnchor.UpperLeft);

            objectives.rectTransform.anchorMin =
                Vector2.zero;
            objectives.rectTransform.anchorMax =
                new Vector2(1f, 0.72f);
            objectives.rectTransform.offsetMin =
                new Vector2(20f, 14f);
            objectives.rectTransform.offsetMax =
                new Vector2(-18f, 0f);
        }

        private void Refresh()
        {
            QuestEntryState quest =
                QuestJournal.Instance.GetTrackedQuest();

            if (quest == null)
            {
                group.alpha = 0f;
                return;
            }

            group.alpha = 1f;
            title.text = quest.title;

            if (quest.objectives == null ||
                quest.objectives.Count == 0)
            {
                objectives.text = quest.description;
                return;
            }

            string[] lines =
                quest.objectives
                    .Where(value =>
                        value != null &&
                        !value.completed)
                    .Take(3)
                    .Select(FormatObjective)
                    .ToArray();

            objectives.text =
                lines.Length > 0
                    ? string.Join("\n", lines)
                    : "Цель выполнена";
        }

        private static string FormatObjective(
            QuestObjectiveState objective)
        {
            string prefix =
                objective.optional
                    ? "◇ "
                    : "• ";

            if (objective.requiredAmount > 1)
            {
                return prefix +
                       objective.description +
                       $"  {objective.currentAmount}/{objective.requiredAmount}";
            }

            return prefix + objective.description;
        }
    }
}
