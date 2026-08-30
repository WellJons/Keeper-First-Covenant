using System.Linq;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Developer;
using KeeperFirstCovenant.Dialogue;
using KeeperFirstCovenant.UI;
using UnityEngine;
using UnityEngine.EventSystems;
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

            PartySelectionService.SelectionChanged +=
                OnSelectionChanged;

            ResolveLeader();
        }

        private void OnDestroy()
        {
            PartySelectionService.SelectionChanged -=
                OnSelectionChanged;
        }

        private void Update()
        {
            if (DeveloperMenu.IsOpen ||
                DialogueRunner.IsDialogueActive ||
                InspectionPanelController.IsOpen)
            {
                return;
            }

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

            if (EventSystem.current != null &&
                EventSystem.current
                    .IsPointerOverGameObject())
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
            PartySelectionService selection =
                PartySelectionService.Instance;

            if (selection != null)
            {
                CombatantRuntime selected =
                    selection.GetSelectedOrDefault();

                if (selected != null &&
                    selected.IsAlive)
                {
                    leader = selected;
                    return;
                }
            }

            CombatantRuntime[] combatants =
                FindObjectsByType<
                    CombatantRuntime>();

            leader =
                combatants
                    .Where(x =>
                        x != null &&
                        x.IsAlive &&
                        (x.Faction ==
                             CombatFaction.Player ||
                         x.Faction ==
                             CombatFaction.Ally))
                    .OrderBy(x =>
                        x.Faction ==
                            CombatFaction.Player
                            ? 0
                            : 1)
                    .ThenBy(x =>
                        x.Definition != null &&
                        x.Definition.characterId ==
                            "edward"
                            ? 0
                            : 1)
                    .FirstOrDefault();
        }

        private void OnSelectionChanged(
            CombatantRuntime member)
        {
            if (member != null &&
                member.IsAlive)
            {
                leader = member;
            }
            else
            {
                ResolveLeader();
            }
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
