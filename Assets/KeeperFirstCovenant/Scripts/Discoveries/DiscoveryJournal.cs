using System;
using System.Collections.Generic;
using System.Linq;
using KeeperFirstCovenant.World;
using UnityEngine;

namespace KeeperFirstCovenant.Discoveries
{
    public enum DiscoveryCategory
    {
        Location,
        Lore,
        Person,
        Faction,
        Creature,
        Magic,
        Clue
    }

    [Serializable]
    public sealed class DiscoveryEntryState
    {
        public string discoveryId;
        public string title;
        public string description;
        public DiscoveryCategory category;
        public string locationName;
        public int discoveredDay = 1;
        public float discoveredHour = 9f;
    }

    [Serializable]
    public sealed class DiscoveryJournalSnapshot
    {
        public int version = 1;
        public List<DiscoveryEntryState> entries =
            new List<DiscoveryEntryState>();
    }

    public sealed class DiscoveryJournal : MonoBehaviour
    {
        private static DiscoveryJournal instance;

        [SerializeField]
        private List<DiscoveryEntryState> entries =
            new List<DiscoveryEntryState>();

        public static DiscoveryJournal Instance
        {
            get
            {
                EnsureExists();
                return instance;
            }
        }

        public static DiscoveryJournal Current => instance;

        public IReadOnlyList<DiscoveryEntryState> Entries =>
            entries;

        public event Action Changed;
        public event Action<DiscoveryEntryState> Discovered;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            EnsureExists();
        }

        public static void EnsureExists()
        {
            if (instance != null)
                return;

            instance =
                FindFirstObjectByType<
                    DiscoveryJournal>();

            if (instance != null)
                return;

            GameObject root =
                new GameObject(
                    "Keeper_DiscoveryJournal");

            instance =
                root.AddComponent<
                    DiscoveryJournal>();
        }

        private void Awake()
        {
            if (instance != null &&
                instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public DiscoveryEntryState Discover(
            string discoveryId,
            string title,
            string description,
            DiscoveryCategory category,
            string locationName = null)
        {
            if (string.IsNullOrWhiteSpace(
                    discoveryId))
            {
                return null;
            }

            DiscoveryEntryState existing =
                Find(discoveryId);

            if (existing != null)
                return existing;

            WorldTimeSystem time =
                WorldTimeSystem.Instance;

            var entry =
                new DiscoveryEntryState
                {
                    discoveryId =
                        discoveryId,
                    title =
                        string.IsNullOrWhiteSpace(
                            title)
                            ? discoveryId
                            : title,
                    description =
                        description ?? string.Empty,
                    category =
                        category,
                    locationName =
                        locationName ?? string.Empty,
                    discoveredDay =
                        time != null
                            ? time.Day
                            : 1,
                    discoveredHour =
                        time != null
                            ? time.Hour
                            : 9f
                };

            entries.Add(entry);

            Discovered?.Invoke(entry);
            Changed?.Invoke();

            return entry;
        }

        public bool HasDiscovery(
            string discoveryId)
        {
            return Find(discoveryId) != null;
        }

        public DiscoveryEntryState Find(
            string discoveryId)
        {
            if (string.IsNullOrWhiteSpace(
                    discoveryId))
            {
                return null;
            }

            return entries.FirstOrDefault(
                value =>
                    value != null &&
                    string.Equals(
                        value.discoveryId,
                        discoveryId,
                        StringComparison.Ordinal));
        }

        public string CaptureJson()
        {
            var snapshot =
                new DiscoveryJournalSnapshot
                {
                    entries =
                        entries
                            .Where(value =>
                                value != null)
                            .Select(Clone)
                            .ToList()
                };

            return JsonUtility.ToJson(snapshot);
        }

        public void RestoreJson(
            string json)
        {
            entries.Clear();

            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    DiscoveryJournalSnapshot snapshot =
                        JsonUtility.FromJson<
                            DiscoveryJournalSnapshot>(
                            json);

                    if (snapshot?.entries != null)
                    {
                        entries =
                            snapshot.entries
                                .Where(value =>
                                    value != null &&
                                    !string.IsNullOrWhiteSpace(
                                        value.discoveryId))
                                .GroupBy(
                                    value =>
                                        value.discoveryId,
                                    StringComparer.Ordinal)
                                .Select(group =>
                                    Clone(
                                        group.First()))
                                .ToList();
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(
                        "Discovery journal could not be restored. " +
                        exception.Message);
                }
            }

            Changed?.Invoke();
        }

        public void ResetJournal()
        {
            entries.Clear();
            Changed?.Invoke();
        }

        private static DiscoveryEntryState Clone(
            DiscoveryEntryState source)
        {
            return new DiscoveryEntryState
            {
                discoveryId =
                    source.discoveryId,
                title =
                    source.title,
                description =
                    source.description,
                category =
                    source.category,
                locationName =
                    source.locationName,
                discoveredDay =
                    Mathf.Max(
                        1,
                        source.discoveredDay),
                discoveredHour =
                    Mathf.Repeat(
                        source.discoveredHour,
                        24f)
            };
        }
    }
}
