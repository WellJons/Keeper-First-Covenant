using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    [RequireComponent(typeof(CombatantRuntime))]
    public sealed class CombatAutoRegister : MonoBehaviour
    {
        private CombatantRuntime _combatant;

        private void Start()
        {
            _combatant = GetComponent<CombatantRuntime>();
            TurnCombatDirector.Instance?.Register(_combatant);
        }

        private void OnDestroy()
        {
            if (_combatant != null && TurnCombatDirector.Instance != null)
                TurnCombatDirector.Instance.Unregister(_combatant);
        }
    }
}
