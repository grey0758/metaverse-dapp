using System.IO;
using UnityEditor;
using UnityEngine;

namespace MetaverseGame.Editor
{
    /// <summary>
    /// Keeps authored boardroom textures within the mobile memory budget while
    /// retaining a sharper standalone presentation.
    /// </summary>
    public sealed class BoardroomAssetPipeline : AssetPostprocessor
    {
        private const string BoardroomTexturePrefix =
            "Assets/MetaverseGame/Resources/Boardroom/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(BoardroomTexturePrefix) ||
                Path.GetExtension(assetPath).ToLowerInvariant() != ".png")
            {
                return;
            }

            TextureImporter importer = (TextureImporter)assetImporter;
            bool isDisplay = Path.GetFileNameWithoutExtension(assetPath)
                .Equals("StrategyDisplay", System.StringComparison.OrdinalIgnoreCase);

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.isReadable = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.anisoLevel = isDisplay ? 1 : 2;
            importer.wrapMode = isDisplay ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;
            importer.maxTextureSize = isDisplay ? 2048 : 1024;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.compressionQuality = 50;
            importer.crunchedCompression = false;

            SetPlatform(
                importer,
                "Standalone",
                isDisplay ? 2048 : 1024,
                TextureImporterFormat.BC7,
                TextureImporterCompression.Compressed);
            SetPlatform(
                importer,
                "Android",
                isDisplay ? 2048 : 1024,
                TextureImporterFormat.ASTC_6x6,
                TextureImporterCompression.Compressed);
            SetPlatform(
                importer,
                "iPhone",
                isDisplay ? 2048 : 1024,
                TextureImporterFormat.ASTC_6x6,
                TextureImporterCompression.Compressed);
        }

        [MenuItem("Metaverse DApp/Validate Boardroom Textures")]
        public static void ValidateBoardroomTextures()
        {
            string[] paths = AssetDatabase.FindAssets(
                "t:Texture2D",
                new[] { BoardroomTexturePrefix.TrimEnd('/') });

            if (paths.Length != 3)
            {
                throw new System.InvalidOperationException(
                    $"Expected 3 boardroom textures, found {paths.Length}.");
            }

            foreach (string guid in paths)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                TextureImporterPlatformSettings android = importer?.GetPlatformTextureSettings("Android");
                if (importer == null ||
                    importer.isReadable ||
                    !importer.mipmapEnabled ||
                    android == null ||
                    !android.overridden ||
                    android.maxTextureSize > 2048)
                {
                    throw new System.InvalidOperationException(
                        $"Boardroom texture import settings are invalid: {path}");
                }
            }

            Debug.Log($"Validated {paths.Length} boardroom textures.");
        }

        private static void SetPlatform(
            TextureImporter importer,
            string platform,
            int maxSize,
            TextureImporterFormat format,
            TextureImporterCompression compression)
        {
            TextureImporterPlatformSettings settings =
                importer.GetPlatformTextureSettings(platform);
            settings.name = platform;
            settings.overridden = true;
            settings.maxTextureSize = maxSize;
            settings.format = format;
            settings.textureCompression = compression;
            settings.compressionQuality = 50;
            settings.crunchedCompression = false;
            importer.SetPlatformTextureSettings(settings);
        }
    }
}
