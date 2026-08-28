using System.Linq;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Developer;
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

        private CombatActionDefinition _selectedAction;
        private CombatantRuntime _currentActor;
        private TacticalTargetPreview _currentPreview;
        private CombatantRuntime _hoveredTarget;
        private bool _hasHoverPreview;

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
                ClearHoverPreview();
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null)
                return;

            HandleHotkeys();

            if (!CanAcceptInput())
            {
                ClearHoverPreview();
                return;
            }

            UpdateHoverPreview(
                mouse.position.ReadValue());

            if (mouse.rightButton
                .wasPressedThisFrame)
            {
                _selectedAction = null;
                ClearHoverPreview();
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
            if (DeveloperMenu.IsOpen)
                return false;

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

        private void HandleHotkeys()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.spaceKey
                .wasPressedThisFrame)
            {
                _selectedAction = null;
                ClearHoverPreview();

                TurnCombatDirector.Instance
                    ?.EndCurrentTurn();

                return;
            }

            if (keyboard.escapeKey
                .wasPressedThisFrame)
            {
                _selectedAction = null;
                ClearHoverPreview();
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
                _currentActor.Definition
                    ?.startingActions;

            if (actions == null ||
                index >= actions.Length)
            {
                return;
            }

            _selectedAction = actions[index];
        }

        private void UpdateHoverPreview(
            Vector2 screenPosition)
        {
            ClearHoverPreview();

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
                worldCamera == null)
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

            Vector3 destination =
                grid.SnapToCell(hit.point);

            if (IsOccupied(destination))
                return;

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

            mover.TryMoveTo(
                grid,
                destination);
        }

        private bool IsOccupied(
            Vector3 destination)
        {
            float radius =
                grid != null
                    ? grid.CellSize * 0.45f
                    : 0.6f;

            return FindObjectsByType<
                    CombatantRuntime>(
                    FindObjectsSortMode.None)
                .Any(x =>
                    x != null &&
                    x != _currentActor &&
                    x.IsAlive &&
                    Vector3.Distance(
                        x.transform.position,
                        destination) <= radius);
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
                    ClearHoverPreview();
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
                ClearHoverPreview();
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

        private void ClearHoverPreview()
        {
            _hasHoverPreview = false;
            _hoveredTarget = null;
            _currentPreview = default;
        }

        private void OnCurrentActorChanged(
            CombatantRuntime actor)
        {
            _currentActor = actor;
            _selectedAction = null;
            ClearHoverPreview();
        }
    }
}
