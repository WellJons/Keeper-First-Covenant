#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    internal sealed class CelSpritePainter
    {
        private readonly int _width;
        private readonly int _height;
        private readonly Color32[] _pixels;

        public int Width => _width;
        public int Height => _height;

        public CelSpritePainter(int width, int height)
        {
            _width = Mathf.Max(8, width);
            _height = Mathf.Max(8, height);
            _pixels = new Color32[_width * _height];
            Clear();
        }

        public void Clear()
        {
            Array.Fill(_pixels, new Color32(0, 0, 0, 0));
        }

        public void Set(int x, int y, Color color)
        {
            if ((uint)x >= (uint)_width || (uint)y >= (uint)_height)
                return;
            _pixels[y * _width + x] = color;
        }

        public void Rect(int x, int y, int width, int height, Color color)
        {
            int x0 = Mathf.Clamp(x, 0, _width);
            int y0 = Mathf.Clamp(y, 0, _height);
            int x1 = Mathf.Clamp(x + width, 0, _width);
            int y1 = Mathf.Clamp(y + height, 0, _height);

            for (int py = y0; py < y1; py++)
                for (int px = x0; px < x1; px++)
                    Set(px, py, color);
        }

        public void Ellipse(Vector2 center, Vector2 radii, Color color)
        {
            int minX = Mathf.FloorToInt(center.x - radii.x);
            int maxX = Mathf.CeilToInt(center.x + radii.x);
            int minY = Mathf.FloorToInt(center.y - radii.y);
            int maxY = Mathf.CeilToInt(center.y + radii.y);

            float rx = Mathf.Max(1f, radii.x);
            float ry = Mathf.Max(1f, radii.y);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float dx = (x - center.x) / rx;
                    float dy = (y - center.y) / ry;
                    if (dx * dx + dy * dy <= 1f)
                        Set(x, y, color);
                }
            }
        }

        public void Ring(Vector2 center, Vector2 radii, float thickness, Color color)
        {
            float outerRx = Mathf.Max(1f, radii.x);
            float outerRy = Mathf.Max(1f, radii.y);
            float innerRx = Mathf.Max(1f, outerRx - thickness);
            float innerRy = Mathf.Max(1f, outerRy - thickness);

            int minX = Mathf.FloorToInt(center.x - outerRx);
            int maxX = Mathf.CeilToInt(center.x + outerRx);
            int minY = Mathf.FloorToInt(center.y - outerRy);
            int maxY = Mathf.CeilToInt(center.y + outerRy);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float odx = (x - center.x) / outerRx;
                    float ody = (y - center.y) / outerRy;
                    float idx = (x - center.x) / innerRx;
                    float idy = (y - center.y) / innerRy;
                    bool outer = odx * odx + ody * ody <= 1f;
                    bool inner = idx * idx + idy * idy <= 1f;
                    if (outer && !inner)
                        Set(x, y, color);
                }
            }
        }

        public void Polygon(Color color, params Vector2[] points)
        {
            if (points == null || points.Length < 3)
                return;

            float minXF = points[0].x;
            float maxXF = points[0].x;
            float minYF = points[0].y;
            float maxYF = points[0].y;

            for (int i = 1; i < points.Length; i++)
            {
                minXF = Mathf.Min(minXF, points[i].x);
                maxXF = Mathf.Max(maxXF, points[i].x);
                minYF = Mathf.Min(minYF, points[i].y);
                maxYF = Mathf.Max(maxYF, points[i].y);
            }

            int minX = Mathf.FloorToInt(minXF);
            int maxX = Mathf.CeilToInt(maxXF);
            int minY = Mathf.FloorToInt(minYF);
            int maxY = Mathf.CeilToInt(maxYF);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (Contains(points, new Vector2(x + 0.5f, y + 0.5f)))
                        Set(x, y, color);
                }
            }
        }

        public void Line(Vector2 a, Vector2 b, float thickness, Color color)
        {
            float distance = Vector2.Distance(a, b);
            int steps = Mathf.Max(1, Mathf.CeilToInt(distance * 1.35f));
            float radius = Mathf.Max(0.8f, thickness * 0.5f);

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector2 point = Vector2.Lerp(a, b, t);
                Ellipse(point, Vector2.one * radius, color);
            }
        }

        public void GlowLine(Vector2 a, Vector2 b, Color core, Color glow)
        {
            Color soft = glow;
            soft.a *= 0.22f;
            Line(a, b, 8f, soft);
            soft.a = glow.a * 0.45f;
            Line(a, b, 4f, soft);
            Line(a, b, 1.7f, core);
        }

        public void Speckles(int seed, Rect area, int count, Color color, float minRadius = 0.6f, float maxRadius = 1.8f)
        {
            System.Random rng = new System.Random(seed);
            for (int i = 0; i < count; i++)
            {
                float x = Mathf.Lerp(area.xMin, area.xMax, (float)rng.NextDouble());
                float y = Mathf.Lerp(area.yMin, area.yMax, (float)rng.NextDouble());
                float r = Mathf.Lerp(minRadius, maxRadius, (float)rng.NextDouble());
                Color c = color;
                c.a *= Mathf.Lerp(0.35f, 1f, (float)rng.NextDouble());
                Ellipse(new Vector2(x, y), Vector2.one * r, c);
            }
        }

        public Texture2D ToTexture()
        {
            Texture2D texture = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
            texture.name = "GeneratedCelSprite";
            texture.SetPixels32(_pixels);
            texture.Apply(false, false);
            return texture;
        }

        public Sprite SaveAsSprite(string assetPath, float pixelsPerUnit, Vector2 pivot)
        {
            EnsureFolderForAsset(assetPath);

            Texture2D texture = ToTexture();
            File.WriteAllBytes(Path.GetFullPath(assetPath), texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                return null;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePivot = pivot;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        public Texture2D SaveAsTexture(string assetPath)
        {
            EnsureFolderForAsset(assetPath);
            Texture2D texture = ToTexture();
            File.WriteAllBytes(Path.GetFullPath(assetPath), texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }

        private static bool Contains(Vector2[] polygon, Vector2 point)
        {
            bool inside = false;
            int j = polygon.Length - 1;

            for (int i = 0; i < polygon.Length; j = i++)
            {
                Vector2 pi = polygon[i];
                Vector2 pj = polygon[j];
                bool intersects = ((pi.y > point.y) != (pj.y > point.y)) &&
                                  (point.x < (pj.x - pi.x) * (point.y - pi.y) /
                                   Mathf.Max(0.00001f, pj.y - pi.y) + pi.x);
                if (intersects)
                    inside = !inside;
            }

            return inside;
        }

        private static void EnsureFolderForAsset(string path)
        {
            string directory = Path.GetDirectoryName(path)?.Replace("\\", "/");
            if (string.IsNullOrEmpty(directory))
                return;

            string[] parts = directory.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
