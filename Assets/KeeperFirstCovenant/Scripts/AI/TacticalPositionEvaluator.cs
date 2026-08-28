using System.Linq;
using KeeperFirstCovenant.Combat;
using UnityEngine;

namespace KeeperFirstCovenant.AI
{
    public static class TacticalPositionEvaluator
    {
        public static bool TryFindBestDestination(
            CombatantRuntime actor,
            CombatantRuntime target,
            CombatActionDefinition action,
            TacticalGrid3D grid,
            out Vector3 destination)
        {
            destination = actor != null
                ? actor.transform.position
                : Vector3.zero;

            if (actor == null ||
                target == null ||
                grid == null ||
                !actor.IsAlive ||
                !target.IsAlive)
            {
                return false;
            }

            var candidates =
                grid.GetReachableCells(
                    actor.transform.position,
                    actor.RemainingMovement);

            candidates.Add(
                grid.SnapToCell(
                    actor.transform.position));

            float bestScore = float.MinValue;
            bool found = false;

            foreach (Vector3 candidate in candidates)
            {
                if (IsOccupied(
                        actor,
                        candidate,
                        grid.CellSize))
                {
                    continue;
                }

                float score =
                    ScorePosition(
                        actor,
                        target,
                        action,
                        candidate);

                if (score <= bestScore)
                    continue;

                bestScore = score;
                destination = candidate;
                found = true;
            }

            return found;
        }

        private static float ScorePosition(
            CombatantRuntime actor,
            CombatantRuntime target,
            CombatActionDefinition action,
            Vector3 position)
        {
            float distance =
                Vector3.Distance(
                    position,
                    target.transform.position);

            float score =
                -distance * 2.25f;

            ElementalSurfaceSystem surfaces =
                ElementalSurfaceSystem.Instance;

            if (surfaces != null)
            {
                float hazard =
                    surfaces.GetHazardCostAt(
                        position,
                        actor);

                score -= hazard * 90f;
            }

            float heightAdvantage =
                position.y -
                target.transform.position.y;

            score +=
                Mathf.Clamp(
                    heightAdvantage * 8f,
                    -30f,
                    30f);

            bool lineOfSight =
                TacticalLineOfSight.Instance == null ||
                TacticalLineOfSight.Instance
                    .HasLineOfSightFromPoint(
                        position,
                        target);

            if (lineOfSight)
                score += 20f;

            if (action != null)
            {
                bool inRange =
                    distance <=
                    action.rangeMeters + 0.05f;

                if (inRange && lineOfSight)
                    score += 500f;

                bool rangedStyle =
                    action.category ==
                        CombatActionCategory.Ranged ||
                    action.category ==
                        CombatActionCategory.Spell ||
                    action.category ==
                        CombatActionCategory.Control;

                if (rangedStyle &&
                    distance < 2.5f)
                {
                    score -= 35f;
                }

                if (action.category ==
                        CombatActionCategory.Melee &&
                    distance >
                        action.rangeMeters + 0.25f)
                {
                    score -= distance * 4f;
                }
            }

            return score;
        }

        private static bool IsOccupied(
            CombatantRuntime actor,
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
                    x != actor &&
                    x.IsAlive &&
                    Vector3.Distance(
                        x.transform.position,
                        point) <= radius);
        }
    }
}
