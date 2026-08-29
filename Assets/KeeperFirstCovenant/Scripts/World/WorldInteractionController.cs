using System;
using System.Linq;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Dialogue;
using KeeperFirstCovenant.Inventory;
using KeeperFirstCovenant.UI;
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
        private WorldInspectable currentInspectable;
        private GameObject currentActor;
        private TacticalGrid3D navigation;
        private Vector3 currentHitPoint;
        private Collider currentCollider;
        private bool currentInRange;
        private float currentDistance;

        public bool HasHoverTarget =>
            currentInteractable != null ||
            currentInspectable != null;

        public IInteractable CurrentInteractable => currentInteractable;
        public WorldInspectable CurrentInspectable => currentInspectable;
        public GameObject CurrentActor => currentActor;
        public Vector3 CurrentHitPoint => currentHitPoint;
        public Collider CurrentCollider => currentCollider;
        public bool CurrentInRange => currentInRange;
        public float CurrentDistance => currentDistance;
        public float MaxInteractionDistance => maxInteractionDistance;

        public string CurrentPrompt =>
            currentInteractable != null
                ? currentInteractable.InteractionPrompt
                : currentInspectable != null
                    ? currentInspectable.DisplayName
                    : string.Empty;

        public string CurrentContextHint
        {
            get
            {
                if (!HasHoverTarget)
                    return string.Empty;

                if (!currentInRange)
                {
                    if (currentInteractable != null &&
                        currentInspectable != null)
                    {
                        return
                            $"ЛКМ — подойти и взаимодействовать   •   " +
                            $"ПКМ — подойти и осмотреть   •   {currentDistance:0.0} м";
                    }

                    if (currentInteractable != null)
                    {
                        return
                            $"ЛКМ — подойти   •   {currentDistance:0.0} м";
                    }

                    if (currentInspectable != null)
                    {
                        return
                            $"ПКМ — подойти и осмотреть   •   {currentDistance:0.0} м";
                    }

                    return
                        $"Слишком далеко   •   {currentDistance:0.0} м";
                }

                string actionHint =
                    string.Empty;

                if (currentInteractable is LockableDoor door)
                {
                    actionHint =
                        door.GetInteractionHint(
                            currentActor);
                }
                else if (currentInteractable is TrapMechanism trap)
                {
                    actionHint =
                        trap.GetInteractionHint(
                            currentActor);
                }
                else if (currentInteractable != null)
                {
                    actionHint =
                        "ЛКМ — " +
                        CurrentPrompt
                            .ToLowerInvariant();
                }

                bool inspectable =
                    currentInspectable != null &&
                    currentInspectable
                        .CanInspect(
                            currentActor);

                if (inspectable)
                {
                    return string.IsNullOrWhiteSpace(
                            actionHint)
                        ? "ПКМ — осмотреть"
                        : actionHint +
                          "   •   ПКМ — осмотреть";
                }

                return actionHint;
            }
        }

        public event Action HoverChanged;

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
        }

        private void Update()
        {
            if (worldCamera == null)
                worldCamera = Camera.main;

            if (DialogueRunner.IsDialogueActive ||
                InspectionPanelController.IsOpen)
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

            if (mouse.rightButton.wasPressedThisFrame &&
                currentInspectable != null &&
                currentActor != null &&
                currentInspectable.CanInspect(
                    currentActor))
            {
                WorldInspectable capturedInspectable =
                    currentInspectable;

                GameObject capturedInspectionActor =
                    currentActor;

                Collider capturedInspectionCollider =
                    currentCollider;

                if (currentInRange)
                {
                    capturedInspectable.Inspect(
                        capturedInspectionActor);
                }
                else
                {
                    TryApproachTarget(
                        capturedInspectionActor,
                        capturedInspectionCollider,
                        () =>
                        {
                            if (capturedInspectable != null &&
                                capturedInspectionActor != null &&
                                capturedInspectable.CanInspect(
                                    capturedInspectionActor))
                            {
                                capturedInspectable.Inspect(
                                    capturedInspectionActor);
                            }
                        });
                }

                return;
            }

            if (!mouse.leftButton.wasPressedThisFrame ||
                currentInteractable == null ||
                currentActor == null ||
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

            IInteractable capturedInteractable =
                currentInteractable;

            GameObject capturedActor =
                currentActor;

            Collider capturedCollider =
                currentCollider;

            if (!currentInRange)
            {
                TryApproachTarget(
                    capturedActor,
                    capturedCollider,
                    () =>
                    {
                        if (capturedActor == null ||
                            capturedInteractable == null)
                        {
                            return;
                        }

                        if (forceDoor &&
                            capturedInteractable is LockableDoor door)
                        {
                            door.TryForceOpen(
                                capturedActor);

                            return;
                        }

                        if (capturedInteractable
                                .CanInteract(
                                    capturedActor))
                        {
                            capturedInteractable
                                .Interact(
                                    capturedActor);
                        }
                    });

                return;
            }

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

        private bool TryApproachTarget(
            GameObject actor,
            Collider targetCollider,
            Action onArrived)
        {
            if (actor == null ||
                targetCollider == null)
            {
                return false;
            }

            if (navigation == null)
            {
                navigation =
                    FindFirstObjectByType<
                        TacticalGrid3D>();
            }

            if (navigation == null)
                return false;

            TacticalUnitMover mover =
                actor.GetComponent<
                    TacticalUnitMover>();

            if (mover == null)
            {
                mover =
                    actor.AddComponent<
                        TacticalUnitMover>();
            }

            Vector3 actorPosition =
                actor.transform.position;

            Vector3 closest =
                targetCollider.ClosestPoint(
                    actorPosition);

            Vector3 away =
                actorPosition -
                closest;

            away.y = 0f;

            if (away.sqrMagnitude <= 0.001f)
            {
                away =
                    actorPosition -
                    targetCollider.bounds.center;

                away.y = 0f;
            }

            if (away.sqrMagnitude <= 0.001f)
                away = -actor.transform.forward;

            float standOff =
                Mathf.Max(
                    0.75f,
                    maxInteractionDistance * 0.72f);

            Vector3 desired =
                closest +
                away.normalized *
                standOff;

            if (!navigation.TryProjectWalkablePoint(
                    desired,
                    out Vector3 projected))
            {
                if (!navigation.TryProjectWalkablePoint(
                        closest,
                        out projected))
                {
                    return false;
                }
            }

            if (mover.IsMoving)
                mover.CancelMovement();

            return mover.TryMoveExploration(
                navigation,
                projected,
                () =>
                {
                    if (actor == null ||
                        targetCollider == null)
                    {
                        return;
                    }

                    Vector3 targetPoint =
                        targetCollider.ClosestPoint(
                            actor.transform.position);

                    float distance =
                        Vector3.Distance(
                            actor.transform.position,
                            targetPoint);

                    if (distance <=
                        maxInteractionDistance +
                        0.35f)
                    {
                        onArrived?.Invoke();
                    }
                });
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

            MonoBehaviour[] behaviours =
                hit.collider
                    .GetComponentsInParent<
                        MonoBehaviour>(true);

            IInteractable interactable =
                behaviours
                    .OfType<IInteractable>()
                    .FirstOrDefault();

            WorldInspectable inspectable =
                hit.collider
                    .GetComponentInParent<
                        WorldInspectable>();

            if (interactable == null &&
                inspectable == null)
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
                currentInspectable !=
                    inspectable ||
                currentCollider != hit.collider ||
                currentActor != actor ||
                currentInRange != inRange;

            currentInteractable =
                interactable;

            currentInspectable =
                inspectable;

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
                currentInspectable == null &&
                currentCollider == null)
            {
                return;
            }

            currentInteractable = null;
            currentInspectable = null;
            currentCollider = null;
            currentActor = null;
            currentHitPoint = Vector3.zero;
            currentDistance = 0f;
            currentInRange = false;

            HoverChanged?.Invoke();
        }
    }
}
