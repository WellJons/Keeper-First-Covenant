using System.Collections;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    public sealed class CombatStartOnPlay : MonoBehaviour
    {
        [SerializeField] private bool startAutomatically = true;
        [SerializeField, Min(0f)] private float startDelay = 0.15f;

        private IEnumerator Start()
        {
            if (!startAutomatically)
                yield break;

            if (startDelay > 0f)
                yield return new WaitForSeconds(startDelay);
            else
                yield return null;

            TurnCombatDirector director = TurnCombatDirector.Instance;
            if (director == null || director.State == CombatState.Active)
                yield break;

            CombatantRuntime[] combatants =
                FindObjectsByType<CombatantRuntime>(FindObjectsSortMode.None);

            director.BeginCombat(combatants);
        }
    }
}
