#if UNITY_EDITOR
using KeeperFirstCovenant.Characters;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Visual;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    public static class HighResEdwardPrefabBuilder
    {
        public const string PrefabPath =
            "Assets/KeeperFirstCovenant/Prefabs/Characters/Edward_HighRes2D.prefab";

        private const string CharacterDataPath =
            "Assets/KeeperFirstCovenant/Data/Characters/Edward.asset";

        [MenuItem("Keeper First Covenant/High-Res 2D/Build Edward Prefab")]
        public static void BuildEdwardPrefab()
        {
            EnsureFolders();

            FrameAnimationLibrary baseLibrary =
                AssetDatabase.LoadAssetAtPath<FrameAnimationLibrary>(
                    FrameAnimationLibraryBuilder.EdwardBaseLibraryPath);

            if (baseLibrary == null)
            {
                FrameAnimationLibraryBuilder.RebuildEdwardBaseLibrary();
                baseLibrary =
                    AssetDatabase.LoadAssetAtPath<FrameAnimationLibrary>(
                        FrameAnimationLibraryBuilder.EdwardBaseLibraryPath);
            }

            CharacterDefinition definition = GetEdwardDefinition();

            GameObject root = new GameObject("Edward_HighRes2D");

            CharacterController controller =
                root.AddComponent<CharacterController>();

            controller.height = 1.85f;
            controller.radius = 0.28f;
            controller.center = new Vector3(0f, 0.92f, 0f);
            controller.stepOffset = 0.25f;
            controller.skinWidth = 0.035f;

            CombatantRuntime combatant =
                root.AddComponent<CombatantRuntime>();

            combatant.SetDefinition(definition);

            root.AddComponent<HighResExplorationController>();
            root.AddComponent<HighResCharacterCombatBridge>();
            root.AddComponent<EdwardHighResTestDriver>();

            GameObject billboard = new GameObject("BillboardRoot");
            billboard.transform.SetParent(root.transform, false);
            billboard.transform.localPosition = new Vector3(0f, 0.98f, 0f);
            billboard.AddComponent<BillboardCharacter2D>();

            HighResFrameCharacter2D animation =
                billboard.AddComponent<HighResFrameCharacter2D>();

            SpriteRenderer baseRenderer =
                CreateLayer(billboard.transform, "Base", 10);

            SpriteRenderer armorRenderer =
                CreateLayer(billboard.transform, "Armor", 20);

            SpriteRenderer cloakRenderer =
                CreateLayer(billboard.transform, "Cloak", 30);

            SpriteRenderer weaponRenderer =
                CreateLayer(billboard.transform, "Weapon", 40);

            SpriteRenderer headgearRenderer =
                CreateLayer(billboard.transform, "Headgear", 50);

            SpriteRenderer accessoryRenderer =
                CreateLayer(billboard.transform, "Accessory", 60);

            animation.Configure(
                baseLibrary,
                baseRenderer,
                armorRenderer,
                cloakRenderer,
                weaponRenderer,
                headgearRenderer,
                accessoryRenderer);

            GameObject shadow = new GameObject("GroundShadow");
            shadow.transform.SetParent(root.transform, false);
            shadow.transform.localPosition = new Vector3(0f, 0.015f, 0f);
            shadow.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            SpriteRenderer shadowRenderer =
                shadow.AddComponent<SpriteRenderer>();

            shadowRenderer.sortingOrder = 1;
            shadowRenderer.color = new Color(0f, 0f, 0f, 0.28f);

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Built Edward high-resolution 2D prefab: " +
                PrefabPath);
        }

        private static SpriteRenderer CreateLayer(
            Transform parent,
            string name,
            int sortingOrder)
        {
            GameObject layer = new GameObject(name);
            layer.transform.SetParent(parent, false);

            SpriteRenderer renderer =
                layer.AddComponent<SpriteRenderer>();

            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private static CharacterDefinition GetEdwardDefinition()
        {
            CharacterDefinition definition =
                AssetDatabase.LoadAssetAtPath<CharacterDefinition>(
                    CharacterDataPath);

            if (definition == null)
            {
                definition =
                    ScriptableObject.CreateInstance<CharacterDefinition>();

                AssetDatabase.CreateAsset(
                    definition,
                    CharacterDataPath);
            }

            definition.characterId = "edward";
            definition.displayName = "Edward";
            definition.faction = CombatFaction.Player;
            definition.maxHealth = 100;
            definition.maxMana = 50;
            definition.strength = 13;
            definition.finesse = 12;
            definition.intellect = 12;
            definition.willpower = 11;
            definition.perception = 12;
            definition.actionPoints = 2;
            definition.movementMeters = 9f;

            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void EnsureFolders()
        {
            EnsureFolder(
                "Assets/KeeperFirstCovenant",
                "Prefabs");

            EnsureFolder(
                "Assets/KeeperFirstCovenant/Prefabs",
                "Characters");

            EnsureFolder(
                "Assets/KeeperFirstCovenant",
                "Data");

            EnsureFolder(
                "Assets/KeeperFirstCovenant/Data",
                "Characters");
        }

        private static void EnsureFolder(
            string parent,
            string child)
        {
            string path = parent + "/" + child;

            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
#endif
