#if UNITY_EDITOR
using System.Collections.Generic;
using KeeperFirstCovenant.Visual;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    public static class ProductionArtValidator
    {
        private static readonly CharacterFrameState[] RequiredEdwardStates =
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

        private static readonly SpriteFacing8[] RequiredFacings =
        {
            SpriteFacing8.North,
            SpriteFacing8.NorthEast,
            SpriteFacing8.East,
            SpriteFacing8.SouthEast,
            SpriteFacing8.South,
            SpriteFacing8.SouthWest,
            SpriteFacing8.West,
            SpriteFacing8.NorthWest
        };

        private static readonly string[] RequiredWorldPrefabs =
        {
            "StoneFloor_A",
            "StoneFloor_B",
            "RuneFloor",
            "RoadStraight",
            "RoadWide",
            "BrokenWall",
            "WallCorner",
            "RuinedArch",
            "RuinedArchWide",
            "Pillar",
            "BrokenPillar",
            "StoneStairs",
            "ShrineAltar",
            "RuneStone",
            "RuneStoneMossy",
            "CovenantCircle",
            "CovenantCircleBlue",
            "Puddle",
            "Rock",
            "Grass",
            "Flowers",
            "OldTree",
            "DeadTree",
            "BrazierOrange",
            "BrazierBlue",
            "Lantern",
            "Banner",
            "Campfire",
            "Crate",
            "Barrel",
            "Bench",
            "Wagon",
            "Tent",
            "Fence",
            "KeeperStatue",
            "CrystalPurple",
            "SmallShrine"
        };

        [MenuItem("Keeper First Covenant/Production Art/Validate In-Game Assets")]
        public static void ValidateMenu()
        {
            Validate(false);
        }

        public static bool Validate(bool throwOnFailure)
        {
            List<string> errors =
                new List<string>();

            ValidateCharacterPrefab(
                ProductionSheetCharacterBuilder.EdwardPrefabPath,
                "Edward",
                errors);

            ValidateCharacterPrefab(
                ProductionSheetCharacterBuilder.EleanorPrefabPath,
                "Eleanor",
                errors);

            ValidateCharacterPrefab(
                ProductionSheetCharacterBuilder.AelisPrefabPath,
                "Aelis",
                errors);

            ValidateCharacterPrefab(
                ProductionSheetCharacterBuilder.WhitePrefabPath,
                "White",
                errors);

            FrameAnimationLibrary edward =
                AssetDatabase.LoadAssetAtPath<FrameAnimationLibrary>(
                    ProductionSheetCharacterBuilder.EdwardLibraryPath);

            ValidateEdwardLibrary(
                edward,
                errors);

            foreach (string id in RequiredWorldPrefabs)
            {
                if (ProductionSheetWorldBuilder.LoadPrefab(id) == null)
                    errors.Add("World prefab missing: " + id);
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    ProductionArtGameBuilder.ScenePath) == null)
            {
                errors.Add(
                    "Playable production-art scene is missing: " +
                    ProductionArtGameBuilder.ScenePath);
            }

            if (errors.Count == 0)
            {
                Debug.Log(
                    "Keeper production-art validation PASSED: characters, " +
                    "animation libraries, world prefabs and playable scene are present.");
                return true;
            }

            string report =
                "Keeper production-art validation FAILED:\n - " +
                string.Join("\n - ", errors);

            Debug.LogError(report);

            if (throwOnFailure)
                throw new System.InvalidOperationException(report);

            return false;
        }

        private static void ValidateCharacterPrefab(
            string path,
            string displayName,
            List<string> errors)
        {
            GameObject prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                errors.Add(
                    displayName +
                    " prefab missing: " +
                    path);
                return;
            }

            HighResFrameCharacter2D animator =
                prefab.GetComponentInChildren<HighResFrameCharacter2D>(true);

            if (animator == null)
            {
                errors.Add(
                    displayName +
                    " has no HighResFrameCharacter2D.");
            }

            if (prefab.GetComponent<CharacterController>() == null)
            {
                errors.Add(
                    displayName +
                    " has no CharacterController.");
            }
        }

        private static void ValidateEdwardLibrary(
            FrameAnimationLibrary library,
            List<string> errors)
        {
            if (library == null)
            {
                errors.Add(
                    "Edward production animation library is missing.");
                return;
            }

            foreach (CharacterFrameState state in RequiredEdwardStates)
            {
                FrameAnimationClip8 clip =
                    library.Find(state);

                if (clip == null)
                {
                    errors.Add(
                        "Edward animation missing: " +
                        state);
                    continue;
                }

                foreach (SpriteFacing8 facing in RequiredFacings)
                {
                    if (!clip.frames.HasResolvableFrames(facing))
                    {
                        errors.Add(
                            "Edward " +
                            state +
                            " has no resolvable frames for " +
                            facing);
                    }
                }
            }
        }
    }
}
#endif
