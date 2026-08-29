using System.Linq;
using KeeperFirstCovenant.Combat;
using UnityEngine;

namespace KeeperFirstCovenant.World
{
    public sealed class WorldCombatEngagementService : MonoBehaviour
    {
        public static WorldCombatEngagementService Instance
        {
            get;
            private set;
        }

        [SerializeField, Min(5f)]
        private float defaultParticipantRadius = 24f;

        private void Awake()
        {
            if (Instance != null &&
                Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public bool BeginCombatAt(
            Vector3 center,
            float radius = -1f)
        {
            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            if (director == null ||
                director.State ==
                    CombatState.Active)
            {
                return false;
            }

            float useRadius =
                radius > 0f
                    ? radius
                    : defaultParticipantRadius;

            CombatantRuntime[] participants =
                FindObjectsByType<
                        CombatantRuntime>(
                        FindObjectsSortMode.None)
                    .Where(x =>
                        x != null &&
                        x.IsAlive &&
                        x.Faction !=
                            CombatFaction.Neutral &&
                        Vector3.Distance(
                            x.transform.position,
                            center) <= useRadius)
                    .ToArray();

            bool hasParty =
                participants.Any(x =>
                    x.Faction ==
                        CombatFaction.Player ||
                    x.Faction ==
                        CombatFaction.Ally);

            bool hasEnemy =
                participants.Any(x =>
                    x.Faction ==
                        CombatFaction.Enemy);

            if (!hasParty || !hasEnemy)
                return false;

            foreach (CombatantRuntime participant
                     in participants)
            {
                TacticalUnitMover mover =
                    participant.GetComponent<
                        TacticalUnitMover>();

                mover?.CancelMovement();
            }

            director.BeginCombat(participants);
            return true;
        }
    }
}
