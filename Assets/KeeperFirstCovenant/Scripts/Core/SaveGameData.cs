using System;

namespace KeeperFirstCovenant.Core
{
    [Serializable]
    public sealed class SaveGameData
    {
        public int version = 2;
        public int slotId;

        public string saveGuid;
        public string displayName;
        public string sceneName;
        public string locationName;

        public string createdUtc;
        public string modifiedUtc;

        public float playTimeSeconds;
        public bool manualSave = true;

        // Reserved extension points. These stay strings so the shell does not
        // couple the save system to unfinished gameplay systems.
        public string worldStateJson = "";
        public string partyStateJson = "";
        public string questStateJson = "";
        public string discoveryStateJson = "";
        public string thumbnailRelativePath = "";

        public DateTime GetModifiedUtc()
        {
            return DateTime.TryParse(
                modifiedUtc,
                null,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out DateTime parsed)
                ? parsed.ToUniversalTime()
                : DateTime.MinValue;
        }
    }
}
