using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KeeperFirstCovenant.Core
{
    public sealed class GameFlowController : MonoBehaviour
    {
        public const string BootSceneName = "Boot";
        public const string MainMenuSceneName = "MainMenu";
        public const string FirstPlayableSceneName = "Prototype_Road";

        private const string ActiveSlotKey = "Keeper.ActiveSaveSlot";
        private static GameFlowController instance;

        public static GameFlowController Instance
        {
            get
            {
                EnsureExists();
                return instance;
            }
        }

        public bool IsTransitioning { get; private set; }
        public int ActiveSlotId { get; private set; }

        public event Action<string> FlowError;
        public event Action<float> TransitionProgress;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            SettingsService.Load();
            EnsureExists();
            GameAudioService.EnsureExists();
        }

        private static void EnsureExists()
        {
            if (instance != null)
                return;

            instance = FindFirstObjectByType<GameFlowController>();
            if (instance != null)
                return;

            var root = new GameObject("Keeper_GameFlow");
            instance = root.AddComponent<GameFlowController>();
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
            ActiveSlotId = PlayerPrefs.GetInt(ActiveSlotKey, -1);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                instance = null;
            }
        }

        public bool TryContinue()
        {
            SaveGameData latest = SaveGameService.GetLatestSave();
            if (latest == null)
            {
                ReportError("Сохранений пока нет.");
                return false;
            }

            return LoadSave(latest);
        }

        public bool TryStartNewGame()
        {
            if (!Application.CanStreamedLevelBeLoaded(FirstPlayableSceneName))
            {
                ReportError("Первая игровая сцена ещё не добавлена в Build Settings.");
                return false;
            }

            SaveGameData data = SaveGameService.CreateNewGame(
                FirstPlayableSceneName,
                "Дорога к Рейнхольму");

            if (data == null)
            {
                ReportError("Все слоты заняты. Удалите ненужное сохранение.");
                return false;
            }

            ActiveSlotId = data.slotId;
            PersistActiveSlot();
            BeginLoad(data.sceneName);
            return true;
        }

        public bool LoadSave(SaveGameData data)
        {
            if (data == null)
            {
                ReportError("Сохранение не найдено.");
                return false;
            }

            string sceneName = string.IsNullOrWhiteSpace(data.sceneName)
                ? FirstPlayableSceneName
                : data.sceneName;

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                ReportError($"Сцена сохранения «{sceneName}» недоступна.");
                return false;
            }

            ActiveSlotId = data.slotId;
            PersistActiveSlot();
            BeginLoad(sceneName);
            return true;
        }

        public void ReturnToMainMenu()
        {
            BeginLoad(MainMenuSceneName);
        }

        public void BeginLoad(string sceneName)
        {
            if (IsTransitioning || string.IsNullOrWhiteSpace(sceneName))
                return;

            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            IsTransitioning = true;
            Time.timeScale = 1f;
            TransitionProgress?.Invoke(0f);

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            if (operation == null)
            {
                IsTransitioning = false;
                ReportError($"Не удалось начать загрузку сцены «{sceneName}».");
                yield break;
            }

            while (!operation.isDone)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                TransitionProgress?.Invoke(progress);
                yield return null;
            }

            TransitionProgress?.Invoke(1f);
            IsTransitioning = false;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == MainMenuSceneName)
            {
                GameAudioService.Instance.PlayMenuAmbience();
                return;
            }

            if (scene.name == BootSceneName)
                return;

            GameAudioService.Instance.StopMenuAmbience();

            if (ActiveSlotId > 0)
            {
                SaveGameData current = SaveGameService.LoadSlot(ActiveSlotId);
                string location = current != null && !string.IsNullOrWhiteSpace(current.locationName)
                    ? current.locationName
                    : scene.name;

                float playTime = current != null ? current.playTimeSeconds : 0f;
                SaveGameService.UpdateLocation(ActiveSlotId, scene.name, location, playTime);
            }
        }

        private void PersistActiveSlot()
        {
            PlayerPrefs.SetInt(ActiveSlotKey, ActiveSlotId);
            PlayerPrefs.Save();
        }

        private void ReportError(string message)
        {
            Debug.LogWarning(message);
            FlowError?.Invoke(message);
        }
    }
}
