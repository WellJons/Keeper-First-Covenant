using System;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    public enum DamageType
    {
        Physical,
        Fire,
        Frost,
        Lightning,
        Arcane,
        Radiant,
        Poison,
        Bleeding
    }

    public enum CombatFaction
    {
        Player,
        Ally,
        Enemy,
        Neutral
    }

    public enum CombatActionCategory
    {
        Melee,
        Ranged,
        Spell,
        Support,
        Control,
        Movement,
        Unique
    }

    public enum TargetKind
    {
        Self,
        Ally,
        Enemy,
        AnyCombatant,
        Ground
    }

    public enum AreaTargetRule
    {
        PrimaryOnly,
        EnemiesOnly,
        AlliesOnly,
        Everyone
    }

    public enum AbilityAttribute
    {
        None,
        Strength,
        Finesse,
        Intellect,
        Willpower,
        Perception
    }

    public enum SurfaceType
    {
        None,
        Fire,
        Water,
        Ice,
        Poison,
        Electrified,
        Arcane,
        Steam,

        // Internal reaction result. It should not be authored directly on abilities.
        Detonation
    }

    public enum ElementalReactionKind
    {
        None,
        ConductiveSurge,
        FlashFreeze,
        ThermalShock,
        Combustion,
        ArcaneResonance
    }

    public enum ActiveDefenseOutcome
    {
        None,
        Failed,
        Dodge,
        PerfectDodge,
        Parry,
        PerfectParry
    }

    [Serializable]
    public struct DiceFormula
    {
        [Min(0)] public int diceCount;
        [Min(2)] public int dieSides;
        public int flatBonus;

        public DiceFormula(int count, int sides, int bonus = 0)
        {
            diceCount = Mathf.Max(0, count);
            dieSides = Mathf.Max(2, sides);
            flatBonus = bonus;
        }

        public int Roll()
        {
            return DeterministicValue;
        }

        public int DeterministicValue
        {
            get
            {
                if (diceCount <= 0)
                    return flatBonus;

                float averagePerDie =
                    (Mathf.Max(2, dieSides) + 1) *
                    0.5f;

                return Mathf.RoundToInt(
                    diceCount *
                    averagePerDie +
                    flatBonus);
            }
        }

        public int Minimum => diceCount + flatBonus;
        public int Maximum => diceCount * dieSides + flatBonus;

        public override string ToString()
        {
            if (Minimum == Maximum)
                return DeterministicValue.ToString();

            return
                DeterministicValue +
                " (" +
                Minimum +
                "-" +
                Maximum +
                ")";
        }
    }

    public readonly struct DamagePacket
    {
        public readonly int Amount;
        public readonly DamageType Type;
        public readonly GameObject Source;
        public readonly bool Critical;

        public DamagePacket(
            int amount,
            DamageType type,
            GameObject source,
            bool critical = false)
        {
            Amount = Mathf.Max(0, amount);
            Type = type;
            Source = source;
            Critical = critical;
        }
    }
}
