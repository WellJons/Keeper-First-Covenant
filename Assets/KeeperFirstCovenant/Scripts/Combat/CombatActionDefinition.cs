using System;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    [Serializable]
    public struct StatusApplication
    {
        public StatusEffectDefinition effect;

        [Tooltip("Legacy authoring value. Runtime status application is deterministic.")]
        [Range(0f, 1f)]
        public float chance;

        public bool requiresResistanceCheck;

        public AbilityAttribute resistanceAttribute;

        [Min(0)]
        public int statusPower;

        [Min(0)]
        public int durationOverride;
    }

    [CreateAssetMenu(
        menuName = "Keeper First Covenant/Combat Action",
        fileName = "CombatAction")]
    public sealed class CombatActionDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string actionId;
        public string displayName = "Action";
        [TextArea] public string description;
        public Sprite icon;
        public CombatPresentationProfile presentationProfile;
        public CombatActionCategory category;
        public TargetKind targetKind = TargetKind.Enemy;

        [Header("Costs and targeting")]
        [Min(0)] public int actionPointCost = 1;
        [Min(0)] public int manaCost;

        [Tooltip("Special anti-spam resource used by extreme techniques such as Edward's Rift.")]
        [Min(0)] public int strainCost;
        [Min(0f)] public float rangeMeters = 1.8f;
        [Min(0f)] public float areaRadius;
        public AreaTargetRule areaTargetRule = AreaTargetRule.PrimaryOnly;
        [Min(0)] public int cooldownTurns;

        [Header("Tactical rules")]
        public bool requiresLineOfSight = true;
        public bool ignoresCover;
        public bool usesHeightAdvantage = true;

        [Header("Attack")]
        [Tooltip("Legacy compatibility flag. Normal combat resolution is deterministic.")]
        public bool requiresAttackRoll = true;

        [Tooltip("Legacy compatibility value. No random hit roll is performed.")]
        [Range(0, 100)]
        public int baseHitChance = 75;

        [Tooltip("Cover and elevation change impact strength instead of causing a random miss.")]
        public bool usesTacticalImpactModifiers = true;

        public DiceFormula damage = new DiceFormula(1, 6);
        public DamageType damageType = DamageType.Physical;
        public AbilityAttribute scalingAttribute = AbilityAttribute.Strength;
        [Range(0f, 3f)] public float scalingMultiplier = 1f;

        [Header("Healing / barrier")]
        public DiceFormula healing;
        public DiceFormula barrier;

        [Header("Status")]
        public StatusApplication[] statusApplications;

        [Header("Rule-breaking movement")]
        [Min(0f)]
        public float freeMovementMetersGranted;

        public bool freeMovementSuppressesOpportunityAttacks;

        [Header("Combo chain")]
        [Tooltip("If set, this action is empowered when the actor currently holds this combo tag.")]
        public string comboRequiresTag;

        [Tooltip("If true, the action cannot be used without the required combo tag.")]
        public bool comboRequirementMandatory;

        [Tooltip("Tag opened for a following action after this action resolves.")]
        public string comboGrantsTag;

        [Min(0)]
        public int comboWindowTurns = 1;

        [Range(1f, 3f)]
        public float comboDamageMultiplier = 1.25f;

        [Min(0)]
        public int comboBreakBonus = 10;

        public bool consumeComboTag = true;

        [Header("Break / stagger")]
        [Min(0)]
        public int breakPower = 10;

        [Header("Forced movement")]
        [Min(0f)]
        public float pushDistanceMeters;

        [Tooltip("If true, push direction is away from the acting combatant.")]
        public bool pushAwayFromActor = true;

        [Header("Environment hook")]
        public SurfaceType createsSurface = SurfaceType.None;
        [Min(0f)] public float surfaceRadius;
        [Min(0)] public int surfaceDurationTurns;
    }
}
