#if UNITY_EDITOR
using KeeperFirstCovenant.Characters;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Inventory;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    public static class PrototypeDeveloperTestContent
    {
        private const string DataRoot =
            "Assets/KeeperFirstCovenant/Generated/Data";

        public static void Build()
        {
            if (!AssetDatabase.IsValidFolder(
                    "Assets/KeeperFirstCovenant/Generated"))
            {
                return;
            }

            CombatActionDefinition swordSlash =
                AssetDatabase.LoadAssetAtPath<
                    CombatActionDefinition>(
                    DataRoot +
                    "/Action_SwordSlash.asset");

            CombatActionDefinition emberBolt =
                GetAction(
                    "EmberBolt",
                    "Ember Bolt",
                    DamageType.Fire,
                    new DiceFormula(2, 4),
                    9f,
                    AbilityAttribute.Intellect);

            CombatActionDefinition cataclysmFireball =
                GetOrCreate<
                    CombatActionDefinition>(
                    DataRoot +
                    "/Action_DevCataclysmFireball.asset");

            cataclysmFireball.actionId =
                "dev_cataclysm_fireball";

            cataclysmFireball.displayName =
                "DEV Cataclysm Fireball";

            cataclysmFireball.category =
                CombatActionCategory.Spell;

            cataclysmFireball.targetKind =
                TargetKind.Ground;

            cataclysmFireball.actionPointCost = 2;
            cataclysmFireball.manaCost = 15;
            cataclysmFireball.strainCost = 0;
            cataclysmFireball.rangeMeters = 14f;
            cataclysmFireball.areaRadius = 4.2f;

            cataclysmFireball.areaTargetRule =
                AreaTargetRule.Everyone;

            cataclysmFireball.requiresLineOfSight = true;
            cataclysmFireball.ignoresCover = true;
            cataclysmFireball.usesHeightAdvantage = false;
            cataclysmFireball.requiresAttackRoll = false;

            cataclysmFireball.damage =
                new DiceFormula(4, 8, 4);

            cataclysmFireball.damageType =
                DamageType.Fire;

            cataclysmFireball.scalingAttribute =
                AbilityAttribute.Intellect;

            cataclysmFireball.scalingMultiplier = 1f;

            cataclysmFireball.createsSurface =
                SurfaceType.Fire;

            cataclysmFireball.surfaceRadius = 4.2f;
            cataclysmFireball.surfaceDurationTurns = 3;

            EditorUtility.SetDirty(
                cataclysmFireball);

            CombatActionDefinition shockNeedle =
                GetAction(
                    "ShockNeedle",
                    "Искровая игла",
                    DamageType.Lightning,
                    new DiceFormula(1, 8),
                    10f,
                    AbilityAttribute.Intellect);

            shockNeedle.breakPower = 12;
            shockNeedle.cooldownTurns = 0;
            EditorUtility.SetDirty(
                shockNeedle);

            CombatActionDefinition stormRupture =
                GetOrCreate<
                    CombatActionDefinition>(
                    DataRoot +
                    "/Action_DevStormRupture.asset");

            stormRupture.actionId =
                "dev_storm_rupture";

            stormRupture.displayName =
                "Грозовой разлом";

            stormRupture.category =
                CombatActionCategory.Spell;

            stormRupture.targetKind =
                TargetKind.Ground;

            stormRupture.actionPointCost = 2;
            stormRupture.manaCost = 7;
            stormRupture.strainCost = 0;
            stormRupture.rangeMeters = 11f;
            stormRupture.areaRadius = 3.4f;

            stormRupture.areaTargetRule =
                AreaTargetRule.EnemiesOnly;

            stormRupture.requiresLineOfSight = true;
            stormRupture.ignoresCover = true;
            stormRupture.usesHeightAdvantage = false;
            stormRupture.requiresAttackRoll = false;

            stormRupture.damage =
                new DiceFormula(3, 6, 3);

            stormRupture.damageType =
                DamageType.Lightning;

            stormRupture.scalingAttribute =
                AbilityAttribute.Intellect;

            stormRupture.scalingMultiplier = 0.8f;
            stormRupture.breakPower = 24;

            stormRupture.createsSurface =
                SurfaceType.Electrified;

            stormRupture.surfaceRadius = 3.4f;
            stormRupture.surfaceDurationTurns = 2;

            stormRupture.windUpTurns = 1;
            stormRupture.interruptWindUpOnBreak = true;
            stormRupture.telegraphRadiusOverride = 3.4f;
            stormRupture.cooldownTurns = 2;

            EditorUtility.SetDirty(
                stormRupture);

            CombatActionDefinition stormExecution =
                GetOrCreate<
                    CombatActionDefinition>(
                    DataRoot +
                    "/Action_DevStormExecution.asset");

            stormExecution.actionId =
                "dev_storm_execution";

            stormExecution.displayName =
                "Удар сердца бури";

            stormExecution.category =
                CombatActionCategory.Unique;

            stormExecution.targetKind =
                TargetKind.Enemy;

            stormExecution.actionPointCost = 2;
            stormExecution.manaCost = 5;
            stormExecution.strainCost = 0;
            stormExecution.rangeMeters = 7.5f;
            stormExecution.areaRadius = 0f;

            stormExecution.areaTargetRule =
                AreaTargetRule.PrimaryOnly;

            stormExecution.requiresLineOfSight = true;
            stormExecution.ignoresCover = false;
            stormExecution.usesHeightAdvantage = true;
            stormExecution.requiresAttackRoll = false;

            stormExecution.damage =
                new DiceFormula(4, 6, 4);

            stormExecution.damageType =
                DamageType.Lightning;

            stormExecution.scalingAttribute =
                AbilityAttribute.Intellect;

            stormExecution.scalingMultiplier = 0.9f;
            stormExecution.breakPower = 32;
            stormExecution.pushDistanceMeters = 1.5f;
            stormExecution.pushAwayFromActor = true;

            stormExecution.createsSurface =
                SurfaceType.Electrified;

            stormExecution.surfaceRadius = 1.6f;
            stormExecution.surfaceDurationTurns = 1;

            stormExecution.windUpTurns = 0;
            stormExecution.cooldownTurns = 2;

            EditorUtility.SetDirty(
                stormExecution);

            CreateEnemy(
                "dev_bandit_skirmisher",
                "DEV Bandit Skirmisher",
                32,
                0,
                0,
                0,
                10.5f,
                10,
                14,
                8,
                9,
                12,
                swordSlash != null
                    ? new[] { swordSlash }
                    : System.Array.Empty<
                        CombatActionDefinition>(),
                System.Array.Empty<DamageAffinity>());

            CreateEnemy(
                "dev_ash_cultist",
                "DEV Ash Cultist",
                42,
                28,
                0,
                3,
                8.5f,
                8,
                10,
                15,
                12,
                11,
                new[] { emberBolt },
                new[]
                {
                    new DamageAffinity
                    {
                        damageType = DamageType.Fire,
                        multiplier = 0.5f
                    },
                    new DamageAffinity
                    {
                        damageType = DamageType.Frost,
                        multiplier = 1.5f
                    }
                });

            CreateEnemy(
                "dev_storm_guard",
                "DEV Storm Guard",
                62,
                18,
                3,
                2,
                7.5f,
                14,
                10,
                11,
                12,
                10,
                swordSlash != null
                    ? new[]
                    {
                        swordSlash,
                        shockNeedle
                    }
                    : new[]
                    {
                        shockNeedle
                    },
                new[]
                {
                    new DamageAffinity
                    {
                        damageType =
                            DamageType.Lightning,
                        multiplier = 0.45f
                    },
                    new DamageAffinity
                    {
                        damageType =
                            DamageType.Arcane,
                        multiplier = 1.4f
                    }
                });

            CreateWeapon(
                "dev_iron_sword",
                "DEV Iron Sword",
                WeaponClass.Sword,
                new DiceFormula(1, 8),
                DamageType.Physical,
                AbilityAttribute.Strength,
                1.8f,
                false,
                false,
                false,
                3.0f,
                35);

            CreateWeapon(
                "dev_hunter_dagger",
                "DEV Hunter Dagger",
                WeaponClass.Dagger,
                new DiceFormula(1, 4),
                DamageType.Physical,
                AbilityAttribute.Finesse,
                1.4f,
                false,
                true,
                false,
                0.8f,
                22);

            CreateWeapon(
                "dev_iron_greatsword",
                "DEV Iron Greatsword",
                WeaponClass.Greatsword,
                new DiceFormula(2, 6),
                DamageType.Physical,
                AbilityAttribute.Strength,
                2.0f,
                true,
                false,
                false,
                5.5f,
                58);

            CreateWeapon(
                "dev_ritual_staff",
                "DEV Ritual Staff",
                WeaponClass.Staff,
                new DiceFormula(1, 6),
                DamageType.Arcane,
                AbilityAttribute.Intellect,
                2.0f,
                true,
                false,
                true,
                3.4f,
                75);

            CreateWeapon(
                "dev_hunter_spear",
                "DEV Hunter Spear",
                WeaponClass.Spear,
                new DiceFormula(1, 6, 1),
                DamageType.Physical,
                AbilityAttribute.Strength,
                2.6f,
                true,
                false,
                false,
                3.8f,
                44);

            CreateArmor(
                "dev_leather_coat",
                "DEV Leather Coat",
                EquipmentSlot.Chest,
                1,
                0,
                0f,
                4.0f,
                38);

            CreateArmor(
                "dev_reinforced_coat",
                "DEV Reinforced Coat",
                EquipmentSlot.Chest,
                3,
                1,
                -0.6f,
                7.5f,
                82);

            CreateArmor(
                "dev_arcane_mantle",
                "DEV Arcane Mantle",
                EquipmentSlot.Cloak,
                0,
                3,
                0.3f,
                2.2f,
                110);

            CreateBasicItem(
                "dev_healing_draught",
                "DEV Healing Draught",
                ItemCategory.Consumable,
                ItemRarity.Common,
                0.25f,
                12,
                true,
                10,
                "Prototype healing consumable for inventory tests.");

            CreateBasicItem(
                "dev_lockpick",
                "DEV Lockpick",
                ItemCategory.Miscellaneous,
                ItemRarity.Common,
                0.05f,
                4,
                true,
                20,
                "Prototype lockpick for exploration tests.");

            CreateBasicItem(
                "dev_road_key",
                "DEV Road Ruin Key",
                ItemCategory.Key,
                ItemRarity.Common,
                0.02f,
                0,
                false,
                1,
                "Prototype key for the generated locked-door test.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static CombatActionDefinition GetAction(
            string id,
            string displayName,
            DamageType damageType,
            DiceFormula damage,
            float range,
            AbilityAttribute scaling)
        {
            CombatActionDefinition action =
                GetOrCreate<
                    CombatActionDefinition>(
                    DataRoot +
                    "/Action_" +
                    id +
                    ".asset");

            action.actionId = id;
            action.displayName = displayName;
            action.category =
                CombatActionCategory.Spell;
            action.targetKind =
                TargetKind.Enemy;
            action.actionPointCost = 1;
            action.manaCost = 2;
            action.rangeMeters = range;
            action.areaRadius = 0f;
            action.areaTargetRule =
                AreaTargetRule.PrimaryOnly;
            action.requiresLineOfSight = true;
            action.ignoresCover = false;
            action.usesHeightAdvantage = true;
            action.requiresAttackRoll = true;
            action.baseHitChance = 78;
            action.damage = damage;
            action.damageType = damageType;
            action.scalingAttribute = scaling;
            action.scalingMultiplier = 1f;

            EditorUtility.SetDirty(action);
            return action;
        }

        private static void CreateEnemy(
            string id,
            string displayName,
            int hp,
            int mana,
            int armor,
            int magicGuard,
            float movement,
            int strength,
            int finesse,
            int intellect,
            int willpower,
            int perception,
            CombatActionDefinition[] actions,
            DamageAffinity[] affinities)
        {
            CharacterDefinition enemy =
                GetOrCreate<CharacterDefinition>(
                    DataRoot +
                    "/Character_" +
                    id +
                    ".asset");

            enemy.characterId = id;
            enemy.displayName = displayName;
            enemy.faction =
                CombatFaction.Enemy;
            enemy.maxHealth = hp;
            enemy.maxMana = mana;
            enemy.armor = armor;
            enemy.magicGuard = magicGuard;
            enemy.actionPoints = 2;
            enemy.movementMeters = movement;
            enemy.strength = strength;
            enemy.finesse = finesse;
            enemy.intellect = intellect;
            enemy.willpower = willpower;
            enemy.perception = perception;
            enemy.startingActions = actions;
            enemy.damageAffinities = affinities;

            EditorUtility.SetDirty(enemy);
        }

        private static void CreateWeapon(
            string id,
            string displayName,
            WeaponClass weaponClass,
            DiceFormula damage,
            DamageType damageType,
            AbilityAttribute scaling,
            float range,
            bool twoHanded,
            bool finesse,
            bool magicalFocus,
            float weight,
            int value)
        {
            WeaponDefinition weapon =
                GetOrCreate<WeaponDefinition>(
                    DataRoot +
                    "/Weapon_" +
                    id +
                    ".asset");

            weapon.itemId = id;
            weapon.displayName = displayName;
            weapon.category =
                ItemCategory.Weapon;
            weapon.rarity =
                ItemRarity.Common;
            weapon.weaponClass =
                weaponClass;
            weapon.damage = damage;
            weapon.damageType = damageType;
            weapon.scalingAttribute = scaling;
            weapon.rangeMeters = range;
            weapon.twoHanded = twoHanded;
            weapon.finesse = finesse;
            weapon.magicalFocus =
                magicalFocus;
            weapon.weight = weight;
            weapon.valueSilver = value;
            weapon.stackable = false;
            weapon.maxStack = 1;

            CombatActionDefinition attack =
                GetOrCreate<CombatActionDefinition>(
                    DataRoot +
                    "/Action_" +
                    id +
                    "_Attack.asset");

            attack.actionId =
                id + "_attack";
            attack.displayName =
                displayName + " Attack";

            bool ranged =
                weaponClass ==
                    WeaponClass.Bow ||
                weaponClass ==
                    WeaponClass.Crossbow;

            attack.category =
                ranged
                    ? CombatActionCategory.Ranged
                    : CombatActionCategory.Melee;

            attack.targetKind =
                TargetKind.Enemy;
            attack.actionPointCost = 1;
            attack.manaCost = 0;
            attack.rangeMeters = range;
            attack.areaRadius = 0f;
            attack.areaTargetRule =
                AreaTargetRule.PrimaryOnly;
            attack.requiresLineOfSight = true;
            attack.ignoresCover = !ranged;
            attack.usesHeightAdvantage = ranged;
            attack.requiresAttackRoll = true;
            attack.baseHitChance = 78;
            attack.damage = damage;
            attack.damageType = damageType;
            attack.scalingAttribute = scaling;
            attack.scalingMultiplier = 1f;
            attack.createsSurface =
                SurfaceType.None;

            weapon.grantedActions =
                new[] { attack };

            EditorUtility.SetDirty(attack);
            EditorUtility.SetDirty(weapon);
        }

        private static void CreateArmor(
            string id,
            string displayName,
            EquipmentSlot slot,
            int armorBonus,
            int magicGuardBonus,
            float movementBonus,
            float weight,
            int value)
        {
            ArmorDefinition armor =
                GetOrCreate<ArmorDefinition>(
                    DataRoot +
                    "/Armor_" +
                    id +
                    ".asset");

            armor.itemId = id;
            armor.displayName = displayName;
            armor.category =
                ItemCategory.Armor;
            armor.rarity =
                ItemRarity.Common;
            armor.equipmentSlot = slot;
            armor.armorBonus = armorBonus;
            armor.magicGuardBonus =
                magicGuardBonus;
            armor.movementBonus =
                movementBonus;
            armor.weight = weight;
            armor.valueSilver = value;
            armor.stackable = false;
            armor.maxStack = 1;

            EditorUtility.SetDirty(armor);
        }

        private static void CreateBasicItem(
            string id,
            string displayName,
            ItemCategory category,
            ItemRarity rarity,
            float weight,
            int value,
            bool stackable,
            int maxStack,
            string description)
        {
            ItemDefinition item =
                GetOrCreate<ItemDefinition>(
                    DataRoot +
                    "/Item_" +
                    id +
                    ".asset");

            item.itemId = id;
            item.displayName = displayName;
            item.category = category;
            item.rarity = rarity;
            item.weight = weight;
            item.valueSilver = value;
            item.stackable = stackable;
            item.maxStack = maxStack;
            item.description = description;

            EditorUtility.SetDirty(item);
        }

        private static T GetOrCreate<T>(
            string path)
            where T : ScriptableObject
        {
            T asset =
                AssetDatabase.LoadAssetAtPath<T>(
                    path);

            if (asset != null)
                return asset;

            asset =
                ScriptableObject
                    .CreateInstance<T>();

            AssetDatabase.CreateAsset(
                asset,
                path);

            return asset;
        }
    }
}
#endif
