#if UNITY_EDITOR
using System.IO;
using KeeperFirstCovenant.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KeeperFirstCovenant.EditorTools
{
    [InitializeOnLoad]
    public static class MainMenuSceneBuilder
    {
        private const string SceneRoot = "Assets/KeeperFirstCovenant/Scenes";
        private const string BootScenePath = SceneRoot + "/Boot.unity";
        private const string MainMenuScenePath = SceneRoot + "/MainMenu.unity";
        private const string PrototypeScenePath = SceneRoot + "/Prototype_Road.unity";
        private const string SessionGuard = "KeeperFirstCovenant.MainMenuShell.AutoBuilt";

        static MainMenuSceneBuilder()
        {
            EditorApplication.delayCall += AutoBuildIfNeeded;
        }

        [MenuItem("Keeper First Covenant/Build Main Menu Shell")]
        public static void BuildAll()
        {
            BuildInternal(false);
        }

        private static void AutoBuildIfNeeded()
        {
            if (Application.isBatchMode ||
                EditorApplication.isCompiling ||
                EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (SessionState.GetBool(SessionGuard, false))
                return;

            bool missing = !File.Exists(BootScenePath) ||
                           !File.Exists(MainMenuScenePath) ||
                           !File.Exists(PrototypeScenePath);

            if (!missing)
            {
                EnsureBuildSettings();
                SessionState.SetBool(SessionGuard, true);
                return;
            }

            SessionState.SetBool(SessionGuard, true);
            BuildInternal(true);
        }

        private static void BuildInternal(bool automatic)
        {
            EnsureFolder("Assets", "KeeperFirstCovenant");
            EnsureFolder("Assets/KeeperFirstCovenant", "Scenes");
            EnsureFolder("Assets/KeeperFirstCovenant", "Art");
            EnsureFolder("Assets/KeeperFirstCovenant/Art", "Menu");
            EnsureFolder("Assets/KeeperFirstCovenant", "Audio");
            EnsureFolder("Assets/KeeperFirstCovenant/Audio", "Music");
            EnsureFolder("Assets/KeeperFirstCovenant/Audio", "UI");

            if (!File.Exists(PrototypeScenePath))
                FirstCovenantPrototypeBuilder.Build();

            BuildBootScene();
            BuildMainMenuScene();
            EnsureBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!automatic)
                EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);

            Debug.Log(
                "Keeper: First Covenant main menu shell is ready. " +
                "Boot, MainMenu and Prototype_Road are registered in Build Settings.");
        }

        private static void BuildBootScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject root = new GameObject("Keeper_Boot");
            root.AddComponent<StartupSplashController>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, BootScenePath);
        }

        private static void BuildMainMenuScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject root = new GameObject("Keeper_MainMenu");
            root.AddComponent<MainMenuController>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        }

        private static void EnsureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootScenePath, true),
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(PrototypeScenePath, true)
            };

            PlayerSettings.productName = "Keeper: First Covenant";
            PlayerSettings.companyName = "WellJons";
            PlayerSettings.runInBackground = true;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
#endif
