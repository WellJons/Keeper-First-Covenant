using System.Collections.Generic;
using System.Linq;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.World;
using UnityEngine;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public sealed class StealthAwarenessHud : MonoBehaviour
    {
        private sealed class Marker
        {
            public GameObject root;
            public RectTransform rect;
            public Text symbol;
            public RectTransform fill;
        }

        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        private CanvasGroup group;
        private Text title;
        private Text state;
        private Text detail;
        private RectTransform suspicionFill;
        private RectTransform markerRoot;

        private readonly List<Marker> markers =
            new List<Marker>();

        private Camera worldCamera;
        private CombatantRuntime leader;
        private StealthSignature signature;
        private float nextLookup;

        private void Start()
        {
            Build();
            SetPanelVisible(false);
        }

        private void Update()
        {
            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            if (director != null &&
                (director.State == CombatState.Active ||
                 director.State == CombatState.Defeat))
            {
                SetPanelVisible(false);
                HideAllMarkers();
                return;
            }

            if (worldCamera == null)
                worldCamera = Camera.main;

            if (leader == null ||
                !leader.IsAlive ||
                Time.unscaledTime >= nextLookup)
            {
                nextLookup =
                    Time.unscaledTime + 0.4f;

                ResolveLeader();
            }

            if (leader == null)
            {
                SetPanelVisible(false);
                HideAllMarkers();
                return;
            }

            Refresh();
        }

        private void Build()
        {
            var canvasObject =
                new GameObject(
                    "StealthAwarenessCanvas",
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

            canvas.sortingOrder = 560;

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

            Image panel =
                MenuUiFactory.CreateImage(
                    "StealthPanel",
                    canvasRect,
                    new Color(
                        MainMenuTheme.Panel.r,
                        MainMenuTheme.Panel.g,
                        MainMenuTheme.Panel.b,
                        0.91f));

            RectTransform panelRect =
                panel.rectTransform;

            MenuUiFactory.SetAnchoredRect(
                panelRect,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(32f, 34f),
                new Vector2(365f, 124f));

            KeeperUiSkin.DecorateMajorPanel(
                panel,
                false);

            group =
                panel.gameObject.AddComponent<CanvasGroup>();

            title =
                MenuUiFactory.CreateText(
                    "Title",
                    panel.transform,
                    "СКРЫТНОСТЬ",
                    15,
                    MainMenuTheme.Silver,
                    TextAnchor.MiddleLeft);

            title.rectTransform.anchorMin =
                new Vector2(0f, 0.67f);

            title.rectTransform.anchorMax =
                Vector2.one;

            title.rectTransform.offsetMin =
                new Vector2(18f, 0f);

            title.rectTransform.offsetMax =
                new Vector2(-18f, -8f);

            state =
                MenuUiFactory.CreateText(
                    "State",
                    panel.transform,
                    string.Empty,
                    13,
                    MainMenuTheme.Text,
                    TextAnchor.MiddleRight);

            state.rectTransform.anchorMin =
                new Vector2(0.42f, 0.67f);

            state.rectTransform.anchorMax =
                Vector2.one;

            state.rectTransform.offsetMin =
                Vector2.zero;

            state.rectTransform.offsetMax =
                new Vector2(-18f, -8f);

            Image track =
                MenuUiFactory.CreateImage(
                    "SuspicionTrack",
                    panel.transform,
                    new Color(
                        0.06f,
                        0.07f,
                        0.08f,
                        0.92f));

            RectTransform trackRect =
                track.rectTransform;

            trackRect.anchorMin =
                new Vector2(0f, 0.42f);

            trackRect.anchorMax =
                new Vector2(1f, 0.42f);

            trackRect.offsetMin =
                new Vector2(18f, -4f);

            trackRect.offsetMax =
                new Vector2(-18f, 4f);

            Image fill =
                MenuUiFactory.CreateImage(
                    "SuspicionFill",
                    track.transform,
                    MainMenuTheme.Silver);

            suspicionFill =
                fill.rectTransform;

            suspicionFill.anchorMin =
                Vector2.zero;

            suspicionFill.anchorMax =
                new Vector2(0f, 1f);

            suspicionFill.offsetMin =
                Vector2.zero;

            suspicionFill.offsetMax =
                Vector2.zero;

            detail =
                MenuUiFactory.CreateText(
                    "Detail",
                    panel.transform,
                    string.Empty,
                    12,
                    MainMenuTheme.MutedText,
                    TextAnchor.MiddleLeft);

            detail.rectTransform.anchorMin =
                Vector2.zero;

            detail.rectTransform.anchorMax =
                new Vector2(1f, 0.35f);

            detail.rectTransform.offsetMin =
                new Vector2(18f, 5f);

            detail.rectTransform.offsetMax =
                new Vector2(-18f, 0f);

            markerRoot =
                MenuUiFactory.CreateRect(
                    "AwarenessMarkers",
                    canvasRect);

            MenuUiFactory.Stretch(markerRoot);
        }

        private void Refresh()
        {
            PerceptionSensor[] sensors =
                FindObjectsByType<PerceptionSensor>(
                    FindObjectsSortMode.None);

            var relevant =
                sensors
                    .Where(sensor =>
                        sensor != null &&
                        sensor.Owner != null &&
                        sensor.Owner.IsAlive &&
                        sensor.Owner.Faction ==
                            CombatFaction.Enemy)
                    .Select(sensor =>
                        new
                        {
                            sensor,
                            suspicion =
                                sensor.GetSuspicionFor(
                                    leader),
                            normalized =
                                sensor.GetSuspicionNormalizedFor(
                                    leader)
                        })
                    .Where(value =>
                        value.suspicion > 0.01f)
                    .OrderByDescending(value =>
                        value.normalized)
                    .ToArray();

            float highest =
                relevant.Length > 0
                    ? relevant[0].normalized
                    : 0f;

            bool crouched =
                signature != null &&
                signature.IsCrouched;

            bool showPanel =
                crouched ||
                highest > 0.001f;

            SetPanelVisible(showPanel);

            if (showPanel)
            {
                RefreshPanel(
                    highest,
                    relevant.Length > 0
                        ? relevant[0].sensor
                        : null);
            }

            RefreshMarkers(
                relevant
                    .Select(value =>
                        value.sensor)
                    .ToArray());
        }

        private void RefreshPanel(
            float normalized,
            PerceptionSensor strongest)
        {
            suspicionFill.anchorMax =
                new Vector2(
                    Mathf.Clamp01(normalized),
                    1f);

            Color meterColor =
                normalized >= 0.82f
                    ? MainMenuTheme.Danger
                    : normalized >= 0.25f
                        ? MainMenuTheme.Warm
                        : MainMenuTheme.Silver;

            suspicionFill.GetComponent<Image>()
                .color = meterColor;

            if (strongest == null ||
                normalized <= 0.001f)
            {
                state.text =
                    "НЕ ЗАМЕЧЕН";

                state.color =
                    MainMenuTheme.Silver;
            }
            else if (strongest.Awareness ==
                     AwarenessLevel.Suspicious)
            {
                state.text =
                    "ПОДОЗРЕНИЕ";

                state.color =
                    MainMenuTheme.Warm;
            }
            else
            {
                state.text =
                    "ВАС ИЩУТ";

                state.color =
                    meterColor;
            }

            float noise =
                signature != null
                    ? signature
                        .CurrentMovementNoiseRadius
                    : 0f;

            float visibility =
                signature != null
                    ? signature
                        .VisibilityMultiplier *
                      100f
                    : 100f;

            string posture =
                signature != null &&
                signature.IsCrouched
                    ? "ПРИГНУТ"
                    : "В ПОЛНЫЙ РОСТ";

            detail.text =
                posture +
                $"   •   заметность {visibility:0}%   •   шум {noise:0.0} м";
        }

        private void RefreshMarkers(
            PerceptionSensor[] sensors)
        {
            EnsureMarkerCount(
                Mathf.Min(
                    12,
                    sensors.Length));

            for (int i = 0;
                 i < markers.Count;
                 i++)
            {
                bool used =
                    i < sensors.Length &&
                    i < 12;

                markers[i].root
                    .SetActive(used);

                if (!used)
                    continue;

                PerceptionSensor sensor =
                    sensors[i];

                if (sensor == null ||
                    sensor.Owner == null ||
                    worldCamera == null)
                {
                    markers[i].root
                        .SetActive(false);
                    continue;
                }

                Vector3 world =
                    sensor.Owner.transform.position +
                    Vector3.up * 2.25f;

                Vector3 screen =
                    worldCamera
                        .WorldToScreenPoint(world);

                if (screen.z <= 0f)
                {
                    markers[i].root
                        .SetActive(false);
                    continue;
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

                if (x < -40f ||
                    x > ReferenceWidth + 40f ||
                    y < -40f ||
                    y > ReferenceHeight + 40f)
                {
                    markers[i].root
                        .SetActive(false);
                    continue;
                }

                Marker marker =
                    markers[i];

                marker.rect
                    .anchoredPosition =
                        new Vector2(x, y);

                float suspicion =
                    sensor
                        .GetSuspicionNormalizedFor(
                            leader);

                marker.fill.anchorMax =
                    new Vector2(
                        suspicion,
                        1f);

                Color color =
                    suspicion >= 0.82f
                        ? MainMenuTheme.Danger
                        : sensor.Awareness ==
                          AwarenessLevel.Suspicious
                            ? MainMenuTheme.Warm
                            : MainMenuTheme.Silver;

                marker.symbol.color = color;
                marker.fill
                    .GetComponent<Image>()
                    .color = color;
            }
        }

        private void EnsureMarkerCount(
            int count)
        {
            while (markers.Count < count)
            {
                Image rootImage =
                    MenuUiFactory.CreateImage(
                        "AwarenessMarker_" +
                        markers.Count,
                        markerRoot,
                        new Color(
                            0.01f,
                            0.014f,
                            0.018f,
                            0.78f));

                RectTransform rect =
                    rootImage.rectTransform;

                rect.anchorMin =
                    rect.anchorMax =
                        Vector2.zero;

                rect.pivot =
                    new Vector2(0.5f, 0.5f);

                rect.sizeDelta =
                    new Vector2(74f, 34f);

                Text symbol =
                    MenuUiFactory.CreateText(
                        "Symbol",
                        rootImage.transform,
                        "◇",
                        20,
                        MainMenuTheme.Silver,
                        TextAnchor.MiddleCenter);

                symbol.rectTransform.anchorMin =
                    new Vector2(0f, 0f);

                symbol.rectTransform.anchorMax =
                    new Vector2(0.42f, 1f);

                symbol.rectTransform.offsetMin =
                    Vector2.zero;

                symbol.rectTransform.offsetMax =
                    Vector2.zero;

                Image track =
                    MenuUiFactory.CreateImage(
                        "Track",
                        rootImage.transform,
                        new Color(
                            0.08f,
                            0.09f,
                            0.10f,
                            0.9f));

                RectTransform trackRect =
                    track.rectTransform;

                trackRect.anchorMin =
                    new Vector2(0.42f, 0.38f);

                trackRect.anchorMax =
                    new Vector2(0.92f, 0.62f);

                trackRect.offsetMin =
                    Vector2.zero;

                trackRect.offsetMax =
                    Vector2.zero;

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
                        root =
                            rootImage.gameObject,
                        rect = rect,
                        symbol = symbol,
                        fill = fillRect
                    });
            }
        }

        private void ResolveLeader()
        {
            CombatantRuntime[] party =
                FindObjectsByType<CombatantRuntime>(
                    FindObjectsSortMode.None);

            leader =
                party
                    .Where(value =>
                        value != null &&
                        value.IsAlive &&
                        value.Faction ==
                            CombatFaction.Player)
                    .OrderBy(value =>
                        value.Definition != null &&
                        value.Definition.characterId ==
                            "edward"
                            ? 0
                            : 1)
                    .FirstOrDefault();

            signature =
                leader != null
                    ? leader.GetComponent<
                        StealthSignature>()
                    : null;
        }

        private void HideAllMarkers()
        {
            foreach (Marker marker
                     in markers)
            {
                marker.root.SetActive(false);
            }
        }

        private void SetPanelVisible(
            bool visible)
        {
            if (group == null)
                return;

            group.alpha =
                visible ? 1f : 0f;

            group.interactable = false;
            group.blocksRaycasts = false;
        }
    }
}
