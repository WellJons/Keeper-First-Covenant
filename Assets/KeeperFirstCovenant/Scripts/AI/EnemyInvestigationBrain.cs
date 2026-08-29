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

        [Header("Search at stimulus")]
        [SerializeField, Min(0.5f)]
        private float searchDuration = 3.2f;

        [SerializeField, Min(0.15f)]
        private float searchTurnInterval = 0.65f;

        [SerializeField, Range(10f, 120f)]
        private float searchTurnAngle = 58f;

        private CombatantRuntime _owner;
        private PerceptionSensor _sensor;
        private TacticalUnitMover _mover;
        private WorldFacing _facing;
        private float _nextRepath;
        private float _searchUntil;
        private float _nextSearchTurn;
        private Vector3 _lastDestination;
        private Vector3 _searchBaseDirection;
        private int _searchStep;

        private void Awake()
        {
            _owner =
                GetComponent<CombatantRuntime>();

            _sensor =
                GetComponent<PerceptionSensor>();

            _mover =
                GetComponent<TacticalUnitMover>();

            _facing =
                GetComponent<WorldFacing>();
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
                ResetSearch();
                return;
            }

            if (Time.timeScale <= 0f)
                return;

            Vector3 destination =
                _sensor.LastStimulusPosition;

            if (Vector3.SqrMagnitude(
                    destination -
                    _lastDestination) >
                0.25f)
            {
                _lastDestination =
                    destination;

                _searchUntil = 0f;
                _searchStep = 0;
            }

            float distance =
                Vector3.Distance(
                    transform.position,
                    destination);

            if (distance <= stoppingDistance)
            {
                SearchArea(destination);
                return;
            }

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
        private void SearchArea(
            Vector3 destination)
        {
            if (_facing == null)
            {
                _facing =
                    GetComponent<WorldFacing>();
            }

            if (_searchUntil <= 0f)
            {
                _searchUntil =
                    Time.unscaledTime +
                    searchDuration;

                _nextSearchTurn =
                    Time.unscaledTime;

                Vector3 baseDirection =
                    destination -
                    transform.position;

                baseDirection.y = 0f;

                _searchBaseDirection =
                    baseDirection.sqrMagnitude >
                    0.001f
                        ? baseDirection.normalized
                        : _facing != null
                            ? _facing.Forward
                            : transform.forward;

                _searchStep = 0;
            }

            if (Time.unscaledTime >
                _searchUntil)
            {
                return;
            }

            if (Time.unscaledTime <
                _nextSearchTurn)
            {
                return;
            }

            _nextSearchTurn =
                Time.unscaledTime +
                searchTurnInterval;

            float angle;

            switch (_searchStep % 4)
            {
                case 0:
                    angle = 0f;
                    break;
                case 1:
                    angle = -searchTurnAngle;
                    break;
                case 2:
                    angle = searchTurnAngle;
                    break;
                default:
                    angle = 0f;
                    break;
            }

            _searchStep++;

            Vector3 direction =
                Quaternion.Euler(
                    0f,
                    angle,
                    0f) *
                _searchBaseDirection;

            _facing?.FaceDirection(
                direction);
        }

        private void ResetSearch()
        {
            _searchUntil = 0f;
            _nextSearchTurn = 0f;
            _searchStep = 0;
        }

        }
    }
}
