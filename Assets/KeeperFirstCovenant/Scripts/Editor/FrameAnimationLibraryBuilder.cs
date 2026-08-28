#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using KeeperFirstCovenant.Visual;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    public static class FrameAnimationLibraryBuilder
    {
        public const string EdwardBaseRoot =
            "Assets/KeeperFirstCovenant/Art/Characters/Edward/Base";

        public const string EdwardDataRoot =
            "Assets/KeeperFirstCovenant/Data/Visual";

        public const string EdwardBaseLibraryPath =
            EdwardDataRoot + "/Edward_BaseFrames.asset";

        private static readonly CharacterFrameState[] RequiredStates =
        {
            CharacterFrameState.Idle,
            CharacterFrameState.Walk,
            CharacterFrameState.Run,
            CharacterFrameState.CombatIdle,
            CharacterFrameState.Guard,
            CharacterFrameState.AttackLight,
            CharacterFrameState.AttackHeavy,
            CharacterFrameState.Cast,
            CharacterFrameState.Interact,
            CharacterFrameState.Hit,
            CharacterFrameState.CriticalHit,
            CharacterFrameState.Knockdown,
            CharacterFrameState.Death
        };

        [MenuItem("Keeper First Covenant/High-Res 2D/Rebuild Edward Base Library")]
        public static void RebuildEdwardBaseLibrary()
        {
            EnsureFolder("Assets/KeeperFirstCovenant", "Data");
            EnsureFolder("Assets/KeeperFirstCovenant/Data", "Visual");

            FrameAnimationLibrary library =
                AssetDatabase.LoadAssetAtPath<FrameAnimationLibrary>(
                    EdwardBaseLibraryPath);

            if (library == null)
            {
                library = ScriptableObject.CreateInstance<FrameAnimationLibrary>();
                AssetDatabase.CreateAsset(library, EdwardBaseLibraryPath);
            }

            library.libraryId = "edward_base";
            library.displayName = "Edward — high-resolution base frames";

            List<FrameAnimationClip8> clips = new List<FrameAnimationClip8>();

            foreach (CharacterFrameState state in RequiredStates)
            {
                FrameAnimationClip8 clip = BuildClip(state, EdwardBaseRoot);
                clips.Add(clip);
            }

            library.clips = clips.ToArray();
            EditorUtility.SetDirty(library);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ValidateLibrary(library);

            Debug.Log(
                "Edward high-resolution frame library rebuilt: " +
                EdwardBaseLibraryPath);
        }

        public static FrameAnimationLibrary BuildEquipmentLibrary(
            string libraryId,
            string displayName,
            string artRoot,
            string assetPath)
        {
            EnsureFolder("Assets/KeeperFirstCovenant", "Data");
            EnsureFolder("Assets/KeeperFirstCovenant/Data", "Visual");

            FrameAnimationLibrary library =
                AssetDatabase.LoadAssetAtPath<FrameAnimationLibrary>(assetPath);

            if (library == null)
            {
                library = ScriptableObject.CreateInstance<FrameAnimationLibrary>();
                AssetDatabase.CreateAsset(library, assetPath);
            }

            library.libraryId = libraryId;
            library.displayName = displayName;

            List<FrameAnimationClip8> clips = new List<FrameAnimationClip8>();
            foreach (CharacterFrameState state in RequiredStates)
                clips.Add(BuildClip(state, artRoot));

            library.clips = clips.ToArray();
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();

            return library;
        }

        private static FrameAnimationClip8 BuildClip(
            CharacterFrameState state,
            string root)
        {
            FrameAnimationClip8 clip = new FrameAnimationClip8
            {
                state = state,
                framesPerSecond = DefaultFps(state),
                loop = IsLooping(state),
                impactFrame = DefaultImpactFrame(state),
                frames = new DirectionalFrameStrip8()
            };

            clip.frames.north = LoadStrip(root, state, "N");
            clip.frames.northEast = LoadStrip(root, state, "NE");
            clip.frames.east = LoadStrip(root, state, "E");
            clip.frames.southEast = LoadStrip(root, state, "SE");
            clip.frames.south = LoadStrip(root, state, "S");

            clip.frames.southWest = LoadStrip(root, state, "SW");
            clip.frames.west = LoadStrip(root, state, "W");
            clip.frames.northWest = LoadStrip(root, state, "NW");

            clip.frames.mirrorMissingWest = true;
            return clip;
        }

        private static Sprite[] LoadStrip(
            string root,
            CharacterFrameState state,
            string facingFolder)
        {
            string folder =
                root + "/" + state + "/" + facingFolder;

            if (!AssetDatabase.IsValidFolder(folder))
                return Array.Empty<Sprite>();

            string absoluteFolder = Path.GetFullPath(folder);
            if (!Directory.Exists(absoluteFolder))
                return Array.Empty<Sprite>();

            string[] files = Directory.GetFiles(
                absoluteFolder,
                "*.png",
                SearchOption.TopDirectoryOnly);

            Array.Sort(files, StringComparer.OrdinalIgnoreCase);

            List<Sprite> sprites = new List<Sprite>();
            foreach (string absolutePath in files)
            {
                string normalized = absolutePath
                    .Replace("\\", "/");

                int assetsIndex = normalized.IndexOf(
                    "/Assets/",
                    StringComparison.OrdinalIgnoreCase);

                string assetPath = assetsIndex >= 0
                    ? normalized.Substring(assetsIndex + 1)
                    : normalized;

                Sprite sprite =
                    AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

                if (sprite != null)
                    sprites.Add(sprite);
            }

            return sprites.ToArray();
        }

        private static float DefaultFps(CharacterFrameState state)
        {
            switch (state)
            {
                case CharacterFrameState.Idle:
                case CharacterFrameState.CombatIdle:
                    return 6f;

                case CharacterFrameState.Walk:
                    return 10f;

                case CharacterFrameState.Run:
                    return 12f;

                case CharacterFrameState.AttackLight:
                    return 14f;

                case CharacterFrameState.AttackHeavy:
                    return 12f;

                case CharacterFrameState.Hit:
                case CharacterFrameState.CriticalHit:
                    return 12f;

                case CharacterFrameState.Death:
                    return 10f;

                default:
                    return 9f;
            }
        }

        private static bool IsLooping(CharacterFrameState state)
        {
            return state == CharacterFrameState.Idle ||
                   state == CharacterFrameState.Walk ||
                   state == CharacterFrameState.Run ||
                   state == CharacterFrameState.CombatIdle ||
                   state == CharacterFrameState.Guard;
        }

        private static int DefaultImpactFrame(CharacterFrameState state)
        {
            switch (state)
            {
                case CharacterFrameState.AttackLight:
                    return 5;

                case CharacterFrameState.AttackHeavy:
                    return 7;

                case CharacterFrameState.Cast:
                    return 6;

                default:
                    return -1;
            }
        }

        private static void ValidateLibrary(FrameAnimationLibrary library)
        {
            if (library == null)
                return;

            List<string> missing = new List<string>();

            foreach (CharacterFrameState state in RequiredStates)
            {
                FrameAnimationClip8 clip = library.Find(state);
                if (clip == null)
                {
                    missing.Add(state + ": clip missing");
                    continue;
                }

                ValidateDirection(clip, SpriteFacing8.North, "N", missing);
                ValidateDirection(clip, SpriteFacing8.NorthEast, "NE", missing);
                ValidateDirection(clip, SpriteFacing8.East, "E", missing);
                ValidateDirection(clip, SpriteFacing8.SouthEast, "SE", missing);
                ValidateDirection(clip, SpriteFacing8.South, "S", missing);
            }

            if (missing.Count > 0)
            {
                Debug.LogWarning(
                    "Edward frame library is not complete yet. Missing:\n - " +
                    string.Join("\n - ", missing));
            }
        }

        private static void ValidateDirection(
            FrameAnimationClip8 clip,
            SpriteFacing8 facing,
            string label,
            List<string> missing)
        {
            if (clip.GetFrameCount(facing) <= 0)
                missing.Add(clip.state + "/" + label);
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
