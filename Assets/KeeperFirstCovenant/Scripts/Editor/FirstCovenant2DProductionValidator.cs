#if UNITY_EDITOR
using System.Collections.Generic;
using KeeperFirstCovenant.Characters;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Visual;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    public static class FirstCovenant2DProductionValidator
    {
        private static readonly FacingDirection8[] AllDirections =
        {
            FacingDirection8.North,
            FacingDirection8.NorthEast,
            FacingDirection8.East,
            FacingDirection8.SouthEast,
            FacingDirection8.South,
            FacingDirection8.SouthWest,
            FacingDirection8.West,
            FacingDirection8.NorthWest
        };

        private static readonly string[] WorldPrefabIds =
        {
            "AncientStoneTile",
            "BrokenWall",
            "BrokenPillar",
            "AncientShrine",
            "RuneStone",
            "OldTree",
            "GrassClump",
            "Brazier",
            "StoneStairs",
            "Puddle",
            "CovenantBanner",
            "RockCluster"
        };

        [MenuItem("Keeper First Covenant/2D Production/Validate Generated Assets")]
        public static void ValidateFromMenu()
        {
            ValidateGeneratedAssets(false);
        }

        public static bool ValidateGeneratedAssets(bool throwOnFailure)
        {
            List<string> errors = new List<string>();

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                Edward2DProductionBuilder.EdwardPrefabPath);

            if (prefab == null)
            {
                errors.Add("Edward production prefab is missing.");
            }
            else
            {
                ValidatePrefab(prefab, errors);
            }

            ValidateEquipment(
                Edward2DProductionBuilder.DataRoot + "/Edward_TravelerCloak.asset",
                false,
                errors);
            ValidateEquipment(
                Edward2DProductionBuilder.DataRoot + "/Edward_LeatherArmor.asset",
                false,
                errors);
            ValidateEquipment(
                Edward2DProductionBuilder.DataRoot + "/edward_travel_sword.asset",
                true,
                errors);
            ValidateEquipment(
                Edward2DProductionBuilder.DataRoot + "/edward_greatsword.asset",
                true,
                errors);

            foreach (string id in WorldPrefabIds)
            {
                string path = FirstCovenant2DWorldBuilder.PrefabRoot + "/" + id + ".prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
                    errors.Add("World prefab missing: " + path);
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    FirstCovenant2DProductionBuilder.ScenePath) == null)
            {
                errors.Add("Production test scene is missing.");
            }

            if (errors.Count == 0)
            {
                Debug.Log(
                    "Keeper 2D validation passed: Edward rig, equipment, 8-direction sprites, " +
                    "world kit and production test scene are present.");
                return true;
            }

            string report = "Keeper 2D validation FAILED:\n - " + string.Join("\n - ", errors);
            Debug.LogError(report);

            if (throwOnFailure)
                throw new System.InvalidOperationException(report);

            return false;
        }

        private static void ValidatePrefab(GameObject prefab, List<string> errors)
        {
            PaperDollCharacterVisual visual = prefab.GetComponentInChildren<PaperDollCharacterVisual>(true);
            PaperDollMotionAnimator motion = prefab.GetComponentInChildren<PaperDollMotionAnimator>(true);

            if (visual == null)
                errors.Add("Edward prefab has no PaperDollCharacterVisual.");
            else
            {
                visual.RebuildLayerCache();

                if (visual.LayerCount < 20)
                    errors.Add("Edward rig has too few paper-doll layers: " + visual.LayerCount);

                if (!visual.HasWeaponRenderer || visual.WeaponSocket == null)
                    errors.Add("Edward weapon renderer/socket is not configured.");

                if (visual.BaseAppearance == null)
                    errors.Add("Edward base appearance is not assigned.");
                else
                    ValidateAppearance(visual.BaseAppearance, errors);
            }

            if (motion == null)
                errors.Add("Edward prefab has no PaperDollMotionAnimator.");

            if (prefab.GetComponent<CharacterController>() == null)
                errors.Add("Edward prefab has no CharacterController.");
            if (prefab.GetComponent<EdwardExplorationController>() == null)
                errors.Add("Edward prefab has no EdwardExplorationController.");
            if (prefab.GetComponent<EdwardVisualTestDriver>() == null)
                errors.Add("Edward prefab has no EdwardVisualTestDriver.");
            if (prefab.GetComponent<CombatantRuntime>() == null)
                errors.Add("Edward prefab has no CombatantRuntime.");
            if (prefab.GetComponent<PaperDollBloodVisual>() == null)
                errors.Add("Edward prefab has no PaperDollBloodVisual.");
            if (prefab.GetComponent<EdwardFireVisual>() == null)
                errors.Add("Edward prefab has no EdwardFireVisual.");
            if (prefab.GetComponent<CombatVisualBridge>() == null)
                errors.Add("Edward prefab has no CombatVisualBridge.");
        }

        private static void ValidateAppearance(
            PaperDollAppearanceDefinition appearance,
            List<string> errors)
        {
            if (appearance.slots == null || appearance.slots.Length == 0)
            {
                errors.Add("Base appearance has no sprite slots.");
                return;
            }

            foreach (PaperDollSlotSprites slot in appearance.slots)
            {
                if (slot == null)
                {
                    errors.Add("Base appearance contains a null slot.");
                    continue;
                }

                ValidateDirectionalSet(
                    "Base/" + slot.slot,
                    slot.sprites,
                    errors);
            }
        }

        private static void ValidateEquipment(
            string path,
            bool requireWeapon,
            List<string> errors)
        {
            EquipmentVisualDefinition equipment =
                AssetDatabase.LoadAssetAtPath<EquipmentVisualDefinition>(path);

            if (equipment == null)
            {
                errors.Add("Equipment asset missing: " + path);
                return;
            }

            if (requireWeapon)
            {
                if (!equipment.hasWeaponVisual)
                    errors.Add(equipment.displayName + " is missing its weapon visual flag.");
                ValidateDirectionalSet(equipment.displayName + "/Weapon", equipment.weaponSprites, errors);
            }

            if (equipment.slotOverrides == null)
                return;

            foreach (PaperDollSlotSprites slot in equipment.slotOverrides)
            {
                if (slot == null)
                {
                    errors.Add(equipment.displayName + " contains a null slot override.");
                    continue;
                }

                ValidateDirectionalSet(
                    equipment.displayName + "/" + slot.slot,
                    slot.sprites,
                    errors);
            }
        }

        private static void ValidateDirectionalSet(
            string name,
            DirectionalSpriteSet8 set,
            List<string> errors)
        {
            if (set == null)
            {
                errors.Add(name + " has no directional set.");
                return;
            }

            foreach (FacingDirection8 direction in AllDirections)
            {
                Sprite sprite = set.Get(direction, out bool ignoredFlip);
                if (sprite == null)
                    errors.Add(name + " is missing a resolvable sprite for " + direction + ".");
            }
        }
    }
}
#endif
