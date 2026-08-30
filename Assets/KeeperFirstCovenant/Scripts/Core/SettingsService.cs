using System;
using System.IO;
using UnityEngine;

namespace KeeperFirstCovenant.Core
{
    public static class SettingsService
    {
        private const string FileName = "settings.json";
        private static bool loaded;

        public static GameSettingsData Current { get; private set; }
        public static event Action<GameSettingsData> SettingsChanged;

        private static string SettingsPath => Path.Combine(Application.persistentDataPath, FileName);

        public static GameSettingsData Load()
        {
            if (loaded && Current != null)
                return Current;

            loaded = true;
            Current = TryLoadFromDisk() ?? GameSettingsData.CreateDefaults();
            Current.Clamp();
            Apply(Current, false);
            return Current;
        }

        public static void SaveAndApply(GameSettingsData data)
        {
            Current = data != null ? data.Clone() : GameSettingsData.CreateDefaults();
            Current.Clamp();
            Apply(Current, true);
            SaveToDisk(Current);
            SettingsChanged?.Invoke(Current);
        }

        public static void ResetToDefaults()
        {
            SaveAndApply(GameSettingsData.CreateDefaults());
        }

        public static void ApplyCurrent()
        {
            Apply(Load(), false);
            SettingsChanged?.Invoke(Current);
        }

        private static GameSettingsData TryLoadFromDisk()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                    return null;

                string json = File.ReadAllText(SettingsPath);
                if (string.IsNullOrWhiteSpace(json))
                    return null;

                return JsonUtility.FromJson<GameSettingsData>(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Keeper settings could not be loaded. Defaults will be used. " + exception.Message);
                return null;
            }
        }

        private static void SaveToDisk(GameSettingsData data)
        {
            try
            {
                Directory.CreateDirectory(Application.persistentDataPath);
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Keeper settings could not be saved. " + exception.Message);
            }
        }

        private static void Apply(GameSettingsData data, bool applyResolution)
        {
            if (data == null)
                return;

            AudioListener.volume = data.masterVolume;

            int qualityCount = QualitySettings.names.Length;
            if (qualityCount > 0)
            {
                int quality = Mathf.Clamp(data.qualityLevel, 0, qualityCount - 1);
                if (QualitySettings.GetQualityLevel() != quality)
                    QualitySettings.SetQualityLevel(quality, true);
            }

            QualitySettings.vSyncCount = data.vSync ? 1 : 0;
            Application.targetFrameRate = data.vSync ? -1 : data.targetFrameRate;

            if (applyResolution)
                Screen.SetResolution(data.width, data.height, data.fullscreenMode);
        }
    }
}
