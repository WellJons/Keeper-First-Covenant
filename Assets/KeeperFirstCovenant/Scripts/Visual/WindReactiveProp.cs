using UnityEngine;

namespace KeeperFirstCovenant.Visual
{
    [DisallowMultipleComponent]
    public sealed class WindReactiveProp : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] renderers;
        [SerializeField, Range(0f, 0.35f)] private float windStrength = 0.05f;
        [SerializeField, Range(0f, 8f)] private float windSpeed = 1.2f;
        [SerializeField, Range(0f, 4f)] private float windScale = 0.7f;
        [SerializeField, Range(0.25f, 6f)] private float baseLock = 2.2f;
        [SerializeField, Range(0f, 1f)] private float gustStrength = 0.2f;
        [SerializeField] private bool randomizePhase = true;

        private static readonly int WindStrengthId = Shader.PropertyToID("_WindStrength");
        private static readonly int WindSpeedId = Shader.PropertyToID("_WindSpeed");
        private static readonly int WindScaleId = Shader.PropertyToID("_WindScale");
        private static readonly int WindPhaseId = Shader.PropertyToID("_WindPhase");
        private static readonly int BaseLockId = Shader.PropertyToID("_BaseLock");
        private static readonly int GustStrengthId = Shader.PropertyToID("_GustStrength");

        private MaterialPropertyBlock _block;
        private float _phase;

        public float WindStrength => windStrength;
        public float WindSpeed => windSpeed;

        private void Awake()
        {
            CacheRenderers();

            _phase = randomizePhase
                ? Mathf.Abs(GetInstanceID() % 2048) * 0.071f
                : 0f;

            Apply();
        }

        private void OnEnable()
        {
            CacheRenderers();
            Apply();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            CacheRenderers();
            Apply();
        }
#endif

        public void Configure(
            float strength,
            float speed,
            float spatialScale,
            float stiffness,
            float gust,
            bool useRandomPhase = true)
        {
            windStrength = Mathf.Clamp(strength, 0f, 0.35f);
            windSpeed = Mathf.Clamp(speed, 0f, 8f);
            windScale = Mathf.Clamp(spatialScale, 0f, 4f);
            baseLock = Mathf.Clamp(stiffness, 0.25f, 6f);
            gustStrength = Mathf.Clamp01(gust);
            randomizePhase = useRandomPhase;

            if (!Application.isPlaying)
                _phase = 0f;

            Apply();
        }

        private void CacheRenderers()
        {
            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        private void Apply()
        {
            if (renderers == null || renderers.Length == 0)
                return;

            if (_block == null)
                _block = new MaterialPropertyBlock();

            foreach (SpriteRenderer renderer in renderers)
            {
                if (renderer == null)
                    continue;

                renderer.GetPropertyBlock(_block);
                _block.SetFloat(WindStrengthId, windStrength);
                _block.SetFloat(WindSpeedId, windSpeed);
                _block.SetFloat(WindScaleId, windScale);
                _block.SetFloat(WindPhaseId, _phase);
                _block.SetFloat(BaseLockId, baseLock);
                _block.SetFloat(GustStrengthId, gustStrength);
                renderer.SetPropertyBlock(_block);
            }
        }
    }
}
