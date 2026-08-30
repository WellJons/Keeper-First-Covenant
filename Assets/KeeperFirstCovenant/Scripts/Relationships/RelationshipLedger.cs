using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KeeperFirstCovenant.Relationships
{
    public enum RelationshipDisposition
    {
        Hostile,
        Tense,
        Reserved,
        Neutral,
        Warm,
        Trusting,
        Devoted
    }

    [Serializable]
    public sealed class RelationshipEntry
    {
        public string characterId;
        public int approval;
    }

    [Serializable]
    public sealed class RelationshipSnapshot
    {
        public int version = 1;
        public List<RelationshipEntry> entries =
            new List<RelationshipEntry>();
    }

    public readonly struct RelationshipChange
    {
        public readonly string CharacterId;
        public readonly int Previous;
        public readonly int Current;
        public readonly int Delta;

        public RelationshipChange(
            string characterId,
            int previous,
            int current)
        {
            CharacterId = characterId;
            Previous = previous;
            Current = current;
            Delta = current - previous;
        }
    }

    public sealed class RelationshipLedger : MonoBehaviour
    {
        private static RelationshipLedger instance;

        private readonly Dictionary<string, int> approval =
            new Dictionary<string, int>(
                StringComparer.Ordinal);

        public static RelationshipLedger Instance
        {
            get
            {
                EnsureExists();
                return instance;
            }
        }

        public static RelationshipLedger Current =>
            instance;

        public event Action Changed;
        public event Action<RelationshipChange>
            RelationshipChanged;

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
                    RelationshipLedger>();

            if (instance != null)
                return;

            GameObject root =
                new GameObject(
                    "Keeper_RelationshipLedger");

            instance =
                root.AddComponent<
                    RelationshipLedger>();
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

        public int GetApproval(
            string characterId)
        {
            if (string.IsNullOrWhiteSpace(
                    characterId))
            {
                return 0;
            }

            return approval.TryGetValue(
                characterId,
                out int value)
                    ? value
                    : 0;
        }

        public int SetApproval(
            string characterId,
            int value)
        {
            if (string.IsNullOrWhiteSpace(
                    characterId))
            {
                return 0;
            }

            int previous =
                GetApproval(characterId);

            int current =
                Mathf.Clamp(
                    value,
                    -100,
                    100);

            if (current == 0)
                approval.Remove(characterId);
            else
                approval[characterId] = current;

            if (previous != current)
            {
                RelationshipChanged?.Invoke(
                    new RelationshipChange(
                        characterId,
                        previous,
                        current));

                Changed?.Invoke();
            }

            return current;
        }

        public int AddApproval(
            string characterId,
            int delta)
        {
            return SetApproval(
                characterId,
                GetApproval(characterId) +
                delta);
        }

        public RelationshipDisposition
            GetDisposition(
                string characterId)
        {
            int value =
                GetApproval(characterId);

            if (value <= -60)
                return RelationshipDisposition.Hostile;

            if (value <= -25)
                return RelationshipDisposition.Tense;

            if (value <= -8)
                return RelationshipDisposition.Reserved;

            if (value < 15)
                return RelationshipDisposition.Neutral;

            if (value < 35)
                return RelationshipDisposition.Warm;

            if (value < 70)
                return RelationshipDisposition.Trusting;

            return RelationshipDisposition.Devoted;
        }

        public string GetDispositionLabel(
            string characterId)
        {
            switch (GetDisposition(
                        characterId))
            {
                case RelationshipDisposition.Hostile:
                    return "Враждебно";
                case RelationshipDisposition.Tense:
                    return "Напряжённо";
                case RelationshipDisposition.Reserved:
                    return "Сдержанно";
                case RelationshipDisposition.Warm:
                    return "Тепло";
                case RelationshipDisposition.Trusting:
                    return "Доверяет";
                case RelationshipDisposition.Devoted:
                    return "Предан";
                default:
                    return "Нейтрально";
            }
        }

        public string CaptureJson()
        {
            var snapshot =
                new RelationshipSnapshot
                {
                    entries =
                        approval
                            .OrderBy(
                                pair =>
                                    pair.Key,
                                StringComparer.Ordinal)
                            .Select(pair =>
                                new RelationshipEntry
                                {
                                    characterId =
                                        pair.Key,
                                    approval =
                                        pair.Value
                                })
                            .ToList()
                };

            return JsonUtility.ToJson(snapshot);
        }

        public void RestoreJson(
            string json)
        {
            approval.Clear();

            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    RelationshipSnapshot snapshot =
                        JsonUtility.FromJson<
                            RelationshipSnapshot>(
                            json);

                    if (snapshot?.entries != null)
                    {
                        foreach (RelationshipEntry entry
                                 in snapshot.entries)
                        {
                            if (entry == null ||
                                string.IsNullOrWhiteSpace(
                                    entry.characterId))
                            {
                                continue;
                            }

                            int value =
                                Mathf.Clamp(
                                    entry.approval,
                                    -100,
                                    100);

                            if (value != 0)
                            {
                                approval[
                                    entry.characterId] =
                                    value;
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(
                        "Relationship ledger could not be restored. " +
                        exception.Message);
                }
            }

            Changed?.Invoke();
        }

        public void ResetLedger()
        {
            approval.Clear();
            Changed?.Invoke();
        }
    }
}
