using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    public enum CombatState
    {
        Exploration,
        Active,
        Victory,
        Defeat
    }

    [DefaultExecutionOrder(-500)]
    public sealed class TurnCombatDirector : MonoBehaviour
    {
        private sealed class InitiativeEntry
        {
            public CombatantRuntime combatant;
            public int initiative;
        }

        public static TurnCombatDirector Instance { get; private set; }

        private readonly List<CombatantRuntime> _registered = new List<CombatantRuntime>();
        private readonly List<InitiativeEntry> _turnOrder = new List<InitiativeEntry>();

        private int _turnIndex = -1;

        public CombatState State { get; private set; } = CombatState.Exploration;
        public int Round { get; private set; }
        public CombatantRuntime CurrentActor { get; private set; }

        public event Action CombatStarted;
        public event Action CombatEnded;
        public event Action<CombatantRuntime> CurrentActorChanged;
        public event Action<int> RoundStarted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Register(CombatantRuntime combatant)
        {
            if (combatant == null || _registered.Contains(combatant))
                return;

            _registered.Add(combatant);
            combatant.Died += OnCombatantDied;
        }

        public void Unregister(CombatantRuntime combatant)
        {
            if (combatant == null)
                return;

            combatant.Died -= OnCombatantDied;
            _registered.Remove(combatant);

            int removedIndex = _turnOrder.FindIndex(x => x.combatant == combatant);
            if (removedIndex >= 0)
            {
                _turnOrder.RemoveAt(removedIndex);

                if (removedIndex <= _turnIndex)
                    _turnIndex--;
            }
        }

        public void BeginCombat(IEnumerable<CombatantRuntime> participants = null)
        {
            if (State == CombatState.Active)
                return;

            IEnumerable<CombatantRuntime> source = participants ?? _registered;

            _turnOrder.Clear();
            foreach (CombatantRuntime combatant in source)
            {
                if (combatant == null || !combatant.IsAlive || combatant.Faction == CombatFaction.Neutral)
                    continue;

                Register(combatant);
                _turnOrder.Add(new InitiativeEntry
                {
                    combatant = combatant,
                    initiative = combatant.RollInitiative()
                });
            }

            _turnOrder.Sort((a, b) =>
            {
                int initiativeCompare = b.initiative.CompareTo(a.initiative);
                if (initiativeCompare != 0)
                    return initiativeCompare;

                int aPerception = a.combatant.Definition != null ? a.combatant.Definition.perception : 0;
                int bPerception = b.combatant.Definition != null ? b.combatant.Definition.perception : 0;
                return bPerception.CompareTo(aPerception);
            });

            if (_turnOrder.Count == 0)
                return;

            State = CombatState.Active;
            Round = 1;
            _turnIndex = -1;
            CombatStarted?.Invoke();
            RoundStarted?.Invoke(Round);
            AdvanceTurn();
        }

        public void EndCurrentTurn()
        {
            if (State != CombatState.Active || CurrentActor == null)
                return;

            CurrentActor.EndTurn();
            AdvanceTurn();
        }

        private void AdvanceTurn()
        {
            if (TryResolveCombat())
                return;

            if (_turnOrder.Count == 0)
            {
                EndCombat(CombatState.Victory);
                return;
            }

            int safety = _turnOrder.Count + 1;
            while (safety-- > 0)
            {
                _turnIndex++;

                if (_turnIndex >= _turnOrder.Count)
                {
                    _turnIndex = 0;
                    Round++;
                    RoundStarted?.Invoke(Round);
                }

                CombatantRuntime candidate = _turnOrder[_turnIndex].combatant;
                if (candidate == null || !candidate.IsAlive)
                    continue;

                CurrentActor = candidate;
                CurrentActor.BeginTurn();

                // A damage-over-time status can kill an actor at turn start.
                // In that case the death event already advanced the queue.
                if (CurrentActor != candidate || !candidate.IsAlive)
                    return;

                CurrentActorChanged?.Invoke(CurrentActor);
                return;
            }

            TryResolveCombat();
        }

        private void OnCombatantDied(CombatantRuntime combatant)
        {
            if (State != CombatState.Active)
                return;

            if (combatant == CurrentActor)
                AdvanceTurn();
            else
                TryResolveCombat();
        }

        private bool TryResolveCombat()
        {
            bool playersAlive = _turnOrder.Any(x =>
                x.combatant != null &&
                x.combatant.IsAlive &&
                (x.combatant.Faction == CombatFaction.Player || x.combatant.Faction == CombatFaction.Ally));

            bool enemiesAlive = _turnOrder.Any(x =>
                x.combatant != null &&
                x.combatant.IsAlive &&
                x.combatant.Faction == CombatFaction.Enemy);

            if (!playersAlive)
            {
                EndCombat(CombatState.Defeat);
                return true;
            }

            if (!enemiesAlive)
            {
                EndCombat(CombatState.Victory);
                return true;
            }

            return false;
        }

        private void EndCombat(CombatState result)
        {
            State = result;
            CurrentActor = null;
            _turnIndex = -1;
            CombatEnded?.Invoke();
            CurrentActorChanged?.Invoke(null);
        }

        public void ReturnToExploration()
        {
            if (State == CombatState.Active)
                return;

            State = CombatState.Exploration;
            Round = 0;
            _turnOrder.Clear();
            CurrentActor = null;
        }
    }
}
