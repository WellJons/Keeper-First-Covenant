using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Inventory;
using UnityEngine;

namespace KeeperFirstCovenant.World
{
    [RequireComponent(typeof(CombatantRuntime))]
    public sealed class CorpseLootOnDeath : MonoBehaviour
    {
        [SerializeField] private LootTableDefinition lootTable;
        [SerializeField] private string corpsePrompt = "Search body";

        private CombatantRuntime _combatant;
        private bool _prepared;

        public void Configure(LootTableDefinition table, string prompt = "Search body")
        {
            lootTable = table;
            corpsePrompt = prompt;
        }

        private void Awake()
        {
            _combatant = GetComponent<CombatantRuntime>();
            _combatant.Died += OnDied;
        }

        private void OnDestroy()
        {
            if (_combatant != null)
                _combatant.Died -= OnDied;
        }

        private void OnDied(CombatantRuntime dead)
        {
            if (_prepared)
                return;

            _prepared = true;

            SearchableLoot searchable = GetComponent<SearchableLoot>();
            if (searchable == null)
                searchable = gameObject.AddComponent<SearchableLoot>();

            searchable.Configure(lootTable, corpsePrompt, true);
        }
    }
}
