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
        Arcane
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
            int total = flatBonus;
            for (int i = 0; i < diceCount; i++)
                total += UnityEngine.Random.Range(1, dieSides + 1);

            return total;
        }

        public int Minimum => diceCount + flatBonus;
        public int Maximum => diceCount * dieSides + flatBonus;

        public override string ToString()
        {
            if (diceCount <= 0)
                return flatBonus.ToString();

            string bonus = flatBonus == 0
                ? string.Empty
                : flatBonus > 0
                    ? " + " + flatBonus
                    : " - " + Mathf.Abs(flatBonus);

            return diceCount + "d" + dieSides + bonus;
        }
    }

    public readonly struct DamagePacket
    {
        public readonly int Amount;
        public readonly DamageType Type;
        public readonly GameObject Source;
        public readonly bool Critical;

        public DamagePacket(int amount, DamageType type, GameObject source, bool critical = false)
        {
            Amount = Mathf.Max(0, amount);
            Type = type;
            Source = source;
            Critical = critical;
        }
    }
}
