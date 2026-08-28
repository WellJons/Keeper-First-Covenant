#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    [InitializeOnLoad]
    public static class EnvironmentPack02AutoBootstrap
    {
        private const string SessionKey =
            "KeeperFirstCovenant.EnvironmentPack02.AutoBuildAttempted";

        static EnvironmentPack02AutoBootstrap()
        {
            EditorApplication.delayCall += TryBuildOnce;
        }

        private static void TryBuildOnce()
        {
            if (SessionState.GetBool(SessionKey, false))
                return;

            SessionState.SetBool(SessionKey, true);

            if (!EnvironmentPack02Builder.SourcesPresent())
                return;

            bool sceneMissing =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    EnvironmentPack02Builder.ScenePath) == null;

            if (EnvironmentPack02Builder.PrefabsPresent() && !sceneMissing)
                return;

            try
            {
                EnvironmentPack02Builder.BuildAll();
                Debug.Log(
                    "Keeper: Environment Pack 02 auto-integrated. " +
                    "Animated foliage and tree prefabs are ready.");
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "Environment Pack 02 auto-build failed. " +
                    "Run 'Keeper First Covenant/Production Art/Environment Pack 02/BUILD EVERYTHING'.\n" +
                    exception);
            }
        }
    }
}
#endif
