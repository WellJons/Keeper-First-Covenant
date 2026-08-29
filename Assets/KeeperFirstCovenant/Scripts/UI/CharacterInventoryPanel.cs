using System;
using System.Collections.Generic;
using System.Linq;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public sealed class CharacterInventoryPanel
    {
        private GameObject root;
        private RectTransform partyList;
        private RectTransform equipmentList;
        private RectTransform inventoryList;

        private Text characterName;
        private Text characterState;
        private Text attributes;
        private Text derivedStats;
        private Text inventoryWeight;
        private Text itemTitle;
        private Text itemDetails;
        private Text status;

        private CombatantRuntime selected;
        private Action onBack;

        public bool IsActive =>
            root != null &&
            root.activeSelf;

        public void Build(
            Transform parent,
            Action backAction)
        {
            onBack = backAction;

            RectTransform overlay =
                MenuUiFactory.CreateRect(
                    "CharacterInventoryPanel",
                    parent);

            MenuUiFactory.Stretch(overlay);

            Image dim =
                overlay.gameObject
                    .AddComponent<Image>();

            dim.color =
                new Color(
                    0.004f,
                    0.007f,
                    0.010f,
                    0.70f);

            root = overlay.gameObject;

            Image window =
                MenuUiFactory.CreateImage(
                    "Window",
                    overlay,
                    new Color(
                        MainMenuTheme.Panel.r,
                        MainMenuTheme.Panel.g,
                        MainMenuTheme.Panel.b,
                        0.98f));

            RectTransform windowRect =
                window.rectTransform;

            windowRect.anchorMin =
                windowRect.anchorMax =
                    new Vector2(0.5f, 0.5f);

            windowRect.pivot =
                new Vector2(0.5f, 0.5f);

            windowRect.sizeDelta =
                new Vector2(1540f, 850f);

            Outline outline =
                window.gameObject
                    .AddComponent<Outline>();

            outline.effectColor =
                new Color(
                    MainMenuTheme.SilverDim.r,
                    MainMenuTheme.SilverDim.g,
                    MainMenuTheme.SilverDim.b,
                    0.58f);

            outline.effectDistance =
                new Vector2(1f, -1f);

            Text heading =
                MenuUiFactory.CreateText(
                    "Heading",
                    window.transform,
                    "ПЕРСОНАЖИ И ИНВЕНТАРЬ",
                    28,
                    MainMenuTheme.Text,
                    TextAnchor.MiddleLeft);

            heading.rectTransform.anchorMin =
                new Vector2(0f, 1f);

            heading.rectTransform.anchorMax =
                new Vector2(1f, 1f);

            heading.rectTransform.pivot =
                new Vector2(0f, 1f);

            heading.rectTransform.offsetMin =
                new Vector2(34f, -82f);

            heading.rectTransform.offsetMax =
                new Vector2(-180f, -20f);

            Button back =
                MenuUiFactory.CreateMenuButton(
                    "Back",
                    window.transform,
                    "Назад",
                    17);

            RectTransform backRect =
                back.GetComponent<RectTransform>();

            backRect.anchorMin =
                backRect.anchorMax =
                    new Vector2(1f, 1f);

            backRect.pivot =
                new Vector2(1f, 1f);

            backRect.anchoredPosition =
                new Vector2(-28f, -22f);

            backRect.sizeDelta =
                new Vector2(136f, 46f);

            back.onClick.AddListener(() =>
            {
                GameAudioService.Instance.PlayClick();
                Hide();
                onBack?.Invoke();
            });

            BuildPartyColumn(window.transform);
            BuildCharacterColumn(window.transform);
            BuildInventoryColumn(window.transform);

            status =
                MenuUiFactory.CreateText(
                    "Status",
                    window.transform,
                    string.Empty,
                    14,
                    MainMenuTheme.Silver,
                    TextAnchor.MiddleLeft);

            status.rectTransform.anchorMin =
                new Vector2(0f, 0f);

            status.rectTransform.anchorMax =
                new Vector2(1f, 0f);

            status.rectTransform.offsetMin =
                new Vector2(34f, 18f);

            status.rectTransform.offsetMax =
                new Vector2(-34f, 52f);

            root.SetActive(false);
        }

        public void Show()
        {
            if (root == null)
                return;

            root.SetActive(true);
            status.text = string.Empty;
            RefreshParty();

            if (selected == null)
            {
                CombatantRuntime[] party =
                    GetParty();

                if (party.Length > 0)
                    selected = party[0];
            }

            RefreshSelected();
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }

        private void BuildPartyColumn(
            Transform window)
        {
            Image panel =
                MenuUiFactory.CreateImage(
                    "PartyColumn",
                    window,
                    new Color(
                        MainMenuTheme.PanelSoft.r,
                        MainMenuTheme.PanelSoft.g,
                        MainMenuTheme.PanelSoft.b,
                        0.64f));

            RectTransform rect =
                panel.rectTransform;

            rect.anchorMin =
                new Vector2(0f, 0f);

            rect.anchorMax =
                new Vector2(0f, 1f);

            rect.pivot =
                new Vector2(0f, 0.5f);

            rect.offsetMin =
                new Vector2(28f, 68f);

            rect.offsetMax =
                new Vector2(278f, -102f);

            Text title =
                MenuUiFactory.CreateText(
                    "Title",
                    panel.transform,
                    "ГРУППА",
                    17,
                    MainMenuTheme.Silver,
                    TextAnchor.MiddleLeft);

            title.rectTransform.anchorMin =
                new Vector2(0f, 1f);

            title.rectTransform.anchorMax =
                new Vector2(1f, 1f);

            title.rectTransform.offsetMin =
                new Vector2(14f, -52f);

            title.rectTransform.offsetMax =
                new Vector2(-14f, -8f);

            partyList =
                CreateScrollList(
                    panel.transform,
                    "PartyList",
                    new Vector2(0f, 0f),
                    new Vector2(1f, 1f),
                    new Vector2(10f, 10f),
                    new Vector2(-10f, -58f),
                    8f);
        }

        private void BuildCharacterColumn(
            Transform window)
        {
            Image panel =
                MenuUiFactory.CreateImage(
                    "CharacterColumn",
                    window,
                    new Color(
                        0.025f,
                        0.031f,
                        0.037f,
                        0.74f));

            RectTransform rect =
                panel.rectTransform;

            rect.anchorMin =
                new Vector2(0f, 0f);

            rect.anchorMax =
                new Vector2(0f, 1f);

            rect.pivot =
                new Vector2(0f, 0.5f);

            rect.offsetMin =
                new Vector2(294f, 68f);

            rect.offsetMax =
                new Vector2(812f, -102f);

            characterName =
                MenuUiFactory.CreateText(
                    "Name",
                    panel.transform,
                    string.Empty,
                    28,
                    MainMenuTheme.Text,
                    TextAnchor.UpperLeft);

            characterName.rectTransform.anchorMin =
                new Vector2(0f, 0.88f);

            characterName.rectTransform.anchorMax =
                Vector2.one;

            characterName.rectTransform.offsetMin =
                new Vector2(22f, 0f);

            characterName.rectTransform.offsetMax =
                new Vector2(-22f, -18f);

            characterState =
                MenuUiFactory.CreateText(
                    "State",
                    panel.transform,
                    string.Empty,
                    14,
                    MainMenuTheme.MutedText,
                    TextAnchor.UpperLeft);

            characterState.rectTransform.anchorMin =
                new Vector2(0f, 0.79f);

            characterState.rectTransform.anchorMax =
                new Vector2(1f, 0.90f);

            characterState.rectTransform.offsetMin =
                new Vector2(22f, 0f);

            characterState.rectTransform.offsetMax =
                new Vector2(-22f, 0f);

            attributes =
                MenuUiFactory.CreateText(
                    "Attributes",
                    panel.transform,
                    string.Empty,
                    16,
                    MainMenuTheme.Text,
                    TextAnchor.UpperLeft);

            attributes.rectTransform.anchorMin =
                new Vector2(0f, 0.57f);

            attributes.rectTransform.anchorMax =
                new Vector2(0.50f, 0.79f);

            attributes.rectTransform.offsetMin =
                new Vector2(22f, 0f);

            attributes.rectTransform.offsetMax =
                new Vector2(-6f, 0f);

            derivedStats =
                MenuUiFactory.CreateText(
                    "DerivedStats",
                    panel.transform,
                    string.Empty,
                    16,
                    MainMenuTheme.Text,
                    TextAnchor.UpperLeft);

            derivedStats.rectTransform.anchorMin =
                new Vector2(0.50f, 0.57f);

            derivedStats.rectTransform.anchorMax =
                new Vector2(1f, 0.79f);

            derivedStats.rectTransform.offsetMin =
                new Vector2(6f, 0f);

            derivedStats.rectTransform.offsetMax =
                new Vector2(-22f, 0f);

            Text equipmentHeading =
                MenuUiFactory.CreateText(
                    "EquipmentHeading",
                    panel.transform,
                    "ЭКИПИРОВКА",
                    16,
                    MainMenuTheme.Silver,
                    TextAnchor.MiddleLeft);

            equipmentHeading.rectTransform.anchorMin =
                new Vector2(0f, 0.51f);

            equipmentHeading.rectTransform.anchorMax =
                new Vector2(1f, 0.57f);

            equipmentHeading.rectTransform.offsetMin =
                new Vector2(22f, 0f);

            equipmentHeading.rectTransform.offsetMax =
                new Vector2(-22f, 0f);

            equipmentList =
                CreateScrollList(
                    panel.transform,
                    "EquipmentList",
                    new Vector2(0f, 0f),
                    new Vector2(1f, 0.51f),
                    new Vector2(18f, 14f),
                    new Vector2(-18f, -4f),
                    6f);
        }

        private void BuildInventoryColumn(
            Transform window)
        {
            Image panel =
                MenuUiFactory.CreateImage(
                    "InventoryColumn",
                    window,
                    new Color(
                        0.025f,
                        0.031f,
                        0.037f,
                        0.74f));

            RectTransform rect =
                panel.rectTransform;

            rect.anchorMin =
                new Vector2(0f, 0f);

            rect.anchorMax =
                new Vector2(1f, 1f);

            rect.offsetMin =
                new Vector2(830f, 68f);

            rect.offsetMax =
                new Vector2(-28f, -102f);

            Text heading =
                MenuUiFactory.CreateText(
                    "Heading",
                    panel.transform,
                    "ИНВЕНТАРЬ",
                    18,
                    MainMenuTheme.Silver,
                    TextAnchor.MiddleLeft);

            heading.rectTransform.anchorMin =
                new Vector2(0f, 0.91f);

            heading.rectTransform.anchorMax =
                Vector2.one;

            heading.rectTransform.offsetMin =
                new Vector2(18f, 0f);

            heading.rectTransform.offsetMax =
                new Vector2(-18f, 0f);

            inventoryWeight =
                MenuUiFactory.CreateText(
                    "Weight",
                    panel.transform,
                    string.Empty,
                    14,
                    MainMenuTheme.MutedText,
                    TextAnchor.MiddleRight);

            inventoryWeight.rectTransform.anchorMin =
                new Vector2(0.42f, 0.91f);

            inventoryWeight.rectTransform.anchorMax =
                Vector2.one;

            inventoryWeight.rectTransform.offsetMin =
                Vector2.zero;

            inventoryWeight.rectTransform.offsetMax =
                new Vector2(-18f, 0f);

            inventoryList =
                CreateScrollList(
                    panel.transform,
                    "InventoryList",
                    new Vector2(0f, 0.34f),
                    new Vector2(1f, 0.91f),
                    new Vector2(14f, 8f),
                    new Vector2(-14f, -6f),
                    6f);

            Image details =
                MenuUiFactory.CreateImage(
                    "ItemDetails",
                    panel.transform,
                    new Color(
                        MainMenuTheme.PanelSoft.r,
                        MainMenuTheme.PanelSoft.g,
                        MainMenuTheme.PanelSoft.b,
                        0.54f));

            RectTransform detailsRect =
                details.rectTransform;

            detailsRect.anchorMin =
                new Vector2(0f, 0f);

            detailsRect.anchorMax =
                new Vector2(1f, 0.33f);

            detailsRect.offsetMin =
                new Vector2(14f, 14f);

            detailsRect.offsetMax =
                new Vector2(-14f, -8f);

            itemTitle =
                MenuUiFactory.CreateText(
                    "ItemTitle",
                    details.transform,
                    "Выберите предмет",
                    18,
                    MainMenuTheme.Text,
                    TextAnchor.UpperLeft);

            itemTitle.rectTransform.anchorMin =
                new Vector2(0f, 0.72f);

            itemTitle.rectTransform.anchorMax =
                Vector2.one;

            itemTitle.rectTransform.offsetMin =
                new Vector2(16f, 0f);

            itemTitle.rectTransform.offsetMax =
                new Vector2(-16f, -12f);

            itemDetails =
                MenuUiFactory.CreateText(
                    "ItemText",
                    details.transform,
                    string.Empty,
                    14,
                    MainMenuTheme.MutedText,
                    TextAnchor.UpperLeft);

            itemDetails.rectTransform.anchorMin =
                Vector2.zero;

            itemDetails.rectTransform.anchorMax =
                new Vector2(1f, 0.75f);

            itemDetails.rectTransform.offsetMin =
                new Vector2(16f, 12f);

            itemDetails.rectTransform.offsetMax =
                new Vector2(-16f, 0f);
        }

        private void RefreshParty()
        {
            for (int i =
                     partyList.childCount - 1;
                 i >= 0;
                 i--)
            {
                UnityEngine.Object.Destroy(
                    partyList.GetChild(i)
                        .gameObject);
            }

            CombatantRuntime[] party =
                GetParty();

            if (selected != null &&
                !party.Contains(selected))
            {
                selected = null;
            }

            foreach (CombatantRuntime member
                     in party)
            {
                CombatantRuntime captured =
                    member;

                Button button =
                    MenuUiFactory.CreateMenuButton(
                        "Member_" +
                        member.Definition.characterId,
                        partyList,
                        member.Definition.displayName,
                        16);

                LayoutElement element =
                    button.gameObject
                        .AddComponent<LayoutElement>();

                element.preferredHeight = 54f;
                element.minHeight = 54f;

                Text label =
                    button.transform.Find("Label")
                        .GetComponent<Text>();

                if (member == selected)
                    label.color = MainMenuTheme.Warm;

                button.onClick.AddListener(() =>
                {
                    GameAudioService.Instance.PlayClick();
                    selected = captured;
                    RefreshParty();
                    RefreshSelected();
                });
            }
        }

        private void RefreshSelected()
        {
            ClearList(equipmentList);
            ClearList(inventoryList);

            if (selected == null ||
                selected.Definition == null)
            {
                characterName.text =
                    "Нет персонажа";

                characterState.text =
                    string.Empty;

                attributes.text =
                    string.Empty;

                derivedStats.text =
                    string.Empty;

                inventoryWeight.text =
                    string.Empty;

                itemTitle.text =
                    "Выберите персонажа";

                itemDetails.text =
                    string.Empty;

                return;
            }

            CharacterDefinitionView();
            RefreshEquipment();
            RefreshInventory();
        }

        private void CharacterDefinitionView()
        {
            var definition =
                selected.Definition;

            characterName.text =
                definition.displayName;

            string state =
                selected.IsDead
                    ? "Мёртв"
                    : selected.IsDowned
                        ? "Тяжело ранен"
                        : "В строю";

            characterState.text =
                $"{state}   •   " +
                $"HP {selected.CurrentHealth}/{definition.maxHealth}   •   " +
                $"MP {selected.CurrentMana}/{definition.maxMana}";

            attributes.text =
                "СИЛА        " + definition.strength + "\n" +
                "ЛОВКОСТЬ    " + definition.finesse + "\n" +
                "ИНТЕЛЛЕКТ   " + definition.intellect + "\n" +
                "ВОЛЯ        " + definition.willpower + "\n" +
                "ВОСПРИЯТИЕ  " + definition.perception;

            EquipmentComponent equipment =
                selected.GetComponent<
                    EquipmentComponent>();

            int armorBonus =
                equipment != null
                    ? equipment.GetArmorBonus()
                    : 0;

            int guardBonus =
                equipment != null
                    ? equipment.GetMagicGuardBonus()
                    : 0;

            float moveBonus =
                equipment != null
                    ? equipment.GetMovementBonus()
                    : 0f;

            derivedStats.text =
                "БРОНЯ      " +
                (definition.armor + armorBonus) + "\n" +
                "МАГ. ЗАЩ.  " +
                (definition.magicGuard + guardBonus) + "\n" +
                "ДВИЖЕНИЕ   " +
                (definition.movementMeters + moveBonus)
                    .ToString("0.0") + " м\n" +
                "ОЧКИ ДЕЙСТВИЙ  " +
                definition.actionPoints + "\n" +
                "БАРЬЕР     " +
                selected.Barrier;
        }

        private void RefreshEquipment()
        {
            EquipmentComponent equipment =
                selected.GetComponent<
                    EquipmentComponent>();

            InventoryComponent inventory =
                selected.GetComponent<
                    InventoryComponent>();

            EquipmentSlot[] slots =
                (EquipmentSlot[])
                    Enum.GetValues(
                        typeof(EquipmentSlot));

            foreach (EquipmentSlot slot
                     in slots)
            {
                ItemDefinition item =
                    equipment != null
                        ? equipment.Get(slot)
                        : null;

                Image row =
                    MenuUiFactory.CreateImage(
                        "Slot_" + slot,
                        equipmentList,
                        new Color(
                            0.04f,
                            0.048f,
                            0.055f,
                            0.76f));

                LayoutElement element =
                    row.gameObject
                        .AddComponent<LayoutElement>();

                element.preferredHeight = 43f;
                element.minHeight = 43f;

                Text label =
                    MenuUiFactory.CreateText(
                        "Label",
                        row.transform,
                        SlotName(slot) +
                        ":  " +
                        (item != null
                            ? item.displayName
                            : "—"),
                        14,
                        item != null
                            ? MainMenuTheme.Text
                            : MainMenuTheme.DisabledText,
                        TextAnchor.MiddleLeft);

                label.rectTransform.anchorMin =
                    Vector2.zero;

                label.rectTransform.anchorMax =
                    new Vector2(0.77f, 1f);

                label.rectTransform.offsetMin =
                    new Vector2(10f, 0f);

                label.rectTransform.offsetMax =
                    Vector2.zero;

                if (item == null ||
                    equipment == null ||
                    inventory == null)
                {
                    continue;
                }

                EquipmentSlot capturedSlot =
                    slot;

                Button remove =
                    MenuUiFactory.CreateMenuButton(
                        "Remove",
                        row.transform,
                        "Снять",
                        13);

                RectTransform removeRect =
                    remove.GetComponent<
                        RectTransform>();

                removeRect.anchorMin =
                    removeRect.anchorMax =
                        new Vector2(1f, 0.5f);

                removeRect.pivot =
                    new Vector2(1f, 0.5f);

                removeRect.anchoredPosition =
                    new Vector2(-6f, 0f);

                removeRect.sizeDelta =
                    new Vector2(92f, 34f);

                remove.onClick.AddListener(() =>
                {
                    GameAudioService.Instance.PlayClick();

                    if (equipment.TryUnequipToInventory(
                            inventory,
                            capturedSlot,
                            out string error))
                    {
                        SetStatus("Предмет снят.");
                    }
                    else
                    {
                        SetStatus(error);
                    }

                    RefreshSelected();
                });
            }
        }

        private void RefreshInventory()
        {
            InventoryComponent inventory =
                selected.GetComponent<
                    InventoryComponent>();

            EquipmentComponent equipment =
                selected.GetComponent<
                    EquipmentComponent>();

            if (inventory == null)
            {
                inventoryWeight.text =
                    "Нет инвентаря";

                itemTitle.text =
                    "Инвентарь недоступен";

                return;
            }

            inventoryWeight.text =
                $"Вес {inventory.CurrentWeight:0.0}/" +
                $"{inventory.MaxCarryWeight:0.0}";

            var grouped =
                inventory.Items
                    .Where(stack =>
                        stack?.item != null &&
                        stack.amount > 0)
                    .GroupBy(stack =>
                        stack.item)
                    .Select(group =>
                        new
                        {
                            item = group.Key,
                            amount = group.Sum(
                                value =>
                                    Mathf.Max(
                                        0,
                                        value.amount))
                        })
                    .OrderBy(value =>
                        value.item.category)
                    .ThenBy(value =>
                        value.item.displayName)
                    .ToArray();

            if (grouped.Length == 0)
            {
                Text empty =
                    MenuUiFactory.CreateText(
                        "Empty",
                        inventoryList,
                        "Инвентарь пуст.",
                        15,
                        MainMenuTheme.MutedText,
                        TextAnchor.MiddleCenter);

                empty.gameObject
                    .AddComponent<LayoutElement>()
                    .preferredHeight = 70f;

                return;
            }

            foreach (var entry in grouped)
            {
                ItemDefinition item =
                    entry.item;

                Image row =
                    MenuUiFactory.CreateImage(
                        "Item_" + item.itemId,
                        inventoryList,
                        new Color(
                            0.04f,
                            0.048f,
                            0.055f,
                            0.76f));

                LayoutElement element =
                    row.gameObject
                        .AddComponent<LayoutElement>();

                element.preferredHeight = 48f;
                element.minHeight = 48f;

                Button inspect =
                    row.gameObject
                        .AddComponent<Button>();

                inspect.transition =
                    Selectable.Transition.None;

                Text label =
                    MenuUiFactory.CreateText(
                        "Label",
                        row.transform,
                        item.displayName +
                        (entry.amount > 1
                            ? $"  ×{entry.amount}"
                            : string.Empty),
                        14,
                        MainMenuTheme.Text,
                        TextAnchor.MiddleLeft);

                label.rectTransform.anchorMin =
                    Vector2.zero;

                label.rectTransform.anchorMax =
                    new Vector2(0.72f, 1f);

                label.rectTransform.offsetMin =
                    new Vector2(12f, 0f);

                label.rectTransform.offsetMax =
                    Vector2.zero;

                inspect.onClick.AddListener(() =>
                {
                    GameAudioService.Instance.PlayClick();
                    ShowItem(item);
                });

                if (!CanEquip(item) ||
                    equipment == null)
                {
                    continue;
                }

                Button equip =
                    MenuUiFactory.CreateMenuButton(
                        "Equip",
                        row.transform,
                        "Экип.",
                        13);

                RectTransform equipRect =
                    equip.GetComponent<
                        RectTransform>();

                equipRect.anchorMin =
                    equipRect.anchorMax =
                        new Vector2(1f, 0.5f);

                equipRect.pivot =
                    new Vector2(1f, 0.5f);

                equipRect.anchoredPosition =
                    new Vector2(-6f, 0f);

                equipRect.sizeDelta =
                    new Vector2(105f, 36f);

                equip.onClick.AddListener(() =>
                {
                    GameAudioService.Instance.PlayClick();

                    if (equipment.TryEquipFromInventory(
                            inventory,
                            item,
                            out string error))
                    {
                        SetStatus(
                            item.displayName +
                            " экипирован.");
                    }
                    else
                    {
                        SetStatus(error);
                    }

                    RefreshSelected();
                });
            }
        }

        private void ShowItem(
            ItemDefinition item)
        {
            if (item == null)
                return;

            itemTitle.text =
                item.displayName;

            string type =
                CategoryName(
                    item.category);

            string extra =
                string.Empty;

            if (item is WeaponDefinition weapon)
            {
                extra =
                    "\nОружие: " +
                    weapon.weaponClass +
                    "\nДальность: " +
                    weapon.rangeMeters.ToString("0.0") +
                    " м" +
                    (weapon.twoHanded
                        ? "\nДвуручное"
                        : string.Empty);
            }
            else if (item is ArmorDefinition armor)
            {
                extra =
                    "\nСлот: " +
                    SlotName(
                        armor.equipmentSlot) +
                    "\nБроня: +" +
                    armor.armorBonus +
                    "\nМагическая защита: +" +
                    armor.magicGuardBonus;
            }

            itemDetails.text =
                type +
                "   •   " +
                RarityName(item.rarity) +
                "\nВес: " +
                item.weight.ToString("0.0") +
                "   •   Стоимость: " +
                item.valueSilver +
                " серебра" +
                extra +
                (string.IsNullOrWhiteSpace(
                    item.description)
                    ? string.Empty
                    : "\n\n" +
                      item.description);
        }

        private void SetStatus(
            string message)
        {
            if (status != null)
                status.text = message;
        }

        private static CombatantRuntime[]
            GetParty()
        {
            return UnityEngine.Object
                .FindObjectsByType<
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
        }

        private static bool CanEquip(
            ItemDefinition item)
        {
            return item is WeaponDefinition ||
                   item is ArmorDefinition;
        }

        private static void ClearList(
            RectTransform root)
        {
            if (root == null)
                return;

            for (int i =
                     root.childCount - 1;
                 i >= 0;
                 i--)
            {
                UnityEngine.Object.Destroy(
                    root.GetChild(i)
                        .gameObject);
            }
        }

        private static RectTransform
            CreateScrollList(
                Transform parent,
                string name,
                Vector2 anchorMin,
                Vector2 anchorMax,
                Vector2 offsetMin,
                Vector2 offsetMax,
                float spacing)
        {
            RectTransform viewport =
                MenuUiFactory.CreateRect(
                    name + "_Viewport",
                    parent);

            viewport.anchorMin =
                anchorMin;

            viewport.anchorMax =
                anchorMax;

            viewport.offsetMin =
                offsetMin;

            viewport.offsetMax =
                offsetMax;

            Image maskImage =
                viewport.gameObject
                    .AddComponent<Image>();

            maskImage.color =
                new Color(0f, 0f, 0f, 0.01f);

            RectMask2D mask =
                viewport.gameObject
                    .AddComponent<RectMask2D>();

            RectTransform content =
                MenuUiFactory.CreateRect(
                    name,
                    viewport);

            content.anchorMin =
                new Vector2(0f, 1f);

            content.anchorMax =
                new Vector2(1f, 1f);

            content.pivot =
                new Vector2(0.5f, 1f);

            content.anchoredPosition =
                Vector2.zero;

            content.sizeDelta =
                Vector2.zero;

            VerticalLayoutGroup layout =
                content.gameObject
                    .AddComponent<
                        VerticalLayoutGroup>();

            layout.spacing = spacing;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            ContentSizeFitter fitter =
                content.gameObject
                    .AddComponent<
                        ContentSizeFitter>();

            fitter.verticalFit =
                ContentSizeFitter.FitMode
                    .PreferredSize;

            ScrollRect scroll =
                viewport.gameObject
                    .AddComponent<ScrollRect>();

            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.movementType =
                ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24f;

            return content;
        }

        private static string SlotName(
            EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.MainHand:
                    return "Основная рука";
                case EquipmentSlot.OffHand:
                    return "Вторая рука";
                case EquipmentSlot.Head:
                    return "Голова";
                case EquipmentSlot.Chest:
                    return "Торс";
                case EquipmentSlot.Hands:
                    return "Руки";
                case EquipmentSlot.Legs:
                    return "Ноги";
                case EquipmentSlot.Feet:
                    return "Обувь";
                case EquipmentSlot.Cloak:
                    return "Плащ";
                case EquipmentSlot.Amulet:
                    return "Амулет";
                case EquipmentSlot.RingLeft:
                    return "Левое кольцо";
                case EquipmentSlot.RingRight:
                    return "Правое кольцо";
                default:
                    return slot.ToString();
            }
        }

        private static string CategoryName(
            ItemCategory category)
        {
            switch (category)
            {
                case ItemCategory.Weapon:
                    return "Оружие";
                case ItemCategory.Armor:
                    return "Броня";
                case ItemCategory.Consumable:
                    return "Расходуемое";
                case ItemCategory.Ingredient:
                    return "Ингредиент";
                case ItemCategory.Key:
                    return "Ключ";
                case ItemCategory.Quest:
                    return "Квестовый предмет";
                case ItemCategory.Treasure:
                    return "Ценность";
                default:
                    return "Предмет";
            }
        }

        private static string RarityName(
            ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Uncommon:
                    return "Необычный";
                case ItemRarity.Rare:
                    return "Редкий";
                case ItemRarity.Epic:
                    return "Эпический";
                case ItemRarity.Legendary:
                    return "Легендарный";
                case ItemRarity.Unique:
                    return "Уникальный";
                default:
                    return "Обычный";
            }
        }
    }
}
