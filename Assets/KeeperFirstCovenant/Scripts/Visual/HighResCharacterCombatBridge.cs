using KeeperFirstCovenant.Combat;
using UnityEngine;

namespace KeeperFirstCovenant.Visual
{
    [DisallowMultipleComponent]
    public sealed class HighResCharacterCombatBridge : MonoBehaviour
    {
        [SerializeField] private CombatantRuntime combatant;
        [SerializeField] private HighResFrameCharacter2D animator2D;

        private bool _returnToCombatIdle;

        private void Awake()
        {
            if (combatant == null)
                combatant = GetComponentInParent<CombatantRuntime>();

            if (animator2D == null)
                animator2D = GetComponentInChildren<HighResFrameCharacter2D>();
        }

        private void OnEnable()
        {
            CombatActionExecutor.ActionResolved += OnActionResolved;

            if (combatant != null)
            {
                combatant.Damaged += OnDamaged;
                combatant.Died += OnDied;
            }

            if (animator2D != null)
                animator2D.AnimationFinished += OnAnimationFinished;
        }

        private void OnDisable()
        {
            CombatActionExecutor.ActionResolved -= OnActionResolved;

            if (combatant != null)
            {
                combatant.Damaged -= OnDamaged;
                combatant.Died -= OnDied;
            }

            if (animator2D != null)
                animator2D.AnimationFinished -= OnAnimationFinished;
        }

        private void OnActionResolved(
            CombatActionDefinition action,
            CombatantRuntime actor,
            CombatantRuntime target,
            CombatActionResult result)
        {
            if (combatant == null ||
                animator2D == null ||
                actor != combatant ||
                action == null ||
                !result.Executed)
            {
                return;
            }

            if (target != null)
            {
                Vector3 direction = target.transform.position - actor.transform.position;
                animator2D.FaceWorldDirection(direction);
            }

            _returnToCombatIdle = true;

            switch (action.category)
            {
                case CombatActionCategory.Melee:
                    animator2D.PlayOneShot(CharacterFrameState.AttackLight);
                    break;

                case CombatActionCategory.Ranged:
                    animator2D.PlayOneShot(CharacterFrameState.AttackHeavy);
                    break;

                case CombatActionCategory.Spell:
                case CombatActionCategory.Support:
                case CombatActionCategory.Control:
                case CombatActionCategory.Unique:
                    animator2D.PlayOneShot(CharacterFrameState.Cast);
                    break;

                default:
                    animator2D.PlayOneShot(CharacterFrameState.Interact);
                    break;
            }
        }

        private void OnDamaged(CombatantRuntime source, DamagePacket packet)
        {
            if (animator2D == null || source == null || !source.IsAlive)
                return;

            _returnToCombatIdle = true;
            animator2D.PlayOneShot(
                packet.Critical || packet.Amount >= 15
                    ? CharacterFrameState.CriticalHit
                    : CharacterFrameState.Hit);
        }

        private void OnDied(CombatantRuntime source)
        {
            if (animator2D == null)
                return;

            _returnToCombatIdle = false;
            animator2D.PlayOneShot(CharacterFrameState.Death);
        }

        private void OnAnimationFinished(CharacterFrameState finishedState)
        {
            if (!_returnToCombatIdle || animator2D == null)
                return;

            switch (finishedState)
            {
                case CharacterFrameState.AttackLight:
                case CharacterFrameState.AttackHeavy:
                case CharacterFrameState.Cast:
                case CharacterFrameState.Interact:
                case CharacterFrameState.Hit:
                case CharacterFrameState.CriticalHit:
                case CharacterFrameState.Knockdown:
                    _returnToCombatIdle = false;
                    animator2D.PlayLoop(CharacterFrameState.CombatIdle);
                    break;
            }
        }
    }
}
