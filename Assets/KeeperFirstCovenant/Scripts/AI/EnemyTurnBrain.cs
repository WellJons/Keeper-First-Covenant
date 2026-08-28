using System.Collections;
using System.Linq;
using KeeperFirstCovenant.Combat;
using UnityEngine;

namespace KeeperFirstCovenant.AI
{
    public sealed class EnemyTurnBrain : MonoBehaviour
    {
        [SerializeField] private TacticalGrid3D grid;
        [SerializeField, Min(0f)] private float thinkDelay = 0.25f;
        [SerializeField, Min(0f)] private float finishDelay = 0.2f;

        private Coroutine _routine;

        private void Start()
        {
            if (grid == null)
                grid = FindFirstObjectByType<TacticalGrid3D>();

            if (TurnCombatDirector.Instance != null)
                TurnCombatDirector.Instance.CurrentActorChanged += OnCurrentActorChanged;
        }

        private void OnDestroy()
        {
            if (TurnCombatDirector.Instance != null)
                TurnCombatDirector.Instance.CurrentActorChanged -= OnCurrentActorChanged;
        }

        private void OnCurrentActorChanged(CombatantRuntime actor)
        {
            if (actor == null || actor.Faction != CombatFaction.Enemy || !actor.IsAlive)
                return;

            if (_routine != null)
                StopCoroutine(_routine);

            _routine = StartCoroutine(TakeTurn(actor));
        }

        private IEnumerator TakeTurn(CombatantRuntime actor)
        {
            if (thinkDelay > 0f)
                yield return new WaitForSeconds(thinkDelay);

            CombatantRuntime target = FindBestTarget(actor);
            if (target == null)
            {
                EndTurn();
                yield break;
            }

            CombatActionDefinition action = ChooseAction(actor, target);

            if (action != null && IsInRange(actor, target, action))
            {
                CombatActionExecutor.Execute(actor, action, target);
            }
            else if (grid != null && actor.RemainingMovement > 0.01f)
            {
                var path = grid.FindPath(actor.transform.position, target.transform.position);

                if (path.Count > 0)
                {
                    TacticalUnitMover mover = actor.GetComponent<TacticalUnitMover>();
                    if (mover == null)
                        mover = actor.gameObject.AddComponent<TacticalUnitMover>();

                    bool finished = false;
                    mover.TryMoveAlongPath(
                        grid,
                        path,
                        actor.RemainingMovement,
                        this,
                        () => finished = true);

                    while (mover.IsMoving && actor.IsAlive)
                        yield return null;

                    if (finished && actor.IsAlive)
                    {
                        action = ChooseAction(actor, target);
                        if (action != null && IsInRange(actor, target, action))
                            CombatActionExecutor.Execute(actor, action, target);
                    }
                }
            }

            if (finishDelay > 0f)
                yield return new WaitForSeconds(finishDelay);

            EndTurn();
        }

        private static CombatantRuntime FindBestTarget(CombatantRuntime actor)
        {
            CombatantRuntime[] all =
                FindObjectsByType<CombatantRuntime>(FindObjectsSortMode.None);

            return all
                .Where(x =>
                    x != null &&
                    x.IsAlive &&
                    (x.Faction == CombatFaction.Player || x.Faction == CombatFaction.Ally))
                .OrderBy(x => Vector3.SqrMagnitude(x.transform.position - actor.transform.position))
                .FirstOrDefault();
        }

        private static CombatActionDefinition ChooseAction(
            CombatantRuntime actor,
            CombatantRuntime target)
        {
            CombatActionDefinition[] actions = actor.Definition?.startingActions;
            if (actions == null || actions.Length == 0)
                return null;

            CombatActionDefinition best = null;
            float bestScore = float.MinValue;

            foreach (CombatActionDefinition action in actions)
            {
                if (action == null ||
                    action.targetKind != TargetKind.Enemy ||
                    actor.CurrentActionPoints < action.actionPointCost ||
                    actor.CurrentMana < action.manaCost)
                {
                    continue;
                }

                float expectedDamage =
                    (action.damage.Minimum + action.damage.Maximum) * 0.5f;

                float rangeBonus = IsInRange(actor, target, action) ? 1000f : action.rangeMeters;
                float score = rangeBonus + expectedDamage;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = action;
                }
            }

            return best;
        }

        private static bool IsInRange(
            CombatantRuntime actor,
            CombatantRuntime target,
            CombatActionDefinition action)
        {
            if (actor == null || target == null || action == null)
                return false;

            return Vector3.Distance(
                       actor.transform.position,
                       target.transform.position) <= action.rangeMeters + 0.05f;
        }

        private void EndTurn()
        {
            _routine = null;

            if (TurnCombatDirector.Instance != null &&
                TurnCombatDirector.Instance.State == CombatState.Active)
            {
                TurnCombatDirector.Instance.EndCurrentTurn();
            }
        }
    }
}
