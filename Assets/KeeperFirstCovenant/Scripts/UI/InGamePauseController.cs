using System;
using System.Collections.Generic;
using System.Linq;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Core;
using KeeperFirstCovenant.Dialogue;
using KeeperFirstCovenant.Quests;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public sealed class InGamePauseController : MonoBehaviour
    {
        private Canvas canvas;
        private GameObject root;
        private GameObject mainPanel;
        private GameObject loadPanel;
        private GameObject journalPanel;
        private CharacterInventoryPanel characterInventoryPanel;
        private GameObject settingsPanel;
        private GameObject confirmPanel;
        private RectTransform loadList;
        private RectTransform journalList;

        private Text statusText;
        private Text activeSaveText;
        private Text confirmMessage;
        private Text confirmActionLabel;
        private Action confirmAction;

        private Slider masterSlider;
        private Slider musicSlider;
        private Slider sfxSlider;
        private Toggle vSyncToggle;
        private Toggle cameraShakeToggle;
        private Dropdown qualityDropdown;

        private float previousTimeScale = 1f;
        private CursorLockMode previousCursorLock;
        private bool previousCursorVisible;
        private bool paused;
        private float statusTimer;

        public bool IsPaused => paused;

        private GameFlowController Flow => GameFlowController.Instance;

        private void Start()
        {
            Build();
            root.SetActive(false);

            Flow.GameSaved += OnGameSaved;
            Flow.FlowError += OnFlowError;
        }

        private void OnDestroy()
        {
            Flow.GameSaved -= OnGameSaved;
            Flow.FlowError -= OnFlowError;

            if (paused)
                RestoreGameplayState();
        }

        private void Update()
        {
            if (Flow.IsTransitioning ||
                DialogueRunner.IsDialogueActive)
            {
                return;
            }

            if (!paused &&
                Keyboard.current != null &&
                Keyboard.current.iKey.wasPressedThisFrame)
            {
                OpenPause();
                OpenCharacterInventory();
                return;
            }

            bool pausePressed =
                Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame;

            if (!pausePressed &&
                Gamepad.current != null &&
                Gamepad.current.startButton.wasPressedThisFrame)
            {
                pausePressed = true;
            }

            if (pausePressed)
            {
                if (!paused)
                {
                    OpenPause();
                }
                else if (confirmPanel.activeSelf)
                {
                    HideConfirm();
                }
                else if (loadPanel.activeSelf)
                {
                    loadPanel.SetActive(false);
                    mainPanel.SetActive(true);
                }
                else if (characterInventoryPanel != null &&
                         characterInventoryPanel.IsActive)
                {
                    characterInventoryPanel.Hide();
                    mainPanel.SetActive(true);
                }
                else if (journalPanel.activeSelf)
                {
                    journalPanel.SetActive(false);
                    mainPanel.SetActive(true);
                }
                else if (settingsPanel.activeSelf)
                {
                    settingsPanel.SetActive(false);
                    mainPanel.SetActive(true);
                }
                else
                {
                    Resume();
                }
            }

            if (!paused &&
                Keyboard.current != null &&
                Keyboard.current.f5Key.wasPressedThisFrame)
            {
                QuickSave();
            }

            if (statusTimer > 0f)
            {
                statusTimer -= Time.unscaledDeltaTime;
                if (statusTimer <= 0f && statusText != null)
                    statusText.text = string.Empty;
            }
        }

        private void Build()
        {
            EnsureEventSystem();

            var canvasObject = new GameObject(
                "PauseCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            canvasObject.transform.SetParent(transform, false);

            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 7000;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();

            root = MenuUiFactory.CreateRect("PauseRoot", canvasRect).gameObject;
            MenuUiFactory.Stretch(root.GetComponent<RectTransform>());

            Image dim = root.AddComponent<Image>();
            dim.color = new Color(0.008f, 0.012f, 0.016f, 0.88f);

            BuildDecor(root.transform);

            // Status/toast is intentionally re-parented outside PauseRoot so F5
            // feedback stays visible while gameplay itself is not paused.
            if (statusText != null)
            {
                statusText.transform.SetParent(canvasRect, false);
                MenuUiFactory.SetAnchoredRect(
                    statusText.rectTransform,
                    new Vector2(0f, 0f),
                    new Vector2(0f, 0f),
                    new Vector2(0f, 0f),
                    new Vector2(54f, 42f),
                    new Vector2(720f, 42f));
            }

            mainPanel = BuildMainPanel(root.transform);
            loadPanel = BuildLoadPanel(root.transform);
            journalPanel = BuildJournalPanel(root.transform);

            characterInventoryPanel =
                new CharacterInventoryPanel();

            characterInventoryPanel.Build(
                root.transform,
                () =>
                {
                    mainPanel.SetActive(true);
                    SelectFirstButton(mainPanel);
                });

            settingsPanel = BuildSettingsPanel(root.transform);
            confirmPanel = BuildConfirmPanel(root.transform);

            loadPanel.SetActive(false);
            journalPanel.SetActive(false);
            characterInventoryPanel?.Hide();
            settingsPanel.SetActive(false);
            confirmPanel.SetActive(false);
        }

        private void BuildDecor(Transform parent)
        {
            Text title = MenuUiFactory.CreateText(
                "PauseTitle",
                parent,
                "ПАУЗА",
                20,
                MainMenuTheme.Silver,
                TextAnchor.MiddleLeft);

            MenuUiFactory.SetAnchoredRect(
                title.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(100f, -60f),
                new Vector2(360f, 42f));

            Text worldTitle = MenuUiFactory.CreateText(
                "WorldTitle",
                parent,
                "ХРАНИТЕЛЬ\nПЕРВЫЙ ЗАВЕТ",
                42,
                new Color(MainMenuTheme.Text.r, MainMenuTheme.Text.g, MainMenuTheme.Text.b, 0.16f),
                TextAnchor.LowerRight,
                FontStyle.Normal);

            MenuUiFactory.SetAnchoredRect(
                worldTitle.rectTransform,
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-78f, 68f),
                new Vector2(620f, 170f));

            statusText = MenuUiFactory.CreateText(
                "Status",
                parent,
                string.Empty,
                16,
                MainMenuTheme.Silver,
                TextAnchor.MiddleLeft);

            MenuUiFactory.SetAnchoredRect(
                statusText.rectTransform,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(100f, 38f),
                new Vector2(720f, 40f));
        }

        private GameObject BuildMainPanel(Transform parent)
        {
            RectTransform panel = MenuUiFactory.CreateRect("MainPanel", parent);
            MenuUiFactory.SetAnchoredRect(
                panel,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(100f, -10f),
                new Vector2(440f, 600f));

            Image background = panel.gameObject.AddComponent<Image>();
            background.color = new Color(MainMenuTheme.Panel.r, MainMenuTheme.Panel.g, MainMenuTheme.Panel.b, 0.78f);

            VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(28, 28, 28, 28);
            layout.spacing = 11f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            Text heading = MenuUiFactory.CreateText(
                "Heading",
                panel,
                "ПЕРВЫЙ ЗАВЕТ",
                29,
                MainMenuTheme.Text,
                TextAnchor.MiddleLeft);

            heading.gameObject.AddComponent<LayoutElement>().preferredHeight = 54f;

            activeSaveText = MenuUiFactory.CreateText(
                "ActiveSave",
                panel,
                string.Empty,
                14,
                MainMenuTheme.MutedText,
                TextAnchor.UpperLeft);

            activeSaveText.gameObject.AddComponent<LayoutElement>().preferredHeight = 54f;

            AddButton(panel, "Resume", "Продолжить", Resume);
            AddButton(panel, "Save", "Сохранить игру", SaveGame);
            AddButton(panel, "Load", "Загрузить игру", OpenLoadPanel);
            AddButton(
                panel,
                "Characters",
                "Персонажи и инвентарь",
                OpenCharacterInventory);
            AddButton(panel, "Journal", "Журнал", OpenJournal);
            AddButton(panel, "Settings", "Настройки", OpenSettings);
            AddButton(panel, "MainMenu", "Главное меню", RequestMainMenu);
            AddButton(panel, "Quit", "Выход из игры", RequestQuit);

            return panel.gameObject;
        }

        private void AddButton(Transform parent, string name, string label, Action action)
        {
            Button button = MenuUiFactory.CreateMenuButton(name, parent, label, 21);
            LayoutElement element = button.gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = 58f;
            element.minHeight = 58f;

            button.onClick.AddListener(() =>
            {
                GameAudioService.Instance.PlayClick();
                action?.Invoke();
            });
        }

        private GameObject BuildLoadPanel(Transform parent)
        {
            GameObject panel = CreateWindow(
                parent,
                "LoadPanel",
                "ЗАГРУЗИТЬ ИГРУ",
                out RectTransform content,
                out Button back);

            back.onClick.AddListener(() =>
            {
                GameAudioService.Instance.PlayClick();
                loadPanel.SetActive(false);
                mainPanel.SetActive(true);
            });

            loadList = MenuUiFactory.CreateRect("Slots", content);
            MenuUiFactory.Stretch(loadList);

            VerticalLayoutGroup layout = loadList.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            return panel;
        }

        private void RefreshLoadSlots()
        {
            for (int i = loadList.childCount - 1; i >= 0; i--)
                Destroy(loadList.GetChild(i).gameObject);

            for (int slot = 1; slot <= SaveGameService.MaxSlots; slot++)
            {
                SaveGameData data = SaveGameService.LoadSlot(slot);

                Image row = MenuUiFactory.CreateImage(
                    "Slot_" + slot,
                    loadList,
                    data != null
                        ? new Color(0.045f, 0.052f, 0.058f, 0.94f)
                        : new Color(0.03f, 0.035f, 0.04f, 0.55f));

                row.gameObject.AddComponent<LayoutElement>().preferredHeight = 84f;

                Text label = MenuUiFactory.CreateText(
                    "Label",
                    row.transform,
                    data != null
                        ? $"Слот {slot}  —  {data.locationName}\n{SaveGameService.FormatTimestamp(data)}   •   {SaveGameService.FormatPlayTime(data.playTimeSeconds)}"
                        : $"Слот {slot}  —  пусто",
                    17,
                    data != null ? MainMenuTheme.Text : MainMenuTheme.DisabledText,
                    TextAnchor.MiddleLeft);

                label.rectTransform.anchorMin = Vector2.zero;
                label.rectTransform.anchorMax = new Vector2(0.72f, 1f);
                label.rectTransform.offsetMin = new Vector2(18f, 8f);
                label.rectTransform.offsetMax = new Vector2(-8f, -8f);

                if (data == null)
                    continue;

                SaveGameData captured = data;

                Button load = MenuUiFactory.CreateMenuButton("Load", row.transform, "Загрузить", 16);
                RectTransform rect = load.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
                rect.pivot = new Vector2(1f, 0.5f);
                rect.anchoredPosition = new Vector2(-18f, 0f);
                rect.sizeDelta = new Vector2(180f, 48f);

                load.onClick.AddListener(() =>
                {
                    GameAudioService.Instance.PlayClick();
                    ShowConfirm(
                        $"Загрузить слот {captured.slotId}?\nНесохранённый прогресс будет потерян.",
                        "Загрузить",
                        () =>
                        {
                            RestoreGameplayState();
                            paused = false;
                            root.SetActive(false);
                            Flow.LoadSave(captured);
                        });
                });
            }
        }

        private GameObject BuildJournalPanel(Transform parent)
        {
            GameObject panel = CreateWindow(
                parent,
                "JournalPanel",
                "ЖУРНАЛ",
                out RectTransform content,
                out Button back);

            back.onClick.AddListener(() =>
            {
                GameAudioService.Instance.PlayClick();
                journalPanel.SetActive(false);
                mainPanel.SetActive(true);
            });

            journalList =
                MenuUiFactory.CreateRect(
                    "QuestList",
                    content);

            MenuUiFactory.Stretch(journalList);

            VerticalLayoutGroup layout =
                journalList.gameObject
                    .AddComponent<VerticalLayoutGroup>();

            layout.spacing = 12f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            return panel;
        }

        private void RefreshJournal()
        {
            for (int i = journalList.childCount - 1;
                 i >= 0;
                 i--)
            {
                Destroy(
                    journalList.GetChild(i).gameObject);
            }

            IReadOnlyList<QuestEntryState> quests =
                QuestJournal.Instance.Quests;

            if (quests == null || quests.Count == 0)
            {
                Text empty = MenuUiFactory.CreateText(
                    "Empty",
                    journalList,
                    "Журнал пока пуст.",
                    19,
                    MainMenuTheme.MutedText,
                    TextAnchor.MiddleCenter);

                empty.gameObject
                    .AddComponent<LayoutElement>()
                    .preferredHeight = 120f;

                return;
            }

            foreach (QuestEntryState quest in quests
                         .Where(value => value != null)
                         .OrderBy(value => value.status)
                         .ThenBy(value => value.title))
            {
                BuildQuestRow(quest);
            }
        }

        private void BuildQuestRow(
            QuestEntryState quest)
        {
            Image row = MenuUiFactory.CreateImage(
                "Quest_" + quest.questId,
                journalList,
                new Color(
                    MainMenuTheme.PanelSoft.r,
                    MainMenuTheme.PanelSoft.g,
                    MainMenuTheme.PanelSoft.b,
                    0.92f));

            LayoutElement element =
                row.gameObject
                    .AddComponent<LayoutElement>();

            int objectiveCount =
                quest.objectives != null
                    ? quest.objectives.Count
                    : 0;

            element.preferredHeight =
                Mathf.Clamp(
                    116f + objectiveCount * 28f,
                    116f,
                    250f);

            string status =
                quest.status == QuestStatus.Completed
                    ? "ЗАВЕРШЕНО"
                    : quest.status == QuestStatus.Failed
                        ? "ПРОВАЛЕНО"
                        : quest.tracked
                            ? "ОТСЛЕЖИВАЕТСЯ"
                            : "АКТИВНО";

            Text heading = MenuUiFactory.CreateText(
                "Heading",
                row.transform,
                quest.title + "   •   " + status,
                19,
                quest.status == QuestStatus.Active
                    ? MainMenuTheme.Text
                    : MainMenuTheme.MutedText,
                TextAnchor.UpperLeft);

            heading.rectTransform.anchorMin =
                new Vector2(0f, 0.74f);
            heading.rectTransform.anchorMax =
                Vector2.one;
            heading.rectTransform.offsetMin =
                new Vector2(18f, 0f);
            heading.rectTransform.offsetMax =
                new Vector2(-18f, -12f);

            string objectiveText =
                quest.objectives == null ||
                quest.objectives.Count == 0
                    ? quest.description
                    : string.Join(
                        "\n",
                        quest.objectives
                            .Where(value => value != null)
                            .Select(value =>
                                (value.completed ? "✓ " : "• ") +
                                value.description +
                                (value.requiredAmount > 1
                                    ? $"  {value.currentAmount}/{value.requiredAmount}"
                                    : string.Empty)));

            Text objectives = MenuUiFactory.CreateText(
                "Objectives",
                row.transform,
                objectiveText,
                14,
                MainMenuTheme.MutedText,
                TextAnchor.UpperLeft);

            objectives.rectTransform.anchorMin =
                Vector2.zero;
            objectives.rectTransform.anchorMax =
                new Vector2(1f, 0.76f);
            objectives.rectTransform.offsetMin =
                new Vector2(18f, 12f);
            objectives.rectTransform.offsetMax =
                new Vector2(-18f, 0f);

            if (quest.status == QuestStatus.Active)
            {
                Button track = MenuUiFactory.CreateMenuButton(
                    "Track",
                    row.transform,
                    quest.tracked
                        ? "Отслеживается"
                        : "Отслеживать",
                    14);

                RectTransform rect =
                    track.GetComponent<RectTransform>();

                rect.anchorMin =
                    rect.anchorMax =
                        new Vector2(1f, 1f);

                rect.pivot =
                    new Vector2(1f, 1f);

                rect.anchoredPosition =
                    new Vector2(-14f, -12f);

                rect.sizeDelta =
                    new Vector2(150f, 38f);

                track.interactable = !quest.tracked;

                string capturedId = quest.questId;

                track.onClick.AddListener(() =>
                {
                    GameAudioService.Instance.PlayClick();
                    QuestJournal.Instance.SetTracked(
                        capturedId,
                        true);
                    RefreshJournal();
                });
            }
        }

        private GameObject BuildSettingsPanel(Transform parent)
        {
            GameObject panel = CreateWindow(
                parent,
                "SettingsPanel",
                "НАСТРОЙКИ",
                out RectTransform content,
                out Button back);

            back.onClick.AddListener(() =>
            {
                GameAudioService.Instance.PlayClick();
                ApplySettings();
                settingsPanel.SetActive(false);
                mainPanel.SetActive(true);
            });

            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            masterSlider = AddSliderSetting(content, "Общая громкость");
            musicSlider = AddSliderSetting(content, "Музыка");
            sfxSlider = AddSliderSetting(content, "Эффекты / интерфейс");

            vSyncToggle = AddToggleSetting(content, "Вертикальная синхронизация");
            cameraShakeToggle = AddToggleSetting(content, "Тряска камеры");

            RectTransform qualityRow = CreateSettingRow(content, "Качество");
            qualityDropdown = MenuUiFactory.CreateDropdown("Quality", qualityRow, 17);
            qualityDropdown.gameObject.AddComponent<LayoutElement>().preferredWidth = 380f;

            Button apply = MenuUiFactory.CreateMenuButton("Apply", content, "Применить", 18);
            apply.gameObject.AddComponent<LayoutElement>().preferredHeight = 54f;
            apply.onClick.AddListener(() =>
            {
                GameAudioService.Instance.PlayClick();
                ApplySettings();
                ShowStatus("Настройки применены.");
            });

            return panel;
        }

        private Slider AddSliderSetting(Transform parent, string label)
        {
            RectTransform row = CreateSettingRow(parent, label);
            Slider slider = MenuUiFactory.CreateSlider("Slider", row, 0f, 1f, 1f);
            LayoutElement element = slider.gameObject.AddComponent<LayoutElement>();
            element.preferredWidth = 380f;
            element.preferredHeight = 42f;
            return slider;
        }

        private Toggle AddToggleSetting(Transform parent, string label)
        {
            RectTransform row = CreateSettingRow(parent, string.Empty);
            Toggle toggle = MenuUiFactory.CreateToggle("Toggle", row, label, true);
            LayoutElement element = toggle.gameObject.AddComponent<LayoutElement>();
            element.preferredWidth = 620f;
            element.preferredHeight = 42f;
            return toggle;
        }

        private RectTransform CreateSettingRow(Transform parent, string label)
        {
            RectTransform row = MenuUiFactory.CreateRect("Setting_" + label, parent);
            LayoutElement rowElement = row.gameObject.AddComponent<LayoutElement>();
            rowElement.preferredHeight = 48f;

            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = false;

            if (!string.IsNullOrWhiteSpace(label))
            {
                Text text = MenuUiFactory.CreateText(
                    "Label",
                    row,
                    label,
                    18,
                    MainMenuTheme.Text,
                    TextAnchor.MiddleLeft);

                LayoutElement textElement = text.gameObject.AddComponent<LayoutElement>();
                textElement.preferredWidth = 260f;
            }

            return row;
        }

        private GameObject BuildConfirmPanel(Transform parent)
        {
            RectTransform root = MenuUiFactory.CreateRect("ConfirmPanel", parent);
            MenuUiFactory.Stretch(root);

            Image dim = root.gameObject.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.72f);

            Image window = MenuUiFactory.CreateImage(
                "Window",
                root,
                new Color(MainMenuTheme.Panel.r, MainMenuTheme.Panel.g, MainMenuTheme.Panel.b, 0.98f));

            RectTransform windowRect = window.rectTransform;
            windowRect.anchorMin = windowRect.anchorMax = new Vector2(0.5f, 0.5f);
            windowRect.pivot = new Vector2(0.5f, 0.5f);
            windowRect.sizeDelta = new Vector2(660f, 340f);

            confirmMessage = MenuUiFactory.CreateText(
                "Message",
                window.transform,
                string.Empty,
                21,
                MainMenuTheme.Text,
                TextAnchor.MiddleCenter);

            confirmMessage.rectTransform.anchorMin = new Vector2(0f, 0.34f);
            confirmMessage.rectTransform.anchorMax = Vector2.one;
            confirmMessage.rectTransform.offsetMin = new Vector2(36f, 0f);
            confirmMessage.rectTransform.offsetMax = new Vector2(-36f, -30f);

            Button cancel = MenuUiFactory.CreateMenuButton("Cancel", window.transform, "Отмена", 18);
            RectTransform cancelRect = cancel.GetComponent<RectTransform>();
            cancelRect.anchorMin = cancelRect.anchorMax = new Vector2(0.5f, 0f);
            cancelRect.pivot = new Vector2(0.5f, 0f);
            cancelRect.anchoredPosition = new Vector2(-120f, 34f);
            cancelRect.sizeDelta = new Vector2(210f, 52f);
            cancel.onClick.AddListener(() =>
            {
                GameAudioService.Instance.PlayClick();
                HideConfirm();
            });

            Button accept = MenuUiFactory.CreateMenuButton("Accept", window.transform, "Подтвердить", 18);
            RectTransform acceptRect = accept.GetComponent<RectTransform>();
            acceptRect.anchorMin = acceptRect.anchorMax = new Vector2(0.5f, 0f);
            acceptRect.pivot = new Vector2(0.5f, 0f);
            acceptRect.anchoredPosition = new Vector2(120f, 34f);
            acceptRect.sizeDelta = new Vector2(210f, 52f);

            confirmActionLabel = accept.transform.Find("Label").GetComponent<Text>();

            accept.onClick.AddListener(() =>
            {
                GameAudioService.Instance.PlayClick();
                Action action = confirmAction;
                HideConfirm();
                action?.Invoke();
            });

            return root.gameObject;
        }

        private GameObject CreateWindow(
            Transform parent,
            string name,
            string title,
            out RectTransform content,
            out Button back)
        {
            RectTransform root = MenuUiFactory.CreateRect(name, parent);
            MenuUiFactory.Stretch(root);

            Image dim = root.gameObject.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.38f);

            Image window = MenuUiFactory.CreateImage(
                "Window",
                root,
                new Color(MainMenuTheme.Panel.r, MainMenuTheme.Panel.g, MainMenuTheme.Panel.b, 0.96f));

            RectTransform windowRect = window.rectTransform;
            windowRect.anchorMin = windowRect.anchorMax = new Vector2(0.5f, 0.5f);
            windowRect.pivot = new Vector2(0.5f, 0.5f);
            windowRect.sizeDelta = new Vector2(980f, 760f);

            Text heading = MenuUiFactory.CreateText(
                "Heading",
                window.transform,
                title,
                30,
                MainMenuTheme.Text,
                TextAnchor.MiddleLeft);

            heading.rectTransform.anchorMin = new Vector2(0f, 1f);
            heading.rectTransform.anchorMax = new Vector2(1f, 1f);
            heading.rectTransform.pivot = new Vector2(0f, 1f);
            heading.rectTransform.offsetMin = new Vector2(36f, -92f);
            heading.rectTransform.offsetMax = new Vector2(-180f, -26f);

            back = MenuUiFactory.CreateMenuButton("Back", window.transform, "Назад", 17);
            RectTransform backRect = back.GetComponent<RectTransform>();
            backRect.anchorMin = backRect.anchorMax = new Vector2(1f, 1f);
            backRect.pivot = new Vector2(1f, 1f);
            backRect.anchoredPosition = new Vector2(-28f, -26f);
            backRect.sizeDelta = new Vector2(136f, 46f);

            content = MenuUiFactory.CreateRect("Content", window.transform);
            content.anchorMin = Vector2.zero;
            content.anchorMax = Vector2.one;
            content.offsetMin = new Vector2(36f, 34f);
            content.offsetMax = new Vector2(-36f, -120f);

            return root.gameObject;
        }

        private void OpenPause()
        {
            previousTimeScale = Mathf.Max(0.0001f, Time.timeScale);
            previousCursorLock = Cursor.lockState;
            previousCursorVisible = Cursor.visible;

            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            paused = true;
            root.SetActive(true);
            mainPanel.SetActive(true);
            loadPanel.SetActive(false);
            journalPanel.SetActive(false);
            settingsPanel.SetActive(false);
            confirmPanel.SetActive(false);

            RefreshActiveSaveLabel();

            SelectFirstButton(mainPanel);
        }

        private void Resume()
        {
            if (!paused)
                return;

            GameAudioService.Instance.PlayClick();
            RestoreGameplayState();
            paused = false;
            root.SetActive(false);
        }

        private void RestoreGameplayState()
        {
            Time.timeScale = previousTimeScale > 0f ? previousTimeScale : 1f;
            Cursor.lockState = previousCursorLock;
            Cursor.visible = previousCursorVisible;
        }

        private void SaveGame()
        {
            if (Flow.SaveCurrentGame(true))
            {
                RefreshActiveSaveLabel();
                ShowStatus("Игра сохранена.");
            }
        }

        private void QuickSave()
        {
            if (Flow.SaveCurrentGame(true))
                ShowStatus("Быстрое сохранение.");
        }

        private void OpenLoadPanel()
        {
            RefreshLoadSlots();
            mainPanel.SetActive(false);
            loadPanel.SetActive(true);
        }

        private void OpenCharacterInventory()
        {
            mainPanel.SetActive(false);
            loadPanel.SetActive(false);
            journalPanel.SetActive(false);
            settingsPanel.SetActive(false);

            characterInventoryPanel?.Show();
        }

        private void OpenJournal()
        {
            RefreshJournal();
            mainPanel.SetActive(false);
            journalPanel.SetActive(true);
        }

        private void OpenSettings()
        {
            PopulateSettings();
            mainPanel.SetActive(false);
            settingsPanel.SetActive(true);
        }

        private void PopulateSettings()
        {
            GameSettingsData settings = SettingsService.Load();

            masterSlider.value = settings.masterVolume;
            musicSlider.value = settings.musicVolume;
            sfxSlider.value = settings.sfxVolume;
            vSyncToggle.isOn = settings.vSync;
            cameraShakeToggle.isOn = settings.cameraShake;

            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(QualitySettings.names.ToList());
            qualityDropdown.value = Mathf.Clamp(
                settings.qualityLevel,
                0,
                Mathf.Max(0, QualitySettings.names.Length - 1));

            qualityDropdown.RefreshShownValue();
        }

        private void ApplySettings()
        {
            GameSettingsData settings = SettingsService.Load().Clone();
            settings.masterVolume = masterSlider.value;
            settings.musicVolume = musicSlider.value;
            settings.sfxVolume = sfxSlider.value;
            settings.vSync = vSyncToggle.isOn;
            settings.cameraShake = cameraShakeToggle.isOn;
            settings.qualityLevel = Mathf.Clamp(
                qualityDropdown.value,
                0,
                Mathf.Max(0, QualitySettings.names.Length - 1));

            SettingsService.SaveAndApply(settings);
        }

        private void RequestMainMenu()
        {
            bool combatActive =
                TurnCombatDirector.Instance != null &&
                TurnCombatDirector.Instance.State == CombatState.Active;

            string message = combatActive
                ? "Вернуться в главное меню?\nАктивный бой не сохраняется — будет использовано последнее сохранение до боя."
                : "Вернуться в главное меню?\nТекущий прогресс будет автоматически сохранён.";

            ShowConfirm(
                message,
                "В меню",
                () =>
                {
                    RestoreGameplayState();
                    paused = false;
                    root.SetActive(false);
                    Flow.ReturnToMainMenu(!combatActive);
                });
        }

        private void RequestQuit()
        {
            bool combatActive =
                TurnCombatDirector.Instance != null &&
                TurnCombatDirector.Instance.State == CombatState.Active;

            string message = combatActive
                ? "Выйти из игры?\nАктивный бой не сохраняется — последнее сохранение до боя останется без изменений."
                : "Выйти из игры?\nТекущий прогресс будет автоматически сохранён.";

            ShowConfirm(
                message,
                "Выйти",
                () =>
                {
                    RestoreGameplayState();
                    paused = false;
                    Flow.QuitGame();
                });
        }

        private void ShowConfirm(string message, string actionLabel, Action action)
        {
            confirmMessage.text = message;
            confirmActionLabel.text = actionLabel;
            confirmAction = action;
            confirmPanel.SetActive(true);
        }

        private void HideConfirm()
        {
            confirmPanel.SetActive(false);
            confirmAction = null;
        }

        private void RefreshActiveSaveLabel()
        {
            SaveGameData data = Flow.ActiveSlotId > 0
                ? SaveGameService.LoadSlot(Flow.ActiveSlotId)
                : null;

            activeSaveText.text = data != null
                ? $"Слот {data.slotId}  •  {data.locationName}\nВремя игры: {SaveGameService.FormatPlayTime(Flow.CurrentPlayTimeSeconds)}"
                : "Слот сохранения не выбран";
        }

        private void OnGameSaved(SaveGameData data)
        {
            if (paused)
                RefreshActiveSaveLabel();

            ShowStatus(
                data != null &&
                data.manualSave
                    ? "Игра сохранена."
                    : "Автосохранение...");
        }

        private void OnFlowError(string message)
        {
            ShowStatus(message);
        }

        private void ShowStatus(string message)
        {
            if (statusText == null)
                return;

            statusText.text = message;
            statusTimer = 4f;
        }

        private static void SelectFirstButton(GameObject panel)
        {
            if (EventSystem.current == null || panel == null)
                return;

            Button button = panel.GetComponentInChildren<Button>();
            if (button != null && button.interactable)
                EventSystem.current.SetSelectedGameObject(button.gameObject);
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;

            var eventSystem = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(InputSystemUIInputModule));

            eventSystem.transform.SetParent(transform, false);
        }
    }
}
