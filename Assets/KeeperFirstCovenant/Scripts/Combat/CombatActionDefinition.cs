using System;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    [Serializable]
    public struct StatusApplication
    {
        public StatusEffectDefinition effect;
        [Range(0f, 1f)] public float chance;
        [Min(0)] public int durationOverride;
    }

    [CreateAssetMenu(menuName = "Keeper First Covenant/Combat Action", fileName = "CombatAction")]
    public sealed class CombatActionDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string actionId;
        public string displayName = "Action";
        [TextArea] public string description;
        public Sprite icon;
        public CombatActionCategory category;
        public TargetKind targetKind = TargetKind.Enemy;

        [Header("Costs and targeting")]
        [Min(0)] public int actionPointCost = 1;
        [Min(0)] public int manaCost;
        [Min(0f)] public float rangeMeters = 1.8f;
        [Min(0f)] public float areaRadius;
        [Min(0)] public int cooldownTurns;

        [Header("Attack")]
        public bool requiresAttackRoll = true;
        [Range(0, 100)] public int baseHitChance = 75;
        public DiceFormula damage = new DiceFormula(1, 6);
        public DamageType damageType = DamageType.Physical;
        public AbilityAttribute scalingAttribute = AbilityAttribute.Strength;
        [Range(0f, 3f)] public float scalingMultiplier = 1f;

        [Header("Healing / barrier")]
        public DiceFormula healing;
        public DiceFormula barrier;

        [Header("Status")]
        public StatusApplication[] statusApplications;

        [Header("Environment hook")]
        public SurfaceType createsSurface = SurfaceType.None;
        [Min(0f)] public float surfaceRadius;
        [Min(0)] public int surfaceDurationTurns;
    }
}
