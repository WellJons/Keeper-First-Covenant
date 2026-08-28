using System;
using KeeperFirstCovenant.Characters;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    public enum ActionFailureReason
    {
        None,
        InvalidActor,
        InvalidAction,
        NotActorsTurn,
        InvalidTarget,
        OutOfRange,
        NotEnoughActionPoints,
        NotEnoughMana
    }

    public readonly struct CombatActionResult
    {
        public readonly bool Executed;
        public readonly bool Hit;
        public readonly bool Critical;
        public readonly int Damage;
        public readonly int Healing;
        public readonly int Barrier;
        public readonly int HitRoll;
        public readonly int HitChance;
        public readonly ActionFailureReason Failure;

        public CombatActionResult(
            bool executed,
            bool hit,
            bool critical,
            int damage,
            int healing,
            int barrier,
            int hitRoll,
            int hitChance,
            ActionFailureReason failure)
        {
            Executed = executed;
            Hit = hit;
            Critical = critical;
            Damage = damage;
            Healing = healing;
            Barrier = barrier;
            HitRoll = hitRoll;
            HitChance = hitChance;
            Failure = failure;
        }

        public static CombatActionResult Failed(ActionFailureReason reason)
        {
            return new CombatActionResult(false, false, false, 0, 0, 0, 0, 0, reason);
        }
    }

    public readonly struct SurfaceRequest
    {
        public readonly SurfaceType Type;
        public readonly Vector3 Point;
        public readonly float Radius;
        public readonly int DurationTurns;
        public readonly GameObject Source;

        public SurfaceRequest(
            SurfaceType type,
            Vector3 point,
            float radius,
            int durationTurns,
            GameObject source)
        {
            Type = type;
            Point = point;
            Radius = radius;
            DurationTurns = durationTurns;
            Source = source;
        }
    }

    public static class CombatActionExecutor
    {
        public static event Action<CombatActionDefinition, CombatantRuntime, CombatantRuntime, CombatActionResult> ActionResolved;
        public static event Action<SurfaceRequest> SurfaceRequested;

        public static CombatActionResult Execute(
            CombatantRuntime actor,
            CombatActionDefinition action,
            CombatantRuntime target = null,
            Vector3? groundPoint = null)
        {
            ActionFailureReason validation = Validate(actor, action, target, groundPoint);
            if (validation != ActionFailureReason.None)
                return CombatActionResult.Failed(validation);

            if (!actor.TrySpendActionPoints(action.actionPointCost))
                return CombatActionResult.Failed(ActionFailureReason.NotEnoughActionPoints);

            if (!actor.TrySpendMana(action.manaCost))
            {
                // This should not happen because validation checks mana first.
                // AP is intentionally not refunded here to avoid hiding state bugs.
                return CombatActionResult.Failed(ActionFailureReason.NotEnoughMana);
            }

            int attributeModifier = actor.Definition != null
                ? actor.Definition.GetModifier(action.scalingAttribute)
                : 0;

            int hitChance = Mathf.Clamp(
                action.baseHitChance + attributeModifier * 5,
                5,
                95);

            int hitRoll = 0;
            bool critical = false;
            bool hit = true;

            if (action.requiresAttackRoll)
            {
                hitRoll = UnityEngine.Random.Range(1, 101);
                critical = hitRoll <= 5;
                hit = critical || hitRoll <= hitChance;
            }

            int damage = 0;
            int healing = 0;
            int barrier = 0;

            if (hit && target != null)
            {
                damage = RollScaled(action.damage, attributeModifier, action.scalingMultiplier, critical);
                if (damage > 0)
                    target.ApplyDamage(new DamagePacket(damage, action.damageType, actor.gameObject, critical));

                healing = RollScaled(action.healing, attributeModifier, action.scalingMultiplier, false);
                if (healing > 0)
                    target.Heal(healing);

                barrier = RollScaled(action.barrier, attributeModifier, action.scalingMultiplier, false);
                if (barrier > 0)
                    target.AddBarrier(barrier);

                ApplyStatuses(action, target);
            }

            Vector3 effectPoint = groundPoint
                ?? (target != null ? target.transform.position : actor.transform.position);

            if (hit &&
                action.createsSurface != SurfaceType.None &&
                action.surfaceRadius > 0f)
            {
                SurfaceRequested?.Invoke(new SurfaceRequest(
                    action.createsSurface,
                    effectPoint,
                    action.surfaceRadius,
                    Mathf.Max(1, action.surfaceDurationTurns),
                    actor.gameObject));
            }

            var result = new CombatActionResult(
                true,
                hit,
                critical,
                damage,
                healing,
                barrier,
                hitRoll,
                hitChance,
                ActionFailureReason.None);

            ActionResolved?.Invoke(action, actor, target, result);
            return result;
        }

        private static ActionFailureReason Validate(
            CombatantRuntime actor,
            CombatActionDefinition action,
            CombatantRuntime target,
            Vector3? groundPoint)
        {
            if (actor == null || !actor.IsAlive || actor.Definition == null)
                return ActionFailureReason.InvalidActor;

            if (action == null)
                return ActionFailureReason.InvalidAction;

            TurnCombatDirector director = TurnCombatDirector.Instance;
            if (director != null &&
                director.State == CombatState.Active &&
                director.CurrentActor != actor)
            {
                return ActionFailureReason.NotActorsTurn;
            }

            if (!ValidateTarget(actor, action.targetKind, target, groundPoint))
                return ActionFailureReason.InvalidTarget;

            Vector3 destination = action.targetKind == TargetKind.Ground
                ? groundPoint.Value
                : target != null
                    ? target.transform.position
                    : actor.transform.position;

            float distance = Vector3.Distance(actor.transform.position, destination);
            if (distance > action.rangeMeters + 0.05f)
                return ActionFailureReason.OutOfRange;

            if (actor.CurrentActionPoints < action.actionPointCost)
                return ActionFailureReason.NotEnoughActionPoints;

            if (actor.CurrentMana < action.manaCost)
                return ActionFailureReason.NotEnoughMana;

            return ActionFailureReason.None;
        }

        private static bool ValidateTarget(
            CombatantRuntime actor,
            TargetKind kind,
            CombatantRuntime target,
            Vector3? groundPoint)
        {
            switch (kind)
            {
                case TargetKind.Self:
                    return target == actor;

                case TargetKind.Ally:
                    return target != null &&
                           target.IsAlive &&
                           AreFriendly(actor.Faction, target.Faction);

                case TargetKind.Enemy:
                    return target != null &&
                           target.IsAlive &&
                           !AreFriendly(actor.Faction, target.Faction) &&
                           target.Faction != CombatFaction.Neutral;

                case TargetKind.AnyCombatant:
                    return target != null && target.IsAlive;

                case TargetKind.Ground:
                    return groundPoint.HasValue;

                default:
                    return false;
            }
        }

        private static bool AreFriendly(CombatFaction a, CombatFaction b)
        {
            if (a == CombatFaction.Neutral || b == CombatFaction.Neutral)
                return a == b;

            bool aFriendly = a == CombatFaction.Player || a == CombatFaction.Ally;
            bool bFriendly = b == CombatFaction.Player || b == CombatFaction.Ally;

            return aFriendly == bFriendly;
        }

        private static int RollScaled(
            DiceFormula formula,
            int attributeModifier,
            float scalingMultiplier,
            bool critical)
        {
            if (formula.diceCount <= 0 && formula.flatBonus == 0)
                return 0;

            int value = formula.Roll();

            if (critical && formula.diceCount > 0)
            {
                for (int i = 0; i < formula.diceCount; i++)
                    value += UnityEngine.Random.Range(1, Mathf.Max(2, formula.dieSides) + 1);
            }

            value += Mathf.RoundToInt(attributeModifier * scalingMultiplier);
            return Mathf.Max(0, value);
        }

        private static void ApplyStatuses(
            CombatActionDefinition action,
            CombatantRuntime target)
        {
            if (action.statusApplications == null)
                return;

            foreach (StatusApplication application in action.statusApplications)
            {
                if (application.effect == null)
                    continue;

                if (UnityEngine.Random.value > application.chance)
                    continue;

                target.ApplyStatus(application.effect, application.durationOverride);
            }
        }
    }
}
