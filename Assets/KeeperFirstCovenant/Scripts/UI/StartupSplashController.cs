using System.Collections;
using KeeperFirstCovenant.Core;
using UnityEngine;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public sealed class StartupSplashController : MonoBehaviour
    {
        [SerializeField] private float holdSeconds = 1.35f;
        [SerializeField] private float fadeSeconds = 0.65f;

        private CanvasGroup contentGroup;
        private RectTransform titleRoot;
        private bool built;

        private void Start()
        {
            Build();
            StartCoroutine(RunSplash());
        }

        private void Build()
        {
            if (built)
                return;

            built = true;

            var canvasObject = new GameObject(
                "BootCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();

            RectTransform background = MenuUiFactory.CreateRect("LiveBackground", canvasRect);
            MenuUiFactory.Stretch(background);
            background.gameObject.AddComponent<MenuLiveBackground>().BuildIfNeeded();

            Image veil = MenuUiFactory.CreateImage("Veil", canvasRect, new Color(0f, 0f, 0f, 0.24f));
            MenuUiFactory.Stretch(veil.rectTransform);
            veil.raycastTarget = false;

            titleRoot = MenuUiFactory.CreateRect("TitleRoot", canvasRect);
            MenuUiFactory.SetAnchoredRect(
                titleRoot,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 18f),
                new Vector2(820f, 240f));

            contentGroup = titleRoot.gameObject.AddComponent<CanvasGroup>();
            contentGroup.alpha = 0f;

            Text title = MenuUiFactory.CreateText(
                "Title",
                titleRoot,
                "ХРАНИТЕЛЬ",
                60,
                MainMenuTheme.Text,
                TextAnchor.MiddleCenter,
                FontStyle.Normal);

            title.rectTransform.anchorMin = new Vector2(0f, 0.46f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.offsetMin = Vector2.zero;
            title.rectTransform.offsetMax = Vector2.zero;

            Text subtitle = MenuUiFactory.CreateText(
                "Subtitle",
                titleRoot,
                "П Е Р В Ы Й   З А В Е Т",
                20,
                MainMenuTheme.Silver,
                TextAnchor.MiddleCenter);

            subtitle.rectTransform.anchorMin = new Vector2(0f, 0.26f);
            subtitle.rectTransform.anchorMax = new Vector2(1f, 0.50f);
            subtitle.rectTransform.offsetMin = Vector2.zero;
            subtitle.rectTransform.offsetMax = Vector2.zero;

            Image line = MenuUiFactory.CreateImage("SilverLine", titleRoot, MainMenuTheme.SilverDim);
            RectTransform lineRect = line.rectTransform;
            lineRect.anchorMin = lineRect.anchorMax = new Vector2(0.5f, 0.24f);
            lineRect.pivot = new Vector2(0.5f, 0.5f);
            lineRect.sizeDelta = new Vector2(220f, 1f);
            lineRect.anchoredPosition = Vector2.zero;

            Text status = MenuUiFactory.CreateText(
                "Status",
                titleRoot,
                "ПЕРВЫЙ ЗАПУСК",
                13,
                new Color(MainMenuTheme.MutedText.r, MainMenuTheme.MutedText.g, MainMenuTheme.MutedText.b, 0.70f),
                TextAnchor.MiddleCenter);

            status.rectTransform.anchorMin = new Vector2(0f, 0f);
            status.rectTransform.anchorMax = new Vector2(1f, 0.18f);
            status.rectTransform.offsetMin = Vector2.zero;
            status.rectTransform.offsetMax = Vector2.zero;
        }

        private IEnumerator RunSplash()
        {
            SettingsService.ApplyCurrent();
            GameAudioService.Instance.PlayMenuAmbience();

            float elapsed = 0f;
            float revealDuration = Mathf.Max(0.2f, fadeSeconds);

            while (elapsed < revealDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / revealDuration);
                contentGroup.alpha = Mathf.SmoothStep(0f, 1f, t);
                titleRoot.localScale = Vector3.one * Mathf.Lerp(0.985f, 1f, t);
                yield return null;
            }

            yield return new WaitForSecondsRealtime(Mathf.Max(0.2f, holdSeconds));

            elapsed = 0f;
            while (elapsed < fadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, fadeSeconds));
                contentGroup.alpha = 1f - Mathf.SmoothStep(0f, 1f, t);
                yield return null;
            }

            GameFlowController.Instance.BeginLoad(GameFlowController.MainMenuSceneName);
        }
    }
}
