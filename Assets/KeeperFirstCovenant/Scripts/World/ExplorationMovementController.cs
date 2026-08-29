using System.Linq;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Developer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KeeperFirstCovenant.World
{
    public sealed class ExplorationMovementController :
        MonoBehaviour
    {
        [SerializeField]
        private Camera worldCamera;

        [SerializeField]
        private TacticalGrid3D navigation;

        [SerializeField]
        private CombatantRuntime leader;

        [Header("Raycasts")]
        [SerializeField]
        private LayerMask groundMask = ~0;

        [SerializeField, Min(10f)]
        private float rayDistance = 500f;

        private void Start()
        {
            if (worldCamera == null)
                worldCamera = Camera.main;

            if (navigation == null)
            {
                navigation =
                    FindFirstObjectByType<
                        TacticalGrid3D>();
            }

            ResolveLeader();
        }

        private void Update()
        {
            if (DeveloperMenu.IsOpen)
                return;

            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            if (director != null &&
                (director.State ==
                     CombatState.Active ||
                 director.State ==
                     CombatState.Defeat))
            {
                return;
            }

            if (leader == null ||
                !leader.IsAlive)
            {
                ResolveLeader();
            }

            if (leader == null ||
                worldCamera == null ||
                navigation == null)
            {
                return;
            }

            Mouse mouse = Mouse.current;

            if (mouse == null ||
                !mouse.leftButton
                    .wasPressedThisFrame)
            {
                return;
            }

            Ray ray =
                worldCamera.ScreenPointToRay(
                    mouse.position.ReadValue());

            if (RayHitsInteractable(ray))
                return;

            if (!TryGetGroundHit(
                    ray,
                    out RaycastHit hit))
            {
                return;
            }

            if (!navigation.TryProjectWalkablePoint(
                    hit.point,
                    out Vector3 destination))
            {
                return;
            }

            TacticalUnitMover mover =
                leader.GetComponent<
                    TacticalUnitMover>();

            if (mover == null)
            {
                mover =
                    leader.gameObject
                        .AddComponent<
                            TacticalUnitMover>();
            }

            if (mover.IsMoving)
                mover.CancelMovement();

            mover.TryMoveExploration(
                navigation,
                destination);
        }

        private void ResolveLeader()
        {
            CombatantRuntime[] combatants =
                FindObjectsByType<
                    CombatantRuntime>(
                    FindObjectsSortMode.None);

            leader =
                combatants
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
        }

        private bool TryGetGroundHit(
            Ray ray,
            out RaycastHit groundHit)
        {
            RaycastHit[] hits =
                Physics.RaycastAll(
                    ray,
                    rayDistance,
                    groundMask,
                    QueryTriggerInteraction.Ignore);

            foreach (RaycastHit hit in
                     hits.OrderBy(x => x.distance))
            {
                if (hit.collider
                        .GetComponentInParent<
                            CombatantRuntime>() != null)
                {
                    continue;
                }

                groundHit = hit;
                return true;
            }

            groundHit = default;
            return false;
        }

        private static bool RayHitsInteractable(
            Ray ray)
        {
            RaycastHit[] hits =
                Physics.RaycastAll(
                    ray,
                    500f,
                    ~0,
                    QueryTriggerInteraction.Collide);

            foreach (RaycastHit hit in
                     hits.OrderBy(x => x.distance))
            {
                MonoBehaviour[] behaviours =
                    hit.collider
                        .GetComponentsInParent<
                            MonoBehaviour>(
                            true);

                if (behaviours.Any(x =>
                        x is IInteractable))
                {
                    return true;
                }

                if (hit.collider
                        .GetComponentInParent<
                            CombatantRuntime>() != null)
                {
                    return false;
                }
            }

            return false;
        }
    }
}
