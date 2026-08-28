using System;
using System.Collections.Generic;
using System.Linq;
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
        NoLineOfSight,
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
        public readonly int AffectedTargets;
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
            int affectedTargets,
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
            AffectedTargets = affectedTargets;
            Failure = failure;
        }

        public static CombatActionResult Failed(
            ActionFailureReason reason)
        {
            return new CombatActionResult(
                false,
                false,
                false,
                0,
                0,
                0,
                0,
                0,
                0,
                reason);
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
        public static event Action<
            CombatActionDefinition,
            CombatantRuntime,
            CombatantRuntime,
            CombatActionResult> ActionResolved;

        public static event Action<SurfaceRequest>
            SurfaceRequested;

        public static CombatActionResult Execute(
            CombatantRuntime actor,
            CombatActionDefinition action,
            CombatantRuntime target = null,
            Vector3? groundPoint = null)
        {
            return ExecuteInternal(
                actor,
                action,
                target,
                groundPoint,
                false);
        }

        public static CombatActionResult ExecuteReaction(
            CombatantRuntime actor,
            CombatActionDefinition action,
            CombatantRuntime target)
        {
            return ExecuteInternal(
                actor,
                action,
                target,
                null,
                true);
        }

        private static CombatActionResult ExecuteInternal(
            CombatantRuntime actor,
            CombatActionDefinition action,
            CombatantRuntime target,
            Vector3? groundPoint,
            bool reaction)
        {
            if (actor == null ||
                !actor.IsAlive ||
                actor.Definition == null)
            {
                return CombatActionResult.Failed(
                    ActionFailureReason.InvalidActor);
            }

            if (action == null)
            {
                return CombatActionResult.Failed(
                    ActionFailureReason.InvalidAction);
            }

            if (!reaction)
            {
                TurnCombatDirector director =
                    TurnCombatDirector.Instance;

                if (director != null &&
                    director.State == CombatState.Active &&
                    director.CurrentActor != actor)
                {
                    return CombatActionResult.Failed(
                        ActionFailureReason.NotActorsTurn);
                }
            }

            TacticalTargetPreview preview =
                CombatTargetingService.Analyze(
                    actor,
                    action,
                    target,
                    groundPoint);

            if (!preview.Valid)
                return CombatActionResult.Failed(
                    preview.Failure);

            if (!reaction)
            {
                if (actor.CurrentActionPoints <
                    action.actionPointCost)
                {
                    return CombatActionResult.Failed(
                        ActionFailureReason
                            .NotEnoughActionPoints);
                }

                if (actor.CurrentMana <
                    action.manaCost)
                {
                    return CombatActionResult.Failed(
                        ActionFailureReason
                            .NotEnoughMana);
                }

                if (!actor.TrySpendActionPoints(
                        action.actionPointCost))
                {
                    return CombatActionResult.Failed(
                        ActionFailureReason
                            .NotEnoughActionPoints);
                }

                if (!actor.TrySpendMana(action.manaCost))
                {
                    return CombatActionResult.Failed(
                        ActionFailureReason
                            .NotEnoughMana);
                }
            }

            Vector3 effectPoint =
                preview.EffectPoint;

            List<CombatantRuntime> affected =
                CollectAffectedTargets(
                    actor,
                    action,
                    target,
                    effectPoint);

            int totalDamage = 0;
            int totalHealing = 0;
            int totalBarrier = 0;
            int firstRoll = 0;
            int firstChance = preview.HitChance;
            bool anyHit = affected.Count == 0 &&
                          !action.requiresAttackRoll;
            bool anyCritical = false;

            if (affected.Count == 0)
            {
                anyHit = true;
            }

            foreach (CombatantRuntime candidate in affected)
            {
                TacticalTargetPreview candidatePreview =
                    action.areaRadius <= 0.01f ||
                    candidate == target
                        ? CombatTargetingService.Analyze(
                            actor,
                            action,
                            candidate,
                            groundPoint)
                        : preview;

                int hitChance =
                    candidatePreview.Valid
                        ? candidatePreview.HitChance
                        : preview.HitChance;

                SingleTargetResolution resolved =
                    ResolveTarget(
                        actor,
                        action,
                        candidate,
                        hitChance);

                if (firstRoll == 0)
                {
                    firstRoll = resolved.hitRoll;
                    firstChance = hitChance;
                }

                anyHit |= resolved.hit;
                anyCritical |= resolved.critical;
                totalDamage += resolved.damage;
                totalHealing += resolved.healing;
                totalBarrier += resolved.barrier;

                var targetResult =
                    new CombatActionResult(
                        true,
                        resolved.hit,
                        resolved.critical,
                        resolved.damage,
                        resolved.healing,
                        resolved.barrier,
                        resolved.hitRoll,
                        hitChance,
                        1,
                        ActionFailureReason.None);

                ActionResolved?.Invoke(
                    action,
                    actor,
                    candidate,
                    targetResult);
            }

            bool surfaceCanAppear =
                !action.requiresAttackRoll ||
                anyHit ||
                action.targetKind == TargetKind.Ground;

            if (surfaceCanAppear &&
                action.createsSurface !=
                SurfaceType.None &&
                action.surfaceRadius > 0f)
            {
                SurfaceRequested?.Invoke(
                    new SurfaceRequest(
                        action.createsSurface,
                        effectPoint,
                        action.surfaceRadius,
                        Mathf.Max(
                            1,
                            action.surfaceDurationTurns),
                        actor.gameObject));
            }

            var result = new CombatActionResult(
                true,
                anyHit,
                anyCritical,
                totalDamage,
                totalHealing,
                totalBarrier,
                firstRoll,
                firstChance,
                affected.Count,
                ActionFailureReason.None);

            if (affected.Count == 0)
            {
                ActionResolved?.Invoke(
                    action,
                    actor,
                    null,
                    result);
            }

            return result;
        }

        private static List<CombatantRuntime>
            CollectAffectedTargets(
                CombatantRuntime actor,
                CombatActionDefinition action,
                CombatantRuntime primaryTarget,
                Vector3 effectPoint)
        {
            if (action.areaRadius <= 0.01f)
            {
                return primaryTarget != null
                    ? new List<CombatantRuntime>
                    {
                        primaryTarget
                    }
                    : new List<CombatantRuntime>();
            }

            if (action.areaTargetRule ==
                AreaTargetRule.PrimaryOnly)
            {
                return primaryTarget != null
                    ? new List<CombatantRuntime>
                    {
                        primaryTarget
                    }
                    : new List<CombatantRuntime>();
            }

            return UnityEngine.Object
                .FindObjectsByType<CombatantRuntime>(
                    FindObjectsSortMode.None)
                .Where(candidate =>
                    candidate != null &&
                    candidate.IsAlive &&
                    Vector3.Distance(
                        candidate.transform.position,
                        effectPoint)
                    <= action.areaRadius + 0.05f &&
                    CombatTargetingService.MatchesAreaRule(
                        actor,
                        candidate,
                        action.areaTargetRule,
                        primaryTarget))
                .ToList();
        }

        private static SingleTargetResolution
            ResolveTarget(
                CombatantRuntime actor,
                CombatActionDefinition action,
                CombatantRuntime target,
                int hitChance)
        {
            int attributeModifier =
                actor.Definition != null
                    ? actor.Definition.GetModifier(
                        action.scalingAttribute)
                    : 0;

            int hitRoll = 0;
            bool critical = false;
            bool hit = true;

            if (action.requiresAttackRoll)
            {
                hitRoll =
                    UnityEngine.Random.Range(1, 101);

                critical = hitRoll <= 5;
                hit =
                    critical ||
                    hitRoll <= hitChance;
            }

            int damage = 0;
            int healing = 0;
            int barrier = 0;

            if (hit && target != null)
            {
                damage = RollScaled(
                    action.damage,
                    attributeModifier,
                    action.scalingMultiplier,
                    critical);

                if (damage > 0)
                {
                    target.ApplyDamage(
                        new DamagePacket(
                            damage,
                            action.damageType,
                            actor.gameObject,
                            critical));
                }

                if (target.IsAlive)
                {
                    healing = RollScaled(
                        action.healing,
                        attributeModifier,
                        action.scalingMultiplier,
                        false);

                    if (healing > 0)
                        target.Heal(healing);

                    barrier = RollScaled(
                        action.barrier,
                        attributeModifier,
                        action.scalingMultiplier,
                        false);

                    if (barrier > 0)
                        target.AddBarrier(barrier);

                    ApplyStatuses(
                        action,
                        target);
                }
            }

            return new SingleTargetResolution
            {
                hit = hit,
                critical = critical,
                hitRoll = hitRoll,
                damage = damage,
                healing = healing,
                barrier = barrier
            };
        }

        private static int RollScaled(
            DiceFormula formula,
            int attributeModifier,
            float scalingMultiplier,
            bool critical)
        {
            if (formula.diceCount <= 0 &&
                formula.flatBonus == 0)
            {
                return 0;
            }

            int value = formula.Roll();

            if (critical &&
                formula.diceCount > 0)
            {
                for (int i = 0;
                     i < formula.diceCount;
                     i++)
                {
                    value +=
                        UnityEngine.Random.Range(
                            1,
                            Mathf.Max(
                                2,
                                formula.dieSides) + 1);
                }
            }

            value += Mathf.RoundToInt(
                attributeModifier *
                scalingMultiplier);

            return Mathf.Max(0, value);
        }

        private static void ApplyStatuses(
            CombatActionDefinition action,
            CombatantRuntime target)
        {
            if (action.statusApplications == null)
                return;

            foreach (StatusApplication application
                     in action.statusApplications)
            {
                if (application.effect == null)
                    continue;

                if (UnityEngine.Random.value >
                    application.chance)
                {
                    continue;
                }

                target.ApplyStatus(
                    application.effect,
                    application.durationOverride);
            }
        }

        private struct SingleTargetResolution
        {
            public bool hit;
            public bool critical;
            public int hitRoll;
            public int damage;
            public int healing;
            public int barrier;
        }
    }
}
