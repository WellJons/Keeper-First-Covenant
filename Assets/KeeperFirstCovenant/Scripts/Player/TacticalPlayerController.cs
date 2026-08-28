using KeeperFirstCovenant.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KeeperFirstCovenant.Player
{
    public sealed class TacticalPlayerController : MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField] private Camera worldCamera;
        [SerializeField] private TacticalGrid3D grid;

        [Header("Raycasts")]
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private LayerMask combatantMask = ~0;
        [SerializeField, Min(10f)] private float rayDistance = 500f;

        private CombatActionDefinition _selectedAction;
        private CombatantRuntime _currentActor;

        public CombatActionDefinition SelectedAction => _selectedAction;
        public CombatantRuntime CurrentActor => _currentActor;

        private void Start()
        {
            if (worldCamera == null)
                worldCamera = Camera.main;

            if (grid == null)
                grid = FindFirstObjectByType<TacticalGrid3D>();

            if (TurnCombatDirector.Instance != null)
            {
                TurnCombatDirector.Instance.CurrentActorChanged += OnCurrentActorChanged;
                OnCurrentActorChanged(TurnCombatDirector.Instance.CurrentActor);
            }
        }

        private void OnDestroy()
        {
            if (TurnCombatDirector.Instance != null)
                TurnCombatDirector.Instance.CurrentActorChanged -= OnCurrentActorChanged;
        }

        private void Update()
        {
            if (!CanAcceptInput())
                return;

            HandleHotkeys();

            Mouse mouse = Mouse.current;
            if (mouse == null)
                return;

            if (mouse.rightButton.wasPressedThisFrame)
            {
                _selectedAction = null;
                return;
            }

            if (!mouse.leftButton.wasPressedThisFrame)
                return;

            if (_selectedAction != null)
                TryUseSelectedAction(mouse.position.ReadValue());
            else
                TryMove(mouse.position.ReadValue());
        }

        private bool CanAcceptInput()
        {
            if (_currentActor == null ||
                !_currentActor.IsAlive ||
                _currentActor.Faction != CombatFaction.Player)
            {
                return false;
            }

            TacticalUnitMover mover = _currentActor.GetComponent<TacticalUnitMover>();
            return mover == null || !mover.IsMoving;
        }

        private void HandleHotkeys()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                _selectedAction = null;
                TurnCombatDirector.Instance?.EndCurrentTurn();
                return;
            }

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                _selectedAction = null;
                return;
            }

            int index = -1;

            if (keyboard.digit1Key.wasPressedThisFrame) index = 0;
            else if (keyboard.digit2Key.wasPressedThisFrame) index = 1;
            else if (keyboard.digit3Key.wasPressedThisFrame) index = 2;
            else if (keyboard.digit4Key.wasPressedThisFrame) index = 3;
            else if (keyboard.digit5Key.wasPressedThisFrame) index = 4;
            else if (keyboard.digit6Key.wasPressedThisFrame) index = 5;
            else if (keyboard.digit7Key.wasPressedThisFrame) index = 6;
            else if (keyboard.digit8Key.wasPressedThisFrame) index = 7;

            if (index < 0)
                return;

            CombatActionDefinition[] actions = _currentActor.Definition?.startingActions;
            if (actions == null || index >= actions.Length)
                return;

            _selectedAction = actions[index];
        }

        private void TryMove(Vector2 screenPosition)
        {
            if (grid == null || worldCamera == null)
                return;

            Ray ray = worldCamera.ScreenPointToRay(screenPosition);
            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    rayDistance,
                    groundMask,
                    QueryTriggerInteraction.Ignore))
            {
                return;
            }

            TacticalUnitMover mover = _currentActor.GetComponent<TacticalUnitMover>();
            if (mover == null)
                mover = _currentActor.gameObject.AddComponent<TacticalUnitMover>();

            mover.TryMoveTo(grid, grid.SnapToCell(hit.point), this);
        }

        private void TryUseSelectedAction(Vector2 screenPosition)
        {
            if (_selectedAction == null || worldCamera == null)
                return;

            Ray ray = worldCamera.ScreenPointToRay(screenPosition);

            if (_selectedAction.targetKind == TargetKind.Ground)
            {
                if (!Physics.Raycast(
                        ray,
                        out RaycastHit groundHit,
                        rayDistance,
                        groundMask,
                        QueryTriggerInteraction.Ignore))
                {
                    return;
                }

                CombatActionResult groundResult = CombatActionExecutor.Execute(
                    _currentActor,
                    _selectedAction,
                    null,
                    groundHit.point);

                if (groundResult.Executed)
                    _selectedAction = null;

                return;
            }

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    rayDistance,
                    combatantMask,
                    QueryTriggerInteraction.Ignore))
            {
                return;
            }

            CombatantRuntime target = hit.collider.GetComponentInParent<CombatantRuntime>();
            if (target == null)
                return;

            CombatActionResult result = CombatActionExecutor.Execute(
                _currentActor,
                _selectedAction,
                target);

            if (result.Executed)
                _selectedAction = null;
        }

        private void OnCurrentActorChanged(CombatantRuntime actor)
        {
            _currentActor = actor;
            _selectedAction = null;
        }
    }
}
