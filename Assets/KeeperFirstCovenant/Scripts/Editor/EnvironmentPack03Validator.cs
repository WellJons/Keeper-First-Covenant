#if UNITY_EDITOR
using System.Collections.Generic;
using KeeperFirstCovenant.Visual;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    public static class EnvironmentPack03Validator
    {
        private static readonly string[] Prefabs =
        {
            "Rocks/Rock_SmallPile_A.prefab",
            "Rocks/Rock_Cluster_Medium_A.prefab",
            "Rocks/Rock_Shard_Tall_A.prefab",
            "Rocks/Rock_Boulder_Mossy_A.prefab",
            "Rocks/Rock_Rubble_Flat_A.prefab",

            "Nature/Grass_Clump_B.prefab",
            "Nature/Grass_Dense_A.prefab",
            "Nature/Flower_Blue_B.prefab",
            "Nature/Flower_Pale_A.prefab",

            "Trees/Tree_Leafy_C.prefab",
            "Trees/Sapling_A.prefab",
            "Trees/Stump_B.prefab",

            "Fillers/Puddle_MuddyCluster_A.prefab",
            "Fillers/BrokenSignpost_A.prefab",
            "Fillers/Debris_Small_A.prefab"
        };

        [MenuItem("Keeper First Covenant/Production Art/Environment Pack 03/Validate Pack")]
        public static void ValidateFromMenu()
        {
            Validate(true);
        }

        public static bool Validate(bool verbose)
        {
            var errors = new List<string>();

            if (!EnvironmentPack03Builder.SourcesPresent())
                errors.Add("One or more Environment Pack 03 source sprites are missing.");

            foreach (string relative in Prefabs)
            {
                string path =
                    EnvironmentPack03Builder.PrefabRoot + "/" + relative;

                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
                    errors.Add("Missing prefab: " + path);
            }

            RequireCollider(
                "Rocks/Rock_Cluster_Medium_A.prefab",
                "Rock_Cluster_Medium_A",
                errors);

            RequireCollider(
                "Rocks/Rock_Shard_Tall_A.prefab",
                "Rock_Shard_Tall_A",
                errors);

            RequireWind(
                "Nature/Grass_Dense_A.prefab",
                "Grass_Dense_A",
                errors);

            RequireWind(
                "Nature/Flower_Blue_B.prefab",
                "Flower_Blue_B",
                errors);

            RequireWind(
                "Trees/Tree_Leafy_C.prefab",
                "Tree_Leafy_C",
                errors);

            GameObject tree = Load("Trees/Tree_Leafy_C.prefab");
            if (tree != null && tree.GetComponent<CapsuleCollider>() == null)
                errors.Add("Tree_Leafy_C has no trunk CapsuleCollider.");

            GameObject stump = Load("Trees/Stump_B.prefab");
            if (stump != null && stump.GetComponent<BoxCollider>() == null)
                errors.Add("Stump_B has no solid collider.");

            GameObject puddle = Load("Fillers/Puddle_MuddyCluster_A.prefab");
            if (puddle != null)
            {
                BoxCollider trigger = puddle.GetComponent<BoxCollider>();
                if (trigger == null || !trigger.isTrigger)
                    errors.Add("Puddle_MuddyCluster_A must use a shallow trigger.");
            }

            if (errors.Count > 0)
            {
                Debug.LogError(
                    "Environment Pack 03 validation failed:\n - " +
                    string.Join("\n - ", errors));

                return false;
            }

            if (verbose)
            {
                Debug.Log(
                    "Environment Pack 03 validation passed: " +
                    "15 prefabs, stable rock/tree collision, wind-reactive plants and puddle trigger are ready.");
            }

            return true;
        }

        private static GameObject Load(string relative)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(
                EnvironmentPack03Builder.PrefabRoot + "/" + relative);
        }

        private static void RequireCollider(
            string relative,
            string label,
            List<string> errors)
        {
            GameObject prefab = Load(relative);
            if (prefab == null)
                return;

            if (prefab.GetComponent<Collider>() == null)
                errors.Add(label + " has no world collider.");
        }

        private static void RequireWind(
            string relative,
            string label,
            List<string> errors)
        {
            GameObject prefab = Load(relative);
            if (prefab == null)
                return;

            if (prefab.GetComponent<WindReactiveProp>() == null)
                errors.Add(label + " has no WindReactiveProp.");

            if (prefab.GetComponentInChildren<SpriteRenderer>(true) == null)
                errors.Add(label + " has no SpriteRenderer.");
        }
    }
}
#endif
