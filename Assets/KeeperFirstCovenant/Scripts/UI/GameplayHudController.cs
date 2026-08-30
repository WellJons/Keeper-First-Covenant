using System;
using System.Collections.Generic;
using System.Linq;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Core;
using KeeperFirstCovenant.Player;
using KeeperFirstCovenant.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public sealed class GameplayHudController : MonoBehaviour
    {
        private sealed class PartyCard
        {
            public CombatantRuntime runtime;
            public GameObject root;
            public Text name;
            public Text healthText;
            public Text manaText;
            public RectTransform healthFill;
            public RectTransform manaFill;
            public Button selectButton;
            public Image panel;
        }

        private sealed class ActionEntry
        {
            public CombatActionDefinition action;
            public Button button;
            public Text label;
        }

        private RectTransform partyRoot;
        private GameObject combatPanel;
        private RectTransform actionRoot;
        private Text turnText;
        private Text resourceText;
        private Text previewText;

        private TacticalPlayerController playerController;
        private TurnCombatDirector director;

        private readonly List<PartyCard> partyCards =
            new List<PartyCard>();

        private readonly List<ActionEntry> actionEntries =
            new List<ActionEntry>();

        private CombatantRuntime lastActionActor;
        private string lastActionSignature = string.Empty;
        private float nextRefresh;

        private void Start()
        {
            DisableLegacyDebugHud();
            EnsureEventSystem();
            Build();
            ResolveRuntimeReferences();
            RefreshAll();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh)
                return;

            nextRefresh =
                Time.unscaledTime + 0.08f;

            ResolveRuntimeReferences();
            RefreshAll();
        }

        private void ResolveRuntimeReferences()
        {
            if (director == null)
                director = TurnCombatDirector.Instance;

            if (playerController == null)
            {
                playerController =
                    FindFirstObjectByType<
                        TacticalPlayerController>();
            }
        }

        private void Build()
        {
            var canvasObject = new GameObject(
                "GameplayHudCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            canvasObject.transform.SetParent(
                transform,
                false);

            Canvas canvas =
                canvasObject.GetComponent<Canvas>();

            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;

            canvas.sortingOrder = 180;

            CanvasScaler scaler =
                canvasObject.GetComponent<CanvasScaler>();

            scaler.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;

            scaler.referenceResolution =
                new Vector2(1920f, 1080f);

            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect =
                canvasObject.GetComponent<RectTransform>();

            BuildPartyArea(canvasRect);
            BuildTurnHeader(canvasRect);
            BuildCombatPanel(canvasRect);
        }

        private void BuildPartyArea(
            RectTransform canvasRect)
        {
            partyRoot =
                MenuUiFactory.CreateRect(
                    "Party",
                    canvasRect);

            MenuUiFactory.SetAnchoredRect(
                partyRoot,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(28f, -30f),
                new Vector2(330f, 430f));

            VerticalLayoutGroup layout =
                partyRoot.gameObject
                    .AddComponent<
                        VerticalLayoutGroup>();

            layout.spacing = 7f;
            layout.childAlignment =
                TextAnchor.UpperLeft;

            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
        }

        private void BuildTurnHeader(
            RectTransform canvasRect)
        {
            Image background =
                MenuUiFactory.CreateImage(
                    "TurnHeader",
                    canvasRect,
                    new Color(
                        MainMenuTheme.Panel.r,
                        MainMenuTheme.Panel.g,
                        MainMenuTheme.Panel.b,
                        0.78f));

            KeeperUiSkin.DecorateSection(
                background);

            RectTransform rect =
                background.rectTransform;

            MenuUiFactory.SetAnchoredRect(
                rect,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -26f),
                new Vector2(520f, 66f));

            turnText =
                MenuUiFactory.CreateText(
                    "Text",
                    background.transform,
                    string.Empty,
                    18,
                    MainMenuTheme.Text,
                    TextAnchor.MiddleCenter);

            MenuUiFactory.Stretch(
                turnText.rectTransform,
                16f,
                8f,
                16f,
                8f);
        }

        private void BuildCombatPanel(
            RectTransform canvasRect)
        {
            Image panel =
                MenuUiFactory.CreateImage(
                    "CombatPanel",
                    canvasRect,
                    new Color(
                        MainMenuTheme.Panel.r,
                        MainMenuTheme.Panel.g,
                        MainMenuTheme.Panel.b,
                        0.90f));

            combatPanel = panel.gameObject;

            KeeperUiSkin.DecorateMajorPanel(
                panel,
                true);

            RectTransform panelRect =
                panel.rectTransform;

            MenuUiFactory.SetAnchoredRect(
                panelRect,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 24f),
                new Vector2(1220f, 170f));

            Outline outline =
                panel.gameObject
                    .AddComponent<Outline>();

            outline.effectColor =
                new Color(
                    MainMenuTheme.SilverDim.r,
                    MainMenuTheme.SilverDim.g,
                    MainMenuTheme.SilverDim.b,
                    0.40f);

            outline.effectDistance =
                new Vector2(1f, -1f);

            resourceText =
                MenuUiFactory.CreateText(
                    "Resources",
                    panel.transform,
                    string.Empty,
                    16,
                    MainMenuTheme.Silver,
                    TextAnchor.MiddleLeft);

            resourceText.rectTransform.anchorMin =
                new Vector2(0f, 0.69f);

            resourceText.rectTransform.anchorMax =
                new Vector2(1f, 1f);

            resourceText.rectTransform.offsetMin =
                new Vector2(20f, 0f);

            resourceText.rectTransform.offsetMax =
                new Vector2(-20f, -4f);

            previewText =
                MenuUiFactory.CreateText(
                    "Preview",
                    panel.transform,
                    string.Empty,
                    14,
                    MainMenuTheme.MutedText,
                    TextAnchor.MiddleRight);

            previewText.rectTransform.anchorMin =
                new Vector2(0.45f, 0.69f);

            previewText.rectTransform.anchorMax =
                new Vector2(1f, 1f);

            previewText.rectTransform.offsetMin =
                Vector2.zero;

            previewText.rectTransform.offsetMax =
                new Vector2(-20f, -4f);

            actionRoot =
                MenuUiFactory.CreateRect(
                    "Actions",
                    panel.transform);

            actionRoot.anchorMin =
                new Vector2(0f, 0f);

            actionRoot.anchorMax =
                new Vector2(1f, 0.70f);

            actionRoot.offsetMin =
                new Vector2(16f, 12f);

            actionRoot.offsetMax =
                new Vector2(-16f, -4f);

            HorizontalLayoutGroup layout =
                actionRoot.gameObject
                    .AddComponent<
                        HorizontalLayoutGroup>();

            layout.spacing = 8f;
            layout.childAlignment =
                TextAnchor.MiddleCenter;

            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;
        }

        private void RefreshAll()
        {
            RefreshParty();

            if (director == null)
            {
                turnText.text =
                    "ИССЛЕДОВАНИЕ";

                combatPanel.SetActive(false);
                return;
            }

            RefreshTurnHeader();

            bool combatActive =
                director.State ==
                CombatState.Active;

            combatPanel.SetActive(
                combatActive);

            if (!combatActive)
                return;

            CombatantRuntime actor =
                director.CurrentActor;

            RefreshActions(actor);
            RefreshResources(actor);
            RefreshPreview();
        }

        private void RefreshParty()
        {
            CombatantRuntime[] party =
                FindObjectsByType<
                        CombatantRuntime>(
                        FindObjectsSortMode.None)
                    .Where(value =>
                        value != null &&
                        value.Definition != null &&
                        (value.Faction ==
                             CombatFaction.Player ||
                         value.Faction ==
                             CombatFaction.Ally))
                    .OrderBy(value =>
                        value.Faction ==
                            CombatFaction.Player
                            ? 0
                            : 1)
                    .ThenBy(value =>
                        value.Definition.characterId)
                    .ToArray();

            bool rebuild =
                partyCards.Count !=
                party.Length;

            if (!rebuild)
            {
                for (int i = 0;
                     i < party.Length;
                     i++)
                {
                    if (partyCards[i].runtime !=
                        party[i])
                    {
                        rebuild = true;
                        break;
                    }
                }
            }

            if (rebuild)
                RebuildPartyCards(party);

            foreach (PartyCard card
                     in partyCards)
            {
                RefreshPartyCard(card);
            }
        }

        private void RebuildPartyCards(
            CombatantRuntime[] party)
        {
            for (int i =
                     partyRoot.childCount - 1;
                 i >= 0;
                 i--)
            {
                Destroy(
                    partyRoot.GetChild(i)
                        .gameObject);
            }

            partyCards.Clear();

            foreach (CombatantRuntime member
                     in party)
            {
                partyCards.Add(
                    BuildPartyCard(member));
            }
        }

        private PartyCard BuildPartyCard(
            CombatantRuntime member)
        {
            Image panel =
                MenuUiFactory.CreateImage(
                    "Party_" +
                    member.Definition.characterId,
                    partyRoot,
                    new Color(
                        MainMenuTheme.Panel.r,
                        MainMenuTheme.Panel.g,
                        MainMenuTheme.Panel.b,
                        0.78f));

            KeeperUiSkin.DecorateSection(
                panel);

            Button selectButton =
                panel.gameObject
                    .AddComponent<Button>();

            selectButton.targetGraphic = panel;

            ColorBlock colors =
                selectButton.colors;

            colors.normalColor = Color.white;
            colors.highlightedColor =
                new Color(
                    1.05f,
                    1.05f,
                    1.05f,
                    1f);

            colors.pressedColor =
                new Color(
                    0.86f,
                    0.88f,
                    0.92f,
                    1f);

            colors.selectedColor =
                Color.white;

            selectButton.colors = colors;

            selectButton.onClick.AddListener(
                () =>
                {
                    PartySelectionService selection =
                        PartySelectionService.Instance;

                    if (selection != null)
                    {
                        selection.Select(
                            member);
                    }
                });

            LayoutElement element =
                panel.gameObject
                    .AddComponent<
                        LayoutElement>();

            element.preferredHeight = 76f;
            element.minHeight = 76f;

            Text name =
                MenuUiFactory.CreateText(
                    "Name",
                    panel.transform,
                    member.Definition.displayName,
                    16,
                    MainMenuTheme.Text,
                    TextAnchor.UpperLeft);

            name.rectTransform.anchorMin =
                new Vector2(0f, 0.54f);

            name.rectTransform.anchorMax =
                new Vector2(1f, 1f);

            name.rectTransform.offsetMin =
                new Vector2(12f, 0f);

            name.rectTransform.offsetMax =
                new Vector2(-12f, -7f);

            RectTransform hpFill =
                BuildResourceBar(
                    panel.transform,
                    "HP",
                    0.30f,
                    MainMenuTheme.Warm);

            RectTransform mpFill =
                BuildResourceBar(
                    panel.transform,
                    "MP",
                    0.10f,
                    MainMenuTheme.Silver);

            Text health =
                MenuUiFactory.CreateText(
                    "HealthText",
                    panel.transform,
                    string.Empty,
                    12,
                    MainMenuTheme.Text,
                    TextAnchor.MiddleLeft);

            health.rectTransform.anchorMin =
                new Vector2(0f, 0.22f);

            health.rectTransform.anchorMax =
                new Vector2(1f, 0.46f);

            health.rectTransform.offsetMin =
                new Vector2(12f, 0f);

            health.rectTransform.offsetMax =
                new Vector2(-12f, 0f);

            Text mana =
                MenuUiFactory.CreateText(
                    "ManaText",
                    panel.transform,
                    string.Empty,
                    11,
                    MainMenuTheme.MutedText,
                    TextAnchor.MiddleLeft);

            mana.rectTransform.anchorMin =
                new Vector2(0f, 0.02f);

            mana.rectTransform.anchorMax =
                new Vector2(1f, 0.22f);

            mana.rectTransform.offsetMin =
                new Vector2(12f, 0f);

            mana.rectTransform.offsetMax =
                new Vector2(-12f, 0f);

            return new PartyCard
            {
                runtime = member,
                root = panel.gameObject,
                name = name,
                healthText = health,
                manaText = mana,
                healthFill = hpFill,
                manaFill = mpFill,
                selectButton = selectButton,
                panel = panel
            };
        }

        private static RectTransform BuildResourceBar(
            Transform parent,
            string name,
            float verticalAnchor,
            Color color)
        {
            Image background =
                MenuUiFactory.CreateImage(
                    name + "Track",
                    parent,
                    new Color(
                        0.08f,
                        0.09f,
                        0.10f,
                        0.85f));

            RectTransform track =
                background.rectTransform;

            track.anchorMin =
                new Vector2(0f, verticalAnchor);

            track.anchorMax =
                new Vector2(1f, verticalAnchor);

            track.pivot =
                new Vector2(0f, 0.5f);

            track.offsetMin =
                new Vector2(12f, -2f);

            track.offsetMax =
                new Vector2(-12f, 2f);

            Image fill =
                MenuUiFactory.CreateImage(
                    name + "Fill",
                    track,
                    color);

            RectTransform fillRect =
                fill.rectTransform;

            fillRect.anchorMin =
                Vector2.zero;

            fillRect.anchorMax =
                Vector2.one;

            fillRect.offsetMin =
                Vector2.zero;

            fillRect.offsetMax =
                Vector2.zero;

            return fillRect;
        }

        private void RefreshPartyCard(
            PartyCard card)
        {
            if (card?.runtime == null ||
                card.runtime.Definition == null)
            {
                return;
            }

            CombatantRuntime runtime =
                card.runtime;

            int maxHealth =
                Mathf.Max(
                    1,
                    runtime.Definition.maxHealth);

            int maxMana =
                Mathf.Max(
                    0,
                    runtime.Definition.maxMana);

            float hp =
                Mathf.Clamp01(
                    runtime.CurrentHealth /
                    (float)maxHealth);

            float mp =
                maxMana > 0
                    ? Mathf.Clamp01(
                        runtime.CurrentMana /
                        (float)maxMana)
                    : 0f;

            card.healthFill.anchorMax =
                new Vector2(hp, 1f);

            card.manaFill.anchorMax =
                new Vector2(mp, 1f);

            string state =
                runtime.IsDead
                    ? "МЁРТВ"
                    : runtime.IsDowned
                        ? "РАНЕН"
                        : $"HP {runtime.CurrentHealth}/{maxHealth}";

            card.healthText.text = state;

            card.manaText.text =
                maxMana > 0
                    ? $"MP {runtime.CurrentMana}/{maxMana}"
                    : string.Empty;

            bool combatActive =
                director != null &&
                director.State ==
                    CombatState.Active;

            bool current =
                combatActive &&
                director.CurrentActor ==
                    runtime;

            bool selected =
                !combatActive &&
                PartySelectionService.Instance !=
                    null &&
                PartySelectionService.Instance
                    .SelectedMember ==
                    runtime;

            card.name.text =
                selected
                    ? "▶ " +
                      runtime.Definition.displayName
                    : runtime.Definition.displayName;

            card.name.color =
                current || selected
                    ? MainMenuTheme.Warm
                    : MainMenuTheme.Text;

            if (card.panel != null)
            {
                card.panel.color =
                    selected
                        ? new Color(
                            MainMenuTheme.Panel.r +
                                0.055f,
                            MainMenuTheme.Panel.g +
                                0.045f,
                            MainMenuTheme.Panel.b +
                                0.025f,
                            0.92f)
                        : new Color(
                            MainMenuTheme.Panel.r,
                            MainMenuTheme.Panel.g,
                            MainMenuTheme.Panel.b,
                            0.78f);
            }

            if (card.selectButton != null)
            {
                card.selectButton.interactable =
                    runtime.IsAlive;
            }
        }

        private void RefreshTurnHeader()
        {
            switch (director.State)
            {
                case CombatState.Active:
                    string actorName =
                        director.CurrentActor != null &&
                        director.CurrentActor.Definition != null
                            ? director.CurrentActor.Definition.displayName
                            : "—";

                    turnText.text =
                        $"РАУНД {director.Round}   •   ХОД: {actorName}";
                    break;

                case CombatState.Victory:
                    turnText.text = "ПОБЕДА";
                    break;

                case CombatState.Defeat:
                    turnText.text = "ПОРАЖЕНИЕ";
                    break;

                default:
                    turnText.text = "ИССЛЕДОВАНИЕ";
                    break;
            }
        }

        private void RefreshResources(
            CombatantRuntime actor)
        {
            if (actor == null ||
                actor.Definition == null)
            {
                resourceText.text =
                    string.Empty;
                return;
            }

            BreakGaugeComponent breakGauge =
                actor.GetComponent<
                    BreakGaugeComponent>();

            string stability =
                breakGauge != null
                    ? $"   Стойкость {breakGauge.Stability}/{breakGauge.MaxStability}"
                    : string.Empty;

            resourceText.text =
                $"{actor.Definition.displayName}   " +
                $"HP {actor.CurrentHealth}/{actor.Definition.maxHealth}   " +
                $"MP {actor.CurrentMana}/{actor.Definition.maxMana}   " +
                $"AP {actor.CurrentActionPoints}   " +
                $"Движение {actor.TotalMovementAvailable:0.0} м" +
                stability;
        }

        private void RefreshActions(
            CombatantRuntime actor)
        {
            CombatActionDefinition[] actions =
                actor != null
                    ? actor.GetAvailableActions()
                    : Array.Empty<
                        CombatActionDefinition>();

            string signature =
                actor != null
                    ? string.Join(
                        "|",
                        actions
                            .Where(value =>
                                value != null)
                            .Take(8)
                            .Select(value =>
                                value.actionId))
                    : string.Empty;

            if (actor != lastActionActor ||
                signature !=
                lastActionSignature)
            {
                lastActionActor = actor;
                lastActionSignature =
                    signature;

                RebuildActionButtons(
                    actions);
            }

            bool partyControlled =
                actor != null &&
                (actor.Faction ==
                     CombatFaction.Player ||
                 actor.Faction ==
                     CombatFaction.Ally);

            for (int i = 0;
                 i < actionEntries.Count;
                 i++)
            {
                ActionEntry entry =
                    actionEntries[i];

                CombatActionDefinition action =
                    entry.action;

                if (action == null)
                    continue;

                bool selected =
                    playerController != null &&
                    playerController.SelectedAction ==
                    action;

                CombatActionStateComponent state =
                    actor != null
                        ? CombatActionStateComponent
                            .Ensure(actor)
                        : null;

                int cooldown =
                    state != null
                        ? state.GetCooldownRemaining(
                            action)
                        : 0;

                bool comboReady =
                    state != null &&
                    state.MatchesCombo(action);

                bool stateUsable =
                    state == null ||
                    state.CanUse(
                        action,
                        out _);

                string combatState =
                    cooldown > 0
                        ? $"   CD {cooldown}"
                        : comboReady
                            ? $"   КОМБО ×{Mathf.Max(2, state.ComboDepth + 1)}"
                            : string.Empty;

                entry.label.text =
                    $"{i + 1}. " +
                    (selected ? "▶ " : string.Empty) +
                    action.displayName +
                    combatState +
                    $"\nAP {action.actionPointCost}" +
                    (action.manaCost > 0
                        ? $"  MP {action.manaCost}"
                        : string.Empty);

                entry.label.color =
                    comboReady
                        ? MainMenuTheme.Warm
                        : stateUsable
                            ? MainMenuTheme.Text
                            : MainMenuTheme.MutedText;

                entry.button.interactable =
                    partyControlled &&
                    stateUsable &&
                    actor.CurrentActionPoints >=
                        action.actionPointCost &&
                    actor.CurrentMana >=
                        action.manaCost;
            }
        }

        private void RebuildActionButtons(
            CombatActionDefinition[] actions)
        {
            for (int i =
                     actionRoot.childCount - 1;
                 i >= 0;
                 i--)
            {
                Destroy(
                    actionRoot.GetChild(i)
                        .gameObject);
            }

            actionEntries.Clear();

            if (actions == null)
                return;

            for (int i = 0;
                 i < actions.Length &&
                 i < 8;
                 i++)
            {
                CombatActionDefinition action =
                    actions[i];

                if (action == null)
                    continue;

                int index = i;

                Button button =
                    MenuUiFactory.CreateMenuButton(
                        "Action_" +
                        action.actionId,
                        actionRoot,
                        string.Empty,
                        14);

                Text label =
                    button.transform
                        .Find("Label")
                        .GetComponent<Text>();

                button.onClick
                    .AddListener(() =>
                    {
                        if (playerController == null)
                            return;

                        GameAudioService.Instance
                            .PlayClick();

                        playerController
                            .SelectAction(
                                action);
                    });

                actionEntries.Add(
                    new ActionEntry
                    {
                        action = action,
                        button = button,
                        label = label
                    });
            }
        }

        private void RefreshPreview()
        {
            if (playerController == null ||
                playerController.SelectedAction ==
                    null)
            {
                previewText.text =
                    string.Empty;
                return;
            }

            if (!playerController
                    .HasHoverPreview)
            {
                previewText.text =
                    "Выберите цель";
                return;
            }

            TacticalTargetPreview preview =
                playerController
                    .CurrentPreview;

            if (!preview.Valid)
            {
                previewText.text =
                    "Недоступно: " +
                    FormatActionFailure(
                        preview.Failure);
                return;
            }

            string damage =
                preview.DamageMin ==
                    preview.DamageMax
                    ? preview.DamageMin.ToString()
                    : preview.DamageMin +
                      "-" +
                      preview.DamageMax;

            string tactical =
                preview.Cover !=
                    CoverQuality.None
                    ? "   •   укрытие " +
                      preview.Cover
                    : string.Empty;

            string flank;

            switch (preview.Flank)
            {
                case FlankQuality.Back:
                    flank =
                        "   •   СПИНА +" +
                        preview.FlankImpactModifier +
                        "%";
                    break;

                case FlankQuality.Side:
                    flank =
                        "   •   ФЛАНГ +" +
                        preview.FlankImpactModifier +
                        "%";
                    break;

                default:
                    flank = string.Empty;
                    break;
            }

            previewText.text =
                $"Урон {damage}   •   " +
                $"{preview.Distance:0.0} м" +
                tactical +
                flank;
        }

        private static string FormatActionFailure(
            ActionFailureReason reason)
        {
            switch (reason)
            {
                case ActionFailureReason.OutOfRange:
                    return "слишком далеко";
                case ActionFailureReason.NoLineOfSight:
                    return "нет линии атаки";
                case ActionFailureReason.NotEnoughActionPoints:
                    return "не хватает очков действия";
                case ActionFailureReason.NotEnoughMana:
                    return "не хватает маны";
                case ActionFailureReason.ActionOnCooldown:
                    return "приём восстанавливается";
                case ActionFailureReason.ComboRequirementMissing:
                    return "нужна предыдущая связка";
                case ActionFailureReason.NotEnoughStrainCapacity:
                    return "слишком высокая нагрузка";
                default:
                    return reason.ToString();
            }
        }

        private static void
            DisableLegacyDebugHud()
        {
            CombatDebugHUD[] legacy =
                FindObjectsByType<
                    CombatDebugHUD>(
                    FindObjectsSortMode.None);

            foreach (CombatDebugHUD hud
                     in legacy)
            {
                if (hud != null)
                    hud.enabled = false;
            }
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;

            var eventSystem =
                new GameObject(
                    "EventSystem",
                    typeof(EventSystem),
                    typeof(
                        InputSystemUIInputModule));

            eventSystem.transform
                .SetParent(
                    transform,
                    false);
        }
    }
}
