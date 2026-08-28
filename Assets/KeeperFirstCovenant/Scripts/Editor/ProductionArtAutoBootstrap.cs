#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    [InitializeOnLoad]
    public static class ProductionArtAutoBootstrap
    {
        private const string SessionKey =
            "KeeperFirstCovenant.ProductionArt.AutoBuildAttempted";

        static ProductionArtAutoBootstrap()
        {
            EditorApplication.delayCall += TryBuildOnce;
        }

        private static void TryBuildOnce()
        {
            if (SessionState.GetBool(SessionKey, false))
                return;

            SessionState.SetBool(SessionKey, true);

            if (!SourcesPresent())
            {
                Debug.LogWarning(
                    "Keeper production-art bootstrap skipped: one or more source sheets are missing.");
                return;
            }

            bool characterMissing =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    ProductionSheetCharacterBuilder.EdwardPrefabPath) == null;

            bool worldMissing =
                ProductionSheetWorldBuilder.LoadPrefab(
                    "StoneFloor_A") == null;

            bool sceneMissing =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    ProductionArtGameBuilder.ScenePath) == null;

            if (!characterMissing &&
                !worldMissing &&
                !sceneMissing)
            {
                return;
            }

            try
            {
                Debug.Log(
                    "Keeper: building approved production art into playable Unity assets...");

                ProductionArtGameBuilder.BuildEverything();

                Debug.Log(
                    "Keeper: production-art characters, world prefabs and playable scene are ready.");
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "Keeper production-art auto build failed. " +
                    "Use 'Keeper First Covenant/Production Art/BUILD EVERYTHING INTO GAME' " +
                    "after fixing the reported issue.\n" +
                    exception);
            }
        }

        private static bool SourcesPresent()
        {
            string root =
                ProductionSheetCharacterBuilder.SheetRoot;

            return AssetDatabase.LoadAssetAtPath<Texture2D>(
                       root + "/Edward_ProductionSheet.jpg") != null &&
                   AssetDatabase.LoadAssetAtPath<Texture2D>(
                       root + "/Eleanor_ProductionSheet.jpg") != null &&
                   AssetDatabase.LoadAssetAtPath<Texture2D>(
                       root + "/Aelis_ProductionSheet.jpg") != null &&
                   AssetDatabase.LoadAssetAtPath<Texture2D>(
                       "Assets/KeeperFirstCovenant/Art/ProductionSheets/WorldKit_ProductionSheet.jpg") != null;
        }
    }
}
#endif
