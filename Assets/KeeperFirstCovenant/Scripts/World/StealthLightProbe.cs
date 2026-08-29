using System.Linq;
using KeeperFirstCovenant.Combat;
using UnityEngine;

namespace KeeperFirstCovenant.World
{
    [RequireComponent(typeof(CombatantRuntime))]
    public sealed class StealthLightProbe : MonoBehaviour
    {
        [SerializeField, Min(0.05f)]
        private float sampleInterval = 0.18f;

        [SerializeField, Range(0.1f, 1f)]
        private float darknessVisibilityMultiplier = 0.58f;

        [SerializeField, Range(1f, 2f)]
        private float brightVisibilityMultiplier = 1.22f;

        [SerializeField, Min(0.1f)]
        private float intensityForFullExposure = 1.8f;

        [SerializeField]
        private LayerMask occlusionMask = ~0;

        [SerializeField]
        private bool testOcclusion = true;

        private float nextSample;

        public float Exposure01
        {
            get;
            private set;
        } = 1f;

        public float VisibilityMultiplier
        {
            get;
            private set;
        } = 1f;

        private void Start()
        {
            Sample();
        }

        private void Update()
        {
            if (Time.unscaledTime <
                nextSample)
            {
                return;
            }

            nextSample =
                Time.unscaledTime +
                sampleInterval;

            Sample();
        }

        public void Sample()
        {
            Vector3 point =
                transform.position +
                Vector3.up * 0.9f;

            float ambient =
                CalculateAmbientExposure();

            float local =
                CalculateLocalExposure(point);

            Exposure01 =
                Mathf.Clamp01(
                    Mathf.Max(
                        ambient,
                        local));

            VisibilityMultiplier =
                Mathf.Lerp(
                    darknessVisibilityMultiplier,
                    brightVisibilityMultiplier,
                    Exposure01);
        }

        private float CalculateAmbientExposure()
        {
            WorldTimeSystem time =
                WorldTimeSystem.Instance;

            if (time == null)
                return 0.72f;

            float visibility =
                time.VisibilityMultiplier;

            return Mathf.InverseLerp(
                0.58f,
                1f,
                visibility);
        }

        private float CalculateLocalExposure(
            Vector3 point)
        {
            Light[] lights =
                FindObjectsByType<Light>(
                    FindObjectsSortMode.None);

            float total = 0f;

            foreach (Light light in lights)
            {
                if (light == null ||
                    !light.enabled ||
                    !light.gameObject.activeInHierarchy ||
                    light.intensity <= 0.001f ||
                    light.type ==
                        LightType.Directional)
                {
                    continue;
                }

                float contribution =
                    EvaluateLight(
                        light,
                        point);

                total += contribution;

                if (total >= 1f)
                    return 1f;
            }

            return Mathf.Clamp01(total);
        }

        private float EvaluateLight(
            Light light,
            Vector3 point)
        {
            Vector3 delta =
                point -
                light.transform.position;

            float distance =
                delta.magnitude;

            if (light.range <= 0.01f ||
                distance > light.range)
            {
                return 0f;
            }

            if (light.type ==
                LightType.Spot)
            {
                Vector3 direction =
                    delta.sqrMagnitude >
                    0.001f
                        ? delta.normalized
                        : light.transform.forward;

                float angle =
                    Vector3.Angle(
                        light.transform.forward,
                        direction);

                if (angle >
                    light.spotAngle * 0.5f)
                {
                    return 0f;
                }
            }

            if (testOcclusion &&
                distance > 0.08f)
            {
                Vector3 origin =
                    light.transform.position;

                RaycastHit[] hits =
                    Physics.RaycastAll(
                        origin,
                        delta.normalized,
                        distance,
                        occlusionMask,
                        QueryTriggerInteraction.Ignore);

                foreach (RaycastHit hit
                         in hits.OrderBy(value =>
                             value.distance))
                {
                    Transform hitTransform =
                        hit.collider.transform;

                    if (hitTransform ==
                            transform ||
                        hitTransform.IsChildOf(
                            transform) ||
                        hitTransform ==
                            light.transform ||
                        hitTransform.IsChildOf(
                            light.transform))
                    {
                        continue;
                    }

                    return 0f;
                }
            }

            float distanceFactor =
                1f -
                Mathf.Clamp01(
                    distance /
                    Mathf.Max(
                        0.01f,
                        light.range));

            distanceFactor *=
                distanceFactor;

            float intensityFactor =
                Mathf.Clamp01(
                    light.intensity /
                    Mathf.Max(
                        0.01f,
                        intensityForFullExposure));

            return
                distanceFactor *
                intensityFactor;
        }
    }
}
