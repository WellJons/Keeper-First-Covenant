using KeeperFirstCovenant.Core;
using KeeperFirstCovenant.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KeeperFirstCovenant.UI
{
    public sealed class InGameMenuInstaller : MonoBehaviour
    {
        private static InGameMenuInstaller instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null)
                return;

            var root = new GameObject("Keeper_InGameMenuInstaller");
            instance = root.AddComponent<InGameMenuInstaller>();
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

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == GameFlowController.BootSceneName ||
                scene.name == GameFlowController.MainMenuSceneName)
            {
                return;
            }

            InGamePauseController existing = FindFirstObjectByType<InGamePauseController>();
            if (existing != null)
                return;

            var root = new GameObject("Keeper_InGamePauseMenu");
            SceneManager.MoveGameObjectToScene(root, scene);
            root.AddComponent<CombatMechanicsRuntimeInstaller>();
            root.AddComponent<InGamePauseController>();
            root.AddComponent<GameplayHudController>();
            root.AddComponent<BreakGaugeHud>();
            root.AddComponent<InteractionPromptHud>();
            root.AddComponent<WorldTargetFrameHud>();
            root.AddComponent<ExplorationFocusHud>();
            root.AddComponent<StealthAwarenessHud>();
            root.AddComponent<LootToastController>();
            root.AddComponent<DiscoveryToastController>();
            root.AddComponent<RelationshipToastController>();
            root.AddComponent<QuestToastController>();
            root.AddComponent<QuestTrackerHud>();
            root.AddComponent<DialogueUiController>();
            root.AddComponent<InspectionPanelController>();
            root.AddComponent<DefeatScreenController>();
        }
    }
}
