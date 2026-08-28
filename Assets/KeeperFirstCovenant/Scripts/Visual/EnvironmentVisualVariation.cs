using UnityEngine;

namespace KeeperFirstCovenant.Visual
{
    [DisallowMultipleComponent]
    public sealed class EnvironmentVisualVariation : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Vector2 uniformScaleRange = new Vector2(0.9f, 1.1f);
        [SerializeField] private bool allowHorizontalMirror = true;
        [SerializeField] private float yOffsetRange = 0.02f;

        private Vector3 _baseScale = Vector3.one;
        private Vector3 _basePosition;

        private void Awake()
        {
            if (visualRoot == null)
                visualRoot = transform.Find("Visual");

            if (visualRoot == null)
                return;

            _baseScale = visualRoot.localScale;
            _basePosition = visualRoot.localPosition;

            int seed = Mathf.Abs(GetInstanceID());
            float t = (seed % 1000) / 999f;
            float scale = Mathf.Lerp(
                Mathf.Min(uniformScaleRange.x, uniformScaleRange.y),
                Mathf.Max(uniformScaleRange.x, uniformScaleRange.y),
                t);

            Vector3 nextScale = _baseScale * scale;

            if (allowHorizontalMirror && ((seed / 7) & 1) == 1)
                nextScale.x *= -1f;

            visualRoot.localScale = nextScale;

            if (yOffsetRange > 0f)
            {
                float offset = (((seed / 13) % 1000) / 999f * 2f - 1f) * yOffsetRange;
                visualRoot.localPosition = _basePosition + Vector3.up * offset;
            }
        }

        public void Configure(
            Transform target,
            Vector2 scaleRange,
            bool mirror,
            float verticalOffset = 0.02f)
        {
            visualRoot = target;
            uniformScaleRange = scaleRange;
            allowHorizontalMirror = mirror;
            yOffsetRange = Mathf.Max(0f, verticalOffset);
        }
    }
}
