using System;
using System.Linq;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Dialogue;
using KeeperFirstCovenant.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KeeperFirstCovenant.World
{
    public sealed class WorldInteractionController : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private LayerMask interactionMask = ~0;
        [SerializeField, Min(10f)] private float rayDistance = 500f;
        [SerializeField, Min(0.5f)] private float maxInteractionDistance = 4.25f;

        private IInteractable currentInteractable;
        private GameObject currentActor;
        private Vector3 currentHitPoint;
        private Collider currentCollider;
        private bool currentInRange;
        private float currentDistance;

        public bool HasHoverTarget => currentInteractable != null;
        public IInteractable CurrentInteractable => currentInteractable;
        public GameObject CurrentActor => currentActor;
        public Vector3 CurrentHitPoint => currentHitPoint;
        public Collider CurrentCollider => currentCollider;
        public bool CurrentInRange => currentInRange;
        public float CurrentDistance => currentDistance;
        public float MaxInteractionDistance => maxInteractionDistance;

        public string CurrentPrompt =>
            currentInteractable != null
                ? currentInteractable.InteractionPrompt
                : string.Empty;

        public string CurrentContextHint
        {
            get
            {
                if (currentInteractable == null)
                    return string.Empty;

                if (!currentInRange)
                {
                    return
                        $"Слишком далеко   •   {currentDistance:0.0} м";
                }

                if (currentInteractable is LockableDoor door)
                    return door.GetInteractionHint(currentActor);

                if (currentInteractable is TrapMechanism trap)
                {
                    if (trap.IsSpent)
                        return "Ловушка обезврежена";

                    return "ЛКМ — попытаться обезвредить";
                }

                return "ЛКМ — " + CurrentPrompt.ToLowerInvariant();
            }
        }

        public event Action HoverChanged;

        private void Start()
        {
            if (worldCamera == null)
                worldCamera = Camera.main;
        }

        private void Update()
        {
            if (worldCamera == null)
                worldCamera = Camera.main;

            if (DialogueRunner.IsDialogueActive)
            {
                ClearHover();
                return;
            }

            TurnCombatDirector director = TurnCombatDirector.Instance;
            if (director != null &&
                director.State == CombatState.Active)
            {
                ClearHover();
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null ||
                worldCamera == null)
            {
                ClearHover();
                return;
            }

            UpdateHover(
                mouse.position.ReadValue());

            if (!mouse.leftButton.wasPressedThisFrame ||
                currentInteractable == null ||
                currentActor == null ||
                !currentInRange ||
                !currentInteractable.CanInteract(currentActor))
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;

            bool forceDoor =
                keyboard != null &&
                (keyboard.leftShiftKey.isPressed ||
                 keyboard.rightShiftKey.isPressed) &&
                currentInteractable is LockableDoor;

            if (forceDoor)
            {
                ((LockableDoor)currentInteractable)
                    .TryForceOpen(currentActor);

                UpdateHover(
                    mouse.position.ReadValue());

                return;
            }

            currentInteractable.Interact(
                currentActor);

            UpdateHover(
                mouse.position.ReadValue());
        }

        private void UpdateHover(
            Vector2 screenPosition)
        {
            Ray ray =
                worldCamera.ScreenPointToRay(
                    screenPosition);

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    rayDistance,
                    interactionMask,
                    QueryTriggerInteraction.Collide))
            {
                ClearHover();
                return;
            }

            IInteractable interactable =
                hit.collider
                    .GetComponentsInParent<
                        MonoBehaviour>(true)
                    .OfType<IInteractable>()
                    .FirstOrDefault();

            if (interactable == null)
            {
                ClearHover();
                return;
            }

            GameObject actor =
                ResolveInteractionActor();

            float distance =
                actor != null
                    ? Vector3.Distance(
                        actor.transform.position,
                        hit.point)
                    : float.PositiveInfinity;

            bool inRange =
                actor != null &&
                distance <=
                    maxInteractionDistance;

            bool changed =
                !ReferenceEquals(
                    currentInteractable,
                    interactable) ||
                currentCollider != hit.collider ||
                currentActor != actor ||
                currentInRange != inRange;

            currentInteractable =
                interactable;

            currentCollider =
                hit.collider;

            currentActor =
                actor;

            currentHitPoint =
                hit.point;

            currentDistance =
                distance;

            currentInRange =
                inRange;

            if (changed)
                HoverChanged?.Invoke();
        }

        private GameObject ResolveInteractionActor()
        {
            if (currentActor != null)
            {
                CombatantRuntime cached =
                    currentActor
                        .GetComponentInParent<
                            CombatantRuntime>();

                if (cached != null &&
                    cached.IsAlive &&
                    cached.Faction ==
                        CombatFaction.Player &&
                    cached.GetComponent<
                        InventoryComponent>() != null)
                {
                    return cached.gameObject;
                }
            }

            CombatantRuntime[] combatants =
                FindObjectsByType<
                    CombatantRuntime>(
                    FindObjectsSortMode.None);

            CombatantRuntime player =
                combatants.FirstOrDefault(x =>
                    x != null &&
                    x.IsAlive &&
                    x.Faction ==
                        CombatFaction.Player &&
                    x.GetComponent<
                        InventoryComponent>() != null);

            return player != null
                ? player.gameObject
                : null;
        }

        private void ClearHover()
        {
            if (currentInteractable == null &&
                currentCollider == null)
            {
                return;
            }

            currentInteractable = null;
            currentCollider = null;
            currentActor = null;
            currentHitPoint = Vector3.zero;
            currentDistance = 0f;
            currentInRange = false;

            HoverChanged?.Invoke();
        }
    }
}
