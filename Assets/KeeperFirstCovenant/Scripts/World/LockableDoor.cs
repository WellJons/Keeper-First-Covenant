using System.Collections;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Inventory;
using UnityEngine;

namespace KeeperFirstCovenant.World
{
    public sealed class LockableDoor : MonoBehaviour, IInteractable
    {
        [Header("State")]
        [SerializeField]
        private bool locked;

        [SerializeField]
        private bool open;

        [Header("Lock")]
        [SerializeField]
        private string requiredKeyItemId;

        [SerializeField]
        private bool consumeKey;

        [SerializeField]
        private string lockpickItemId =
            "dev_lockpick";

        [SerializeField, Min(1)]
        private int lockDifficulty = 12;

        [SerializeField, Min(1)]
        private int forceDifficulty = 14;

        [Header("Door movement")]
        [SerializeField]
        private Transform hinge;

        [SerializeField]
        private float openAngle = 95f;

        [SerializeField, Min(30f)]
        private float rotationSpeed = 220f;

        [Header("Noise")]
        [SerializeField, Min(0f)]
        private float normalOpenNoiseRadius = 2f;

        [SerializeField, Min(0f)]
        private float failedPickNoiseRadius = 4f;

        [SerializeField, Min(0f)]
        private float forceNoiseRadius = 12f;

        private Quaternion _closedRotation;
        private Coroutine _rotationRoutine;

        public bool IsLocked => locked;
        public bool IsOpen => open;

        public string InteractionPrompt
        {
            get
            {
                if (open)
                    return "Close";

                if (locked)
                    return "Locked";

                return "Open";
            }
        }

        private void Awake()
        {
            if (hinge == null)
                hinge = transform;

            _closedRotation =
                hinge.localRotation;
        }

        public void ConfigurePrototype(
            bool startLocked,
            string keyItemId,
            string pickItemId,
            int pickDifficulty,
            int bashDifficulty)
        {
            locked = startLocked;
            requiredKeyItemId = keyItemId;
            lockpickItemId = pickItemId;
            lockDifficulty =
                Mathf.Max(
                    1,
                    pickDifficulty);
            forceDifficulty =
                Mathf.Max(
                    1,
                    bashDifficulty);
        }

        public bool CanInteract(
            GameObject actor)
        {
            return actor != null;
        }

        public void Interact(
            GameObject actor)
        {
            if (!CanInteract(actor))
                return;

            if (open)
            {
                SetOpen(
                    false,
                    actor,
                    normalOpenNoiseRadius);
                return;
            }

            if (!locked)
            {
                SetOpen(
                    true,
                    actor,
                    normalOpenNoiseRadius);
                return;
            }

            InventoryComponent inventory =
                actor.GetComponentInParent<
                    InventoryComponent>();

            if (inventory != null &&
                !string.IsNullOrWhiteSpace(
                    requiredKeyItemId) &&
                inventory.ContainsItemId(
                    requiredKeyItemId))
            {
                locked = false;

                if (consumeKey)
                {
                    inventory.RemoveByItemId(
                        requiredKeyItemId);
                }

                SetOpen(
                    true,
                    actor,
                    normalOpenNoiseRadius);
                return;
            }

            if (inventory != null &&
                !string.IsNullOrWhiteSpace(
                    lockpickItemId) &&
                inventory.ContainsItemId(
                    lockpickItemId))
            {
                TryPickLock(actor);
            }
        }

        public bool TryPickLock(
            GameObject actor)
        {
            if (!locked ||
                actor == null)
            {
                return !locked;
            }

            CombatantRuntime combatant =
                actor.GetComponentInParent<
                    CombatantRuntime>();

            int finesseModifier =
                combatant?.Definition != null
                    ? combatant.Definition
                        .GetModifier(
                            AbilityAttribute.Finesse)
                    : 0;

            int perceptionModifier =
                combatant?.Definition != null
                    ? combatant.Definition
                        .GetModifier(
                            AbilityAttribute.Perception)
                    : 0;

            int roll =
                Random.Range(1, 21) +
                finesseModifier +
                Mathf.FloorToInt(
                    perceptionModifier * 0.5f);

            if (roll >= lockDifficulty)
            {
                locked = false;

                SetOpen(
                    true,
                    actor,
                    0.6f);

                return true;
            }

            WorldNoiseSystem.Emit(
                transform.position,
                failedPickNoiseRadius,
                actor);

            return false;
        }

        public bool TryForceOpen(
            GameObject actor,
            int bonusForce = 0)
        {
            if (!locked ||
                actor == null)
            {
                return !locked;
            }

            CombatantRuntime combatant =
                actor.GetComponentInParent<
                    CombatantRuntime>();

            int strengthModifier =
                combatant?.Definition != null
                    ? combatant.Definition
                        .GetModifier(
                            AbilityAttribute.Strength)
                    : 0;

            int roll =
                Random.Range(1, 21) +
                strengthModifier +
                bonusForce;

            WorldNoiseSystem.Emit(
                transform.position,
                forceNoiseRadius,
                actor,
                1.25f);

            if (roll < forceDifficulty)
                return false;

            locked = false;

            SetOpen(
                true,
                actor,
                0f);

            return true;
        }

        public void Unlock()
        {
            locked = false;
        }

        public void Lock()
        {
            if (!open)
                locked = true;
        }

        private void SetOpen(
            bool value,
            GameObject actor,
            float noiseRadius)
        {
            open = value;

            if (noiseRadius > 0f)
            {
                WorldNoiseSystem.Emit(
                    transform.position,
                    noiseRadius,
                    actor);
            }

            Quaternion target =
                open
                    ? _closedRotation *
                      Quaternion.Euler(
                          0f,
                          openAngle,
                          0f)
                    : _closedRotation;

            if (_rotationRoutine != null)
                StopCoroutine(
                    _rotationRoutine);

            _rotationRoutine =
                StartCoroutine(
                    RotateRoutine(target));
        }

        private static void RebuildNavigation()
        {
            TacticalGrid3D navigation =
                FindFirstObjectByType<
                    TacticalGrid3D>();

            navigation
                ?.RebuildForDynamicWorld();
        }

        private IEnumerator RotateRoutine(
            Quaternion target)
        {
            while (Quaternion.Angle(
                       hinge.localRotation,
                       target) > 0.5f)
            {
                hinge.localRotation =
                    Quaternion.RotateTowards(
                        hinge.localRotation,
                        target,
                        rotationSpeed *
                        Time.deltaTime);

                yield return null;
            }

            hinge.localRotation = target;
            _rotationRoutine = null;

            RebuildNavigation();
        }
    }
}
