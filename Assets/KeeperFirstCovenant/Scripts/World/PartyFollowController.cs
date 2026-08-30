using System.Linq;
using KeeperFirstCovenant.Combat;
using UnityEngine;

namespace KeeperFirstCovenant.World
{
    public sealed class PartyFollowController : MonoBehaviour
    {
        [SerializeField]
        private TacticalGrid3D navigation;

        [SerializeField, Min(0.5f)]
        private float repathInterval = 0.35f;

        [SerializeField, Min(0.2f)]
        private float followTolerance = 0.9f;

        private float _nextUpdate;

        private void Start()
        {
            if (navigation == null)
            {
                navigation =
                    FindFirstObjectByType<
                        TacticalGrid3D>();
            }
        }

        private void Update()
        {
            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            if (director != null &&
                director.State ==
                    CombatState.Active)
            {
                return;
            }

            if (navigation == null ||
                Time.unscaledTime < _nextUpdate)
            {
                return;
            }

            _nextUpdate =
                Time.unscaledTime +
                repathInterval;

            CombatantRuntime[] all =
                FindObjectsByType<
                    CombatantRuntime>(
                    FindObjectsSortMode.None);

            CombatantRuntime leader =
                all
                    .Where(x =>
                        x != null &&
                        x.IsAlive &&
                        x.Faction ==
                            CombatFaction.Player)
                    .OrderBy(x =>
                        x.Definition != null &&
                        x.Definition.characterId ==
                            "edward"
                            ? 0
                            : 1)
                    .FirstOrDefault();

            if (leader == null)
                return;

            CombatantRuntime[] companions =
                all
                    .Where(x =>
                        x != null &&
                        x.IsAlive &&
                        x.Faction ==
                            CombatFaction.Ally)
                    .OrderBy(x =>
                        x.Definition != null
                            ? x.Definition.characterId
                            : x.name)
                    .ThenBy(x =>
                        x.name)
                    .ToArray();

            for (int i = 0;
                 i < companions.Length;
                 i++)
            {
                CombatantRuntime companion =
                    companions[i];

                Vector3 destination =
                    GetFormationPoint(
                        leader.transform,
                        i);

                if (!navigation
                        .TryProjectWalkablePoint(
                            destination,
                            out Vector3 projected))
                {
                    continue;
                }

                float distance =
                    Vector3.Distance(
                        companion.transform.position,
                        projected);

                if (distance <=
                    followTolerance)
                {
                    continue;
                }

                TacticalUnitMover mover =
                    companion.GetComponent<
                        TacticalUnitMover>();

                if (mover == null)
                {
                    mover =
                        companion.gameObject
                            .AddComponent<
                                TacticalUnitMover>();
                }

                if (mover.IsMoving)
                    continue;

                mover.TryMoveExploration(
                    navigation,
                    projected);
            }
        }

        private static Vector3 GetFormationPoint(
            Transform leader,
            int index)
        {
            Vector2[] formation =
            {
                new Vector2(-1.25f, -1.8f),
                new Vector2(1.25f, -1.8f),
                new Vector2(0f, -3.0f),
                new Vector2(-1.8f, -3.2f),
                new Vector2(1.8f, -3.2f)
            };

            Vector2 offset =
                formation[
                    index %
                    formation.Length];

            return leader.position +
                   leader.right * offset.x +
                   leader.forward * offset.y;
        }
    }
}
