#if UNITY_EDITOR
using System.Collections.Generic;
using KeeperFirstCovenant.Visual;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    public static class ProductionSheetWorldBuilder
    {
        public const string SheetPath =
            "Assets/KeeperFirstCovenant/Art/ProductionSheets/WorldKit_ProductionSheet.jpg";

        public const string LibraryPath =
            "Assets/KeeperFirstCovenant/Data/ProductionWorld/World_FromSheet.asset";

        public const string PrefabRoot =
            "Assets/KeeperFirstCovenant/Prefabs/ProductionWorld";

        private const float WorldPixelsPerUnit = 115f;

        private readonly struct PropSpec
        {
            public readonly string Id;
            public readonly SheetRect Rect;
            public readonly Vector2 WorldSize;
            public readonly bool Horizontal;
            public readonly bool Solid;
            public readonly Vector2 ColliderSize;

            public PropSpec(
                string id,
                SheetRect rect,
                Vector2 worldSize,
                bool horizontal,
                bool solid,
                Vector2 colliderSize)
            {
                Id = id;
                Rect = rect;
                WorldSize = worldSize;
                Horizontal = horizontal;
                Solid = solid;
                ColliderSize = colliderSize;
            }
        }

        [MenuItem("Keeper First Covenant/Production Art/Build World From Sheet")]
        public static void BuildWorld()
        {
            EnsureFolders();

            Texture2D sheet =
                ProductionSheetSpriteFactory.LoadSheet(SheetPath);

            WorldSpriteLibrary library =
                AssetDatabase.LoadAssetAtPath<WorldSpriteLibrary>(
                    LibraryPath);

            if (library == null)
            {
                library =
                    ScriptableObject.CreateInstance<WorldSpriteLibrary>();

                AssetDatabase.CreateAsset(
                    library,
                    LibraryPath);
            }

            ProductionSheetSpriteFactory.ClearSpriteSubAssets(library);

            Material cutout =
                ProductionSheetSpriteFactory.GetOrCreateSheetMaterial();

            List<WorldSpriteEntry> entries =
                new List<WorldSpriteEntry>();

            foreach (PropSpec spec in Specs())
            {
                Sprite sprite =
                    ProductionSheetSpriteFactory.CreatePersistentSprite(
                        sheet,
                        spec.Rect,
                        "World_" + spec.Id,
                        library,
                        WorldPixelsPerUnit,
                        spec.Horizontal
                            ? new Vector2(0.5f, 0.5f)
                            : new Vector2(0.5f, 0.04f));

                WorldSpriteEntry entry =
                    new WorldSpriteEntry
                    {
                        id = spec.Id,
                        sprite = sprite,
                        worldSize = spec.WorldSize,
                        horizontal = spec.Horizontal,
                        solid = spec.Solid,
                        colliderSize = spec.ColliderSize
                    };

                entries.Add(entry);

                BuildPrefab(
                    entry,
                    cutout);
            }

            library.entries = entries.ToArray();
            EditorUtility.SetDirty(library);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Production world kit built from approved sheet: " +
                entries.Count + " independent prefabs.");
        }

        public static GameObject LoadPrefab(string id)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(
                PrefabRoot + "/" + id + ".prefab");
        }

        private static IEnumerable<PropSpec> Specs()
        {
            yield return P(
                "StoneFloor_A",
                310, 45, 90, 105,
                1.8f, 1.25f,
                true, false, 0f, 0f);

            yield return P(
                "StoneFloor_B",
                420, 45, 105, 105,
                1.9f, 1.3f,
                true, false, 0f, 0f);

            yield return P(
                "RuneFloor",
                510, 48, 82, 102,
                1.7f, 1.2f,
                true, false, 0f, 0f);

            yield return P(
                "RoadStraight",
                605, 45, 120, 110,
                2.25f, 1.3f,
                true, false, 0f, 0f);

            yield return P(
                "RoadWide",
                730, 42, 105, 115,
                2.1f, 1.35f,
                true, false, 0f, 0f);

            yield return P(
                "BrokenWall",
                850, 43, 105, 120,
                1.55f, 1.7f,
                false, true, 1.1f, 0.35f);

            yield return P(
                "WallCorner",
                965, 43, 110, 120,
                1.6f, 1.7f,
                false, true, 1.0f, 0.4f);

            yield return P(
                "RuinedArch",
                1110, 35, 105, 132,
                1.8f, 2.25f,
                false, true, 1.2f, 0.45f);

            yield return P(
                "RuinedArchWide",
                1220, 35, 120, 132,
                2.0f, 2.25f,
                false, true, 1.35f, 0.45f);

            yield return P(
                "Pillar",
                1430, 35, 63, 132,
                0.8f, 2.15f,
                false, true, 0.45f, 0.45f);

            yield return P(
                "BrokenPillar",
                1560, 55, 62, 110,
                0.75f, 1.6f,
                false, true, 0.4f, 0.4f);

            yield return P(
                "StoneStairs",
                25, 185, 110, 125,
                1.6f, 1.5f,
                false, true, 1.0f, 0.55f);

            yield return P(
                "ShrineAltar",
                345, 190, 235, 120,
                2.5f, 1.65f,
                false, true, 1.5f, 0.8f);

            yield return P(
                "RuneStone",
                620, 188, 75, 125,
                0.85f, 1.65f,
                false, true, 0.45f, 0.4f);

            yield return P(
                "RuneStoneMossy",
                700, 188, 90, 125,
                0.95f, 1.65f,
                false, true, 0.5f, 0.45f);

            yield return P(
                "CovenantCircle",
                850, 198, 190, 112,
                2.5f, 1.45f,
                true, false, 0f, 0f);

            yield return P(
                "CovenantCircleBlue",
                1100, 198, 190, 112,
                2.5f, 1.45f,
                true, false, 0f, 0f);

            yield return P(
                "Puddle",
                25, 335, 110, 82,
                1.3f, 0.8f,
                true, false, 0f, 0f);

            yield return P(
                "Rock",
                270, 335, 70, 88,
                0.8f, 0.85f,
                false, true, 0.45f, 0.4f);

            yield return P(
                "Grass",
                515, 342, 75, 75,
                0.8f, 0.75f,
                false, false, 0f, 0f);

            yield return P(
                "Flowers",
                760, 342, 90, 75,
                0.95f, 0.75f,
                false, false, 0f, 0f);

            yield return P(
                "OldTree",
                15, 442, 160, 122,
                2.1f, 2.25f,
                false, true, 0.75f, 0.65f);

            yield return P(
                "DeadTree",
                280, 442, 150, 122,
                1.9f, 2.2f,
                false, true, 0.7f, 0.6f);

            yield return P(
                "BrazierOrange",
                500, 448, 65, 115,
                0.75f, 1.1f,
                false, false, 0f, 0f);

            yield return P(
                "BrazierBlue",
                620, 448, 65, 115,
                0.75f, 1.1f,
                false, false, 0f, 0f);

            yield return P(
                "Lantern",
                770, 448, 68, 115,
                0.75f, 1.25f,
                false, false, 0f, 0f);

            yield return P(
                "Banner",
                25, 578, 74, 102,
                0.75f, 1.55f,
                false, false, 0f, 0f);

            yield return P(
                "Campfire",
                455, 582, 155, 96,
                1.35f, 0.85f,
                false, false, 0f, 0f);

            yield return P(
                "Crate",
                645, 582, 75, 95,
                0.8f, 0.8f,
                false, true, 0.55f, 0.55f);

            yield return P(
                "Barrel",
                805, 580, 64, 98,
                0.7f, 0.9f,
                false, true, 0.45f, 0.45f);

            yield return P(
                "Bench",
                20, 694, 115, 88,
                1.25f, 0.65f,
                false, true, 0.95f, 0.35f);

            yield return P(
                "Wagon",
                245, 693, 180, 95,
                1.85f, 1.05f,
                false, true, 1.35f, 0.6f);

            yield return P(
                "Tent",
                445, 695, 175, 92,
                1.75f, 1.15f,
                false, true, 1.2f, 0.75f);

            yield return P(
                "Fence",
                645, 698, 165, 88,
                1.8f, 0.85f,
                false, true, 1.4f, 0.25f);

            yield return P(
                "KeeperStatue",
                20, 798, 125, 128,
                1.3f, 1.9f,
                false, true, 0.65f, 0.55f);

            yield return P(
                "CrystalPurple",
                175, 802, 95, 122,
                0.9f, 1.35f,
                false, true, 0.4f, 0.4f);

            yield return P(
                "SmallShrine",
                390, 805, 100, 118,
                0.95f, 1.1f,
                false, true, 0.5f, 0.45f);
        }

        private static PropSpec P(
            string id,
            float x,
            float y,
            float width,
            float height,
            float worldWidth,
            float worldHeight,
            bool horizontal,
            bool solid,
            float colliderWidth,
            float colliderDepth)
        {
            return new PropSpec(
                id,
                new SheetRect(x, y, width, height),
                new Vector2(worldWidth, worldHeight),
                horizontal,
                solid,
                new Vector2(colliderWidth, colliderDepth));
        }

        private static void BuildPrefab(
            WorldSpriteEntry entry,
            Material cutout)
        {
            GameObject root =
                new GameObject(entry.id);

            GameObject visual =
                new GameObject("Visual");

            visual.transform.SetParent(
                root.transform,
                false);

            visual.AddComponent<BillboardCharacter2D>();

            SpriteRenderer renderer =
                visual.AddComponent<SpriteRenderer>();

            renderer.sprite = entry.sprite;
            renderer.sharedMaterial = cutout;
            renderer.sortingOrder =
                entry.horizontal ? -40 : 0;

            float sourceWidth =
                entry.sprite != null
                    ? entry.sprite.bounds.size.x
                    : 1f;

            float sourceHeight =
                entry.sprite != null
                    ? entry.sprite.bounds.size.y
                    : 1f;

            visual.transform.localScale =
                new Vector3(
                    entry.worldSize.x / Mathf.Max(0.01f, sourceWidth),
                    entry.worldSize.y / Mathf.Max(0.01f, sourceHeight),
                    1f);

            if (!entry.horizontal)
            {
                visual.transform.localPosition =
                    new Vector3(
                        0f,
                        entry.worldSize.y * 0.5f,
                        0f);
            }
            else
            {
                visual.transform.localPosition =
                    new Vector3(
                        0f,
                        0.015f,
                        0f);
            }

            if (entry.solid)
            {
                BoxCollider collider =
                    root.AddComponent<BoxCollider>();

                collider.size =
                    new Vector3(
                        Mathf.Max(0.1f, entry.colliderSize.x),
                        Mathf.Max(0.25f, entry.worldSize.y),
                        Mathf.Max(0.1f, entry.colliderSize.y));

                collider.center =
                    new Vector3(
                        0f,
                        entry.worldSize.y * 0.5f,
                        0f);
            }

            PrefabUtility.SaveAsPrefabAsset(
                root,
                PrefabRoot + "/" + entry.id + ".prefab");

            UnityEngine.Object.DestroyImmediate(root);
        }

        private static void EnsureFolders()
        {
            ProductionSheetSpriteFactory.EnsureFolder(
                "Assets/KeeperFirstCovenant",
                "Data");

            ProductionSheetSpriteFactory.EnsureFolder(
                "Assets/KeeperFirstCovenant/Data",
                "ProductionWorld");

            ProductionSheetSpriteFactory.EnsureFolder(
                "Assets/KeeperFirstCovenant",
                "Prefabs");

            ProductionSheetSpriteFactory.EnsureFolder(
                "Assets/KeeperFirstCovenant/Prefabs",
                "ProductionWorld");
        }
    }
}
#endif
