using System;
using System.Collections.Generic;
using UnityEngine;

namespace KeeperFirstCovenant.World
{
    public sealed class WorldState : MonoBehaviour
    {
        public static WorldState Instance { get; private set; }

        private readonly HashSet<string> _flags = new HashSet<string>();
        private readonly Dictionary<string, int> _values = new Dictionary<string, int>();

        public event Action<string, bool> FlagChanged;
        public event Action<string, int> ValueChanged;

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
    }
}
