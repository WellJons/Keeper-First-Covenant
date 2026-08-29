using System.Collections.Generic;
using System.Linq;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Developer;
using KeeperFirstCovenant.Dialogue;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KeeperFirstCovenant.Player
{
    public sealed class TacticalPlayerController :
        MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField] private Camera worldCamera;
        [SerializeField] private TacticalGrid3D grid;

        [Header("Raycasts")]
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private LayerMask combatantMask = ~0;
        [SerializeField, Min(10f)]
        private float rayDistance = 500f;

        [Header("Free movement")]
        [SerializeField, Min(0.1f)]
        private float destinationOccupancyRadius = 0.55f;

        private CombatActionDefinition _selectedAction;
        private CombatantRuntime _currentActor;
        private TacticalTargetPreview _currentPreview;
        private CombatantRuntime _hoveredTarget;
        private bool _hasHoverPreview;

        private readonly List<Vector3>
            _movementPreviewPath =
                new List<Vector3>();

        private float _movementPreviewCost;
        private bool _movementPreviewValid;
        private Vector3 _movementPreviewDestination;

        public CombatActionDefinition SelectedAction =>
            _selectedAction;

        public CombatantRuntime CurrentActor =>
            _currentActor;

        public TacticalTargetPreview CurrentPreview =>
            _currentPreview;

        public CombatantRuntime HoveredTarget =>
            _hoveredTarget;

        public bool HasHoverPreview =>
            _hasHoverPreview;

        public IReadOnlyList<Vector3>
            MovementPreviewPath =>
                _movementPreviewPath;

        public bool HasMovementPreview =>
            _movementPreviewPath.Count > 0;

        public float MovementPreviewCost =>
            _movementPreviewCost;

        public bool MovementPreviewValid =>
            _movementPreviewValid;

        public Vector3 MovementPreviewDestination =>
            _movementPreviewDestination;

        private void Start()
        {
            if (worldCamera == null)
                worldCamera = Camera.main;

            if (grid == null)
            {
                grid =
                    FindFirstObjectByType<
                        TacticalGrid3D>();
            }

            if (TurnCombatDirector.Instance != null)
            {
                TurnCombatDirector.Instance
                    .CurrentActorChanged +=
                    OnCurrentActorChanged;

                OnCurrentActorChanged(
                    TurnCombatDirector.Instance
                        .CurrentActor);
            }
        }

        private void OnDestroy()
        {
            if (TurnCombatDirector.Instance != null)
            {
                TurnCombatDirector.Instance
                    .CurrentActorChanged -=
                    OnCurrentActorChanged;
            }
        }

        private void Update()
        {
            if (!CanAcceptInput())
            {
                ClearCursorPreview();
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null)
                return;

            HandleHotkeys();

            if (!CanAcceptInput())
            {
                ClearCursorPreview();
                return;
            }

            UpdateCursorPreview(
                mouse.position.ReadValue());

            if (mouse.rightButton
                .wasPressedThisFrame)
            {
                _selectedAction = null;
                ClearCursorPreview();
                return;
            }

            if (!mouse.leftButton
                .wasPressedThisFrame)
            {
                return;
            }

            if (_selectedAction != null)
            {
                TryUseSelectedAction(
                    mouse.position.ReadValue());
            }
            else
            {
                TryMove(
                    mouse.position.ReadValue());
            }
        }

        private bool CanAcceptInput()
        {
            if (DeveloperMenu.IsOpen ||
                DialogueRunner.IsDialogueActive)
            {
                return false;
            }

            if (_currentActor == null ||
                !_currentActor.IsAlive ||
                !IsPartyControlled(
                    _currentActor.Faction))
            {
                return false;
            }

            TacticalUnitMover mover =
                _currentActor
                    .GetComponent<
                        TacticalUnitMover>();

            return mover == null ||
                   !mover.IsMoving;
        }

        private static bool IsPartyControlled(
            CombatFaction faction)
        {
            return faction ==
                       CombatFaction.Player ||
                   faction ==
                       CombatFaction.Ally;
        }

        public bool SelectAction(
            CombatActionDefinition action)
        {
            if (!CanAcceptInput() ||
                action == null ||
                _currentActor == null)
            {
                return false;
            }

            CombatActionDefinition[] actions =
                _currentActor
                    .GetAvailableActions();

            if (actions == null ||
                !actions.Contains(action))
            {
                return false;
            }

            _selectedAction = action;
            ClearCursorPreview();

            if (_selectedAction.targetKind ==
                TargetKind.Self)
            {
                CombatActionResult result =
                    CombatActionExecutor.Execute(
                        _currentActor,
                        _selectedAction,
                        _currentActor);

                if (result.Executed)
                {
                    _selectedAction = null;
                    ClearCursorPreview();
                }

                return result.Executed;
            }

            return true;
        }

        public void CancelSelectedAction()
        {
            _selectedAction = null;
            ClearCursorPreview();
        }

        private void HandleHotkeys()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.spaceKey
                .wasPressedThisFrame)
            {
                _selectedAction = null;
                ClearCursorPreview();

                TurnCombatDirector.Instance
                    ?.EndCurrentTurn();

                return;
            }

            if (keyboard.escapeKey
                .wasPressedThisFrame)
            {
                _selectedAction = null;
                ClearCursorPreview();
                return;
            }

            int index = -1;

            if (keyboard.digit1Key.wasPressedThisFrame)
                index = 0;
            else if (keyboard.digit2Key.wasPressedThisFrame)
                index = 1;
            else if (keyboard.digit3Key.wasPressedThisFrame)
                index = 2;
            else if (keyboard.digit4Key.wasPressedThisFrame)
                index = 3;
            else if (keyboard.digit5Key.wasPressedThisFrame)
                index = 4;
            else if (keyboard.digit6Key.wasPressedThisFrame)
                index = 5;
            else if (keyboard.digit7Key.wasPressedThisFrame)
                index = 6;
            else if (keyboard.digit8Key.wasPressedThisFrame)
                index = 7;

            if (index < 0)
                return;

            CombatActionDefinition[] actions =
                _currentActor.GetAvailableActions();

            if (actions == null ||
                index >= actions.Length)
            {
                return;
            }

            SelectAction(actions[index]);
        }

        private void UpdateCursorPreview(
            Vector2 screenPosition)
        {
            ClearCursorPreview();

            if (worldCamera == null)
                return;

            if (_selectedAction == null)
            {
                UpdateMovementPreview(
                    screenPosition);
                return;
            }

            UpdateActionPreview(
                screenPosition);
        }

        private void UpdateMovementPreview(
            Vector2 screenPosition)
        {
            if (grid == null ||
                _currentActor == null)
            {
                return;
            }

            Ray ray =
                worldCamera.ScreenPointToRay(
                    screenPosition);

            if (!TryGetGroundHit(
                    ray,
                    out RaycastHit groundHit))
            {
                return;
            }

            if (!grid.TryProjectWalkablePoint(
                    groundHit.point,
                    out Vector3 destination))
            {
                return;
            }

            List<Vector3> path =
                grid.FindContinuousPath(
                    _currentActor.transform.position,
                    destination);

            if (path.Count == 0)
                return;

            _movementPreviewPath.AddRange(path);
            _movementPreviewDestination =
                destination;

            _movementPreviewCost =
                grid.CalculatePathLength(
                    path,
                    _currentActor.transform.position);

            _movementPreviewValid =
                _movementPreviewCost > 0.01f &&
                _movementPreviewCost <=
                    _currentActor.TotalMovementAvailable +
                    0.01f &&
                !IsOccupied(destination);
        }

        private void UpdateActionPreview(
            Vector2 screenPosition)
        {
            Ray ray =
                worldCamera.ScreenPointToRay(
                    screenPosition);

            if (_selectedAction.targetKind ==
                TargetKind.Ground)
            {
                if (!TryGetGroundHit(
                        ray,
                        out RaycastHit groundHit))
                {
                    return;
                }

                _currentPreview =
                    CombatTargetingService.Analyze(
                        _currentActor,
                        _selectedAction,
                        null,
                        groundHit.point);

                _hasHoverPreview = true;
                return;
            }

            CombatantRuntime target =
                TryGetCombatant(ray);

            if (target == null)
                return;

            _hoveredTarget = target;

            _currentPreview =
                CombatTargetingService.Analyze(
                    _currentActor,
                    _selectedAction,
                    target);

            _hasHoverPreview = true;
        }

        private void TryMove(
            Vector2 screenPosition)
        {
            if (grid == null ||
                worldCamera == null ||
                _currentActor == null)
            {
                return;
            }

            Ray ray =
                worldCamera.ScreenPointToRay(
                    screenPosition);

            if (!TryGetGroundHit(
                    ray,
                    out RaycastHit hit))
            {
                return;
            }

            if (!grid.TryProjectWalkablePoint(
                    hit.point,
                    out Vector3 destination))
            {
                return;
            }

            if (IsOccupied(destination))
                return;

            List<Vector3> path =
                grid.FindContinuousPath(
                    _currentActor.transform.position,
                    destination);

            if (path.Count == 0)
                return;

            float pathLength =
                grid.CalculatePathLength(
                    path,
                    _currentActor.transform.position);

            if (pathLength >
                _currentActor.TotalMovementAvailable +
                0.01f)
            {
                return;
            }

            TacticalUnitMover mover =
                _currentActor
                    .GetComponent<
                        TacticalUnitMover>();

            if (mover == null)
            {
                mover =
                    _currentActor.gameObject
                        .AddComponent<
                            TacticalUnitMover>();
            }

            if (mover.TryMoveTo(
                    grid,
                    destination))
            {
                ClearCursorPreview();
            }
        }

        private bool IsOccupied(
            Vector3 destination)
        {
            return FindObjectsByType<
                    CombatantRuntime>(
                    FindObjectsSortMode.None)
                .Any(x =>
                    x != null &&
                    x != _currentActor &&
                    x.CanBeTargeted &&
                    Vector3.Distance(
                        x.transform.position,
                        destination) <=
                    destinationOccupancyRadius);
        }

        private void TryUseSelectedAction(
            Vector2 screenPosition)
        {
            if (_selectedAction == null ||
                worldCamera == null)
            {
                return;
            }

            Ray ray =
                worldCamera.ScreenPointToRay(
                    screenPosition);

            if (_selectedAction.targetKind ==
                TargetKind.Ground)
            {
                if (!TryGetGroundHit(
                        ray,
                        out RaycastHit groundHit))
                {
                    return;
                }

                CombatActionResult result =
                    CombatActionExecutor.Execute(
                        _currentActor,
                        _selectedAction,
                        null,
                        groundHit.point);

                if (result.Executed)
                {
                    _selectedAction = null;
                    ClearCursorPreview();
                }

                return;
            }

            CombatantRuntime target =
                TryGetCombatant(ray);

            if (target == null)
                return;

            CombatActionResult targetResult =
                CombatActionExecutor.Execute(
                    _currentActor,
                    _selectedAction,
                    target);

            if (targetResult.Executed)
            {
                _selectedAction = null;
                ClearCursorPreview();
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

        private CombatantRuntime
            TryGetCombatant(Ray ray)
        {
            RaycastHit[] hits =
                Physics.RaycastAll(
                    ray,
                    rayDistance,
                    combatantMask,
                    QueryTriggerInteraction.Ignore);

            foreach (RaycastHit hit in
                     hits.OrderBy(x => x.distance))
            {
                CombatantRuntime combatant =
                    hit.collider
                        .GetComponentInParent<
                            CombatantRuntime>();

                if (combatant != null)
                    return combatant;
            }

            return null;
        }

        private void ClearCursorPreview()
        {
            _hasHoverPreview = false;
            _hoveredTarget = null;
            _currentPreview = default;

            _movementPreviewPath.Clear();
            _movementPreviewCost = 0f;
            _movementPreviewValid = false;
            _movementPreviewDestination =
                Vector3.zero;
        }

        private void OnCurrentActorChanged(
            CombatantRuntime actor)
        {
            _currentActor = actor;
            _selectedAction = null;
            ClearCursorPreview();
        }
    }
}
