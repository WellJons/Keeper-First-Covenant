using UnityEngine;

namespace KeeperFirstCovenant.Environment
{
    [DefaultExecutionOrder(-500)]
    public sealed class EnvironmentWindController : MonoBehaviour
    {
        private static readonly int WindId = Shader.PropertyToID("_KfcWind");
        private static readonly int WindPulseId = Shader.PropertyToID("_KfcWindPulse");

        [SerializeField, Range(0f, 360f)] private float directionDegrees = 32f;
        [SerializeField, Range(0f, 2f)] private float baseStrength = 0.82f;
        [SerializeField, Range(0f, 1f)] private float gustStrength = 0.24f;
        [SerializeField, Min(0.05f)] private float gustFrequency = 0.19f;
        [SerializeField, Min(0f)] private float gustNoiseSpeed = 0.12f;

        private void OnEnable()
        {
            ApplyGlobals(0f);
        }

        private void Update()
        {
            float time = Time.time;
            float slowWave = Mathf.Sin(time * gustFrequency * Mathf.PI * 2f);
            float noise = Mathf.PerlinNoise(time * gustNoiseSpeed, 0.417f) * 2f - 1f;
            float gust = Mathf.Clamp01(0.5f + slowWave * 0.28f + noise * 0.22f);
            ApplyGlobals(gust);
        }

        private void OnDisable()
        {
            Shader.SetGlobalVector(WindId, new Vector4(1f, 0f, 0f, 0f));
            Shader.SetGlobalFloat(WindPulseId, 0f);
        }

        private void ApplyGlobals(float gust)
        {
            float radians = directionDegrees * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            Shader.SetGlobalVector(
                WindId,
                new Vector4(direction.x, direction.y, baseStrength, gustStrength));
            Shader.SetGlobalFloat(WindPulseId, gust);
        }
    }
}
