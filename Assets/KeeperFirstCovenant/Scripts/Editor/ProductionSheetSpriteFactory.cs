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

                if (!importer.isReadable)
                {
                    importer.isReadable = true;
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

                if (asset is Sprite || asset is Texture2D)
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

        public static Sprite CreateExtractedCharacterSprite(
            Texture2D source,
            SheetRect sheetRect,
            string name,
            UnityEngine.Object owner,
            float pixelsPerUnit,
            Vector2 pivot)
        {
            if (source == null)
                return null;

            Rect sourceRect = sheetRect.ToSpriteRect(source);
            int x = Mathf.RoundToInt(sourceRect.x);
            int y = Mathf.RoundToInt(sourceRect.y);
            int width = Mathf.Max(1, Mathf.RoundToInt(sourceRect.width));
            int height = Mathf.Max(1, Mathf.RoundToInt(sourceRect.height));

            Color32[] pixels = source.GetPixels32();
            Color32[] crop = new Color32[width * height];
            bool[] candidate = new bool[crop.Length];

            for (int py = 0; py < height; py++)
            {
                int sourceY = y + py;

                for (int px = 0; px < width; px++)
                {
                    int sourceX = x + px;
                    Color32 color =
                        pixels[sourceY * source.width + sourceX];

                    int index = py * width + px;
                    crop[index] = color;

                    int luminance =
                        (color.r + color.g + color.b) / 3;

                    candidate[index] =
                        color.b - color.r < 10 ||
                        color.r - color.g > 5 ||
                        luminance > 115;
                }
            }

            int[] labels = new int[crop.Length];
            List<ComponentInfo> components =
                LabelComponents(
                    candidate,
                    crop,
                    labels,
                    width,
                    height);

            bool[] keep = SelectCharacterComponents(
                components,
                labels,
                width,
                height);

            Color32[] output =
                new Color32[crop.Length];

            for (int i = 0; i < crop.Length; i++)
            {
                Color32 color = crop[i];
                color.a = keep[i] ? (byte)255 : (byte)0;
                output[i] = color;
            }

            FeatherCharacterMask(
                output,
                keep,
                width,
                height);

            Texture2D extracted =
                new Texture2D(
                    width,
                    height,
                    TextureFormat.RGBA32,
                    false);

            extracted.name = name + "_Texture";
            extracted.wrapMode = TextureWrapMode.Clamp;
            extracted.filterMode = FilterMode.Bilinear;
            extracted.SetPixels32(output);
            extracted.Apply(false, false);

            AssetDatabase.AddObjectToAsset(
                extracted,
                owner);

            Sprite sprite =
                Sprite.Create(
                    extracted,
                    new Rect(
                        0f,
                        0f,
                        width,
                        height),
                    pivot,
                    Mathf.Max(1f, pixelsPerUnit),
                    0,
                    SpriteMeshType.FullRect);

            sprite.name = name;
            AssetDatabase.AddObjectToAsset(
                sprite,
                owner);

            return sprite;
        }

        public static Sprite[] CreateExtractedCharacterRow(
            Texture2D texture,
            string prefix,
            SheetRect[] rects,
            UnityEngine.Object owner,
            float pixelsPerUnit,
            Vector2 pivot)
        {
            if (rects == null)
                return Array.Empty<Sprite>();

            Sprite[] result =
                new Sprite[rects.Length];

            for (int i = 0; i < rects.Length; i++)
            {
                result[i] =
                    CreateExtractedCharacterSprite(
                        texture,
                        rects[i],
                        prefix + "_" + i.ToString("000"),
                        owner,
                        pixelsPerUnit,
                        pivot);
            }

            return result;
        }

        private sealed class ComponentInfo
        {
            public int Label;
            public int Area;
            public int MinX;
            public int MinY;
            public int MaxX;
            public int MaxY;
            public long Red;
            public long Green;
            public long Blue;

            public float CenterX =>
                (MinX + MaxX) * 0.5f;

            public float CenterY =>
                (MinY + MaxY) * 0.5f;

            public bool Warm =>
                Area > 0 &&
                Red / (float)Area >
                    Green / (float)Area + 8f &&
                Red / (float)Area >
                    Blue / (float)Area + 8f;
        }

        private static List<ComponentInfo> LabelComponents(
            bool[] candidate,
            Color32[] colors,
            int[] labels,
            int width,
            int height)
        {
            List<ComponentInfo> result =
                new List<ComponentInfo>();

            int nextLabel = 1;
            int[] queue = new int[candidate.Length];

            for (int start = 0; start < candidate.Length; start++)
            {
                if (!candidate[start] ||
                    labels[start] != 0)
                {
                    continue;
                }

                int head = 0;
                int tail = 0;
                queue[tail++] = start;
                labels[start] = nextLabel;

                ComponentInfo component =
                    new ComponentInfo
                    {
                        Label = nextLabel,
                        MinX = width,
                        MinY = height,
                        MaxX = 0,
                        MaxY = 0
                    };

                while (head < tail)
                {
                    int index = queue[head++];
                    int px = index % width;
                    int py = index / width;

                    component.Area++;
                    component.MinX =
                        Mathf.Min(component.MinX, px);
                    component.MinY =
                        Mathf.Min(component.MinY, py);
                    component.MaxX =
                        Mathf.Max(component.MaxX, px);
                    component.MaxY =
                        Mathf.Max(component.MaxY, py);

                    Color32 color = colors[index];
                    component.Red += color.r;
                    component.Green += color.g;
                    component.Blue += color.b;

                    for (int oy = -1; oy <= 1; oy++)
                    {
                        int ny = py + oy;
                        if (ny < 0 || ny >= height)
                            continue;

                        for (int ox = -1; ox <= 1; ox++)
                        {
                            if (ox == 0 && oy == 0)
                                continue;

                            int nx = px + ox;
                            if (nx < 0 || nx >= width)
                                continue;

                            int neighbor =
                                ny * width + nx;

                            if (!candidate[neighbor] ||
                                labels[neighbor] != 0)
                            {
                                continue;
                            }

                            labels[neighbor] = nextLabel;
                            queue[tail++] = neighbor;
                        }
                    }
                }

                result.Add(component);
                nextLabel++;
            }

            return result;
        }

        private static bool[] SelectCharacterComponents(
            List<ComponentInfo> components,
            int[] labels,
            int width,
            int height)
        {
            bool[] keep =
                new bool[labels.Length];

            if (components == null ||
                components.Count == 0)
            {
                return keep;
            }

            float expectedX = width * 0.5f;
            float expectedY = height * 0.48f;

            ComponentInfo main = null;
            float bestScore = float.MaxValue;

            foreach (ComponentInfo component in components)
            {
                if (component.Area < 8)
                    continue;

                float dx =
                    (component.CenterX - expectedX) /
                    Mathf.Max(1f, width * 0.55f);

                float dy =
                    (component.CenterY - expectedY) /
                    Mathf.Max(1f, height * 0.55f);

                float heightRatio =
                    (component.MaxY - component.MinY + 1f) /
                    Mathf.Max(1f, height);

                float areaRatio =
                    component.Area /
                    Mathf.Max(
                        1f,
                        width * height);

                bool touchesSide =
                    component.MinX <= 1 ||
                    component.MaxX >= width - 2;

                float score =
                    dx * dx +
                    dy * dy -
                    heightRatio * 0.7f -
                    areaRatio * 4f +
                    (touchesSide ? 0.9f : 0f);

                if (score < bestScore)
                {
                    bestScore = score;
                    main = component;
                }
            }

            if (main == null)
                return keep;

            HashSet<int> keptLabels =
                new HashSet<int>
                {
                    main.Label
                };

            foreach (ComponentInfo component in components)
            {
                if (component == main ||
                    component.Area < 6)
                {
                    continue;
                }

                int dx =
                    Mathf.Max(
                        0,
                        Mathf.Max(
                            main.MinX - component.MaxX,
                            component.MinX - main.MaxX));

                int dy =
                    Mathf.Max(
                        0,
                        Mathf.Max(
                            main.MinY - component.MaxY,
                            component.MinY - main.MaxY));

                bool touchesSide =
                    component.MinX <= 1 ||
                    component.MaxX >= width - 2;

                bool nearby =
                    !touchesSide &&
                    dx <= 5 &&
                    dy <= 8 &&
                    component.Area >= 10;

                bool effect =
                    component.Warm &&
                    component.Area >= 6;

                if (nearby || effect)
                    keptLabels.Add(component.Label);
            }

            for (int i = 0; i < labels.Length; i++)
            {
                if (keptLabels.Contains(labels[i]))
                    keep[i] = true;
            }

            return keep;
        }

        private static void FeatherCharacterMask(
            Color32[] pixels,
            bool[] keep,
            int width,
            int height)
        {
            Color32[] source =
                (Color32[])pixels.Clone();

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int index = y * width + x;
                    if (keep[index])
                        continue;

                    int neighbors = 0;

                    for (int oy = -1; oy <= 1; oy++)
                    {
                        for (int ox = -1; ox <= 1; ox++)
                        {
                            if (ox == 0 && oy == 0)
                                continue;

                            if (keep[
                                (y + oy) * width +
                                (x + ox)])
                            {
                                neighbors++;
                            }
                        }
                    }

                    if (neighbors <= 0)
                        continue;

                    Color32 color = source[index];
                    color.a =
                        (byte)Mathf.Clamp(
                            neighbors * 24,
                            0,
                            120);

                    pixels[index] = color;
                }
            }
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
