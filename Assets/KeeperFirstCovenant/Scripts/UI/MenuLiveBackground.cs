using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KeeperFirstCovenant.UI
{
    public sealed class MenuLiveBackground : MonoBehaviour
    {
        private const string BundledBackgroundResource = "Menu/MainMenu_Background";

        [Header("Optional authored layers")]
        [SerializeField] private Sprite farBackground;
        [SerializeField] private Sprite midground;
        [SerializeField] private Sprite foreground;

        [Header("Motion")]
        [SerializeField] private float cameraDrift = 16f;
        [SerializeField] private float ringSpeed = 2.4f;
        [SerializeField] private int emberCount = 30;

        private RectTransform farArtRect;
        private RectTransform midArtRect;
        private RectTransform foregroundRect;
        private RawImage fogA;
        private RawImage fogB;
        private RectTransform ringA;
        private RectTransform ringB;
        private readonly List<Ember> embers = new List<Ember>();

        private float time;

        public void BuildIfNeeded()
        {
            if (transform.Find("GeneratedLiveBackground") != null)
                return;

            RectTransform root = MenuUiFactory.CreateRect("GeneratedLiveBackground", transform);
            MenuUiFactory.Stretch(root);
            root.SetAsFirstSibling();

            RawImage baseImage = root.gameObject.AddComponent<RawImage>();
            baseImage.texture = BuildGradientTexture(512, 256);
            baseImage.color = Color.white;
            baseImage.raycastTarget = false;

            bool hasPrimaryArt = false;

            if (farBackground != null)
            {
                BuildAuthoredLayer(root, farBackground, "FarArt", out farArtRect, 1.06f, 1f);
                hasPrimaryArt = true;
            }
            else
            {
                Texture2D bundledBackground = Resources.Load<Texture2D>(BundledBackgroundResource);
                if (bundledBackground != null)
                {
                    BuildAuthoredTextureLayer(
                        root,
                        bundledBackground,
                        "BundledMainMenuBackground",
                        out farArtRect,
                        1.06f,
                        1f);

                    hasPrimaryArt = true;
                }
            }

            BuildAuthoredLayer(root, midground, "MidgroundArt", out midArtRect, 1.08f, 0.55f);

            fogA = BuildFog(
                root,
                "FogCold",
                new Color(0.42f, 0.58f, 0.64f, hasPrimaryArt ? 0.065f : 0.10f),
                0.15f);

            fogB = BuildFog(
                root,
                "FogWarm",
                new Color(0.68f, 0.30f, 0.12f, hasPrimaryArt ? 0.045f : 0.075f),
                -0.11f);

            if (!hasPrimaryArt)
            {
                ringA = BuildRing(root, "SilverIncompleteRing", 520f, 0.72f, 302f);
                ringA.anchorMin = ringA.anchorMax = new Vector2(0.76f, 0.47f);
                ringA.anchoredPosition = Vector2.zero;

                ringB = BuildRing(root, "BrokenRestraintRing", 760f, 0.18f, 236f);
                ringB.anchorMin = ringB.anchorMax = new Vector2(0.79f, 0.45f);
                ringB.anchoredPosition = Vector2.zero;
            }

            BuildAuthoredLayer(root, foreground, "ForegroundArt", out foregroundRect, 1.10f, 0.82f);

            Image lowerShade = MenuUiFactory.CreateImage(
                "LowerShade",
                root,
                new Color(0f, 0f, 0f, hasPrimaryArt ? 0.18f : 0.30f));

            RectTransform shadeRect = lowerShade.rectTransform;
            shadeRect.anchorMin = new Vector2(0f, 0f);
            shadeRect.anchorMax = new Vector2(1f, 0.32f);
            shadeRect.offsetMin = Vector2.zero;
            shadeRect.offsetMax = Vector2.zero;
            lowerShade.raycastTarget = false;

            BuildEmbers(root);
        }

        private void Awake()
        {
            BuildIfNeeded();
        }

        private void Update()
        {
            time += Time.unscaledDeltaTime;

            float driftX = Mathf.Sin(time * 0.075f) * cameraDrift;
            float driftY = Mathf.Sin(time * 0.052f + 0.8f) * cameraDrift * 0.32f;

            if (farArtRect != null)
            {
                farArtRect.anchoredPosition = new Vector2(driftX * 0.22f, driftY * 0.20f);
                float breathe = 1.06f + Mathf.Sin(time * 0.045f) * 0.006f;
                farArtRect.localScale = Vector3.one * breathe;
            }

            if (midArtRect != null)
                midArtRect.anchoredPosition = new Vector2(driftX * 0.52f, driftY * 0.45f);

            if (foregroundRect != null)
                foregroundRect.anchoredPosition = new Vector2(driftX * 0.88f, driftY * 0.70f);

            if (fogA != null)
            {
                Rect uv = fogA.uvRect;
                uv.x += Time.unscaledDeltaTime * 0.0045f;
                uv.y = Mathf.Sin(time * 0.033f) * 0.02f;
                fogA.uvRect = uv;
            }

            if (fogB != null)
            {
                Rect uv = fogB.uvRect;
                uv.x -= Time.unscaledDeltaTime * 0.0032f;
                uv.y = Mathf.Cos(time * 0.027f) * 0.018f;
                fogB.uvRect = uv;
            }

            if (ringA != null)
                ringA.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(time * 0.11f) * ringSpeed);

            if (ringB != null)
                ringB.localRotation = Quaternion.Euler(0f, 0f, -10f + Mathf.Sin(time * 0.075f + 1f) * ringSpeed * 0.55f);

            UpdateEmbers();
        }

        private static void BuildAuthoredTextureLayer(
            RectTransform parent,
            Texture2D texture,
            string name,
            out RectTransform rect,
            float scale,
            float alpha)
        {
            if (texture == null)
            {
                rect = null;
                return;
            }

            RectTransform imageRect = MenuUiFactory.CreateRect(name, parent);
            MenuUiFactory.Stretch(imageRect, -80f, -50f, -80f, -50f);
            imageRect.localScale = Vector3.one * scale;

            RawImage image = imageRect.gameObject.AddComponent<RawImage>();
            image.texture = texture;
            image.color = new Color(1f, 1f, 1f, alpha);
            image.raycastTarget = false;

            rect = imageRect;
        }

        private static void BuildAuthoredLayer(
            RectTransform parent,
            Sprite sprite,
            string name,
            out RectTransform rect,
            float scale,
            float alpha)
        {
            if (sprite == null)
            {
                rect = null;
                return;
            }

            Image image = MenuUiFactory.CreateImage(name, parent, new Color(1f, 1f, 1f, alpha));
            rect = image.rectTransform;
            MenuUiFactory.Stretch(rect, -80f, -50f, -80f, -50f);
            rect.localScale = Vector3.one * scale;
            image.sprite = sprite;
            image.preserveAspect = false;
            image.raycastTarget = false;
        }

        private static RawImage BuildFog(RectTransform parent, string name, Color color, float verticalAnchor)
        {
            RectTransform rect = MenuUiFactory.CreateRect(name, parent);
            rect.anchorMin = new Vector2(0f, Mathf.Clamp01(0.36f + verticalAnchor));
            rect.anchorMax = new Vector2(1f, Mathf.Clamp01(0.83f + verticalAnchor));
            rect.offsetMin = new Vector2(-120f, 0f);
            rect.offsetMax = new Vector2(120f, 0f);

            RawImage image = rect.gameObject.AddComponent<RawImage>();
            image.texture = BuildFogTexture(256, 96, name.GetHashCode());
            image.color = color;
            image.uvRect = new Rect(0f, 0f, 2.2f, 1f);
            image.raycastTarget = false;
            return image;
        }

        private static RectTransform BuildRing(
            RectTransform parent,
            string name,
            float size,
            float alpha,
            float arcDegrees)
        {
            RawImage image = MenuUiFactory.CreateRect(name, parent).gameObject.AddComponent<RawImage>();
            image.texture = BuildRingTexture(384, arcDegrees);
            image.color = new Color(MainMenuTheme.Silver.r, MainMenuTheme.Silver.g, MainMenuTheme.Silver.b, alpha);
            image.raycastTarget = false;

            RectTransform rect = image.rectTransform;
            rect.sizeDelta = new Vector2(size, size);
            return rect;
        }

        private void BuildEmbers(RectTransform parent)
        {
            emberCount = Mathf.Clamp(emberCount, 8, 80);

            for (int i = 0; i < emberCount; i++)
            {
                Image image = MenuUiFactory.CreateImage(
                    "Ember_" + i,
                    parent,
                    new Color(MainMenuTheme.Warm.r, MainMenuTheme.Warm.g, MainMenuTheme.Warm.b, Random.Range(0.12f, 0.52f)));

                image.raycastTarget = false;
                RectTransform rect = image.rectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 0f);
                rect.pivot = new Vector2(0.5f, 0.5f);

                float size = Random.Range(1.5f, 4.5f);
                rect.sizeDelta = new Vector2(size, size);

                embers.Add(new Ember
                {
                    rect = rect,
                    normalized = new Vector2(Random.value, Random.Range(-0.15f, 0.75f)),
                    speed = Random.Range(0.006f, 0.020f),
                    sway = Random.Range(6f, 28f),
                    phase = Random.Range(0f, Mathf.PI * 2f)
                });
            }
        }

        private void UpdateEmbers()
        {
            RectTransform self = transform as RectTransform;
            Vector2 size = self != null ? self.rect.size : new Vector2(Screen.width, Screen.height);
            if (size.x <= 1f || size.y <= 1f)
                size = new Vector2(Screen.width, Screen.height);

            for (int i = 0; i < embers.Count; i++)
            {
                Ember ember = embers[i];
                ember.normalized.y += ember.speed * Time.unscaledDeltaTime;

                if (ember.normalized.y > 1.10f)
                {
                    ember.normalized.y = -0.10f;
                    ember.normalized.x = Random.value;
                }

                float x = ember.normalized.x * size.x;
                x += Mathf.Sin(time * 0.8f + ember.phase) * ember.sway;
                float y = ember.normalized.y * size.y;

                ember.rect.anchoredPosition = new Vector2(x, y);
                embers[i] = ember;
            }
        }

        private static Texture2D BuildGradientTexture(int width, int height)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                float v = y / (float)(height - 1);

                for (int x = 0; x < width; x++)
                {
                    float u = x / (float)(width - 1);
                    Color baseColor = Color.Lerp(
                        MainMenuTheme.BackgroundWarm,
                        MainMenuTheme.BackgroundCold,
                        Mathf.SmoothStep(0f, 1f, v));

                    float warmGlow = Radial(u, v, 0.18f, 0.18f, 0.44f) * 0.34f;
                    float coldGlow = Radial(u, v, 0.78f, 0.52f, 0.50f) * 0.22f;

                    Color color = baseColor;
                    color += new Color(0.22f, 0.07f, 0.01f, 0f) * warmGlow;
                    color += new Color(0.05f, 0.12f, 0.15f, 0f) * coldGlow;

                    float grain = (Mathf.PerlinNoise(u * 14f, v * 14f) - 0.5f) * 0.018f;
                    color.r += grain;
                    color.g += grain;
                    color.b += grain;
                    color.a = 1f;

                    pixels[y * width + x] = color;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);
            return texture;
        }

        private static Texture2D BuildFogTexture(int width, int height, int seed)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Bilinear;

            Color[] pixels = new Color[width * height];
            float seedX = Mathf.Abs(seed % 997) * 0.013f;
            float seedY = Mathf.Abs(seed % 457) * 0.017f;

            for (int y = 0; y < height; y++)
            {
                float v = y / (float)(height - 1);
                float vertical = Mathf.Sin(v * Mathf.PI);

                for (int x = 0; x < width; x++)
                {
                    float u = x / (float)(width - 1);
                    float noiseA = Mathf.PerlinNoise(seedX + u * 3.8f, seedY + v * 2.1f);
                    float noiseB = Mathf.PerlinNoise(seedY + u * 7.2f, seedX + v * 4.6f);
                    float alpha = Mathf.Clamp01((noiseA * 0.68f + noiseB * 0.32f - 0.36f) * 1.8f) * vertical;
                    pixels[y * width + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);
            return texture;
        }

        private static Texture2D BuildRingTexture(int size, float arcDegrees)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            Color[] pixels = new Color[size * size];
            Vector2 center = new Vector2(0.5f, 0.5f);

            for (int y = 0; y < size; y++)
            {
                float v = y / (float)(size - 1);

                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)(size - 1);
                    Vector2 delta = new Vector2(u, v) - center;
                    float radius = delta.magnitude;
                    float ring = 1f - Mathf.Clamp01(Mathf.Abs(radius - 0.44f) / 0.010f);

                    float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
                    if (angle < 0f)
                        angle += 360f;

                    float arc = angle <= arcDegrees ? 1f : 0f;
                    float alpha = ring * arc;

                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);
            return texture;
        }

        private static float Radial(float u, float v, float cx, float cy, float radius)
        {
            float distance = Vector2.Distance(new Vector2(u, v), new Vector2(cx, cy));
            return 1f - Mathf.Clamp01(distance / Mathf.Max(0.0001f, radius));
        }

        private struct Ember
        {
            public RectTransform rect;
            public Vector2 normalized;
            public float speed;
            public float sway;
            public float phase;
        }
    }
}
