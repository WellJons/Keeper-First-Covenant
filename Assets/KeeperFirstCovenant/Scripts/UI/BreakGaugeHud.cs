using System.Collections.Generic;
using System.Linq;
using KeeperFirstCovenant.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public sealed class BreakGaugeHud : MonoBehaviour
    {
        private sealed class Marker
        {
            public GameObject root;
            public RectTransform rect;
            public Text name;
            public Text state;
            public RectTransform fill;
            public Image fillImage;
        }

        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        private readonly List<Marker> markers =
            new List<Marker>();

        private RectTransform markerRoot;
        private Camera worldCamera;

        private void Start()
        {
            Build();
        }

        private void Update()
        {
            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            if (director == null ||
                director.State != CombatState.Active)
            {
                HideAll();
                return;
            }

            if (worldCamera == null)
                worldCamera = Camera.main;

            if (worldCamera == null)
            {
                HideAll();
                return;
            }

            CombatantRuntime[] enemies =
                FindObjectsByType<
                        CombatantRuntime>(
                        FindObjectsSortMode.None)
                    .Where(value =>
                        value != null &&
                        value.IsAlive &&
                        value.Faction ==
                            CombatFaction.Enemy &&
                        director.IsParticipant(value))
                    .OrderBy(value =>
                        value.GetInstanceID())
                    .ToArray();

            EnsureMarkerCount(enemies.Length);

            for (int i = 0;
                 i < markers.Count;
                 i++)
            {
                bool used =
                    i < enemies.Length;

                markers[i].root.SetActive(used);

                if (!used)
                    continue;

                RefreshMarker(
                    markers[i],
                    enemies[i]);
            }
        }

        private void Build()
        {
            var canvasObject =
                new GameObject(
                    "BreakGaugeCanvas",
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

            canvas.sortingOrder = 548;

            CanvasScaler scaler =
                canvasObject.GetComponent<
                    CanvasScaler>();

            scaler.uiScaleMode =
                CanvasScaler.ScaleMode
                    .ScaleWithScreenSize;

            scaler.referenceResolution =
                new Vector2(
                    ReferenceWidth,
                    ReferenceHeight);

            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect =
                canvasObject.GetComponent<
                    RectTransform>();

            markerRoot =
                MenuUiFactory.CreateRect(
                    "BreakMarkers",
                    canvasRect);

            MenuUiFactory.Stretch(markerRoot);
        }

        private void RefreshMarker(
            Marker marker,
            CombatantRuntime enemy)
        {
            BreakGaugeComponent gauge =
                enemy.GetComponent<
                    BreakGaugeComponent>();

            if (gauge == null)
            {
                marker.root.SetActive(false);
                return;
            }

            Vector3 world =
                enemy.transform.position +
                Vector3.up * 2.15f;

            Vector3 screen =
                worldCamera.WorldToScreenPoint(
                    world);

            if (screen.z <= 0f)
            {
                marker.root.SetActive(false);
                return;
            }

            float x =
                Screen.width > 0
                    ? screen.x /
                      Screen.width *
                      ReferenceWidth
                    : screen.x;

            float y =
                Screen.height > 0
                    ? screen.y /
                      Screen.height *
                      ReferenceHeight
                    : screen.y;

            if (x < -100f ||
                x > ReferenceWidth + 100f ||
                y < -80f ||
                y > ReferenceHeight + 80f)
            {
                marker.root.SetActive(false);
                return;
            }

            marker.rect.anchoredPosition =
                new Vector2(x, y);

            marker.name.text =
                enemy.Definition != null &&
                !string.IsNullOrWhiteSpace(
                    enemy.Definition.displayName)
                    ? enemy.Definition.displayName
                    : "Противник";

            marker.fill.anchorMax =
                new Vector2(
                    gauge.Normalized,
                    1f);

            Color color =
                gauge.IsBroken
                    ? MainMenuTheme.Danger
                    : gauge.Normalized >= 0.72f
                        ? MainMenuTheme.Warm
                        : MainMenuTheme.Silver;

            marker.fillImage.color = color;

            marker.state.text =
                gauge.IsBroken
                    ? "СЛОМ"
                    : gauge.Normalized >= 0.72f
                        ? "НЕУСТОЙЧИВ"
                        : string.Empty;

            marker.state.color = color;
        }

        private void EnsureMarkerCount(
            int count)
        {
            while (markers.Count < count)
            {
                Image panel =
                    MenuUiFactory.CreateImage(
                        "BreakMarker_" +
                        markers.Count,
                        markerRoot,
                        new Color(
                            0.008f,
                            0.012f,
                            0.016f,
                            0.84f));

                RectTransform rect =
                    panel.rectTransform;

                rect.anchorMin =
                    rect.anchorMax =
                        Vector2.zero;

                rect.pivot =
                    new Vector2(
                        0.5f,
                        0.5f);

                rect.sizeDelta =
                    new Vector2(
                        188f,
                        44f);

                KeeperUiSkin.DecorateSection(panel);

                Text name =
                    MenuUiFactory.CreateText(
                        "Name",
                        panel.transform,
                        string.Empty,
                        11,
                        MainMenuTheme.Text,
                        TextAnchor.MiddleLeft);

                name.rectTransform.anchorMin =
                    new Vector2(0f, 0.48f);

                name.rectTransform.anchorMax =
                    new Vector2(0.72f, 1f);

                name.rectTransform.offsetMin =
                    new Vector2(9f, 0f);

                name.rectTransform.offsetMax =
                    Vector2.zero;

                Text state =
                    MenuUiFactory.CreateText(
                        "State",
                        panel.transform,
                        string.Empty,
                        10,
                        MainMenuTheme.Warm,
                        TextAnchor.MiddleRight);

                state.rectTransform.anchorMin =
                    new Vector2(0.58f, 0.48f);

                state.rectTransform.anchorMax =
                    Vector2.one;

                state.rectTransform.offsetMin =
                    Vector2.zero;

                state.rectTransform.offsetMax =
                    new Vector2(-8f, 0f);

                Image track =
                    MenuUiFactory.CreateImage(
                        "Track",
                        panel.transform,
                        new Color(
                            0.05f,
                            0.06f,
                            0.07f,
                            0.96f));

                RectTransform trackRect =
                    track.rectTransform;

                trackRect.anchorMin =
                    new Vector2(0f, 0f);

                trackRect.anchorMax =
                    new Vector2(1f, 0.42f);

                trackRect.offsetMin =
                    new Vector2(9f, 8f);

                trackRect.offsetMax =
                    new Vector2(-9f, -3f);

                Image fill =
                    MenuUiFactory.CreateImage(
                        "Fill",
                        track.transform,
                        MainMenuTheme.Silver);

                RectTransform fillRect =
                    fill.rectTransform;

                fillRect.anchorMin =
                    Vector2.zero;

                fillRect.anchorMax =
                    new Vector2(0f, 1f);

                fillRect.offsetMin =
                    Vector2.zero;

                fillRect.offsetMax =
                    Vector2.zero;

                markers.Add(
                    new Marker
                    {
                        root = panel.gameObject,
                        rect = rect,
                        name = name,
                        state = state,
                        fill = fillRect,
                        fillImage = fill
                    });
            }
        }

        private void HideAll()
        {
            foreach (Marker marker
                     in markers)
            {
                marker.root.SetActive(false);
            }
        }
    }
}
