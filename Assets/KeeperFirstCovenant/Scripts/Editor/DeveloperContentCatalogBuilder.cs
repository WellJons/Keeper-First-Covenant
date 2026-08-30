#if UNITY_EDITOR
using System.Linq;
using KeeperFirstCovenant.Characters;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Developer;
using KeeperFirstCovenant.Inventory;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    public static class DeveloperContentCatalogBuilder
    {
        public static DeveloperContentCatalog BuildOn(
            GameObject host)
        {
            if (host == null)
                return null;

            DeveloperContentCatalog catalog =
                host.GetComponent<
                    DeveloperContentCatalog>();

            if (catalog == null)
            {
                catalog =
                    Undo.AddComponent<
                        DeveloperContentCatalog>(
                            host);
            }

            CharacterDefinition[] characters =
                FindAssets<CharacterDefinition>(
                    "t:CharacterDefinition");

            ItemDefinition[] items =
                FindAssets<ItemDefinition>(
                        "t:ItemDefinition")
                    .Concat(
                        FindAssets<WeaponDefinition>(
                            "t:WeaponDefinition"))
                    .Concat(
                        FindAssets<ArmorDefinition>(
                            "t:ArmorDefinition"))
                    .Where(x => x != null)
                    .Distinct()
                    .OrderBy(x =>
                        x.displayName)
                    .ToArray();

            CombatActionDefinition[] actions =
                FindAssets<CombatActionDefinition>(
                        "t:CombatActionDefinition")
                    .OrderBy(x =>
                        x.displayName)
                    .ToArray();

            catalog.Configure(
                characters
                    .OrderBy(x =>
                        x.displayName)
                    .ToArray(),
                items,
                actions);

            EditorUtility.SetDirty(catalog);

            Debug.Log(
                $"Developer catalog rebuilt: " +
                $"{characters.Length} characters, " +
                $"{items.Length} items, " +
                $"{actions.Length} abilities.");

            return catalog;
        }

        [MenuItem(
            "Keeper First Covenant/" +
            "Rebuild Developer F1 Catalog")]
        public static void RebuildCurrentScene()
        {
            TurnCombatDirector director =
                Object.FindAnyObjectByType<
                    TurnCombatDirector>();

            if (director == null)
            {
                Debug.LogError(
                    "No GameSystems/TurnCombatDirector " +
                    "found in the open scene.");
                return;
            }

            BuildOn(director.gameObject);
            EditorUtility.SetDirty(
                director.gameObject);
        }

        private static T[] FindAssets<T>(
            string filter)
            where T : Object
        {
            return AssetDatabase
                .FindAssets(filter)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(
                    AssetDatabase
                        .LoadAssetAtPath<T>)
                .Where(x => x != null)
                .ToArray();
        }
    }
}
#endif
