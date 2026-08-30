using System;
using System.Linq;
using KeeperFirstCovenant.World;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    public readonly struct ForcedMovementCollisionEvent
    {
        public readonly CombatantRuntime Target;
        public readonly CombatantRuntime Other;
        public readonly Vector3 Point;
        public readonly float ForceMeters;

        public ForcedMovementCollisionEvent(
            CombatantRuntime target,
            CombatantRuntime other,
            Vector3 point,
            float forceMeters)
        {
            Target = target;
            Other = other;
            Point = point;
            ForceMeters = forceMeters;
        }
    }

    public sealed class ForcedMovementSystem : MonoBehaviour
    {
        public static ForcedMovementSystem Instance { get; private set; }

        [SerializeField, Min(0f)]
        private float maxStepUp = 0.8f;

        [SerializeField, Min(0f)]
        private float safeDrop = 1.25f;

        [SerializeField, Min(0.1f)]
        private float fallDamagePerMeter = 4f;

        [SerializeField, Min(1f)]
        private float fatalDrop = 12f;

        [Header("Collision impact")]
        [SerializeField, Min(0f)]
        private float collisionDamagePerMeter = 3f;

        [SerializeField, Min(0f)]
        private float collisionBreakPerMeter = 8f;

        public static event Action<
            ForcedMovementCollisionEvent>
            CollisionOccurred;

        private TacticalGrid3D _grid;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            _grid =
                FindFirstObjectByType<
                    TacticalGrid3D>();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Push(
            CombatantRuntime source,
            CombatantRuntime target,
            float distanceMeters,
            bool awayFromSource = true)
        {
            if (target == null ||
                !target.IsAlive ||
                distanceMeters <= 0.01f)
            {
                return;
            }

            if (_grid == null)
            {
                _grid =
                    FindFirstObjectByType<
                        TacticalGrid3D>();
            }

            if (_grid == null)
                return;

            Vector3 direction =
                target.transform.position -
                (source != null
                    ? source.transform.position
                    : target.transform.position -
                      target.transform.forward);

            direction.y = 0f;

            if (!awayFromSource)
                direction = -direction;

            if (direction.sqrMagnitude < 0.001f)
                direction = Vector3.forward;

            direction.Normalize();

            int steps =
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        distanceMeters /
                        _grid.CellSize));

            Vector3 current =
                target.transform.position;

            for (int i = 0;
                 i < steps &&
                 target.IsAlive;
                 i++)
            {
                Vector3 probe =
                    current +
                    direction *
                    _grid.CellSize;

                bool inside =
                    _grid.TryGetDirectCellInfo(
                        probe,
                        out Vector3 cellWorld,
                        out bool hasGround,
                        out bool walkable);

                if (!inside || !hasGround)
                {
                    ApplyFatalFall(
                        source,
                        target);
                    return;
                }

                float verticalDelta =
                    cellWorld.y - current.y;

                float remainingForce =
                    Mathf.Max(
                        _grid.CellSize,
                        (steps - i) *
                        _grid.CellSize);

                if (verticalDelta > maxStepUp)
                {
                    ApplyCollisionImpact(
                        source,
                        target,
                        null,
                        current +
                        direction *
                        (_grid.CellSize * 0.5f),
                        remainingForce);

                    return;
                }

                CombatantRuntime occupant =
                    FindOccupant(
                        target,
                        cellWorld,
                        _grid.CellSize);

                if (occupant != null)
                {
                    ApplyCollisionImpact(
                        source,
                        target,
                        occupant,
                        (target.transform.position +
                         occupant.transform.position) *
                        0.5f,
                        remainingForce);

                    return;
                }

                if (!walkable)
                {
                    ApplyCollisionImpact(
                        source,
                        target,
                        null,
                        current +
                        direction *
                        (_grid.CellSize * 0.5f),
                        remainingForce);

                    return;
                }

                target.transform.position =
                    cellWorld;

                float drop =
                    current.y - cellWorld.y;

                if (drop > safeDrop)
                {
                    ApplyFallDamage(
                        source,
                        target,
                        drop);

                    if (!target.IsAlive)
                        return;
                }

                current = cellWorld;
            }
        }

        private void ApplyFallDamage(
            CombatantRuntime source,
            CombatantRuntime target,
            float drop)
        {
            if (drop >= fatalDrop)
            {
                ApplyFatalFall(
                    source,
                    target);
                return;
            }

            int damage =
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        drop *
                        fallDamagePerMeter));

            target.ApplyDamage(
                new DamagePacket(
                    damage,
                    DamageType.Physical,
                    source != null
                        ? source.gameObject
                        : gameObject));

            target.GetComponent<
                    BreakGaugeComponent>()
                ?.AddBreak(
                    Mathf.CeilToInt(
                        drop * 6f));
        }

        private void ApplyCollisionImpact(
            CombatantRuntime source,
            CombatantRuntime target,
            CombatantRuntime other,
            Vector3 point,
            float forceMeters)
        {
            float force =
                Mathf.Max(
                    0.5f,
                    forceMeters);

            int damage =
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        force *
                        collisionDamagePerMeter));

            int breakDamage =
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        force *
                        collisionBreakPerMeter));

            target.ApplyDamage(
                new DamagePacket(
                    damage,
                    DamageType.Physical,
                    source != null
                        ? source.gameObject
                        : gameObject));

            target.GetComponent<
                    BreakGaugeComponent>()
                ?.AddBreak(
                    breakDamage);

            if (other != null &&
                other.IsAlive)
            {
                int secondaryDamage =
                    Mathf.Max(
                        1,
                        Mathf.CeilToInt(
                            damage * 0.65f));

                other.ApplyDamage(
                    new DamagePacket(
                        secondaryDamage,
                        DamageType.Physical,
                        source != null
                            ? source.gameObject
                            : target.gameObject));

                other.GetComponent<
                        BreakGaugeComponent>()
                    ?.AddBreak(
                        Mathf.CeilToInt(
                            breakDamage * 0.75f));
            }
            else
            {
                Collider[] colliders =
                    Physics.OverlapSphere(
                        point,
                        0.6f,
                        ~0,
                        QueryTriggerInteraction.Ignore);

                foreach (EnvironmentalDestructible destructible
                         in colliders
                            .Select(value =>
                                value.GetComponentInParent<
                                    EnvironmentalDestructible>())
                            .Where(value =>
                                value != null)
                            .Distinct())
                {
                    destructible.ApplyImpact(
                        ImpactTier.Heavy,
                        force * 2.5f,
                        point);
                }
            }

            WorldNoiseSystem.Emit(
                point,
                Mathf.Clamp(
                    5f + force * 2f,
                    6f,
                    18f),
                source != null
                    ? source.gameObject
                    : target.gameObject,
                Mathf.Clamp(
                    0.8f + force * 0.12f,
                    0.8f,
                    1.8f));

            CollisionOccurred?.Invoke(
                new ForcedMovementCollisionEvent(
                    target,
                    other,
                    point,
                    force));
        }

        private static void ApplyFatalFall(
            CombatantRuntime source,
            CombatantRuntime target)
        {
            if (target == null ||
                !target.CanBeTargeted)
            {
                return;
            }

            target.ApplyDamage(
                new DamagePacket(
                    9999,
                    DamageType.Physical,
                    source != null
                        ? source.gameObject
                        : target.gameObject));
        }

        private static CombatantRuntime
            FindOccupant(
                CombatantRuntime target,
                Vector3 point,
                float cellSize)
        {
            float radius =
                Mathf.Max(
                    0.35f,
                    cellSize * 0.42f);

            return UnityEngine.Object
                .FindObjectsByType<
                    CombatantRuntime>(
                    FindObjectsSortMode.None)
                .Where(x =>
                    x != null &&
                    x != target &&
                    x.CanBeTargeted)
                .OrderBy(x =>
                    Vector3.Distance(
                        x.transform.position,
                        point))
                .FirstOrDefault(x =>
                    Vector3.Distance(
                        x.transform.position,
                        point) <= radius);
        }
    }
}
