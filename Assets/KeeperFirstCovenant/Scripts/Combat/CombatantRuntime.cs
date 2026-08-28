using System;
using System.Collections.Generic;
using KeeperFirstCovenant.Characters;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    public sealed class CombatantRuntime : MonoBehaviour
    {
        [Serializable]
        private sealed class ActiveStatus
        {
            public StatusEffectDefinition definition;
            public int turnsRemaining;
            public int intensity = 1;
        }

        [SerializeField] private CharacterDefinition definition;

        private readonly List<ActiveStatus> _statuses = new List<ActiveStatus>();
        private int _currentHealth;
        private int _currentMana;
        private int _currentActionPoints;
        private float _remainingMovement;
        private int _barrier;
        private bool _deathRaised;

        public CharacterDefinition Definition => definition;
        public CombatFaction Faction => definition != null ? definition.faction : CombatFaction.Neutral;
        public bool IsAlive => _currentHealth > 0;
        public int CurrentHealth => _currentHealth;
        public int CurrentMana => _currentMana;
        public int CurrentActionPoints => _currentActionPoints;
        public float RemainingMovement => _remainingMovement;
        public int Barrier => _barrier;

        public event Action<CombatantRuntime> Changed;
        public event Action<CombatantRuntime, DamagePacket> Damaged;
        public event Action<CombatantRuntime> Died;
        public event Action<CombatantRuntime> TurnStarted;
        public event Action<CombatantRuntime> TurnEnded;

        private void Awake()
        {
            ResetRuntime();
        }

        public void SetDefinition(CharacterDefinition value, bool reset = true)
        {
            definition = value;
            if (reset)
                ResetRuntime();
        }

        public void ResetRuntime()
        {
            _statuses.Clear();
            _barrier = 0;
            _deathRaised = false;

            if (definition == null)
            {
                _currentHealth = 1;
                _currentMana = 0;
                _currentActionPoints = 0;
                _remainingMovement = 0f;
                return;
            }

            _currentHealth = definition.maxHealth;
            _currentMana = definition.maxMana;
            _currentActionPoints = 0;
            _remainingMovement = 0f;
            Changed?.Invoke(this);
        }

        public int RollInitiative()
        {
            int perception = definition != null
                ? definition.GetModifier(AbilityAttribute.Perception)
                : 0;

            int bonus = definition != null ? definition.initiativeBonus : 0;
            return UnityEngine.Random.Range(1, 21) + perception + bonus + GetStatusInitiativeModifier();
        }

        public void BeginTurn()
        {
            if (!IsAlive || definition == null)
                return;

            TickStatuses();

            if (!IsAlive)
                return;

            _currentActionPoints = Mathf.Max(0, definition.actionPoints + GetStatusActionPointModifier());
            _remainingMovement = Mathf.Max(0f, definition.movementMeters * GetStatusMovementMultiplier());
            TurnStarted?.Invoke(this);
            Changed?.Invoke(this);
        }

        public void EndTurn()
        {
            if (!IsAlive)
                return;

            TurnEnded?.Invoke(this);
        }

        public bool TrySpendActionPoints(int amount)
        {
            if (!IsAlive || amount < 0 || _currentActionPoints < amount)
                return false;

            _currentActionPoints -= amount;
            Changed?.Invoke(this);
            return true;
        }

        public bool TrySpendMana(int amount)
        {
            if (!IsAlive || amount < 0 || _currentMana < amount)
                return false;

            _currentMana -= amount;
            Changed?.Invoke(this);
            return true;
        }

        public bool TrySpendMovement(float meters)
        {
            if (!IsAlive || meters < 0f || _remainingMovement + 0.001f < meters)
                return false;

            _remainingMovement -= meters;
            Changed?.Invoke(this);
            return true;
        }

        public void ApplyDamage(DamagePacket packet)
        {
            if (!IsAlive)
                return;

            int remaining = packet.Amount;

            if (_barrier > 0)
            {
                int absorbed = Mathf.Min(_barrier, remaining);
                _barrier -= absorbed;
                remaining -= absorbed;
            }

            if (remaining > 0)
            {
                int mitigation = packet.Type == DamageType.Physical
                    ? GetArmor()
                    : GetMagicGuard();

                int applied = Mathf.Max(1, remaining - mitigation);
                _currentHealth = Mathf.Max(0, _currentHealth - applied);
            }

            Damaged?.Invoke(this, packet);
            Changed?.Invoke(this);

            if (_currentHealth <= 0)
                RaiseDeath();
        }

        public void Heal(int amount)
        {
            if (!IsAlive || definition == null || amount <= 0)
                return;

            _currentHealth = Mathf.Min(definition.maxHealth, _currentHealth + amount);
            Changed?.Invoke(this);
        }

        public void RestoreMana(int amount)
        {
            if (!IsAlive || definition == null || amount <= 0)
                return;

            _currentMana = Mathf.Min(definition.maxMana, _currentMana + amount);
            Changed?.Invoke(this);
        }

        public void AddBarrier(int amount)
        {
            if (!IsAlive || amount <= 0)
                return;

            _barrier += amount;
            Changed?.Invoke(this);
        }

        public void ApplyStatus(StatusEffectDefinition effect, int durationOverride = 0)
        {
            if (effect == null || !IsAlive)
                return;

            int duration = durationOverride > 0
                ? durationOverride
                : effect.defaultDurationTurns;

            ActiveStatus existing = _statuses.Find(x => x.definition == effect);
            if (existing == null)
            {
                _statuses.Add(new ActiveStatus
                {
                    definition = effect,
                    turnsRemaining = duration,
                    intensity = 1
                });

                Changed?.Invoke(this);
                return;
            }

            switch (effect.stacking)
            {
                case StatusStacking.RefreshDuration:
                    existing.turnsRemaining = Mathf.Max(existing.turnsRemaining, duration);
                    break;
                case StatusStacking.StackDuration:
                    existing.turnsRemaining += duration;
                    break;
                case StatusStacking.StackIntensity:
                    existing.intensity++;
                    existing.turnsRemaining = Mathf.Max(existing.turnsRemaining, duration);
                    break;
                case StatusStacking.IgnoreNew:
                    break;
            }

            Changed?.Invoke(this);
        }

        private void TickStatuses()
        {
            for (int i = _statuses.Count - 1; i >= 0; i--)
            {
                ActiveStatus active = _statuses[i];

                if (active.definition != null && active.definition.dealsDamageEachTurn)
                {
                    int amount = active.definition.turnDamage.Roll() * Mathf.Max(1, active.intensity);
                    ApplyDamage(new DamagePacket(amount, active.definition.turnDamageType, gameObject));

                    if (!IsAlive)
                        return;
                }

                active.turnsRemaining--;
                if (active.turnsRemaining <= 0)
                    _statuses.RemoveAt(i);
            }
        }

        private void RaiseDeath()
        {
            if (_deathRaised)
                return;

            _deathRaised = true;
            _currentActionPoints = 0;
            _remainingMovement = 0f;
            Died?.Invoke(this);
        }

        private int GetArmor()
        {
            int value = definition != null ? definition.armor : 0;
            foreach (ActiveStatus status in _statuses)
                if (status.definition != null)
                    value += status.definition.armorModifier * status.intensity;
            return Mathf.Max(0, value);
        }

        private int GetMagicGuard()
        {
            int value = definition != null ? definition.magicGuard : 0;
            foreach (ActiveStatus status in _statuses)
                if (status.definition != null)
                    value += status.definition.magicGuardModifier * status.intensity;
            return Mathf.Max(0, value);
        }

        private int GetStatusInitiativeModifier()
        {
            int value = 0;
            foreach (ActiveStatus status in _statuses)
                if (status.definition != null)
                    value += status.definition.initiativeModifier * status.intensity;
            return value;
        }

        private int GetStatusActionPointModifier()
        {
            int value = 0;
            foreach (ActiveStatus status in _statuses)
                if (status.definition != null)
                    value += status.definition.actionPointModifier * status.intensity;
            return value;
        }

        private float GetStatusMovementMultiplier()
        {
            float value = 1f;
            foreach (ActiveStatus status in _statuses)
            {
                if (status.definition == null)
                    continue;

                value *= Mathf.Max(0f, status.definition.movementMultiplier);
            }
            return value;
        }
    }
}
