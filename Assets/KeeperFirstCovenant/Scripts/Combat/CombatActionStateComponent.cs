using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    public readonly struct ComboExecutionContext
    {
        public readonly bool Matched;
        public readonly int Depth;
        public readonly float DamageMultiplier;
        public readonly int BreakBonus;

        public ComboExecutionContext(
            bool matched,
            int depth,
            float damageMultiplier,
            int breakBonus)
        {
            Matched = matched;
            Depth = depth;
            DamageMultiplier = damageMultiplier;
            BreakBonus = breakBonus;
        }

        public static ComboExecutionContext None =>
            new ComboExecutionContext(
                false,
                0,
                1f,
                0);
    }

    public sealed class CombatActionStateComponent :
        MonoBehaviour
    {
        private readonly Dictionary<string, int>
            cooldowns =
                new Dictionary<string, int>(
                    StringComparer.Ordinal);

        private CombatantRuntime owner;
        private string comboTag;
        private int comboTurnsRemaining;
        private int comboDepth;

        public static event Action<
            CombatantRuntime,
            CombatActionDefinition,
            ComboExecutionContext>
            ComboResolved;

        public static event Action<
            CombatantRuntime>
            StateChanged;

        public string ActiveComboTag =>
            comboTag ?? string.Empty;

        public int ComboDepth => comboDepth;

        public bool HasCombo =>
            !string.IsNullOrWhiteSpace(
                comboTag) &&
            comboTurnsRemaining > 0;

        private void Awake()
        {
            owner =
                GetComponent<
                    CombatantRuntime>();
        }

        private void OnEnable()
        {
            if (owner == null)
            {
                owner =
                    GetComponent<
                        CombatantRuntime>();
            }

            if (owner != null)
            {
                owner.TurnStarted +=
                    OnTurnStarted;

                owner.Died +=
                    OnOwnerDied;
            }
        }

        private void OnDisable()
        {
            if (owner != null)
            {
                owner.TurnStarted -=
                    OnTurnStarted;

                owner.Died -=
                    OnOwnerDied;
            }
        }

        public static CombatActionStateComponent
            Ensure(
                CombatantRuntime actor)
        {
            if (actor == null)
                return null;

            CombatActionStateComponent state =
                actor.GetComponent<
                    CombatActionStateComponent>();

            if (state == null)
            {
                state =
                    actor.gameObject
                        .AddComponent<
                            CombatActionStateComponent>();
            }

            return state;
        }

        public int GetCooldownRemaining(
            CombatActionDefinition action)
        {
            string key =
                GetActionKey(action);

            if (string.IsNullOrWhiteSpace(key))
                return 0;

            return cooldowns.TryGetValue(
                key,
                out int remaining)
                    ? Mathf.Max(
                        0,
                        remaining)
                    : 0;
        }

        public bool IsOnCooldown(
            CombatActionDefinition action)
        {
            return
                GetCooldownRemaining(action) >
                0;
        }

        public bool MatchesCombo(
            CombatActionDefinition action)
        {
            if (action == null ||
                string.IsNullOrWhiteSpace(
                    action.comboRequiresTag))
            {
                return false;
            }

            return
                HasCombo &&
                string.Equals(
                    comboTag,
                    action.comboRequiresTag,
                    StringComparison.Ordinal);
        }

        public bool CanUse(
            CombatActionDefinition action,
            out ActionFailureReason failure)
        {
            failure =
                ActionFailureReason.None;

            if (action == null)
            {
                failure =
                    ActionFailureReason.InvalidAction;

                return false;
            }

            if (IsOnCooldown(action))
            {
                failure =
                    ActionFailureReason.ActionOnCooldown;

                return false;
            }

            if (action.comboRequirementMandatory &&
                !string.IsNullOrWhiteSpace(
                    action.comboRequiresTag) &&
                !MatchesCombo(action))
            {
                failure =
                    ActionFailureReason.ComboRequirementMissing;

                return false;
            }

            return true;
        }

        public ComboExecutionContext
            PreviewCombo(
                CombatActionDefinition action)
        {
            if (action == null ||
                !MatchesCombo(action))
            {
                return ComboExecutionContext.None;
            }

            return
                new ComboExecutionContext(
                    true,
                    Mathf.Max(
                        1,
                        comboDepth + 1),
                    Mathf.Max(
                        1f,
                        action.comboDamageMultiplier),
                    Mathf.Max(
                        0,
                        action.comboBreakBonus));
        }

        public ComboExecutionContext
            CommitAction(
                CombatActionDefinition action)
        {
            if (action == null)
                return ComboExecutionContext.None;

            ComboExecutionContext context =
                PreviewCombo(action);

            if (context.Matched &&
                action.consumeComboTag)
            {
                ClearCombo(false);
            }

            if (!string.IsNullOrWhiteSpace(
                    action.comboGrantsTag))
            {
                comboTag =
                    action.comboGrantsTag.Trim();

                comboTurnsRemaining =
                    Mathf.Max(
                        1,
                        action.comboWindowTurns + 1);

                comboDepth =
                    context.Matched
                        ? Mathf.Max(
                            1,
                            context.Depth)
                        : 1;
            }
            else if (context.Matched &&
                     action.consumeComboTag)
            {
                comboDepth = 0;
            }

            if (action.cooldownTurns > 0)
            {
                string key =
                    GetActionKey(action);

                if (!string.IsNullOrWhiteSpace(
                        key))
                {
                    cooldowns[key] =
                        Mathf.Max(
                            cooldowns.TryGetValue(
                                key,
                                out int existing)
                                ? existing
                                : 0,
                            action.cooldownTurns +
                            1);
                }
            }

            if (context.Matched)
            {
                ComboResolved?.Invoke(
                    owner,
                    action,
                    context);
            }

            StateChanged?.Invoke(owner);

            return context;
        }

        public void ResetState()
        {
            cooldowns.Clear();
            ClearCombo(false);

            StateChanged?.Invoke(owner);
        }

        private void OnTurnStarted(
            CombatantRuntime combatant)
        {
            bool changed = false;

            foreach (string key in
                     cooldowns.Keys.ToArray())
            {
                int remaining =
                    cooldowns[key] - 1;

                if (remaining <= 0)
                {
                    cooldowns.Remove(key);
                }
                else
                {
                    cooldowns[key] =
                        remaining;
                }

                changed = true;
            }

            if (comboTurnsRemaining > 0)
            {
                comboTurnsRemaining--;

                if (comboTurnsRemaining <= 0)
                {
                    ClearCombo(false);
                }

                changed = true;
            }

            if (changed)
                StateChanged?.Invoke(owner);
        }

        private void OnOwnerDied(
            CombatantRuntime combatant)
        {
            ResetState();
        }

        private void ClearCombo(
            bool notify)
        {
            comboTag = string.Empty;
            comboTurnsRemaining = 0;
            comboDepth = 0;

            if (notify)
                StateChanged?.Invoke(owner);
        }

        private static string GetActionKey(
            CombatActionDefinition action)
        {
            if (action == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(
                    action.actionId))
            {
                return action.actionId;
            }

            return action.name ?? string.Empty;
        }
    }
}
