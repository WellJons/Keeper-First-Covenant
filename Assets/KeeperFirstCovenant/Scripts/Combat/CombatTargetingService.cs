using System.Linq;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    public readonly struct TacticalTargetPreview
    {
        public readonly bool Valid;
        public readonly ActionFailureReason Failure;
        public readonly bool HasLineOfSight;
        public readonly CoverQuality Cover;
        public readonly float Distance;
        public readonly float HeightDifference;
        public readonly int HeightHitModifier;
        public readonly int CoverHitModifier;
        public readonly int HitChance;
        public readonly int DamageMin;
        public readonly int DamageMax;
        public readonly int AffectedTargets;
        public readonly Vector3 EffectPoint;

        public TacticalTargetPreview(
            bool valid,
            ActionFailureReason failure,
            bool hasLineOfSight,
            CoverQuality cover,
            float distance,
            float heightDifference,
            int heightHitModifier,
            int coverHitModifier,
            int hitChance,
            int damageMin,
            int damageMax,
            int affectedTargets,
            Vector3 effectPoint)
        {
            Valid = valid;
            Failure = failure;
            HasLineOfSight = hasLineOfSight;
            Cover = cover;
            Distance = distance;
            HeightDifference = heightDifference;
            HeightHitModifier = heightHitModifier;
            CoverHitModifier = coverHitModifier;
            HitChance = hitChance;
            DamageMin = damageMin;
            DamageMax = damageMax;
            AffectedTargets = affectedTargets;
            EffectPoint = effectPoint;
        }

        public static TacticalTargetPreview Invalid(
            ActionFailureReason failure,
            Vector3 effectPoint)
        {
            return new TacticalTargetPreview(
                false,
                failure,
                false,
                CoverQuality.Full,
                0f,
                0f,
                0,
                0,
                0,
                0,
                0,
                0,
                effectPoint);
        }
    }

    public static class CombatTargetingService
    {
        public static TacticalTargetPreview Analyze(
            CombatantRuntime actor,
            CombatActionDefinition action,
            CombatantRuntime target = null,
            Vector3? groundPoint = null)
        {
            if (actor == null || !actor.IsAlive || actor.Definition == null)
                return TacticalTargetPreview.Invalid(
                    ActionFailureReason.InvalidActor,
                    actor != null ? actor.transform.position : Vector3.zero);

            if (action == null)
                return TacticalTargetPreview.Invalid(
                    ActionFailureReason.InvalidAction,
                    actor.transform.position);

            if (!IsTargetKindValid(actor, action.targetKind, target, groundPoint))
            {
                return TacticalTargetPreview.Invalid(
                    ActionFailureReason.InvalidTarget,
                    groundPoint ?? (target != null
                        ? target.transform.position
                        : actor.transform.position));
            }

            Vector3 effectPoint = action.targetKind == TargetKind.Ground
                ? groundPoint.Value
                : target != null
                    ? target.transform.position
                    : actor.transform.position;

            float distance = Vector3.Distance(actor.transform.position, effectPoint);
            if (distance > action.rangeMeters + 0.05f)
            {
                return TacticalTargetPreview.Invalid(
                    ActionFailureReason.OutOfRange,
                    effectPoint);
            }

            bool hasLineOfSight = true;
            CoverQuality cover = CoverQuality.None;

            TacticalLineOfSight los = TacticalLineOfSight.Instance;
            if (action.requiresLineOfSight && los != null)
            {
                if (target != null)
                {
                    LineOfSightResult result = los.Evaluate(actor, target);
                    hasLineOfSight = result.HasLineOfSight;
                    cover = action.ignoresCover
                        ? CoverQuality.None
                        : result.Cover;
                }
                else
                {
                    hasLineOfSight = los.HasLineOfSightToPoint(actor, effectPoint);
                }

                if (!hasLineOfSight)
                {
                    return TacticalTargetPreview.Invalid(
                        ActionFailureReason.NoLineOfSight,
                        effectPoint);
                }
            }

            float heightDifference =
                effectPoint.y - actor.transform.position.y;

            int heightModifier = action.usesHeightAdvantage
                ? GetHeightHitModifier(-heightDifference)
                : 0;

            int coverModifier = action.ignoresCover
                ? 0
                : GetCoverHitModifier(cover);

            int attributeModifier =
                actor.Definition.GetModifier(action.scalingAttribute);

            int hitChance = action.requiresAttackRoll
                ? Mathf.Clamp(
                    action.baseHitChance +
                    attributeModifier * 5 +
                    heightModifier +
                    coverModifier,
                    5,
                    95)
                : 100;

            int scaledBonus = Mathf.RoundToInt(
                attributeModifier * action.scalingMultiplier);

            int damageMin = Mathf.Max(
                0,
                action.damage.Minimum + scaledBonus);

            int damageMax = Mathf.Max(
                damageMin,
                action.damage.Maximum + scaledBonus);

            if (target != null &&
                damageMax > 0)
            {
                float multiplier =
                    target.GetDamageMultiplier(
                        action.damageType);

                if (multiplier <= 0f)
                {
                    damageMin = 0;
                    damageMax = 0;
                }
                else
                {
                    damageMin =
                        Mathf.RoundToInt(
                            damageMin * multiplier);

                    damageMax =
                        Mathf.RoundToInt(
                            damageMax * multiplier);

                    int mitigation =
                        target.GetDamageMitigation(
                            action.damageType);

                    if (damageMin > 0)
                    {
                        damageMin =
                            Mathf.Max(
                                1,
                                damageMin -
                                mitigation);
                    }

                    if (damageMax > 0)
                    {
                        damageMax =
                            Mathf.Max(
                                damageMin,
                                Mathf.Max(
                                    1,
                                    damageMax -
                                    mitigation));
                    }
                }
            }

            int affectedTargets = CountAffectedTargets(
                actor,
                action,
                target,
                effectPoint);

            return new TacticalTargetPreview(
                true,
                ActionFailureReason.None,
                hasLineOfSight,
                cover,
                distance,
                heightDifference,
                heightModifier,
                coverModifier,
                hitChance,
                damageMin,
                damageMax,
                affectedTargets,
                effectPoint);
        }

        public static bool IsFriendly(
            CombatFaction a,
            CombatFaction b)
        {
            if (a == CombatFaction.Neutral || b == CombatFaction.Neutral)
                return a == b;

            bool aParty =
                a == CombatFaction.Player || a == CombatFaction.Ally;
            bool bParty =
                b == CombatFaction.Player || b == CombatFaction.Ally;

            return aParty == bParty;
        }

        public static bool MatchesAreaRule(
            CombatantRuntime actor,
            CombatantRuntime candidate,
            AreaTargetRule rule,
            CombatantRuntime primaryTarget)
        {
            if (actor == null ||
                candidate == null ||
                !candidate.CanBeTargeted)
            {
                return false;
            }

            switch (rule)
            {
                case AreaTargetRule.PrimaryOnly:
                    return candidate == primaryTarget;

                case AreaTargetRule.EnemiesOnly:
                    return !IsFriendly(actor.Faction, candidate.Faction) &&
                           candidate.Faction != CombatFaction.Neutral;

                case AreaTargetRule.AlliesOnly:
                    return IsFriendly(actor.Faction, candidate.Faction);

                case AreaTargetRule.Everyone:
                    return true;

                default:
                    return false;
            }
        }

        private static int CountAffectedTargets(
            CombatantRuntime actor,
            CombatActionDefinition action,
            CombatantRuntime primaryTarget,
            Vector3 effectPoint)
        {
            if (action.areaRadius <= 0.01f)
                return primaryTarget != null ? 1 : 0;

            return Object.FindObjectsByType<CombatantRuntime>(
                    FindObjectsSortMode.None)
                .Count(candidate =>
                    candidate != null &&
                    candidate.CanBeTargeted &&
                    Vector3.Distance(candidate.transform.position, effectPoint)
                        <= action.areaRadius + 0.05f &&
                    MatchesAreaRule(
                        actor,
                        candidate,
                        action.areaTargetRule,
                        primaryTarget));
        }

        private static bool IsTargetKindValid(
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
                           target.CanBeTargeted &&
                           IsFriendly(actor.Faction, target.Faction);

                case TargetKind.Enemy:
                    return target != null &&
                           target.IsAlive &&
                           !IsFriendly(actor.Faction, target.Faction) &&
                           target.Faction != CombatFaction.Neutral;

                case TargetKind.AnyCombatant:
                    return target != null &&
                           target.CanBeTargeted;

                case TargetKind.Ground:
                    return groundPoint.HasValue;

                default:
                    return false;
            }
        }

        private static int GetCoverHitModifier(CoverQuality cover)
        {
            switch (cover)
            {
                case CoverQuality.Half: return -15;
                case CoverQuality.Full: return -30;
                default: return 0;
            }
        }

        private static int GetHeightHitModifier(float attackerAboveTarget)
        {
            if (attackerAboveTarget >= 3f)
                return 20;

            if (attackerAboveTarget >= 1.25f)
                return 10;

            if (attackerAboveTarget <= -3f)
                return -20;

            if (attackerAboveTarget <= -1.25f)
                return -10;

            return 0;
        }
    }
}
