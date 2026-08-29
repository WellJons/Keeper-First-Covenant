using System;
using System.Collections.Generic;
using KeeperFirstCovenant.Characters;
using KeeperFirstCovenant.Developer;
using KeeperFirstCovenant.Inventory;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    [Serializable]
    public sealed class CombatantRuntimeSnapshot
    {
        public string characterId;
        public int currentHealth;
        public int currentMana;
        public int barrier;
        public int downedRoundsRemaining;
        public bool isDowned;
        public bool isDead;
    }

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

        private readonly List<ActiveStatus> _statuses =
            new List<ActiveStatus>();

        private int _currentHealth;
        private int _currentMana;
        private int _currentActionPoints;
        private float _remainingMovement;
        private int _barrier;
        private int _reactionsRemaining;
        private float _freeMovementRemaining;
        private bool _freeMovementSuppressesReactions;
        private int _downedRoundsRemaining;
        private bool _isDowned;
        private bool _isDead;
        private bool _deathRaised;

        public CharacterDefinition Definition => definition;
        public CombatFaction Faction =>
            definition != null
                ? definition.faction
                : CombatFaction.Neutral;

        public bool IsAlive =>
            !_isDead &&
            !_isDowned &&
            _currentHealth > 0;

        public bool IsDowned =>
            _isDowned && !_isDead;

        public bool IsDead => _isDead;

        public bool CanBeTargeted => !_isDead;

        public int DownedRoundsRemaining =>
            _downedRoundsRemaining;

        public int CurrentHealth => _currentHealth;
        public int CurrentMana => _currentMana;
        public int CurrentActionPoints => _currentActionPoints;
        public float RemainingMovement => _remainingMovement;
        public float FreeMovementRemaining => _freeMovementRemaining;
        public float TotalMovementAvailable =>
            _remainingMovement + _freeMovementRemaining;

        public bool SuppressOpportunityAttacks =>
            _freeMovementSuppressesReactions &&
            _freeMovementRemaining > 0.01f;

        public int Barrier => _barrier;
        public int ReactionsRemaining => _reactionsRemaining;

        public event Action<CombatantRuntime> Changed;
        public event Action<CombatantRuntime, DamagePacket> Damaged;
        public event Action<CombatantRuntime> Died;
        public event Action<CombatantRuntime> Downed;
        public event Action<CombatantRuntime> TurnStarted;
        public event Action<CombatantRuntime> TurnEnded;

        private void Awake()
        {
            ResetRuntime();
        }

        public void SetDefinition(
            CharacterDefinition value,
            bool reset = true)
        {
            definition = value;
            if (reset)
                ResetRuntime();
        }

        public void ResetRuntime()
        {
            _statuses.Clear();
            _barrier = 0;
            _reactionsRemaining = 0;
            _freeMovementRemaining = 0f;
            _freeMovementSuppressesReactions = false;
            _downedRoundsRemaining = 0;
            _isDowned = false;
            _isDead = false;
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

        public CombatActionDefinition[] GetAvailableActions()
        {
            var actions =
                new List<CombatActionDefinition>();

            if (definition?.startingActions != null)
            {
                foreach (CombatActionDefinition action
                         in definition.startingActions)
                {
                    if (action != null &&
                        !actions.Contains(action))
                    {
                        actions.Add(action);
                    }
                }
            }

            EquipmentComponent equipment =
                GetComponent<EquipmentComponent>();

            equipment?.CollectGrantedActions(actions);

            DeveloperGrantedActions developerActions =
                GetComponent<DeveloperGrantedActions>();

            developerActions?.Collect(actions);

            return actions.ToArray();
        }

        public float GetMovementCapacity()
        {
            if (definition == null)
                return 0f;

            EquipmentComponent equipment =
                GetComponent<EquipmentComponent>();

            float baseMovement =
                definition.movementMeters +
                (equipment != null
                    ? equipment.GetMovementBonus()
                    : 0f);

            ArcaneStrainComponent strain =
                GetComponent<ArcaneStrainComponent>();

            float strainMultiplier =
                strain != null
                    ? strain.GetMovementMultiplier()
                    : 1f;

            return Mathf.Max(
                0f,
                baseMovement *
                GetStatusMovementMultiplier() *
                strainMultiplier);
        }

        public void PrepareForCombat()
        {
            if (!IsAlive)
                return;

            _reactionsRemaining = 1;
            Changed?.Invoke(this);
        }

        public int RollInitiative()
        {
            int perception = definition != null
                ? definition.GetModifier(AbilityAttribute.Perception)
                : 0;

            int bonus =
                definition != null
                    ? definition.initiativeBonus
                    : 0;

            return UnityEngine.Random.Range(1, 21) +
                   perception +
                   bonus +
                   GetStatusInitiativeModifier();
        }

        public void BeginTurn()
        {
            if (!IsAlive || definition == null)
                return;

            TickStatuses();

            if (!IsAlive)
                return;

            ArcaneStrainComponent strain =
                GetComponent<ArcaneStrainComponent>();

            strain?.OnOwnerTurnStarted();

            _freeMovementRemaining = 0f;
            _freeMovementSuppressesReactions = false;

            int strainApPenalty =
                strain != null
                    ? strain.GetActionPointPenalty()
                    : 0;

            _currentActionPoints = Mathf.Max(
                0,
                definition.actionPoints +
                GetStatusActionPointModifier() -
                strainApPenalty);

            _remainingMovement =
                GetMovementCapacity();

            _reactionsRemaining =
                strain != null &&
                strain.BlocksReactions()
                    ? 0
                    : 1;

            TurnStarted?.Invoke(this);
            Changed?.Invoke(this);
        }

        public void EndTurn()
        {
            if (!IsAlive)
                return;

            _freeMovementRemaining = 0f;
            _freeMovementSuppressesReactions = false;

            TurnEnded?.Invoke(this);
            Changed?.Invoke(this);
        }

        public bool TrySpendActionPoints(int amount)
        {
            if (!IsAlive ||
                amount < 0 ||
                _currentActionPoints < amount)
            {
                return false;
            }

            _currentActionPoints -= amount;
            Changed?.Invoke(this);
            return true;
        }

        public bool TrySpendMana(int amount)
        {
            if (!IsAlive ||
                amount < 0 ||
                _currentMana < amount)
            {
                return false;
            }

            _currentMana -= amount;
            Changed?.Invoke(this);
            return true;
        }

        public bool TrySpendMovement(float meters)
        {
            if (!IsAlive ||
                meters < 0f ||
                TotalMovementAvailable + 0.001f < meters)
            {
                return false;
            }

            float remainingCost = meters;

            if (_freeMovementRemaining > 0f)
            {
                float fromFree =
                    Mathf.Min(
                        _freeMovementRemaining,
                        remainingCost);

                _freeMovementRemaining -= fromFree;
                remainingCost -= fromFree;
            }

            if (remainingCost > 0f)
                _remainingMovement -= remainingCost;

            if (_freeMovementRemaining <= 0.01f)
            {
                _freeMovementRemaining = 0f;
                _freeMovementSuppressesReactions = false;
            }

            Changed?.Invoke(this);
            return true;
        }

        public void GrantFreeMovement(
            float meters,
            bool suppressOpportunityAttacks)
        {
            if (!IsAlive || meters <= 0f)
                return;

            _freeMovementRemaining += meters;

            if (suppressOpportunityAttacks)
                _freeMovementSuppressesReactions = true;

            Changed?.Invoke(this);
        }

        public void ApplyCurrentStrainRestrictions()
        {
            if (!IsAlive)
                return;

            ArcaneStrainComponent strain =
                GetComponent<ArcaneStrainComponent>();

            if (strain == null)
                return;

            _remainingMovement =
                Mathf.Min(
                    _remainingMovement,
                    GetMovementCapacity());

            if (strain.BlocksReactions())
                _reactionsRemaining = 0;

            Changed?.Invoke(this);
        }

        public bool TrySpendReaction()
        {
            if (!IsAlive || _reactionsRemaining <= 0)
                return false;

            _reactionsRemaining--;
            Changed?.Invoke(this);
            return true;
        }

        public CombatantRuntimeSnapshot CaptureRuntimeSnapshot()
        {
            return new CombatantRuntimeSnapshot
            {
                characterId = definition != null
                    ? definition.characterId
                    : string.Empty,
                currentHealth = _currentHealth,
                currentMana = _currentMana,
                barrier = _barrier,
                downedRoundsRemaining = _downedRoundsRemaining,
                isDowned = _isDowned,
                isDead = _isDead
            };
        }

        public void RestoreRuntimeSnapshot(
            CombatantRuntimeSnapshot snapshot)
        {
            if (snapshot == null)
                return;

            _statuses.Clear();
            _freeMovementRemaining = 0f;
            _freeMovementSuppressesReactions = false;
            _currentActionPoints = 0;
            _remainingMovement = 0f;
            _reactionsRemaining = 0;

            int maxHealth = definition != null
                ? Mathf.Max(1, definition.maxHealth)
                : 1;

            int maxMana = definition != null
                ? Mathf.Max(0, definition.maxMana)
                : 0;

            _currentHealth = Mathf.Clamp(
                snapshot.currentHealth,
                0,
                maxHealth);

            _currentMana = Mathf.Clamp(
                snapshot.currentMana,
                0,
                maxMana);

            _barrier = Mathf.Max(0, snapshot.barrier);
            _isDead = snapshot.isDead;
            _isDowned = !_isDead && snapshot.isDowned;
            _downedRoundsRemaining = _isDowned
                ? Mathf.Max(1, snapshot.downedRoundsRemaining)
                : 0;

            if (_isDead || _isDowned)
                _currentHealth = 0;
            else if (_currentHealth <= 0)
                _currentHealth = 1;

            _deathRaised = _isDead;
            Changed?.Invoke(this);
        }

        public void DebugRestoreFull()
        {
            if (definition == null)
                return;

            _deathRaised = false;
            _isDead = false;
            _isDowned = false;
            _downedRoundsRemaining = 0;
            _currentHealth = definition.maxHealth;
            _currentMana = definition.maxMana;
            _barrier = 0;
            DebugRestoreTurnResources();
            Changed?.Invoke(this);
        }

        public void DebugRestoreTurnResources()
        {
            if (definition == null || !IsAlive)
                return;

            _currentActionPoints = Mathf.Max(
                0,
                definition.actionPoints +
                GetStatusActionPointModifier());

            _remainingMovement =
                GetMovementCapacity();

            _freeMovementRemaining = 0f;
            _freeMovementSuppressesReactions = false;

            ArcaneStrainComponent strain =
                GetComponent<ArcaneStrainComponent>();

            _reactionsRemaining =
                strain != null &&
                strain.BlocksReactions()
                    ? 0
                    : 1;

            Changed?.Invoke(this);
        }

        public void DebugKill()
        {
            if (!IsAlive)
                return;

            _currentHealth = 0;
            _currentActionPoints = 0;
            _remainingMovement = 0f;
            _reactionsRemaining = 0;
            Changed?.Invoke(this);
            RaiseDeath();
        }

        public int GetDamageMitigation(
            DamageType damageType)
        {
            return damageType ==
                       DamageType.Physical
                ? GetArmor()
                : GetMagicGuard();
        }

        public float GetDamageMultiplier(
            DamageType damageType)
        {
            return definition != null
                ? definition.GetDamageMultiplier(
                    damageType)
                : 1f;
        }

        public void ApplyDamage(DamagePacket packet)
        {
            if (_isDead)
                return;

            if (_isDowned)
            {
                RaiseDeath();
                return;
            }

            if (!IsAlive)
                return;

            float multiplier =
                GetDamageMultiplier(packet.Type);

            int remaining =
                Mathf.Max(
                    0,
                    Mathf.RoundToInt(
                        packet.Amount *
                        multiplier));

            if (remaining <= 0)
            {
                Damaged?.Invoke(this, packet);
                Changed?.Invoke(this);
                return;
            }

            if (_barrier > 0)
            {
                int absorbed = Mathf.Min(_barrier, remaining);
                _barrier -= absorbed;
                remaining -= absorbed;
            }

            if (remaining > 0)
            {
                int mitigation =
                    GetDamageMitigation(
                        packet.Type);

                int applied = Mathf.Max(
                    1,
                    remaining - mitigation);

                _currentHealth = Mathf.Max(
                    0,
                    _currentHealth - applied);
            }

            Damaged?.Invoke(this, packet);
            Changed?.Invoke(this);

            if (_currentHealth <= 0)
            {
                if (CanEnterDownedState())
                    EnterDownedState();
                else
                    RaiseDeath();
            }
        }

        public void Heal(int amount)
        {
            if (_isDead ||
                definition == null ||
                amount <= 0)
            {
                return;
            }

            if (_isDowned)
            {
                _isDowned = false;
                _downedRoundsRemaining = 0;

                _currentHealth =
                    Mathf.Clamp(
                        amount,
                        1,
                        definition.maxHealth);

                Changed?.Invoke(this);
                return;
            }

            if (!IsAlive)
                return;

            _currentHealth = Mathf.Min(
                definition.maxHealth,
                _currentHealth + amount);

            Changed?.Invoke(this);
        }

        public void RestoreMana(int amount)
        {
            if (!IsAlive ||
                definition == null ||
                amount <= 0)
            {
                return;
            }

            _currentMana = Mathf.Min(
                definition.maxMana,
                _currentMana + amount);

            Changed?.Invoke(this);
        }

        public void AddBarrier(int amount)
        {
            if (!IsAlive || amount <= 0)
                return;

            _barrier += amount;
            Changed?.Invoke(this);
        }

        public void ApplyStatus(
            StatusEffectDefinition effect,
            int durationOverride = 0)
        {
            if (effect == null || !IsAlive)
                return;

            int duration =
                durationOverride > 0
                    ? durationOverride
                    : effect.defaultDurationTurns;

            ActiveStatus existing =
                _statuses.Find(x =>
                    x.definition == effect);

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
                    existing.turnsRemaining =
                        Mathf.Max(
                            existing.turnsRemaining,
                            duration);
                    break;

                case StatusStacking.StackDuration:
                    existing.turnsRemaining += duration;
                    break;

                case StatusStacking.StackIntensity:
                    existing.intensity++;
                    existing.turnsRemaining =
                        Mathf.Max(
                            existing.turnsRemaining,
                            duration);
                    break;

                case StatusStacking.IgnoreNew:
                    break;
            }

            Changed?.Invoke(this);
        }

        public void AdvanceDownedRound()
        {
            if (!IsDowned)
                return;

            _downedRoundsRemaining =
                Mathf.Max(
                    0,
                    _downedRoundsRemaining - 1);

            Changed?.Invoke(this);

            if (_downedRoundsRemaining <= 0)
                RaiseDeath();
        }

        private bool CanEnterDownedState()
        {
            return definition != null &&
                   (Faction == CombatFaction.Player ||
                    Faction == CombatFaction.Ally);
        }

        private void EnterDownedState()
        {
            if (_isDead || _isDowned)
                return;

            _currentHealth = 0;
            _isDowned = true;
            _downedRoundsRemaining =
                Mathf.Max(
                    1,
                    definition != null
                        ? definition.downedRounds
                        : 3);

            _currentActionPoints = 0;
            _remainingMovement = 0f;
            _reactionsRemaining = 0;

            Downed?.Invoke(this);
            Changed?.Invoke(this);
        }

        private void TickStatuses()
        {
            for (int i = _statuses.Count - 1; i >= 0; i--)
            {
                ActiveStatus active = _statuses[i];

                if (active.definition != null &&
                    active.definition.dealsDamageEachTurn)
                {
                    int amount =
                        active.definition.turnDamage.Roll() *
                        Mathf.Max(1, active.intensity);

                    ApplyDamage(new DamagePacket(
                        amount,
                        active.definition.turnDamageType,
                        gameObject));

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
            _isDead = true;
            _isDowned = false;
            _downedRoundsRemaining = 0;
            _currentActionPoints = 0;
            _remainingMovement = 0f;
            _reactionsRemaining = 0;
            Died?.Invoke(this);
        }

        private int GetArmor()
        {
            int value =
                definition != null
                    ? definition.armor
                    : 0;

            foreach (ActiveStatus status in _statuses)
            {
                if (status.definition != null)
                {
                    value +=
                        status.definition.armorModifier *
                        status.intensity;
                }
            }

            EquipmentComponent equipment =
                GetComponent<EquipmentComponent>();

            if (equipment != null)
                value += equipment.GetArmorBonus();

            return Mathf.Max(0, value);
        }

        private int GetMagicGuard()
        {
            int value =
                definition != null
                    ? definition.magicGuard
                    : 0;

            foreach (ActiveStatus status in _statuses)
            {
                if (status.definition != null)
                {
                    value +=
                        status.definition.magicGuardModifier *
                        status.intensity;
                }
            }

            EquipmentComponent equipment =
                GetComponent<EquipmentComponent>();

            if (equipment != null)
                value += equipment.GetMagicGuardBonus();

            return Mathf.Max(0, value);
        }

        private int GetStatusInitiativeModifier()
        {
            int value = 0;

            foreach (ActiveStatus status in _statuses)
            {
                if (status.definition != null)
                {
                    value +=
                        status.definition.initiativeModifier *
                        status.intensity;
                }
            }

            return value;
        }

        private int GetStatusActionPointModifier()
        {
            int value = 0;

            foreach (ActiveStatus status in _statuses)
            {
                if (status.definition != null)
                {
                    value +=
                        status.definition.actionPointModifier *
                        status.intensity;
                }
            }

            return value;
        }

        private float GetStatusMovementMultiplier()
        {
            float value = 1f;

            foreach (ActiveStatus status in _statuses)
            {
                if (status.definition == null)
                    continue;

                value *= Mathf.Max(
                    0f,
                    status.definition.movementMultiplier);
            }

            return value;
        }
    }
}
