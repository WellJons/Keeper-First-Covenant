using System;
using System.Collections.Generic;
using System.Linq;
using KeeperFirstCovenant.World;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    [Serializable]
    public sealed class BossPhaseStep
    {
        [Range(0.01f, 0.99f)]
        public float healthThreshold = 0.65f;

        public string phaseName = "Новая фаза";
        public Color phaseColor = Color.white;

        public CombatActionDefinition[] unlockActions;

        [Min(0)]
        public int barrierGain;

        public bool resetBreakGauge = true;
        public bool resetActionCooldowns;

        public SurfaceType pulseSurface =
            SurfaceType.None;

        [Min(0f)]
        public float pulseRadius;

        [Min(0)]
        public int pulseDurationTurns = 1;
    }

    public readonly struct BossPhaseEvent
    {
        public readonly CombatantRuntime Boss;
        public readonly int PhaseNumber;
        public readonly BossPhaseStep Step;

        public BossPhaseEvent(
            CombatantRuntime boss,
            int phaseNumber,
            BossPhaseStep step)
        {
            Boss = boss;
            PhaseNumber = phaseNumber;
            Step = step;
        }
    }

    [RequireComponent(typeof(CombatantRuntime))]
    public sealed class BossPhaseController :
        MonoBehaviour
    {
        [SerializeField]
        private BossPhaseStep[] phases =
            Array.Empty<BossPhaseStep>();

        private readonly HashSet<
            CombatActionDefinition>
            unlockedActions =
                new HashSet<
                    CombatActionDefinition>();

        private CombatantRuntime owner;
        private int nextPhaseIndex;

        public static event Action<BossPhaseEvent>
            PhaseChanged;

        public int CurrentPhaseNumber =>
            nextPhaseIndex + 1;

        public bool HasRemainingPhases =>
            phases != null &&
            nextPhaseIndex < phases.Length;

        private void Awake()
        {
            owner =
                GetComponent<
                    CombatantRuntime>();

            NormalizePhases();
        }

        private void OnEnable()
        {
            if (owner == null)
            {
                owner =
                    GetComponent<
                        CombatantRuntime>();
            }

            if (owner != null)
            {
                owner.Damaged +=
                    OnDamaged;

                owner.Died +=
                    OnDied;
            }
        }

        private void OnDisable()
        {
            if (owner != null)
            {
                owner.Damaged -=
                    OnDamaged;

                owner.Died -=
                    OnDied;
            }
        }

        public void Configure(
            BossPhaseStep[] values)
        {
            phases =
                values ??
                Array.Empty<BossPhaseStep>();

            nextPhaseIndex = 0;
            unlockedActions.Clear();

            NormalizePhases();
        }

        public void CollectActions(
            List<CombatActionDefinition> output)
        {
            if (output == null)
                return;

            foreach (CombatActionDefinition action
                     in unlockedActions)
            {
                if (action != null &&
                    !output.Contains(action))
                {
                    output.Add(action);
                }
            }
        }

        private void OnDamaged(
            CombatantRuntime combatant,
            DamagePacket packet)
        {
            if (combatant != owner ||
                owner == null ||
                !owner.IsAlive ||
                owner.Definition == null)
            {
                return;
            }

            EvaluatePhases();
        }

        private void EvaluatePhases()
        {
            if (phases == null ||
                phases.Length == 0 ||
                owner == null ||
                owner.Definition == null)
            {
                return;
            }

            float healthRatio =
                owner.CurrentHealth /
                (float)Mathf.Max(
                    1,
                    owner.Definition.maxHealth);

            while (nextPhaseIndex <
                       phases.Length &&
                   phases[nextPhaseIndex] != null &&
                   healthRatio <=
                       phases[nextPhaseIndex]
                           .healthThreshold)
            {
                BossPhaseStep step =
                    phases[nextPhaseIndex];

                nextPhaseIndex++;

                EnterPhase(
                    step,
                    nextPhaseIndex + 1);
            }
        }

        private void EnterPhase(
            BossPhaseStep step,
            int phaseNumber)
        {
            if (step == null ||
                owner == null ||
                !owner.IsAlive)
            {
                return;
            }

            if (step.unlockActions != null)
            {
                foreach (CombatActionDefinition action
                         in step.unlockActions)
                {
                    if (action != null)
                        unlockedActions.Add(action);
                }
            }

            if (step.barrierGain > 0)
            {
                owner.AddBarrier(
                    step.barrierGain);
            }

            if (step.resetBreakGauge)
            {
                owner.GetComponent<
                        BreakGaugeComponent>()
                    ?.ResetGauge();
            }

            if (step.resetActionCooldowns)
            {
                CombatActionStateComponent
                    .Ensure(owner)
                    ?.ResetState();
            }

            if (step.pulseSurface !=
                    SurfaceType.None &&
                step.pulseRadius > 0.05f)
            {
                ElementalSurfaceSystem
                    .Instance
                    ?.CreateOrReact(
                        step.pulseSurface,
                        owner.transform.position,
                        step.pulseRadius,
                        Mathf.Max(
                            1,
                            step.pulseDurationTurns),
                        owner.gameObject);
            }

            WorldNoiseSystem.Emit(
                owner.transform.position,
                22f,
                owner.gameObject,
                1.5f);

            PhaseChanged?.Invoke(
                new BossPhaseEvent(
                    owner,
                    phaseNumber,
                    step));
        }

        private void OnDied(
            CombatantRuntime combatant)
        {
            unlockedActions.Clear();
        }

        private void NormalizePhases()
        {
            phases =
                (phases ??
                 Array.Empty<BossPhaseStep>())
                .Where(value =>
                    value != null)
                .OrderByDescending(value =>
                    value.healthThreshold)
                .ToArray();
        }
    }
}
