using KeeperFirstCovenant.Combat;
using UnityEngine;

namespace KeeperFirstCovenant.World
{
    [RequireComponent(typeof(CombatantRuntime))]
    public sealed class StealthSignature : MonoBehaviour
    {
        [SerializeField, Min(0f)]
        private float standingVisibility = 1f;

        [SerializeField, Min(0f)]
        private float crouchedVisibility = 0.55f;

        [SerializeField, Min(0f)]
        private float standingMoveNoiseRadius = 3.5f;

        [SerializeField, Min(0f)]
        private float crouchedMoveNoiseRadius = 1.4f;

        [SerializeField, Range(0.2f, 1f)]
        private float crouchedMoveSpeedMultiplier = 0.68f;

        [SerializeField]
        private bool crouched;

        private TacticalUnitMover _mover;

        public bool IsCrouched => crouched;

        public float VisibilityMultiplier =>
            crouched
                ? crouchedVisibility
                : standingVisibility;

        public float CurrentMovementNoiseRadius =>
            crouched
                ? crouchedMoveNoiseRadius
                : standingMoveNoiseRadius;

        public float MovementSpeedMultiplier =>
            crouched
                ? crouchedMoveSpeedMultiplier
                : 1f;

        public bool IsMoving =>
            _mover != null &&
            _mover.IsMoving;

        private void Awake()
        {
            _mover =
                GetComponent<TacticalUnitMover>();
        }

        private void Start()
        {
            if (_mover == null)
            {
                _mover =
                    GetComponent<TacticalUnitMover>();
            }
        }

        public void SetCrouched(bool value)
        {
            crouched = value;
        }

        public void ToggleCrouched()
        {
            crouched = !crouched;
        }
    }
}
