#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    public static class ProductionWorldPack01Validator
    {
        [MenuItem("Keeper First Covenant/Production Art/World Pack 01/Validate Pack")]
        public static void ValidateFromMenu()
        {
            Validate(true);
        }

        public static bool Validate(bool verbose)
        {
            var errors = new List<string>();

            RequireSprite(
                ProductionWorldPack01Builder.WideArchPath,
                errors);

            RequireSprite(
                ProductionWorldPack01Builder.TallRuneStonePath,
                errors);

            RequireSprite(
                ProductionWorldPack01Builder.BrightCirclePath,
                errors);

            RequireSprite(
                ProductionWorldPack01Builder.BlueBrazierPath,
                errors);

            ValidateArch(errors);
            ValidateRuneStone(errors);
            ValidateCircle(errors);
            ValidateBrazier(errors);

            if (errors.Count > 0)
            {
                Debug.LogError(
                    "Production World Pack 01 validation failed:\n - " +
                    string.Join("\n - ", errors));

                return false;
            }

            if (verbose)
            {
                Debug.Log(
                    "Production World Pack 01 validation passed: " +
                    "4 sprites, 4 prefabs, solid/trigger collision and magical VFX are wired.");
            }

            return true;
        }

        private static void RequireSprite(
            string path,
            List<string> errors)
        {
            Sprite sprite =
                AssetDatabase.LoadAssetAtPath<Sprite>(
                    path);

            if (sprite == null)
                errors.Add("Missing sprite: " + path);
        }

        private static GameObject LoadPrefab(
            string path,
            List<string> errors)
        {
            GameObject prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    path);

            if (prefab == null)
                errors.Add("Missing prefab: " + path);

            return prefab;
        }

        private static void ValidateArch(
            List<string> errors)
        {
            GameObject prefab =
                LoadPrefab(
                    ProductionWorldPack01Builder.WideArchPrefabPath,
                    errors);

            if (prefab == null)
                return;

            if (prefab.GetComponent<SpriteRenderer>() == null)
                errors.Add("Wide arch has no SpriteRenderer.");

            BoxCollider[] colliders =
                prefab.GetComponents<BoxCollider>();

            if (colliders.Length < 3)
            {
                errors.Add(
                    "Wide arch must use separate left/right/lintel colliders so the opening remains walkable.");
            }
        }

        private static void ValidateRuneStone(
            List<string> errors)
        {
            GameObject prefab =
                LoadPrefab(
                    ProductionWorldPack01Builder.TallRuneStonePrefabPath,
                    errors);

            if (prefab == null)
                return;

            if (prefab.GetComponent<BoxCollider>() == null)
                errors.Add("Tall rune stone has no solid collider.");

            RequireFx(prefab, "Tall rune stone", errors);
        }

        private static void ValidateCircle(
            List<string> errors)
        {
            GameObject prefab =
                LoadPrefab(
                    ProductionWorldPack01Builder.BrightCirclePrefabPath,
                    errors);

            if (prefab == null)
                return;

            BoxCollider trigger =
                prefab.GetComponent<BoxCollider>();

            if (trigger == null || !trigger.isTrigger)
            {
                errors.Add(
                    "Covenant circle must be walkable and use a trigger volume instead of solid collision.");
            }

            RequireFx(prefab, "Covenant circle", errors);
        }

        private static void ValidateBrazier(
            List<string> errors)
        {
            GameObject prefab =
                LoadPrefab(
                    ProductionWorldPack01Builder.BlueBrazierPrefabPath,
                    errors);

            if (prefab == null)
                return;

            if (prefab.GetComponent<BoxCollider>() == null)
                errors.Add("Blue brazier has no solid base collider.");

            SphereCollider heat =
                prefab.GetComponent<SphereCollider>();

            if (heat == null || !heat.isTrigger)
                errors.Add("Blue brazier has no heat/VFX trigger volume.");

            RequireFx(prefab, "Blue brazier", errors);
        }

        private static void RequireFx(
            GameObject prefab,
            string label,
            List<string> errors)
        {
            if (prefab.GetComponentInChildren<Light>(true) == null)
                errors.Add(label + " has no dynamic light.");

            if (prefab.GetComponentInChildren<ParticleSystem>(true) == null)
                errors.Add(label + " has no particle VFX.");

            if (prefab.GetComponent<KeeperFirstCovenant.Visual.ProductionWorldPropFx>() == null)
                errors.Add(label + " has no pulsing FX controller.");
        }
    }
}
#endif
