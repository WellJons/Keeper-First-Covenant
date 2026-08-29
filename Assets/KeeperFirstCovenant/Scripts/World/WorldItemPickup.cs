using System;
using KeeperFirstCovenant.Inventory;
using UnityEngine;

namespace KeeperFirstCovenant.World
{
    public sealed class WorldItemPickup :
        MonoBehaviour,
        IInteractable,
        IPersistentWorldObject
    {
        [Serializable]
        private sealed class PersistentState
        {
            public bool collected;
            public int remainingAmount;
        }

        [SerializeField]
        private string persistenceId;

        [SerializeField]
        private ItemDefinition item;

        [SerializeField, Min(1)]
        private int amount = 1;

        [SerializeField]
        private string prompt = "Поднять";

        [SerializeField]
        private bool disableObjectWhenCollected = true;

        private int remainingAmount;
        private bool collected;

        public static event Action<
            WorldItemPickup,
            GameObject,
            InventoryStack,
            bool> ItemTransferred;

        public string InteractionPrompt =>
            collected
                ? "Пусто"
                : prompt +
                  (remainingAmount > 1
                      ? " ×" + remainingAmount
                      : string.Empty);

        public ItemDefinition Item => item;
        public int RemainingAmount => remainingAmount;
        public bool IsCollected => collected;

        public string PersistenceId =>
            WorldPersistenceUtility.GetStableId(
                this,
                persistenceId);

        private void Awake()
        {
            if (remainingAmount <= 0)
                remainingAmount = Mathf.Max(1, amount);

            ApplyVisualState();
        }

        public bool CanInteract(
            GameObject actor)
        {
            return actor != null &&
                   item != null &&
                   !collected &&
                   remainingAmount > 0;
        }

        public void Interact(
            GameObject actor)
        {
            if (!CanInteract(actor))
                return;

            InventoryComponent inventory =
                actor.GetComponentInParent<
                    InventoryComponent>();

            if (inventory == null)
                return;

            int carryable =
                inventory.GetMaxCarryableAmount(
                    item);

            int moved =
                Mathf.Min(
                    remainingAmount,
                    carryable);

            if (moved <= 0)
            {
                ItemTransferred?.Invoke(
                    this,
                    actor,
                    null,
                    true);

                return;
            }

            if (!inventory.Add(
                    item,
                    moved))
            {
                ItemTransferred?.Invoke(
                    this,
                    actor,
                    null,
                    true);

                return;
            }

            remainingAmount -= moved;
            collected =
                remainingAmount <= 0;

            if (collected)
                remainingAmount = 0;

            ApplyVisualState();

            ItemTransferred?.Invoke(
                this,
                actor,
                new InventoryStack
                {
                    item = item,
                    amount = moved
                },
                !collected);
        }

        public string CapturePersistentState()
        {
            return JsonUtility.ToJson(
                new PersistentState
                {
                    collected = collected,
                    remainingAmount =
                        remainingAmount
                });
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

            collected = state.collected;

            remainingAmount =
                collected
                    ? 0
                    : Mathf.Max(
                        0,
                        state.remainingAmount);

            if (!collected &&
                remainingAmount <= 0)
            {
                remainingAmount =
                    Mathf.Max(1, amount);
            }

            ApplyVisualState();
        }

        private void ApplyVisualState()
        {
            if (!disableObjectWhenCollected)
                return;

            foreach (Renderer renderer in
                     GetComponentsInChildren<
                         Renderer>(true))
            {
                renderer.enabled =
                    !collected;
            }

            foreach (Collider collider in
                     GetComponentsInChildren<
                         Collider>(true))
            {
                collider.enabled =
                    !collected;
            }
        }
    }
}
