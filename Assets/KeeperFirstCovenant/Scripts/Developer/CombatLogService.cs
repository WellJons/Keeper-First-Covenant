using System;
using System.Collections.Generic;
using KeeperFirstCovenant.Combat;
using UnityEngine;

namespace KeeperFirstCovenant.Developer
{
    public sealed class CombatLogService : MonoBehaviour
    {
        public static CombatLogService Instance { get; private set; }

        [SerializeField, Min(20)]
        private int maxEntries = 250;

        private readonly List<string> _entries =
            new List<string>();

        public IReadOnlyList<string> Entries =>
            _entries;

        public event Action Changed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            CombatActionExecutor.ActionResolved +=
                OnActionResolved;

            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            if (director != null)
            {
                director.RoundStarted +=
                    OnRoundStarted;

                director.CurrentActorChanged +=
                    OnActorChanged;

                director.CombatStarted +=
                    OnCombatStarted;

                director.CombatEnded +=
                    OnCombatEnded;
            }

            Add("Combat log ready.");
        }

        private void OnDestroy()
        {
            CombatActionExecutor.ActionResolved -=
                OnActionResolved;

            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            if (director != null)
            {
                director.RoundStarted -=
                    OnRoundStarted;

                director.CurrentActorChanged -=
                    OnActorChanged;

                director.CombatStarted -=
                    OnCombatStarted;

                director.CombatEnded -=
                    OnCombatEnded;
            }

            if (Instance == this)
                Instance = null;
        }

        public void Clear()
        {
            _entries.Clear();
            Changed?.Invoke();
        }

        public void Add(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            _entries.Add(message);

            while (_entries.Count > maxEntries)
                _entries.RemoveAt(0);

            Changed?.Invoke();
        }

        private void OnCombatStarted()
        {
            Add("=== COMBAT START ===");
        }

        private void OnCombatEnded()
        {
            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            Add(
                $"=== COMBAT END: " +
                $"{director?.State} ===");
        }

        private void OnRoundStarted(int round)
        {
            Add($"--- ROUND {round} ---");
        }

        private void OnActorChanged(
            CombatantRuntime actor)
        {
            if (actor?.Definition == null)
                return;

            Add(
                $"> TURN: " +
                $"{actor.Definition.displayName}");
        }

        private void OnActionResolved(
            CombatActionDefinition action,
            CombatantRuntime actor,
            CombatantRuntime target,
            CombatActionResult result)
        {
            string actorName =
                actor?.Definition != null
                    ? actor.Definition.displayName
                    : "Unknown";

            string targetName =
                target?.Definition != null
                    ? target.Definition.displayName
                    : "Ground";

            string outcome =
                result.Hit
                    ? result.Critical
                        ? "CRIT"
                        : "HIT"
                    : "MISS";

            string values = string.Empty;

            if (result.Damage > 0)
                values += $" dmg:{result.Damage}";

            if (result.Healing > 0)
                values += $" heal:{result.Healing}";

            if (result.Barrier > 0)
                values += $" barrier:{result.Barrier}";

            if (result.HitRoll > 0)
            {
                values +=
                    $" roll:{result.HitRoll}/" +
                    $"{result.HitChance}%";
            }

            Add(
                $"{actorName} -> {targetName} | " +
                $"{action?.displayName ?? "Action"} | " +
                $"{outcome}{values}");
        }
    }
}
