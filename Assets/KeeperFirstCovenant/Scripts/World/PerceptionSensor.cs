using System.Collections.Generic;
using System.Linq;
using KeeperFirstCovenant.Combat;
using UnityEngine;

namespace KeeperFirstCovenant.World
{
    [RequireComponent(typeof(CombatantRuntime))]
    public sealed class PerceptionSensor : MonoBehaviour
    {
        [Header("Vision")]
        [SerializeField, Min(1f)]
        private float visionRange = 15f;

        [SerializeField, Range(10f, 180f)]
        private float fieldOfView = 105f;

        [SerializeField, Min(0.1f)]
        private float eyeHeight = 1.35f;

        [SerializeField]
        private LayerMask visionBlockerMask = ~0;

        [Header("Suspicion")]
        [SerializeField, Min(1f)]
        private float detectionThreshold = 100f;

        [SerializeField, Min(0f)]
        private float visibleSuspicionPerSecond = 58f;

        [SerializeField, Min(0f)]
        private float movingNoiseSuspicionPerSecond = 30f;

        [SerializeField, Min(0f)]
        private float suspicionDecayPerSecond = 22f;

        [SerializeField, Min(0.03f)]
        private float scanInterval = 0.12f;

        [Header("Engagement")]
        [SerializeField, Min(5f)]
        private float engagementRadius = 24f;

        private readonly Dictionary<
            CombatantRuntime,
            float> _suspicion =
                new Dictionary<
                    CombatantRuntime,
                    float>();

        private CombatantRuntime _owner;
        private float _nextScan;

        public float HighestSuspicion =>
            _suspicion.Count == 0
                ? 0f
                : _suspicion.Values.Max();

        private void Awake()
        {
            _owner =
                GetComponent<CombatantRuntime>();
        }

        private void OnEnable()
        {
            WorldNoiseSystem.NoiseEmitted +=
                OnNoiseEmitted;
        }

        private void OnDisable()
        {
            WorldNoiseSystem.NoiseEmitted -=
                OnNoiseEmitted;
        }

        private void Update()
        {
            if (_owner == null ||
                !_owner.IsAlive ||
                _owner.Faction !=
                    CombatFaction.Enemy)
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

            if (Time.unscaledTime < _nextScan)
                return;

            float delta = scanInterval;
            _nextScan =
                Time.unscaledTime +
                scanInterval;

            Scan(delta);
        }

        private void Scan(float delta)
        {
            CombatantRuntime[] targets =
                FindObjectsByType<
                        CombatantRuntime>(
                        FindObjectsSortMode.None)
                    .Where(x =>
                        x != null &&
                        x.IsAlive &&
                        (x.Faction ==
                             CombatFaction.Player ||
                         x.Faction ==
                             CombatFaction.Ally))
                    .ToArray();

            var seenThisScan =
                new HashSet<CombatantRuntime>();

            foreach (CombatantRuntime target
                     in targets)
            {
                float gain = 0f;

                if (CanSee(target))
                {
                    StealthSignature signature =
                        target.GetComponent<
                            StealthSignature>();

                    float visibility =
                        signature != null
                            ? signature.VisibilityMultiplier
                            : 1f;

                    float distance =
                        Vector3.Distance(
                            transform.position,
                            target.transform.position);

                    float distanceFactor =
                        Mathf.Lerp(
                            1.25f,
                            0.55f,
                            Mathf.Clamp01(
                                distance /
                                visionRange));

                    gain +=
                        visibleSuspicionPerSecond *
                        visibility *
                        distanceFactor *
                        delta;
                }

                StealthSignature noiseSignature =
                    target.GetComponent<
                        StealthSignature>();

                if (noiseSignature != null &&
                    noiseSignature.IsMoving)
                {
                    float distance =
                        Vector3.Distance(
                            transform.position,
                            target.transform.position);

                    float radius =
                        noiseSignature
                            .CurrentMovementNoiseRadius;

                    if (radius > 0f &&
                        distance <= radius)
                    {
                        float factor =
                            1f -
                            Mathf.Clamp01(
                                distance / radius);

                        gain +=
                            movingNoiseSuspicionPerSecond *
                            (0.35f + factor) *
                            delta;
                    }
                }

                if (gain > 0f)
                {
                    seenThisScan.Add(target);

                    float current =
                        GetSuspicion(target);

                    SetSuspicion(
                        target,
                        current + gain);

                    if (GetSuspicion(target) >=
                        detectionThreshold)
                    {
                        Engage();
                        return;
                    }
                }
            }

            foreach (CombatantRuntime target
                     in _suspicion.Keys.ToArray())
            {
                if (target == null ||
                    !target.IsAlive)
                {
                    _suspicion.Remove(target);
                    continue;
                }

                if (seenThisScan.Contains(target))
                    continue;

                SetSuspicion(
                    target,
                    GetSuspicion(target) -
                    suspicionDecayPerSecond *
                    delta);
            }
        }

        private bool CanSee(
            CombatantRuntime target)
        {
            Vector3 targetPoint =
                target.transform.position +
                Vector3.up * 0.9f;

            Vector3 origin =
                transform.position +
                Vector3.up * eyeHeight;

            Vector3 delta =
                targetPoint - origin;

            float distance =
                delta.magnitude;

            if (distance >
                    visionRange ||
                distance <= 0.01f)
            {
                return false;
            }

            Vector3 flat =
                target.transform.position -
                transform.position;

            flat.y = 0f;

            if (flat.sqrMagnitude > 0.001f)
            {
                float angle =
                    Vector3.Angle(
                        transform.forward,
                        flat.normalized);

                if (angle >
                    fieldOfView * 0.5f)
                {
                    return false;
                }
            }

            RaycastHit[] hits =
                Physics.RaycastAll(
                    origin,
                    delta / distance,
                    distance,
                    visionBlockerMask,
                    QueryTriggerInteraction.Ignore);

            foreach (RaycastHit hit
                     in hits.OrderBy(x => x.distance))
            {
                Transform hitTransform =
                    hit.collider.transform;

                if (hitTransform == transform ||
                    hitTransform.IsChildOf(
                        transform))
                {
                    continue;
                }

                if (hitTransform ==
                        target.transform ||
                    hitTransform.IsChildOf(
                        target.transform))
                {
                    return true;
                }

                return false;
            }

            return true;
        }

        private void OnNoiseEmitted(
            WorldNoiseEvent noise)
        {
            if (_owner == null ||
                !_owner.IsAlive ||
                _owner.Faction !=
                    CombatFaction.Enemy)
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

            float distance =
                Vector3.Distance(
                    transform.position,
                    noise.Position);

            if (distance > noise.Radius)
                return;

            CombatantRuntime source =
                noise.Source != null
                    ? noise.Source.GetComponentInParent<
                        CombatantRuntime>()
                    : null;

            if (source == null ||
                !source.IsAlive ||
                (source.Faction !=
                     CombatFaction.Player &&
                 source.Faction !=
                     CombatFaction.Ally))
            {
                return;
            }

            float closeness =
                1f -
                Mathf.Clamp01(
                    distance /
                    Mathf.Max(
                        0.01f,
                        noise.Radius));

            float gain =
                25f *
                noise.Intensity *
                (0.5f + closeness);

            SetSuspicion(
                source,
                GetSuspicion(source) + gain);

            if (GetSuspicion(source) >=
                detectionThreshold)
            {
                Engage();
            }
        }

        private float GetSuspicion(
            CombatantRuntime target)
        {
            return _suspicion.TryGetValue(
                target,
                out float value)
                    ? value
                    : 0f;
        }

        private void SetSuspicion(
            CombatantRuntime target,
            float value)
        {
            if (target == null)
                return;

            float clamped =
                Mathf.Clamp(
                    value,
                    0f,
                    detectionThreshold);

            if (clamped <= 0.001f)
                _suspicion.Remove(target);
            else
                _suspicion[target] = clamped;
        }

        private void Engage()
        {
            WorldCombatEngagementService service =
                WorldCombatEngagementService.Instance;

            if (service != null)
            {
                service.BeginCombatAt(
                    transform.position,
                    engagementRadius);
                return;
            }

            CombatantRuntime[] participants =
                FindObjectsByType<
                    CombatantRuntime>(
                    FindObjectsSortMode.None);

            TurnCombatDirector.Instance
                ?.BeginCombat(participants);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(
                transform.position,
                visionRange);
        }
    }
}
