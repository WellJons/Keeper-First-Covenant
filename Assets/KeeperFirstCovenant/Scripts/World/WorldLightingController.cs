using System.Linq;
using UnityEngine;

namespace KeeperFirstCovenant.World
{
    public sealed class WorldLightingController : MonoBehaviour
    {
        [SerializeField]
        private Light sun;

        [SerializeField, Min(0f)]
        private float dayIntensity = 1.15f;

        [SerializeField, Min(0f)]
        private float nightIntensity = 0.08f;

        [SerializeField, Range(0f, 8f)]
        private float dayAmbientIntensity = 1f;

        [SerializeField, Range(0f, 8f)]
        private float nightAmbientIntensity = 0.28f;

        [SerializeField]
        private float sunYaw = 155f;

        private void Start()
        {
            if (sun == null)
            {
                sun =
                    FindObjectsByType<Light>(
                            FindObjectsSortMode.None)
                        .FirstOrDefault(x =>
                            x != null &&
                            x.type ==
                                LightType.Directional);
            }

            Apply();
        }

        private void LateUpdate()
        {
            Apply();
        }

        private void Apply()
        {
            WorldTimeSystem time =
                WorldTimeSystem.Instance;

            if (time == null)
                return;

            float hour =
                time.Hour;

            float solar =
                Mathf.Sin(
                    ((hour - 6f) / 12f) *
                    Mathf.PI);

            float daylight =
                Mathf.Clamp01(solar);

            if (sun != null)
            {
                float pitch =
                    (hour / 24f) *
                    360f -
                    90f;

                sun.transform.rotation =
                    Quaternion.Euler(
                        pitch,
                        sunYaw,
                        0f);

                sun.intensity =
                    Mathf.Lerp(
                        nightIntensity,
                        dayIntensity,
                        daylight);

                sun.enabled =
                    sun.intensity > 0.001f;
            }

            RenderSettings.ambientIntensity =
                Mathf.Lerp(
                    nightAmbientIntensity,
                    dayAmbientIntensity,
                    daylight);
        }
    }
}
