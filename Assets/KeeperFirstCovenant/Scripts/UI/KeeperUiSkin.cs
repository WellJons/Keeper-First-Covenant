using KeeperFirstCovenant.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public static class KeeperUiSkin
    {
        public static void DecorateMajorPanel(
            Image panel,
            bool warmFocus = false)
        {
            if (panel == null)
                return;

            Color border =
                warmFocus
                    ? new Color(
                        MainMenuTheme.Warm.r,
                        MainMenuTheme.Warm.g,
                        MainMenuTheme.Warm.b,
                        0.48f)
                    : new Color(
                        MainMenuTheme.SilverDim.r,
                        MainMenuTheme.SilverDim.g,
                        MainMenuTheme.SilverDim.b,
                        0.62f);

            Outline outline =
                panel.GetComponent<Outline>();

            if (outline == null)
                outline =
                    panel.gameObject
                        .AddComponent<Outline>();

            outline.effectColor = border;
            outline.effectDistance =
                new Vector2(1f, -1f);

            AddCornerMarks(
                panel.transform,
                border,
                24f,
                2f);

            Image topRule =
                MenuUiFactory.CreateImage(
                    "Skin_TopRule",
                    panel.transform,
                    warmFocus
                        ? new Color(
                            MainMenuTheme.Warm.r,
                            MainMenuTheme.Warm.g,
                            MainMenuTheme.Warm.b,
                            0.58f)
                        : new Color(
                            MainMenuTheme.Silver.r,
                            MainMenuTheme.Silver.g,
                            MainMenuTheme.Silver.b,
                            0.22f));

            RectTransform rule =
                topRule.rectTransform;

            rule.anchorMin =
                new Vector2(0.08f, 1f);

            rule.anchorMax =
                new Vector2(0.92f, 1f);

            rule.pivot =
                new Vector2(0.5f, 1f);

            rule.offsetMin =
                new Vector2(0f, -2f);

            rule.offsetMax =
                Vector2.zero;

            Image sigil =
                MenuUiFactory.CreateImage(
                    "Skin_Sigil",
                    panel.transform,
                    warmFocus
                        ? MainMenuTheme.Warm
                        : MainMenuTheme.Silver);

            RectTransform sigilRect =
                sigil.rectTransform;

            sigilRect.anchorMin =
                sigilRect.anchorMax =
                    new Vector2(0.5f, 1f);

            sigilRect.pivot =
                new Vector2(0.5f, 0.5f);

            sigilRect.anchoredPosition =
                new Vector2(0f, -1f);

            sigilRect.sizeDelta =
                new Vector2(8f, 8f);

            sigilRect.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    45f);

            sigil.raycastTarget = false;
            topRule.raycastTarget = false;
        }

        public static void DecorateSection(
            Image panel)
        {
            if (panel == null)
                return;

            Outline outline =
                panel.GetComponent<Outline>();

            if (outline == null)
                outline =
                    panel.gameObject
                        .AddComponent<Outline>();

            outline.effectColor =
                new Color(
                    MainMenuTheme.SilverDim.r,
                    MainMenuTheme.SilverDim.g,
                    MainMenuTheme.SilverDim.b,
                    0.28f);

            outline.effectDistance =
                new Vector2(1f, -1f);

            Image accent =
                MenuUiFactory.CreateImage(
                    "Skin_SectionAccent",
                    panel.transform,
                    new Color(
                        MainMenuTheme.Silver.r,
                        MainMenuTheme.Silver.g,
                        MainMenuTheme.Silver.b,
                        0.26f));

            RectTransform rect =
                accent.rectTransform;

            rect.anchorMin =
                new Vector2(0f, 0.08f);

            rect.anchorMax =
                new Vector2(0f, 0.92f);

            rect.pivot =
                new Vector2(0f, 0.5f);

            rect.offsetMin =
                Vector2.zero;

            rect.offsetMax =
                new Vector2(2f, 0f);

            accent.raycastTarget = false;
        }

        public static void AddRarityAccent(
            Transform parent,
            ItemRarity rarity)
        {
            if (parent == null)
                return;

            Image accent =
                MenuUiFactory.CreateImage(
                    "RarityAccent",
                    parent,
                    GetRarityColor(rarity));

            RectTransform rect =
                accent.rectTransform;

            rect.anchorMin =
                new Vector2(0f, 0.12f);

            rect.anchorMax =
                new Vector2(0f, 0.88f);

            rect.pivot =
                new Vector2(0f, 0.5f);

            rect.offsetMin =
                new Vector2(0f, 0f);

            rect.offsetMax =
                new Vector2(3f, 0f);

            accent.raycastTarget = false;
        }

        public static Color GetRarityColor(
            ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Uncommon:
                    return new Color(
                        0.42f, 0.72f, 0.48f, 1f);

                case ItemRarity.Rare:
                    return new Color(
                        0.42f, 0.62f, 0.88f, 1f);

                case ItemRarity.Epic:
                    return new Color(
                        0.66f, 0.46f, 0.86f, 1f);

                case ItemRarity.Legendary:
                    return new Color(
                        0.95f, 0.56f, 0.18f, 1f);

                case ItemRarity.Unique:
                    return new Color(
                        0.78f, 0.84f, 0.90f, 1f);

                default:
                    return MainMenuTheme.Text;
            }
        }

        private static void AddCornerMarks(
            Transform parent,
            Color color,
            float length,
            float thickness)
        {
            AddCorner(
                parent,
                "TL",
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                color,
                length,
                thickness,
                true,
                false);

            AddCorner(
                parent,
                "TR",
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                color,
                length,
                thickness,
                false,
                false);

            AddCorner(
                parent,
                "BL",
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                color,
                length,
                thickness,
                true,
                true);

            AddCorner(
                parent,
                "BR",
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                color,
                length,
                thickness,
                false,
                true);
        }

        private static void AddCorner(
            Transform parent,
            string suffix,
            Vector2 anchor,
            Vector2 pivot,
            Color color,
            float length,
            float thickness,
            bool left,
            bool bottom)
        {
            Image horizontal =
                MenuUiFactory.CreateImage(
                    "Skin_CornerH_" + suffix,
                    parent,
                    color);

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
                h.localScale =
                    new Vector3(
                        -1f,
                        1f,
                        1f);

            Image vertical =
                MenuUiFactory.CreateImage(
                    "Skin_CornerV_" + suffix,
                    parent,
                    color);

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

            if (bottom)
                v.localScale =
                    new Vector3(
                        1f,
                        -1f,
                        1f);

            horizontal.raycastTarget = false;
            vertical.raycastTarget = false;
        }
    }
}
