using System;
using System.Collections.Generic;
using System.Linq;
using KeeperFirstCovenant.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [Header("Optional authored presentation")]
        [SerializeField] private AudioClip menuMusic;

        private Canvas canvas;
        private Button continueButton;
        private Button firstMainButton;

        private GameObject savePanel;
        private RectTransform saveList;
        private GameObject settingsPanel;
        private GameObject confirmPanel;

        private Text statusText;
        private float statusTimer;

        private Text confirmText;
        private Text confirmActionLabel;
        private Action confirmAction;

        private Dropdown resolutionDropdown;
        private Dropdown fullscreenDropdown;
        private Dropdown qualityDropdown;
        private Dropdown fpsDropdown;
        private Slider masterSlider;
        private Slider musicSlider;
        private Slider sfxSlider;
        private Toggle vSyncToggle;
        private Toggle cameraShakeToggle;
        private Text masterValue;
        private Text musicValue;
        private Text sfxValue;

        private readonly List<Vector2Int> resolutionOptions = new List<Vector2Int>();
        private readonly List<FullScreenMode> fullscreenModes = new List<FullScreenMode>
        {
            FullScreenMode.FullScreenWindow,
            FullScreenMode.ExclusiveFullScreen,
            FullScreenMode.Windowed
        };

        private readonly List<int> fpsOptions = new List<int>
        {
            -1, 30, 60, 90, 120, 144, 165, 240
        };

        private CanvasGroup transitionGroup;
        private bool introFade = true;
        private bool built;

        private GameFlowController Flow => GameFlowController.Instance;

        private void Start()
        {
            BuildIfNeeded();

            Flow.FlowError += OnFlowError;
            Flow.TransitionProgress += OnTransitionProgress;

            GameAudioService.Instance.PlayMenuAmbience(menuMusic);
            RefreshContinueState();

            transitionGroup.alpha = 1f;
            introFade = true;

            if (firstMainButton != null)
                EventSystem.current?.SetSelectedGameObject(firstMainButton.gameObject);
        }

        private void OnDestroy()
        {
            Flow.FlowError -= OnFlowError;
            Flow.TransitionProgress -= OnTransitionProgress;
        }

        private void Update()
        {
            if (introFade && transitionGroup != null)
            {
                transitionGroup.alpha = Mathf.MoveTowards(
                    transitionGroup.alpha,
                    0f,
                    Time.unscaledDeltaTime / 0.75f);

                if (transitionGroup.alpha <= 0.001f)
                {
                    transitionGroup.alpha = 0f;
                    transitionGroup.blocksRaycasts = false;
                    introFade = false;
                }
            }

            if (statusTimer > 0f)
            {
                statusTimer -= Time.unscaledDeltaTime;
                if (statusTimer <= 0f && statusText != null)
                    statusText.text = string.Empty;
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                CloseTopmostPanel();
        }

        private void BuildIfNeeded()
        {
            if (built)
                return;

            built = true;
            EnsureEventSystem();

            var canvasObject = new GameObject(
                "MainMenuCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            canvasObject.transform.SetParent(transform, false);

            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();

            BuildBackground(canvasRect);
            BuildBrand(canvasRect);
            BuildMainActions(canvasRect);
            BuildFooter(canvasRect);

            savePanel = BuildSavePanel(canvasRect);
            settingsPanel = BuildSettingsPanel(canvasRect);
            confirmPanel = BuildConfirmPanel(canvasRect);

            BuildTransition(canvasRect);
        }

        private void BuildBackground(RectTransform canvasRect)
        {
            RectTransform host = MenuUiFactory.CreateRect("LiveBackground", canvasRect);
            MenuUiFactory.Stretch(host);
            host.gameObject.AddComponent<MenuLiveBackground>().BuildIfNeeded();

            Image leftShade = MenuUiFactory.CreateImage(
                "LeftShade",
                canvasRect,
                new Color(0.01f, 0.015f, 0.02f, 0.56f));

            RectTransform shadeRect = leftShade.rectTransform;
            shadeRect.anchorMin = new Vector2(0f, 0f);
            shadeRect.anchorMax = new Vector2(0.42f, 1f);
            shadeRect.offsetMin = Vector2.zero;
            shadeRect.offsetMax = Vector2.zero;
            leftShade.raycastTarget = false;
        }

        private void BuildBrand(RectTransform canvasRect)
        {
            RectTransform brand = MenuUiFactory.CreateRect("Brand", canvasRect);
            MenuUiFactory.SetAnchoredRect(
                brand,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(110f, -86f),
                new Vector2(620f, 178f));

            Text title = MenuUiFactory.CreateText(
                "Title",
                brand,
                "ХРАНИТЕЛЬ",
                54,
                MainMenuTheme.Text,
                TextAnchor.UpperLeft);

            title.rectTransform.anchorMin = new Vector2(0f, 0.46f);
            title.rectTransform.anchorMax = Vector2.one;
            title.rectTransform.offsetMin = Vector2.zero;
            title.rectTransform.offsetMax = Vector2.zero;

            Text subtitle = MenuUiFactory.CreateText(
                "Subtitle",
                brand,
                "П Е Р В Ы Й   З А В Е Т",
                18,
                MainMenuTheme.Silver,
                TextAnchor.UpperLeft);

            subtitle.rectTransform.anchorMin = new Vector2(0f, 0.23f);
            subtitle.rectTransform.anchorMax = new Vector2(1f, 0.50f);
            subtitle.rectTransform.offsetMin = Vector2.zero;
            subtitle.rectTransform.offsetMax = Vector2.zero;

            Image line = MenuUiFactory.CreateImage("BrandLine", brand, MainMenuTheme.SilverDim);
            RectTransform lineRect = line.rectTransform;
            lineRect.anchorMin = lineRect.anchorMax = new Vector2(0f, 0.21f);
            lineRect.pivot = new Vector2(0f, 0.5f);
            lineRect.sizeDelta = new Vector2(175f, 1f);
            lineRect.anchoredPosition = Vector2.zero;

            Text motif = MenuUiFactory.CreateText(
                "Motif",
                brand,
                "ЖИВАЯ КЛЯТВА  •  СЕРЕБРЯНЫЙ КРУГ  •  СЛОМАННОЕ КОЛЬЦО",
                12,
                new Color(MainMenuTheme.MutedText.r, MainMenuTheme.MutedText.g, MainMenuTheme.MutedText.b, 0.72f),
                TextAnchor.LowerLeft);

            motif.rectTransform.anchorMin = Vector2.zero;
            motif.rectTransform.anchorMax = new Vector2(1f, 0.20f);
            motif.rectTransform.offsetMin = Vector2.zero;
            motif.rectTransform.offsetMax = Vector2.zero;
        }

        private void BuildMainActions(RectTransform canvasRect)
        {
            RectTransform column = MenuUiFactory.CreateRect("MainActions", canvasRect);
            MenuUiFactory.SetAnchoredRect(
                column,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(110f, -70f),
                new Vector2(410f, 420f));

            VerticalLayoutGroup layout = column.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            continueButton = AddMainButton(column, "Continue", "Продолжить", ContinueGame);
            firstMainButton = continueButton;

            Button newGame = AddMainButton(column, "NewGame", "Начать новую игру", NewGame);
            AddMainButton(column, "LoadGame", "Выбрать сохранение", OpenSavePanel);
            AddMainButton(column, "Settings", "Настройки", OpenSettingsPanel);
            AddMainButton(column, "Exit", "Выход", RequestExit);

            if (!SaveGameService.HasAnySave())
                firstMainButton = newGame;
        }

        private Button AddMainButton(Transform parent, string name, string label, Action action)
        {
            Button button = MenuUiFactory.CreateMenuButton(name, parent, label, 25);
            LayoutElement layout = button.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 62f;
            layout.minHeight = 62f;

            button.onClick.AddListener(() =>
            {
                GameAudioService.Instance.PlayClick();
                action?.Invoke();
            });

            return button;
        }

        private void BuildFooter(RectTransform canvasRect)
        {
            statusText = MenuUiFactory.CreateText(
                "Status",
                canvasRect,
                string.Empty,
                16,
                MainMenuTheme.Silver,
                TextAnchor.MiddleLeft);

            MenuUiFactory.SetAnchoredRect(
                statusText.rectTransform,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(110f, 38f),
                new Vector2(700f, 42f));

            Text version = MenuUiFactory.CreateText(
                "Version",
                canvasRect,
                "PRE-ALPHA  •  MAIN MENU SHELL",
                12,
                new Color(MainMenuTheme.MutedText.r, MainMenuTheme.MutedText.g, MainMenuTheme.MutedText.b, 0.60f),
                TextAnchor.MiddleRight);

            MenuUiFactory.SetAnchoredRect(
                version.rectTransform,
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-42f, 30f),
                new Vector2(420f, 34f));
        }

        private GameObject BuildSavePanel(RectTransform canvasRect)
        {
            GameObject root = CreateOverlayWindow(
                canvasRect,
                "SavePanel",
                "СОХРАНЕНИЯ",
                "Выберите сохранение для продолжения игры.",
                out RectTransform content,
                out Button close);

            close.onClick.AddListener(() =>
            {
                GameAudioService.Instance.PlayClick();
                CloseSavePanel();
            });

            saveList = MenuUiFactory.CreateRect("SaveList", content);
            MenuUiFactory.Stretch(saveList);

            VerticalLayoutGroup layout = saveList.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            root.SetActive(false);
            return root;
        }

        private void RefreshSavePanel()
        {
            if (saveList == null)
                return;

            for (int i = saveList.childCount - 1; i >= 0; i--)
                Destroy(saveList.GetChild(i).gameObject);

            for (int slot = 1; slot <= SaveGameService.MaxSlots; slot++)
            {
                int capturedSlot = slot;
                SaveGameData data = SaveGameService.LoadSlot(slot);

                Image row = MenuUiFactory.CreateImage(
                    "Slot_" + slot,
                    saveList,
                    data != null
                        ? new Color(0.045f, 0.052f, 0.058f, 0.92f)
                        : new Color(0.03f, 0.035f, 0.04f, 0.68f));

                LayoutElement rowLayout = row.gameObject.AddComponent<LayoutElement>();
                rowLayout.preferredHeight = 88f;
                rowLayout.minHeight = 88f;

                Text title = MenuUiFactory.CreateText(
                    "Title",
                    row.transform,
                    data != null
                        ? $"Слот {slot}  —  {data.locationName}"
                        : $"Слот {slot}  —  пусто",
                    20,
                    data != null ? MainMenuTheme.Text : MainMenuTheme.DisabledText,
                    TextAnchor.UpperLeft);

                RectTransform titleRect = title.rectTransform;
                titleRect.anchorMin = new Vector2(0f, 0.48f);
                titleRect.anchorMax = new Vector2(0.58f, 1f);
                titleRect.offsetMin = new Vector2(18f, 0f);
                titleRect.offsetMax = new Vector2(-8f, -8f);

                Text info = MenuUiFactory.CreateText(
                    "Info",
                    row.transform,
                    data != null
                        ? $"{SaveGameService.FormatTimestamp(data)}   •   {SaveGameService.FormatPlayTime(data.playTimeSeconds)}"
                        : "Свободный слот",
                    14,
                    MainMenuTheme.MutedText,
                    TextAnchor.LowerLeft);

                RectTransform infoRect = info.rectTransform;
                infoRect.anchorMin = new Vector2(0f, 0f);
                infoRect.anchorMax = new Vector2(0.58f, 0.52f);
                infoRect.offsetMin = new Vector2(18f, 8f);
                infoRect.offsetMax = new Vector2(-8f, 0f);

                if (data != null)
                {
                    SaveGameData capturedData = data;

                    Button load = CreateSmallButton(row.transform, "Load", "Загрузить", 130f);
                    RectTransform loadRect = load.GetComponent<RectTransform>();
                    loadRect.anchorMin = loadRect.anchorMax = new Vector2(1f, 0.5f);
                    loadRect.pivot = new Vector2(1f, 0.5f);
                    loadRect.anchoredPosition = new Vector2(-132f, 0f);
                    load.onClick.AddListener(() =>
                    {
                        GameAudioService.Instance.PlayClick();
                        Flow.LoadSave(capturedData);
                    });

                    Button delete = CreateSmallButton(row.transform, "Delete", "Удалить", 105f);
                    RectTransform deleteRect = delete.GetComponent<RectTransform>();
                    deleteRect.anchorMin = deleteRect.anchorMax = new Vector2(1f, 0.5f);
                    deleteRect.pivot = new Vector2(1f, 0.5f);
                    deleteRect.anchoredPosition = new Vector2(-16f, 0f);
                    delete.onClick.AddListener(() =>
                    {
                        GameAudioService.Instance.PlayClick();
                        ShowConfirm(
                            $"Удалить сохранение из слота {capturedSlot}?\nЭто действие нельзя отменить.",
                            "Удалить",
                            () =>
                            {
                                SaveGameService.DeleteSlot(capturedSlot);
                                RefreshSavePanel();
                                RefreshContinueState();
                                ShowStatus($"Слот {capturedSlot} очищен.");
                            });
                    });
                }
            }
        }

        private GameObject BuildSettingsPanel(RectTransform canvasRect)
        {
            GameObject root = CreateOverlayWindow(
                canvasRect,
                "SettingsPanel",
                "НАСТРОЙКИ",
                "Графика, звук и основные параметры игры.",
                out RectTransform content,
                out Button close);

            close.onClick.AddListener(() =>
            {
                GameAudioService.Instance.PlayClick();
                CloseSettingsPanel();
            });

            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            resolutionDropdown = AddDropdownRow(content, "Разрешение", out _);
            fullscreenDropdown = AddDropdownRow(content, "Режим экрана", out _);
            qualityDropdown = AddDropdownRow(content, "Качество", out _);
            fpsDropdown = AddDropdownRow(content, "Ограничение FPS", out _);

            masterSlider = AddSliderRow(content, "Общая громкость", out masterValue);
            musicSlider = AddSliderRow(content, "Музыка", out musicValue);
            sfxSlider = AddSliderRow(content, "Эффекты / интерфейс", out sfxValue);

            vSyncToggle = AddToggleRow(content, "Вертикальная синхронизация");
            cameraShakeToggle = AddToggleRow(content, "Тряска камеры в бою");

            RectTransform buttons = MenuUiFactory.CreateRect("Buttons", content);
            LayoutElement buttonsLayout = buttons.gameObject.AddComponent<LayoutElement>();
            buttonsLayout.preferredHeight = 58f;

            HorizontalLayoutGroup buttonLayout = buttons.gameObject.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.spacing = 10f;
            buttonLayout.childControlHeight = true;
            buttonLayout.childControlWidth = true;
            buttonLayout.childForceExpandHeight = true;
            buttonLayout.childForceExpandWidth = true;

            Button defaults = MenuUiFactory.CreateMenuButton("Defaults", buttons, "По умолчанию", 18);
            defaults.onClick.AddListener(() =>
            {
                GameAudioService.Instance.PlayClick();
                SettingsService.ResetToDefaults();
                PopulateSettingsControls();
                ShowStatus("Настройки сброшены.");
            });

            Button apply = MenuUiFactory.CreateMenuButton("Apply", buttons, "Применить", 18);
            apply.onClick.AddListener(() =>
            {
                GameAudioService.Instance.PlayClick();
                ApplySettingsFromControls();
            });

            root.SetActive(false);
            return root;
        }

        private Dropdown AddDropdownRow(Transform parent, string label, out Text labelText)
        {
            RectTransform row = CreateSettingRow(parent, label, out labelText);
            Dropdown dropdown = MenuUiFactory.CreateDropdown("Value", row, 18);

            LayoutElement layout = dropdown.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = 360f;
            layout.flexibleWidth = 1f;
            layout.preferredHeight = 42f;

            return dropdown;
        }

        private Slider AddSliderRow(Transform parent, string label, out Text valueLabel)
        {
            RectTransform row = CreateSettingRow(parent, label, out _);

            Slider slider = MenuUiFactory.CreateSlider("Slider", row, 0f, 1f, 1f);
            LayoutElement sliderLayout = slider.gameObject.AddComponent<LayoutElement>();
            sliderLayout.preferredWidth = 300f;
            sliderLayout.flexibleWidth = 1f;
            sliderLayout.preferredHeight = 42f;

            valueLabel = MenuUiFactory.CreateText(
                "Value",
                row,
                "100%",
                16,
                MainMenuTheme.Silver,
                TextAnchor.MiddleRight);

            LayoutElement valueLayout = valueLabel.gameObject.AddComponent<LayoutElement>();
            valueLayout.preferredWidth = 66f;

            Text capturedLabel = valueLabel;
            slider.onValueChanged.AddListener(value =>
            {
                capturedLabel.text = Mathf.RoundToInt(value * 100f) + "%";
            });

            return slider;
        }

        private Toggle AddToggleRow(Transform parent, string label)
        {
            RectTransform row = CreateSettingRow(parent, string.Empty, out Text rowLabel);
            rowLabel.gameObject.SetActive(false);

            Toggle toggle = MenuUiFactory.CreateToggle("Toggle", row, label, true);
            LayoutElement toggleLayout = toggle.gameObject.AddComponent<LayoutElement>();
            toggleLayout.flexibleWidth = 1f;
            toggleLayout.preferredHeight = 42f;

            return toggle;
        }

        private RectTransform CreateSettingRow(Transform parent, string label, out Text labelText)
        {
            RectTransform row = MenuUiFactory.CreateRect("Row_" + label, parent);
            LayoutElement rowElement = row.gameObject.AddComponent<LayoutElement>();
            rowElement.preferredHeight = 48f;
            rowElement.minHeight = 48f;

            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = false;

            labelText = MenuUiFactory.CreateText(
                "Label",
                row,
                label,
                18,
                MainMenuTheme.Text,
                TextAnchor.MiddleLeft);

            LayoutElement labelLayout = labelText.gameObject.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 250f;
            labelLayout.minWidth = 250f;

            return row;
        }

        private GameObject BuildConfirmPanel(RectTransform canvasRect)
        {
            GameObject root = CreateOverlayWindow(
                canvasRect,
                "ConfirmPanel",
                "ПОДТВЕРЖДЕНИЕ",
                string.Empty,
                out RectTransform content,
                out Button close,
                new Vector2(670f, 360f));

            close.onClick.AddListener(() =>
            {
                GameAudioService.Instance.PlayClick();
                HideConfirm();
            });

            confirmText = MenuUiFactory.CreateText(
                "Message",
                content,
                string.Empty,
                22,
                MainMenuTheme.Text,
                TextAnchor.MiddleCenter);

            confirmText.rectTransform.anchorMin = new Vector2(0f, 0.30f);
            confirmText.rectTransform.anchorMax = Vector2.one;
            confirmText.rectTransform.offsetMin = new Vector2(20f, 0f);
            confirmText.rectTransform.offsetMax = new Vector2(-20f, -8f);

            Button accept = MenuUiFactory.CreateMenuButton("Accept", content, "Подтвердить", 19);
            RectTransform acceptRect = accept.GetComponent<RectTransform>();
            acceptRect.anchorMin = acceptRect.anchorMax = new Vector2(0.5f, 0f);
            acceptRect.pivot = new Vector2(0.5f, 0f);
            acceptRect.anchoredPosition = new Vector2(115f, 10f);
            acceptRect.sizeDelta = new Vector2(210f, 52f);

            confirmActionLabel = accept.transform.Find("Label").GetComponent<Text>();

            accept.onClick.AddListener(() =>
            {
                GameAudioService.Instance.PlayClick();
                Action action = confirmAction;
                HideConfirm();
                action?.Invoke();
            });

            Button cancel = MenuUiFactory.CreateMenuButton("Cancel", content, "Отмена", 19);
            RectTransform cancelRect = cancel.GetComponent<RectTransform>();
            cancelRect.anchorMin = cancelRect.anchorMax = new Vector2(0.5f, 0f);
            cancelRect.pivot = new Vector2(0.5f, 0f);
            cancelRect.anchoredPosition = new Vector2(-115f, 10f);
            cancelRect.sizeDelta = new Vector2(210f, 52f);

            cancel.onClick.AddListener(() =>
            {
                GameAudioService.Instance.PlayClick();
                HideConfirm();
            });

            root.SetActive(false);
            return root;
        }

        private GameObject CreateOverlayWindow(
            RectTransform canvasRect,
            string name,
            string title,
            string subtitle,
            out RectTransform content,
            out Button close,
            Vector2? windowSize = null)
        {
            RectTransform root = MenuUiFactory.CreateRect(name, canvasRect);
            MenuUiFactory.Stretch(root);

            Image dim = root.gameObject.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.64f);

            Image window = MenuUiFactory.CreateImage("Window", root, MainMenuTheme.Panel);
            RectTransform windowRect = window.rectTransform;
            windowRect.anchorMin = windowRect.anchorMax = new Vector2(0.5f, 0.5f);
            windowRect.pivot = new Vector2(0.5f, 0.5f);
            windowRect.sizeDelta = windowSize ?? new Vector2(980f, 790f);

            Outline outline = window.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(MainMenuTheme.SilverDim.r, MainMenuTheme.SilverDim.g, MainMenuTheme.SilverDim.b, 0.65f);
            outline.effectDistance = new Vector2(1f, -1f);

            Text titleText = MenuUiFactory.CreateText(
                "Title",
                window.transform,
                title,
                30,
                MainMenuTheme.Text,
                TextAnchor.MiddleLeft);

            RectTransform titleRect = titleText.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0f, 1f);
            titleRect.offsetMin = new Vector2(36f, -96f);
            titleRect.offsetMax = new Vector2(-100f, -28f);

            Text subtitleText = MenuUiFactory.CreateText(
                "Subtitle",
                window.transform,
                subtitle,
                15,
                MainMenuTheme.MutedText,
                TextAnchor.MiddleLeft);

            RectTransform subtitleRect = subtitleText.rectTransform;
            subtitleRect.anchorMin = new Vector2(0f, 1f);
            subtitleRect.anchorMax = new Vector2(1f, 1f);
            subtitleRect.pivot = new Vector2(0f, 1f);
            subtitleRect.offsetMin = new Vector2(36f, -126f);
            subtitleRect.offsetMax = new Vector2(-100f, -92f);

            Image line = MenuUiFactory.CreateImage("HeaderLine", window.transform, MainMenuTheme.SilverDim);
            RectTransform lineRect = line.rectTransform;
            lineRect.anchorMin = new Vector2(0f, 1f);
            lineRect.anchorMax = new Vector2(1f, 1f);
            lineRect.pivot = new Vector2(0.5f, 1f);
            lineRect.offsetMin = new Vector2(36f, -136f);
            lineRect.offsetMax = new Vector2(-36f, -135f);

            close = MenuUiFactory.CreateMenuButton("Close", window.transform, "Назад", 17);
            RectTransform closeRect = close.GetComponent<RectTransform>();
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-28f, -28f);
            closeRect.sizeDelta = new Vector2(128f, 46f);

            content = MenuUiFactory.CreateRect("Content", window.transform);
            content.anchorMin = new Vector2(0f, 0f);
            content.anchorMax = new Vector2(1f, 1f);
            content.offsetMin = new Vector2(36f, 32f);
            content.offsetMax = new Vector2(-36f, -154f);

            return root.gameObject;
        }

        private Button CreateSmallButton(Transform parent, string name, string text, float width)
        {
            Button button = MenuUiFactory.CreateMenuButton(name, parent, text, 16);
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, 48f);
            return button;
        }

        private void BuildTransition(RectTransform canvasRect)
        {
            Image transition = MenuUiFactory.CreateImage("SceneTransition", canvasRect, Color.black);
            MenuUiFactory.Stretch(transition.rectTransform);
            transition.transform.SetAsLastSibling();

            transitionGroup = transition.gameObject.AddComponent<CanvasGroup>();
            transitionGroup.alpha = 1f;
            transitionGroup.blocksRaycasts = true;
            transition.raycastTarget = true;
        }

        private void EnsureEventSystem()
        {
            EventSystem existing = EventSystem.current;
            if (existing != null)
                return;

            var eventSystem = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(InputSystemUIInputModule));

            eventSystem.transform.SetParent(transform, false);
        }

        private void ContinueGame()
        {
            Flow.TryContinue();
        }

        private void NewGame()
        {
            if (SaveGameService.GetFirstFreeSlot() < 1)
            {
                ShowStatus("Все шесть слотов заняты. Удалите ненужное сохранение.");
                OpenSavePanel();
                return;
            }

            if (SaveGameService.HasAnySave())
            {
                ShowConfirm(
                    "Начать новую игру?\nСуществующие сохранения останутся на месте.",
                    "Начать",
                    StartNewGameNow);
                return;
            }

            StartNewGameNow();
        }

        private void StartNewGameNow()
        {
            if (!Flow.TryStartNewGame())
                RefreshContinueState();
        }

        private void OpenSavePanel()
        {
            ClosePanelsExcept(savePanel);
            RefreshSavePanel();
            savePanel.SetActive(true);
        }

        private void CloseSavePanel()
        {
            savePanel.SetActive(false);
            RefreshContinueState();
            SelectFirstMainButton();
        }

        private void OpenSettingsPanel()
        {
            ClosePanelsExcept(settingsPanel);
            PopulateSettingsControls();
            settingsPanel.SetActive(true);
        }

        private void CloseSettingsPanel()
        {
            settingsPanel.SetActive(false);
            SelectFirstMainButton();
        }

        private void PopulateSettingsControls()
        {
            GameSettingsData settings = SettingsService.Load();

            BuildResolutionOptions(settings);
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(resolutionOptions
                .Select(value => $"{value.x} × {value.y}")
                .ToList());

            int resolutionIndex = resolutionOptions.FindIndex(
                value => value.x == settings.width && value.y == settings.height);

            resolutionDropdown.value = Mathf.Max(0, resolutionIndex);
            resolutionDropdown.RefreshShownValue();

            fullscreenDropdown.ClearOptions();
            fullscreenDropdown.AddOptions(new List<string>
            {
                "Окно без рамки",
                "Полный экран",
                "Оконный режим"
            });

            int fullscreenIndex = fullscreenModes.IndexOf(settings.fullscreenMode);
            fullscreenDropdown.value = Mathf.Max(0, fullscreenIndex);
            fullscreenDropdown.RefreshShownValue();

            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(QualitySettings.names.ToList());
            qualityDropdown.value = Mathf.Clamp(settings.qualityLevel, 0, Mathf.Max(0, QualitySettings.names.Length - 1));
            qualityDropdown.RefreshShownValue();

            fpsDropdown.ClearOptions();
            fpsDropdown.AddOptions(fpsOptions
                .Select(value => value < 0 ? "Без ограничения" : value.ToString())
                .ToList());

            int fpsIndex = fpsOptions.IndexOf(settings.targetFrameRate);
            fpsDropdown.value = fpsIndex >= 0 ? fpsIndex : fpsOptions.IndexOf(60);
            fpsDropdown.RefreshShownValue();

            masterSlider.value = settings.masterVolume;
            musicSlider.value = settings.musicVolume;
            sfxSlider.value = settings.sfxVolume;
            vSyncToggle.isOn = settings.vSync;
            cameraShakeToggle.isOn = settings.cameraShake;

            masterValue.text = Mathf.RoundToInt(settings.masterVolume * 100f) + "%";
            musicValue.text = Mathf.RoundToInt(settings.musicVolume * 100f) + "%";
            sfxValue.text = Mathf.RoundToInt(settings.sfxVolume * 100f) + "%";
        }

        private void BuildResolutionOptions(GameSettingsData settings)
        {
            resolutionOptions.Clear();

            foreach (Resolution resolution in Screen.resolutions)
            {
                var candidate = new Vector2Int(resolution.width, resolution.height);
                if (!resolutionOptions.Contains(candidate))
                    resolutionOptions.Add(candidate);
            }

            var current = new Vector2Int(settings.width, settings.height);
            if (!resolutionOptions.Contains(current))
                resolutionOptions.Add(current);

            resolutionOptions.Sort((a, b) =>
            {
                int pixels = (b.x * b.y).CompareTo(a.x * a.y);
                return pixels != 0 ? pixels : b.x.CompareTo(a.x);
            });
        }

        private void ApplySettingsFromControls()
        {
            GameSettingsData settings = SettingsService.Load().Clone();

            if (resolutionOptions.Count > 0)
            {
                int index = Mathf.Clamp(resolutionDropdown.value, 0, resolutionOptions.Count - 1);
                settings.width = resolutionOptions[index].x;
                settings.height = resolutionOptions[index].y;
            }

            settings.fullscreenMode = fullscreenModes[
                Mathf.Clamp(fullscreenDropdown.value, 0, fullscreenModes.Count - 1)];

            settings.qualityLevel = Mathf.Clamp(
                qualityDropdown.value,
                0,
                Mathf.Max(0, QualitySettings.names.Length - 1));

            settings.targetFrameRate = fpsOptions[
                Mathf.Clamp(fpsDropdown.value, 0, fpsOptions.Count - 1)];

            settings.masterVolume = masterSlider.value;
            settings.musicVolume = musicSlider.value;
            settings.sfxVolume = sfxSlider.value;
            settings.vSync = vSyncToggle.isOn;
            settings.cameraShake = cameraShakeToggle.isOn;

            SettingsService.SaveAndApply(settings);
            ShowStatus("Настройки применены.");
        }

        private void RequestExit()
        {
            ShowConfirm(
                "Выйти из игры?",
                "Выйти",
                Flow.QuitGame);
        }

        private void ShowConfirm(string message, string actionLabel, Action action)
        {
            confirmText.text = message;
            confirmActionLabel.text = actionLabel;
            confirmAction = action;
            confirmPanel.SetActive(true);
        }

        private void HideConfirm()
        {
            confirmPanel.SetActive(false);
            confirmAction = null;
        }

        private void CloseTopmostPanel()
        {
            if (confirmPanel != null && confirmPanel.activeSelf)
            {
                HideConfirm();
                return;
            }

            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettingsPanel();
                return;
            }

            if (savePanel != null && savePanel.activeSelf)
            {
                CloseSavePanel();
                return;
            }
        }

        private void ClosePanelsExcept(GameObject keep)
        {
            if (savePanel != null && savePanel != keep)
                savePanel.SetActive(false);

            if (settingsPanel != null && settingsPanel != keep)
                settingsPanel.SetActive(false);

            if (confirmPanel != null)
                confirmPanel.SetActive(false);
        }

        private void RefreshContinueState()
        {
            if (continueButton == null)
                return;

            bool hasSave = SaveGameService.HasAnySave();
            continueButton.interactable = hasSave;

            if (!hasSave && firstMainButton == continueButton)
            {
                Transform actions = continueButton.transform.parent;
                if (actions != null && actions.childCount > 1)
                    firstMainButton = actions.GetChild(1).GetComponent<Button>();
            }
            else if (hasSave)
            {
                firstMainButton = continueButton;
            }
        }

        private void SelectFirstMainButton()
        {
            if (firstMainButton != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(firstMainButton.gameObject);
        }

        private void ShowStatus(string message)
        {
            if (statusText == null)
                return;

            statusText.text = message;
            statusTimer = 4.5f;
        }

        private void OnFlowError(string message)
        {
            ShowStatus(message);
        }

        private void OnTransitionProgress(float progress)
        {
            if (transitionGroup == null)
                return;

            introFade = false;
            transitionGroup.blocksRaycasts = true;
            transitionGroup.alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress));
        }
    }
}
