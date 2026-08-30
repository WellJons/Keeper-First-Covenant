using System;
using System.Collections;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Dialogue;
using KeeperFirstCovenant.UI;
using KeeperFirstCovenant.World;
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

        private float sessionBasePlayTime;
        private float sessionElapsedPlayTime;
        private bool sessionClockRunning;

        private string pendingTravelSceneName;
        private string pendingTravelSpawnId;
        private string pendingTravelLocationName;

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

        public bool IsGameplayScene
        {
            get
            {
                Scene scene = SceneManager.GetActiveScene();
                return scene.IsValid() &&
                       scene.isLoaded &&
                       scene.name != BootSceneName &&
                       scene.name != MainMenuSceneName;
            }
        }

        public float CurrentPlayTimeSeconds =>
            Mathf.Max(0f, sessionBasePlayTime + sessionElapsedPlayTime);

        public event Action<string> FlowError;
        public event Action<float> TransitionProgress;
        public event Action<SaveGameData> GameSaved;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            SettingsService.Load();
            EnsureExists();
            GameAudioService.EnsureExists();
            LoadingScreenController.EnsureExists();
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

        private void Update()
        {
            if (!sessionClockRunning ||
                IsTransitioning ||
                Time.timeScale <= 0f ||
                !IsGameplayScene)
            {
                return;
            }

            sessionElapsedPlayTime += Time.unscaledDeltaTime;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                instance = null;
            }
        }

        private void OnApplicationQuit()
        {
            if (IsGameplayScene && ActiveSlotId > 0)
                SaveCurrentGame(false);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && IsGameplayScene && ActiveSlotId > 0)
                SaveCurrentGame(false);
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

            ClearPendingTravel();
            GameplaySaveBridge.ResetRuntimeState();

            SaveGameData data = SaveGameService.CreateNewGame(
                FirstPlayableSceneName,
                "Дорога к Рейнхольму");

            if (data == null)
            {
                ReportError("Все слоты заняты. Удалите ненужное сохранение.");
                return false;
            }

            ActiveSlotId = data.slotId;
            sessionBasePlayTime = data.playTimeSeconds;
            sessionElapsedPlayTime = 0f;
            sessionClockRunning = false;

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

            ClearPendingTravel();

            ActiveSlotId = data.slotId;
            sessionBasePlayTime = Mathf.Max(0f, data.playTimeSeconds);
            sessionElapsedPlayTime = 0f;
            sessionClockRunning = false;

            PersistActiveSlot();
            BeginLoad(sceneName);
            return true;
        }

        public bool LoadActiveSlot()
        {
            SaveGameData data = ActiveSlotId > 0
                ? SaveGameService.LoadSlot(ActiveSlotId)
                : null;

            if (data == null)
            {
                ReportError("Активное сохранение не найдено.");
                return false;
            }

            return LoadSave(data);
        }

        public bool SaveCurrentGame(
            bool manualSave = true,
            string locationOverride = null)
        {
            if (DialogueRunner.IsDialogueActive)
            {
                ReportError("Сохранение во время диалога недоступно.");
                return false;
            }

            if (TurnCombatDirector.Instance != null &&
                TurnCombatDirector.Instance.State == CombatState.Active)
            {
                ReportError("Сохранение во время активного боя пока недоступно.");
                return false;
            }

            if (!IsGameplayScene)
            {
                ReportError("Сохранять игру можно только внутри игровой сцены.");
                return false;
            }

            if (ActiveSlotId < 1)
            {
                ReportError("У текущей игры нет активного слота сохранения.");
                return false;
            }

            SaveGameData data = SaveGameService.LoadSlot(ActiveSlotId);
            if (data == null)
            {
                ReportError("Активное сохранение повреждено или отсутствует.");
                return false;
            }

            Scene scene = SceneManager.GetActiveScene();
            data.sceneName = scene.name;

            if (!string.IsNullOrWhiteSpace(locationOverride))
                data.locationName = locationOverride;
            else if (string.IsNullOrWhiteSpace(data.locationName))
                data.locationName = scene.name;

            data.playTimeSeconds = CurrentPlayTimeSeconds;
            data.manualSave = manualSave;

            GameplaySaveBridge.CaptureInto(data);

            if (!SaveGameService.WriteSave(data))
            {
                ReportError("Не удалось записать сохранение.");
                return false;
            }

            sessionBasePlayTime = data.playTimeSeconds;
            sessionElapsedPlayTime = 0f;

            GameSaved?.Invoke(data);
            return true;
        }

        public bool TravelToScene(
            string sceneName,
            string spawnId = "default",
            string locationName = null,
            bool saveBeforeTravel = true)
        {
            if (IsTransitioning)
                return false;

            if (DialogueRunner.IsDialogueActive)
            {
                ReportError("Нельзя сменить локацию во время диалога.");
                return false;
            }

            if (TurnCombatDirector.Instance != null &&
                TurnCombatDirector.Instance.State == CombatState.Active)
            {
                ReportError("Нельзя сменить локацию во время активного боя.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(sceneName) ||
                !Application.CanStreamedLevelBeLoaded(sceneName))
            {
                ReportError($"Локация «{sceneName}» недоступна.");
                return false;
            }

            if (saveBeforeTravel &&
                IsGameplayScene &&
                ActiveSlotId > 0 &&
                !SaveCurrentGame(false))
            {
                return false;
            }

            pendingTravelSceneName = sceneName;
            pendingTravelSpawnId = string.IsNullOrWhiteSpace(spawnId)
                ? "default"
                : spawnId;
            pendingTravelLocationName = locationName;

            BeginLoad(sceneName);
            return true;
        }

        public void ReturnToMainMenu(bool saveBeforeLeave = true)
        {
            if (saveBeforeLeave && IsGameplayScene && ActiveSlotId > 0)
                SaveCurrentGame(false);

            ClearPendingTravel();
            sessionClockRunning = false;
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
            if (IsGameplayScene && ActiveSlotId > 0)
                SaveCurrentGame(false);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            IsTransitioning = true;
            sessionClockRunning = false;
            Time.timeScale = 1f;

            LoadingScreenController.Instance.Show(sceneName);
            TransitionProgress?.Invoke(0f);

            yield return null;

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            if (operation == null)
            {
                IsTransitioning = false;
                ClearPendingTravel();
                LoadingScreenController.Instance.HideImmediate();
                ReportError($"Не удалось начать загрузку сцены «{sceneName}».");
                yield break;
            }

            operation.allowSceneActivation = true;

            while (!operation.isDone)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                TransitionProgress?.Invoke(progress);
                LoadingScreenController.Instance.SetProgress(progress);
                yield return null;
            }

            TransitionProgress?.Invoke(1f);
            LoadingScreenController.Instance.SetProgress(1f);

            yield return new WaitForSecondsRealtime(0.12f);

            LoadingScreenController.Instance.Hide();
            IsTransitioning = false;

            if (IsGameplayScene)
                sessionClockRunning = true;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == MainMenuSceneName)
            {
                ClearPendingTravel();
                sessionClockRunning = false;
                GameAudioService.Instance.PlayMenuAmbience();
                return;
            }

            if (scene.name == BootSceneName)
            {
                sessionClockRunning = false;
                return;
            }

            GameAudioService.Instance.StopMenuAmbience();

            SaveGameData current = ActiveSlotId > 0
                ? SaveGameService.LoadSlot(ActiveSlotId)
                : null;

            if (current != null)
            {
                sessionBasePlayTime = Mathf.Max(0f, current.playTimeSeconds);
                sessionElapsedPlayTime = 0f;
                GameplaySaveBridge.RestoreFrom(current);
            }

            bool explicitTravel =
                !string.IsNullOrWhiteSpace(pendingTravelSceneName) &&
                string.Equals(
                    pendingTravelSceneName,
                    scene.name,
                    StringComparison.Ordinal);

            if (explicitTravel)
            {
                string spawnId = pendingTravelSpawnId;
                string location = pendingTravelLocationName;
                ClearPendingTravel();

                if (!SceneSpawnPoint.TryPlaceParty(spawnId))
                {
                    Debug.LogWarning(
                        $"Spawn point «{spawnId}» was not found in scene «{scene.name}».");
                }

                if (ActiveSlotId > 0)
                {
                    SaveCurrentGame(
                        false,
                        string.IsNullOrWhiteSpace(location)
                            ? scene.name
                            : location);
                }
            }

            sessionClockRunning = !IsTransitioning;
        }

        private void ClearPendingTravel()
        {
            pendingTravelSceneName = null;
            pendingTravelSpawnId = null;
            pendingTravelLocationName = null;
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
