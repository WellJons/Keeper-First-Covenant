using System.Collections;
using System.Linq;
using KeeperFirstCovenant.Quests;
using UnityEngine;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public sealed class QuestToastController : MonoBehaviour
    {
        private CanvasGroup group;
        private Text header;
        private Text title;
        private Text objective;
        private Coroutine routine;

        private void Start()
        {
            Build();

            QuestJournal journal =
                QuestJournal.Instance;

            journal.QuestStarted +=
                OnQuestStarted;

            journal.QuestUpdated +=
                OnQuestUpdated;

            journal.QuestCompleted +=
                OnQuestCompleted;

            journal.QuestFailed +=
                OnQuestFailed;

            group.alpha = 0f;
        }

        private void OnDestroy()
        {
            QuestJournal journal =
                QuestJournal.Current;

            if (journal == null)
                return;

            journal.QuestStarted -=
                OnQuestStarted;

            journal.QuestUpdated -=
                OnQuestUpdated;

            journal.QuestCompleted -=
                OnQuestCompleted;

            journal.QuestFailed -=
                OnQuestFailed;
        }

        private void Build()
        {
            var canvasObject =
                new GameObject(
                    "QuestToastCanvas",
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

            canvas.sortingOrder = 650;

            CanvasScaler scaler =
                canvasObject.GetComponent<
                    CanvasScaler>();

            scaler.uiScaleMode =
                CanvasScaler.ScaleMode
                    .ScaleWithScreenSize;

            scaler.referenceResolution =
                new Vector2(1920f, 1080f);

            scaler.matchWidthOrHeight =
                0.5f;

            RectTransform canvasRect =
                canvasObject.GetComponent<
                    RectTransform>();

            Image panel =
                MenuUiFactory.CreateImage(
                    "QuestToast",
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
                new Vector2(0f, -100f),
                new Vector2(560f, 150f));

            KeeperUiSkin.DecorateMajorPanel(
                panel,
                true);

            group =
                panel.gameObject
                    .AddComponent<CanvasGroup>();

            header =
                MenuUiFactory.CreateText(
                    "Header",
                    panel.transform,
                    string.Empty,
                    13,
                    MainMenuTheme.Warm,
                    TextAnchor.MiddleCenter);

            header.rectTransform.anchorMin =
                new Vector2(0f, 0.72f);

            header.rectTransform.anchorMax =
                Vector2.one;

            header.rectTransform.offsetMin =
                new Vector2(18f, 0f);

            header.rectTransform.offsetMax =
                new Vector2(-18f, -8f);

            title =
                MenuUiFactory.CreateText(
                    "Title",
                    panel.transform,
                    string.Empty,
                    20,
                    MainMenuTheme.Text,
                    TextAnchor.MiddleCenter);

            title.rectTransform.anchorMin =
                new Vector2(0f, 0.39f);

            title.rectTransform.anchorMax =
                new Vector2(1f, 0.75f);

            title.rectTransform.offsetMin =
                new Vector2(24f, 0f);

            title.rectTransform.offsetMax =
                new Vector2(-24f, 0f);

            objective =
                MenuUiFactory.CreateText(
                    "Objective",
                    panel.transform,
                    string.Empty,
                    13,
                    MainMenuTheme.MutedText,
                    TextAnchor.UpperCenter);

            objective.rectTransform.anchorMin =
                Vector2.zero;

            objective.rectTransform.anchorMax =
                new Vector2(1f, 0.42f);

            objective.rectTransform.offsetMin =
                new Vector2(24f, 12f);

            objective.rectTransform.offsetMax =
                new Vector2(-24f, 0f);
        }

        private void OnQuestStarted(
            QuestEntryState quest)
        {
            Show(
                "НОВОЕ ЗАДАНИЕ",
                quest,
                MainMenuTheme.Warm);
        }

        private void OnQuestUpdated(
            QuestEntryState quest)
        {
            Show(
                "ЗАДАНИЕ ОБНОВЛЕНО",
                quest,
                MainMenuTheme.Silver);
        }

        private void OnQuestCompleted(
            QuestEntryState quest)
        {
            Show(
                "ЗАДАНИЕ ЗАВЕРШЕНО",
                quest,
                new Color(
                    0.52f,
                    0.78f,
                    0.58f,
                    1f));
        }

        private void OnQuestFailed(
            QuestEntryState quest)
        {
            Show(
                "ЗАДАНИЕ ПРОВАЛЕНО",
                quest,
                MainMenuTheme.Danger);
        }

        private void Show(
            string state,
            QuestEntryState quest,
            Color stateColor)
        {
            if (quest == null)
                return;

            header.text = state;
            header.color = stateColor;
            title.text = quest.title;

            QuestObjectiveState active =
                quest.objectives?
                    .FirstOrDefault(value =>
                        value != null &&
                        !value.completed);

            objective.text =
                active != null
                    ? active.description +
                      (active.requiredAmount > 1
                          ? $"   {active.currentAmount}/{active.requiredAmount}"
                          : string.Empty)
                    : quest.status ==
                        QuestStatus.Completed
                        ? "Все обязательные цели выполнены."
                        : quest.status ==
                            QuestStatus.Failed
                            ? "Эта ветка больше недоступна."
                            : string.Empty;

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
                    3.1f);

            elapsed = 0f;

            while (elapsed < 0.55f)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                group.alpha =
                    1f -
                    Mathf.Clamp01(
                        elapsed / 0.55f);

                yield return null;
            }

            group.alpha = 0f;
            routine = null;
        }
    }
}
