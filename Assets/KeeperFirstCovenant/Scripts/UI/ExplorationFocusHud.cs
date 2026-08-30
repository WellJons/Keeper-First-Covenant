using System.Collections.Generic;
using System.Linq;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Dialogue;
using KeeperFirstCovenant.Inventory;
using KeeperFirstCovenant.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public sealed class ExplorationFocusHud : MonoBehaviour
    {
        private sealed class FocusTarget
        {
            public Component component;
            public IInteractable interactable;
            public WorldInspectable inspectable;
            public Vector3 worldPoint;
            public string label;
        }

        private sealed class Marker
        {
            public GameObject root;
            public RectTransform rect;
            public Text symbol;
            public Text label;
        }

        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        [SerializeField, Min(3f)]
        private float focusRadius = 18f;

        [SerializeField, Range(4, 40)]
        private int maxMarkers = 20;

        private RectTransform markerRoot;
        private Text modeHint;

        private readonly List<FocusTarget> targets =
            new List<FocusTarget>();

        private readonly List<Marker> markers =
            new List<Marker>();

        private Camera worldCamera;
        private CombatantRuntime leader;
        private GameObject actor;
        private float nextScan;
        private bool active;

        private void Start()
        {
            Build();
            SetModeHint(false);
        }

        private void Update()
        {
            Keyboard keyboard =
                Keyboard.current;

            bool requested =
                keyboard != null &&
                (keyboard.leftAltKey.isPressed ||
                 keyboard.rightAltKey.isPressed);

            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            bool blocked =
                DialogueRunner.IsDialogueActive ||
                InspectionPanelController.IsOpen ||
                (director != null &&
                 (director.State == CombatState.Active ||
                  director.State == CombatState.Defeat));

            active =
                requested &&
                !blocked;

            SetModeHint(active);

            if (!active)
            {
                targets.Clear();
                HideAllMarkers();
                return;
            }

            if (worldCamera == null)
                worldCamera = Camera.main;

            if (leader == null ||
                !leader.IsAlive)
            {
                ResolveLeader();
            }

            if (leader == null ||
                worldCamera == null)
            {
                HideAllMarkers();
                return;
            }

            if (Time.unscaledTime >= nextScan)
            {
                nextScan =
                    Time.unscaledTime + 0.16f;

                Scan();
            }

            RefreshMarkers();
        }

        private void Build()
        {
            var canvasObject =
                new GameObject(
                    "ExplorationFocusCanvas",
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

            canvas.sortingOrder = 545;

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

            markerRoot =
                MenuUiFactory.CreateRect(
                    "FocusMarkers",
                    canvasRect);

            MenuUiFactory.Stretch(markerRoot);

            Image hintPanel =
                MenuUiFactory.CreateImage(
                    "FocusModeHint",
                    canvasRect,
                    new Color(
                        MainMenuTheme.Panel.r,
                        MainMenuTheme.Panel.g,
                        MainMenuTheme.Panel.b,
                        0.84f));

            RectTransform hintRect =
                hintPanel.rectTransform;

            MenuUiFactory.SetAnchoredRect(
                hintRect,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 28f),
                new Vector2(260f, 38f));

            KeeperUiSkin.DecorateSection(
                hintPanel);

            modeHint =
                MenuUiFactory.CreateText(
                    "Text",
                    hintPanel.transform,
                    "РЕЖИМ ИССЛЕДОВАНИЯ",
                    12,
                    MainMenuTheme.Silver,
                    TextAnchor.MiddleCenter);

            MenuUiFactory.Stretch(
                modeHint.rectTransform,
                8f,
                4f,
                8f,
                4f);
        }

        private void Scan()
        {
            ResolveLeader();

            if (leader == null ||
                actor == null)
            {
                targets.Clear();
                return;
            }

            MonoBehaviour[] behaviours =
                FindObjectsByType<MonoBehaviour>(
                    FindObjectsSortMode.None);

            var unique =
                new Dictionary<int, FocusTarget>();

            foreach (MonoBehaviour behaviour
                     in behaviours)
            {
                if (behaviour == null ||
                    !behaviour.isActiveAndEnabled)
                {
                    continue;
                }

                IInteractable interactable =
                    behaviour as IInteractable;

                WorldInspectable inspectable =
                    behaviour as WorldInspectable;

                if (interactable == null &&
                    inspectable == null)
                {
                    continue;
                }

                HiddenDiscoverable hidden =
                    behaviour.GetComponentInParent<
                        HiddenDiscoverable>();

                if (hidden != null &&
                    !hidden.IsDiscovered)
                {
                    continue;
                }

                bool canInteract =
                    interactable != null &&
                    interactable.CanInteract(
                        actor);

                bool canInspect =
                    inspectable != null &&
                    inspectable.CanInspect(
                        actor);

                if (!canInteract &&
                    !canInspect)
                {
                    continue;
                }

                Vector3 point =
                    ResolveWorldPoint(
                        behaviour);

                float distance =
                    Vector3.Distance(
                        leader.transform.position,
                        point);

                if (distance > focusRadius)
                    continue;

                int key =
                    behaviour.gameObject
                        .GetInstanceID();

                string label =
                    interactable != null &&
                    canInteract
                        ? interactable
                            .InteractionPrompt
                        : inspectable != null
                            ? inspectable.DisplayName
                            : behaviour.name;

                if (!unique.TryGetValue(
                        key,
                        out FocusTarget existing))
                {
                    unique[key] =
                        new FocusTarget
                        {
                            component = behaviour,
                            interactable =
                                interactable,
                            inspectable =
                                inspectable,
                            worldPoint = point,
                            label = label
                        };
                }
                else
                {
                    if (existing.interactable == null &&
                        interactable != null)
                    {
                        existing.interactable =
                            interactable;

                        existing.label =
                            interactable
                                .InteractionPrompt;
                    }

                    if (existing.inspectable == null &&
                        inspectable != null)
                    {
                        existing.inspectable =
                            inspectable;
                    }
                }
            }

            targets.Clear();

            targets.AddRange(
                unique.Values
                    .OrderBy(value =>
                        Vector3.Distance(
                            leader.transform.position,
                            value.worldPoint))
                    .Take(maxMarkers));
        }

        private void RefreshMarkers()
        {
            EnsureMarkerCount(
                targets.Count);

            for (int i = 0;
                 i < markers.Count;
                 i++)
            {
                bool used =
                    i < targets.Count;

                markers[i].root
                    .SetActive(used);

                if (!used)
                    continue;

                FocusTarget target =
                    targets[i];

                if (target.component == null)
                {
                    markers[i].root
                        .SetActive(false);
                    continue;
                }

                target.worldPoint =
                    ResolveWorldPoint(
                        target.component);

                Vector3 screen =
                    worldCamera.WorldToScreenPoint(
                        target.worldPoint);

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

                if (x < -80f ||
                    x > ReferenceWidth + 80f ||
                    y < -80f ||
                    y > ReferenceHeight + 80f)
                {
                    markers[i].root
                        .SetActive(false);
                    continue;
                }

                Marker marker =
                    markers[i];

                marker.rect.anchoredPosition =
                    new Vector2(x, y);

                marker.label.text =
                    target.label;

                bool inspectOnly =
                    target.interactable == null &&
                    target.inspectable != null;

                marker.symbol.text =
                    inspectOnly
                        ? "◇"
                        : "◆";

                marker.symbol.color =
                    inspectOnly
                        ? MainMenuTheme.Silver
                        : MainMenuTheme.Warm;
            }
        }

        private void EnsureMarkerCount(
            int count)
        {
            while (markers.Count < count)
            {
                Image panel =
                    MenuUiFactory.CreateImage(
                        "FocusMarker_" +
                        markers.Count,
                        markerRoot,
                        new Color(
                            0.008f,
                            0.012f,
                            0.016f,
                            0.80f));

                RectTransform rect =
                    panel.rectTransform;

                rect.anchorMin =
                    rect.anchorMax =
                        Vector2.zero;

                rect.pivot =
                    new Vector2(0.5f, 0.5f);

                rect.sizeDelta =
                    new Vector2(190f, 42f);

                KeeperUiSkin.DecorateSection(
                    panel);

                Text symbol =
                    MenuUiFactory.CreateText(
                        "Symbol",
                        panel.transform,
                        "◆",
                        17,
                        MainMenuTheme.Warm,
                        TextAnchor.MiddleCenter);

                symbol.rectTransform.anchorMin =
                    Vector2.zero;

                symbol.rectTransform.anchorMax =
                    new Vector2(0.22f, 1f);

                symbol.rectTransform.offsetMin =
                    Vector2.zero;

                symbol.rectTransform.offsetMax =
                    Vector2.zero;

                Text label =
                    MenuUiFactory.CreateText(
                        "Label",
                        panel.transform,
                        string.Empty,
                        12,
                        MainMenuTheme.Text,
                        TextAnchor.MiddleLeft);

                label.rectTransform.anchorMin =
                    new Vector2(0.22f, 0f);

                label.rectTransform.anchorMax =
                    Vector2.one;

                label.rectTransform.offsetMin =
                    new Vector2(2f, 0f);

                label.rectTransform.offsetMax =
                    new Vector2(-8f, 0f);

                markers.Add(
                    new Marker
                    {
                        root =
                            panel.gameObject,
                        rect = rect,
                        symbol = symbol,
                        label = label
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

            actor =
                leader != null &&
                leader.GetComponent<
                    InventoryComponent>() != null
                    ? leader.gameObject
                    : null;
        }

        private static Vector3 ResolveWorldPoint(
            Component component)
        {
            if (component == null)
                return Vector3.zero;

            Collider collider =
                component.GetComponentInChildren<
                    Collider>();

            if (collider != null &&
                collider.enabled)
            {
                return collider.bounds.center +
                       Vector3.up *
                       Mathf.Min(
                           0.6f,
                           collider.bounds.extents.y);
            }

            Renderer renderer =
                component.GetComponentInChildren<
                    Renderer>();

            if (renderer != null &&
                renderer.enabled)
            {
                return renderer.bounds.center +
                       Vector3.up *
                       Mathf.Min(
                           0.6f,
                           renderer.bounds.extents.y);
            }

            return component.transform.position +
                   Vector3.up * 0.8f;
        }

        private void HideAllMarkers()
        {
            foreach (Marker marker
                     in markers)
            {
                marker.root.SetActive(false);
            }
        }

        private void SetModeHint(
            bool visible)
        {
            if (modeHint == null)
                return;

            modeHint.transform.parent
                .gameObject
                .SetActive(visible);
        }
    }
}
