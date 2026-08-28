using System.Linq;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
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

                if (verticalDelta > maxStepUp)
                    return;

                if (IsOccupied(
                        target,
                        cellWorld,
                        _grid.CellSize))
                {
                    return;
                }

                if (!walkable)
                    return;

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

        private static bool IsOccupied(
            CombatantRuntime target,
            Vector3 point,
            float cellSize)
        {
            float radius =
                Mathf.Max(
                    0.35f,
                    cellSize * 0.42f);

            return Object
                .FindObjectsByType<
                    CombatantRuntime>(
                    FindObjectsSortMode.None)
                .Any(x =>
                    x != null &&
                    x != target &&
                    x.CanBeTargeted &&
                    Vector3.Distance(
                        x.transform.position,
                        point) <= radius);
        }
    }
}
