#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    [InitializeOnLoad]
    public static class ProductionWorldPack01AutoBootstrap
    {
        private const string SessionKey =
            "KeeperFirstCovenant.ProductionWorldPack01.AutoBuildAttempted";

        static ProductionWorldPack01AutoBootstrap()
        {
            EditorApplication.delayCall += TryBuildOnce;
        }

        private static void TryBuildOnce()
        {
            if (SessionState.GetBool(SessionKey, false))
                return;

            SessionState.SetBool(SessionKey, true);

            if (!ProductionWorldPack01Builder.SourcesPresent())
                return;

            if (ProductionWorldPack01Builder.PrefabsPresent() &&
                AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    ProductionWorldPack01Builder.ScenePath) != null)
            {
                return;
            }

            try
            {
                ProductionWorldPack01Builder.BuildAll();

                Debug.Log(
                    "Keeper: Production World Pack 01 auto-integrated into Unity.");
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "Keeper Production World Pack 01 auto-build failed. " +
                    "Run 'Keeper First Covenant/Production Art/World Pack 01/BUILD PACK'.\n" +
                    exception);
            }
        }
    }
}
#endif
