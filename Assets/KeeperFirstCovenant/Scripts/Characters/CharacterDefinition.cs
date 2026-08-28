using KeeperFirstCovenant.Combat;
using UnityEngine;

namespace KeeperFirstCovenant.Characters
{
    [CreateAssetMenu(menuName = "Keeper First Covenant/Character Definition", fileName = "CharacterDefinition")]
    public sealed class CharacterDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string characterId;
        public string displayName = "Character";
        [TextArea] public string shortDescription;
        public Sprite portrait;
        public CombatFaction faction = CombatFaction.Neutral;

        [Header("Primary attributes")]
        [Min(1)] public int strength = 10;
        [Min(1)] public int finesse = 10;
        [Min(1)] public int intellect = 10;
        [Min(1)] public int willpower = 10;
        [Min(1)] public int perception = 10;

        [Header("Combat")]
        [Min(1)] public int maxHealth = 50;
        [Min(0)] public int maxMana = 20;
        [Min(0)] public int armor = 0;
        [Min(0)] public int magicGuard = 0;
        [Min(1)] public int actionPoints = 2;
        [Min(0.5f)] public float movementMeters = 9f;
        public int initiativeBonus = 0;

        [Header("Starting abilities")]
        public CombatActionDefinition[] startingActions;

        public int GetAttribute(AbilityAttribute attribute)
        {
            switch (attribute)
            {
                case AbilityAttribute.Strength: return strength;
                case AbilityAttribute.Finesse: return finesse;
                case AbilityAttribute.Intellect: return intellect;
                case AbilityAttribute.Willpower: return willpower;
                case AbilityAttribute.Perception: return perception;
                default: return 0;
            }
        }

        public int GetModifier(AbilityAttribute attribute)
        {
            int score = GetAttribute(attribute);
            return Mathf.FloorToInt((score - 10) / 2f);
        }
    }
}
