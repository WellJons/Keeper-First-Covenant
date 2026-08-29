using System;
using System.Collections.Generic;
using UnityEngine;

namespace KeeperFirstCovenant.Inventory
{
    [Serializable]
    public sealed class InventoryStack
    {
        public ItemDefinition item;
        [Min(1)] public int amount = 1;
    }

    [Serializable]
    public sealed class InventoryItemSnapshot
    {
        public string itemId;
        public int amount;
    }

    [Serializable]
    public sealed class InventorySnapshot
    {
        public List<InventoryItemSnapshot> items = new List<InventoryItemSnapshot>();
    }

    public sealed class InventoryComponent : MonoBehaviour
    {
        [SerializeField] private List<InventoryStack> items = new List<InventoryStack>();
        [SerializeField] private float maxCarryWeight = 60f;

        public IReadOnlyList<InventoryStack> Items => items;
        public float MaxCarryWeight => maxCarryWeight;

        public float CurrentWeight
        {
            get
            {
                float total = 0f;
                foreach (InventoryStack stack in items)
                {
                    if (stack?.item != null)
                        total += stack.item.weight * Mathf.Max(0, stack.amount);
                }

                return total;
            }
        }

        public event Action Changed;

        public bool CanCarry(ItemDefinition item, int amount)
        {
            if (item == null || amount <= 0)
                return false;

            return CurrentWeight + item.weight * amount <= maxCarryWeight + 0.001f;
        }

        public int GetMaxCarryableAmount(
            ItemDefinition item)
        {
            if (item == null)
                return 0;

            if (item.weight <= 0.0001f)
                return int.MaxValue;

            float available =
                Mathf.Max(
                    0f,
                    maxCarryWeight -
                    CurrentWeight);

            return Mathf.Max(
                0,
                Mathf.FloorToInt(
                    (available + 0.001f) /
                    item.weight));
        }

        public bool Add(ItemDefinition item, int amount = 1)
        {
            if (item == null || amount <= 0 || !CanCarry(item, amount))
                return false;

            if (item.stackable)
            {
                InventoryStack existing = items.Find(x =>
                    x != null &&
                    x.item == item &&
                    x.amount < Mathf.Max(1, item.maxStack));

                if (existing != null)
                {
                    int room = Mathf.Max(1, item.maxStack) - existing.amount;
                    int moved = Mathf.Min(room, amount);
                    existing.amount += moved;
                    amount -= moved;
                }
            }

            while (amount > 0)
            {
                int stackAmount = item.stackable
                    ? Mathf.Min(Mathf.Max(1, item.maxStack), amount)
                    : 1;

                items.Add(new InventoryStack
                {
                    item = item,
                    amount = stackAmount
                });

                amount -= stackAmount;
            }

            Changed?.Invoke();
            return true;
        }

        public bool Remove(ItemDefinition item, int amount = 1)
        {
            if (item == null || amount <= 0 || Count(item) < amount)
                return false;

            for (int i = items.Count - 1; i >= 0 && amount > 0; i--)
            {
                InventoryStack stack = items[i];
                if (stack == null || stack.item != item)
                    continue;

                int removed = Mathf.Min(stack.amount, amount);
                stack.amount -= removed;
                amount -= removed;

                if (stack.amount <= 0)
                    items.RemoveAt(i);
            }

            Changed?.Invoke();
            return true;
        }

        public bool ContainsItemId(
            string itemId,
            int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(
                    itemId) ||
                amount <= 0)
            {
                return false;
            }

            int count = 0;

            foreach (InventoryStack stack
                     in items)
            {
                if (stack?.item == null ||
                    stack.item.itemId != itemId)
                {
                    continue;
                }

                count +=
                    Mathf.Max(
                        0,
                        stack.amount);

                if (count >= amount)
                    return true;
            }

            return false;
        }

        public bool RemoveByItemId(
            string itemId,
            int amount = 1)
        {
            if (!ContainsItemId(
                    itemId,
                    amount))
            {
                return false;
            }

            for (int i = items.Count - 1;
                 i >= 0 &&
                 amount > 0;
                 i--)
            {
                InventoryStack stack =
                    items[i];

                if (stack?.item == null ||
                    stack.item.itemId != itemId)
                {
                    continue;
                }

                int removed =
                    Mathf.Min(
                        stack.amount,
                        amount);

                stack.amount -= removed;
                amount -= removed;

                if (stack.amount <= 0)
                    items.RemoveAt(i);
            }

            Changed?.Invoke();
            return true;
        }

        public int Count(ItemDefinition item)
        {
            int count = 0;
            foreach (InventoryStack stack in items)
                if (stack != null && stack.item == item)
                    count += Mathf.Max(0, stack.amount);
            return count;
        }

        public InventorySnapshot CaptureSnapshot()
        {
            var snapshot = new InventorySnapshot();

            foreach (InventoryStack stack in items)
            {
                if (stack?.item == null ||
                    string.IsNullOrWhiteSpace(stack.item.itemId) ||
                    stack.amount <= 0)
                {
                    continue;
                }

                snapshot.items.Add(new InventoryItemSnapshot
                {
                    itemId = stack.item.itemId,
                    amount = stack.amount
                });
            }

            return snapshot;
        }

        public void RestoreSnapshot(
            InventorySnapshot snapshot,
            Func<string, ItemDefinition> resolver)
        {
            items.Clear();

            if (snapshot?.items != null && resolver != null)
            {
                foreach (InventoryItemSnapshot saved in snapshot.items)
                {
                    if (saved == null ||
                        string.IsNullOrWhiteSpace(saved.itemId) ||
                        saved.amount <= 0)
                    {
                        continue;
                    }

                    ItemDefinition item = resolver(saved.itemId);
                    if (item == null)
                    {
                        Debug.LogWarning(
                            $"Inventory item '{saved.itemId}' could not be restored on {name}.");
                        continue;
                    }

                    int remaining = saved.amount;

                    while (remaining > 0)
                    {
                        int stackAmount = item.stackable
                            ? Mathf.Min(Mathf.Max(1, item.maxStack), remaining)
                            : 1;

                        items.Add(new InventoryStack
                        {
                            item = item,
                            amount = stackAmount
                        });

                        remaining -= stackAmount;
                    }
                }
            }

            Changed?.Invoke();
        }
    }
}
