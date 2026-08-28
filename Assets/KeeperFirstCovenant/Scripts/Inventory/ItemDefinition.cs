using KeeperFirstCovenant.Combat;
using UnityEngine;

namespace KeeperFirstCovenant.Inventory
{
    public enum ItemCategory
    {
        Weapon,
        Armor,
        Consumable,
        Ingredient,
        Key,
        Quest,
        Treasure,
        Miscellaneous
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Unique
    }

    [CreateAssetMenu(menuName = "Keeper First Covenant/Items/Item", fileName = "Item")]
    public class ItemDefinition : ScriptableObject
    {
        public string itemId;
        public string displayName = "Item";
        [TextArea] public string description;
        public Sprite icon;
        public ItemCategory category;
        public ItemRarity rarity;

        [Min(0f)] public float weight = 0.1f;
        [Min(0)] public int valueSilver;
        public bool stackable;
        [Min(1)] public int maxStack = 1;

        [Header("Discovery")]
        public bool questCritical;
        public bool illegalOrSuspicious;
    }

    public enum WeaponClass
    {
        Sword,
        Greatsword,
        Dagger,
        Axe,
        Spear,
        Bow,
        Crossbow,
        Staff,
        Wand,
        Unarmed
    }

    [CreateAssetMenu(menuName = "Keeper First Covenant/Items/Weapon", fileName = "Weapon")]
    public sealed class WeaponDefinition : ItemDefinition
    {
        public WeaponClass weaponClass;
        public DiceFormula damage = new DiceFormula(1, 8);
        public DamageType damageType = DamageType.Physical;
        public AbilityAttribute scalingAttribute = AbilityAttribute.Strength;
        [Min(0.5f)] public float rangeMeters = 1.8f;
        public bool twoHanded;
        public bool finesse;
        public bool magicalFocus;
    }
}
