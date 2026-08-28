using KeeperFirstCovenant.Characters;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Inventory;
using UnityEngine;

namespace KeeperFirstCovenant.Developer
{
    public sealed class DeveloperContentCatalog : MonoBehaviour
    {
        [SerializeField] private CharacterDefinition[] characters;
        [SerializeField] private ItemDefinition[] items;
        [SerializeField] private CombatActionDefinition[] actions;

        public CharacterDefinition[] Characters =>
            characters ?? System.Array.Empty<CharacterDefinition>();

        public ItemDefinition[] Items =>
            items ?? System.Array.Empty<ItemDefinition>();

        public CombatActionDefinition[] Actions =>
            actions ?? System.Array.Empty<CombatActionDefinition>();

        public void Configure(
            CharacterDefinition[] characterDefinitions,
            ItemDefinition[] itemDefinitions,
            CombatActionDefinition[] actionDefinitions)
        {
            characters = characterDefinitions ??
                System.Array.Empty<CharacterDefinition>();

            items = itemDefinitions ??
                System.Array.Empty<ItemDefinition>();

            actions = actionDefinitions ??
                System.Array.Empty<CombatActionDefinition>();
        }
    }
}
