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
