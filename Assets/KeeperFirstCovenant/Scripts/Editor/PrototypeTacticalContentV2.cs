#if UNITY_EDITOR
using KeeperFirstCovenant.Characters;
using KeeperFirstCovenant.Combat;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    public static class PrototypeTacticalContentV2
    {
        private const string DataRoot =
            "Assets/KeeperFirstCovenant/" +
            "Generated/Data";

        [MenuItem(
            "Keeper First Covenant/" +
            "Upgrade Prototype Tactical Abilities")]
        public static void Apply()
        {
            if (!AssetDatabase.IsValidFolder(
                    "Assets/KeeperFirstCovenant/" +
                    "Generated"))
            {
                Debug.LogError(
                    "Build the prototype road scene " +
                    "before upgrading tactical abilities.");
                return;
            }

            CharacterDefinition edward =
                AssetDatabase.LoadAssetAtPath<
                    CharacterDefinition>(
                    DataRoot +
                    "/Character_edward.asset");

            CharacterDefinition lucian =
                AssetDatabase.LoadAssetAtPath<
                    CharacterDefinition>(
                    DataRoot +
                    "/Character_lucian.asset");

            CombatActionDefinition swordSlash =
                AssetDatabase.LoadAssetAtPath<
                    CombatActionDefinition>(
                    DataRoot +
                    "/Action_SwordSlash.asset");

            CombatActionDefinition sealBolt =
                AssetDatabase.LoadAssetAtPath<
                    CombatActionDefinition>(
                    DataRoot +
                    "/Action_SealBolt.asset");

            if (edward == null ||
                swordSlash == null)
            {
                Debug.LogError(
                    "Generated prototype character " +
                    "data was not found. Build the " +
                    "prototype road scene first.");
                return;
            }

            ConfigureMelee(swordSlash);

            CombatActionDefinition shove =
                GetOrCreateAction(
                    "Shove",
                    "Shove");

            shove.category =
                CombatActionCategory.Control;
            shove.targetKind =
                TargetKind.Enemy;
            shove.actionPointCost = 1;
            shove.manaCost = 0;
            shove.rangeMeters = 1.8f;
            shove.areaRadius = 0f;
            shove.areaTargetRule =
                AreaTargetRule.PrimaryOnly;
            shove.requiresLineOfSight = true;
            shove.ignoresCover = true;
            shove.usesHeightAdvantage = false;
            shove.requiresAttackRoll = true;
            shove.baseHitChance = 75;
            shove.damage =
                new DiceFormula(0, 2, 0);
            shove.healing =
                new DiceFormula(0, 2, 0);
            shove.barrier =
                new DiceFormula(0, 2, 0);
            shove.damageType =
                DamageType.Physical;
            shove.scalingAttribute =
                AbilityAttribute.Strength;
            shove.scalingMultiplier = 0f;
            shove.pushDistanceMeters = 3f;
            shove.pushAwayFromActor = true;
            shove.createsSurface =
                SurfaceType.None;
            EditorUtility.SetDirty(shove);

            CombatActionDefinition fireBurst =
                GetOrCreateAction(
                    "FireBurst",
                    "Fire Burst");

            fireBurst.category =
                CombatActionCategory.Spell;

            fireBurst.targetKind =
                TargetKind.Ground;

            fireBurst.actionPointCost = 1;
            fireBurst.manaCost = 4;
            fireBurst.rangeMeters = 9f;
            fireBurst.areaRadius = 2.4f;

            fireBurst.areaTargetRule =
                AreaTargetRule.EnemiesOnly;

            fireBurst.requiresLineOfSight = true;
            fireBurst.ignoresCover = true;
            fireBurst.usesHeightAdvantage = false;
            fireBurst.requiresAttackRoll = false;

            fireBurst.damage =
                new DiceFormula(2, 6);

            fireBurst.damageType =
                DamageType.Fire;

            fireBurst.scalingAttribute =
                AbilityAttribute.Intellect;

            fireBurst.scalingMultiplier = 0.5f;

            fireBurst.createsSurface =
                SurfaceType.Fire;

            fireBurst.surfaceRadius = 2.4f;
            fireBurst.surfaceDurationTurns = 2;

            EditorUtility.SetDirty(fireBurst);

            CombatActionDefinition lightningArc =
                GetOrCreateAction(
                    "LightningArc",
                    "Lightning Arc");

            lightningArc.category =
                CombatActionCategory.Spell;

            lightningArc.targetKind =
                TargetKind.Enemy;

            lightningArc.actionPointCost = 1;
            lightningArc.manaCost = 3;
            lightningArc.rangeMeters = 11f;
            lightningArc.areaRadius = 0f;

            lightningArc.areaTargetRule =
                AreaTargetRule.PrimaryOnly;

            lightningArc.requiresLineOfSight = true;
            lightningArc.ignoresCover = false;
            lightningArc.usesHeightAdvantage = true;
            lightningArc.requiresAttackRoll = true;
            lightningArc.baseHitChance = 80;

            lightningArc.damage =
                new DiceFormula(2, 6);

            lightningArc.damageType =
                DamageType.Lightning;

            lightningArc.scalingAttribute =
                AbilityAttribute.Intellect;

            lightningArc.scalingMultiplier = 1f;

            lightningArc.createsSurface =
                SurfaceType.Electrified;

            lightningArc.surfaceRadius = 1.5f;
            lightningArc.surfaceDurationTurns = 1;

            EditorUtility.SetDirty(lightningArc);

            CombatActionDefinition frostField =
                GetOrCreateAction(
                    "FrostField",
                    "Frost Field");

            frostField.category =
                CombatActionCategory.Control;

            frostField.targetKind =
                TargetKind.Ground;

            frostField.actionPointCost = 1;
            frostField.manaCost = 3;
            frostField.rangeMeters = 10f;
            frostField.areaRadius = 2.6f;

            frostField.areaTargetRule =
                AreaTargetRule.EnemiesOnly;

            frostField.requiresLineOfSight = true;
            frostField.ignoresCover = true;
            frostField.usesHeightAdvantage = false;
            frostField.requiresAttackRoll = false;

            frostField.damage =
                new DiceFormula(1, 4);

            frostField.damageType =
                DamageType.Frost;

            frostField.scalingAttribute =
                AbilityAttribute.Intellect;

            frostField.scalingMultiplier = 0.4f;

            frostField.createsSurface =
                SurfaceType.Ice;

            frostField.surfaceRadius = 2.6f;
            frostField.surfaceDurationTurns = 2;

            EditorUtility.SetDirty(frostField);

            CombatActionDefinition waterRune =
                GetOrCreateAction(
                    "WaterRune",
                    "Water Rune");

            waterRune.category =
                CombatActionCategory.Control;

            waterRune.targetKind =
                TargetKind.Ground;

            waterRune.actionPointCost = 1;
            waterRune.manaCost = 2;
            waterRune.rangeMeters = 10f;
            waterRune.areaRadius = 0f;

            waterRune.areaTargetRule =
                AreaTargetRule.PrimaryOnly;

            waterRune.requiresLineOfSight = true;
            waterRune.ignoresCover = true;
            waterRune.usesHeightAdvantage = false;
            waterRune.requiresAttackRoll = false;

            waterRune.damage =
                new DiceFormula(0, 2, 0);

            waterRune.damageType =
                DamageType.Frost;

            waterRune.scalingAttribute =
                AbilityAttribute.None;

            waterRune.scalingMultiplier = 0f;

            waterRune.createsSurface =
                SurfaceType.Water;

            waterRune.surfaceRadius = 2.5f;
            waterRune.surfaceDurationTurns = 3;

            EditorUtility.SetDirty(waterRune);

            if (sealBolt != null)
            {
                sealBolt.requiresLineOfSight = true;
                sealBolt.ignoresCover = false;
                sealBolt.usesHeightAdvantage = true;
                EditorUtility.SetDirty(sealBolt);
            }

            CombatActionDefinition healingLight =
                GetOrCreateAction(
                    "HealingLight",
                    "Healing Light");

            healingLight.category =
                CombatActionCategory.Support;
            healingLight.targetKind =
                TargetKind.Ally;
            healingLight.actionPointCost = 1;
            healingLight.manaCost = 3;
            healingLight.rangeMeters = 9f;
            healingLight.areaRadius = 0f;
            healingLight.areaTargetRule =
                AreaTargetRule.PrimaryOnly;
            healingLight.requiresLineOfSight = true;
            healingLight.ignoresCover = true;
            healingLight.usesHeightAdvantage = false;
            healingLight.requiresAttackRoll = false;
            healingLight.damage =
                new DiceFormula(0, 2, 0);
            healingLight.healing =
                new DiceFormula(2, 6, 2);
            healingLight.barrier =
                new DiceFormula(0, 2, 0);
            healingLight.scalingAttribute =
                AbilityAttribute.Willpower;
            healingLight.scalingMultiplier = 0.8f;
            healingLight.createsSurface =
                SurfaceType.None;
            EditorUtility.SetDirty(healingLight);

            CombatActionDefinition silverBarrier =
                GetOrCreateAction(
                    "SilverBarrier",
                    "Silver Barrier");

            silverBarrier.category =
                CombatActionCategory.Support;
            silverBarrier.targetKind =
                TargetKind.Ally;
            silverBarrier.actionPointCost = 1;
            silverBarrier.manaCost = 4;
            silverBarrier.rangeMeters = 8f;
            silverBarrier.areaRadius = 0f;
            silverBarrier.areaTargetRule =
                AreaTargetRule.PrimaryOnly;
            silverBarrier.requiresLineOfSight = true;
            silverBarrier.ignoresCover = true;
            silverBarrier.usesHeightAdvantage = false;
            silverBarrier.requiresAttackRoll = false;
            silverBarrier.damage =
                new DiceFormula(0, 2, 0);
            silverBarrier.healing =
                new DiceFormula(0, 2, 0);
            silverBarrier.barrier =
                new DiceFormula(2, 6, 3);
            silverBarrier.scalingAttribute =
                AbilityAttribute.Willpower;
            silverBarrier.scalingMultiplier = 0.6f;
            silverBarrier.createsSurface =
                SurfaceType.None;
            EditorUtility.SetDirty(silverBarrier);

            CharacterDefinition aelis =
                GetOrCreateCharacter(
                    "aelis",
                    "Aelis");

            aelis.faction =
                CombatFaction.Ally;
            aelis.maxHealth = 40;
            aelis.maxMana = 38;
            aelis.armor = 0;
            aelis.magicGuard = 2;
            aelis.actionPoints = 2;
            aelis.movementMeters = 9f;
            aelis.strength = 8;
            aelis.finesse = 10;
            aelis.intellect = 14;
            aelis.willpower = 16;
            aelis.perception = 13;
            aelis.downedRounds = 3;
            aelis.startingActions =
                new[]
                {
                    healingLight,
                    silverBarrier
                };
            EditorUtility.SetDirty(aelis);

            edward.startingActions =
                new[]
                {
                    swordSlash,
                    shove,
                    fireBurst
                };

            if (lucian != null)
            {
                lucian.startingActions =
                    sealBolt != null
                        ? new[]
                        {
                            sealBolt,
                            lightningArc,
                            frostField,
                            waterRune
                        }
                        : new[]
                        {
                            lightningArc,
                            frostField,
                            waterRune
                        };

                EditorUtility.SetDirty(lucian);
            }

            EditorUtility.SetDirty(edward);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Prototype tactical abilities upgraded: " +
                "Fire Burst, Lightning Arc, " +
                "Frost Field and Water Rune.");
        }

        private static void ConfigureMelee(
            CombatActionDefinition swordSlash)
        {
            swordSlash.category =
                CombatActionCategory.Melee;

            swordSlash.targetKind =
                TargetKind.Enemy;

            swordSlash.areaRadius = 0f;

            swordSlash.areaTargetRule =
                AreaTargetRule.PrimaryOnly;

            swordSlash.requiresLineOfSight = true;
            swordSlash.ignoresCover = true;
            swordSlash.usesHeightAdvantage = false;

            EditorUtility.SetDirty(swordSlash);
        }

        private static CharacterDefinition
            GetOrCreateCharacter(
                string id,
                string displayName)
        {
            string path =
                DataRoot +
                "/Character_" +
                id +
                ".asset";

            CharacterDefinition character =
                AssetDatabase.LoadAssetAtPath<
                    CharacterDefinition>(path);

            if (character == null)
            {
                character =
                    ScriptableObject.CreateInstance<
                        CharacterDefinition>();

                AssetDatabase.CreateAsset(
                    character,
                    path);
            }

            character.characterId = id;
            character.displayName = displayName;

            return character;
        }

        private static CombatActionDefinition
            GetOrCreateAction(
                string id,
                string displayName)
        {
            string path =
                DataRoot +
                "/Action_" +
                id +
                ".asset";

            CombatActionDefinition action =
                AssetDatabase.LoadAssetAtPath<
                    CombatActionDefinition>(path);

            if (action == null)
            {
                action =
                    ScriptableObject.CreateInstance<
                        CombatActionDefinition>();

                AssetDatabase.CreateAsset(
                    action,
                    path);
            }

            action.actionId = id;
            action.displayName = displayName;

            return action;
        }
    }
}
#endif
