using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    [Serializable]
    public sealed class SurfacePatch
    {
        public int id;
        public SurfaceType type;
        public Vector3 center;
        public float radius;
        public int roundsRemaining;
        public GameObject source;
    }

    public readonly struct ElementalReactionEvent
    {
        public readonly ElementalReactionKind Kind;
        public readonly Vector3 Point;
        public readonly float Radius;
        public readonly DamageType DamageType;
        public readonly int BonusDamage;
        public readonly GameObject Source;

        public ElementalReactionEvent(
            ElementalReactionKind kind,
            Vector3 point,
            float radius,
            DamageType damageType,
            int bonusDamage,
            GameObject source)
        {
            Kind = kind;
            Point = point;
            Radius = radius;
            DamageType = damageType;
            BonusDamage = bonusDamage;
            Source = source;
        }
    }

    public sealed class ElementalSurfaceSystem : MonoBehaviour
    {
        public static ElementalSurfaceSystem Instance { get; private set; }

        [SerializeField] private List<SurfacePatch> patches =
            new List<SurfacePatch>();

        private readonly HashSet<
            SurfaceApplicationKey>
            _appliedThisRound =
                new HashSet<
                    SurfaceApplicationKey>();

        private int _nextId = 1;

        public IReadOnlyList<SurfacePatch> Patches => patches;

        public static event Action<ElementalReactionEvent>
            ReactionTriggered;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            CombatActionExecutor.SurfaceRequested += OnSurfaceRequested;
            TacticalUnitMover.StepCompleted += OnStepCompleted;

            if (TurnCombatDirector.Instance != null)
            {
                TurnCombatDirector.Instance.RoundStarted += OnRoundStarted;
                TurnCombatDirector.Instance.CurrentActorChanged +=
                    OnCurrentActorChanged;
            }
        }

        private void Start()
        {
            if (TurnCombatDirector.Instance != null)
            {
                TurnCombatDirector.Instance.RoundStarted -= OnRoundStarted;
                TurnCombatDirector.Instance.CurrentActorChanged -=
                    OnCurrentActorChanged;

                TurnCombatDirector.Instance.RoundStarted += OnRoundStarted;
                TurnCombatDirector.Instance.CurrentActorChanged +=
                    OnCurrentActorChanged;
            }
        }

        private void OnDisable()
        {
            CombatActionExecutor.SurfaceRequested -= OnSurfaceRequested;
            TacticalUnitMover.StepCompleted -= OnStepCompleted;

            if (TurnCombatDirector.Instance != null)
            {
                TurnCombatDirector.Instance.RoundStarted -= OnRoundStarted;
                TurnCombatDirector.Instance.CurrentActorChanged -=
                    OnCurrentActorChanged;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void OnSurfaceRequested(SurfaceRequest request)
        {
            CreateOrReact(
                request.Type,
                request.Point,
                request.Radius,
                request.DurationTurns,
                request.Source);
        }

        public float GetHazardCostAt(
            Vector3 position,
            CombatantRuntime actor = null)
        {
            float total = 0f;

            foreach (SurfacePatch patch in patches)
            {
                if (patch == null ||
                    Vector3.Distance(
                        position,
                        patch.center) >
                    patch.radius + 0.05f)
                {
                    continue;
                }

                float baseCost;
                DamageType damageType;
                bool hasDamageType = true;

                switch (patch.type)
                {
                    case SurfaceType.Fire:
                        baseCost = 4f;
                        damageType = DamageType.Fire;
                        break;

                    case SurfaceType.Poison:
                        baseCost = 3f;
                        damageType = DamageType.Poison;
                        break;

                    case SurfaceType.Electrified:
                        baseCost = 4f;
                        damageType = DamageType.Lightning;
                        break;

                    case SurfaceType.Arcane:
                        baseCost = 3f;
                        damageType = DamageType.Arcane;
                        break;

                    case SurfaceType.Ice:
                        baseCost = 1.5f;
                        damageType = DamageType.Frost;
                        break;

                    case SurfaceType.Steam:
                        baseCost = 0.75f;
                        damageType = DamageType.Fire;
                        break;

                    default:
                        baseCost = 0f;
                        damageType = DamageType.Physical;
                        hasDamageType = false;
                        break;
                }

                if (actor != null &&
                    hasDamageType)
                {
                    baseCost *=
                        actor.GetDamageMultiplier(
                            damageType);
                }

                total += baseCost;
            }

            return total;
        }

        public void CreateOrReact(
            SurfaceType incoming,
            Vector3 center,
            float radius,
            int durationRounds,
            GameObject source)
        {
            if (incoming == SurfaceType.None || radius <= 0.01f)
                return;

            SurfacePatch overlapping = patches
                .Where(x => x != null)
                .OrderBy(x => Vector3.Distance(x.center, center))
                .FirstOrDefault(x =>
                    Vector3.Distance(x.center, center)
                    <= Mathf.Max(x.radius, radius) * 0.8f);

            if (overlapping != null)
            {
                SurfaceType reacted = React(overlapping.type, incoming);

                if (reacted == SurfaceType.Detonation)
                {
                    Detonate(
                        (overlapping.center + center) * 0.5f,
                        Mathf.Max(overlapping.radius, radius) + 0.5f,
                        source);

                    patches.Remove(overlapping);

                    AddPatch(
                        SurfaceType.Fire,
                        center,
                        Mathf.Max(overlapping.radius, radius),
                        Mathf.Max(1, durationRounds),
                        source);

                    return;
                }

                if (reacted != SurfaceType.None)
                {
                    overlapping.type = reacted;
                    overlapping.center =
                        (overlapping.center + center) * 0.5f;
                    overlapping.radius =
                        Mathf.Max(overlapping.radius, radius);
                    overlapping.roundsRemaining =
                        Mathf.Max(
                            overlapping.roundsRemaining,
                            Mathf.Max(1, durationRounds));
                    overlapping.source = source ?? overlapping.source;
                    return;
                }
            }

            AddPatch(
                incoming,
                center,
                radius,
                Mathf.Max(1, durationRounds),
                source);
        }

        public ElementalReactionKind PredictImpactReaction(
            DamageType incomingDamage,
            Vector3 point)
        {
            SurfacePatch patch =
                FindPatchAt(point);

            return patch != null
                ? ResolveImpactReaction(
                    patch.type,
                    incomingDamage)
                : ElementalReactionKind.None;
        }

        public float GetImpactReactionScore(
            DamageType incomingDamage,
            Vector3 point)
        {
            switch (PredictImpactReaction(
                        incomingDamage,
                        point))
            {
                case ElementalReactionKind.Combustion:
                    return 32f;
                case ElementalReactionKind.ConductiveSurge:
                    return 26f;
                case ElementalReactionKind.ArcaneResonance:
                    return 24f;
                case ElementalReactionKind.ThermalShock:
                    return 20f;
                case ElementalReactionKind.FlashFreeze:
                    return 18f;
                default:
                    return 0f;
            }
        }

        public ElementalReactionKind ReactToImpact(
            DamageType incomingDamage,
            Vector3 point,
            GameObject source)
        {
            SurfacePatch patch =
                FindPatchAt(point);

            if (patch == null)
                return ElementalReactionKind.None;

            ElementalReactionKind reaction =
                ResolveImpactReaction(
                    patch.type,
                    incomingDamage);

            if (reaction ==
                ElementalReactionKind.None)
            {
                return reaction;
            }

            float radius =
                Mathf.Max(
                    1.5f,
                    patch.radius);

            int bonusDamage = 0;
            DamageType reactionDamage =
                incomingDamage;

            switch (reaction)
            {
                case ElementalReactionKind
                    .ConductiveSurge:
                    patch.type =
                        SurfaceType.Electrified;
                    patch.roundsRemaining =
                        Mathf.Max(
                            patch.roundsRemaining,
                            2);
                    bonusDamage = 6;
                    reactionDamage =
                        DamageType.Lightning;
                    radius += 0.65f;
                    break;

                case ElementalReactionKind
                    .FlashFreeze:
                    patch.type =
                        SurfaceType.Ice;
                    patch.roundsRemaining =
                        Mathf.Max(
                            patch.roundsRemaining,
                            2);
                    bonusDamage = 3;
                    reactionDamage =
                        DamageType.Frost;
                    break;

                case ElementalReactionKind
                    .ThermalShock:
                    patch.type =
                        SurfaceType.Steam;
                    patch.roundsRemaining =
                        Mathf.Max(
                            patch.roundsRemaining,
                            1);
                    bonusDamage = 5;
                    reactionDamage =
                        DamageType.Fire;
                    radius += 0.3f;
                    break;

                case ElementalReactionKind
                    .Combustion:
                    bonusDamage = 12;
                    reactionDamage =
                        DamageType.Fire;
                    radius += 0.9f;
                    patches.Remove(patch);
                    break;

                case ElementalReactionKind
                    .ArcaneResonance:
                    bonusDamage = 7;
                    reactionDamage =
                        DamageType.Arcane;
                    radius += 0.55f;
                    patch.type =
                        SurfaceType.Arcane;
                    patch.roundsRemaining =
                        Mathf.Max(
                            patch.roundsRemaining,
                            1);
                    break;
            }

            if (bonusDamage > 0)
            {
                ApplyReactionDamage(
                    point,
                    radius,
                    bonusDamage,
                    reactionDamage,
                    source);
            }

            ReactionTriggered?.Invoke(
                new ElementalReactionEvent(
                    reaction,
                    point,
                    radius,
                    reactionDamage,
                    bonusDamage,
                    source));

            return reaction;
        }

        private SurfacePatch FindPatchAt(
            Vector3 point)
        {
            return patches
                .Where(value =>
                    value != null &&
                    Vector3.Distance(
                        point,
                        value.center) <=
                    value.radius + 0.05f)
                .OrderBy(value =>
                    Vector3.Distance(
                        point,
                        value.center))
                .FirstOrDefault();
        }

        private static ElementalReactionKind
            ResolveImpactReaction(
                SurfaceType existing,
                DamageType incomingDamage)
        {
            if (existing == SurfaceType.Water &&
                incomingDamage ==
                    DamageType.Lightning)
            {
                return ElementalReactionKind
                    .ConductiveSurge;
            }

            if (existing == SurfaceType.Water &&
                incomingDamage ==
                    DamageType.Frost)
            {
                return ElementalReactionKind
                    .FlashFreeze;
            }

            if (existing == SurfaceType.Ice &&
                incomingDamage ==
                    DamageType.Fire)
            {
                return ElementalReactionKind
                    .ThermalShock;
            }

            if (existing == SurfaceType.Poison &&
                incomingDamage ==
                    DamageType.Fire)
            {
                return ElementalReactionKind
                    .Combustion;
            }

            if (incomingDamage ==
                    DamageType.Arcane &&
                existing != SurfaceType.Arcane &&
                existing != SurfaceType.None)
            {
                return ElementalReactionKind
                    .ArcaneResonance;
            }

            return ElementalReactionKind.None;
        }

        private static void ApplyReactionDamage(
            Vector3 center,
            float radius,
            int damage,
            DamageType damageType,
            GameObject source)
        {
            CombatantRuntime[] combatants =
                FindObjectsByType<
                    CombatantRuntime>(
                    FindObjectsSortMode.None);

            foreach (CombatantRuntime target
                     in combatants)
            {
                if (target == null ||
                    !target.IsAlive ||
                    Vector3.Distance(
                        target.transform.position,
                        center) >
                    radius)
                {
                    continue;
                }

                target.ApplyDamage(
                    new DamagePacket(
                        damage,
                        damageType,
                        source));
            }
        }

        private void AddPatch(
            SurfaceType type,
            Vector3 center,
            float radius,
            int durationRounds,
            GameObject source)
        {
            patches.Add(new SurfacePatch
            {
                id = _nextId++,
                type = type,
                center = center,
                radius = radius,
                roundsRemaining = durationRounds,
                source = source
            });
        }

        private void OnCurrentActorChanged(CombatantRuntime actor)
        {
            if (actor == null || !actor.IsAlive)
                return;

            ApplyAtPosition(actor, actor.transform.position, false);
        }

        private void OnStepCompleted(
            CombatantRuntime actor,
            Vector3 from,
            Vector3 to)
        {
            if (actor == null || !actor.IsAlive)
                return;

            ApplyAtPosition(actor, to, true);
        }

        private void ApplyAtPosition(
            CombatantRuntime actor,
            Vector3 position,
            bool enteredByMovement)
        {
            int round =
                TurnCombatDirector.Instance != null
                    ? TurnCombatDirector.Instance.Round
                    : 0;

            foreach (SurfacePatch patch in patches.ToArray())
            {
                if (patch == null ||
                    Vector3.Distance(position, patch.center)
                    > patch.radius + 0.05f)
                {
                    continue;
                }

                var key =
                    new SurfaceApplicationKey(
                        actor,
                        patch.id,
                        round);

                if (!_appliedThisRound.Add(key))
                    continue;

                switch (patch.type)
                {
                    case SurfaceType.Fire:
                        actor.ApplyDamage(new DamagePacket(
                            4,
                            DamageType.Fire,
                            patch.source));
                        break;

                    case SurfaceType.Poison:
                        actor.ApplyDamage(new DamagePacket(
                            3,
                            DamageType.Poison,
                            patch.source));
                        break;

                    case SurfaceType.Electrified:
                        actor.ApplyDamage(new DamagePacket(
                            5,
                            DamageType.Lightning,
                            patch.source));
                        break;

                    case SurfaceType.Ice:
                        if (enteredByMovement)
                            actor.TrySpendMovement(0.75f);
                        break;

                    case SurfaceType.Arcane:
                        actor.ApplyDamage(new DamagePacket(
                            4,
                            DamageType.Arcane,
                            patch.source));
                        break;
                }

                if (!actor.IsAlive)
                    return;
            }
        }

        private readonly struct SurfaceApplicationKey :
            IEquatable<SurfaceApplicationKey>
        {
            private readonly CombatantRuntime actor;
            private readonly int patchId;
            private readonly int round;

            public SurfaceApplicationKey(
                CombatantRuntime actor,
                int patchId,
                int round)
            {
                this.actor = actor;
                this.patchId = patchId;
                this.round = round;
            }

            public bool Equals(
                SurfaceApplicationKey other)
            {
                return
                    ReferenceEquals(
                        actor,
                        other.actor) &&
                    patchId == other.patchId &&
                    round == other.round;
            }

            public override bool Equals(
                object obj)
            {
                return
                    obj is SurfaceApplicationKey other &&
                    Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;

                    hash =
                        hash * 31 +
                        (actor != null
                            ? actor.GetHashCode()
                            : 0);

                    hash =
                        hash * 31 +
                        patchId;

                    hash =
                        hash * 31 +
                        round;

                    return hash;
                }
            }
        }

        private void OnRoundStarted(int round)
        {
            _appliedThisRound.Clear();

            for (int i = patches.Count - 1; i >= 0; i--)
            {
                SurfacePatch patch = patches[i];

                if (patch == null)
                {
                    patches.RemoveAt(i);
                    continue;
                }

                patch.roundsRemaining--;
                if (patch.roundsRemaining <= 0)
                    patches.RemoveAt(i);
            }
        }

        private static SurfaceType React(
            SurfaceType existing,
            SurfaceType incoming)
        {
            if (existing == incoming)
                return existing;

            if ((existing == SurfaceType.Water &&
                 incoming == SurfaceType.Ice) ||
                (existing == SurfaceType.Ice &&
                 incoming == SurfaceType.Water))
            {
                return SurfaceType.Ice;
            }

            if ((existing == SurfaceType.Water &&
                 incoming == SurfaceType.Electrified) ||
                (existing == SurfaceType.Electrified &&
                 incoming == SurfaceType.Water))
            {
                return SurfaceType.Electrified;
            }

            if ((existing == SurfaceType.Fire &&
                 incoming == SurfaceType.Water) ||
                (existing == SurfaceType.Water &&
                 incoming == SurfaceType.Fire))
            {
                return SurfaceType.Steam;
            }

            if ((existing == SurfaceType.Fire &&
                 incoming == SurfaceType.Ice) ||
                (existing == SurfaceType.Ice &&
                 incoming == SurfaceType.Fire))
            {
                return SurfaceType.Water;
            }

            if ((existing == SurfaceType.Fire &&
                 incoming == SurfaceType.Poison) ||
                (existing == SurfaceType.Poison &&
                 incoming == SurfaceType.Fire))
            {
                return SurfaceType.Detonation;
            }

            return SurfaceType.None;
        }

        private static void Detonate(
            Vector3 center,
            float radius,
            GameObject source)
        {
            CombatantRuntime[] combatants =
                FindObjectsByType<CombatantRuntime>(
                    FindObjectsSortMode.None);

            foreach (CombatantRuntime target in combatants)
            {
                if (target == null ||
                    !target.IsAlive ||
                    Vector3.Distance(target.transform.position, center)
                    > radius)
                {
                    continue;
                }

                target.ApplyDamage(new DamagePacket(
                    12,
                    DamageType.Fire,
                    source));
            }
        }

        private void OnDrawGizmos()
        {
            if (patches == null)
                return;

            foreach (SurfacePatch patch in patches)
            {
                if (patch == null)
                    continue;

                Gizmos.DrawWireSphere(
                    patch.center + Vector3.up * 0.05f,
                    patch.radius);
            }
        }
    }
}
