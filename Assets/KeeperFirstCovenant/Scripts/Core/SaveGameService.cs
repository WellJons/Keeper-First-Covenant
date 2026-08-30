using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace KeeperFirstCovenant.Core
{
    public static class SaveGameService
    {
        public const int MaxSlots = 6;
        private const string DirectoryName = "Saves";

        private static string SaveDirectory => Path.Combine(Application.persistentDataPath, DirectoryName);

        public static SaveGameData CreateNewGame(string startScene, string locationName)
        {
            int freeSlot = GetFirstFreeSlot();
            if (freeSlot < 1)
                return null;

            string now = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
            var data = new SaveGameData
            {
                slotId = freeSlot,
                saveGuid = Guid.NewGuid().ToString("N"),
                displayName = "Новая игра",
                sceneName = startScene,
                locationName = string.IsNullOrWhiteSpace(locationName) ? "Начало пути" : locationName,
                createdUtc = now,
                modifiedUtc = now,
                playTimeSeconds = 0f,
                manualSave = true
            };

            WriteSave(data);
            return data;
        }

        public static bool WriteSave(SaveGameData data)
        {
            if (data == null || data.slotId < 1 || data.slotId > MaxSlots)
                return false;

            try
            {
                Directory.CreateDirectory(SaveDirectory);

                if (string.IsNullOrWhiteSpace(data.createdUtc))
                    data.createdUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);

                data.modifiedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);

                string json = JsonUtility.ToJson(data, true);
                string path = GetSlotPath(data.slotId);
                string tempPath = path + ".tmp";

                File.WriteAllText(tempPath, json);

                if (File.Exists(path))
                    File.Delete(path);

                File.Move(tempPath, path);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Keeper save could not be written. " + exception.Message);
                return false;
            }
        }

        public static SaveGameData LoadSlot(int slotId)
        {
            if (slotId < 1 || slotId > MaxSlots)
                return null;

            string path = GetSlotPath(slotId);
            if (!File.Exists(path))
                return null;

            try
            {
                string json = File.ReadAllText(path);
                SaveGameData data = JsonUtility.FromJson<SaveGameData>(json);

                if (data == null)
                    return null;

                data.slotId = slotId;
                return data;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Keeper save slot {slotId} is invalid. {exception.Message}");
                return null;
            }
        }

        public static List<SaveGameData> GetAllSlots()
        {
            var result = new List<SaveGameData>(MaxSlots);

            for (int slot = 1; slot <= MaxSlots; slot++)
            {
                SaveGameData data = LoadSlot(slot);
                if (data != null)
                    result.Add(data);
            }

            return result.OrderBy(data => data.slotId).ToList();
        }

        public static SaveGameData GetLatestSave()
        {
            return GetAllSlots()
                .OrderByDescending(data => data.GetModifiedUtc())
                .FirstOrDefault();
        }

        public static bool HasAnySave()
        {
            return GetLatestSave() != null;
        }

        public static int GetFirstFreeSlot()
        {
            for (int slot = 1; slot <= MaxSlots; slot++)
            {
                if (!File.Exists(GetSlotPath(slot)))
                    return slot;
            }

            return -1;
        }

        public static bool DeleteSlot(int slotId)
        {
            if (slotId < 1 || slotId > MaxSlots)
                return false;

            try
            {
                string path = GetSlotPath(slotId);
                if (File.Exists(path))
                    File.Delete(path);

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Keeper save slot {slotId} could not be deleted. {exception.Message}");
                return false;
            }
        }

        public static bool UpdateLocation(int slotId, string sceneName, string locationName, float playTimeSeconds)
        {
            SaveGameData data = LoadSlot(slotId);
            if (data == null)
                return false;

            if (!string.IsNullOrWhiteSpace(sceneName))
                data.sceneName = sceneName;

            if (!string.IsNullOrWhiteSpace(locationName))
                data.locationName = locationName;

            data.playTimeSeconds = Mathf.Max(0f, playTimeSeconds);
            return WriteSave(data);
        }

        public static string FormatTimestamp(SaveGameData data)
        {
            if (data == null)
                return string.Empty;

            DateTime utc = data.GetModifiedUtc();
            if (utc == DateTime.MinValue)
                return "Дата неизвестна";

            return utc.ToLocalTime().ToString("dd.MM.yyyy  HH:mm");
        }

        public static string FormatPlayTime(float seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(Math.Max(0d, seconds));
            if (time.TotalHours >= 1d)
                return $"{(int)time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}";

            return $"{time.Minutes:00}:{time.Seconds:00}";
        }

        private static string GetSlotPath(int slotId)
        {
            return Path.Combine(SaveDirectory, $"save_{slotId:00}.json");
        }
    }
}
