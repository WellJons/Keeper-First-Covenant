#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    internal readonly struct SheetRect
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Width;
        public readonly float Height;

        public SheetRect(float x, float y, float width, float height)
        {
            X = x / 1672f;
            Y = y / 941f;
            Width = width / 1672f;
            Height = height / 941f;
        }

        public Rect ToSpriteRect(Texture2D texture)
        {
            float x = Mathf.Round(X * texture.width);
            float top = Mathf.Round(Y * texture.height);
            float width = Mathf.Max(1f, Mathf.Round(Width * texture.width));
            float height = Mathf.Max(1f, Mathf.Round(Height * texture.height));
            float y = texture.height - top - height;

            x = Mathf.Clamp(x, 0f, texture.width - 1f);
            y = Mathf.Clamp(y, 0f, texture.height - 1f);
            width = Mathf.Clamp(width, 1f, texture.width - x);
            height = Mathf.Clamp(height, 1f, texture.height - y);

            return new Rect(x, y, width, height);
        }
    }

    internal static class ProductionSheetSpriteFactory
    {
        public static Texture2D LoadSheet(string assetPath)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (texture == null)
                throw new InvalidOperationException("Production sheet is missing: " + assetPath);

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                bool dirty = false;

                if (importer.textureType != TextureImporterType.Default)
                {
                    importer.textureType = TextureImporterType.Default;
                    dirty = true;
                }

                if (importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    dirty = true;
                }

                if (importer.wrapMode != TextureWrapMode.Clamp)
                {
                    importer.wrapMode = TextureWrapMode.Clamp;
                    dirty = true;
                }

                if (importer.filterMode != FilterMode.Bilinear)
                {
                    importer.filterMode = FilterMode.Bilinear;
                    dirty = true;
                }

                if (dirty)
                    importer.SaveAndReimport();

                texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            }

            return texture;
        }

        public static void ClearSpriteSubAssets(UnityEngine.Object owner)
        {
            if (owner == null)
                return;

            string path = AssetDatabase.GetAssetPath(owner);
            if (string.IsNullOrEmpty(path))
                return;

            UnityEngine.Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (UnityEngine.Object asset in all)
            {
                if (asset == null || asset == owner)
                    continue;

                if (asset is Sprite)
                    UnityEngine.Object.DestroyImmediate(asset, true);
            }
        }

        public static Sprite CreatePersistentSprite(
            Texture2D texture,
            SheetRect sheetRect,
            string name,
            UnityEngine.Object owner,
            float pixelsPerUnit,
            Vector2 pivot)
        {
            if (texture == null)
                return null;

            Rect rect = sheetRect.ToSpriteRect(texture);

            Sprite sprite = Sprite.Create(
                texture,
                rect,
                pivot,
                Mathf.Max(1f, pixelsPerUnit),
                0,
                SpriteMeshType.FullRect);

            sprite.name = name;
            AssetDatabase.AddObjectToAsset(sprite, owner);
            return sprite;
        }

        public static Sprite[] CreateRow(
            Texture2D texture,
            string prefix,
            SheetRect[] rects,
            UnityEngine.Object owner,
            float pixelsPerUnit,
            Vector2 pivot)
        {
            if (rects == null)
                return Array.Empty<Sprite>();

            Sprite[] result = new Sprite[rects.Length];

            for (int i = 0; i < rects.Length; i++)
            {
                result[i] = CreatePersistentSprite(
                    texture,
                    rects[i],
                    prefix + "_" + i.ToString("000"),
                    owner,
                    pixelsPerUnit,
                    pivot);
            }

            return result;
        }

        public static Sprite[] Repeat(Sprite sprite, int count)
        {
            if (sprite == null || count <= 0)
                return Array.Empty<Sprite>();

            Sprite[] result = new Sprite[count];
            for (int i = 0; i < result.Length; i++)
                result[i] = sprite;
            return result;
        }

        public static SheetRect[] EqualRow(
            float x,
            float y,
            float width,
            float height,
            int count,
            float insetX = 2f)
        {
            List<SheetRect> rects = new List<SheetRect>(count);
            float cell = width / Mathf.Max(1, count);

            for (int i = 0; i < count; i++)
            {
                float left = x + i * cell + insetX;
                float w = Mathf.Max(1f, cell - insetX * 2f);
                rects.Add(new SheetRect(left, y, w, height));
            }

            return rects.ToArray();
        }

        public static Material GetOrCreateSheetMaterial()
        {
            const string materialPath =
                "Assets/KeeperFirstCovenant/Materials/ProductionSheetCutout.mat";

            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material != null)
                return material;

            EnsureFolder("Assets/KeeperFirstCovenant", "Materials");

            Shader shader = Shader.Find("Keeper First Covenant/Production Sheet Cutout");
            if (shader == null)
                throw new InvalidOperationException(
                    "Production sheet cutout shader was not found.");

            material = new Material(shader)
            {
                name = "ProductionSheetCutout"
            };

            material.SetFloat("_KeyLuma", 0.34f);
            material.SetFloat("_BlueBias", 0.018f);
            material.SetFloat("_Softness", 0.055f);
            material.SetFloat("_AlphaCutoff", 0.02f);

            AssetDatabase.CreateAsset(material, materialPath);
            return material;
        }

        public static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
#endif
