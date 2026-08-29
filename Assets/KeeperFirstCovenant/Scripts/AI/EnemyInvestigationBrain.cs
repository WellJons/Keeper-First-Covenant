using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.World;
using UnityEngine;

namespace KeeperFirstCovenant.AI
{
    [RequireComponent(typeof(CombatantRuntime))]
    [RequireComponent(typeof(PerceptionSensor))]
    public sealed class EnemyInvestigationBrain : MonoBehaviour
    {
        [SerializeField]
        private TacticalGrid3D navigation;

        [SerializeField, Min(0.2f)]
        private float stoppingDistance = 1.3f;

        [SerializeField, Min(0.1f)]
        private float repathInterval = 0.45f;

        private CombatantRuntime _owner;
        private PerceptionSensor _sensor;
        private TacticalUnitMover _mover;
        private float _nextRepath;

        private void Awake()
        {
            _owner =
                GetComponent<CombatantRuntime>();

            _sensor =
                GetComponent<PerceptionSensor>();

            _mover =
                GetComponent<TacticalUnitMover>();
        }

        private void Start()
        {
            if (navigation == null)
            {
                navigation =
                    FindFirstObjectByType<
                        TacticalGrid3D>();
            }

            if (_mover == null)
            {
                _mover =
                    gameObject.AddComponent<
                        TacticalUnitMover>();
            }
        }

        private void Update()
        {
            if (_owner == null ||
                !_owner.IsAlive ||
                _sensor == null ||
                navigation == null)
            {
                return;
            }

            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            if (director != null &&
                director.State ==
                    CombatState.Active)
            {
                return;
            }

            if (_sensor.Awareness !=
                AwarenessLevel.Suspicious)
            {
                return;
            }

            Vector3 destination =
                _sensor.LastStimulusPosition;

            float distance =
                Vector3.Distance(
                    transform.position,
                    destination);

            if (distance <= stoppingDistance)
                return;

            if (Time.unscaledTime <
                _nextRepath)
            {
                return;
            }

            _nextRepath =
                Time.unscaledTime +
                repathInterval;

            if (_mover.IsMoving)
                return;

            _mover.TryMoveExploration(
                navigation,
                destination);
        }
    }
}
