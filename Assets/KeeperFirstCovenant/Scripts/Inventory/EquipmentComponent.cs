using System;
using System.Collections.Generic;
using KeeperFirstCovenant.Combat;
using UnityEngine;

namespace KeeperFirstCovenant.Inventory
{
    [Serializable]
    public sealed class EquippedItemEntry
    {
        public EquipmentSlot slot;
        public ItemDefinition item;
    }

    [Serializable]
    public sealed class EquippedItemSnapshot
    {
        public EquipmentSlot slot;
        public string itemId;
    }

    [Serializable]
    public sealed class EquipmentSnapshot
    {
        public List<EquippedItemSnapshot> items =
            new List<EquippedItemSnapshot>();
    }

    public sealed class EquipmentComponent : MonoBehaviour
    {
        [SerializeField]
        private List<EquippedItemEntry> equipped =
            new List<EquippedItemEntry>();

        public IReadOnlyList<EquippedItemEntry> Equipped =>
            equipped;

        public event Action Changed;

        public ItemDefinition Get(
            EquipmentSlot slot)
        {
            EquippedItemEntry entry =
                equipped.Find(x =>
                    x != null &&
                    x.slot == slot);

            return entry != null
                ? entry.item
                : null;
        }

        public bool Equip(
            ItemDefinition item)
        {
            if (item == null)
                return false;

            if (!TryGetSlot(
                    item,
                    out EquipmentSlot slot))
            {
                return false;
            }

            if (item is WeaponDefinition weapon &&
                weapon.twoHanded)
            {
                UnequipInternal(
                    EquipmentSlot.OffHand,
                    false);
            }

            if (slot == EquipmentSlot.OffHand)
            {
                WeaponDefinition main =
                    Get(EquipmentSlot.MainHand)
                    as WeaponDefinition;

                if (main != null &&
                    main.twoHanded)
                {
                    return false;
                }
            }

            EquippedItemEntry existing =
                equipped.Find(x =>
                    x != null &&
                    x.slot == slot);

            if (existing == null)
            {
                equipped.Add(
                    new EquippedItemEntry
                    {
                        slot = slot,
                        item = item
                    });
            }
            else
            {
                existing.item = item;
            }

            Changed?.Invoke();
            return true;
        }

        public ItemDefinition Unequip(
            EquipmentSlot slot)
        {
            return UnequipInternal(
                slot,
                true);
        }

        public int GetArmorBonus()
        {
            int value = 0;

            foreach (EquippedItemEntry entry
                     in equipped)
            {
                if (entry?.item is
                    ArmorDefinition armor)
                {
                    value += armor.armorBonus;
                }
            }

            return value;
        }

        public int GetMagicGuardBonus()
        {
            int value = 0;

            foreach (EquippedItemEntry entry
                     in equipped)
            {
                if (entry?.item is
                    ArmorDefinition armor)
                {
                    value += armor.magicGuardBonus;
                }
            }

            return value;
        }

        public float GetMovementBonus()
        {
            float value = 0f;

            foreach (EquippedItemEntry entry
                     in equipped)
            {
                if (entry?.item is
                    ArmorDefinition armor)
                {
                    value += armor.movementBonus;
                }
            }

            return value;
        }

        public void CollectGrantedActions(
            List<CombatActionDefinition> output)
        {
            if (output == null)
                return;

            foreach (EquippedItemEntry entry
                     in equipped)
            {
                if (entry == null ||
                    entry.item == null)
                {
                    continue;
                }

                CombatActionDefinition[] actions =
                    null;

                if (entry.item is
                    WeaponDefinition weapon)
                {
                    actions =
                        weapon.grantedActions;
                }
                else if (entry.item is
                         ArmorDefinition armor)
                {
                    actions =
                        armor.grantedActions;
                }

                if (actions == null)
                    continue;

                foreach (CombatActionDefinition action
                         in actions)
                {
                    if (action != null &&
                        !output.Contains(action))
                    {
                        output.Add(action);
                    }
                }
            }
        }

        public bool TryEquipFromInventory(
            InventoryComponent inventory,
            ItemDefinition item,
            out string error)
        {
            error = string.Empty;

            if (inventory == null || item == null)
            {
                error = "Предмет или инвентарь недоступен.";
                return false;
            }

            if (inventory.Count(item) <= 0)
            {
                error = "Предмета нет в инвентаре.";
                return false;
            }

            if (!TryGetSlot(item, out EquipmentSlot slot))
            {
                error = "Этот предмет нельзя экипировать.";
                return false;
            }

            ItemDefinition current =
                Get(slot);

            ItemDefinition displacedOffHand = null;

            if (item is WeaponDefinition weapon &&
                weapon.twoHanded)
            {
                displacedOffHand =
                    Get(EquipmentSlot.OffHand);
            }

            if (!inventory.Remove(item, 1))
            {
                error = "Не удалось взять предмет из инвентаря.";
                return false;
            }

            var returned =
                new List<ItemDefinition>();

            if (!TryReturnItem(
                    inventory,
                    current,
                    returned))
            {
                RollbackInventoryTransaction(
                    inventory,
                    item,
                    returned);

                error = "Не хватает места, чтобы снять текущую экипировку.";
                return false;
            }

            if (displacedOffHand != null &&
                displacedOffHand != current &&
                !TryReturnItem(
                    inventory,
                    displacedOffHand,
                    returned))
            {
                RollbackInventoryTransaction(
                    inventory,
                    item,
                    returned);

                error = "Не хватает места для предмета из второй руки.";
                return false;
            }

            if (!Equip(item))
            {
                RollbackInventoryTransaction(
                    inventory,
                    item,
                    returned);

                error = "Предмет нельзя экипировать в этот слот.";
                return false;
            }

            return true;
        }

        public bool TryUnequipToInventory(
            InventoryComponent inventory,
            EquipmentSlot slot,
            out string error)
        {
            error = string.Empty;

            if (inventory == null)
            {
                error = "Инвентарь недоступен.";
                return false;
            }

            ItemDefinition item =
                Get(slot);

            if (item == null)
            {
                error = "В этом слоте ничего нет.";
                return false;
            }

            if (!inventory.Add(item, 1))
            {
                error = "Недостаточно места в инвентаре.";
                return false;
            }

            ItemDefinition removed =
                Unequip(slot);

            if (removed == null)
            {
                inventory.Remove(item, 1);
                error = "Не удалось снять предмет.";
                return false;
            }

            return true;
        }

        private static bool TryReturnItem(
            InventoryComponent inventory,
            ItemDefinition item,
            List<ItemDefinition> returned)
        {
            if (item == null)
                return true;

            if (!inventory.Add(item, 1))
                return false;

            returned.Add(item);
            return true;
        }

        private static void RollbackInventoryTransaction(
            InventoryComponent inventory,
            ItemDefinition equippedCandidate,
            List<ItemDefinition> returned)
        {
            if (returned != null)
            {
                for (int i = returned.Count - 1;
                     i >= 0;
                     i--)
                {
                    inventory.Remove(
                        returned[i],
                        1);
                }
            }

            inventory.Add(
                equippedCandidate,
                1);
        }

        public EquipmentSnapshot CaptureSnapshot()
        {
            var snapshot = new EquipmentSnapshot();

            foreach (EquippedItemEntry entry in equipped)
            {
                if (entry?.item == null ||
                    string.IsNullOrWhiteSpace(entry.item.itemId))
                {
                    continue;
                }

                snapshot.items.Add(new EquippedItemSnapshot
                {
                    slot = entry.slot,
                    itemId = entry.item.itemId
                });
            }

            return snapshot;
        }

        public void RestoreSnapshot(
            EquipmentSnapshot snapshot,
            Func<string, ItemDefinition> resolver)
        {
            equipped.Clear();

            if (snapshot?.items != null && resolver != null)
            {
                foreach (EquippedItemSnapshot saved in snapshot.items)
                {
                    if (saved == null ||
                        string.IsNullOrWhiteSpace(saved.itemId))
                    {
                        continue;
                    }

                    ItemDefinition item = resolver(saved.itemId);
                    if (item == null)
                    {
                        Debug.LogWarning(
                            $"Equipped item '{saved.itemId}' could not be restored on {name}.");
                        continue;
                    }

                    equipped.Add(new EquippedItemEntry
                    {
                        slot = saved.slot,
                        item = item
                    });
                }
            }

            Changed?.Invoke();
        }

        private ItemDefinition UnequipInternal(
            EquipmentSlot slot,
            bool notify)
        {
            int index =
                equipped.FindIndex(x =>
                    x != null &&
                    x.slot == slot);

            if (index < 0)
                return null;

            ItemDefinition item =
                equipped[index].item;

            equipped.RemoveAt(index);

            if (notify)
                Changed?.Invoke();

            return item;
        }

        private static bool TryGetSlot(
            ItemDefinition item,
            out EquipmentSlot slot)
        {
            if (item is WeaponDefinition)
            {
                slot = EquipmentSlot.MainHand;
                return true;
            }

            if (item is ArmorDefinition armor)
            {
                slot = armor.equipmentSlot;
                return true;
            }

            slot = default;
            return false;
        }
    }
}
