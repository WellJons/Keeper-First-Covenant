using System;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    [RequireComponent(typeof(CombatantRuntime))]
    public sealed class BreakGaugeComponent :
        MonoBehaviour
    {
        [SerializeField, Min(20)]
        private int baseStability = 45;

        [SerializeField, Range(1f, 2f)]
        private float brokenDamageMultiplier = 1.35f;

        [SerializeField, Range(0f, 1f)]
        private float recoveryAfterBreak = 0.25f;

        [SerializeField, Range(0f, 1f)]
        private float brokenMovementLoss = 0.5f;

        [SerializeField, Min(0)]
        private int brokenActionPointLoss = 1;

        private CombatantRuntime owner;
        private int stability;
        private bool broken;
        private bool brokenTurnConsumed;

        public static event Action<
            BreakGaugeComponent>
            GaugeChanged;

        public static event Action<
            BreakGaugeComponent>
            Broken;

        public CombatantRuntime Owner => owner;

        public int MaxStability
        {
            get
            {
                if (owner?.Definition == null)
                    return baseStability;

                int strength =
                    owner.Definition.GetAttribute(
                        AbilityAttribute.Strength);

                int willpower =
                    owner.Definition.GetAttribute(
                        AbilityAttribute.Willpower);

                return Mathf.Max(
                    20,
                    baseStability +
                    strength * 2 +
                    willpower * 3);
            }
        }

        public int Stability => stability;
        public bool IsBroken => broken;

        public float Normalized =>
            MaxStability > 0
                ? Mathf.Clamp01(
                    stability /
                    (float)MaxStability)
                : 0f;

        public float IncomingDamageMultiplier =>
            broken
                ? brokenDamageMultiplier
                : 1f;

        private void Awake()
        {
            owner =
                GetComponent<
                    CombatantRuntime>();

            ResetGauge();
        }

        private void OnEnable()
        {
            CombatActionExecutor.ActionResolved +=
                OnActionResolved;

            ElementalSurfaceSystem.ReactionTriggered +=
                OnElementalReaction;

            if (owner == null)
            {
                owner =
                    GetComponent<
                        CombatantRuntime>();
            }

            if (owner != null)
            {
                owner.TurnStarted +=
                    OnTurnStarted;

                owner.TurnEnded +=
                    OnTurnEnded;

                owner.Died +=
                    OnOwnerDied;
            }
        }

        private void OnDisable()
        {
            CombatActionExecutor.ActionResolved -=
                OnActionResolved;

            ElementalSurfaceSystem.ReactionTriggered -=
                OnElementalReaction;

            if (owner != null)
            {
                owner.TurnStarted -=
                    OnTurnStarted;

                owner.TurnEnded -=
                    OnTurnEnded;

                owner.Died -=
                    OnOwnerDied;
            }
        }

        public void ResetGauge()
        {
            stability = 0;
            broken = false;
            brokenTurnConsumed = false;

            GaugeChanged?.Invoke(this);
        }

        public void AddBreak(
            int amount)
        {
            if (owner == null ||
                !owner.IsAlive ||
                amount <= 0)
            {
                return;
            }

            stability =
                Mathf.Clamp(
                    stability + amount,
                    0,
                    MaxStability);

            if (!broken &&
                stability >= MaxStability)
            {
                EnterBrokenState();
            }

            GaugeChanged?.Invoke(this);
        }

        private void OnActionResolved(
            CombatActionDefinition action,
            CombatantRuntime actor,
            CombatantRuntime target,
            CombatActionResult result)
        {
            if (target != owner ||
                action == null ||
                !result.Executed ||
                !result.Hit ||
                result.Damage <= 0)
            {
                return;
            }

            int amount =
                Mathf.Max(
                    0,
                    action.breakPower);

            amount +=
                Mathf.Max(
                    0,
                    result.ComboBreakBonus);

            amount +=
                Mathf.RoundToInt(
                    Mathf.Max(
                        0f,
                        action.pushDistanceMeters) *
                    4f);

            if (action.presentationProfile != null)
            {
                amount +=
                    (int)action
                        .presentationProfile
                        .impactTier * 4;
            }

            AddBreak(amount);
        }

        private void OnElementalReaction(
            ElementalReactionEvent reaction)
        {
            if (owner == null ||
                !owner.IsAlive)
            {
                return;
            }

            float distance =
                Vector3.Distance(
                    owner.transform.position,
                    reaction.Point);

            if (distance >
                reaction.Radius + 0.05f)
            {
                return;
            }

            int amount;

            switch (reaction.Kind)
            {
                case ElementalReactionKind
                    .Combustion:
                    amount = 30;
                    break;

                case ElementalReactionKind
                    .ConductiveSurge:
                    amount = 24;
                    break;

                case ElementalReactionKind
                    .ArcaneResonance:
                    amount = 22;
                    break;

                case ElementalReactionKind
                    .ThermalShock:
                    amount = 18;
                    break;

                case ElementalReactionKind
                    .FlashFreeze:
                    amount = 15;
                    break;

                default:
                    amount = 0;
                    break;
            }

            AddBreak(amount);
        }

        private void EnterBrokenState()
        {
            broken = true;
            brokenTurnConsumed = false;

            while (owner != null &&
                   owner.ReactionsRemaining > 0)
            {
                if (!owner.TrySpendReaction())
                    break;
            }

            Broken?.Invoke(this);
        }

        private void OnTurnStarted(
            CombatantRuntime combatant)
        {
            if (combatant != owner ||
                !broken ||
                brokenTurnConsumed)
            {
                return;
            }

            brokenTurnConsumed = true;

            int apLoss =
                Mathf.Min(
                    brokenActionPointLoss,
                    owner.CurrentActionPoints);

            if (apLoss > 0)
            {
                owner.TrySpendActionPoints(
                    apLoss);
            }

            float movementLoss =
                owner.TotalMovementAvailable *
                brokenMovementLoss;

            if (movementLoss > 0.01f)
            {
                owner.TrySpendMovement(
                    movementLoss);
            }
        }

        private void OnTurnEnded(
            CombatantRuntime combatant)
        {
            if (combatant != owner ||
                !broken ||
                !brokenTurnConsumed)
            {
                return;
            }

            broken = false;
            brokenTurnConsumed = false;

            stability =
                Mathf.RoundToInt(
                    MaxStability *
                    recoveryAfterBreak);

            GaugeChanged?.Invoke(this);
        }

        private void OnOwnerDied(
            CombatantRuntime combatant)
        {
            stability = 0;
            broken = false;
            brokenTurnConsumed = false;

            GaugeChanged?.Invoke(this);
        }
    }
}
