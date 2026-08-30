using KeeperFirstCovenant.Core;
using KeeperFirstCovenant.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public sealed class InspectionPanelController : MonoBehaviour
    {
        private static InspectionPanelController instance;

        private GameObject root;
        private Text category;
        private Text title;
        private Text description;
        private Button closeButton;

        private float previousTimeScale = 1f;
        private CursorLockMode previousCursorLock;
        private bool previousCursorVisible;

        public static bool IsOpen =>
            instance != null &&
            instance.root != null &&
            instance.root.activeSelf;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            Build();
            root.SetActive(false);

            WorldInspectable.InspectionRequested +=
                OnInspectionRequested;
        }

        private void OnDestroy()
        {
            WorldInspectable.InspectionRequested -=
                OnInspectionRequested;

            if (instance == this)
                instance = null;

            if (root != null &&
                root.activeSelf)
            {
                RestoreGameplayState();
            }
        }

        private void Update()
        {
            if (!IsOpen)
                return;

            Keyboard keyboard =
                Keyboard.current;

            if (keyboard != null &&
                keyboard.escapeKey.wasPressedThisFrame)
            {
                Close();
            }
        }

        private void Build()
        {
            EnsureEventSystem();

            var canvasObject =
                new GameObject(
                    "InspectionCanvas",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster));

            canvasObject.transform.SetParent(
                transform,
                false);

            Canvas canvas =
                canvasObject.GetComponent<Canvas>();

            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;

            canvas.sortingOrder = 7000;

            CanvasScaler scaler =
                canvasObject.GetComponent<CanvasScaler>();

            scaler.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;

            scaler.referenceResolution =
                new Vector2(1920f, 1080f);

            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect =
                canvasObject.GetComponent<RectTransform>();

            root =
                MenuUiFactory.CreateRect(
                    "InspectionRoot",
                    canvasRect).gameObject;

            MenuUiFactory.Stretch(
                root.GetComponent<RectTransform>());

            Image veil =
                root.AddComponent<Image>();

            veil.color =
                new Color(
                    0.005f,
                    0.008f,
                    0.011f,
                    0.76f);

            Image panel =
                MenuUiFactory.CreateImage(
                    "InspectionPanel",
                    root.transform,
                    new Color(
                        MainMenuTheme.Panel.r,
                        MainMenuTheme.Panel.g,
                        MainMenuTheme.Panel.b,
                        0.98f));

            RectTransform panelRect =
                panel.rectTransform;

            MenuUiFactory.SetAnchoredRect(
                panelRect,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(860f, 500f));

            KeeperUiSkin.DecorateMajorPanel(
                panel,
                true);

            category =
                MenuUiFactory.CreateText(
                    "Category",
                    panel.transform,
                    string.Empty,
                    13,
                    MainMenuTheme.Warm,
                    TextAnchor.MiddleLeft);

            category.rectTransform.anchorMin =
                new Vector2(0f, 0.82f);

            category.rectTransform.anchorMax =
                new Vector2(1f, 1f);

            category.rectTransform.offsetMin =
                new Vector2(34f, 0f);

            category.rectTransform.offsetMax =
                new Vector2(-34f, -18f);

            title =
                MenuUiFactory.CreateText(
                    "Title",
                    panel.transform,
                    string.Empty,
                    31,
                    MainMenuTheme.Text,
                    TextAnchor.UpperLeft);

            title.rectTransform.anchorMin =
                new Vector2(0f, 0.68f);

            title.rectTransform.anchorMax =
                new Vector2(1f, 0.88f);

            title.rectTransform.offsetMin =
                new Vector2(34f, 0f);

            title.rectTransform.offsetMax =
                new Vector2(-34f, 0f);

            Image rule =
                MenuUiFactory.CreateImage(
                    "Rule",
                    panel.transform,
                    new Color(
                        MainMenuTheme.Silver.r,
                        MainMenuTheme.Silver.g,
                        MainMenuTheme.Silver.b,
                        0.24f));

            RectTransform ruleRect =
                rule.rectTransform;

            ruleRect.anchorMin =
                new Vector2(0f, 0.64f);

            ruleRect.anchorMax =
                new Vector2(1f, 0.64f);

            ruleRect.offsetMin =
                new Vector2(34f, -1f);

            ruleRect.offsetMax =
                new Vector2(-34f, 1f);

            description =
                MenuUiFactory.CreateText(
                    "Description",
                    panel.transform,
                    string.Empty,
                    18,
                    MainMenuTheme.Text,
                    TextAnchor.UpperLeft);

            description.rectTransform.anchorMin =
                new Vector2(0f, 0.17f);

            description.rectTransform.anchorMax =
                new Vector2(1f, 0.62f);

            description.rectTransform.offsetMin =
                new Vector2(34f, 0f);

            description.rectTransform.offsetMax =
                new Vector2(-34f, 0f);

            closeButton =
                MenuUiFactory.CreateMenuButton(
                    "Close",
                    panel.transform,
                    "Закрыть",
                    16);

            RectTransform closeRect =
                closeButton.GetComponent<RectTransform>();

            closeRect.anchorMin =
                closeRect.anchorMax =
                    new Vector2(1f, 0f);

            closeRect.pivot =
                new Vector2(1f, 0f);

            closeRect.anchoredPosition =
                new Vector2(-30f, 28f);

            closeRect.sizeDelta =
                new Vector2(150f, 46f);

            closeButton.onClick.AddListener(
                () =>
                {
                    GameAudioService.Instance.PlayClick();
                    Close();
                });
        }

        private void OnInspectionRequested(
            WorldInspectable inspectable,
            GameObject actor)
        {
            if (inspectable == null)
                return;

            Show(inspectable);
        }

        private void Show(
            WorldInspectable inspectable)
        {
            if (root.activeSelf)
                return;

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

            category.text =
                CategoryName(
                    inspectable.Category);

            title.text =
                inspectable.DisplayName;

            description.text =
                string.IsNullOrWhiteSpace(
                    inspectable.Description)
                    ? "Ничего примечательного."
                    : inspectable.Description;

            root.SetActive(true);

            if (EventSystem.current != null)
            {
                EventSystem.current
                    .SetSelectedGameObject(
                        closeButton.gameObject);
            }
        }

        private void Close()
        {
            if (!IsOpen)
                return;

            root.SetActive(false);
            RestoreGameplayState();
        }

        private void RestoreGameplayState()
        {
            Time.timeScale =
                previousTimeScale > 0f
                    ? previousTimeScale
                    : 1f;

            Cursor.lockState =
                previousCursorLock;

            Cursor.visible =
                previousCursorVisible;
        }

        private static string CategoryName(
            InspectionCategory value)
        {
            switch (value)
            {
                case InspectionCategory.Place:
                    return "МЕСТО";
                case InspectionCategory.Clue:
                    return "УЛИКА";
                case InspectionCategory.Lore:
                    return "ИСТОРИЯ";
                case InspectionCategory.Magic:
                    return "МАГИЯ";
                case InspectionCategory.Corpse:
                    return "ОСТАНКИ";
                case InspectionCategory.Mechanism:
                    return "МЕХАНИЗМ";
                default:
                    return "ОСМОТР";
            }
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;

            var eventSystem =
                new GameObject(
                    "EventSystem",
                    typeof(EventSystem),
                    typeof(InputSystemUIInputModule));

            eventSystem.transform.SetParent(
                transform,
                false);
        }
    }
}
