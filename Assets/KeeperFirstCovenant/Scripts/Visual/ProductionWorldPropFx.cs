using UnityEngine;

namespace KeeperFirstCovenant.Visual
{
    public sealed class ProductionWorldPropFx : MonoBehaviour
    {
        [SerializeField] private Light pulseLight;
        [SerializeField] private float baseIntensity = 1f;
        [SerializeField] private float pulseAmplitude = 0.2f;
        [SerializeField] private float pulseSpeed = 1.5f;
        [SerializeField] private Transform animatedVisual;
        [SerializeField] private float bobAmplitude = 0f;
        [SerializeField] private float bobSpeed = 1f;

        private Vector3 _baseLocalPosition;
        private float _phase;

        private void Awake()
        {
            if (animatedVisual != null)
                _baseLocalPosition = animatedVisual.localPosition;

            _phase = Mathf.Abs(GetInstanceID() % 1024) * 0.013f;
        }

        private void Update()
        {
            float wave = Mathf.Sin(Time.time * pulseSpeed + _phase);

            if (pulseLight != null)
                pulseLight.intensity = Mathf.Max(0f, baseIntensity + wave * pulseAmplitude);

            if (animatedVisual != null && bobAmplitude > 0f)
            {
                Vector3 p = _baseLocalPosition;
                p.y += Mathf.Sin(Time.time * bobSpeed + _phase) * bobAmplitude;
                animatedVisual.localPosition = p;
            }
        }

        public void Configure(
            Light lightSource,
            float intensity,
            float amplitude,
            float speed,
            Transform visual = null,
            float bob = 0f,
            float bobFrequency = 1f)
        {
            pulseLight = lightSource;
            baseIntensity = intensity;
            pulseAmplitude = amplitude;
            pulseSpeed = speed;
            animatedVisual = visual;
            bobAmplitude = bob;
            bobSpeed = bobFrequency;

            if (animatedVisual != null)
                _baseLocalPosition = animatedVisual.localPosition;
        }
    }
}
