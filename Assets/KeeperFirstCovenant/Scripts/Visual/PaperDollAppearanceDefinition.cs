using UnityEngine;

namespace KeeperFirstCovenant.Visual
{
    public enum PaperDollSlot
    {
        Body,
        Head,
        Hair,
        CloakBack,
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
        CloakFront
    }

    [System.Serializable]
    public sealed class PaperDollSlotSprites
    {
        public PaperDollSlot slot;
        public DirectionalSpriteSet4 sprites = new DirectionalSpriteSet4();
    }

    [CreateAssetMenu(
        menuName = "Keeper First Covenant/Visual/Paper Doll Appearance",
        fileName = "PaperDollAppearance")]
    public sealed class PaperDollAppearanceDefinition : ScriptableObject
    {
        public string appearanceId;
        public string displayName;

        [Tooltip("Base body/head/hair or an outfit/armor overlay. Empty slots leave the currently equipped layer unchanged.")]
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

        [Tooltip("Only the slots actually changed by this piece of gear need sprites.")]
        public PaperDollSlotSprites[] slotOverrides;

        [Header("Weapon")]
        public DirectionalSpriteSet4 weaponSprites = new DirectionalSpriteSet4();
        public bool hasWeaponVisual;
    }
}
