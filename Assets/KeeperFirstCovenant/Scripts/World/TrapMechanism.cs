using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Inventory;
using UnityEngine;

namespace KeeperFirstCovenant.World
{
    [RequireComponent(typeof(Collider))]
    public sealed class TrapMechanism : MonoBehaviour, IInteractable
    {
        [SerializeField]
        private bool revealed;

        [SerializeField, Min(1)]
        private int disarmDifficulty = 13;

        [SerializeField]
        private string requiredToolItemId =
            "dev_lockpick";

        [SerializeField]
        private bool toolRequired;

        [Header("Trigger")]
        [SerializeField]
        private DiceFormula damage =
            new DiceFormula(2, 6);

        [SerializeField]
        private DamageType damageType =
            DamageType.Physical;

        [SerializeField, Min(0f)]
        private float triggerNoiseRadius = 10f;

        [SerializeField]
        private bool triggerOnce = true;

        [Header("Optional surface")]
        [SerializeField]
        private SurfaceType createsSurface =
            SurfaceType.None;

        [SerializeField, Min(0f)]
        private float surfaceRadius = 2f;

        [SerializeField, Min(1)]
        private int surfaceDuration = 2;

        private bool _spent;

        public string InteractionPrompt =>
            revealed
                ? "Disarm trap"
                : "Unknown mechanism";

        private void Awake()
        {
            Collider collider =
                GetComponent<Collider>();

            collider.isTrigger = true;
        }

        public void Reveal()
        {
            revealed = true;
        }

        public bool CanInteract(
            GameObject actor)
        {
            return actor != null &&
                   revealed &&
                   !_spent;
        }

        public void Interact(
            GameObject actor)
        {
            TryDisarm(actor);
        }

        public bool TryDisarm(
            GameObject actor)
        {
            if (!CanInteract(actor))
                return false;

            InventoryComponent inventory =
                actor.GetComponentInParent<
                    InventoryComponent>();

            if (toolRequired &&
                (inventory == null ||
                 !inventory.ContainsItemId(
                     requiredToolItemId)))
            {
                return false;
            }

            CombatantRuntime combatant =
                actor.GetComponentInParent<
                    CombatantRuntime>();

            int finesse =
                combatant?.Definition != null
                    ? combatant.Definition
                        .GetModifier(
                            AbilityAttribute.Finesse)
                    : 0;

            int perception =
                combatant?.Definition != null
                    ? combatant.Definition
                        .GetModifier(
                            AbilityAttribute.Perception)
                    : 0;

            int roll =
                Random.Range(1, 21) +
                finesse +
                Mathf.Max(
                    0,
                    perception);

            if (roll >= disarmDifficulty)
            {
                _spent = true;
                return true;
            }

            Trigger(actor);
            return false;
        }

        private void OnTriggerEnter(
            Collider other)
        {
            if (_spent)
                return;

            CombatantRuntime combatant =
                other.GetComponentInParent<
                    CombatantRuntime>();

            if (combatant == null ||
                !combatant.IsAlive ||
                (combatant.Faction !=
                     CombatFaction.Player &&
                 combatant.Faction !=
                     CombatFaction.Ally))
            {
                return;
            }

            Trigger(
                combatant.gameObject);
        }

        public void Trigger(
            GameObject source)
        {
            if (_spent)
                return;

            CombatantRuntime target =
                source != null
                    ? source.GetComponentInParent<
                        CombatantRuntime>()
                    : null;

            if (target != null &&
                target.IsAlive)
            {
                target.ApplyDamage(
                    new DamagePacket(
                        damage.Roll(),
                        damageType,
                        gameObject));
            }

            WorldNoiseSystem.Emit(
                transform.position,
                triggerNoiseRadius,
                source,
                1.2f);

            if (createsSurface !=
                    SurfaceType.None &&
                ElementalSurfaceSystem.Instance !=
                    null)
            {
                ElementalSurfaceSystem.Instance
                    .CreateOrReact(
                        createsSurface,
                        transform.position,
                        surfaceRadius,
                        surfaceDuration,
                        gameObject);
            }

            if (triggerOnce)
                _spent = true;
        }
    }
}
