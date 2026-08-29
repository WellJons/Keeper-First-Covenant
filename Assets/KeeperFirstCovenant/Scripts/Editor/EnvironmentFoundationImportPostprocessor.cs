#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    public sealed class EnvironmentFoundationImportPostprocessor : AssetPostprocessor
    {
        private const string GroundRoot =
            "Assets/KeeperFirstCovenant/Art/Environment/Foundation/Ground/";

        private const string FoliageRoot =
            "Assets/KeeperFirstCovenant/Art/Environment/Foundation/Foliage/";

        private void OnPreprocessTexture()
        {
            TextureImporter importer = assetImporter as TextureImporter;
            if (importer == null)
                return;

            if (assetPath.StartsWith(GroundRoot))
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.alphaSource = TextureImporterAlphaSource.None;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.mipmapEnabled = true;
                importer.streamingMipmaps = true;
                importer.filterMode = FilterMode.Trilinear;
                importer.anisoLevel = 4;
                importer.wrapMode = TextureWrapMode.Mirror;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.maxTextureSize = 2048;
                return;
            }

            if (!assetPath.StartsWith(FoliageRoot))
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.sRGBTexture = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.maxTextureSize = 2048;
            importer.spritePixelsPerUnit = 512f;

            TextureImporterSettings spriteSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(spriteSettings);
            spriteSettings.spriteAlignment = (int)SpriteAlignment.Custom;
            spriteSettings.spritePivot = new Vector2(0.5f, 0.02f);
            importer.SetTextureSettings(spriteSettings);
        }
    }
}
#endif
