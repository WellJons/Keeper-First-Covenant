using System.Linq;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Developer;
using KeeperFirstCovenant.UI;
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
            if (DeveloperMenu.IsOpen ||
                InspectionPanelController.IsOpen)
            {
                return;
            }

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

            CombatantRuntime[] party =
                FindObjectsByType<
                        CombatantRuntime>(
                        FindObjectsSortMode.None)
                    .Where(x =>
                        x != null &&
                        x.IsAlive &&
                        (x.Faction ==
                             CombatFaction.Player ||
                         x.Faction ==
                             CombatFaction.Ally))
                    .ToArray();

            CombatantRuntime leader =
                party
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

            if (leader == null)
                return;

            StealthSignature leaderSignature =
                leader.GetComponent<
                    StealthSignature>();

            if (leaderSignature == null)
            {
                leaderSignature =
                    leader.gameObject
                        .AddComponent<
                            StealthSignature>();
            }

            bool crouch =
                !leaderSignature.IsCrouched;

            foreach (CombatantRuntime member
                     in party)
            {
                StealthSignature signature =
                    member.GetComponent<
                        StealthSignature>();

                if (signature == null)
                {
                    signature =
                        member.gameObject
                            .AddComponent<
                                StealthSignature>();
                }

                signature.SetCrouched(crouch);
            }
        }
    }
}
