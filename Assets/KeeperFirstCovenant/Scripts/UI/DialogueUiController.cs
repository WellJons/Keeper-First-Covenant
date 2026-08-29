using KeeperFirstCovenant.Core;
using KeeperFirstCovenant.Dialogue;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public sealed class DialogueUiController :
        MonoBehaviour
    {
        private GameObject root;
        private Text speaker;
        private Text body;
        private Image portrait;
        private RectTransform choicesRoot;
        private Button continueButton;
        private Button cancelButton;

        private DialogueRunner Runner =>
            DialogueRunner.Instance;

        private void Start()
        {
            Build();

            Runner.DialogueStarted +=
                OnDialogueStarted;

            Runner.NodeChanged +=
                OnNodeChanged;

            Runner.DialogueEnded +=
                OnDialogueEnded;

            root.SetActive(
                Runner.IsActive);

            if (Runner.IsActive)
                Refresh();
        }

        private void OnDestroy()
        {
            DialogueRunner current =
                DialogueRunner.Current;

            if (current == null)
                return;

            current.DialogueStarted -=
                OnDialogueStarted;

            current.NodeChanged -=
                OnNodeChanged;

            current.DialogueEnded -=
                OnDialogueEnded;
        }

        private void Update()
        {
            if (!Runner.IsActive)
                return;

            Keyboard keyboard =
                Keyboard.current;

            if (keyboard == null)
                return;

            if (keyboard.enterKey
                    .wasPressedThisFrame ||
                keyboard.spaceKey
                    .wasPressedThisFrame)
            {
                if (Runner
                        .AvailableChoices
                        .Count == 0)
                {
                    Runner.Continue();
                }

                return;
            }

            for (int i = 0;
                 i < Runner
                     .AvailableChoices.Count &&
                 i < 8;
                 i++)
            {
                if (WasNumberPressed(
                        keyboard,
                        i))
                {
                    Runner.SelectChoice(i);
                    return;
                }
            }

            if (keyboard.escapeKey
                    .wasPressedThisFrame &&
                Runner.Definition != null &&
                Runner.Definition.allowCancel)
            {
                Runner.Cancel();
            }
        }

        private void Build()
        {
            EnsureEventSystem();

            var canvasObject =
                new GameObject(
                    "DialogueCanvas",
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
                RenderMode
                    .ScreenSpaceOverlay;

            canvas.sortingOrder =
                6200;

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

            RectTransform canvasRect =
                canvasObject
                    .GetComponent<
                        RectTransform>();

            root =
                MenuUiFactory.CreateRect(
                    "DialogueRoot",
                    canvasRect)
                    .gameObject;

            MenuUiFactory.Stretch(
                root.GetComponent<
                    RectTransform>());

            Image lowerVeil =
                MenuUiFactory.CreateImage(
                    "LowerVeil",
                    root.transform,
                    new Color(
                        0.005f,
                        0.008f,
                        0.011f,
                        0.78f));

            RectTransform veilRect =
                lowerVeil.rectTransform;

            veilRect.anchorMin =
                new Vector2(0f, 0f);

            veilRect.anchorMax =
                new Vector2(1f, 0.45f);

            veilRect.offsetMin =
                Vector2.zero;

            veilRect.offsetMax =
                Vector2.zero;

            Image panel =
                MenuUiFactory.CreateImage(
                    "DialoguePanel",
                    root.transform,
                    new Color(
                        MainMenuTheme.Panel.r,
                        MainMenuTheme.Panel.g,
                        MainMenuTheme.Panel.b,
                        0.96f));

            RectTransform panelRect =
                panel.rectTransform;

            panelRect.anchorMin =
                new Vector2(
                    0.07f,
                    0.055f);

            panelRect.anchorMax =
                new Vector2(
                    0.93f,
                    0.40f);

            panelRect.offsetMin =
                Vector2.zero;

            panelRect.offsetMax =
                Vector2.zero;

            Outline outline =
                panel.gameObject
                    .AddComponent<Outline>();

            outline.effectColor =
                new Color(
                    MainMenuTheme
                        .SilverDim.r,
                    MainMenuTheme
                        .SilverDim.g,
                    MainMenuTheme
                        .SilverDim.b,
                    0.65f);

            outline.effectDistance =
                new Vector2(1f, -1f);

            portrait =
                MenuUiFactory.CreateImage(
                    "Portrait",
                    panel.transform,
                    new Color(
                        1f,
                        1f,
                        1f,
                        0f));

            RectTransform portraitRect =
                portrait.rectTransform;

            portraitRect.anchorMin =
                new Vector2(0f, 0f);

            portraitRect.anchorMax =
                new Vector2(0f, 1f);

            portraitRect.pivot =
                new Vector2(0f, 0.5f);

            portraitRect
                .anchoredPosition =
                    new Vector2(24f, 0f);

            portraitRect.sizeDelta =
                new Vector2(
                    230f,
                    -44f);

            portrait.preserveAspect =
                true;

            portrait.raycastTarget =
                false;

            speaker =
                MenuUiFactory.CreateText(
                    "Speaker",
                    panel.transform,
                    string.Empty,
                    22,
                    MainMenuTheme.Silver,
                    TextAnchor.MiddleLeft);

            speaker.rectTransform
                .anchorMin =
                    new Vector2(
                        0f,
                        0.78f);

            speaker.rectTransform
                .anchorMax =
                    new Vector2(
                        0.62f,
                        1f);

            speaker.rectTransform
                .offsetMin =
                    new Vector2(
                        270f,
                        0f);

            speaker.rectTransform
                .offsetMax =
                    new Vector2(
                        -18f,
                        -12f);

            body =
                MenuUiFactory.CreateText(
                    "Body",
                    panel.transform,
                    string.Empty,
                    20,
                    MainMenuTheme.Text,
                    TextAnchor.UpperLeft);

            body.rectTransform
                .anchorMin =
                    new Vector2(
                        0f,
                        0.18f);

            body.rectTransform
                .anchorMax =
                    new Vector2(
                        0.61f,
                        0.80f);

            body.rectTransform
                .offsetMin =
                    new Vector2(
                        270f,
                        10f);

            body.rectTransform
                .offsetMax =
                    new Vector2(
                        -18f,
                        -4f);

            choicesRoot =
                MenuUiFactory.CreateRect(
                    "Choices",
                    panel.transform);

            choicesRoot.anchorMin =
                new Vector2(
                    0.64f,
                    0.12f);

            choicesRoot.anchorMax =
                new Vector2(
                    0.98f,
                    0.90f);

            choicesRoot.offsetMin =
                Vector2.zero;

            choicesRoot.offsetMax =
                Vector2.zero;

            VerticalLayoutGroup layout =
                choicesRoot.gameObject
                    .AddComponent<
                        VerticalLayoutGroup>();

            layout.spacing = 8f;
            layout.childControlHeight =
                true;
            layout.childControlWidth =
                true;
            layout.childForceExpandHeight =
                false;
            layout.childForceExpandWidth =
                true;
            layout.childAlignment =
                TextAnchor.LowerCenter;

            continueButton =
                MenuUiFactory
                    .CreateMenuButton(
                        "Continue",
                        panel.transform,
                        "Продолжить",
                        17);

            RectTransform continueRect =
                continueButton
                    .GetComponent<
                        RectTransform>();

            continueRect.anchorMin =
                continueRect.anchorMax =
                    new Vector2(
                        0.98f,
                        0.05f);

            continueRect.pivot =
                new Vector2(1f, 0f);

            continueRect.sizeDelta =
                new Vector2(
                    190f,
                    46f);

            continueRect
                .anchoredPosition =
                    Vector2.zero;

            continueButton.onClick
                .AddListener(() =>
                {
                    GameAudioService
                        .Instance
                        .PlayClick();

                    Runner.Continue();
                });

            cancelButton =
                MenuUiFactory
                    .CreateMenuButton(
                        "Cancel",
                        panel.transform,
                        "Закрыть",
                        15);

            RectTransform cancelRect =
                cancelButton
                    .GetComponent<
                        RectTransform>();

            cancelRect.anchorMin =
                cancelRect.anchorMax =
                    new Vector2(
                        0.02f,
                        0.05f);

            cancelRect.pivot =
                new Vector2(0f, 0f);

            cancelRect.sizeDelta =
                new Vector2(
                    140f,
                    42f);

            cancelRect
                .anchoredPosition =
                    Vector2.zero;

            cancelButton.onClick
                .AddListener(() =>
                {
                    GameAudioService
                        .Instance
                        .PlayClick();

                    Runner.Cancel();
                });
        }

        private void OnDialogueStarted()
        {
            root.SetActive(true);
        }

        private void OnNodeChanged(
            DialogueNode node)
        {
            Refresh();
        }

        private void OnDialogueEnded()
        {
            root.SetActive(false);
        }

        private void Refresh()
        {
            DialogueNode node =
                Runner.CurrentNode;

            if (node == null)
                return;

            speaker.text =
                string.IsNullOrWhiteSpace(
                    node.speakerName)
                    ? node.speakerId
                    : node.speakerName;

            body.text =
                node.text ??
                string.Empty;

            if (node.speakerPortrait != null)
            {
                portrait.sprite =
                    node.speakerPortrait;

                portrait.color =
                    Color.white;
            }
            else
            {
                portrait.sprite = null;
                portrait.color =
                    new Color(
                        1f,
                        1f,
                        1f,
                        0f);
            }

            for (int i =
                     choicesRoot.childCount - 1;
                 i >= 0;
                 i--)
            {
                Destroy(
                    choicesRoot
                        .GetChild(i)
                        .gameObject);
            }

            for (int i = 0;
                 i <
                 Runner.AvailableChoices.Count;
                 i++)
            {
                int index = i;

                DialogueChoice choice =
                    Runner.AvailableChoices[i];

                Button button =
                    MenuUiFactory
                        .CreateMenuButton(
                            "Choice_" + i,
                            choicesRoot,
                            $"{i + 1}. {choice.text}",
                            16);

                LayoutElement element =
                    button.gameObject
                        .AddComponent<
                            LayoutElement>();

                element.preferredHeight =
                    50f;

                element.minHeight =
                    46f;

                button.onClick
                    .AddListener(
                        () =>
                        {
                            GameAudioService
                                .Instance
                                .PlayClick();

                            Runner.SelectChoice(
                                index);
                        });
            }

            bool hasChoices =
                Runner
                    .AvailableChoices
                    .Count > 0;

            continueButton.gameObject
                .SetActive(
                    !hasChoices);

            cancelButton.gameObject
                .SetActive(
                    Runner.Definition != null &&
                    Runner.Definition
                        .allowCancel);

            if (EventSystem.current == null)
                return;

            if (hasChoices &&
                choicesRoot.childCount > 0)
            {
                EventSystem.current
                    .SetSelectedGameObject(
                        choicesRoot
                            .GetChild(0)
                            .gameObject);
            }
            else
            {
                EventSystem.current
                    .SetSelectedGameObject(
                        continueButton
                            .gameObject);
            }
        }

        private static bool
            WasNumberPressed(
                Keyboard keyboard,
                int zeroBased)
        {
            switch (zeroBased)
            {
                case 0:
                    return keyboard
                        .digit1Key
                        .wasPressedThisFrame;
                case 1:
                    return keyboard
                        .digit2Key
                        .wasPressedThisFrame;
                case 2:
                    return keyboard
                        .digit3Key
                        .wasPressedThisFrame;
                case 3:
                    return keyboard
                        .digit4Key
                        .wasPressedThisFrame;
                case 4:
                    return keyboard
                        .digit5Key
                        .wasPressedThisFrame;
                case 5:
                    return keyboard
                        .digit6Key
                        .wasPressedThisFrame;
                case 6:
                    return keyboard
                        .digit7Key
                        .wasPressedThisFrame;
                case 7:
                    return keyboard
                        .digit8Key
                        .wasPressedThisFrame;
                default:
                    return false;
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
                    typeof(
                        InputSystemUIInputModule));

            eventSystem.transform
                .SetParent(
                    transform,
                    false);
        }
    }
}
