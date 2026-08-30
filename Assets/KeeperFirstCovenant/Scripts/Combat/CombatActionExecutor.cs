using System;
using System.Collections.Generic;
using System.Linq;
using KeeperFirstCovenant.World;
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
        NotEnoughMana,
        MissingStrainResource,
        NotEnoughStrainCapacity,
        ActionOnCooldown,
        ComboRequirementMissing
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
        public readonly ActiveDefenseOutcome DefenseOutcome;
        public readonly bool ComboTriggered;
        public readonly int ComboDepth;
        public readonly int ComboBreakBonus;

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
            ActionFailureReason failure,
            ActiveDefenseOutcome defenseOutcome =
                ActiveDefenseOutcome.None,
            bool comboTriggered = false,
            int comboDepth = 0,
            int comboBreakBonus = 0)
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
            DefenseOutcome = defenseOutcome;
            ComboTriggered = comboTriggered;
            ComboDepth = comboDepth;
            ComboBreakBonus = comboBreakBonus;
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

        public static event Action<
            CombatPresentationRequest>
            ActionPresentationRequested;

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
                false,
                ActiveDefenseOutcome.None);
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
                true,
                ActiveDefenseOutcome.None);
        }

        public static CombatActionResult ExecuteWithDefense(
            CombatantRuntime actor,
            CombatActionDefinition action,
            CombatantRuntime target,
            ActiveDefenseOutcome defenseOutcome)
        {
            return ExecuteInternal(
                actor,
                action,
                target,
                null,
                false,
                defenseOutcome);
        }

        private static CombatActionResult ExecuteInternal(
            CombatantRuntime actor,
            CombatActionDefinition action,
            CombatantRuntime target,
            Vector3? groundPoint,
            bool reaction,
            ActiveDefenseOutcome defenseOutcome)
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

            CombatActionStateComponent actionState =
                null;

            if (!reaction)
            {
                actionState =
                    CombatActionStateComponent
                        .Ensure(actor);

                if (actionState != null &&
                    !actionState.CanUse(
                        action,
                        out ActionFailureReason
                            stateFailure))
                {
                    return CombatActionResult.Failed(
                        stateFailure);
                }

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

                ArcaneStrainComponent strain =
                    null;

                if (action.strainCost > 0)
                {
                    strain =
                        actor.GetComponent<
                            ArcaneStrainComponent>();

                    if (strain == null)
                    {
                        return CombatActionResult.Failed(
                            ActionFailureReason
                                .MissingStrainResource);
                    }

                    if (!strain.CanAccept(
                            action.strainCost))
                    {
                        return CombatActionResult.Failed(
                            ActionFailureReason
                                .NotEnoughStrainCapacity);
                    }

                    if (!strain.TryAdd(
                            action.strainCost))
                    {
                        return CombatActionResult.Failed(
                            ActionFailureReason
                                .NotEnoughStrainCapacity);
                    }

                    actor.ApplyCurrentStrainRestrictions();
                }

                if (!actor.TrySpendActionPoints(
                        action.actionPointCost))
                {
                    strain?.Recover(
                        action.strainCost);

                    return CombatActionResult.Failed(
                        ActionFailureReason
                            .NotEnoughActionPoints);
                }

                if (!actor.TrySpendMana(action.manaCost))
                {
                    strain?.Recover(
                        action.strainCost);

                    return CombatActionResult.Failed(
                        ActionFailureReason
                            .NotEnoughMana);
                }
            }

            ComboExecutionContext comboContext =
                !reaction &&
                actionState != null
                    ? actionState.CommitAction(
                        action)
                    : ComboExecutionContext.None;

            Vector3 effectPoint =
                preview.EffectPoint;

            actor.GetComponent<
                    WorldFacing>()
                ?.FacePoint(
                    effectPoint);

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

                ActiveDefenseOutcome candidateDefense =
                    candidate == target
                        ? defenseOutcome
                        : ActiveDefenseOutcome.None;

                SingleTargetResolution resolved =
                    ResolveTarget(
                        actor,
                        action,
                        candidate,
                        hitChance,
                        candidateDefense,
                        comboContext);

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
                        ActionFailureReason.None,
                        candidateDefense,
                        comboContext.Matched,
                        comboContext.Depth,
                        comboContext.BreakBonus);

                ActionResolved?.Invoke(
                    action,
                    actor,
                    candidate,
                    targetResult);
            }

            if (anyHit &&
                action.damageType !=
                    DamageType.Physical &&
                action.damageType !=
                    DamageType.Bleeding)
            {
                ElementalSurfaceSystem.Instance
                    ?.ReactToImpact(
                        action.damageType,
                        effectPoint,
                        actor.gameObject);
            }

            bool surfaceCanAppear =
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
                ActionFailureReason.None,
                defenseOutcome,
                comboContext.Matched,
                comboContext.Depth,
                comboContext.BreakBonus);

            if (affected.Count == 0)
            {
                ActionResolved?.Invoke(
                    action,
                    actor,
                    null,
                    result);
            }

            if (action.freeMovementMetersGranted > 0f &&
                actor.IsAlive)
            {
                actor.GrantFreeMovement(
                    action.freeMovementMetersGranted,
                    action.freeMovementSuppressesOpportunityAttacks);
            }

            ActionPresentationRequested?.Invoke(
                new CombatPresentationRequest(
                    action,
                    actor,
                    target,
                    actor.transform.position,
                    effectPoint,
                    result));

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
                    candidate.CanBeTargeted &&
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
                int hitChance,
                ActiveDefenseOutcome defenseOutcome,
                ComboExecutionContext comboContext)
        {
            int attributeModifier =
                actor.Definition != null
                    ? actor.Definition.GetModifier(
                        action.scalingAttribute)
                    : 0;

            int hitRoll = 100;
            bool critical = false;
            bool hit = true;

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

                if (comboContext.Matched)
                {
                    damage =
                        Mathf.RoundToInt(
                            damage *
                            Mathf.Max(
                                1f,
                                comboContext
                                    .DamageMultiplier));
                }

                damage =
                    ApplyActiveDefense(
                        damage,
                        defenseOutcome);

                if (damage > 0)
                {
                    target.ApplyDamage(
                        new DamagePacket(
                            damage,
                            action.damageType,
                            actor.gameObject,
                            critical));
                }

                bool avoidsSecondaryEffects =
                    defenseOutcome ==
                        ActiveDefenseOutcome.PerfectDodge ||
                    defenseOutcome ==
                        ActiveDefenseOutcome.PerfectParry;

                if (defenseOutcome ==
                    ActiveDefenseOutcome.PerfectParry)
                {
                    ApplyPerfectParryCounter(
                        actor,
                        target);
                }

                if (target.CanBeTargeted &&
                    !avoidsSecondaryEffects)
                {
                    healing = RollScaled(
                        action.healing,
                        attributeModifier,
                        action.scalingMultiplier,
                        false);

                    if (healing > 0)
                        target.Heal(healing);

                    if (target.IsAlive)
                    {
                        barrier = RollScaled(
                            action.barrier,
                            attributeModifier,
                            action.scalingMultiplier,
                            false);

                        if (barrier > 0)
                            target.AddBarrier(barrier);

                        ApplyStatuses(
                            actor,
                            action,
                            target);
                    }

                    if (target.IsAlive &&
                        action.pushDistanceMeters > 0.01f)
                    {
                        ForcedMovementSystem.Instance?.Push(
                            actor,
                            target,
                            action.pushDistanceMeters,
                            action.pushAwayFromActor);
                    }
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

        private static int ApplyActiveDefense(
            int damage,
            ActiveDefenseOutcome outcome)
        {
            switch (outcome)
            {
                case ActiveDefenseOutcome.Dodge:
                    return Mathf.RoundToInt(
                        damage * 0.45f);

                case ActiveDefenseOutcome.Parry:
                    return Mathf.RoundToInt(
                        damage * 0.25f);

                case ActiveDefenseOutcome.PerfectDodge:
                case ActiveDefenseOutcome.PerfectParry:
                    return 0;

                default:
                    return damage;
            }
        }

        private static void ApplyPerfectParryCounter(
            CombatantRuntime attacker,
            CombatantRuntime defender)
        {
            if (attacker == null ||
                defender == null ||
                !attacker.IsAlive)
            {
                return;
            }

            int finesse =
                defender.Definition != null
                    ? defender.Definition.GetAttribute(
                        AbilityAttribute.Finesse)
                    : 10;

            int counterDamage =
                Mathf.Max(
                    3,
                    2 + finesse / 4);

            attacker.ApplyDamage(
                new DamagePacket(
                    counterDamage,
                    DamageType.Physical,
                    defender.gameObject,
                    true));

            BreakGaugeComponent gauge =
                attacker.GetComponent<
                    BreakGaugeComponent>();

            gauge?.AddBreak(28);
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

            int value =
                formula.DeterministicValue;

            value += Mathf.RoundToInt(
                attributeModifier *
                scalingMultiplier);

            if (critical)
            {
                value =
                    Mathf.RoundToInt(
                        value * 1.5f);
            }

            return Mathf.Max(0, value);
        }

        private static void ApplyStatuses(
            CombatantRuntime actor,
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

                if (application.requiresResistanceCheck)
                {
                    int attackPower =
                        actor.Definition != null
                            ? actor.Definition.GetAttribute(
                                action.scalingAttribute)
                            : 0;

                    attackPower +=
                        Mathf.Max(
                            0,
                            application.statusPower);

                    int resistance =
                        target.Definition != null
                            ? target.Definition.GetAttribute(
                                application.resistanceAttribute)
                            : 0;

                    if (attackPower < resistance)
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
