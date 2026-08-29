using KeeperFirstCovenant.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public sealed class InteractionPromptHud : MonoBehaviour
    {
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        private CanvasGroup group;
        private RectTransform panelRect;
        private Image panel;
        private Text prompt;
        private Text detail;
        private Text glyph;

        private WorldInteractionController controller;
        private float nextLookup;

        private void Start()
        {
            Build();
            SetVisible(false);
        }

        private void Update()
        {
            if (controller == null &&
                Time.unscaledTime >= nextLookup)
            {
                nextLookup =
                    Time.unscaledTime + 0.5f;

                controller =
                    FindFirstObjectByType<
                        WorldInteractionController>();
            }

            if (controller == null ||
                !controller.HasHoverTarget)
            {
                SetVisible(false);
                return;
            }

            Refresh();
        }

        private void Build()
        {
            var canvasObject =
                new GameObject(
                    "InteractionPromptCanvas",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler));

            canvasObject.transform
                .SetParent(
                    transform,
                    false);

            Canvas canvas =
                canvasObject
                    .GetComponent<Canvas>();

            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;

            canvas.sortingOrder = 520;

            CanvasScaler scaler =
                canvasObject
                    .GetComponent<
                        CanvasScaler>();

            scaler.uiScaleMode =
                CanvasScaler.ScaleMode
                    .ScaleWithScreenSize;

            scaler.referenceResolution =
                new Vector2(
                    ReferenceWidth,
                    ReferenceHeight);

            scaler.matchWidthOrHeight =
                0.5f;

            RectTransform canvasRect =
                canvasObject
                    .GetComponent<
                        RectTransform>();

            panel =
                MenuUiFactory.CreateImage(
                    "InteractionPrompt",
                    canvasRect,
                    new Color(
                        MainMenuTheme.Panel.r,
                        MainMenuTheme.Panel.g,
                        MainMenuTheme.Panel.b,
                        0.94f));

            panelRect =
                panel.rectTransform;

            panelRect.anchorMin =
                panelRect.anchorMax =
                    Vector2.zero;

            panelRect.pivot =
                Vector2.zero;

            panelRect.sizeDelta =
                new Vector2(
                    390f,
                    88f);

            group =
                panel.gameObject
                    .AddComponent<CanvasGroup>();

            KeeperUiSkin.DecorateMajorPanel(
                panel,
                false);

            glyph =
                MenuUiFactory.CreateText(
                    "Glyph",
                    panel.transform,
                    "◇",
                    25,
                    MainMenuTheme.Silver,
                    TextAnchor.MiddleCenter);

            RectTransform glyphRect =
                glyph.rectTransform;

            glyphRect.anchorMin =
                new Vector2(0f, 0f);

            glyphRect.anchorMax =
                new Vector2(0f, 1f);

            glyphRect.pivot =
                new Vector2(0f, 0.5f);

            glyphRect.anchoredPosition =
                new Vector2(14f, 0f);

            glyphRect.sizeDelta =
                new Vector2(38f, 0f);

            prompt =
                MenuUiFactory.CreateText(
                    "Prompt",
                    panel.transform,
                    string.Empty,
                    17,
                    MainMenuTheme.Text,
                    TextAnchor.MiddleLeft);

            prompt.rectTransform.anchorMin =
                new Vector2(0f, 0.44f);

            prompt.rectTransform.anchorMax =
                Vector2.one;

            prompt.rectTransform.offsetMin =
                new Vector2(58f, 0f);

            prompt.rectTransform.offsetMax =
                new Vector2(-14f, -7f);

            detail =
                MenuUiFactory.CreateText(
                    "Detail",
                    panel.transform,
                    string.Empty,
                    12,
                    MainMenuTheme.MutedText,
                    TextAnchor.MiddleLeft);

            detail.rectTransform.anchorMin =
                Vector2.zero;

            detail.rectTransform.anchorMax =
                new Vector2(1f, 0.46f);

            detail.rectTransform.offsetMin =
                new Vector2(58f, 7f);

            detail.rectTransform.offsetMax =
                new Vector2(-14f, 0f);
        }

        private void Refresh()
        {
            bool canInteract =
                controller.CurrentActor != null &&
                controller.CurrentInteractable != null &&
                controller.CurrentInteractable
                    .CanInteract(
                        controller.CurrentActor);

            bool canInspect =
                controller.CurrentActor != null &&
                controller.CurrentInspectable != null &&
                controller.CurrentInspectable
                    .CanInspect(
                        controller.CurrentActor);

            bool usable =
                controller.CurrentInRange &&
                (canInteract || canInspect);

            prompt.text =
                controller.CurrentPrompt;

            detail.text =
                canInteract || canInspect
                    ? controller.CurrentContextHint
                    : "Недоступно";

            Color stateColor =
                usable
                    ? MainMenuTheme.Warm
                    : controller.CurrentInRange
                        ? MainMenuTheme.MutedText
                        : MainMenuTheme.Danger;

            glyph.color = stateColor;

            prompt.color =
                usable
                    ? MainMenuTheme.Text
                    : MainMenuTheme.MutedText;

            detail.color =
                controller.CurrentInRange
                    ? MainMenuTheme.MutedText
                    : MainMenuTheme.Danger;

            UpdatePosition();
            SetVisible(true);
        }

        private void UpdatePosition()
        {
            Mouse mouse =
                Mouse.current;

            if (mouse == null)
                return;

            Vector2 raw =
                mouse.position.ReadValue();

            float x =
                Screen.width > 0
                    ? raw.x /
                      Screen.width *
                      ReferenceWidth
                    : raw.x;

            float y =
                Screen.height > 0
                    ? raw.y /
                      Screen.height *
                      ReferenceHeight
                    : raw.y;

            float desiredX =
                x + 24f;

            float desiredY =
                y + 24f;

            float maxX =
                ReferenceWidth -
                panelRect.sizeDelta.x -
                18f;

            float maxY =
                ReferenceHeight -
                panelRect.sizeDelta.y -
                18f;

            panelRect.anchoredPosition =
                new Vector2(
                    Mathf.Clamp(
                        desiredX,
                        18f,
                        maxX),
                    Mathf.Clamp(
                        desiredY,
                        18f,
                        maxY));
        }

        private void SetVisible(
            bool visible)
        {
            if (group == null)
                return;

            group.alpha =
                visible ? 1f : 0f;

            group.blocksRaycasts = false;
            group.interactable = false;
        }
    }
}
