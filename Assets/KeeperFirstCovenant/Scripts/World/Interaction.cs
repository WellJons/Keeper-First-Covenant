using System;
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

    public sealed class LootTransferReport
    {
        public IReadOnlyList<InventoryStack> Collected { get; }
        public bool HasRemaining { get; }

        public LootTransferReport(
            IReadOnlyList<InventoryStack> collected,
            bool hasRemaining)
        {
            Collected = collected;
            HasRemaining = hasRemaining;
        }
    }

    public sealed class SearchableLoot :
        MonoBehaviour,
        IInteractable,
        IPersistentWorldObject
    {
        [Serializable]
        private sealed class PendingLootState
        {
            public string itemId;
            public int amount;
        }

        [Serializable]
        private sealed class PersistentState
        {
            public bool searched;
            public bool rolled;
            public List<PendingLootState> pending =
                new List<PendingLootState>();
        }

        [SerializeField] private string persistenceId;
        [SerializeField] private string prompt = "Обыскать";
        [SerializeField] private LootTableDefinition lootTable;
        [SerializeField] private bool searchOnce = true;

        private bool _searched;
        private bool _rolled;

        private readonly List<InventoryStack>
            _pendingLoot =
                new List<InventoryStack>();

        private readonly List<InventoryStack>
            _lastResult =
                new List<InventoryStack>();

        public static event Action<
            SearchableLoot,
            GameObject,
            LootTransferReport>
            LootTransferred;

        public string InteractionPrompt
        {
            get
            {
                if (_pendingLoot.Count > 0)
                    return "Забрать оставшееся";

                if (searchOnce && _searched)
                    return "Пусто";

                return prompt;
            }
        }

        public IReadOnlyList<InventoryStack>
            LastResult =>
                _lastResult;

        public bool IsSearched => _searched;
        public bool HasPendingLoot =>
            _pendingLoot.Count > 0;

        public string PersistenceId =>
            WorldPersistenceUtility.GetStableId(
                this,
                persistenceId);

        public void Configure(
            LootTableDefinition table,
            string interactionPrompt =
                "Обыскать",
            bool oneTime = true)
        {
            lootTable = table;
            prompt = interactionPrompt;
            searchOnce = oneTime;
        }

        public bool CanInteract(GameObject actor)
        {
            if (actor == null)
                return false;

            if (_pendingLoot.Count > 0)
                return true;

            return !searchOnce || !_searched;
        }

        public void Interact(GameObject actor)
        {
            if (!CanInteract(actor))
                return;

            InventoryComponent inventory =
                actor.GetComponentInParent<
                    InventoryComponent>();

            if (inventory == null ||
                lootTable == null)
            {
                return;
            }

            if (!_rolled ||
                (!searchOnce &&
                 _searched &&
                 _pendingLoot.Count == 0))
            {
                RollLoot(actor);
            }

            TransferAvailableLoot(
                actor,
                inventory);
        }

        private void RollLoot(
            GameObject actor)
        {
            int perception = 10;

            CombatantRuntime combatant =
                actor.GetComponentInParent<
                    CombatantRuntime>();

            if (combatant != null &&
                combatant.Definition != null)
            {
                perception =
                    combatant.Definition
                        .perception;
            }

            _pendingLoot.Clear();

            List<InventoryStack> rolled =
                lootTable.Roll(perception);

            foreach (InventoryStack stack
                     in rolled)
            {
                if (stack?.item == null ||
                    stack.amount <= 0)
                {
                    continue;
                }

                _pendingLoot.Add(
                    new InventoryStack
                    {
                        item = stack.item,
                        amount = stack.amount
                    });
            }

            _rolled = true;
            _searched = false;
        }

        private void TransferAvailableLoot(
            GameObject actor,
            InventoryComponent inventory)
        {
            _lastResult.Clear();

            for (int i =
                     _pendingLoot.Count - 1;
                 i >= 0;
                 i--)
            {
                InventoryStack pending =
                    _pendingLoot[i];

                if (pending?.item == null ||
                    pending.amount <= 0)
                {
                    _pendingLoot.RemoveAt(i);
                    continue;
                }

                int carryable =
                    inventory
                        .GetMaxCarryableAmount(
                            pending.item);

                int moved =
                    Mathf.Min(
                        pending.amount,
                        carryable);

                if (moved <= 0)
                    continue;

                if (!inventory.Add(
                        pending.item,
                        moved))
                {
                    continue;
                }

                _lastResult.Add(
                    new InventoryStack
                    {
                        item = pending.item,
                        amount = moved
                    });

                pending.amount -= moved;

                if (pending.amount <= 0)
                    _pendingLoot.RemoveAt(i);
            }

            _searched =
                _pendingLoot.Count == 0;

            var collectedCopy =
                new List<InventoryStack>();

            foreach (InventoryStack stack
                     in _lastResult)
            {
                collectedCopy.Add(
                    new InventoryStack
                    {
                        item = stack.item,
                        amount = stack.amount
                    });
            }

            LootTransferred?.Invoke(
                this,
                actor,
                new LootTransferReport(
                    collectedCopy,
                    _pendingLoot.Count > 0));
        }

        public string CapturePersistentState()
        {
            var state =
                new PersistentState
                {
                    searched = _searched,
                    rolled = _rolled
                };

            foreach (InventoryStack stack
                     in _pendingLoot)
            {
                if (stack?.item == null ||
                    string.IsNullOrWhiteSpace(
                        stack.item.itemId) ||
                    stack.amount <= 0)
                {
                    continue;
                }

                state.pending.Add(
                    new PendingLootState
                    {
                        itemId =
                            stack.item.itemId,
                        amount =
                            stack.amount
                    });
            }

            return JsonUtility.ToJson(state);
        }

        public void RestorePersistentState(
            string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return;

            PersistentState state =
                JsonUtility.FromJson<
                    PersistentState>(json);

            if (state == null)
                return;

            _searched = state.searched;
            _rolled = state.rolled;
            _pendingLoot.Clear();
            _lastResult.Clear();

            if (state.pending == null)
                return;

            foreach (PendingLootState saved
                     in state.pending)
            {
                if (saved == null ||
                    string.IsNullOrWhiteSpace(
                        saved.itemId) ||
                    saved.amount <= 0)
                {
                    continue;
                }

                ItemDefinition item =
                    ItemCatalogService.Resolve(
                        saved.itemId);

                if (item == null)
                {
                    Debug.LogWarning(
                        "Could not restore pending loot item '" +
                        saved.itemId +
                        "' on " +
                        name +
                        ".");

                    continue;
                }

                _pendingLoot.Add(
                    new InventoryStack
                    {
                        item = item,
                        amount = saved.amount
                    });
            }

            if (_pendingLoot.Count > 0)
                _searched = false;
        }
    }
}
