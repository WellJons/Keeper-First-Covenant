using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    public sealed class CameraImpactShake : MonoBehaviour
    {
        private float _remaining;
        private float _duration;
        private float _amplitude;
        private float _frequency;
        private Vector3 _lastOffset;
        private float _seed;

        public void AddImpulse(
            float amplitude,
            float duration,
            float frequency)
        {
            if (amplitude <= 0f ||
                duration <= 0f)
            {
                return;
            }

            _amplitude =
                Mathf.Max(_amplitude, amplitude);

            _duration =
                Mathf.Max(_duration, duration);

            _remaining =
                Mathf.Max(_remaining, duration);

            _frequency =
                Mathf.Max(0.1f, frequency);

            _seed += 17.317f;
        }

        private void LateUpdate()
        {
            // Remove only the offset this component added last frame,
            // preserving camera movement from another controller.
            transform.localPosition -= _lastOffset;
            _lastOffset = Vector3.zero;

            if (_remaining <= 0f)
                return;

            _remaining =
                Mathf.Max(
                    0f,
                    _remaining -
                    Time.unscaledDeltaTime);

            float normalized =
                _duration > 0.001f
                    ? _remaining / _duration
                    : 0f;

            float envelope =
                normalized * normalized;

            float t =
                Time.unscaledTime *
                _frequency;

            Vector3 noise =
                new Vector3(
                    Mathf.PerlinNoise(
                        _seed,
                        t) - 0.5f,
                    Mathf.PerlinNoise(
                        _seed + 11.1f,
                        t + 3.7f) - 0.5f,
                    Mathf.PerlinNoise(
                        _seed + 29.4f,
                        t + 7.9f) - 0.5f) * 2f;

            _lastOffset =
                noise *
                _amplitude *
                envelope;

            transform.localPosition += _lastOffset;
        }

        private void OnDisable()
        {
            transform.localPosition -= _lastOffset;
            _lastOffset = Vector3.zero;
            _remaining = 0f;
        }
    }
}
