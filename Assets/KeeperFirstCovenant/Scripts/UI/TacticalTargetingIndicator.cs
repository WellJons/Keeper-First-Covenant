using KeeperFirstCovenant.Player;
using UnityEngine;

namespace KeeperFirstCovenant.UI
{
    [RequireComponent(typeof(LineRenderer))]
    public sealed class TacticalTargetingIndicator : MonoBehaviour
    {
        [SerializeField] private TacticalPlayerController controller;
        [SerializeField, Range(12, 96)] private int segments = 48;
        [SerializeField] private float lineWidth = 0.045f;
        [SerializeField] private float heightOffset = 0.08f;

        private LineRenderer _line;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _line.useWorldSpace = true;
            _line.loop = true;
            _line.widthMultiplier = lineWidth;
            _line.positionCount = segments;
            _line.enabled = false;

            if (_line.sharedMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader != null)
                    _line.material = new Material(shader);
            }
        }

        private void Start()
        {
            if (controller == null)
                controller = FindFirstObjectByType<TacticalPlayerController>();
        }

        private void LateUpdate()
        {
            if (controller == null ||
                controller.SelectedAction == null ||
                !controller.HasHoverPreview)
            {
                _line.enabled = false;
                return;
            }

            float radius = controller.SelectedAction.areaRadius;

            if (radius <= 0.05f)
            {
                _line.enabled = false;
                return;
            }

            Vector3 center =
                controller.CurrentPreview.EffectPoint +
                Vector3.up * heightOffset;

            for (int i = 0; i < segments; i++)
            {
                float angle =
                    i / (float)segments * Mathf.PI * 2f;

                _line.SetPosition(
                    i,
                    center + new Vector3(
                        Mathf.Cos(angle) * radius,
                        0f,
                        Mathf.Sin(angle) * radius));
            }

            _line.enabled = true;
        }
    }
}
