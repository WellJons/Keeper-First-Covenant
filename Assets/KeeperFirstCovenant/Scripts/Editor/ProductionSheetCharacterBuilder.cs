#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using KeeperFirstCovenant.Characters;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Visual;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    public static class ProductionSheetCharacterBuilder
    {
        public const string SheetRoot =
            "Assets/KeeperFirstCovenant/Art/ProductionSheets";

        public const string DataRoot =
            "Assets/KeeperFirstCovenant/Data/ProductionCharacters";

        public const string PrefabRoot =
            "Assets/KeeperFirstCovenant/Prefabs/ProductionCharacters";

        public const string EdwardLibraryPath =
            DataRoot + "/Edward_FromSheet.asset";

        public const string EleanorLibraryPath =
            DataRoot + "/Eleanor_FromSheet.asset";

        public const string AelisLibraryPath =
            DataRoot + "/Aelis_FromSheet.asset";

        public const string WhiteLibraryPath =
            DataRoot + "/White_FromSheet.asset";

        public const string EdwardPrefabPath =
            PrefabRoot + "/Edward_FromSheet.prefab";

        public const string EleanorPrefabPath =
            PrefabRoot + "/Eleanor_FromSheet.prefab";

        public const string AelisPrefabPath =
            PrefabRoot + "/Aelis_FromSheet.prefab";

        public const string WhitePrefabPath =
            PrefabRoot + "/White_FromSheet.prefab";

        private const float CharacterPixelsPerUnit = 150f;

        private readonly struct FacingSprites
        {
            public readonly Sprite N;
            public readonly Sprite NE;
            public readonly Sprite E;
            public readonly Sprite SE;
            public readonly Sprite S;

            public FacingSprites(
                Sprite n,
                Sprite ne,
                Sprite e,
                Sprite se,
                Sprite s)
            {
                N = n;
                NE = ne;
                E = e;
                SE = se;
                S = s;
            }
        }

        [MenuItem("Keeper First Covenant/Production Art/Build Characters From Sheets")]
        public static void BuildAllCharacters()
        {
            EnsureFolders();

            FrameAnimationLibrary edward =
                BuildEdwardLibrary();

            FrameAnimationLibrary eleanor =
                BuildEleanorLibrary();

            FrameAnimationLibrary aelis =
                BuildAelisLibrary();

            FrameAnimationLibrary white =
                BuildWhiteLibrary();

            BuildCharacterPrefab(
                "Edward_FromSheet",
                edward,
                GetOrCreateDefinition(
                    "Edward",
                    CombatFaction.Player,
                    428,
                    68,
                    DataRoot + "/Edward_Character.asset"),
                EdwardPrefabPath,
                true,
                1.85f,
                0.28f);

            BuildCharacterPrefab(
                "Eleanor_FromSheet",
                eleanor,
                GetOrCreateDefinition(
                    "Eleanor",
                    CombatFaction.Ally,
                    372,
                    122,
                    DataRoot + "/Eleanor_Character.asset"),
                EleanorPrefabPath,
                false,
                1.80f,
                0.27f);

            BuildCharacterPrefab(
                "Aelis_FromSheet",
                aelis,
                GetOrCreateDefinition(
                    "Aelis",
                    CombatFaction.Ally,
                    360,
                    118,
                    DataRoot + "/Aelis_Character.asset"),
                AelisPrefabPath,
                false,
                1.76f,
                0.26f);

            BuildCharacterPrefab(
                "White_FromSheet",
                white,
                GetOrCreateDefinition(
                    "White",
                    CombatFaction.Ally,
                    160,
                    0,
                    DataRoot + "/White_Character.asset"),
                WhitePrefabPath,
                false,
                0.92f,
                0.22f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Production-sheet characters built: Edward, Eleanor, Aelis and White.");
        }

        private static FrameAnimationLibrary BuildEdwardLibrary()
        {
            Texture2D sheet =
                ProductionSheetSpriteFactory.LoadSheet(
                    SheetRoot + "/Edward_ProductionSheet.jpg");

            FrameAnimationLibrary library =
                GetOrCreateLibrary(
                    EdwardLibraryPath,
                    "edward_sheet",
                    "Edward — approved production sheet");

            ProductionSheetSpriteFactory.ClearSpriteSubAssets(library);

            FacingSprites facing = BuildFacingSprites(
                sheet,
                library,
                "Edward",
                new SheetRect(308, 62, 115, 230),
                new SheetRect(454, 62, 125, 230),
                new SheetRect(606, 62, 130, 230),
                new SheetRect(754, 62, 135, 230),
                new SheetRect(905, 62, 135, 230));

            Sprite[] idle = Row(
                sheet, library, "Edward_Idle",
                ProductionSheetSpriteFactory.EqualRow(
                    300, 366, 520, 104, 7, 3));

            Sprite[] walk = Row(
                sheet, library, "Edward_Walk",
                ProductionSheetSpriteFactory.EqualRow(
                    300, 493, 520, 88, 6, 3));

            Sprite[] run = Row(
                sheet, library, "Edward_Run",
                ProductionSheetSpriteFactory.EqualRow(
                    300, 612, 520, 80, 6, 3));

            Sprite[] combatIdle = Row(
                sheet, library, "Edward_CombatIdle",
                ProductionSheetSpriteFactory.EqualRow(
                    300, 724, 520, 108, 5, 4));

            Sprite[] attackLight = Row(
                sheet, library, "Edward_AttackLight",
                ProductionSheetSpriteFactory.EqualRow(
                    840, 370, 770, 104, 8, 3));

            Sprite[] attackHeavy = Row(
                sheet, library, "Edward_AttackHeavy",
                ProductionSheetSpriteFactory.EqualRow(
                    840, 498, 770, 88, 8, 3));

            Sprite[] cast = Row(
                sheet, library, "Edward_Cast",
                ProductionSheetSpriteFactory.EqualRow(
                    840, 611, 770, 87, 7, 3));

            Sprite[] hit = Row(
                sheet, library, "Edward_Hit",
                ProductionSheetSpriteFactory.EqualRow(
                    840, 724, 770, 82, 7, 3));

            Sprite[] death = Row(
                sheet, library, "Edward_Death",
                ProductionSheetSpriteFactory.EqualRow(
                    840, 833, 770, 78, 7, 3));

            library.clips = new[]
            {
                DirectionalClip(CharacterFrameState.Idle, 7f, true, -1, facing, idle),
                DirectionalClip(CharacterFrameState.Walk, 10f, true, -1, facing, walk),
                DirectionalClip(CharacterFrameState.Run, 12f, true, -1, facing, run),
                DirectionalClip(CharacterFrameState.CombatIdle, 7f, true, -1, facing, combatIdle),
                DirectionalClip(CharacterFrameState.Guard, 7f, true, -1, facing, combatIdle),
                ActionClip(CharacterFrameState.AttackLight, 14f, false, 4, attackLight),
                ActionClip(CharacterFrameState.AttackHeavy, 12f, false, 5, attackHeavy),
                ActionClip(CharacterFrameState.Cast, 10f, false, 5, cast),
                ActionClip(CharacterFrameState.Interact, 8f, false, -1, idle),
                ActionClip(CharacterFrameState.Hit, 12f, false, -1, hit),
                ActionClip(CharacterFrameState.CriticalHit, 11f, false, -1, hit),
                ActionClip(CharacterFrameState.Knockdown, 10f, false, -1, death),
                ActionClip(CharacterFrameState.Death, 9f, false, -1, death)
            };

            EditorUtility.SetDirty(library);
            return library;
        }

        private static FrameAnimationLibrary BuildEleanorLibrary()
        {
            Texture2D sheet =
                ProductionSheetSpriteFactory.LoadSheet(
                    SheetRoot + "/Eleanor_ProductionSheet.jpg");

            FrameAnimationLibrary library =
                GetOrCreateLibrary(
                    EleanorLibraryPath,
                    "eleanor_sheet",
                    "Eleanor — approved production sheet");

            ProductionSheetSpriteFactory.ClearSpriteSubAssets(library);

            FacingSprites facing = BuildFacingSprites(
                sheet,
                library,
                "Eleanor",
                new SheetRect(285, 58, 120, 225),
                new SheetRect(432, 58, 125, 225),
                new SheetRect(582, 58, 125, 225),
                new SheetRect(728, 58, 130, 225),
                new SheetRect(878, 58, 125, 225));

            Sprite[] idle = Row(
                sheet, library, "Eleanor_Idle",
                ProductionSheetSpriteFactory.EqualRow(
                    280, 336, 610, 80, 7, 3));

            Sprite[] walk = Row(
                sheet, library, "Eleanor_Walk",
                ProductionSheetSpriteFactory.EqualRow(
                    280, 438, 610, 78, 7, 3));

            Sprite[] combat = Row(
                sheet, library, "Eleanor_CombatIdle",
                ProductionSheetSpriteFactory.EqualRow(
                    280, 546, 610, 70, 6, 3));

            Sprite[] cast = Row(
                sheet, library, "Eleanor_Cast",
                ProductionSheetSpriteFactory.EqualRow(
                    280, 641, 610, 96, 6, 3));

            Sprite[] shield = Row(
                sheet, library, "Eleanor_Shield",
                ProductionSheetSpriteFactory.EqualRow(
                    920, 344, 690, 105, 7, 3));

            Sprite[] hit = Row(
                sheet, library, "Eleanor_Hit",
                ProductionSheetSpriteFactory.EqualRow(
                    920, 482, 690, 86, 7, 3));

            Sprite[] death = Row(
                sheet, library, "Eleanor_Death",
                ProductionSheetSpriteFactory.EqualRow(
                    920, 604, 690, 84, 6, 3));

            library.clips = new[]
            {
                DirectionalClip(CharacterFrameState.Idle, 7f, true, -1, facing, idle),
                DirectionalClip(CharacterFrameState.Walk, 9f, true, -1, facing, walk),
                DirectionalClip(CharacterFrameState.Run, 10f, true, -1, facing, walk),
                DirectionalClip(CharacterFrameState.CombatIdle, 7f, true, -1, facing, combat),
                DirectionalClip(CharacterFrameState.Guard, 7f, true, -1, facing, shield),
                ActionClip(CharacterFrameState.AttackLight, 9f, false, 4, cast),
                ActionClip(CharacterFrameState.AttackHeavy, 9f, false, 4, shield),
                ActionClip(CharacterFrameState.Cast, 9f, false, 4, cast),
                ActionClip(CharacterFrameState.Interact, 8f, false, -1, idle),
                ActionClip(CharacterFrameState.Hit, 10f, false, -1, hit),
                ActionClip(CharacterFrameState.CriticalHit, 10f, false, -1, hit),
                ActionClip(CharacterFrameState.Knockdown, 8f, false, -1, death),
                ActionClip(CharacterFrameState.Death, 8f, false, -1, death)
            };

            EditorUtility.SetDirty(library);
            return library;
        }

        private static FrameAnimationLibrary BuildAelisLibrary()
        {
            Texture2D sheet =
                ProductionSheetSpriteFactory.LoadSheet(
                    SheetRoot + "/Aelis_ProductionSheet.jpg");

            FrameAnimationLibrary library =
                GetOrCreateLibrary(
                    AelisLibraryPath,
                    "aelis_sheet",
                    "Aelis — approved production sheet");

            ProductionSheetSpriteFactory.ClearSpriteSubAssets(library);

            FacingSprites facing = BuildFacingSprites(
                sheet,
                library,
                "Aelis",
                new SheetRect(300, 72, 120, 230),
                new SheetRect(446, 72, 125, 230),
                new SheetRect(592, 72, 125, 230),
                new SheetRect(738, 72, 130, 230),
                new SheetRect(885, 72, 125, 230));

            Sprite[] idle = Row(
                sheet, library, "Aelis_Idle",
                ProductionSheetSpriteFactory.EqualRow(
                    285, 356, 475, 105, 5, 3));

            Sprite[] walk = Row(
                sheet, library, "Aelis_Walk",
                ProductionSheetSpriteFactory.EqualRow(
                    285, 482, 475, 90, 5, 3));

            Sprite[] combat = Row(
                sheet, library, "Aelis_CombatIdle",
                ProductionSheetSpriteFactory.EqualRow(
                    285, 598, 475, 91, 5, 3));

            Sprite[] heal = Row(
                sheet, library, "Aelis_Heal",
                ProductionSheetSpriteFactory.EqualRow(
                    790, 355, 815, 108, 7, 3));

            Sprite[] revive = Row(
                sheet, library, "Aelis_Revive",
                ProductionSheetSpriteFactory.EqualRow(
                    790, 490, 430, 86, 5, 3));

            Sprite[] barrier = Row(
                sheet, library, "Aelis_Barrier",
                ProductionSheetSpriteFactory.EqualRow(
                    790, 610, 430, 83, 5, 3));

            Sprite[] hit = Row(
                sheet, library, "Aelis_Hit",
                ProductionSheetSpriteFactory.EqualRow(
                    1245, 490, 365, 86, 4, 3));

            Sprite[] death = Row(
                sheet, library, "Aelis_Death",
                ProductionSheetSpriteFactory.EqualRow(
                    1245, 610, 365, 83, 4, 3));

            library.clips = new[]
            {
                DirectionalClip(CharacterFrameState.Idle, 7f, true, -1, facing, idle),
                DirectionalClip(CharacterFrameState.Walk, 9f, true, -1, facing, walk),
                DirectionalClip(CharacterFrameState.Run, 10f, true, -1, facing, walk),
                DirectionalClip(CharacterFrameState.CombatIdle, 7f, true, -1, facing, combat),
                DirectionalClip(CharacterFrameState.Guard, 7f, true, -1, facing, barrier),
                ActionClip(CharacterFrameState.AttackLight, 9f, false, 4, heal),
                ActionClip(CharacterFrameState.AttackHeavy, 9f, false, 3, revive),
                ActionClip(CharacterFrameState.Cast, 9f, false, 4, heal),
                ActionClip(CharacterFrameState.Interact, 8f, false, -1, idle),
                ActionClip(CharacterFrameState.Hit, 10f, false, -1, hit),
                ActionClip(CharacterFrameState.CriticalHit, 10f, false, -1, hit),
                ActionClip(CharacterFrameState.Knockdown, 8f, false, -1, death),
                ActionClip(CharacterFrameState.Death, 8f, false, -1, death)
            };

            EditorUtility.SetDirty(library);
            return library;
        }

        private static FrameAnimationLibrary BuildWhiteLibrary()
        {
            Texture2D sheet =
                ProductionSheetSpriteFactory.LoadSheet(
                    SheetRoot + "/Eleanor_ProductionSheet.jpg");

            FrameAnimationLibrary library =
                GetOrCreateLibrary(
                    WhiteLibraryPath,
                    "white_sheet",
                    "White — goat form from approved sheet");

            ProductionSheetSpriteFactory.ClearSpriteSubAssets(library);

            FacingSprites facing = BuildFacingSprites(
                sheet,
                library,
                "White",
                new SheetRect(440, 770, 105, 120),
                new SheetRect(555, 770, 115, 120),
                new SheetRect(675, 770, 120, 120),
                new SheetRect(805, 770, 120, 120),
                new SheetRect(925, 770, 105, 120));

            Sprite[] idle = Row(
                sheet, library, "White_Idle",
                ProductionSheetSpriteFactory.EqualRow(
                    1070, 770, 540, 58, 6, 3));

            Sprite[] walk = Row(
                sheet, library, "White_Walk",
                ProductionSheetSpriteFactory.EqualRow(
                    1070, 837, 540, 62, 6, 3));

            library.clips = new[]
            {
                DirectionalClip(CharacterFrameState.Idle, 6f, true, -1, facing, idle),
                DirectionalClip(CharacterFrameState.Walk, 9f, true, -1, facing, walk),
                DirectionalClip(CharacterFrameState.Run, 10f, true, -1, facing, walk),
                DirectionalClip(CharacterFrameState.CombatIdle, 6f, true, -1, facing, idle),
                DirectionalClip(CharacterFrameState.Guard, 6f, true, -1, facing, idle),
                DirectionalClip(CharacterFrameState.Hit, 8f, false, -1, facing, idle),
                DirectionalClip(CharacterFrameState.CriticalHit, 8f, false, -1, facing, idle),
                DirectionalClip(CharacterFrameState.Death, 8f, false, -1, facing, idle)
            };

            EditorUtility.SetDirty(library);
            return library;
        }

        private static FacingSprites BuildFacingSprites(
            Texture2D sheet,
            UnityEngine.Object owner,
            string prefix,
            SheetRect n,
            SheetRect ne,
            SheetRect e,
            SheetRect se,
            SheetRect s)
        {
            return new FacingSprites(
                Sprite(sheet, owner, prefix + "_Facing_N", n),
                Sprite(sheet, owner, prefix + "_Facing_NE", ne),
                Sprite(sheet, owner, prefix + "_Facing_E", e),
                Sprite(sheet, owner, prefix + "_Facing_SE", se),
                Sprite(sheet, owner, prefix + "_Facing_S", s));
        }

        private static Sprite Sprite(
            Texture2D sheet,
            UnityEngine.Object owner,
            string name,
            SheetRect rect)
        {
            return ProductionSheetSpriteFactory.CreateExtractedCharacterSprite(
                sheet,
                rect,
                name,
                owner,
                CharacterPixelsPerUnit,
                new Vector2(0.5f, 0.04f));
        }

        private static Sprite[] Row(
            Texture2D sheet,
            UnityEngine.Object owner,
            string prefix,
            SheetRect[] rects)
        {
            return ProductionSheetSpriteFactory.CreateExtractedCharacterRow(
                sheet,
                prefix,
                rects,
                owner,
                CharacterPixelsPerUnit,
                new Vector2(0.5f, 0.04f));
        }

        private static FrameAnimationClip8 DirectionalClip(
            CharacterFrameState state,
            float fps,
            bool loop,
            int impact,
            FacingSprites facing,
            Sprite[] primarySequence)
        {
            int count =
                primarySequence != null && primarySequence.Length > 0
                    ? primarySequence.Length
                    : 1;

            return new FrameAnimationClip8
            {
                state = state,
                framesPerSecond = fps,
                loop = loop,
                impactFrame = impact,
                frames = new DirectionalFrameStrip8
                {
                    north = ProductionSheetSpriteFactory.Repeat(facing.N, count),
                    northEast = ProductionSheetSpriteFactory.Repeat(facing.NE, count),
                    east = ProductionSheetSpriteFactory.Repeat(facing.E, count),
                    southEast =
                        primarySequence != null && primarySequence.Length > 0
                            ? primarySequence
                            : ProductionSheetSpriteFactory.Repeat(facing.SE, count),
                    south = ProductionSheetSpriteFactory.Repeat(facing.S, count),
                    mirrorMissingWest = true
                }
            };
        }

        private static FrameAnimationClip8 ActionClip(
            CharacterFrameState state,
            float fps,
            bool loop,
            int impact,
            Sprite[] sequence)
        {
            Sprite[] safe =
                sequence != null
                    ? sequence
                    : Array.Empty<Sprite>();

            return new FrameAnimationClip8
            {
                state = state,
                framesPerSecond = fps,
                loop = loop,
                impactFrame = impact,
                frames = new DirectionalFrameStrip8
                {
                    north = safe,
                    northEast = safe,
                    east = safe,
                    southEast = safe,
                    south = safe,
                    mirrorMissingWest = true
                }
            };
        }

        private static FrameAnimationLibrary GetOrCreateLibrary(
            string path,
            string id,
            string displayName)
        {
            FrameAnimationLibrary library =
                AssetDatabase.LoadAssetAtPath<FrameAnimationLibrary>(path);

            if (library == null)
            {
                library =
                    ScriptableObject.CreateInstance<FrameAnimationLibrary>();

                AssetDatabase.CreateAsset(library, path);
            }

            library.libraryId = id;
            library.displayName = displayName;
            return library;
        }

        private static CharacterDefinition GetOrCreateDefinition(
            string displayName,
            CombatFaction faction,
            int hp,
            int mana,
            string path)
        {
            CharacterDefinition definition =
                AssetDatabase.LoadAssetAtPath<CharacterDefinition>(path);

            if (definition == null)
            {
                definition =
                    ScriptableObject.CreateInstance<CharacterDefinition>();

                AssetDatabase.CreateAsset(definition, path);
            }

            definition.characterId =
                displayName.ToLowerInvariant();

            definition.displayName = displayName;
            definition.faction = faction;
            definition.maxHealth = Mathf.Max(1, hp);
            definition.maxMana = Mathf.Max(0, mana);
            definition.actionPoints = 2;
            definition.movementMeters = 9f;

            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void BuildCharacterPrefab(
            string objectName,
            FrameAnimationLibrary library,
            CharacterDefinition definition,
            string prefabPath,
            bool playerControlled,
            float controllerHeight,
            float controllerRadius)
        {
            Material cutout =
                ProductionSheetSpriteFactory.GetOrCreateSheetMaterial();

            GameObject root = new GameObject(objectName);

            CharacterController controller =
                root.AddComponent<CharacterController>();

            controller.height = controllerHeight;
            controller.radius = controllerRadius;
            controller.center =
                new Vector3(0f, controllerHeight * 0.5f, 0f);

            CombatantRuntime combatant =
                root.AddComponent<CombatantRuntime>();

            combatant.SetDefinition(definition);

            if (playerControlled)
            {
                root.AddComponent<HighResExplorationController>();
                root.AddComponent<EdwardHighResTestDriver>();
            }

            root.AddComponent<HighResCharacterCombatBridge>();

            GameObject billboard =
                new GameObject("BillboardRoot");

            billboard.transform.SetParent(root.transform, false);
            billboard.transform.localPosition =
                new Vector3(0f, controllerHeight * 0.52f, 0f);

            billboard.AddComponent<BillboardCharacter2D>();

            HighResFrameCharacter2D animator =
                billboard.AddComponent<HighResFrameCharacter2D>();

            SpriteRenderer baseRenderer =
                CreateRenderer(
                    billboard.transform,
                    "Base",
                    10,
                    cutout);

            SpriteRenderer armor =
                CreateRenderer(
                    billboard.transform,
                    "Armor",
                    20,
                    cutout);

            SpriteRenderer cloak =
                CreateRenderer(
                    billboard.transform,
                    "Cloak",
                    30,
                    cutout);

            SpriteRenderer weapon =
                CreateRenderer(
                    billboard.transform,
                    "Weapon",
                    40,
                    cutout);

            SpriteRenderer headgear =
                CreateRenderer(
                    billboard.transform,
                    "Headgear",
                    50,
                    cutout);

            SpriteRenderer accessory =
                CreateRenderer(
                    billboard.transform,
                    "Accessory",
                    60,
                    cutout);

            animator.Configure(
                library,
                baseRenderer,
                armor,
                cloak,
                weapon,
                headgear,
                accessory);

            PrefabUtility.SaveAsPrefabAsset(
                root,
                prefabPath);

            UnityEngine.Object.DestroyImmediate(root);
        }

        private static SpriteRenderer CreateRenderer(
            Transform parent,
            string layerName,
            int sortingOrder,
            Material material)
        {
            GameObject layer =
                new GameObject(layerName);

            layer.transform.SetParent(parent, false);

            SpriteRenderer renderer =
                layer.AddComponent<SpriteRenderer>();

            renderer.sortingOrder = sortingOrder;
            renderer.sharedMaterial = material;
            return renderer;
        }

        private static void EnsureFolders()
        {
            ProductionSheetSpriteFactory.EnsureFolder(
                "Assets/KeeperFirstCovenant",
                "Data");

            ProductionSheetSpriteFactory.EnsureFolder(
                "Assets/KeeperFirstCovenant/Data",
                "ProductionCharacters");

            ProductionSheetSpriteFactory.EnsureFolder(
                "Assets/KeeperFirstCovenant",
                "Prefabs");

            ProductionSheetSpriteFactory.EnsureFolder(
                "Assets/KeeperFirstCovenant/Prefabs",
                "ProductionCharacters");
        }
    }
}
#endif
