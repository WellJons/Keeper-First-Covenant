#if UNITY_EDITOR
using KeeperFirstCovenant.AI;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Developer;
using KeeperFirstCovenant.Inventory;
using KeeperFirstCovenant.Player;
using KeeperFirstCovenant.UI;
using KeeperFirstCovenant.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    public static class
        PrototypeCombatLoopInstaller
    {
        private const string LootPath =
            "Assets/KeeperFirstCovenant/" +
            "Generated/Data/" +
            "RoadsideLoot.asset";

        [MenuItem(
            "Keeper First Covenant/" +
            "Install Combat Loop In Open Scene")]
        public static void Install()
        {
            TurnCombatDirector director =
                Object.FindFirstObjectByType<
                    TurnCombatDirector>();

            if (director == null)
            {
                Debug.LogError(
                    "No TurnCombatDirector found. " +
                    "Build the prototype road " +
                    "scene first.");
                return;
            }

            TacticalGrid3D grid =
                Object.FindFirstObjectByType<
                    TacticalGrid3D>();

            if (grid == null)
            {
                Debug.LogError(
                    "No TacticalGrid3D found.");
                return;
            }

            PrototypeTacticalContentV2.Apply();
            PrototypeDeveloperTestContent.Build();
            PrototypeCombatPresentationContent.Build();

            GameObject systems =
                director.gameObject;

            DeveloperContentCatalogBuilder.BuildOn(
                systems);

            AddIfMissing<
                DeveloperMenu>(systems);

            AddIfMissing<
                CombatLogService>(systems);

            AddIfMissing<
                CombatPresentationDirector>(
                    systems);

            CombatStartOnPlay autoStart =
                AddIfMissing<
                    CombatStartOnPlay>(systems);

            autoStart.Configure(false);

            AddIfMissing<
                TacticalPlayerController>(
                    systems);

            AddIfMissing<
                EnemyTurnBrain>(systems);

            AddIfMissing<
                WorldInteractionController>(
                    systems);

            AddIfMissing<
                ExplorationMovementController>(
                    systems);

            AddIfMissing<
                CombatDebugHUD>(systems);

            AddIfMissing<
                TacticalLineOfSight>(systems);

            AddIfMissing<
                ElementalSurfaceSystem>(
                    systems);

            AddIfMissing<
                OpportunityAttackSystem>(
                    systems);

            AddIfMissing<
                ForcedMovementSystem>(
                    systems);

            RemoveIfPresent<
                TacticalTargetingIndicator>(
                    systems);

            RemoveIfPresent<
                MovementPathIndicator>(
                    systems);

            LineRenderer legacyLine =
                systems.GetComponent<
                    LineRenderer>();

            if (legacyLine != null)
            {
                Undo.DestroyObjectImmediate(
                    legacyLine);
            }

            GameObject targetingPreview =
                GetOrCreateChild(
                    systems,
                    "TargetingPreview");

            AddIfMissing<
                TacticalTargetingIndicator>(
                    targetingPreview);

            GameObject movementPreview =
                GetOrCreateChild(
                    systems,
                    "MovementPathPreview");

            AddIfMissing<
                MovementPathIndicator>(
                    movementPreview);

            LootTableDefinition loot =
                AssetDatabase
                    .LoadAssetAtPath<
                        LootTableDefinition>(
                        LootPath);

            CombatantRuntime[] combatants =
                Object.FindObjectsByType<
                    CombatantRuntime>(
                    FindObjectsSortMode.None);

            foreach (CombatantRuntime combatant
                     in combatants)
            {
                AddIfMissing<
                    CombatAutoRegister>(
                        combatant.gameObject);

                AddIfMissing<
                    TacticalUnitMover>(
                        combatant.gameObject);

                AddIfMissing<
                    EquipmentComponent>(
                        combatant.gameObject);

                if (combatant.Definition != null &&
                    combatant.Definition.characterId ==
                        "edward")
                {
                    AddIfMissing<
                        ArcaneStrainComponent>(
                            combatant.gameObject);
                }

                if (combatant.Faction ==
                    CombatFaction.Enemy)
                {
                    CorpseLootOnDeath corpse =
                        AddIfMissing<
                            CorpseLootOnDeath>(
                                combatant
                                    .gameObject);

                    if (loot != null)
                    {
                        corpse.Configure(
                            loot,
                            "Search body");
                    }
                }
            }

            EditorSceneManager
                .MarkSceneDirty(
                    EditorSceneManager
                        .GetActiveScene());

            EditorSceneManager
                .SaveOpenScenes();

            Debug.Log(
                "Keeper tactical combat installed. " +
                "Includes party control, free movement, " +
                "LOS, cover, height, " +
                "AoE preview, surfaces and reactions. " +
                "F1 opens the developer sandbox.");
        }

        private static GameObject GetOrCreateChild(
            GameObject parent,
            string childName)
        {
            Transform existing =
                parent.transform.Find(
                    childName);

            if (existing != null)
                return existing.gameObject;

            GameObject child =
                new GameObject(childName);

            Undo.RegisterCreatedObjectUndo(
                child,
                "Create " + childName);

            child.transform.SetParent(
                parent.transform,
                false);

            return child;
        }

        private static void RemoveIfPresent<T>(
            GameObject go)
            where T : Component
        {
            T component =
                go.GetComponent<T>();

            if (component != null)
            {
                Undo.DestroyObjectImmediate(
                    component);
            }
        }

        private static T AddIfMissing<T>(
            GameObject go)
            where T : Component
        {
            T component =
                go.GetComponent<T>();

            if (component == null)
            {
                component =
                    Undo.AddComponent<T>(go);
            }

            EditorUtility.SetDirty(
                component);

            return component;
        }
    }
}
#endif
