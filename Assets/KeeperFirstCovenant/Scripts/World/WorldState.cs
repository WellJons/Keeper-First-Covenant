using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KeeperFirstCovenant.World
{
    [Serializable]
    public sealed class WorldStateIntEntry
    {
        public string key;
        public int value;
    }

    [Serializable]
    public sealed class WorldStateSnapshot
    {
        public List<string> flags = new List<string>();
        public List<WorldStateIntEntry> values = new List<WorldStateIntEntry>();
    }

    public sealed class WorldState : MonoBehaviour
    {
        public static WorldState Instance { get; private set; }

        private readonly HashSet<string> _flags = new HashSet<string>();
        private readonly Dictionary<string, int> _values = new Dictionary<string, int>();

        public event Action<string, bool> FlagChanged;
        public event Action<string, int> ValueChanged;
        public event Action StateRestored;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public bool HasFlag(string key)
        {
            return !string.IsNullOrWhiteSpace(key) && _flags.Contains(key);
        }

        public void SetFlag(string key, bool value = true)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            bool changed = value ? _flags.Add(key) : _flags.Remove(key);
            if (changed)
                FlagChanged?.Invoke(key, value);
        }

        public int GetValue(string key, int fallback = 0)
        {
            if (string.IsNullOrWhiteSpace(key))
                return fallback;

            return _values.TryGetValue(key, out int value) ? value : fallback;
        }

        public void SetValue(string key, int value)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            _values[key] = value;
            ValueChanged?.Invoke(key, value);
        }

        public int AddValue(string key, int delta)
        {
            int value = GetValue(key) + delta;
            SetValue(key, value);
            return value;
        }

        public WorldStateSnapshot CaptureSnapshot()
        {
            var snapshot = new WorldStateSnapshot
            {
                flags = _flags
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToList(),
                values = _values
                    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(pair => new WorldStateIntEntry
                    {
                        key = pair.Key,
                        value = pair.Value
                    })
                    .ToList()
            };

            return snapshot;
        }

        public void RestoreSnapshot(WorldStateSnapshot snapshot)
        {
            _flags.Clear();
            _values.Clear();

            if (snapshot != null)
            {
                if (snapshot.flags != null)
                {
                    foreach (string flag in snapshot.flags)
                    {
                        if (!string.IsNullOrWhiteSpace(flag))
                            _flags.Add(flag);
                    }
                }

                if (snapshot.values != null)
                {
                    foreach (WorldStateIntEntry entry in snapshot.values)
                    {
                        if (entry == null || string.IsNullOrWhiteSpace(entry.key))
                            continue;

                        _values[entry.key] = entry.value;
                    }
                }
            }

            StateRestored?.Invoke();

            foreach (string flag in _flags)
                FlagChanged?.Invoke(flag, true);

            foreach (KeyValuePair<string, int> pair in _values)
                ValueChanged?.Invoke(pair.Key, pair.Value);
        }

        public void ResetState()
        {
            RestoreSnapshot(new WorldStateSnapshot());
        }
    }
}
