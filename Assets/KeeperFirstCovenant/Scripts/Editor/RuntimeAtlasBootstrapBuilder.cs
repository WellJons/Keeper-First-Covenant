#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using KeeperFirstCovenant.Visual;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    public static class RuntimeAtlasBootstrapBuilder
    {
        private const string EdwardTexture =
            "Assets/KeeperFirstCovenant/Art/Runtime/Characters/Edward/Edward_Directions_5.png";

        private const string EleanorTexture =
            "Assets/KeeperFirstCovenant/Art/Runtime/Characters/Eleanor/Eleanor_Directions_5.png";

        private const string AelisTexture =
            "Assets/KeeperFirstCovenant/Art/Runtime/Characters/Aelis/Aelis_Directions_5.png";

        private const string WorldTexture =
            "Assets/KeeperFirstCovenant/Art/Runtime/World/World_StarterAtlas.png";

        private const string DataRoot =
            "Assets/KeeperFirstCovenant/Data/RuntimeArt";

        private const string WorldPrefabRoot =
            "Assets/KeeperFirstCovenant/Prefabs/RuntimeWorld";

        private static readonly string[] WorldNames =
        {
            "RockMonolith",
            "RuinedArch",
            "StoneFloor",
            "ShrineAltar",
            "CovenantRuneCircle",
            "RockCluster",
            "Boulder",
            "Campfire",
            "Brazier",
            "Wagon",
            "BrokenWall",
            "CovenantCrystal"
        };

        [MenuItem("Keeper First Covenant/High-Res 2D/Hydrate + Build Runtime Art")]
        public static void HydrateAndBuild()
        {
            PackedArtHydrator.HydrateFromMenu();

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            BuildDirectionalLibrary(
                "Edward",
                EdwardTexture,
                FrameAnimationLibraryBuilder.EdwardBaseLibraryPath);

            BuildDirectionalLibrary(
                "Eleanor",
                EleanorTexture,
                DataRoot + "/Eleanor_BaseFrames.asset");

            BuildDirectionalLibrary(
                "Aelis",
                AelisTexture,
                DataRoot + "/Aelis_BaseFrames.asset");

            BuildWorldArt();

            HighResEdwardPrefabBuilder.BuildEdwardPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Keeper runtime art bootstrap complete. " +
                "Painted direction atlases are now Unity sprites, " +
                "Edward prefab is rebuilt, and reusable world prefabs are available.");
        }

        public static FrameAnimationLibrary BuildDirectionalLibrary(
            string characterName,
            string texturePath,
            string assetPath)
        {
            EnsureFolders();

            Texture2D texture =
                AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

            if (texture == null)
            {
                Debug.LogWarning(
                    characterName +
                    " runtime direction atlas is not available yet: " +
                    texturePath);
                return null;
            }

            FrameAnimationLibrary library =
                AssetDatabase.LoadAssetAtPath<FrameAnimationLibrary>(
                    assetPath);

            if (library == null)
            {
                library =
                    ScriptableObject.CreateInstance<FrameAnimationLibrary>();

                AssetDatabase.CreateAsset(library, assetPath);
            }

            RemoveOldGeneratedSprites(library);

            Sprite[] directions = SliceFiveDirections(
                texture,
                library,
                characterName);

            library.libraryId =
                characterName.ToLowerInvariant() + "_runtime";

            library.displayName =
                characterName + " — runtime painted directions";

            var clips = new List<FrameAnimationClip8>();

            CharacterFrameState[] previewStates =
            {
                CharacterFrameState.Idle,
                CharacterFrameState.Walk,
                CharacterFrameState.Run,
                CharacterFrameState.CombatIdle,
                CharacterFrameState.Guard
            };

            foreach (CharacterFrameState state in previewStates)
            {
                clips.Add(
                    BuildSingleFrameClip(
                        state,
                        directions,
                        state == CharacterFrameState.Walk
                            ? 10f
                            : state == CharacterFrameState.Run
                                ? 12f
                                : 6f));
            }

            library.clips = clips.ToArray();

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();

            return library;
        }

        private static Sprite[] SliceFiveDirections(
            Texture2D texture,
            UnityEngine.Object owner,
            string prefix)
        {
            int cellWidth = texture.width / 5;
            int cellHeight = texture.height;

            Sprite[] sprites = new Sprite[5];

            string[] labels =
            {
                "N",
                "NE",
                "E",
                "SE",
                "S"
            };

            for (int i = 0; i < 5; i++)
            {
                Rect rect = new Rect(
                    i * cellWidth,
                    0,
                    cellWidth,
                    cellHeight);

                Sprite sprite = Sprite.Create(
                    texture,
                    rect,
                    new Vector2(0.5f, 0.08f),
                    256f,
                    0,
                    SpriteMeshType.FullRect);

                sprite.name =
                    prefix + "_" + labels[i];

                AssetDatabase.AddObjectToAsset(
                    sprite,
                    owner);

                sprites[i] = sprite;
            }

            return sprites;
        }

        private static FrameAnimationClip8 BuildSingleFrameClip(
            CharacterFrameState state,
            Sprite[] s,
            float fps)
        {
            return new FrameAnimationClip8
            {
                state = state,
                framesPerSecond = fps,
                loop = true,
                impactFrame = -1,
                frames = new DirectionalFrameStrip8
                {
                    north = One(s[0]),
                    northEast = One(s[1]),
                    east = One(s[2]),
                    southEast = One(s[3]),
                    south = One(s[4]),
                    mirrorMissingWest = true
                }
            };
        }

        private static Sprite[] One(Sprite sprite)
        {
            return sprite != null
                ? new[] { sprite }
                : Array.Empty<Sprite>();
        }

        private static void RemoveOldGeneratedSprites(
            UnityEngine.Object owner)
        {
            string path = AssetDatabase.GetAssetPath(owner);

            UnityEngine.Object[] assets =
                AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (UnityEngine.Object asset in assets)
            {
                if (asset == null || asset == owner)
                    continue;

                if (asset is Sprite)
                    UnityEngine.Object.DestroyImmediate(
                        asset,
                        true);
            }
        }

        public static RuntimeWorldArtDefinition BuildWorldArt()
        {
            EnsureFolders();

            Texture2D texture =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    WorldTexture);

            if (texture == null)
            {
                Debug.LogWarning(
                    "World runtime atlas is not available yet: " +
                    WorldTexture);
                return null;
            }

            string assetPath =
                DataRoot + "/World_StarterArt.asset";

            RuntimeWorldArtDefinition definition =
                AssetDatabase.LoadAssetAtPath<RuntimeWorldArtDefinition>(
                    assetPath);

            if (definition == null)
            {
                definition =
                    ScriptableObject.CreateInstance<RuntimeWorldArtDefinition>();

                AssetDatabase.CreateAsset(
                    definition,
                    assetPath);
            }

            RemoveOldGeneratedSprites(definition);

            int cellWidth = texture.width / 4;
            int cellHeight = texture.height / 3;

            Sprite[] sprites = new Sprite[12];

            for (int topRow = 0; topRow < 3; topRow++)
            {
                int sourceRow = 2 - topRow;

                for (int column = 0; column < 4; column++)
                {
                    int index =
                        topRow * 4 + column;

                    Rect rect = new Rect(
                        column * cellWidth,
                        sourceRow * cellHeight,
                        cellWidth,
                        cellHeight);

                    Sprite sprite = Sprite.Create(
                        texture,
                        rect,
                        new Vector2(0.5f, 0.08f),
                        256f,
                        0,
                        SpriteMeshType.Tight);

                    sprite.name =
                        WorldNames[index];

                    AssetDatabase.AddObjectToAsset(
                        sprite,
                        definition);

                    sprites[index] = sprite;
                }
            }

            definition.rockMonolith = sprites[0];
            definition.ruinedArch = sprites[1];
            definition.stoneFloor = sprites[2];
            definition.shrineAltar = sprites[3];
            definition.covenantRuneCircle = sprites[4];
            definition.rockCluster = sprites[5];
            definition.boulder = sprites[6];
            definition.campfire = sprites[7];
            definition.brazier = sprites[8];
            definition.wagon = sprites[9];
            definition.brokenWall = sprites[10];
            definition.covenantCrystal = sprites[11];

            EditorUtility.SetDirty(definition);

            BuildWorldPrefabs(definition);

            AssetDatabase.SaveAssets();

            return definition;
        }

        private static void BuildWorldPrefabs(
            RuntimeWorldArtDefinition definition)
        {
            for (int i = 0; i < WorldNames.Length; i++)
            {
                Sprite sprite = definition.Get(i);
                if (sprite == null)
                    continue;

                GameObject root =
                    new GameObject(WorldNames[i]);

                SpriteRenderer renderer =
                    root.AddComponent<SpriteRenderer>();

                renderer.sprite = sprite;
                renderer.sortingOrder = 10;

                bool floor =
                    i == 2 ||
                    i == 4;

                if (floor)
                {
                    root.transform.rotation =
                        Quaternion.Euler(90f, 0f, 0f);

                    renderer.sortingOrder = -20;
                }
                else
                {
                    root.AddComponent<BillboardCharacter2D>();
                }

                if (i == 0 ||
                    i == 1 ||
                    i == 3 ||
                    i == 5 ||
                    i == 6 ||
                    i == 9 ||
                    i == 10 ||
                    i == 11)
                {
                    BoxCollider collider =
                        root.AddComponent<BoxCollider>();

                    collider.center =
                        new Vector3(0f, 0.75f, 0f);

                    collider.size =
                        new Vector3(1.1f, 1.5f, 0.45f);
                }

                string path =
                    WorldPrefabRoot +
                    "/" +
                    WorldNames[i] +
                    ".prefab";

                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    path);

                UnityEngine.Object.DestroyImmediate(
                    root);
            }
        }

        private static void EnsureFolders()
        {
            EnsureFolder(
                "Assets/KeeperFirstCovenant/Data",
                "RuntimeArt");

            EnsureFolder(
                "Assets/KeeperFirstCovenant/Prefabs",
                "RuntimeWorld");
        }

        private static void EnsureFolder(
            string parent,
            string child)
        {
            string path =
                parent + "/" + child;

            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(
                    parent,
                    child);
        }
    }
}
#endif
