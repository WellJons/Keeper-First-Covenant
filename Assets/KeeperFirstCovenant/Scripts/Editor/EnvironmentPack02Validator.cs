#if UNITY_EDITOR
using System.Collections.Generic;
using KeeperFirstCovenant.Visual;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    public static class EnvironmentPack02Validator
    {
        private static readonly string[] Prefabs =
        {
            "Ground/Ground_Dirt_A.prefab",
            "Ground/Ground_Dirt_B.prefab",
            "Ground/Ground_Stone_A.prefab",
            "Ground/Ground_Stone_B.prefab",
            "Ground/Ground_Grass_A.prefab",
            "Ground/Path_Stone_A.prefab",
            "Ground/Transition_GrassToStone_A.prefab",
            "Ground/Puddle_A.prefab",
            "Nature/Grass_Small_A.prefab",
            "Nature/Grass_Tall_A.prefab",
            "Nature/Flower_Blue_A.prefab",
            "Trees/Tree_Living_A.prefab",
            "Trees/Tree_Living_B.prefab",
            "Trees/Tree_Twisted_A.prefab",
            "Trees/Tree_Dead_A.prefab",
            "Trees/Log_A.prefab"
        };

        [MenuItem("Keeper First Covenant/Production Art/Environment Pack 02/Validate Pack")]
        public static void ValidateFromMenu()
        {
            Validate(true);
        }

        public static bool Validate(bool verbose)
        {
            List<string> errors = new List<string>();

            if (!EnvironmentPack02Builder.SourcesPresent())
                errors.Add("One or more source PNG sprites are missing.");

            foreach (string relative in Prefabs)
            {
                string path =
                    EnvironmentPack02Builder.PrefabRoot + "/" + relative;

                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
                    errors.Add("Missing prefab: " + path);
            }

            ValidateWind(
                EnvironmentPack02Builder.PrefabRoot +
                "/Nature/Grass_Small_A.prefab",
                "Grass_Small_A",
                errors);

            ValidateWind(
                EnvironmentPack02Builder.PrefabRoot +
                "/Nature/Grass_Tall_A.prefab",
                "Grass_Tall_A",
                errors);

            ValidateTree(
                EnvironmentPack02Builder.PrefabRoot +
                "/Trees/Tree_Living_A.prefab",
                "Tree_Living_A",
                errors);

            ValidateTree(
                EnvironmentPack02Builder.PrefabRoot +
                "/Trees/Tree_Twisted_A.prefab",
                "Tree_Twisted_A",
                errors);

            GameObject puddle =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    EnvironmentPack02Builder.PrefabRoot +
                    "/Ground/Puddle_A.prefab");

            if (puddle != null)
            {
                BoxCollider trigger = puddle.GetComponent<BoxCollider>();
                if (trigger == null || !trigger.isTrigger)
                    errors.Add("Puddle_A must have a shallow trigger volume.");
            }

            if (errors.Count > 0)
            {
                Debug.LogError(
                    "Environment Pack 02 validation failed:\n - " +
                    string.Join("\n - ", errors));

                return false;
            }

            if (verbose)
            {
                Debug.Log(
                    "Environment Pack 02 validation passed: " +
                    "16 gameplay prefabs, foliage wind, tree colliders and water trigger are ready.");
            }

            return true;
        }

        private static void ValidateWind(
            string path,
            string label,
            List<string> errors)
        {
            GameObject prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
                return;

            if (prefab.GetComponent<WindReactiveProp>() == null)
                errors.Add(label + " has no WindReactiveProp.");

            if (prefab.GetComponentInChildren<SpriteRenderer>(true) == null)
                errors.Add(label + " has no SpriteRenderer.");
        }

        private static void ValidateTree(
            string path,
            string label,
            List<string> errors)
        {
            GameObject prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
                return;

            if (prefab.GetComponent<WindReactiveProp>() == null)
                errors.Add(label + " has no WindReactiveProp.");

            if (prefab.GetComponent<CapsuleCollider>() == null)
                errors.Add(label + " has no trunk CapsuleCollider.");
        }
    }
}
#endif
