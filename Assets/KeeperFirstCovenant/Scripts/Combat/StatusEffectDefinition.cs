using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    public enum StatusStacking
    {
        RefreshDuration,
        StackDuration,
        StackIntensity,
        IgnoreNew
    }

    [CreateAssetMenu(menuName = "Keeper First Covenant/Status Effect", fileName = "StatusEffect")]
    public sealed class StatusEffectDefinition : ScriptableObject
    {
        public string effectId;
        public string displayName = "Status";
        [TextArea] public string description;
        public Sprite icon;

        [Min(1)] public int defaultDurationTurns = 2;
        public StatusStacking stacking = StatusStacking.RefreshDuration;

        [Header("Turn tick")]
        public bool dealsDamageEachTurn;
        public DiceFormula turnDamage = new DiceFormula(1, 4);
        public DamageType turnDamageType = DamageType.Physical;

        [Header("Modifiers")]
        public int armorModifier;
        public int magicGuardModifier;
        public int initiativeModifier;
        public int actionPointModifier;
        public float movementMultiplier = 1f;

        [Header("Tags")]
        public bool burning;
        public bool wet;
        public bool frozen;
        public bool shocked;
        public bool poisoned;
        public bool bleeding;
        public bool barrier;
    }
}
