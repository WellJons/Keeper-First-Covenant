using System.Collections.Generic;
using KeeperFirstCovenant.Combat;
using UnityEngine;

namespace KeeperFirstCovenant.Developer
{
    public sealed class DeveloperGrantedActions : MonoBehaviour
    {
        [SerializeField]
        private List<CombatActionDefinition> actions =
            new List<CombatActionDefinition>();

        public IReadOnlyList<CombatActionDefinition> Actions =>
            actions;

        public void Grant(
            CombatActionDefinition action)
        {
            if (action == null ||
                actions.Contains(action))
            {
                return;
            }

            actions.Add(action);
        }

        public void Clear()
        {
            actions.Clear();
        }

        public void Collect(
            List<CombatActionDefinition> output)
        {
            if (output == null)
                return;

            foreach (CombatActionDefinition action
                     in actions)
            {
                if (action != null &&
                    !output.Contains(action))
                {
                    output.Add(action);
                }
            }
        }
    }
}
