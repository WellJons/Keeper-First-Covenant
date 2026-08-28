using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    [RequireComponent(typeof(CombatantRuntime))]
    public sealed class TacticalUnitMover : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float moveSpeed = 4.5f;
        [SerializeField, Min(0f)] private float stopDistance = 0.03f;

        private CombatantRuntime _combatant;
        private Coroutine _moveRoutine;

        public bool IsMoving => _moveRoutine != null;

        private void Awake()
        {
            _combatant = GetComponent<CombatantRuntime>();
        }

        public bool TryMoveTo(
            TacticalGrid3D grid,
            Vector3 destination,
            System.Action onComplete = null)
        {
            if (grid == null || _combatant == null || !_combatant.IsAlive || IsMoving)
                return false;

            List<Vector3> path = grid.FindPath(transform.position, destination);
            if (path.Count == 0)
                return false;

            float pathLength = grid.CalculatePathLength(path, transform.position);
            if (pathLength <= 0f || pathLength > _combatant.RemainingMovement + 0.01f)
                return false;

            if (!_combatant.TrySpendMovement(pathLength))
                return false;

            _moveRoutine = StartCoroutine(MoveRoutine(path, onComplete));
            return true;
        }

        public bool TryMoveAlongPath(
            TacticalGrid3D grid,
            IReadOnlyList<Vector3> fullPath,
            float maxDistance,
            System.Action onComplete = null)
        {
            if (grid == null ||
                fullPath == null ||
                fullPath.Count == 0 ||
                _combatant == null ||
                !_combatant.IsAlive ||
                IsMoving)
            {
                return false;
            }

            float budget = Mathf.Min(Mathf.Max(0f, maxDistance), _combatant.RemainingMovement);
            if (budget <= 0.01f)
                return false;

            var trimmed = new List<Vector3>();
            Vector3 previous = transform.position;
            float used = 0f;

            for (int i = 0; i < fullPath.Count; i++)
            {
                Vector3 point = fullPath[i];
                float step = Vector3.Distance(previous, point);

                if (used + step > budget + 0.01f)
                    break;

                trimmed.Add(point);
                used += step;
                previous = point;
            }

            if (trimmed.Count == 0 || used <= 0f)
                return false;

            if (!_combatant.TrySpendMovement(used))
                return false;

            _moveRoutine = StartCoroutine(MoveRoutine(trimmed, onComplete));
            return true;
        }

        public void CancelMovement()
        {
            if (_moveRoutine == null)
                return;

            StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }

        private IEnumerator MoveRoutine(IReadOnlyList<Vector3> path, System.Action onComplete)
        {
            for (int i = 0; i < path.Count; i++)
            {
                Vector3 target = path[i];

                while (Vector3.Distance(transform.position, target) > stopDistance)
                {
                    transform.position = Vector3.MoveTowards(
                        transform.position,
                        target,
                        moveSpeed * Time.deltaTime);

                    yield return null;
                }

                transform.position = target;
            }

            _moveRoutine = null;
            onComplete?.Invoke();
        }
    }
}
