using System.Linq;
using KeeperFirstCovenant.Combat;
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

        private void Start()
        {
            if (worldCamera == null)
                worldCamera = Camera.main;
        }

        private void Update()
        {
            TurnCombatDirector director = TurnCombatDirector.Instance;
            if (director != null && director.State == CombatState.Active)
                return;

            Mouse mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame || worldCamera == null)
                return;

            Ray ray = worldCamera.ScreenPointToRay(mouse.position.ReadValue());
            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    rayDistance,
                    interactionMask,
                    QueryTriggerInteraction.Collide))
            {
                return;
            }

            IInteractable interactable = hit.collider
                .GetComponentsInParent<MonoBehaviour>(true)
                .OfType<IInteractable>()
                .FirstOrDefault();

            if (interactable == null)
                return;

            GameObject actor = FindInteractionActor();

            if (actor == null ||
                !interactable.CanInteract(actor))
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;

            bool forceDoor =
                keyboard != null &&
                (keyboard.leftShiftKey.isPressed ||
                 keyboard.rightShiftKey.isPressed) &&
                interactable is LockableDoor;

            if (forceDoor)
            {
                ((LockableDoor)interactable)
                    .TryForceOpen(actor);

                return;
            }

            interactable.Interact(actor);
        }

        private static GameObject FindInteractionActor()
        {
            CombatantRuntime[] combatants =
                FindObjectsByType<CombatantRuntime>(FindObjectsSortMode.None);

            CombatantRuntime player = combatants.FirstOrDefault(x =>
                x != null &&
                x.IsAlive &&
                x.Faction == CombatFaction.Player &&
                x.GetComponent<InventoryComponent>() != null);

            return player != null ? player.gameObject : null;
        }
    }
}
