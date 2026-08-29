using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    public sealed class CombatMechanicsRuntimeInstaller :
        MonoBehaviour
    {
        private float nextScan;

        private void Start()
        {
            EnsureCoreSystems();
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

            EnsureCoreSystems();
            EnsureMechanics();
        }

        private void EnsureCoreSystems()
        {
            if (FindFirstObjectByType<
                    ElementalSurfaceSystem>() ==
                null)
            {
                gameObject.AddComponent<
                    ElementalSurfaceSystem>();
            }

            if (FindFirstObjectByType<
                    ForcedMovementSystem>() ==
                null)
            {
                gameObject.AddComponent<
                    ForcedMovementSystem>();
            }

            if (FindFirstObjectByType<
                    OpportunityAttackSystem>() ==
                null)
            {
                gameObject.AddComponent<
                    OpportunityAttackSystem>();
            }

            if (FindFirstObjectByType<
                    CombatPresentationDirector>() ==
                null)
            {
                gameObject.AddComponent<
                    CombatPresentationDirector>();
            }

            if (FindFirstObjectByType<
                    ElementalReactionVfxController>() ==
                null)
            {
                gameObject.AddComponent<
                    ElementalReactionVfxController>();
            }
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
