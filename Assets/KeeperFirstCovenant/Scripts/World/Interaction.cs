using System.Collections.Generic;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Inventory;
using UnityEngine;

namespace KeeperFirstCovenant.World
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }
        bool CanInteract(GameObject actor);
        void Interact(GameObject actor);
    }

    public sealed class SearchableLoot : MonoBehaviour, IInteractable
    {
        [SerializeField] private string prompt = "Search";
        [SerializeField] private LootTableDefinition lootTable;
        [SerializeField] private bool searchOnce = true;

        private bool _searched;
        private readonly List<InventoryStack> _lastResult = new List<InventoryStack>();

        public string InteractionPrompt => prompt;
        public IReadOnlyList<InventoryStack> LastResult => _lastResult;

        public void Configure(LootTableDefinition table, string interactionPrompt = "Search", bool oneTime = true)
        {
            lootTable = table;
            prompt = interactionPrompt;
            searchOnce = oneTime;
        }

        public bool CanInteract(GameObject actor)
        {
            return actor != null && (!searchOnce || !_searched);
        }

        public void Interact(GameObject actor)
        {
            if (!CanInteract(actor))
                return;

            InventoryComponent inventory = actor.GetComponentInParent<InventoryComponent>();
            if (inventory == null || lootTable == null)
                return;

            int perception = 10;
            CombatantRuntime combatant = actor.GetComponentInParent<CombatantRuntime>();
            if (combatant != null && combatant.Definition != null)
                perception = combatant.Definition.perception;

            _lastResult.Clear();
            _lastResult.AddRange(lootTable.Roll(perception));

            foreach (InventoryStack stack in _lastResult)
            {
                if (stack?.item != null)
                    inventory.Add(stack.item, stack.amount);
            }

            _searched = true;
        }
    }
}
