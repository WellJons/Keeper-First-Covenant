using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KeeperFirstCovenant.Inventory
{
    [CreateAssetMenu(
        menuName = "Keeper First Covenant/Items/Item Catalog",
        fileName = "ItemCatalog")]
    public sealed class ItemCatalogDefinition :
        ScriptableObject
    {
        [SerializeField]
        private List<ItemDefinition> items =
            new List<ItemDefinition>();

        public IReadOnlyList<ItemDefinition> Items =>
            items;

        public void SetItems(
            IEnumerable<ItemDefinition> values)
        {
            items =
                values != null
                    ? values
                        .Where(value =>
                            value != null)
                        .Distinct()
                        .OrderBy(value =>
                            value.itemId,
                            StringComparer.Ordinal)
                        .ToList()
                    : new List<ItemDefinition>();
        }
    }

    public static class ItemCatalogService
    {
        private const string ResourceName =
            "ItemCatalog";

        private static ItemCatalogDefinition catalog;
        private static Dictionary<string, ItemDefinition> lookup;

        public static ItemDefinition Resolve(
            string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return null;

            EnsureLookup();

            return lookup.TryGetValue(
                itemId,
                out ItemDefinition item)
                ? item
                : null;
        }

        public static Dictionary<string, ItemDefinition>
            BuildLookup()
        {
            EnsureLookup();

            return new Dictionary<string, ItemDefinition>(
                lookup,
                StringComparer.Ordinal);
        }

        public static void Invalidate()
        {
            catalog = null;
            lookup = null;
        }

        private static void EnsureLookup()
        {
            if (lookup != null)
                return;

            lookup =
                new Dictionary<string, ItemDefinition>(
                    StringComparer.Ordinal);

            catalog =
                Resources.Load<ItemCatalogDefinition>(
                    ResourceName);

            if (catalog != null)
            {
                AddItems(catalog.Items);
            }

            ItemDefinition[] loaded =
                Resources.FindObjectsOfTypeAll<
                    ItemDefinition>();

            AddItems(loaded);
        }

        private static void AddItems(
            IEnumerable<ItemDefinition> values)
        {
            if (values == null)
                return;

            foreach (ItemDefinition item in values)
            {
                if (item == null ||
                    string.IsNullOrWhiteSpace(
                        item.itemId))
                {
                    continue;
                }

                lookup[item.itemId] = item;
            }
        }
    }
}
