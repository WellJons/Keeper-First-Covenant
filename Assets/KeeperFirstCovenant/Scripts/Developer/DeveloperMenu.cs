using System;
using System.Collections.Generic;
using System.Linq;
using KeeperFirstCovenant.Characters;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;
using UObject = UnityEngine.Object;

namespace KeeperFirstCovenant.Developer
{
    public sealed class DeveloperMenu : MonoBehaviour
    {
        private enum Tab
        {
            Characters,
            Enemies,
            Items,
            Weapons,
            Abilities,
            Log,
            Cheats
        }

        public static bool IsOpen { get; private set; }

        [SerializeField] private DeveloperContentCatalog catalog;
        [SerializeField] private bool pauseGameWhileOpen = true;

        private readonly List<GameObject> _spawned =
            new List<GameObject>();

        private readonly List<GameObject> _disabledAllies =
            new List<GameObject>();

        private Tab _tab;
        private Vector2 _listScroll;
        private Vector2 _leftScroll;
        private Vector2 _rightScroll;
        private Vector2 _logScroll;
        private string _search = string.Empty;

        private UObject _left;
        private UObject _right;

        private float _previousTimeScale = 1f;

        private GUIStyle _windowStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _sectionStyle;
        private GUIStyle _smallStyle;

        private void Start()
        {
            if (catalog == null)
            {
                catalog =
                    GetComponent<DeveloperContentCatalog>();
            }

            if (catalog == null)
            {
                catalog =
                    FindFirstObjectByType<
                        DeveloperContentCatalog>();
            }
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;

            if (keyboard != null &&
                keyboard.f1Key.wasPressedThisFrame)
            {
                Toggle();
            }
        }

        private void OnDestroy()
        {
            if (IsOpen)
                Close();
        }

        public void Toggle()
        {
            if (IsOpen)
                Close();
            else
                Open();
        }

        private void Open()
        {
            IsOpen = true;

            if (pauseGameWhileOpen)
            {
                _previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
        }

        private void Close()
        {
            IsOpen = false;

            if (pauseGameWhileOpen)
                Time.timeScale = _previousTimeScale;
        }

        private void OnGUI()
        {
            if (!IsOpen)
                return;

            EnsureStyles();

            float width =
                Mathf.Min(Screen.width - 40f, 1160f);

            float height =
                Mathf.Min(Screen.height - 40f, 760f);

            Rect rect = new Rect(
                (Screen.width - width) * 0.5f,
                (Screen.height - height) * 0.5f,
                width,
                height);

            GUILayout.BeginArea(rect, _windowStyle);

            DrawTopBar();

            GUILayout.Space(6f);

            _tab = (Tab)GUILayout.Toolbar(
                (int)_tab,
                new[]
                {
                    "Characters",
                    "Enemies",
                    "Items",
                    "Weapons",
                    "Abilities",
                    "Log",
                    "Cheats"
                });

            GUILayout.Space(8f);

            if (_tab == Tab.Cheats)
            {
                DrawCheats();
            }
            else if (_tab == Tab.Log)
            {
                DrawLog();
            }
            else
            {
                DrawBrowser();
            }

            GUILayout.EndArea();
        }

        private void DrawTopBar()
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(
                "KEEPER — DEVELOPER SANDBOX",
                _headerStyle);

            GUILayout.FlexibleSpace();

            GUILayout.Label(
                "F1 — close",
                _smallStyle);

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Label(
                "Search:",
                GUILayout.Width(55f));

            _search =
                GUILayout.TextField(
                    _search ?? string.Empty);

            if (GUILayout.Button(
                    "Clear",
                    GUILayout.Width(70f)))
            {
                _search = string.Empty;
            }

            GUILayout.EndHorizontal();
        }

        private void DrawBrowser()
        {
            UObject[] entries =
                GetCurrentEntries();

            GUILayout.BeginHorizontal();

            DrawEntryList(entries);

            GUILayout.Space(8f);

            DrawComparisonColumn(
                "A",
                _left,
                ref _leftScroll,
                true);

            GUILayout.Space(8f);

            DrawComparisonColumn(
                "B",
                _right,
                ref _rightScroll,
                false);

            GUILayout.EndHorizontal();
        }

        private void DrawEntryList(UObject[] entries)
        {
            GUILayout.BeginVertical(
                GUILayout.Width(285f));

            GUILayout.Label(
                $"Library ({entries.Length})",
                _sectionStyle);

            _listScroll =
                GUILayout.BeginScrollView(
                    _listScroll,
                    GUI.skin.box);

            foreach (UObject entry in entries)
            {
                if (entry == null)
                    continue;

                GUILayout.BeginHorizontal();

                GUILayout.Label(
                    GetDisplayName(entry),
                    GUILayout.MinWidth(120f));

                if (GUILayout.Button(
                        "A",
                        GUILayout.Width(30f)))
                {
                    _left = entry;
                }

                if (GUILayout.Button(
                        "B",
                        GUILayout.Width(30f)))
                {
                    _right = entry;
                }

                GUILayout.EndHorizontal();

                DrawEntryQuickActions(entry);
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        private void DrawEntryQuickActions(
            UObject entry)
        {
            if (entry is CharacterDefinition character)
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button(
                        "Spawn",
                        GUILayout.Height(22f)))
                {
                    SpawnCharacter(character);
                }

                GUILayout.EndHorizontal();
            }
            else if (entry is ItemDefinition item)
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button(
                        "Give x1",
                        GUILayout.Height(22f)))
                {
                    GiveItem(item, 1);
                }

                if (item.stackable &&
                    GUILayout.Button(
                        "Give x10",
                        GUILayout.Height(22f)))
                {
                    GiveItem(item, 10);
                }

                if ((item is WeaponDefinition ||
                     item is ArmorDefinition) &&
                    GUILayout.Button(
                        "Equip",
                        GUILayout.Height(22f)))
                {
                    EquipItem(item);
                }

                GUILayout.EndHorizontal();
            }

            else if (entry is CombatActionDefinition action)
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button(
                        "Grant ability",
                        GUILayout.Height(22f)))
                {
                    GrantAbility(action);
                }

                GUILayout.EndHorizontal();
            }
        }

        private void DrawComparisonColumn(
            string title,
            UObject selection,
            ref Vector2 scroll,
            bool isLeft)
        {
            GUILayout.BeginVertical(
                GUILayout.ExpandWidth(true));

            GUILayout.Label(
                $"COMPARE {title}",
                _sectionStyle);

            if (selection == null)
            {
                GUILayout.Label(
                    "Select an entry from the library.");

                GUILayout.EndVertical();
                return;
            }

            scroll =
                GUILayout.BeginScrollView(
                    scroll,
                    GUI.skin.box);

            DrawDetails(selection);

            GUILayout.EndScrollView();

            if (GUILayout.Button(
                    $"Clear {title}"))
            {
                if (isLeft)
                    _left = null;
                else
                    _right = null;
            }

            GUILayout.EndVertical();
        }

        private void DrawDetails(UObject entry)
        {
            GUILayout.Label(
                GetDisplayName(entry),
                _headerStyle);

            GUILayout.Space(5f);

            if (entry is CharacterDefinition character)
            {
                DrawCharacter(character);
            }
            else if (entry is WeaponDefinition weapon)
            {
                DrawWeapon(weapon);
            }
            else if (entry is ArmorDefinition armor)
            {
                DrawArmor(armor);
            }
            else if (entry is ItemDefinition item)
            {
                DrawItem(item);
            }
            else if (entry is CombatActionDefinition action)
            {
                DrawAction(action);
            }
            else
            {
                GUILayout.Label(entry.name);
            }
        }

        private static void DrawCharacter(
            CharacterDefinition c)
        {
            LabelPair("ID", c.characterId);
            LabelPair("Faction", c.faction.ToString());

            GUILayout.Space(4f);

            LabelPair("HP", c.maxHealth);
            LabelPair("Mana", c.maxMana);
            LabelPair("Armor", c.armor);
            LabelPair("Magic Guard", c.magicGuard);
            LabelPair("AP", c.actionPoints);
            LabelPair("Movement", $"{c.movementMeters:0.0} m");
            LabelPair("Initiative", Signed(c.initiativeBonus));

            GUILayout.Space(4f);

            LabelPair("Strength", c.strength);
            LabelPair("Finesse", c.finesse);
            LabelPair("Intellect", c.intellect);
            LabelPair("Willpower", c.willpower);
            LabelPair("Perception", c.perception);

            if (c.damageAffinities != null &&
                c.damageAffinities.Length > 0)
            {
                GUILayout.Space(6f);
                GUILayout.Label("Damage affinities:");

                foreach (DamageAffinity affinity
                         in c.damageAffinities)
                {
                    string read =
                        affinity.multiplier <= 0f
                            ? "IMMUNE"
                            : affinity.multiplier < 1f
                                ? $"RESIST x{affinity.multiplier:0.00}"
                                : affinity.multiplier > 1f
                                    ? $"VULNERABLE x{affinity.multiplier:0.00}"
                                    : "NORMAL";

                    GUILayout.Label(
                        $"• {affinity.damageType}: {read}");
                }
            }

            GUILayout.Space(6f);

            GUILayout.Label("Abilities:");

            CombatActionDefinition[] actions =
                c.startingActions;

            if (actions == null ||
                actions.Length == 0)
            {
                GUILayout.Label("—");
                return;
            }

            foreach (CombatActionDefinition action in actions)
            {
                if (action != null)
                {
                    GUILayout.Label(
                        $"• {action.displayName}");
                }
            }
        }

        private static void DrawWeapon(
            WeaponDefinition w)
        {
            DrawItem(w);

            GUILayout.Space(5f);

            LabelPair(
                "Weapon class",
                w.weaponClass.ToString());

            LabelPair(
                "Damage",
                $"{w.damage} {w.damageType}");

            LabelPair(
                "Scaling",
                w.scalingAttribute.ToString());

            LabelPair(
                "Range",
                $"{w.rangeMeters:0.0} m");

            LabelPair(
                "Two-handed",
                w.twoHanded ? "Yes" : "No");

            LabelPair(
                "Finesse",
                w.finesse ? "Yes" : "No");

            LabelPair(
                "Magic focus",
                w.magicalFocus ? "Yes" : "No");
        }

        private static void DrawArmor(
            ArmorDefinition armor)
        {
            DrawItem(armor);

            GUILayout.Space(5f);

            LabelPair(
                "Equipment slot",
                armor.equipmentSlot.ToString());

            LabelPair(
                "Armor bonus",
                Signed(armor.armorBonus));

            LabelPair(
                "Magic Guard bonus",
                Signed(armor.magicGuardBonus));

            LabelPair(
                "Movement bonus",
                $"{armor.movementBonus:+0.0;-0.0;0.0} m");

            if (armor.grantedActions != null &&
                armor.grantedActions.Length > 0)
            {
                GUILayout.Space(4f);
                GUILayout.Label("Granted actions:");

                foreach (CombatActionDefinition action
                         in armor.grantedActions)
                {
                    if (action != null)
                        GUILayout.Label(
                            $"• {action.displayName}");
                }
            }
        }

        private static void DrawItem(
            ItemDefinition item)
        {
            LabelPair("ID", item.itemId);
            LabelPair("Category", item.category.ToString());
            LabelPair("Rarity", item.rarity.ToString());
            LabelPair("Weight", $"{item.weight:0.00}");
            LabelPair("Value", $"{item.valueSilver} silver");
            LabelPair("Stackable", item.stackable ? "Yes" : "No");

            if (!string.IsNullOrWhiteSpace(item.description))
            {
                GUILayout.Space(5f);
                GUILayout.Label(item.description);
            }
        }

        private static void DrawAction(
            CombatActionDefinition a)
        {
            LabelPair("ID", a.actionId);
            LabelPair("Category", a.category.ToString());
            LabelPair("Target", a.targetKind.ToString());
            LabelPair("AP cost", a.actionPointCost);
            LabelPair("Mana cost", a.manaCost);
            LabelPair("Strain cost", a.strainCost);
            LabelPair("Range", $"{a.rangeMeters:0.0} m");
            LabelPair("AoE", $"{a.areaRadius:0.0} m");
            LabelPair("AoE rule", a.areaTargetRule.ToString());

            GUILayout.Space(4f);

            LabelPair(
                "Damage",
                $"{a.damage} {a.damageType}");

            LabelPair(
                "Scaling",
                $"{a.scalingAttribute} x{a.scalingMultiplier:0.0}");

            LabelPair(
                "Base hit",
                $"{a.baseHitChance}%");

            LabelPair(
                "Attack roll",
                a.requiresAttackRoll ? "Yes" : "No");

            LabelPair(
                "Line of sight",
                a.requiresLineOfSight ? "Required" : "Ignored");

            LabelPair(
                "Cover",
                a.ignoresCover ? "Ignored" : "Applied");

            LabelPair(
                "Height",
                a.usesHeightAdvantage ? "Applied" : "Ignored");

            if (a.freeMovementMetersGranted > 0f)
            {
                LabelPair(
                    "Free movement",
                    $"{a.freeMovementMetersGranted:0.0} m");

                LabelPair(
                    "Suppress reactions",
                    a.freeMovementSuppressesOpportunityAttacks
                        ? "Yes"
                        : "No");
            }

            if (a.presentationProfile != null)
            {
                LabelPair(
                    "Impact tier",
                    a.presentationProfile
                        .impactTier.ToString());
            }

            if (a.createsSurface != SurfaceType.None)
            {
                GUILayout.Space(4f);

                LabelPair(
                    "Creates surface",
                    a.createsSurface.ToString());

                LabelPair(
                    "Surface radius",
                    $"{a.surfaceRadius:0.0} m");

                LabelPair(
                    "Surface duration",
                    $"{a.surfaceDurationTurns} rounds");
            }

            if (!string.IsNullOrWhiteSpace(a.description))
            {
                GUILayout.Space(5f);
                GUILayout.Label(a.description);
            }
        }

        private void DrawLog()
        {
            CombatLogService log =
                CombatLogService.Instance;

            GUILayout.BeginHorizontal();

            GUILayout.Label(
                "COMBAT LOG",
                _sectionStyle);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(
                    "Clear",
                    GUILayout.Width(80f)))
            {
                log?.Clear();
            }

            GUILayout.EndHorizontal();

            _logScroll =
                GUILayout.BeginScrollView(
                    _logScroll,
                    GUI.skin.box);

            if (log == null)
            {
                GUILayout.Label(
                    "CombatLogService is not installed.");
            }
            else
            {
                IReadOnlyList<string> entries =
                    log.Entries;

                for (int i = entries.Count - 1;
                     i >= 0;
                     i--)
                {
                    GUILayout.Label(
                        entries[i],
                        _smallStyle);
                }
            }

            GUILayout.EndScrollView();
        }

        private void DrawCheats()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(
                GUI.skin.box);

            GUILayout.Label(
                "PARTY",
                _sectionStyle);

            if (GUILayout.Button(
                    "Restore party HP / MP"))
            {
                foreach (CombatantRuntime c in PartyMembers())
                {
                    c.DebugRestoreFull();
                }
            }

            if (GUILayout.Button(
                    "Restore turn resources"))
            {
                foreach (CombatantRuntime c in PartyMembers())
                {
                    c.DebugRestoreTurnResources();
                }
            }

            if (GUILayout.Button(
                    "Kill all enemies"))
            {
                foreach (CombatantRuntime c in Enemies())
                {
                    c.DebugKill();
                }
            }

            CombatantRuntime preferred =
                PreferredPartyMember();

            ArcaneStrainComponent strain =
                preferred != null
                    ? preferred.GetComponent<
                        ArcaneStrainComponent>()
                    : null;

            if (strain != null)
            {
                GUILayout.Space(6f);

                GUILayout.Label(
                    $"Current strain: " +
                    $"{strain.Current}/{strain.Max}");

                if (GUILayout.Button(
                        "Clear strain"))
                {
                    strain.Clear();
                    preferred.DebugRestoreTurnResources();
                }
            }

            GUILayout.Space(6f);

            if (GUILayout.Button(
                    "Solo test: disable allies"))
            {
                DisableOptionalAllies();
            }

            if (_disabledAllies.Count > 0 &&
                GUILayout.Button(
                    "Restore disabled allies"))
            {
                RestoreOptionalAllies();
            }

            GUILayout.EndVertical();

            GUILayout.Space(8f);

            GUILayout.BeginVertical(
                GUI.skin.box);

            GUILayout.Label(
                "COMBAT",
                _sectionStyle);

            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            if (director != null)
            {
                GUILayout.Label(
                    $"State: {director.State}");

                GUILayout.Label(
                    $"Round: {director.Round}");

                if (GUILayout.Button(
                        "Start / restart combat"))
                {
                    director.DebugRestartCombat();
                }

                if (GUILayout.Button(
                        "End current turn"))
                {
                    director.EndCurrentTurn();
                }
            }

            if (GUILayout.Button(
                    "Clear DEV spawns"))
            {
                ClearDevSpawns();
            }

            GUILayout.EndVertical();

            GUILayout.Space(8f);

            GUILayout.BeginVertical(
                GUI.skin.box);

            GUILayout.Label(
                "SURFACES",
                _sectionStyle);

            foreach (SurfaceType type in Enum
                         .GetValues(typeof(SurfaceType))
                         .Cast<SurfaceType>())
            {
                if (type == SurfaceType.None ||
                    type == SurfaceType.Detonation)
                {
                    continue;
                }

                if (GUILayout.Button(
                        $"Create {type} near party"))
                {
                    CreateSurface(type);
                }
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private UObject[] GetCurrentEntries()
        {
            if (catalog == null)
                return Array.Empty<UObject>();

            IEnumerable<UObject> source;

            switch (_tab)
            {
                case Tab.Characters:
                    source =
                        catalog.Characters
                            .Where(x =>
                                x != null &&
                                x.faction !=
                                    CombatFaction.Enemy);
                    break;

                case Tab.Enemies:
                    source =
                        catalog.Characters
                            .Where(x =>
                                x != null &&
                                x.faction ==
                                    CombatFaction.Enemy);
                    break;

                case Tab.Items:
                    source =
                        catalog.Items
                            .Where(x =>
                                x != null &&
                                !(x is WeaponDefinition));
                    break;

                case Tab.Weapons:
                    source =
                        catalog.Items
                            .OfType<WeaponDefinition>();
                    break;

                case Tab.Abilities:
                    source = catalog.Actions;
                    break;

                default:
                    return Array.Empty<UObject>();
            }

            if (!string.IsNullOrWhiteSpace(_search))
            {
                string query =
                    _search.Trim();

                source = source.Where(x =>
                    GetDisplayName(x)
                        .IndexOf(
                            query,
                            StringComparison.OrdinalIgnoreCase)
                    >= 0);
            }

            return source
                .OrderBy(GetDisplayName)
                .ToArray();
        }

        private void SpawnCharacter(
            CharacterDefinition definition)
        {
            if (definition == null)
                return;

            Vector3 anchor =
                GetPartyAnchor();

            int index = _spawned.Count;

            Vector3 offset =
                new Vector3(
                    2f + (index % 4) * 1.5f,
                    1f,
                    2f + (index / 4) * 1.5f);

            GameObject go =
                GameObject.CreatePrimitive(
                    PrimitiveType.Capsule);

            go.name =
                $"DEV_{definition.displayName}";

            go.transform.position =
                anchor + offset;

            CombatantRuntime runtime =
                go.AddComponent<CombatantRuntime>();

            runtime.SetDefinition(definition);

            go.AddComponent<TacticalUnitMover>();
            go.AddComponent<EquipmentComponent>();

            if (definition.characterId == "edward")
            {
                go.AddComponent<
                    ArcaneStrainComponent>();
            }

            if (definition.faction ==
                CombatFaction.Player ||
                definition.faction ==
                CombatFaction.Ally)
            {
                go.AddComponent<InventoryComponent>();
            }

            _spawned.Add(go);

            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            if (director != null)
            {
                director.AddParticipant(
                    runtime);
            }
        }

        private void GrantAbility(
            CombatActionDefinition action)
        {
            if (action == null)
                return;

            CombatantRuntime recipient =
                PreferredPartyMember();

            if (recipient == null)
                return;

            DeveloperGrantedActions grants =
                recipient.GetComponent<
                    DeveloperGrantedActions>();

            if (grants == null)
            {
                grants =
                    recipient.gameObject
                        .AddComponent<
                            DeveloperGrantedActions>();
            }

            if (action.strainCost > 0 &&
                recipient.GetComponent<
                    ArcaneStrainComponent>() == null)
            {
                recipient.gameObject
                    .AddComponent<
                        ArcaneStrainComponent>();
            }

            grants.Grant(action);
        }

        private void GiveItem(
            ItemDefinition item,
            int amount)
        {
            if (item == null ||
                amount <= 0)
            {
                return;
            }

            CombatantRuntime recipient =
                PreferredPartyMember();

            if (recipient == null)
                return;

            InventoryComponent inventory =
                recipient
                    .GetComponent<
                        InventoryComponent>();

            if (inventory == null)
            {
                inventory =
                    recipient.gameObject
                        .AddComponent<
                            InventoryComponent>();
            }

            inventory.Add(item, amount);
        }

        private void EquipItem(
            ItemDefinition item)
        {
            if (item == null)
                return;

            CombatantRuntime recipient =
                PreferredPartyMember();

            if (recipient == null)
                return;

            InventoryComponent inventory =
                recipient.GetComponent<
                    InventoryComponent>();

            if (inventory == null)
            {
                inventory =
                    recipient.gameObject
                        .AddComponent<
                            InventoryComponent>();
            }

            if (inventory.Count(item) <= 0)
                inventory.Add(item, 1);

            EquipmentComponent equipment =
                recipient.GetComponent<
                    EquipmentComponent>();

            if (equipment == null)
            {
                equipment =
                    recipient.gameObject
                        .AddComponent<
                            EquipmentComponent>();
            }

            if (equipment.Equip(item))
                recipient.DebugRestoreTurnResources();
        }

        private void CreateSurface(
            SurfaceType type)
        {
            ElementalSurfaceSystem system =
                ElementalSurfaceSystem.Instance;

            if (system == null)
                return;

            Vector3 center =
                GetPartyAnchor() +
                new Vector3(2f, 0f, 0f);

            system.CreateOrReact(
                type,
                center,
                2.5f,
                3,
                gameObject);
        }

        private void ClearDevSpawns()
        {
            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            bool removedAny = false;

            foreach (GameObject go in _spawned)
            {
                if (go == null)
                    continue;

                CombatantRuntime runtime =
                    go.GetComponent<
                        CombatantRuntime>();

                if (runtime != null &&
                    director != null)
                {
                    director.Unregister(runtime);
                }

                Destroy(go);
                removedAny = true;
            }

            _spawned.Clear();

            if (removedAny &&
                director != null &&
                director.State ==
                    CombatState.Active)
            {
                director.DebugRestartCombat();
            }
        }

        private void DisableOptionalAllies()
        {
            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            CombatantRuntime[] allies =
                FindObjectsByType<
                    CombatantRuntime>(
                    FindObjectsSortMode.None)
                .Where(x =>
                    x != null &&
                    x.gameObject.activeInHierarchy &&
                    x.Faction ==
                        CombatFaction.Ally)
                .ToArray();

            foreach (CombatantRuntime ally in allies)
            {
                if (!_disabledAllies.Contains(
                        ally.gameObject))
                {
                    _disabledAllies.Add(
                        ally.gameObject);
                }

                if (director != null)
                    director.Unregister(ally);

                ally.gameObject.SetActive(false);
            }

            if (allies.Length > 0 &&
                director != null &&
                director.State ==
                    CombatState.Active)
            {
                director.DebugRestartCombat();
            }
        }

        private void RestoreOptionalAllies()
        {
            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            foreach (GameObject go
                     in _disabledAllies.ToArray())
            {
                if (go == null)
                {
                    _disabledAllies.Remove(go);
                    continue;
                }

                go.SetActive(true);

                CombatantRuntime runtime =
                    go.GetComponent<
                        CombatantRuntime>();

                if (runtime != null &&
                    director != null)
                {
                    director.AddParticipant(runtime);
                }
            }

            _disabledAllies.Clear();

            if (director != null &&
                director.State ==
                    CombatState.Active)
            {
                director.DebugRestartCombat();
            }
        }

        private static IEnumerable<CombatantRuntime>
            PartyMembers()
        {
            return FindObjectsByType<
                    CombatantRuntime>(
                    FindObjectsSortMode.None)
                .Where(x =>
                    x != null &&
                    x.CanBeTargeted &&
                    (x.Faction ==
                         CombatFaction.Player ||
                     x.Faction ==
                         CombatFaction.Ally));
        }

        private static IEnumerable<CombatantRuntime>
            Enemies()
        {
            return FindObjectsByType<
                    CombatantRuntime>(
                    FindObjectsSortMode.None)
                .Where(x =>
                    x != null &&
                    x.IsAlive &&
                    x.Faction ==
                        CombatFaction.Enemy);
        }

        private static CombatantRuntime
            PreferredPartyMember()
        {
            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            CombatantRuntime current =
                director != null
                    ? director.CurrentActor
                    : null;

            if (current != null &&
                (current.Faction ==
                     CombatFaction.Player ||
                 current.Faction ==
                     CombatFaction.Ally))
            {
                return current;
            }

            return PartyMembers()
                .OrderBy(x =>
                    x.Faction ==
                        CombatFaction.Player
                        ? 0
                        : 1)
                .FirstOrDefault();
        }

        private static Vector3 GetPartyAnchor()
        {
            CombatantRuntime preferred =
                PreferredPartyMember();

            return preferred != null
                ? preferred.transform.position
                : Vector3.zero;
        }

        private static string GetDisplayName(
            UObject obj)
        {
            switch (obj)
            {
                case CharacterDefinition c:
                    return string.IsNullOrWhiteSpace(
                        c.displayName)
                        ? c.name
                        : c.displayName;

                case ItemDefinition i:
                    return string.IsNullOrWhiteSpace(
                        i.displayName)
                        ? i.name
                        : i.displayName;

                case CombatActionDefinition a:
                    return string.IsNullOrWhiteSpace(
                        a.displayName)
                        ? a.name
                        : a.displayName;

                default:
                    return obj != null
                        ? obj.name
                        : "None";
            }
        }

        private static string Signed(int value)
        {
            return value > 0
                ? $"+{value}"
                : value.ToString();
        }

        private static void LabelPair(
            string label,
            object value)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(
                label,
                GUILayout.Width(120f));

            GUILayout.Label(
                value?.ToString() ?? "—");

            GUILayout.EndHorizontal();
        }

        private void EnsureStyles()
        {
            if (_windowStyle != null)
                return;

            _windowStyle =
                new GUIStyle(GUI.skin.window)
                {
                    padding =
                        new RectOffset(
                            14,
                            14,
                            12,
                            12)
                };

            _headerStyle =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 18,
                    fontStyle =
                        FontStyle.Bold
                };

            _sectionStyle =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 15,
                    fontStyle =
                        FontStyle.Bold
                };

            _smallStyle =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12
                };
        }
    }
}
