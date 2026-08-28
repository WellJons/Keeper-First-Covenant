using System.Linq;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    public sealed class OpportunityAttackSystem : MonoBehaviour
    {
        private void OnEnable()
        {
            TacticalUnitMover.BeforeStep += OnBeforeStep;
        }

        private void OnDisable()
        {
            TacticalUnitMover.BeforeStep -= OnBeforeStep;
        }

        private static void OnBeforeStep(
            CombatantRuntime moving,
            Vector3 from,
            Vector3 to)
        {
            if (moving == null ||
                !moving.IsAlive ||
                TurnCombatDirector.Instance == null ||
                TurnCombatDirector.Instance.State != CombatState.Active)
            {
                return;
            }

            CombatantRuntime[] possibleReactors =
                Object.FindObjectsByType<CombatantRuntime>(
                    FindObjectsSortMode.None);

            foreach (CombatantRuntime reactor in possibleReactors)
            {
                if (reactor == null ||
                    !reactor.IsAlive ||
                    reactor == moving ||
                    reactor.ReactionsRemaining <= 0 ||
                    CombatTargetingService.IsFriendly(
                        reactor.Faction,
                        moving.Faction))
                {
                    continue;
                }

                CombatActionDefinition melee =
                    FindReactionMeleeAction(reactor);

                if (melee == null)
                    continue;

                float before =
                    Vector3.Distance(reactor.transform.position, from);

                float after =
                    Vector3.Distance(reactor.transform.position, to);

                bool wasThreatened =
                    before <= melee.rangeMeters + 0.15f;

                bool leavesThreat =
                    after > melee.rangeMeters + 0.15f;

                if (!wasThreatened || !leavesThreat)
                    continue;

                TacticalTargetPreview preview =
                    CombatTargetingService.Analyze(
                        reactor,
                        melee,
                        moving);

                if (!preview.Valid)
                    continue;

                if (!reactor.TrySpendReaction())
                    continue;

                CombatActionExecutor.ExecuteReaction(
                    reactor,
                    melee,
                    moving);

                if (!moving.IsAlive)
                    break;
            }
        }

        private static CombatActionDefinition FindReactionMeleeAction(
            CombatantRuntime combatant)
        {
            CombatActionDefinition[] actions =
                combatant.GetAvailableActions();

            if (actions == null)
                return null;

            return actions
                .Where(x =>
                    x != null &&
                    x.category == CombatActionCategory.Melee &&
                    x.targetKind == TargetKind.Enemy)
                .OrderByDescending(x => x.rangeMeters)
                .FirstOrDefault();
        }
    }
}
