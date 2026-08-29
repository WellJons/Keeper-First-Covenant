using System.Linq;
using KeeperFirstCovenant.Combat;
using UnityEngine;

namespace KeeperFirstCovenant.World
{
    [RequireComponent(typeof(Collider))]
    public sealed class SeamlessCombatEncounter : MonoBehaviour
    {
        [SerializeField, Min(1f)]
        private float participantRadius = 24f;

        [SerializeField]
        private bool triggerWhenPartyEnters = true;

        [SerializeField]
        private bool oneShot = false;

        [SerializeField]
        private bool includeNearbyNeutralCombatants;

        private bool _triggered;

        private void Awake()
        {
            Collider trigger =
                GetComponent<Collider>();

            trigger.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!triggerWhenPartyEnters ||
                (oneShot && _triggered))
            {
                return;
            }

            CombatantRuntime entering =
                other.GetComponentInParent<
                    CombatantRuntime>();

            if (entering == null ||
                !entering.IsAlive ||
                (entering.Faction !=
                     CombatFaction.Player &&
                 entering.Faction !=
                     CombatFaction.Ally))
            {
                return;
            }

            BeginEncounter();
        }

        public void BeginEncounter()
        {
            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            if (director == null ||
                director.State ==
                    CombatState.Active)
            {
                return;
            }

            Vector3 center =
                transform.position;

            CombatantRuntime[] participants =
                FindObjectsByType<
                        CombatantRuntime>(
                        FindObjectsSortMode.None)
                    .Where(x =>
                        x != null &&
                        x.IsAlive &&
                        Vector3.Distance(
                            x.transform.position,
                            center) <=
                        participantRadius &&
                        (includeNearbyNeutralCombatants ||
                         x.Faction !=
                            CombatFaction.Neutral))
                    .ToArray();

            bool hasParty =
                participants.Any(x =>
                    x.Faction ==
                        CombatFaction.Player ||
                    x.Faction ==
                        CombatFaction.Ally);

            bool hasEnemies =
                participants.Any(x =>
                    x.Faction ==
                        CombatFaction.Enemy);

            if (!hasParty ||
                !hasEnemies)
            {
                return;
            }

            _triggered = true;

            foreach (CombatantRuntime participant
                     in participants)
            {
                TacticalUnitMover mover =
                    participant.GetComponent<
                        TacticalUnitMover>();

                mover?.CancelMovement();
            }

            director.BeginCombat(
                participants);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(
                transform.position,
                participantRadius);
        }
    }
}
