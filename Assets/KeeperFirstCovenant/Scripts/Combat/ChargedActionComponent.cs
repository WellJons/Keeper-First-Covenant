using System;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    public readonly struct ChargedActionEvent
    {
        public readonly CombatantRuntime Owner;
        public readonly CombatActionDefinition Action;
        public readonly CombatantRuntime Target;
        public readonly Vector3 Point;
        public readonly int TurnsRemaining;

        public ChargedActionEvent(
            CombatantRuntime owner,
            CombatActionDefinition action,
            CombatantRuntime target,
            Vector3 point,
            int turnsRemaining)
        {
            Owner = owner;
            Action = action;
            Target = target;
            Point = point;
            TurnsRemaining = turnsRemaining;
        }
    }

    [RequireComponent(typeof(CombatantRuntime))]
    public sealed class ChargedActionComponent :
        MonoBehaviour
    {
        private CombatantRuntime owner;
        private CombatActionDefinition action;
        private CombatantRuntime target;
        private Vector3 point;
        private int turnsRemaining;

        public static event Action<ChargedActionEvent>
            ChargeStarted;

        public static event Action<ChargedActionEvent>
            ChargeUpdated;

        public static event Action<ChargedActionEvent>
            ChargeReleased;

        public static event Action<ChargedActionEvent>
            ChargeCancelled;

        public bool HasCharge =>
            action != null &&
            turnsRemaining > 0;

        public CombatActionDefinition Action =>
            action;

        public CombatantRuntime Target =>
            target;

        public Vector3 Point =>
            target != null &&
            action != null &&
            action.targetKind !=
                TargetKind.Ground
                ? target.transform.position
                : point;

        public int TurnsRemaining =>
            Mathf.Max(
                0,
                turnsRemaining);

        private void Awake()
        {
            owner =
                GetComponent<
                    CombatantRuntime>();
        }

        private void OnEnable()
        {
            if (owner == null)
            {
                owner =
                    GetComponent<
                        CombatantRuntime>();
            }

            BreakGaugeComponent.Broken +=
                OnBreak;

            if (owner != null)
            {
                owner.Died +=
                    OnOwnerDied;
            }
        }

        private void OnDisable()
        {
            BreakGaugeComponent.Broken -=
                OnBreak;

            if (owner != null)
            {
                owner.Died -=
                    OnOwnerDied;
            }
        }

        public bool TryBegin(
            CombatActionDefinition definition,
            CombatantRuntime targetCombatant,
            Vector3? groundPoint = null)
        {
            if (definition == null ||
                definition.windUpTurns <= 0 ||
                owner == null ||
                !owner.IsAlive ||
                HasCharge)
            {
                return false;
            }

            if (definition.targetKind ==
                    TargetKind.Ground &&
                !groundPoint.HasValue)
            {
                return false;
            }

            if (definition.targetKind !=
                    TargetKind.Ground &&
                targetCombatant == null)
            {
                return false;
            }

            action = definition;
            target = targetCombatant;
            point =
                groundPoint ??
                (targetCombatant != null
                    ? targetCombatant
                        .transform.position
                    : owner.transform.position);

            turnsRemaining =
                Mathf.Max(
                    1,
                    definition.windUpTurns);

            ChargeStarted?.Invoke(
                BuildEvent());

            return true;
        }

        public bool TryTakeReadyAction(
            out CombatActionDefinition
                readyAction,
            out CombatantRuntime
                readyTarget,
            out Vector3 readyPoint)
        {
            readyAction = null;
            readyTarget = null;
            readyPoint = Vector3.zero;

            if (!HasCharge ||
                owner == null ||
                !owner.IsAlive)
            {
                return false;
            }

            turnsRemaining--;

            if (turnsRemaining > 0)
            {
                ChargeUpdated?.Invoke(
                    BuildEvent());

                return false;
            }

            readyAction = action;
            readyTarget = target;
            readyPoint = Point;

            ChargedActionEvent released =
                new ChargedActionEvent(
                    owner,
                    action,
                    target,
                    readyPoint,
                    0);

            ClearState();

            ChargeReleased?.Invoke(
                released);

            return true;
        }

        public bool CancelCharge()
        {
            if (!HasCharge)
                return false;

            ChargedActionEvent cancelled =
                BuildEvent();

            ClearState();

            ChargeCancelled?.Invoke(
                cancelled);

            return true;
        }

        public void ResetCharge()
        {
            ClearState();
        }

        private void OnBreak(
            BreakGaugeComponent gauge)
        {
            if (!HasCharge ||
                action == null ||
                !action.interruptWindUpOnBreak ||
                gauge == null ||
                gauge.Owner != owner)
            {
                return;
            }

            CancelCharge();
        }

        private void OnOwnerDied(
            CombatantRuntime combatant)
        {
            CancelCharge();
        }

        private ChargedActionEvent BuildEvent()
        {
            return new ChargedActionEvent(
                owner,
                action,
                target,
                Point,
                turnsRemaining);
        }

        private void ClearState()
        {
            action = null;
            target = null;
            point = Vector3.zero;
            turnsRemaining = 0;
        }
    }
}
