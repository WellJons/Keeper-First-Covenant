#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    public sealed class HighResFrameArtPostprocessor : AssetPostprocessor
    {
        private const string CharacterArtRoot =
            "Assets/KeeperFirstCovenant/Art/Characters/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(CharacterArtRoot))
                return;

            TextureImporter importer = assetImporter as TextureImporter;
            if (importer == null)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.spritePixelsPerUnit = 256f;
        }
    }
}
#endif
