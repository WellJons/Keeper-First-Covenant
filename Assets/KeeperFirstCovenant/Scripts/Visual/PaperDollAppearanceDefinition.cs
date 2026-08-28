using UnityEngine;

namespace KeeperFirstCovenant.Visual
{
    public enum PaperDollSlot
    {
        Body,
        Neck,
        Head,
        Eyes,
        Mouth,
        Hair,
        HairBack,
        HairFront,
        CloakBack,
        CloakBackLeft,
        CloakBackCenter,
        CloakBackRight,
        Torso,
        UpperArmLeft,
        ForearmLeft,
        HandLeft,
        UpperArmRight,
        ForearmRight,
        HandRight,
        Pelvis,
        ThighLeft,
        ShinLeft,
        BootLeft,
        ThighRight,
        ShinRight,
        BootRight,
        CloakFront,
        CloakFrontLeft,
        CloakFrontRight,
        ShoulderAccessory,
        BeltAccessory
    }

    public enum EquipmentVisualSlot
    {
        Head,
        Torso,
        Hands,
        Legs,
        Feet,
        Cloak,
        Weapon,
        Accessory
    }

    [System.Serializable]
    public sealed class PaperDollSlotSprites
    {
        public PaperDollSlot slot;
        public DirectionalSpriteSet8 sprites = new DirectionalSpriteSet8();
    }

    [CreateAssetMenu(
        menuName = "Keeper First Covenant/Visual/Paper Doll Appearance",
        fileName = "PaperDollAppearance")]
    public sealed class PaperDollAppearanceDefinition : ScriptableObject
    {
        public string appearanceId;
        public string displayName;

        [Tooltip("Base body/head/hair or an outfit layer. Each slot is independently replaceable.")]
        public PaperDollSlotSprites[] slots;

        public PaperDollSlotSprites Find(PaperDollSlot slot)
        {
            if (slots == null)
                return null;

            for (int i = 0; i < slots.Length; i++)
            {
                PaperDollSlotSprites entry = slots[i];
                if (entry != null && entry.slot == slot)
                    return entry;
            }

            return null;
        }
    }

    [CreateAssetMenu(
        menuName = "Keeper First Covenant/Visual/Equipment Visual",
        fileName = "EquipmentVisual")]
    public sealed class EquipmentVisualDefinition : ScriptableObject
    {
        public string visualId;
        public string displayName;
        public EquipmentVisualSlot equipSlot;

        [Tooltip("Only slots actually changed by this equipment piece need entries.")]
        public PaperDollSlotSprites[] slotOverrides;

        [Tooltip("Slots hidden while this equipment is worn, e.g. hair under a full helmet.")]
        public PaperDollSlot[] hiddenSlots;

        [Header("Weapon")]
        public DirectionalSpriteSet8 weaponSprites = new DirectionalSpriteSet8();
        public bool hasWeaponVisual;

        public PaperDollSlotSprites Find(PaperDollSlot slot)
        {
            if (slotOverrides == null)
                return null;

            for (int i = 0; i < slotOverrides.Length; i++)
            {
                PaperDollSlotSprites entry = slotOverrides[i];
                if (entry != null && entry.slot == slot)
                    return entry;
            }

            return null;
        }

        public bool Hides(PaperDollSlot slot)
        {
            if (hiddenSlots == null)
                return false;

            for (int i = 0; i < hiddenSlots.Length; i++)
                if (hiddenSlots[i] == slot)
                    return true;

            return false;
        }
    }
}
