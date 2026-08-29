using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    public sealed class CombatMechanicsRuntimeInstaller :
        MonoBehaviour
    {
        private float nextScan;

        private void Start()
        {
            EnsureMechanics();
        }

        private void Update()
        {
            if (Time.unscaledTime <
                nextScan)
            {
                return;
            }

            nextScan =
                Time.unscaledTime + 0.75f;

            EnsureMechanics();
        }

        private static void EnsureMechanics()
        {
            CombatantRuntime[] combatants =
                FindObjectsByType<
                    CombatantRuntime>(
                    FindObjectsSortMode.None);

            foreach (CombatantRuntime combatant
                     in combatants)
            {
                if (combatant == null)
                    continue;

                if (combatant.GetComponent<
                        BreakGaugeComponent>() ==
                    null)
                {
                    combatant.gameObject
                        .AddComponent<
                            BreakGaugeComponent>();
                }
            }
        }
    }
}
