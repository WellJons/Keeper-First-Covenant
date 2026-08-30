#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    [InitializeOnLoad]
    public static class PackedArtHydrator
    {
        [Serializable]
        private sealed class PackedArtManifest
        {
            public PackedArtEntry[] assets;
        }

        [Serializable]
        private sealed class PackedArtEntry
        {
            public string id;
            public string targetPath;
            public string chunkFolder;
            public string chunkPrefix;
            public int chunkCount;
            public string sha256;
        }

        private const string ManifestPath =
            "Assets/KeeperFirstCovenant/ArtPacked/manifest.json";

        private static bool _scheduled;

        static PackedArtHydrator()
        {
            ScheduleHydration();
        }

        [MenuItem("Keeper First Covenant/High-Res 2D/Hydrate Packed Art Assets")]
        public static void HydrateFromMenu()
        {
            HydrateAll(true);
        }

        private static void ScheduleHydration()
        {
            if (_scheduled)
                return;

            _scheduled = true;
            EditorApplication.delayCall += () =>
            {
                _scheduled = false;
                HydrateAll(false);
            };
        }

        private static void HydrateAll(bool verbose)
        {
            if (!File.Exists(ManifestPath))
                return;

            string json = File.ReadAllText(ManifestPath);
            PackedArtManifest manifest =
                JsonUtility.FromJson<PackedArtManifest>(json);

            if (manifest == null || manifest.assets == null)
                return;

            bool wroteAny = false;
            List<string> errors = new List<string>();

            foreach (PackedArtEntry entry in manifest.assets)
            {
                if (entry == null)
                    continue;

                try
                {
                    if (Hydrate(entry, verbose))
                        wroteAny = true;
                }
                catch (Exception ex)
                {
                    errors.Add(entry.id + ": " + ex.Message);
                }
            }

            if (wroteAny)
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            if (errors.Count > 0)
            {
                Debug.LogError(
                    "Packed art hydration failed:\n - " +
                    string.Join("\n - ", errors));
            }
            else if (verbose)
            {
                Debug.Log(
                    "Packed art hydration complete. " +
                    "Repository art payloads are materialized as Unity textures.");
            }
        }

        private static bool Hydrate(
            PackedArtEntry entry,
            bool verbose)
        {
            if (string.IsNullOrWhiteSpace(entry.targetPath) ||
                string.IsNullOrWhiteSpace(entry.chunkFolder) ||
                string.IsNullOrWhiteSpace(entry.chunkPrefix) ||
                entry.chunkCount <= 0)
            {
                throw new InvalidOperationException(
                    "Invalid packed art manifest entry.");
            }

            if (File.Exists(entry.targetPath) &&
                VerifySha256(entry.targetPath, entry.sha256))
            {
                return false;
            }

            StringBuilder base64 = new StringBuilder();

            for (int i = 0; i < entry.chunkCount; i++)
            {
                string chunkPath = Path.Combine(
                        entry.chunkFolder,
                        entry.chunkPrefix + "_" +
                        i.ToString("D2") +
                        ".b64part")
                    .Replace("\\", "/");

                if (!File.Exists(chunkPath))
                {
                    throw new FileNotFoundException(
                        "Missing packed art chunk: " +
                        chunkPath);
                }

                string chunk =
                    File.ReadAllText(chunkPath).Trim();

                base64.Append(chunk);
            }

            byte[] bytes =
                Convert.FromBase64String(base64.ToString());

            string directory =
                Path.GetDirectoryName(entry.targetPath);

            if (!string.IsNullOrEmpty(directory) &&
                !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(entry.targetPath, bytes);

            if (!VerifySha256(
                    entry.targetPath,
                    entry.sha256))
            {
                File.Delete(entry.targetPath);
                throw new InvalidDataException(
                    "SHA-256 mismatch after hydration.");
            }

            if (verbose)
            {
                Debug.Log(
                    "Hydrated " + entry.id +
                    " -> " + entry.targetPath);
            }

            return true;
        }

        private static bool VerifySha256(
            string filePath,
            string expected)
        {
            if (string.IsNullOrWhiteSpace(expected))
                return File.Exists(filePath);

            if (!File.Exists(filePath))
                return false;

            using (SHA256 sha = SHA256.Create())
            using (FileStream stream =
                   File.OpenRead(filePath))
            {
                byte[] hash = sha.ComputeHash(stream);
                string actual =
                    BitConverter
                        .ToString(hash)
                        .Replace("-", "")
                        .ToLowerInvariant();

                return string.Equals(
                    actual,
                    expected.Trim().ToLowerInvariant(),
                    StringComparison.Ordinal);
            }
        }
    }
}
#endif
