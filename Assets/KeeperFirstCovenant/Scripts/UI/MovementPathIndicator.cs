using KeeperFirstCovenant.Player;
using UnityEngine;

namespace KeeperFirstCovenant.UI
{
    [RequireComponent(typeof(LineRenderer))]
    public sealed class MovementPathIndicator : MonoBehaviour
    {
        [SerializeField]
        private TacticalPlayerController controller;

        [SerializeField, Min(0.01f)]
        private float lineWidth = 0.055f;

        [SerializeField]
        private Color validColor =
            new Color(0.35f, 0.95f, 0.55f, 0.95f);

        [SerializeField]
        private Color invalidColor =
            new Color(1f, 0.35f, 0.3f, 0.95f);

        [SerializeField]
        private float heightOffset = 0.06f;

        private LineRenderer _line;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _line.useWorldSpace = true;
            _line.widthMultiplier = lineWidth;
            _line.enabled = false;

            if (_line.sharedMaterial == null)
            {
                Shader shader =
                    Shader.Find("Sprites/Default");

                if (shader != null)
                    _line.material = new Material(shader);
            }
        }

        private void Start()
        {
            if (controller == null)
            {
                controller =
                    FindFirstObjectByType<
                        TacticalPlayerController>();
            }
        }

        private void LateUpdate()
        {
            if (controller == null ||
                !controller.HasMovementPreview ||
                controller.SelectedAction != null)
            {
                _line.enabled = false;
                return;
            }

            int count =
                controller.MovementPreviewPath.Count;

            _line.positionCount = count + 1;

            Vector3 start =
                controller.CurrentActor != null
                    ? controller.CurrentActor
                        .transform.position
                    : Vector3.zero;

            _line.SetPosition(
                0,
                start + Vector3.up * heightOffset);

            for (int i = 0; i < count; i++)
            {
                _line.SetPosition(
                    i + 1,
                    controller.MovementPreviewPath[i] +
                    Vector3.up * heightOffset);
            }

            Color color =
                controller.MovementPreviewValid
                    ? validColor
                    : invalidColor;

            _line.startColor = color;
            _line.endColor = color;
            _line.enabled = true;
        }
    }
}
