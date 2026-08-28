#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using KeeperFirstCovenant.Visual;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    public static class FirstCovenant2DWorldBuilder
    {
        public const string Root = Edward2DProductionBuilder.Root;
        public const string SpriteRoot = Root + "/Sprites/World";
        public const string PrefabRoot = Root + "/Prefabs/World";

        private static readonly Color Outline = new Color(0.045f, 0.055f, 0.065f, 1f);
        private static readonly Color Stone = new Color(0.24f, 0.27f, 0.28f, 1f);
        private static readonly Color StoneLight = new Color(0.37f, 0.40f, 0.39f, 1f);
        private static readonly Color StoneDark = new Color(0.12f, 0.15f, 0.16f, 1f);
        private static readonly Color Moss = new Color(0.17f, 0.24f, 0.16f, 1f);
        private static readonly Color MossLight = new Color(0.31f, 0.38f, 0.22f, 1f);
        private static readonly Color Bark = new Color(0.13f, 0.10f, 0.08f, 1f);
        private static readonly Color Leaves = new Color(0.09f, 0.17f, 0.13f, 1f);
        private static readonly Color LeavesLight = new Color(0.16f, 0.28f, 0.20f, 1f);
        private static readonly Color Ancient = new Color(0.64f, 0.79f, 0.82f, 1f);
        private static readonly Color AncientCore = new Color(0.88f, 0.98f, 1f, 1f);
        private static readonly Color Fire = new Color(1f, 0.27f, 0.045f, 1f);
        private static readonly Color FireCore = new Color(1f, 0.78f, 0.18f, 1f);

        public struct PropInfo
        {
            public string id;
            public string prefabPath;
            public bool horizontal;

            public PropInfo(string id, string prefabPath, bool horizontal)
            {
                this.id = id;
                this.prefabPath = prefabPath;
                this.horizontal = horizontal;
            }
        }

        [MenuItem("Keeper First Covenant/2D Production/Rebuild World Kit")]
        public static void BuildWorldKit()
        {
            EnsureFolders();

            List<PropInfo> props = new List<PropInfo>
            {
                BuildProp("AncientStoneTile", true, 1.9f, false, DrawStoneTile),
                BuildProp("BrokenWall", false, 1.65f, true, DrawBrokenWall),
                BuildProp("BrokenPillar", false, 1.65f, true, DrawBrokenPillar),
                BuildProp("AncientShrine", false, 1.7f, true, DrawShrine),
                BuildProp("RuneStone", false, 1.5f, true, DrawRuneStone),
                BuildProp("OldTree", false, 2.15f, true, DrawTree),
                BuildProp("GrassClump", false, 0.75f, false, DrawGrass),
                BuildProp("Brazier", false, 1.0f, false, DrawBrazier),
                BuildProp("StoneStairs", true, 1.9f, false, DrawStairs),
                BuildProp("Puddle", true, 1.25f, false, DrawPuddle),
                BuildProp("CovenantRuneCircle", true, 1.55f, false, DrawRuneCircle),
                BuildProp("CovenantBanner", false, 1.55f, false, DrawBanner),
                BuildProp("RockCluster", false, 0.95f, true, DrawRocks),
                BuildProp("WoodenCrate", false, 0.75f, true, DrawCrate),
                BuildProp("RoadSign", false, 1.0f, false, DrawRoadSign),
                BuildProp("Campfire", false, 0.72f, false, DrawCampfire)
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Keeper 2D world kit rebuilt: " + props.Count + " reusable props.");
        }

        public static PropInfo[] EnsureAndGetProps()
        {
            EnsureFolders();

            string[] ids =
            {
                "AncientStoneTile", "BrokenWall", "BrokenPillar", "AncientShrine",
                "RuneStone", "OldTree", "GrassClump", "Brazier", "StoneStairs",
                "Puddle", "CovenantRuneCircle", "CovenantBanner", "RockCluster",
                "WoodenCrate", "RoadSign", "Campfire"
            };

            foreach (string id in ids)
            {
                string path = PrefabRoot + "/" + id + ".prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
                {
                    BuildWorldKit();
                    break;
                }
            }

            return new[]
            {
                Info("AncientStoneTile", true), Info("BrokenWall", false),
                Info("BrokenPillar", false), Info("AncientShrine", false),
                Info("RuneStone", false), Info("OldTree", false),
                Info("GrassClump", false), Info("Brazier", false),
                Info("StoneStairs", true), Info("Puddle", true),
                Info("CovenantRuneCircle", true), Info("CovenantBanner", false),
                Info("RockCluster", false), Info("WoodenCrate", false),
                Info("RoadSign", false), Info("Campfire", false)
            };
        }

        private static PropInfo Info(string id, bool horizontal)
        {
            return new PropInfo(id, PrefabRoot + "/" + id + ".prefab", horizontal);
        }

        private static PropInfo BuildProp(
            string id,
            bool horizontal,
            float scale,
            bool solid,
            Action<CelSpritePainter> draw)
        {
            CelSpritePainter canvas = new CelSpritePainter(256, 256);
            draw(canvas);
            string spritePath = SpriteRoot + "/" + id + ".png";
            Sprite sprite = canvas.SaveAsSprite(spritePath, 128f, new Vector2(0.5f, 0.5f));

            GameObject root = new GameObject(id);
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);

            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = horizontal ? -20 : 0;

            if (horizontal)
            {
                visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                visual.transform.localScale = Vector3.one * scale;
                visual.transform.localPosition = new Vector3(0f, 0.006f, 0f);
            }
            else
            {
                visual.transform.localScale = Vector3.one * scale;
                visual.transform.localPosition = new Vector3(0f, scale * 0.96f, 0f);
                visual.AddComponent<BillboardCharacter2D>();
            }

            if (solid)
            {
                BoxCollider collider = root.AddComponent<BoxCollider>();
                collider.center = new Vector3(0f, scale * 0.88f, 0f);
                collider.size = new Vector3(
                    id == "BrokenWall" ? scale * 1.5f : scale * 0.72f,
                    scale * 1.55f,
                    0.38f);
            }

            string prefabPath = PrefabRoot + "/" + id + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);

            return new PropInfo(id, prefabPath, horizontal);
        }

        private static void DrawStoneTile(CelSpritePainter c)
        {
            c.Rect(16, 16, 224, 224, StoneDark);
            c.Rect(21, 21, 214, 214, Stone);
            for (int x = 22; x < 236; x += 53)
                c.Line(new Vector2(x, 22), new Vector2(x + 3, 234), 2f, StoneDark);
            for (int y = 22; y < 236; y += 53)
                c.Line(new Vector2(22, y), new Vector2(234, y + 2), 2f, StoneDark);

            c.Line(new Vector2(37, 210), new Vector2(77, 174), 2.5f, StoneLight);
            c.Line(new Vector2(77, 174), new Vector2(101, 183), 2f, StoneLight);
            c.Line(new Vector2(167, 66), new Vector2(198, 92), 2f, StoneDark);
            c.Speckles(71, new Rect(22, 22, 212, 212), 65, StoneLight, 0.5f, 1.4f);
            c.Speckles(19, new Rect(22, 22, 212, 212), 45, Moss, 0.8f, 2.3f);
        }

        private static void DrawBrokenWall(CelSpritePainter c)
        {
            c.Polygon(Outline,
                new Vector2(22, 45), new Vector2(22, 176), new Vector2(49, 199),
                new Vector2(81, 184), new Vector2(109, 214), new Vector2(142, 189),
                new Vector2(178, 210), new Vector2(232, 176), new Vector2(232, 45));
            c.Polygon(Stone,
                new Vector2(28, 50), new Vector2(28, 171), new Vector2(51, 191),
                new Vector2(81, 176), new Vector2(110, 205), new Vector2(142, 180),
                new Vector2(179, 201), new Vector2(226, 171), new Vector2(226, 50));

            for (int y = 66; y <= 164; y += 33)
                c.Line(new Vector2(31, y), new Vector2(223, y + 4), 3f, StoneDark);
            for (int x = 54; x <= 210; x += 50)
                c.Line(new Vector2(x, 52), new Vector2(x + 5, 169), 2f, StoneDark);

            c.Polygon(Moss,
                new Vector2(31, 165), new Vector2(65, 169), new Vector2(84, 158),
                new Vector2(110, 170), new Vector2(139, 159), new Vector2(167, 173),
                new Vector2(190, 162), new Vector2(222, 169), new Vector2(222, 184),
                new Vector2(31, 184));
            c.Line(new Vector2(42, 152), new Vector2(73, 117), 2f, MossLight);
        }

        private static void DrawBrokenPillar(CelSpritePainter c)
        {
            c.Polygon(Outline,
                new Vector2(65, 31), new Vector2(188, 31), new Vector2(178, 58),
                new Vector2(165, 66), new Vector2(170, 194), new Vector2(145, 226),
                new Vector2(96, 217), new Vector2(82, 190), new Vector2(88, 65));
            c.Polygon(Stone,
                new Vector2(72, 37), new Vector2(181, 37), new Vector2(171, 53),
                new Vector2(158, 62), new Vector2(163, 188), new Vector2(141, 216),
                new Vector2(102, 208), new Vector2(89, 185), new Vector2(95, 61));
            c.Polygon(StoneLight,
                new Vector2(97, 64), new Vector2(115, 61), new Vector2(112, 194),
                new Vector2(103, 205), new Vector2(94, 183));
            c.Line(new Vector2(93, 92), new Vector2(159, 84), 3f, StoneDark);
            c.Line(new Vector2(95, 155), new Vector2(161, 145), 3f, StoneDark);
            c.Polygon(Moss,
                new Vector2(91, 177), new Vector2(113, 168), new Vector2(136, 178),
                new Vector2(162, 165), new Vector2(161, 189), new Vector2(139, 213),
                new Vector2(105, 207));
        }

        private static void DrawShrine(CelSpritePainter c)
        {
            c.Polygon(Outline,
                new Vector2(50, 31), new Vector2(205, 31), new Vector2(205, 69),
                new Vector2(185, 69), new Vector2(185, 186), new Vector2(159, 223),
                new Vector2(96, 223), new Vector2(70, 186), new Vector2(70, 69),
                new Vector2(50, 69));
            c.Polygon(StoneDark,
                new Vector2(57, 38), new Vector2(198, 38), new Vector2(198, 62),
                new Vector2(178, 62), new Vector2(178, 181), new Vector2(154, 216),
                new Vector2(101, 216), new Vector2(77, 181), new Vector2(77, 62),
                new Vector2(57, 62));
            c.Polygon(Stone,
                new Vector2(86, 62), new Vector2(169, 62), new Vector2(169, 180),
                new Vector2(149, 206), new Vector2(106, 206), new Vector2(86, 180));

            c.Ring(new Vector2(127, 141), new Vector2(36, 36), 4f, Ancient);
            c.Ring(new Vector2(127, 141), new Vector2(20, 20), 2.5f, AncientCore);
            c.GlowLine(new Vector2(127, 104), new Vector2(127, 177), AncientCore, Ancient);
            c.GlowLine(new Vector2(96, 141), new Vector2(158, 141), AncientCore, Ancient);
            c.GlowLine(new Vector2(105, 119), new Vector2(149, 163), AncientCore, Ancient);
            c.GlowLine(new Vector2(149, 119), new Vector2(105, 163), AncientCore, Ancient);
            c.Speckles(43, new Rect(80, 55, 96, 155), 24, Moss, 1f, 2.4f);
        }

        private static void DrawRuneStone(CelSpritePainter c)
        {
            c.Polygon(Outline,
                new Vector2(81, 28), new Vector2(164, 28), new Vector2(181, 71),
                new Vector2(170, 205), new Vector2(141, 231), new Vector2(92, 219),
                new Vector2(75, 190), new Vector2(76, 69));
            c.Polygon(StoneDark,
                new Vector2(87, 35), new Vector2(158, 35), new Vector2(174, 74),
                new Vector2(163, 199), new Vector2(137, 223), new Vector2(97, 212),
                new Vector2(82, 186), new Vector2(83, 73));
            c.GlowLine(new Vector2(126, 67), new Vector2(126, 190), AncientCore, Ancient);
            c.GlowLine(new Vector2(105, 101), new Vector2(146, 86), AncientCore, Ancient);
            c.GlowLine(new Vector2(105, 145), new Vector2(147, 162), AncientCore, Ancient);
            c.Ring(new Vector2(126, 125), new Vector2(24, 24), 3f, AncientCore);
        }

        private static void DrawTree(CelSpritePainter c)
        {
            c.Polygon(Outline,
                new Vector2(103, 23), new Vector2(147, 23), new Vector2(147, 103),
                new Vector2(166, 140), new Vector2(149, 153), new Vector2(141, 224),
                new Vector2(112, 225), new Vector2(105, 155), new Vector2(83, 137),
                new Vector2(103, 105));
            c.Polygon(Bark,
                new Vector2(110, 28), new Vector2(140, 28), new Vector2(140, 106),
                new Vector2(156, 137), new Vector2(143, 145), new Vector2(135, 216),
                new Vector2(118, 217), new Vector2(112, 149), new Vector2(93, 135),
                new Vector2(110, 102));
            c.Line(new Vector2(119, 38), new Vector2(129, 206), 3f, new Color(0.25f, 0.16f, 0.10f));
            c.Line(new Vector2(136, 62), new Vector2(118, 125), 2f, new Color(0.25f, 0.16f, 0.10f));

            c.Ellipse(new Vector2(77, 181), new Vector2(60, 42), Outline);
            c.Ellipse(new Vector2(123, 205), new Vector2(71, 45), Outline);
            c.Ellipse(new Vector2(178, 177), new Vector2(55, 40), Outline);
            c.Ellipse(new Vector2(78, 183), new Vector2(53, 35), Leaves);
            c.Ellipse(new Vector2(123, 205), new Vector2(64, 38), Leaves);
            c.Ellipse(new Vector2(178, 179), new Vector2(48, 33), Leaves);
            c.Ellipse(new Vector2(100, 217), new Vector2(30, 16), LeavesLight);
            c.Ellipse(new Vector2(161, 190), new Vector2(25, 14), LeavesLight);
        }

        private static void DrawGrass(CelSpritePainter c)
        {
            for (int i = 0; i < 15; i++)
            {
                float x = 60f + i * 10f;
                float top = 92f + (i % 4) * 18f;
                c.Line(new Vector2(128, 42), new Vector2(x, top), 5f, Outline);
                c.Line(new Vector2(128, 45), new Vector2(x, top - 3f), 2.4f,
                    i % 3 == 0 ? MossLight : Moss);
            }
        }

        private static void DrawBrazier(CelSpritePainter c)
        {
            c.Polygon(Outline,
                new Vector2(75, 51), new Vector2(181, 51), new Vector2(166, 105),
                new Vector2(142, 119), new Vector2(142, 178), new Vector2(163, 196),
                new Vector2(163, 209), new Vector2(93, 209), new Vector2(93, 196),
                new Vector2(114, 178), new Vector2(114, 119), new Vector2(90, 105));
            c.Polygon(StoneDark,
                new Vector2(84, 58), new Vector2(172, 58), new Vector2(158, 99),
                new Vector2(135, 111), new Vector2(135, 183), new Vector2(151, 197),
                new Vector2(105, 197), new Vector2(121, 183), new Vector2(121, 111),
                new Vector2(98, 99));

            c.Polygon(Fire,
                new Vector2(96, 117), new Vector2(105, 148), new Vector2(121, 129),
                new Vector2(127, 179), new Vector2(141, 143), new Vector2(154, 163),
                new Vector2(161, 118), new Vector2(144, 92), new Vector2(128, 109),
                new Vector2(112, 88));
            c.Polygon(FireCore,
                new Vector2(114, 119), new Vector2(124, 148), new Vector2(132, 125),
                new Vector2(143, 139), new Vector2(145, 112), new Vector2(132, 101));
        }

        private static void DrawStairs(CelSpritePainter c)
        {
            c.Polygon(Outline,
                new Vector2(30, 40), new Vector2(226, 40), new Vector2(210, 216),
                new Vector2(46, 216));
            c.Polygon(Stone,
                new Vector2(37, 47), new Vector2(219, 47), new Vector2(204, 209),
                new Vector2(52, 209));
            for (int i = 0; i < 6; i++)
            {
                float y = 60 + i * 25f;
                float inset = i * 6f;
                c.Line(new Vector2(43 + inset, y), new Vector2(213 - inset, y), 6f, StoneDark);
                c.Line(new Vector2(48 + inset, y + 5f), new Vector2(208 - inset, y + 5f), 2f, StoneLight);
            }
            c.Speckles(58, new Rect(50, 55, 156, 145), 34, Moss, 0.8f, 2.2f);
        }

        private static void DrawPuddle(CelSpritePainter c)
        {
            Color water = new Color(0.10f, 0.21f, 0.26f, 0.72f);
            Color shine = new Color(0.47f, 0.69f, 0.75f, 0.60f);
            c.Ellipse(new Vector2(128, 126), new Vector2(91, 52), new Color(0.025f, 0.055f, 0.065f, 0.55f));
            c.Ellipse(new Vector2(128, 128), new Vector2(84, 45), water);
            c.Line(new Vector2(67, 137), new Vector2(119, 151), 2f, shine);
            c.Line(new Vector2(137, 111), new Vector2(188, 125), 2f, shine);
        }

        private static void DrawRuneCircle(CelSpritePainter c)
        {
            Color soft = new Color(0.42f, 0.68f, 0.74f, 0.34f);
            c.Ring(new Vector2(128, 128), new Vector2(94, 94), 5f, soft);
            c.Ring(new Vector2(128, 128), new Vector2(71, 71), 3f, Ancient);
            c.Ring(new Vector2(128, 128), new Vector2(32, 32), 3f, AncientCore);
            c.GlowLine(new Vector2(128, 34), new Vector2(128, 222), AncientCore, Ancient);
            c.GlowLine(new Vector2(34, 128), new Vector2(222, 128), AncientCore, Ancient);
            c.GlowLine(new Vector2(61, 61), new Vector2(195, 195), AncientCore, Ancient);
            c.GlowLine(new Vector2(195, 61), new Vector2(61, 195), AncientCore, Ancient);
            c.Polygon(Ancient,
                new Vector2(128, 48), new Vector2(143, 80), new Vector2(176, 83),
                new Vector2(153, 107), new Vector2(161, 139), new Vector2(128, 121),
                new Vector2(95, 139), new Vector2(103, 107), new Vector2(80, 83),
                new Vector2(113, 80));
        }

        private static void DrawCrate(CelSpritePainter c)
        {
            Color wood = new Color(0.22f, 0.14f, 0.085f, 1f);
            Color woodLight = new Color(0.38f, 0.24f, 0.13f, 1f);
            c.Polygon(Outline,
                new Vector2(52, 48), new Vector2(204, 48), new Vector2(218, 190),
                new Vector2(128, 222), new Vector2(38, 190));
            c.Polygon(wood,
                new Vector2(59, 55), new Vector2(197, 55), new Vector2(210, 184),
                new Vector2(128, 213), new Vector2(46, 184));
            c.Line(new Vector2(64, 70), new Vector2(193, 190), 10f, Outline);
            c.Line(new Vector2(192, 70), new Vector2(63, 190), 10f, Outline);
            c.Line(new Vector2(65, 72), new Vector2(190, 187), 4f, woodLight);
            c.Line(new Vector2(190, 72), new Vector2(66, 187), 4f, woodLight);
            c.Rect(51, 113, 155, 11, Outline);
            c.Rect(55, 116, 147, 5, Metal);
        }

        private static void DrawRoadSign(CelSpritePainter c)
        {
            Color wood = new Color(0.20f, 0.13f, 0.08f, 1f);
            c.Rect(116, 28, 25, 195, Outline);
            c.Rect(122, 33, 13, 186, wood);
            c.Polygon(Outline,
                new Vector2(39, 159), new Vector2(184, 159), new Vector2(218, 187),
                new Vector2(184, 215), new Vector2(39, 215));
            c.Polygon(wood,
                new Vector2(47, 166), new Vector2(181, 166), new Vector2(207, 187),
                new Vector2(181, 208), new Vector2(47, 208));
            c.Line(new Vector2(67, 185), new Vector2(164, 185), 3f, new Color(0.46f, 0.31f, 0.17f));
            c.Line(new Vector2(67, 194), new Vector2(145, 194), 2f, new Color(0.46f, 0.31f, 0.17f));
            c.Speckles(77, new Rect(51, 170, 128, 31), 12, Moss, 0.8f, 2f);
        }

        private static void DrawCampfire(CelSpritePainter c)
        {
            c.Line(new Vector2(67, 59), new Vector2(186, 93), 16f, Outline);
            c.Line(new Vector2(72, 62), new Vector2(181, 91), 9f, Bark);
            c.Line(new Vector2(184, 59), new Vector2(70, 96), 16f, Outline);
            c.Line(new Vector2(179, 63), new Vector2(75, 93), 9f, Bark);
            c.Polygon(Fire,
                new Vector2(89, 86), new Vector2(102, 128), new Vector2(116, 111),
                new Vector2(126, 180), new Vector2(142, 137), new Vector2(158, 160),
                new Vector2(166, 103), new Vector2(145, 78), new Vector2(127, 101),
                new Vector2(108, 76));
            c.Polygon(FireCore,
                new Vector2(111, 95), new Vector2(121, 132), new Vector2(130, 109),
                new Vector2(145, 128), new Vector2(148, 100), new Vector2(132, 86));
        }

        private static void DrawBanner(CelSpritePainter c)
        {
            c.Rect(119, 29, 12, 203, Outline);
            c.Rect(123, 32, 5, 198, StoneLight);
            c.Polygon(Outline,
                new Vector2(68, 108), new Vector2(188, 108), new Vector2(181, 212),
                new Vector2(150, 193), new Vector2(128, 218), new Vector2(105, 193),
                new Vector2(75, 212));
            c.Polygon(new Color(0.075f, 0.085f, 0.10f, 1f),
                new Vector2(74, 114), new Vector2(182, 114), new Vector2(176, 200),
                new Vector2(149, 183), new Vector2(128, 207), new Vector2(106, 183),
                new Vector2(81, 201));
            c.Ring(new Vector2(128, 155), new Vector2(26, 26), 3f, Ancient);
            c.GlowLine(new Vector2(128, 126), new Vector2(128, 184), AncientCore, Ancient);
            c.GlowLine(new Vector2(103, 155), new Vector2(153, 155), AncientCore, Ancient);
        }

        private static void DrawRocks(CelSpritePainter c)
        {
            c.Polygon(Outline,
                new Vector2(35, 49), new Vector2(85, 38), new Vector2(113, 78),
                new Vector2(144, 57), new Vector2(203, 75), new Vector2(225, 125),
                new Vector2(194, 180), new Vector2(138, 189), new Vector2(91, 175),
                new Vector2(43, 142));
            c.Polygon(StoneDark,
                new Vector2(43, 57), new Vector2(81, 47), new Vector2(111, 89),
                new Vector2(145, 66), new Vector2(195, 82), new Vector2(216, 125),
                new Vector2(187, 171), new Vector2(139, 180), new Vector2(95, 166),
                new Vector2(52, 135));
            c.Polygon(Stone,
                new Vector2(54, 72), new Vector2(82, 58), new Vector2(104, 96),
                new Vector2(85, 134), new Vector2(52, 128));
            c.Polygon(StoneLight,
                new Vector2(148, 76), new Vector2(188, 89), new Vector2(203, 122),
                new Vector2(178, 119), new Vector2(159, 97));
            c.Speckles(28, new Rect(45, 90, 165, 88), 28, Moss, 1f, 2.6f);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "KeeperFirstCovenant");
            EnsureFolder("Assets/KeeperFirstCovenant", "Generated2D");
            EnsureFolder(Root, "Sprites");
            EnsureFolder(Root + "/Sprites", "World");
            EnsureFolder(Root, "Prefabs");
            EnsureFolder(Root + "/Prefabs", "World");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
#endif
