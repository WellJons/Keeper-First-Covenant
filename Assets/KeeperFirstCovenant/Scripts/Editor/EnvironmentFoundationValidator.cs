#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using KeeperFirstCovenant.Environment;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    public static class EnvironmentFoundationValidator
    {
        private static readonly string[] GroundTextures =
        {
            EnvironmentFoundationPackBuilder.ArtRoot + "/Ground/Ground_MeadowGrass_A.png",
            EnvironmentFoundationPackBuilder.ArtRoot + "/Ground/Ground_WoodlandDirt_A.png",
            EnvironmentFoundationPackBuilder.ArtRoot + "/Ground/Ground_NaturalStone_A.png",
            EnvironmentFoundationPackBuilder.ArtRoot + "/Ground/Road_PackedDirt_A.png",
            EnvironmentFoundationPackBuilder.ArtRoot + "/Ground/Road_OldCobble_A.png"
        };

        private static readonly string[] FoliageTextures =
        {
            EnvironmentFoundationPackBuilder.ArtRoot + "/Foliage/Grass_LowMeadow_A.png",
            EnvironmentFoundationPackBuilder.ArtRoot + "/Foliage/Grass_TallMeadow_A.png",
            EnvironmentFoundationPackBuilder.ArtRoot + "/Foliage/Grass_Wildflowers_A.png"
        };

        private static readonly string[] GroundPrefabs =
        {
            "Ground_MeadowGrass_4m",
            "Ground_WoodlandDirt_4m",
            "Ground_NaturalStone_4m",
            "Road_PackedDirt_Straight_4m",
            "Road_PackedDirt_Corner_4m",
            "Road_PackedDirt_Cross_4m",
            "Road_PackedDirt_TJunction_4m",
            "Road_OldCobble_Straight_4m",
            "Road_OldCobble_Corner_4m",
            "Road_OldCobble_Cross_4m",
            "Road_OldCobble_TJunction_4m",
            "Transition_GrassToDirt_4m",
            "Transition_GrassToStone_4m"
        };

        private static readonly string[] FoliagePrefabs =
        {
            "Grass_LowMeadow_A",
            "Grass_TallMeadow_A",
            "Grass_Wildflowers_A"
        };

        [MenuItem("Keeper First Covenant/Environment Foundation/Validate Complete Pack")]
        public static void ValidateMenu()
        {
            Validate(false);
        }

        public static bool Validate(bool throwOnFailure)
        {
            List<string> errors = new List<string>();

            foreach (string path in GroundTextures)
                ValidateGroundTexture(path, errors);

            foreach (string path in FoliageTextures)
                ValidateFoliageTexture(path, errors);

            ValidateShader(
                "Keeper First Covenant/Environment/Painterly Ground",
                errors);
            ValidateShader(
                "Keeper First Covenant/Environment/Interactive Foliage",
                errors);
            ValidateShader(
                "Keeper First Covenant/Environment/Soft World Particle",
                errors);

            foreach (string id in GroundPrefabs)
                ValidateGroundPrefab(id, errors);

            foreach (string id in FoliagePrefabs)
                ValidateFoliagePrefab(id, errors);

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    EnvironmentFoundationPackBuilder.ScenePath) == null)
            {
                errors.Add(
                    "Test scene missing: " +
                    EnvironmentFoundationPackBuilder.ScenePath);
            }

            if (errors.Count == 0)
            {
                Debug.Log(
                    "Environment Foundation validation PASSED: 8 source textures, " +
                    "16 gameplay prefabs, surface physics, wind, interaction VFX and test scene.");
                return true;
            }

            string report =
                "Environment Foundation validation FAILED:\n - " +
                string.Join("\n - ", errors);
            Debug.LogError(report);

            if (throwOnFailure)
                throw new System.InvalidOperationException(report);

            return false;
        }

        private static void ValidateGroundTexture(
            string path,
            List<string> errors)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null)
            {
                errors.Add("Ground texture missing: " + path);
                return;
            }

            if (texture.width < 1024 || texture.height < 1024)
            {
                errors.Add(
                    "Ground texture below 1024 px: " +
                    path +
                    " (" + texture.width + "x" + texture.height + ")");
            }

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                errors.Add("No TextureImporter for: " + path);
                return;
            }

            if (!importer.mipmapEnabled)
                errors.Add("Ground mipmaps disabled: " + path);

            if (importer.wrapMode != TextureWrapMode.Mirror)
                errors.Add("Ground texture does not use mirrored repeat: " + path);
        }

        private static void ValidateFoliageTexture(
            string path,
            List<string> errors)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                errors.Add("Foliage sprite missing: " + path);
                return;
            }

            Texture2D texture = sprite.texture;
            if (texture.width < 1024 || texture.height < 1024)
            {
                errors.Add(
                    "Foliage sprite below 1024 px: " +
                    path +
                    " (" + texture.width + "x" + texture.height + ")");
            }

            ValidateAlphaMargin(path, errors);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null || !importer.alphaIsTransparency)
                errors.Add("Foliage alpha import is not configured: " + path);
        }

        private static void ValidateAlphaMargin(
            string path,
            List<string> errors)
        {
            if (!File.Exists(path))
            {
                errors.Add("Foliage source file not readable: " + path);
                return;
            }

            byte[] bytes = File.ReadAllBytes(path);
            Texture2D readable = new Texture2D(2, 2, TextureFormat.RGBA32, false);

            if (!ImageConversion.LoadImage(readable, bytes, false))
            {
                Object.DestroyImmediate(readable);
                errors.Add("Could not decode foliage PNG: " + path);
                return;
            }

            Color32[] pixels = readable.GetPixels32();
            int width = readable.width;
            int height = readable.height;
            int minX = width;
            int minY = height;
            int maxX = -1;
            int maxY = -1;

            for (int y = 0; y < height; y++)
            {
                int row = y * width;
                for (int x = 0; x < width; x++)
                {
                    if (pixels[row + x].a <= 12)
                        continue;

                    minX = Mathf.Min(minX, x);
                    minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x);
                    maxY = Mathf.Max(maxY, y);
                }
            }

            int requiredMargin = Mathf.Max(8, Mathf.Min(width, height) / 100);
            bool hasContent = maxX >= minX && maxY >= minY;
            bool touchesEdge =
                !hasContent ||
                minX < requiredMargin ||
                minY < requiredMargin ||
                maxX >= width - requiredMargin ||
                maxY >= height - requiredMargin;

            Object.DestroyImmediate(readable);

            if (touchesEdge)
            {
                errors.Add(
                    "Foliage alpha bounds are cropped or too close to an edge: " +
                    path);
            }
        }

        private static void ValidateShader(
            string shaderName,
            List<string> errors)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                errors.Add("Shader missing: " + shaderName);
                return;
            }

            if (!shader.isSupported)
                errors.Add("Shader is not supported by current pipeline: " + shaderName);
        }

        private static void ValidateGroundPrefab(
            string id,
            List<string> errors)
        {
            GameObject prefab = EnvironmentFoundationPackBuilder.LoadPrefab(id);
            if (prefab == null)
            {
                errors.Add("Ground prefab missing: " + id);
                return;
            }

            BoxCollider collider = prefab.GetComponent<BoxCollider>();
            if (collider == null || collider.isTrigger)
                errors.Add("Ground prefab has no solid BoxCollider: " + id);
            else if (collider.sharedMaterial == null)
                errors.Add("Ground prefab has no PhysicsMaterial: " + id);

            if (prefab.GetComponent<EnvironmentSurface>() == null)
                errors.Add("Ground prefab has no EnvironmentSurface: " + id);

            MeshRenderer renderer = prefab.GetComponentInChildren<MeshRenderer>(true);
            if (renderer == null || renderer.sharedMaterial == null)
                errors.Add("Ground prefab has no materialized visual: " + id);
        }

        private static void ValidateFoliagePrefab(
            string id,
            List<string> errors)
        {
            GameObject prefab = EnvironmentFoundationPackBuilder.LoadPrefab(id);
            if (prefab == null)
            {
                errors.Add("Foliage prefab missing: " + id);
                return;
            }

            Collider trigger = prefab.GetComponent<Collider>();
            if (trigger == null || !trigger.isTrigger)
                errors.Add("Foliage prefab has no interaction trigger: " + id);

            if (prefab.GetComponent<InteractiveFoliage>() == null)
                errors.Add("Foliage prefab has no InteractiveFoliage: " + id);

            if (prefab.GetComponent<FoliageGameplayVolume>() == null)
                errors.Add("Foliage prefab has no gameplay volume: " + id);

            SpriteRenderer renderer = prefab.GetComponentInChildren<SpriteRenderer>(true);
            if (renderer == null || renderer.sprite == null || renderer.sharedMaterial == null)
                errors.Add("Foliage prefab visual is incomplete: " + id);

            if (id == "Grass_Wildflowers_A" &&
                prefab.GetComponentInChildren<ParticleSystem>(true) == null)
            {
                errors.Add("Wildflower prefab has no pollen VFX: " + id);
            }
        }
    }
}
#endif
