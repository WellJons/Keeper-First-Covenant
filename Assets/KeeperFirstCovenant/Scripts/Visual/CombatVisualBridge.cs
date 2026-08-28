using KeeperFirstCovenant.Combat;
using UnityEngine;

namespace KeeperFirstCovenant.Visual
{
    [DisallowMultipleComponent]
    public sealed class CombatVisualBridge : MonoBehaviour
    {
        [SerializeField] private CombatantRuntime combatant;
        [SerializeField] private PaperDollMotionAnimator motion;
        [SerializeField] private PaperDollCharacterVisual visual;

        private void Awake()
        {
            if (combatant == null)
                combatant = GetComponentInParent<CombatantRuntime>();
            if (motion == null)
                motion = GetComponentInChildren<PaperDollMotionAnimator>();
            if (visual == null)
                visual = GetComponentInChildren<PaperDollCharacterVisual>();
        }

        private void OnEnable()
        {
            CombatActionExecutor.ActionResolved += OnActionResolved;
        }

        private void OnDisable()
        {
            CombatActionExecutor.ActionResolved -= OnActionResolved;
        }

        private void OnActionResolved(
            CombatActionDefinition action,
            CombatantRuntime actor,
            CombatantRuntime target,
            CombatActionResult result)
        {
            if (combatant == null || actor != combatant || action == null || motion == null || !result.Executed)
                return;

            if (target != null)
            {
                Vector3 direction = target.transform.position - actor.transform.position;
                visual?.FaceWorldDirection(direction);
            }

            switch (action.category)
            {
                case CombatActionCategory.Melee:
                    motion.PlayLightAttack();
                    break;
                case CombatActionCategory.Ranged:
                    motion.PlayHeavyAttack();
                    break;
                case CombatActionCategory.Spell:
                case CombatActionCategory.Support:
                case CombatActionCategory.Control:
                case CombatActionCategory.Unique:
                    motion.PlayCast();
                    break;
            }
        }
    }
}
