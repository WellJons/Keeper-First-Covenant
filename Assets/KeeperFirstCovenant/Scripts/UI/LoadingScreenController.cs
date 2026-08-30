using System.Collections;
using KeeperFirstCovenant.Core;
using UnityEngine;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public sealed class LoadingScreenController : MonoBehaviour
    {
        private static LoadingScreenController instance;

        private CanvasGroup group;
        private Image progressFill;
        private Text loadingText;
        private Text hintText;
        private RectTransform spinner;
        private Coroutine fadeRoutine;

        public static LoadingScreenController Instance
        {
            get
            {
                EnsureExists();
                return instance;
            }
        }

        public static void EnsureExists()
        {
            if (instance != null)
                return;

            instance = FindFirstObjectByType<LoadingScreenController>();
            if (instance != null)
                return;

            var root = new GameObject("Keeper_LoadingScreen");
            instance = root.AddComponent<LoadingScreenController>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            Build();
            HideImmediate();
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        private void Update()
        {
            if (spinner != null && group != null && group.alpha > 0.01f)
                spinner.Rotate(0f, 0f, -42f * Time.unscaledDeltaTime);
        }

        public void Show(string sceneName)
        {
            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);

            gameObject.SetActive(true);
            group.alpha = 1f;
            group.blocksRaycasts = true;
            group.interactable = true;

            SetProgress(0f);

            loadingText.text = GetLoadingLabel(sceneName);
            hintText.text = GetHint(sceneName);
        }

        public void SetProgress(float progress)
        {
            if (progressFill == null)
                return;

            float value = Mathf.Clamp01(progress);
            RectTransform rect = progressFill.rectTransform;
            rect.anchorMax = new Vector2(value, 1f);
        }

        public void Hide()
        {
            if (!gameObject.activeSelf)
                return;

            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);

            fadeRoutine = StartCoroutine(FadeOut());
        }

        public void HideImmediate()
        {
            if (group != null)
            {
                group.alpha = 0f;
                group.blocksRaycasts = false;
                group.interactable = false;
            }

            if (progressFill != null)
            {
                RectTransform rect = progressFill.rectTransform;
                rect.anchorMax = new Vector2(0f, 1f);
            }
        }

        private void Build()
        {
            var canvasObject = new GameObject(
                "LoadingCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();

            group = canvasObject.AddComponent<CanvasGroup>();

            Image baseBlack = MenuUiFactory.CreateImage(
                "Base",
                canvasRect,
                new Color(0.008f, 0.011f, 0.014f, 1f));

            MenuUiFactory.Stretch(baseBlack.rectTransform);

            RectTransform artHost = MenuUiFactory.CreateRect("Art", canvasRect);
            MenuUiFactory.Stretch(artHost);
            var liveBackground = artHost.gameObject.AddComponent<MenuLiveBackground>();
            liveBackground.BuildIfNeeded();

            Image veil = MenuUiFactory.CreateImage(
                "Veil",
                canvasRect,
                new Color(0f, 0f, 0f, 0.58f));

            MenuUiFactory.Stretch(veil.rectTransform);

            RectTransform info = MenuUiFactory.CreateRect("LoadingInfo", canvasRect);
            MenuUiFactory.SetAnchoredRect(
                info,
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 58f),
                new Vector2(-180f, 170f));

            loadingText = MenuUiFactory.CreateText(
                "Loading",
                info,
                "ЗАГРУЗКА",
                24,
                MainMenuTheme.Text,
                TextAnchor.LowerLeft,
                FontStyle.Normal);

            loadingText.rectTransform.anchorMin = new Vector2(0f, 0.56f);
            loadingText.rectTransform.anchorMax = new Vector2(1f, 1f);
            loadingText.rectTransform.offsetMin = Vector2.zero;
            loadingText.rectTransform.offsetMax = Vector2.zero;

            hintText = MenuUiFactory.CreateText(
                "Hint",
                info,
                string.Empty,
                14,
                MainMenuTheme.MutedText,
                TextAnchor.UpperLeft);

            hintText.rectTransform.anchorMin = new Vector2(0f, 0.25f);
            hintText.rectTransform.anchorMax = new Vector2(1f, 0.60f);
            hintText.rectTransform.offsetMin = Vector2.zero;
            hintText.rectTransform.offsetMax = Vector2.zero;

            Image track = MenuUiFactory.CreateImage(
                "ProgressTrack",
                info,
                new Color(MainMenuTheme.SilverDim.r, MainMenuTheme.SilverDim.g, MainMenuTheme.SilverDim.b, 0.22f));

            RectTransform trackRect = track.rectTransform;
            trackRect.anchorMin = new Vector2(0f, 0f);
            trackRect.anchorMax = new Vector2(1f, 0f);
            trackRect.pivot = new Vector2(0.5f, 0f);
            trackRect.sizeDelta = new Vector2(0f, 3f);
            trackRect.anchoredPosition = Vector2.zero;

            progressFill = MenuUiFactory.CreateImage(
                "ProgressFill",
                track.transform,
                MainMenuTheme.Warm);

            RectTransform fillRect = progressFill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            spinner = BuildSpinner(canvasRect);
        }

        private static RectTransform BuildSpinner(RectTransform parent)
        {
            RectTransform root = MenuUiFactory.CreateRect("Spinner", parent);
            root.anchorMin = root.anchorMax = new Vector2(1f, 0f);
            root.pivot = new Vector2(1f, 0f);
            root.anchoredPosition = new Vector2(-78f, 68f);
            root.sizeDelta = new Vector2(54f, 54f);

            for (int i = 0; i < 8; i++)
            {
                Image mark = MenuUiFactory.CreateImage(
                    "Mark_" + i,
                    root,
                    new Color(
                        MainMenuTheme.Silver.r,
                        MainMenuTheme.Silver.g,
                        MainMenuTheme.Silver.b,
                        Mathf.Lerp(0.18f, 0.90f, i / 7f)));

                RectTransform rect = mark.rectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.sizeDelta = new Vector2(3f, 13f);
                rect.anchoredPosition = Vector2.zero;
                rect.localRotation = Quaternion.Euler(0f, 0f, i * 45f);
                rect.anchoredPosition = rect.up * 17f;
            }

            return root;
        }

        private IEnumerator FadeOut()
        {
            float start = group.alpha;
            float elapsed = 0f;
            const float duration = 0.35f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                group.alpha = Mathf.Lerp(start, 0f, t);
                yield return null;
            }

            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
            fadeRoutine = null;
        }

        private static string GetLoadingLabel(string sceneName)
        {
            if (sceneName == GameFlowController.MainMenuSceneName)
                return "ВОЗВРАЩЕНИЕ В ГЛАВНОЕ МЕНЮ";

            if (sceneName == GameFlowController.FirstPlayableSceneName)
                return "ДОРОГА К РЕЙНХОЛЬМУ";

            return "ЗАГРУЗКА";
        }

        private static string GetHint(string sceneName)
        {
            if (sceneName == GameFlowController.MainMenuSceneName)
                return "Первый Завет помнит последнее сохранение.";

            return "Мир сохраняет последствия решений — не только победы.";
        }
    }
}
