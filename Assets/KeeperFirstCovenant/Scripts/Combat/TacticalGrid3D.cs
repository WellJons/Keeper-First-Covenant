using System.Collections.Generic;
using KeeperFirstCovenant.World;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    public sealed class TacticalGrid3D : MonoBehaviour
    {
        private sealed class Node
        {
            public int x;
            public int z;
            public Vector3 world;
            public bool hasGround;
            public bool walkable;
            public float g;
            public float h;
            public Node parent;
            public int searchId;
            public bool closed;
            public float F => g + h;
        }

        [Header("Grid")]
        [SerializeField] private Vector2 worldSize = new Vector2(36f, 36f);
        [SerializeField] private float cellSize = 0.75f;
        [SerializeField] private float agentRadius = 0.4f;
        [SerializeField] private float groundProbeHeight = 8f;
        [SerializeField] private float groundProbeDistance = 20f;

        [SerializeField, Min(0.05f)]
        private float maxStepHeight = 0.6f;

        [SerializeField, Min(0.05f)]
        private float maxWalkDrop = 1.1f;

        [SerializeField, Range(0f, 1f)]
        private float minimumGroundNormalY = 0.65f;

        [Header("Layers")]
        [SerializeField] private LayerMask obstacleMask = ~0;
        [SerializeField] private LayerMask groundMask = ~0;

        private Node[,] _nodes;
        private int _width;
        private int _height;
        private Vector3 _origin;
        private int _searchId;

        public float CellSize => cellSize;

        private void Awake()
        {
            Build();
        }

        public void RebuildForDynamicWorld()
        {
            Physics.SyncTransforms();
            Build();
        }

        public void Build()
        {
            _width = Mathf.Max(2, Mathf.RoundToInt(worldSize.x / cellSize));
            _height = Mathf.Max(2, Mathf.RoundToInt(worldSize.y / cellSize));
            _origin = transform.position - new Vector3(worldSize.x, 0f, worldSize.y) * 0.5f;
            _nodes = new Node[_width, _height];

            for (int x = 0; x < _width; x++)
            {
                for (int z = 0; z < _height; z++)
                {
                    Vector3 horizontal = _origin + new Vector3((x + 0.5f) * cellSize, 0f, (z + 0.5f) * cellSize);
                    Vector3 probeStart = horizontal + Vector3.up * groundProbeHeight;

                    bool hasGround =
                        TryProbeGround(
                            horizontal,
                            out RaycastHit hit);

                    Vector3 world =
                        hasGround
                            ? hit.point
                            : horizontal;
                    bool blocked =
                        IsBlockedByWorld(
                            world);

                    _nodes[x, z] = new Node
                    {
                        x = x,
                        z = z,
                        world = world,
                        hasGround = hasGround,
                        walkable = hasGround && !blocked
                    };
                }
            }
        }

        public bool TryProjectWalkablePoint(
            Vector3 world,
            out Vector3 projected)
        {
            EnsureBuilt();

            bool inside =
                world.x >= _origin.x &&
                world.z >= _origin.z &&
                world.x <= _origin.x + worldSize.x &&
                world.z <= _origin.z + worldSize.y;

            if (!inside)
            {
                projected = world;
                return false;
            }

            Vector3 horizontal =
                new Vector3(
                    world.x,
                    transform.position.y,
                    world.z);

            bool hasGround =
                TryProbeGround(
                    horizontal,
                    out RaycastHit hit);

            if (!hasGround)
            {
                projected = world;
                return false;
            }

            projected = hit.point;

            bool blocked =
                IsBlockedByWorld(
                    projected);

            return !blocked;
        }

        public List<Vector3> FindContinuousPath(
            Vector3 startWorld,
            Vector3 endWorld)
        {
            if (!TryProjectWalkablePoint(
                    endWorld,
                    out Vector3 exactEnd))
            {
                return new List<Vector3>();
            }

            if (Vector3.Distance(
                    startWorld,
                    exactEnd) <= 0.05f)
            {
                return new List<Vector3>();
            }

            Node start = ClosestNode(startWorld);
            Node goal = ClosestNode(exactEnd);

            if (start == null ||
                goal == null ||
                !start.walkable ||
                !goal.walkable)
            {
                return new List<Vector3>();
            }

            if (start == goal &&
                IsDirectSegmentClear(
                    startWorld,
                    exactEnd))
            {
                return new List<Vector3>
                {
                    exactEnd
                };
            }

            List<Vector3> raw =
                FindPath(
                    startWorld,
                    exactEnd);

            if (raw.Count == 0)
                return raw;

            if (Vector3.Distance(
                    raw[raw.Count - 1],
                    exactEnd) > 0.05f)
            {
                raw.Add(exactEnd);
            }
            else
            {
                raw[raw.Count - 1] =
                    exactEnd;
            }

            return RemoveTinySegments(
                raw,
                startWorld);
        }

        private bool IsDirectSegmentClear(
            Vector3 from,
            Vector3 to)
        {
            Vector3 delta = to - from;
            delta.y = 0f;

            float distance = delta.magnitude;

            if (distance <= 0.05f)
                return true;

            Vector3 origin =
                from +
                Vector3.up * agentRadius;

            return !Physics.SphereCast(
                origin,
                agentRadius,
                delta.normalized,
                out _,
                distance,
                obstacleMask,
                QueryTriggerInteraction.Ignore);
        }

        private static List<Vector3>
            RemoveTinySegments(
                IReadOnlyList<Vector3> path,
                Vector3 start)
        {
            var result =
                new List<Vector3>();

            Vector3 previous = start;

            for (int i = 0;
                 i < path.Count;
                 i++)
            {
                Vector3 point = path[i];

                if (Vector3.Distance(
                        previous,
                        point) < 0.08f)
                {
                    continue;
                }

                result.Add(point);
                previous = point;
            }

            return result;
        }

        public bool TryGetDirectCellInfo(
            Vector3 world,
            out Vector3 cellWorld,
            out bool hasGround,
            out bool walkable)
        {
            EnsureBuilt();

            int x =
                Mathf.FloorToInt(
                    (world.x - _origin.x) /
                    cellSize);

            int z =
                Mathf.FloorToInt(
                    (world.z - _origin.z) /
                    cellSize);

            if (x < 0 ||
                z < 0 ||
                x >= _width ||
                z >= _height)
            {
                cellWorld = world;
                hasGround = false;
                walkable = false;
                return false;
            }

            Node node = _nodes[x, z];

            cellWorld = node.world;
            hasGround = node.hasGround;
            walkable = node.walkable;
            return true;
        }

        public Vector3 SnapToCell(Vector3 world)
        {
            Node node = ClosestNode(world);
            return node != null ? node.world : world;
        }

        public bool IsWalkable(Vector3 world)
        {
            Node node = ClosestNode(world);
            return node != null && node.walkable;
        }

        public List<Vector3> GetReachableCells(Vector3 startWorld, float maxMeters)
        {
            var result = new List<Vector3>();
            if (maxMeters <= 0f)
                return result;

            EnsureBuilt();
            Node start = ClosestNode(startWorld);
            if (start == null || !start.walkable)
                return result;

            var frontier = new List<Node> { start };
            var distance = new Dictionary<Node, float> { [start] = 0f };

            while (frontier.Count > 0)
            {
                int bestIndex = 0;
                for (int i = 1; i < frontier.Count; i++)
                {
                    if (distance[frontier[i]] < distance[frontier[bestIndex]])
                        bestIndex = i;
                }

                Node current = frontier[bestIndex];
                frontier.RemoveAt(bestIndex);
                float currentDistance = distance[current];

                if (current != start)
                    result.Add(current.world);

                foreach (Node next in Neighbours(current))
                {
                    if (next == null || !next.walkable)
                        continue;

                    float step = Vector3.Distance(current.world, next.world);
                    float candidate = currentDistance + step;

                    if (candidate > maxMeters + 0.001f)
                        continue;

                    if (!distance.TryGetValue(next, out float known) || candidate < known)
                    {
                        distance[next] = candidate;
                        if (!frontier.Contains(next))
                            frontier.Add(next);
                    }
                }
            }

            return result;
        }

        public List<Vector3> FindPath(Vector3 startWorld, Vector3 endWorld)
        {
            EnsureBuilt();

            Node start = ClosestNode(startWorld);
            Node goal = ClosestNode(endWorld);
            if (start == null || goal == null || !start.walkable || !goal.walkable)
                return new List<Vector3>();

            _searchId++;
            if (_searchId == int.MaxValue)
                ResetSearchIds();

            var open = new List<Node>(128);
            PrepareNode(start);
            start.g = 0f;
            start.h = Heuristic(start, goal);
            open.Add(start);

            while (open.Count > 0)
            {
                int currentIndex = 0;
                Node current = open[0];

                for (int i = 1; i < open.Count; i++)
                {
                    Node candidate = open[i];
                    if (candidate.F < current.F ||
                        (Mathf.Approximately(candidate.F, current.F) && candidate.h < current.h))
                    {
                        current = candidate;
                        currentIndex = i;
                    }
                }

                open.RemoveAt(currentIndex);
                current.closed = true;

                if (current == goal)
                    return Retrace(start, goal);

                foreach (Node next in Neighbours(current))
                {
                    if (!next.walkable)
                        continue;

                    PrepareNode(next);
                    if (next.closed)
                        continue;

                    float tentative = current.g + Vector3.Distance(current.world, next.world);
                    bool inOpen = open.Contains(next);

                    if (!inOpen || tentative < next.g)
                    {
                        next.g = tentative;
                        next.h = Heuristic(next, goal);
                        next.parent = current;

                        if (!inOpen)
                            open.Add(next);
                    }
                }
            }

            return new List<Vector3>();
        }

        public float CalculatePathLength(IReadOnlyList<Vector3> path, Vector3 start)
        {
            float total = 0f;
            Vector3 previous = start;
            for (int i = 0; i < path.Count; i++)
            {
                total += Vector3.Distance(previous, path[i]);
                previous = path[i];
            }
            return total;
        }

        private bool TryProbeGround(
            Vector3 horizontal,
            out RaycastHit bestHit)
        {
            Vector3 start =
                new Vector3(
                    horizontal.x,
                    transform.position.y +
                    groundProbeHeight,
                    horizontal.z);

            RaycastHit[] hits =
                Physics.RaycastAll(
                    start,
                    Vector3.down,
                    groundProbeDistance,
                    groundMask,
                    QueryTriggerInteraction.Ignore);

            bool found = false;
            float bestDistance =
                float.PositiveInfinity;

            bestHit = default;

            foreach (RaycastHit hit in hits)
            {
                if (hit.distance >= bestDistance)
                    continue;

                if (hit.normal.y <
                    minimumGroundNormalY)
                {
                    continue;
                }

                Collider collider =
                    hit.collider;

                if (collider == null)
                    continue;

                if (collider
                        .GetComponentInParent<
                            CombatantRuntime>() != null)
                {
                    continue;
                }

                if (collider
                        .GetComponentInParent<
                            LockableDoor>() != null ||
                    collider
                        .GetComponentInParent<
                            EnvironmentalDestructible>() != null ||
                    collider.attachedRigidbody != null)
                {
                    continue;
                }

                bestDistance =
                    hit.distance;

                bestHit = hit;
                found = true;
            }

            return found;
        }

        private bool IsBlockedByWorld(
            Vector3 groundPoint)
        {
            Vector3 bottom =
                groundPoint +
                Vector3.up *
                (agentRadius + 0.08f);

            Vector3 top =
                groundPoint +
                Vector3.up * 1.45f;

            Collider[] overlaps =
                Physics.OverlapCapsule(
                    bottom,
                    top,
                    agentRadius,
                    obstacleMask,
                    QueryTriggerInteraction.Ignore);

            foreach (Collider collider
                     in overlaps)
            {
                if (collider == null)
                    continue;

                if (collider
                        .GetComponentInParent<
                            CombatantRuntime>() != null)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private IEnumerable<Node> Neighbours(Node node)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dz == 0)
                        continue;

                    int nx = node.x + dx;
                    int nz = node.z + dz;
                    if (nx < 0 || nz < 0 || nx >= _width || nz >= _height)
                        continue;

                    Node candidate =
                        _nodes[nx, nz];

                    float verticalDelta =
                        candidate.world.y -
                        node.world.y;

                    if (verticalDelta >
                            maxStepHeight ||
                        verticalDelta <
                            -maxWalkDrop)
                    {
                        continue;
                    }

                    if (dx != 0 && dz != 0)
                    {
                        Node sideA =
                            _nodes[
                                node.x + dx,
                                node.z];

                        Node sideB =
                            _nodes[
                                node.x,
                                node.z + dz];

                        if (!sideA.walkable ||
                            !sideB.walkable)
                        {
                            continue;
                        }

                        if (Mathf.Abs(
                                sideA.world.y -
                                node.world.y) >
                                maxStepHeight ||
                            Mathf.Abs(
                                sideB.world.y -
                                node.world.y) >
                                maxStepHeight)
                        {
                            continue;
                        }
                    }

                    yield return candidate;
                }
            }
        }

        private Node ClosestNode(Vector3 world)
        {
            EnsureBuilt();

            int x = Mathf.Clamp(Mathf.FloorToInt((world.x - _origin.x) / cellSize), 0, _width - 1);
            int z = Mathf.Clamp(Mathf.FloorToInt((world.z - _origin.z) / cellSize), 0, _height - 1);

            Node direct = _nodes[x, z];
            if (direct.walkable)
                return direct;

            for (int radius = 1; radius <= 3; radius++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dz = -radius; dz <= radius; dz++)
                    {
                        int nx = x + dx;
                        int nz = z + dz;
                        if (nx < 0 || nz < 0 || nx >= _width || nz >= _height)
                            continue;

                        if (_nodes[nx, nz].walkable)
                            return _nodes[nx, nz];
                    }
                }
            }

            return direct;
        }

        private void EnsureBuilt()
        {
            if (_nodes == null)
                Build();
        }

        private void PrepareNode(Node node)
        {
            if (node == null || node.searchId == _searchId)
                return;

            node.searchId = _searchId;
            node.g = float.PositiveInfinity;
            node.h = 0f;
            node.parent = null;
            node.closed = false;
        }

        private void ResetSearchIds()
        {
            _searchId = 1;
            for (int x = 0; x < _width; x++)
                for (int z = 0; z < _height; z++)
                    _nodes[x, z].searchId = 0;
        }

        private static float Heuristic(Node a, Node b)
        {
            return Vector3.Distance(a.world, b.world);
        }

        private static List<Vector3> Retrace(Node start, Node end)
        {
            var result = new List<Vector3>();
            Node current = end;

            while (current != null && current != start)
            {
                result.Add(current.world);
                current = current.parent;
            }

            result.Reverse();
            return result;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(
                transform.position,
                new Vector3(worldSize.x, 0.1f, worldSize.y));
        }
    }
}
