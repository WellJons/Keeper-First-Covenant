using KeeperFirstCovenant.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public static class MenuUiFactory
    {
        private static Font cachedFont;

        public static Font DefaultFont
        {
            get
            {
                if (cachedFont != null)
                    return cachedFont;

                cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (cachedFont == null)
                    cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

                return cachedFont;
            }
        }

        public static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        public static Image CreateImage(string name, Transform parent, Color color)
        {
            RectTransform rect = CreateRect(name, parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        public static Text CreateText(
            string name,
            Transform parent,
            string value,
            int fontSize,
            Color color,
            TextAnchor alignment = TextAnchor.MiddleLeft,
            FontStyle style = FontStyle.Normal)
        {
            RectTransform rect = CreateRect(name, parent);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = DefaultFont;
            text.text = value;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        public static Button CreateMenuButton(string name, Transform parent, string label, int fontSize = 26)
        {
            Image background = CreateImage(name, parent, new Color(0.03f, 0.04f, 0.05f, 0.62f));
            Button button = background.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;

            Outline outline = background.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(MainMenuTheme.SilverDim.r, MainMenuTheme.SilverDim.g, MainMenuTheme.SilverDim.b, 0.48f);
            outline.effectDistance = new Vector2(1f, -1f);

            Text text = CreateText("Label", background.transform, label, fontSize, MainMenuTheme.Text);
            Stretch(text.rectTransform, 22f, 10f, 22f, 10f);

            Image accent = CreateImage("WarmAccent", background.transform, MainMenuTheme.Warm);
            RectTransform accentRect = accent.rectTransform;
            accentRect.anchorMin = new Vector2(0f, 0f);
            accentRect.anchorMax = new Vector2(0f, 1f);
            accentRect.pivot = new Vector2(0f, 0.5f);
            accentRect.offsetMin = Vector2.zero;
            accentRect.offsetMax = new Vector2(3f, 0f);

            var visual = background.gameObject.AddComponent<MenuButtonVisual>();
            visual.Configure(background, text, accent, button);
            return button;
        }

        public static Slider CreateSlider(string name, Transform parent, float min, float max, float value)
        {
            RectTransform root = CreateRect(name, parent);
            Slider slider = root.gameObject.AddComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;

            Image background = CreateImage("Background", root, new Color(0.12f, 0.14f, 0.15f, 1f));
            Stretch(background.rectTransform, 0f, 12f, 0f, 12f);

            RectTransform fillArea = CreateRect("Fill Area", root);
            Stretch(fillArea, 0f, 12f, 0f, 12f);

            Image fill = CreateImage("Fill", fillArea, MainMenuTheme.Silver);
            Stretch(fill.rectTransform);

            RectTransform handleArea = CreateRect("Handle Slide Area", root);
            Stretch(handleArea, 0f, 4f, 0f, 4f);

            Image handle = CreateImage("Handle", handleArea, MainMenuTheme.Warm);
            RectTransform handleRect = handle.rectTransform;
            handleRect.sizeDelta = new Vector2(14f, 28f);

            slider.fillRect = fill.rectTransform;
            slider.handleRect = handleRect;
            slider.targetGraphic = handle;

            return slider;
        }

        public static Toggle CreateToggle(string name, Transform parent, string label, bool value)
        {
            RectTransform root = CreateRect(name, parent);
            Toggle toggle = root.gameObject.AddComponent<Toggle>();

            Image background = CreateImage("Box", root, new Color(0.05f, 0.06f, 0.07f, 1f));
            RectTransform boxRect = background.rectTransform;
            boxRect.anchorMin = new Vector2(0f, 0.5f);
            boxRect.anchorMax = new Vector2(0f, 0.5f);
            boxRect.pivot = new Vector2(0f, 0.5f);
            boxRect.sizeDelta = new Vector2(30f, 30f);
            boxRect.anchoredPosition = new Vector2(0f, 0f);

            Image check = CreateImage("Check", background.transform, MainMenuTheme.Warm);
            Stretch(check.rectTransform, 6f, 6f, 6f, 6f);

            Text text = CreateText("Label", root, label, 21, MainMenuTheme.Text);
            RectTransform textRect = text.rectTransform;
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.offsetMin = new Vector2(46f, 0f);
            textRect.offsetMax = Vector2.zero;

            toggle.targetGraphic = background;
            toggle.graphic = check;
            toggle.isOn = value;
            return toggle;
        }

        public static Dropdown CreateDropdown(string name, Transform parent, int fontSize = 20)
        {
            Image rootImage = CreateImage(name, parent, new Color(0.045f, 0.052f, 0.06f, 0.94f));
            Dropdown dropdown = rootImage.gameObject.AddComponent<Dropdown>();

            Text label = CreateText("Label", rootImage.transform, string.Empty, fontSize, MainMenuTheme.Text);
            Stretch(label.rectTransform, 14f, 8f, 42f, 8f);

            Text arrow = CreateText("Arrow", rootImage.transform, "▾", 20, MainMenuTheme.Silver, TextAnchor.MiddleCenter);
            RectTransform arrowRect = arrow.rectTransform;
            arrowRect.anchorMin = new Vector2(1f, 0f);
            arrowRect.anchorMax = new Vector2(1f, 1f);
            arrowRect.pivot = new Vector2(1f, 0.5f);
            arrowRect.sizeDelta = new Vector2(36f, 0f);
            arrowRect.anchoredPosition = Vector2.zero;

            Image templateImage = CreateImage("Template", rootImage.transform, new Color(0.025f, 0.03f, 0.036f, 0.98f));
            RectTransform template = templateImage.rectTransform;
            template.anchorMin = new Vector2(0f, 0f);
            template.anchorMax = new Vector2(1f, 0f);
            template.pivot = new Vector2(0.5f, 1f);
            template.anchoredPosition = new Vector2(0f, -4f);
            template.sizeDelta = new Vector2(0f, 220f);
            templateImage.gameObject.SetActive(false);

            ScrollRect scrollRect = templateImage.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;

            Image viewportImage = CreateImage("Viewport", template, new Color(0f, 0f, 0f, 0.01f));
            viewportImage.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            Stretch(viewportImage.rectTransform);

            RectTransform content = CreateRect("Content", viewportImage.transform);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = new Vector2(0f, 34f);

            Toggle itemToggle = CreateToggle("Item", content, string.Empty, false);
            RectTransform itemRect = itemToggle.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0f, 0.5f);
            itemRect.anchorMax = new Vector2(1f, 0.5f);
            itemRect.sizeDelta = new Vector2(0f, 34f);

            Text itemText = itemRect.Find("Label").GetComponent<Text>();
            itemText.fontSize = fontSize;
            itemText.color = MainMenuTheme.Text;

            scrollRect.viewport = viewportImage.rectTransform;
            scrollRect.content = content;

            dropdown.targetGraphic = rootImage;
            dropdown.template = template;
            dropdown.captionText = label;
            dropdown.itemText = itemText;
            return dropdown;
        }

        public static void Stretch(RectTransform rect, float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        public static void SetAnchoredRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }
    }

    public sealed class MenuButtonVisual : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        ISelectHandler,
        IDeselectHandler
    {
        private Image background;
        private Text label;
        private Image accent;
        private Button button;
        private bool highlighted;

        public void Configure(Image backgroundImage, Text labelText, Image accentImage, Button targetButton)
        {
            background = backgroundImage;
            label = labelText;
            accent = accentImage;
            button = targetButton;
            Apply(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!IsInteractable())
                return;

            highlighted = true;
            Apply(true);
            GameAudioService.Instance.PlayHover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            highlighted = false;
            Apply(false);
        }

        public void OnSelect(BaseEventData eventData)
        {
            highlighted = true;
            Apply(true);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            highlighted = false;
            Apply(false);
        }

        private void Update()
        {
            if (!IsInteractable() && highlighted)
            {
                highlighted = false;
                Apply(false);
            }
        }

        private bool IsInteractable()
        {
            return button != null && button.IsInteractable();
        }

        private void Apply(bool active)
        {
            bool interactable = IsInteractable();

            if (background != null)
            {
                background.color = active && interactable
                    ? new Color(0.075f, 0.085f, 0.092f, 0.90f)
                    : new Color(0.03f, 0.04f, 0.05f, 0.62f);
            }

            if (label != null)
                label.color = interactable
                    ? (active ? Color.white : MainMenuTheme.Text)
                    : MainMenuTheme.DisabledText;

            if (accent != null)
                accent.color = interactable
                    ? (active ? MainMenuTheme.Warm : new Color(MainMenuTheme.Warm.r, MainMenuTheme.Warm.g, MainMenuTheme.Warm.b, 0.48f))
                    : new Color(MainMenuTheme.SilverDim.r, MainMenuTheme.SilverDim.g, MainMenuTheme.SilverDim.b, 0.25f);
        }
    }
}
