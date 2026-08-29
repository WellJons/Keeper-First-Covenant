using System.Linq;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Developer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KeeperFirstCovenant.World
{
    public sealed class StealthController : MonoBehaviour
    {
        [SerializeField]
        private Key toggleKey = Key.C;

        private void Update()
        {
            if (DeveloperMenu.IsOpen)
                return;

            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            if (director != null &&
                director.State ==
                    CombatState.Active)
            {
                return;
            }

            Keyboard keyboard =
                Keyboard.current;

            if (keyboard == null ||
                !keyboard[toggleKey]
                    .wasPressedThisFrame)
            {
                return;
            }

            CombatantRuntime leader =
                FindObjectsByType<
                        CombatantRuntime>(
                        FindObjectsSortMode.None)
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

            StealthSignature signature =
                leader.GetComponent<
                    StealthSignature>();

            if (signature == null)
            {
                signature =
                    leader.gameObject
                        .AddComponent<
                            StealthSignature>();
            }

            signature.ToggleCrouched();
        }
    }
}
