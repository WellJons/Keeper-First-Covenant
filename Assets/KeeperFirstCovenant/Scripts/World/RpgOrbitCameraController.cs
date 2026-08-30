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
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Camera))]
    public sealed class RpgOrbitCameraController :
        MonoBehaviour
    {
        [Header("Orbit")]
        [SerializeField]
        private float yaw = 38f;

        [SerializeField, Range(20f, 78f)]
        private float pitch = 48f;

        [SerializeField, Min(0.5f)]
        private float distance = 10.5f;

        [SerializeField, Min(0.5f)]
        private float minDistance = 4.5f;

        [SerializeField, Min(1f)]
        private float maxDistance = 17f;

        [SerializeField, Min(0.1f)]
        private float pivotHeight = 1.15f;

        [Header("Mouse")]
        [SerializeField, Min(0.01f)]
        private float mouseOrbitSensitivity = 0.24f;

        [SerializeField, Min(0.001f)]
        private float zoomPerScrollUnit = 0.0125f;

        [SerializeField, Min(1f)]
        private float keyboardOrbitSpeed = 90f;

        [Header("Smoothing")]
        [SerializeField, Min(0.01f)]
        private float targetFollowSmoothTime = 0.14f;

        [SerializeField, Min(0.01f)]
        private float cameraPositionSmoothTime = 0.08f;

        [SerializeField, Min(0f)]
        private float targetSnapDistance = 22f;

        [Header("Collision")]
        [SerializeField, Min(0.01f)]
        private float collisionRadius = 0.28f;

        [SerializeField, Min(0.05f)]
        private float collisionPadding = 0.18f;

        [SerializeField, Min(0.5f)]
        private float collisionMinDistance = 1.1f;

        private Camera worldCamera;
        private CombatantRuntime focusTarget;

        private Vector3 currentPivot;
        private Vector3 pivotVelocity;
        private Vector3 cameraVelocity;

        private bool initialized;

        public CombatantRuntime FocusTarget =>
            focusTarget;

        private void Awake()
        {
            worldCamera =
                GetComponent<Camera>();
        }

        private void Start()
        {
            ResolveFocusTarget(true);
        }

        private void LateUpdate()
        {
            if (worldCamera == null)
                return;

            ResolveFocusTarget(false);
            HandleInput();

            if (focusTarget == null)
                return;

            Vector3 desiredPivot =
                focusTarget.transform.position +
                Vector3.up * pivotHeight;

            if (!initialized ||
                Vector3.Distance(
                    currentPivot,
                    desiredPivot) >
                targetSnapDistance)
            {
                currentPivot = desiredPivot;
                pivotVelocity = Vector3.zero;
                cameraVelocity = Vector3.zero;
                initialized = true;
            }
            else
            {
                currentPivot =
                    Vector3.SmoothDamp(
                        currentPivot,
                        desiredPivot,
                        ref pivotVelocity,
                        targetFollowSmoothTime,
                        Mathf.Infinity,
                        Time.unscaledDeltaTime);
            }

            Quaternion orbitRotation =
                Quaternion.Euler(
                    pitch,
                    yaw,
                    0f);

            Vector3 desiredPosition =
                currentPivot +
                orbitRotation *
                (Vector3.back * distance);

            desiredPosition =
                ResolveCollision(
                    currentPivot,
                    desiredPosition);

            transform.position =
                Vector3.SmoothDamp(
                    transform.position,
                    desiredPosition,
                    ref cameraVelocity,
                    cameraPositionSmoothTime,
                    Mathf.Infinity,
                    Time.unscaledDeltaTime);

            Vector3 lookDirection =
                currentPivot -
                transform.position;

            if (lookDirection.sqrMagnitude >
                0.001f)
            {
                Quaternion lookRotation =
                    Quaternion.LookRotation(
                        lookDirection.normalized,
                        Vector3.up);

                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        lookRotation,
                        1f -
                        Mathf.Exp(
                            -18f *
                            Time.unscaledDeltaTime));
            }
        }

        private void HandleInput()
        {
            if (DeveloperMenu.IsOpen ||
                DialogueRunner.IsDialogueActive ||
                InspectionPanelController.IsOpen)
            {
                return;
            }

            Mouse mouse =
                Mouse.current;

            Keyboard keyboard =
                Keyboard.current;

            bool combatActive =
                TurnCombatDirector.Instance != null &&
                TurnCombatDirector.Instance.State ==
                    CombatState.Active;

            if (mouse != null)
            {
                bool pointerOverUi =
                    EventSystem.current != null &&
                    EventSystem.current
                        .IsPointerOverGameObject();

                bool middleOrbit =
                    mouse.middleButton.isPressed;

                bool rightOrbit =
                    !combatActive &&
                    !pointerOverUi &&
                    mouse.rightButton.isPressed;

                if (middleOrbit ||
                    rightOrbit)
                {
                    Vector2 delta =
                        mouse.delta.ReadValue();

                    yaw +=
                        delta.x *
                        mouseOrbitSensitivity;

                    pitch -=
                        delta.y *
                        mouseOrbitSensitivity;

                    pitch =
                        Mathf.Clamp(
                            pitch,
                            24f,
                            72f);
                }

                float scroll =
                    mouse.scroll
                        .ReadValue().y;

                if (Mathf.Abs(scroll) >
                    0.01f)
                {
                    distance -=
                        scroll *
                        zoomPerScrollUnit;

                    distance =
                        Mathf.Clamp(
                            distance,
                            minDistance,
                            maxDistance);
                }
            }

            if (keyboard != null)
            {
                float orbit =
                    0f;

                if (keyboard.qKey.isPressed)
                    orbit -= 1f;

                if (keyboard.eKey.isPressed)
                    orbit += 1f;

                if (Mathf.Abs(orbit) >
                    0.01f)
                {
                    yaw +=
                        orbit *
                        keyboardOrbitSpeed *
                        Time.unscaledDeltaTime;
                }

                if (keyboard.homeKey
                    .wasPressedThisFrame)
                {
                    SnapToFocus();
                }
            }
        }

        private void ResolveFocusTarget(
            bool force)
        {
            CombatantRuntime next = null;

            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            if (director != null &&
                director.State ==
                    CombatState.Active &&
                director.CurrentActor != null &&
                director.CurrentActor.IsAlive)
            {
                next =
                    director.CurrentActor;
            }
            else
            {
                PartySelectionService selection =
                    PartySelectionService.Instance;

                if (selection != null)
                {
                    next =
                        selection
                            .GetSelectedOrDefault();
                }

                if (next == null)
                {
                    next =
                        FindDefaultPartyMember();
                }
            }

            if (!force &&
                next == focusTarget)
            {
                return;
            }

            focusTarget = next;

            if (focusTarget == null)
                return;

            if (!initialized)
            {
                currentPivot =
                    focusTarget.transform.position +
                    Vector3.up * pivotHeight;
            }
        }

        public void SnapToFocus()
        {
            ResolveFocusTarget(true);

            if (focusTarget == null)
                return;

            currentPivot =
                focusTarget.transform.position +
                Vector3.up * pivotHeight;

            pivotVelocity = Vector3.zero;
            cameraVelocity = Vector3.zero;
            initialized = true;

            Quaternion orbitRotation =
                Quaternion.Euler(
                    pitch,
                    yaw,
                    0f);

            transform.position =
                currentPivot +
                orbitRotation *
                (Vector3.back * distance);

            transform.LookAt(
                currentPivot,
                Vector3.up);
        }

        private Vector3 ResolveCollision(
            Vector3 pivot,
            Vector3 desiredPosition)
        {
            Vector3 delta =
                desiredPosition - pivot;

            float desiredDistance =
                delta.magnitude;

            if (desiredDistance <= 0.01f)
                return desiredPosition;

            Vector3 direction =
                delta / desiredDistance;

            RaycastHit[] hits =
                Physics.SphereCastAll(
                    pivot,
                    collisionRadius,
                    direction,
                    desiredDistance,
                    ~0,
                    QueryTriggerInteraction.Ignore);

            float nearest =
                desiredDistance;

            foreach (RaycastHit hit in
                     hits.OrderBy(value =>
                         value.distance))
            {
                if (hit.collider == null)
                    continue;

                CombatantRuntime owner =
                    hit.collider
                        .GetComponentInParent<
                            CombatantRuntime>();

                if (owner != null &&
                    owner == focusTarget)
                {
                    continue;
                }

                nearest =
                    Mathf.Max(
                        collisionMinDistance,
                        hit.distance -
                        collisionPadding);

                break;
            }

            return
                pivot +
                direction * nearest;
        }

        private static CombatantRuntime
            FindDefaultPartyMember()
        {
            return
                FindObjectsByType<
                        CombatantRuntime>()
                    .Where(value =>
                        value != null &&
                        value.IsAlive &&
                        (value.Faction ==
                             CombatFaction.Player ||
                         value.Faction ==
                             CombatFaction.Ally))
                    .OrderBy(value =>
                        value.Faction ==
                            CombatFaction.Player
                            ? 0
                            : 1)
                    .ThenBy(value =>
                        value.Definition != null &&
                        value.Definition.characterId ==
                            "edward"
                            ? 0
                            : 1)
                    .FirstOrDefault();
        }
    }
}
