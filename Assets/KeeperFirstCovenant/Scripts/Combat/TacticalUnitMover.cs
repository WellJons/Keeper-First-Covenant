using System;
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

        public static event Action<
            CombatantRuntime,
            Vector3,
            Vector3> BeforeStep;

        public static event Action<
            CombatantRuntime,
            Vector3,
            Vector3> StepCompleted;

        private void Awake()
        {
            _combatant = GetComponent<CombatantRuntime>();
        }

        public bool TryMoveTo(
            TacticalGrid3D grid,
            Vector3 destination,
            Action onComplete = null)
        {
            if (grid == null ||
                _combatant == null ||
                !_combatant.IsAlive ||
                IsMoving)
            {
                return false;
            }

            List<Vector3> path =
                grid.FindContinuousPath(
                    transform.position,
                    destination);

            if (path.Count == 0)
                return false;

            float pathLength =
                grid.CalculatePathLength(
                    path,
                    transform.position);

            if (pathLength <= 0f ||
                pathLength >
                _combatant.RemainingMovement + 0.01f)
            {
                return false;
            }

            _moveRoutine =
                StartCoroutine(
                    MoveRoutine(
                        path,
                        onComplete,
                        true));

            return true;
        }

        public bool TryMoveAlongPath(
            TacticalGrid3D grid,
            IReadOnlyList<Vector3> fullPath,
            float maxDistance,
            Action onComplete = null)
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

            float budget = Mathf.Min(
                Mathf.Max(0f, maxDistance),
                _combatant.RemainingMovement);

            if (budget <= 0.01f)
                return false;

            var trimmed = new List<Vector3>();
            Vector3 previous = transform.position;
            float used = 0f;

            for (int i = 0; i < fullPath.Count; i++)
            {
                Vector3 point = fullPath[i];
                float step =
                    Vector3.Distance(previous, point);

                if (used + step > budget + 0.01f)
                    break;

                trimmed.Add(point);
                used += step;
                previous = point;
            }

            if (trimmed.Count == 0 || used <= 0f)
                return false;

            _moveRoutine =
                StartCoroutine(
                    MoveRoutine(
                        trimmed,
                        onComplete,
                        true));

            return true;
        }

        public bool TryMoveExploration(
            TacticalGrid3D grid,
            Vector3 destination,
            Action onComplete = null)
        {
            if (grid == null ||
                _combatant == null ||
                !_combatant.IsAlive ||
                IsMoving)
            {
                return false;
            }

            List<Vector3> path =
                grid.FindContinuousPath(
                    transform.position,
                    destination);

            if (path.Count == 0)
                return false;

            _moveRoutine =
                StartCoroutine(
                    MoveRoutine(
                        path,
                        onComplete,
                        false));

            return true;
        }

        public void CancelMovement()
        {
            if (_moveRoutine == null)
                return;

            StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }

        private IEnumerator MoveRoutine(
            IReadOnlyList<Vector3> path,
            Action onComplete,
            bool consumeMovement)
        {
            for (int i = 0; i < path.Count; i++)
            {
                if (!_combatant.IsAlive)
                    break;

                Vector3 from = transform.position;
                Vector3 target = path[i];
                float stepCost =
                    Vector3.Distance(from, target);

                if (consumeMovement &&
                    !_combatant.TrySpendMovement(
                        stepCost))
                {
                    break;
                }

                BeforeStep?.Invoke(
                    _combatant,
                    from,
                    target);

                if (!_combatant.IsAlive)
                    break;

                while (Vector3.Distance(
                           transform.position,
                           target) > stopDistance)
                {
                    if (!_combatant.IsAlive)
                        break;

                    transform.position =
                        Vector3.MoveTowards(
                            transform.position,
                            target,
                            moveSpeed * Time.deltaTime);

                    yield return null;
                }

                if (!_combatant.IsAlive)
                    break;

                transform.position = target;

                StepCompleted?.Invoke(
                    _combatant,
                    from,
                    target);
            }

            _moveRoutine = null;

            if (_combatant.IsAlive)
                onComplete?.Invoke();
        }
    }
}
