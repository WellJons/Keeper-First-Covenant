using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KeeperFirstCovenant.Combat;
using UnityEngine;

namespace KeeperFirstCovenant.AI
{
    public sealed class EnemyTurnBrain :
        MonoBehaviour
    {
        [SerializeField]
        private TacticalGrid3D grid;

        [SerializeField, Min(0f)]
        private float thinkDelay = 0.25f;

        [SerializeField, Min(0f)]
        private float finishDelay = 0.2f;

        private Coroutine _routine;

        private void Start()
        {
            if (grid == null)
            {
                grid =
                    FindFirstObjectByType<
                        TacticalGrid3D>();
            }

            if (TurnCombatDirector.Instance != null)
            {
                TurnCombatDirector.Instance
                    .CurrentActorChanged +=
                    OnCurrentActorChanged;
            }
        }

        private void OnDestroy()
        {
            if (TurnCombatDirector.Instance != null)
            {
                TurnCombatDirector.Instance
                    .CurrentActorChanged -=
                    OnCurrentActorChanged;
            }
        }

        private void OnCurrentActorChanged(
            CombatantRuntime actor)
        {
            if (actor == null ||
                actor.Faction !=
                    CombatFaction.Enemy ||
                !actor.IsAlive)
            {
                return;
            }

            if (_routine != null)
                StopCoroutine(_routine);

            _routine =
                StartCoroutine(
                    TakeTurn(actor));
        }

        private IEnumerator TakeTurn(
            CombatantRuntime actor)
        {
            if (thinkDelay > 0f)
            {
                yield return
                    new WaitForSeconds(
                        thinkDelay);
            }

            CombatantRuntime target =
                FindBestTarget(actor);

            if (target == null)
            {
                EndTurn();
                yield break;
            }

            CombatActionDefinition action =
                ChooseAction(actor, target);

            if (action != null &&
                CanUseNow(
                    actor,
                    target,
                    action))
            {
                yield return
                    ExecuteActionWithDefense(
                        actor,
                        target,
                        action);
            }
            else if (grid != null &&
                     actor.RemainingMovement >
                     0.01f)
            {
                if (TacticalPositionEvaluator
                        .TryFindBestDestination(
                            actor,
                            target,
                            action,
                            grid,
                            out Vector3 destination))
                {
                    List<Vector3> path =
                        grid.FindContinuousPath(
                            actor.transform.position,
                            destination);

                    if (path.Count > 0)
                    {
                        TacticalUnitMover mover =
                            actor.GetComponent<
                                TacticalUnitMover>();

                        if (mover == null)
                        {
                            mover =
                                actor.gameObject
                                    .AddComponent<
                                        TacticalUnitMover>();
                        }

                        mover.TryMoveAlongPath(
                            grid,
                            path,
                            actor.RemainingMovement);

                        while (mover.IsMoving &&
                               actor.IsAlive)
                        {
                            yield return null;
                        }
                    }

                    if (actor.IsAlive)
                    {
                        action =
                            ChooseAction(
                                actor,
                                target);

                        if (action != null &&
                            CanUseNow(
                                actor,
                                target,
                                action))
                        {
                            yield return
                                ExecuteActionWithDefense(
                                    actor,
                                    target,
                                    action);
                        }
                    }
                }
            }

            if (finishDelay > 0f)
            {
                yield return
                    new WaitForSeconds(
                        finishDelay);
            }

            EndTurn();
        }

        private static bool CanUseNow(
            CombatantRuntime actor,
            CombatantRuntime target,
            CombatActionDefinition action)
        {
            if (actor == null ||
                target == null ||
                action == null)
            {
                return false;
            }

            if (actor.CurrentActionPoints <
                action.actionPointCost ||
                actor.CurrentMana <
                action.manaCost)
            {
                return false;
            }

            TacticalTargetPreview preview =
                CombatTargetingService.Analyze(
                    actor,
                    action,
                    target);

            return preview.Valid;
        }

        private static CombatantRuntime
            FindBestTarget(
                CombatantRuntime actor)
        {
            CombatantRuntime[] all =
                FindObjectsByType<
                    CombatantRuntime>(
                    FindObjectsSortMode.None);

            return all
                .Where(x =>
                    x != null &&
                    x.IsAlive &&
                    (x.Faction ==
                         CombatFaction.Player ||
                     x.Faction ==
                         CombatFaction.Ally))
                .OrderBy(x =>
                    Vector3.SqrMagnitude(
                        x.transform.position -
                        actor.transform.position))
                .FirstOrDefault();
        }

        private static CombatActionDefinition
            ChooseAction(
                CombatantRuntime actor,
                CombatantRuntime target)
        {
            CombatActionDefinition[] actions =
                actor.GetAvailableActions();

            if (actions == null ||
                actions.Length == 0)
            {
                return null;
            }

            CombatActionDefinition best = null;
            float bestScore = float.MinValue;

            foreach (CombatActionDefinition action
                     in actions)
            {
                if (action == null ||
                    action.targetKind !=
                        TargetKind.Enemy ||
                    actor.CurrentActionPoints <
                        action.actionPointCost ||
                    actor.CurrentMana <
                        action.manaCost)
                {
                    continue;
                }

                float expectedDamage =
                    (action.damage.Minimum +
                     action.damage.Maximum) *
                    0.5f;

                TacticalTargetPreview preview =
                    CombatTargetingService.Analyze(
                        actor,
                        action,
                        target);

                float score =
                    expectedDamage +
                    action.rangeMeters;

                ElementalSurfaceSystem surfaces =
                    ElementalSurfaceSystem.Instance;

                if (surfaces != null)
                {
                    score +=
                        surfaces
                            .GetImpactReactionScore(
                                action.damageType,
                                target.transform.position);
                }

                if (preview.Valid)
                {
                    score += 1000f;
                    score +=
                        preview.HitChance * 0.2f;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    best = action;
                }
            }

            return best;
        }

        private static IEnumerator
            ExecuteActionWithDefense(
                CombatantRuntime actor,
                CombatantRuntime target,
                CombatActionDefinition action)
        {
            if (actor == null ||
                target == null ||
                action == null ||
                !actor.IsAlive ||
                !target.IsAlive)
            {
                yield break;
            }

            ActiveDefenseOutcome outcome =
                ActiveDefenseOutcome.None;

            ActiveDefenseSystem defense =
                ActiveDefenseSystem.Instance;

            if (defense != null &&
                defense.CanReact(
                    target,
                    action))
            {
                var resolution =
                    new ActiveDefenseResolution();

                yield return defense
                    .ResolveIncomingAttack(
                        actor,
                        target,
                        action,
                        resolution);

                outcome =
                    resolution.Outcome;
            }

            if (actor == null ||
                target == null ||
                !actor.IsAlive ||
                !target.CanBeTargeted)
            {
                yield break;
            }

            CombatActionExecutor
                .ExecuteWithDefense(
                    actor,
                    action,
                    target,
                    outcome);
        }

        private void EndTurn()
        {
            _routine = null;

            if (TurnCombatDirector.Instance != null &&
                TurnCombatDirector.Instance.State ==
                CombatState.Active)
            {
                TurnCombatDirector.Instance
                    .EndCurrentTurn();
            }
        }
    }
}
