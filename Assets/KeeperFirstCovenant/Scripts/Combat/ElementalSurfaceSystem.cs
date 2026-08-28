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

    public sealed class ElementalSurfaceSystem : MonoBehaviour
    {
        public static ElementalSurfaceSystem Instance { get; private set; }

        [SerializeField] private List<SurfacePatch> patches =
            new List<SurfacePatch>();

        private readonly HashSet<string> _appliedThisRound =
            new HashSet<string>();

        private int _nextId = 1;

        public IReadOnlyList<SurfacePatch> Patches => patches;

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

                string key =
                    $"{actor.GetInstanceID()}:{patch.id}:{round}";

                if (!_appliedThisRound.Add(key))
                    continue;

                switch (patch.type)
                {
                    case SurfaceType.Fire:
                        actor.ApplyDamage(new DamagePacket(
                            UnityEngine.Random.Range(1, 5),
                            DamageType.Fire,
                            patch.source));
                        break;

                    case SurfaceType.Poison:
                        actor.ApplyDamage(new DamagePacket(
                            UnityEngine.Random.Range(1, 5),
                            DamageType.Poison,
                            patch.source));
                        break;

                    case SurfaceType.Electrified:
                        actor.ApplyDamage(new DamagePacket(
                            UnityEngine.Random.Range(1, 5),
                            DamageType.Lightning,
                            patch.source));
                        break;

                    case SurfaceType.Ice:
                        if (enteredByMovement)
                            actor.TrySpendMovement(0.75f);
                        break;

                    case SurfaceType.Arcane:
                        actor.ApplyDamage(new DamagePacket(
                            UnityEngine.Random.Range(1, 4),
                            DamageType.Arcane,
                            patch.source));
                        break;
                }

                if (!actor.IsAlive)
                    return;
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

                int damage =
                    UnityEngine.Random.Range(1, 7) +
                    UnityEngine.Random.Range(1, 7);

                target.ApplyDamage(new DamagePacket(
                    damage,
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
