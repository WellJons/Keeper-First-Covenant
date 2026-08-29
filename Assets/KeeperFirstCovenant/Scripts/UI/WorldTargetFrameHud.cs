using System.Collections.Generic;
using KeeperFirstCovenant.World;
using UnityEngine;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public sealed class WorldTargetFrameHud : MonoBehaviour
    {
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        private readonly List<Image> strokes =
            new List<Image>();

        private CanvasGroup group;
        private RectTransform frame;
        private WorldInteractionController controller;
        private Camera worldCamera;
        private float nextLookup;

        private void Start()
        {
            Build();
            SetVisible(false);
        }

        private void Update()
        {
            if (controller == null &&
                Time.unscaledTime >= nextLookup)
            {
                nextLookup =
                    Time.unscaledTime + 0.4f;

                controller =
                    FindFirstObjectByType<
                        WorldInteractionController>();
            }

            if (worldCamera == null)
                worldCamera = Camera.main;

            if (controller == null ||
                worldCamera == null ||
                !controller.HasHoverTarget ||
                controller.CurrentCollider == null)
            {
                SetVisible(false);
                return;
            }

            if (!TryProjectBounds(
                    controller.CurrentCollider.bounds,
                    out Vector2 min,
                    out Vector2 max))
            {
                SetVisible(false);
                return;
            }

            const float padding = 10f;

            min -=
                Vector2.one * padding;

            max +=
                Vector2.one * padding;

            min.x =
                Mathf.Clamp(
                    min.x,
                    4f,
                    ReferenceWidth - 4f);

            min.y =
                Mathf.Clamp(
                    min.y,
                    4f,
                    ReferenceHeight - 4f);

            max.x =
                Mathf.Clamp(
                    max.x,
                    4f,
                    ReferenceWidth - 4f);

            max.y =
                Mathf.Clamp(
                    max.y,
                    4f,
                    ReferenceHeight - 4f);

            Vector2 size =
                max - min;

            if (size.x < 10f ||
                size.y < 10f)
            {
                SetVisible(false);
                return;
            }

            frame.anchoredPosition =
                min;

            frame.sizeDelta =
                size;

            Color color =
                controller.CurrentInRange
                    ? MainMenuTheme.Warm
                    : MainMenuTheme.Danger;

            foreach (Image stroke
                     in strokes)
            {
                stroke.color =
                    new Color(
                        color.r,
                        color.g,
                        color.b,
                        0.82f);
            }

            SetVisible(true);
        }

        private void Build()
        {
            var canvasObject =
                new GameObject(
                    "WorldTargetFrameCanvas",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler));

            canvasObject.transform.SetParent(
                transform,
                false);

            Canvas canvas =
                canvasObject.GetComponent<Canvas>();

            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;

            canvas.sortingOrder = 535;

            CanvasScaler scaler =
                canvasObject.GetComponent<CanvasScaler>();

            scaler.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;

            scaler.referenceResolution =
                new Vector2(
                    ReferenceWidth,
                    ReferenceHeight);

            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect =
                canvasObject.GetComponent<RectTransform>();

            frame =
                MenuUiFactory.CreateRect(
                    "TargetFrame",
                    canvasRect);

            frame.anchorMin =
                frame.anchorMax =
                    Vector2.zero;

            frame.pivot =
                Vector2.zero;

            group =
                frame.gameObject
                    .AddComponent<CanvasGroup>();

            BuildCorner(
                "TL",
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                true,
                true);

            BuildCorner(
                "TR",
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                false,
                true);

            BuildCorner(
                "BL",
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                true,
                false);

            BuildCorner(
                "BR",
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                false,
                false);
        }

        private void BuildCorner(
            string suffix,
            Vector2 anchor,
            Vector2 pivot,
            bool left,
            bool top)
        {
            const float length = 24f;
            const float thickness = 2f;

            Image horizontal =
                MenuUiFactory.CreateImage(
                    "H_" + suffix,
                    frame,
                    MainMenuTheme.Warm);

            RectTransform h =
                horizontal.rectTransform;

            h.anchorMin =
                h.anchorMax =
                    anchor;

            h.pivot = pivot;

            h.sizeDelta =
                new Vector2(
                    length,
                    thickness);

            h.anchoredPosition =
                Vector2.zero;

            if (!left)
            {
                h.localScale =
                    new Vector3(
                        -1f,
                        1f,
                        1f);
            }

            Image vertical =
                MenuUiFactory.CreateImage(
                    "V_" + suffix,
                    frame,
                    MainMenuTheme.Warm);

            RectTransform v =
                vertical.rectTransform;

            v.anchorMin =
                v.anchorMax =
                    anchor;

            v.pivot = pivot;

            v.sizeDelta =
                new Vector2(
                    thickness,
                    length);

            v.anchoredPosition =
                Vector2.zero;

            if (!top)
            {
                v.localScale =
                    new Vector3(
                        1f,
                        -1f,
                        1f);
            }

            horizontal.raycastTarget = false;
            vertical.raycastTarget = false;

            strokes.Add(horizontal);
            strokes.Add(vertical);
        }

        private bool TryProjectBounds(
            Bounds bounds,
            out Vector2 min,
            out Vector2 max)
        {
            min =
                new Vector2(
                    float.PositiveInfinity,
                    float.PositiveInfinity);

            max =
                new Vector2(
                    float.NegativeInfinity,
                    float.NegativeInfinity);

            Vector3 center =
                bounds.center;

            Vector3 ext =
                bounds.extents;

            bool any = false;

            for (int x = -1;
                 x <= 1;
                 x += 2)
            {
                for (int y = -1;
                     y <= 1;
                     y += 2)
                {
                    for (int z = -1;
                         z <= 1;
                         z += 2)
                    {
                        Vector3 world =
                            center +
                            Vector3.Scale(
                                ext,
                                new Vector3(
                                    x,
                                    y,
                                    z));

                        Vector3 screen =
                            worldCamera
                                .WorldToScreenPoint(
                                    world);

                        if (screen.z <= 0f)
                            continue;

                        float sx =
                            Screen.width > 0
                                ? screen.x /
                                  Screen.width *
                                  ReferenceWidth
                                : screen.x;

                        float sy =
                            Screen.height > 0
                                ? screen.y /
                                  Screen.height *
                                  ReferenceHeight
                                : screen.y;

                        min =
                            Vector2.Min(
                                min,
                                new Vector2(
                                    sx,
                                    sy));

                        max =
                            Vector2.Max(
                                max,
                                new Vector2(
                                    sx,
                                    sy));

                        any = true;
                    }
                }
            }

            return any;
        }

        private void SetVisible(
            bool visible)
        {
            if (group == null)
                return;

            group.alpha =
                visible ? 1f : 0f;

            group.blocksRaycasts = false;
            group.interactable = false;
        }
    }
}
