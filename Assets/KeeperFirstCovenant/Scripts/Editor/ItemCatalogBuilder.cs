#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using KeeperFirstCovenant.Inventory;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    public static class ItemCatalogBuilder
    {
        private const string ResourcesRoot =
            "Assets/KeeperFirstCovenant/Resources";

        private const string CatalogPath =
            ResourcesRoot + "/ItemCatalog.asset";

        [MenuItem(
            "Keeper First Covenant/Rebuild Item Catalog")]
        public static void Rebuild()
        {
            EnsureResourcesFolder();

            ItemCatalogDefinition catalog =
                AssetDatabase.LoadAssetAtPath<
                    ItemCatalogDefinition>(
                    CatalogPath);

            if (catalog == null)
            {
                catalog =
                    ScriptableObject.CreateInstance<
                        ItemCatalogDefinition>();

                AssetDatabase.CreateAsset(
                    catalog,
                    CatalogPath);
            }

            var guids =
                new HashSet<string>();

            AddGuids(
                guids,
                AssetDatabase.FindAssets(
                    "t:ItemDefinition",
                    new[]
                    {
                        "Assets/KeeperFirstCovenant"
                    }));

            AddGuids(
                guids,
                AssetDatabase.FindAssets(
                    "t:WeaponDefinition",
                    new[]
                    {
                        "Assets/KeeperFirstCovenant"
                    }));

            AddGuids(
                guids,
                AssetDatabase.FindAssets(
                    "t:ArmorDefinition",
                    new[]
                    {
                        "Assets/KeeperFirstCovenant"
                    }));

            ItemDefinition[] items =
                guids
                    .Select(
                        AssetDatabase.GUIDToAssetPath)
                    .Select(path =>
                        AssetDatabase
                            .LoadAssetAtPath<
                                ItemDefinition>(
                                path))
                    .Where(item =>
                        item != null &&
                        !string.IsNullOrWhiteSpace(
                            item.itemId))
                    .Distinct()
                    .OrderBy(item =>
                        item.itemId)
                    .ToArray();

            catalog.SetItems(items);

            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();

            ItemCatalogService.Invalidate();

            Debug.Log(
                "Keeper item catalog rebuilt: " +
                items.Length +
                " item definitions.");
        }

        private static void AddGuids(
            HashSet<string> target,
            IEnumerable<string> values)
        {
            foreach (string guid in values)
                target.Add(guid);
        }

        private static void EnsureResourcesFolder()
        {
            if (!AssetDatabase.IsValidFolder(
                    ResourcesRoot))
            {
                AssetDatabase.CreateFolder(
                    "Assets/KeeperFirstCovenant",
                    "Resources");
            }
        }
    }
}
#endif
