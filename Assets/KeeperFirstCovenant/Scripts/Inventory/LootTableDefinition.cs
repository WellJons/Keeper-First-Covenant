using System;
using System.Collections.Generic;
using UnityEngine;

namespace KeeperFirstCovenant.Inventory
{
    [Serializable]
    public struct LootEntry
    {
        public ItemDefinition item;
        [Range(0f, 1f)] public float chance;
        [Min(1)] public int minAmount;
        [Min(1)] public int maxAmount;

        [Header("Search")]
        public bool hidden;
        [Min(0)] public int requiredPerception;
    }

    [CreateAssetMenu(menuName = "Keeper First Covenant/Items/Loot Table", fileName = "LootTable")]
    public sealed class LootTableDefinition : ScriptableObject
    {
        public LootEntry[] entries;

        public List<InventoryStack> Roll(int perception)
        {
            var result = new List<InventoryStack>();

            if (entries == null)
                return result;

            foreach (LootEntry entry in entries)
            {
                if (entry.item == null)
                    continue;

                if (entry.hidden && perception < entry.requiredPerception)
                    continue;

                if (UnityEngine.Random.value > entry.chance)
                    continue;

                int min = Mathf.Max(1, entry.minAmount);
                int max = Mathf.Max(min, entry.maxAmount);
                int amount = UnityEngine.Random.Range(min, max + 1);

                result.Add(new InventoryStack
                {
                    item = entry.item,
                    amount = amount
                });
            }

            return result;
        }
    }
}
