using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public sealed class DefeatScreenController :
        MonoBehaviour
    {
        private GameObject root;
        private Button loadButton;

        private bool shown;
        private float previousTimeScale = 1f;
        private CursorLockMode previousCursorLock;
        private bool previousCursorVisible;

        private void Start()
        {
            Build();
            root.SetActive(false);
        }

        private void Update()
        {
            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            bool defeated =
                director != null &&
                director.State ==
                    CombatState.Defeat;

            if (defeated && !shown)
            {
                Show();
            }
            else if (!defeated && shown)
            {
                Hide(false);
            }
        }

        private void Build()
        {
            EnsureEventSystem();

            var canvasObject =
                new GameObject(
                    "DefeatCanvas",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster));

            canvasObject.transform
                .SetParent(
                    transform,
                    false);

            Canvas canvas =
                canvasObject
                    .GetComponent<Canvas>();

            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;

            canvas.sortingOrder = 9000;

            CanvasScaler scaler =
                canvasObject
                    .GetComponent<
                        CanvasScaler>();

            scaler.uiScaleMode =
                CanvasScaler.ScaleMode
                    .ScaleWithScreenSize;

            scaler.referenceResolution =
                new Vector2(
                    1920f,
                    1080f);

            scaler.matchWidthOrHeight =
                0.5f;

            RectTransform canvasRect =
                canvasObject
                    .GetComponent<
                        RectTransform>();

            root =
                MenuUiFactory.CreateRect(
                    "DefeatRoot",
                    canvasRect)
                    .gameObject;

            MenuUiFactory.Stretch(
                root.GetComponent<
                    RectTransform>());

            Image backdrop =
                root.AddComponent<Image>();

            backdrop.color =
                new Color(
                    0.006f,
                    0.008f,
                    0.011f,
                    0.94f);

            Image vignette =
                MenuUiFactory.CreateImage(
                    "CenterPanel",
                    root.transform,
                    new Color(
                        MainMenuTheme.Panel.r,
                        MainMenuTheme.Panel.g,
                        MainMenuTheme.Panel.b,
                        0.72f));

            KeeperUiSkin.DecorateMajorPanel(
                vignette,
                true);

            RectTransform panelRect =
                vignette.rectTransform;

            panelRect.anchorMin =
                panelRect.anchorMax =
                    new Vector2(0.5f, 0.5f);

            panelRect.pivot =
                new Vector2(0.5f, 0.5f);

            panelRect.sizeDelta =
                new Vector2(
                    680f,
                    430f);

            Text title =
                MenuUiFactory.CreateText(
                    "Title",
                    vignette.transform,
                    "ПОРАЖЕНИЕ",
                    48,
                    MainMenuTheme.Text,
                    TextAnchor.MiddleCenter);

            title.rectTransform.anchorMin =
                new Vector2(0f, 0.67f);

            title.rectTransform.anchorMax =
                Vector2.one;

            title.rectTransform.offsetMin =
                new Vector2(20f, 0f);

            title.rectTransform.offsetMax =
                new Vector2(-20f, -28f);

            Image line =
                MenuUiFactory.CreateImage(
                    "Line",
                    vignette.transform,
                    MainMenuTheme.WarmSoft);

            RectTransform lineRect =
                line.rectTransform;

            lineRect.anchorMin =
                lineRect.anchorMax =
                    new Vector2(0.5f, 0.64f);

            lineRect.pivot =
                new Vector2(0.5f, 0.5f);

            lineRect.sizeDelta =
                new Vector2(180f, 2f);

            Text message =
                MenuUiFactory.CreateText(
                    "Message",
                    vignette.transform,
                    "Путь оборвался здесь.\nПоследнее сохранение осталось нетронутым.",
                    18,
                    MainMenuTheme.MutedText,
                    TextAnchor.MiddleCenter);

            message.rectTransform.anchorMin =
                new Vector2(0f, 0.40f);

            message.rectTransform.anchorMax =
                new Vector2(1f, 0.64f);

            message.rectTransform.offsetMin =
                new Vector2(40f, 0f);

            message.rectTransform.offsetMax =
                new Vector2(-40f, 0f);

            loadButton =
                MenuUiFactory.CreateMenuButton(
                    "LoadLast",
                    vignette.transform,
                    "Загрузить последнее сохранение",
                    18);

            RectTransform loadRect =
                loadButton
                    .GetComponent<
                        RectTransform>();

            loadRect.anchorMin =
                loadRect.anchorMax =
                    new Vector2(0.5f, 0.25f);

            loadRect.pivot =
                new Vector2(0.5f, 0.5f);

            loadRect.sizeDelta =
                new Vector2(390f, 54f);

            loadButton.onClick
                .AddListener(
                    LoadLastSave);

            Button menuButton =
                MenuUiFactory.CreateMenuButton(
                    "MainMenu",
                    vignette.transform,
                    "Главное меню",
                    18);

            RectTransform menuRect =
                menuButton
                    .GetComponent<
                        RectTransform>();

            menuRect.anchorMin =
                menuRect.anchorMax =
                    new Vector2(0.5f, 0.09f);

            menuRect.pivot =
                new Vector2(0.5f, 0.5f);

            menuRect.sizeDelta =
                new Vector2(390f, 54f);

            menuButton.onClick
                .AddListener(
                    ReturnToMenu);
        }

        private void Show()
        {
            shown = true;

            previousTimeScale =
                Mathf.Max(
                    0.0001f,
                    Time.timeScale);

            previousCursorLock =
                Cursor.lockState;

            previousCursorVisible =
                Cursor.visible;

            Time.timeScale = 0f;
            Cursor.lockState =
                CursorLockMode.None;
            Cursor.visible = true;

            root.SetActive(true);

            bool hasActiveSave =
                GameFlowController
                    .Instance
                    .ActiveSlotId > 0 &&
                SaveGameService.LoadSlot(
                    GameFlowController
                        .Instance
                        .ActiveSlotId) != null;

            loadButton.interactable =
                hasActiveSave;

            if (EventSystem.current != null)
            {
                EventSystem.current
                    .SetSelectedGameObject(
                        hasActiveSave
                            ? loadButton.gameObject
                            : null);
            }
        }

        private void LoadLastSave()
        {
            GameAudioService.Instance
                .PlayClick();

            Hide(true);

            if (!GameFlowController
                    .Instance
                    .LoadActiveSlot())
            {
                GameFlowController
                    .Instance
                    .ReturnToMainMenu(
                        false);
            }
        }

        private void ReturnToMenu()
        {
            GameAudioService.Instance
                .PlayClick();

            Hide(true);

            GameFlowController.Instance
                .ReturnToMainMenu(
                    false);
        }

        private void Hide(
            bool restoreGameplayState)
        {
            if (!shown)
                return;

            shown = false;

            if (root != null)
                root.SetActive(false);

            if (!restoreGameplayState)
                return;

            Time.timeScale =
                previousTimeScale > 0f
                    ? previousTimeScale
                    : 1f;

            Cursor.lockState =
                previousCursorLock;

            Cursor.visible =
                previousCursorVisible;
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;

            var eventSystem =
                new GameObject(
                    "EventSystem",
                    typeof(EventSystem),
                    typeof(
                        InputSystemUIInputModule));

            eventSystem.transform
                .SetParent(
                    transform,
                    false);
        }
    }
}
