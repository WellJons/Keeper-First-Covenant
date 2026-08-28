#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    [InitializeOnLoad]
    public static class EnvironmentPack03AutoBootstrap
    {
        private const string SessionKey =
            "KeeperFirstCovenant.EnvironmentPack03.AutoBuildAttempted";

        static EnvironmentPack03AutoBootstrap()
        {
            EditorApplication.delayCall += TryBuildOnce;
        }

        private static void TryBuildOnce()
        {
            if (SessionState.GetBool(SessionKey, false))
                return;

            SessionState.SetBool(SessionKey, true);

            if (!EnvironmentPack03Builder.SourcesPresent())
                return;

            bool sceneMissing =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    EnvironmentPack03Builder.ScenePath) == null;

            if (EnvironmentPack03Builder.PrefabsPresent() &&
                !sceneMissing)
            {
                return;
            }

            try
            {
                EnvironmentPack03Builder.BuildAll();

                Debug.Log(
                    "Keeper: Environment Pack 03 auto-integrated into the project.");
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "Environment Pack 03 auto-build failed. " +
                    "Run 'Keeper First Covenant/Production Art/Environment Pack 03/BUILD EVERYTHING'.\n" +
                    exception);
            }
        }
    }
}
#endif
