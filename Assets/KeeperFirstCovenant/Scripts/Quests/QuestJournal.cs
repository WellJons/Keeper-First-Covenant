using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KeeperFirstCovenant.Quests
{
    public enum QuestStatus
    {
        Active,
        Completed,
        Failed
    }

    [Serializable]
    public sealed class QuestObjectiveState
    {
        public string objectiveId;
        public string description;
        public int currentAmount;
        public int requiredAmount = 1;
        public bool optional;
        public bool completed;
    }

    [Serializable]
    public sealed class QuestEntryState
    {
        public string questId;
        public string title;
        public string description;
        public QuestCategory category;
        public QuestStatus status = QuestStatus.Active;
        public bool tracked;
        public List<QuestObjectiveState> objectives =
            new List<QuestObjectiveState>();
    }

    [Serializable]
    public sealed class QuestJournalSnapshot
    {
        public int version = 1;
        public List<QuestEntryState> quests =
            new List<QuestEntryState>();
    }

    public sealed class QuestJournal : MonoBehaviour
    {
        private static QuestJournal instance;

        [SerializeField]
        private List<QuestEntryState> quests =
            new List<QuestEntryState>();

        public static QuestJournal Instance
        {
            get
            {
                EnsureExists();
                return instance;
            }
        }

        public IReadOnlyList<QuestEntryState> Quests => quests;

        public event Action Changed;
        public event Action<QuestEntryState> QuestStarted;
        public event Action<QuestEntryState> QuestUpdated;
        public event Action<QuestEntryState> QuestCompleted;
        public event Action<QuestEntryState> QuestFailed;

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

            instance = FindFirstObjectByType<QuestJournal>();
            if (instance != null)
                return;

            GameObject root =
                new GameObject("Keeper_QuestJournal");

            instance = root.AddComponent<QuestJournal>();
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

        public QuestEntryState StartQuest(
            QuestDefinition definition,
            bool track = true)
        {
            if (definition == null ||
                string.IsNullOrWhiteSpace(
                    definition.questId))
            {
                return null;
            }

            QuestEntryState existing =
                FindQuest(definition.questId);

            if (existing != null)
                return existing;

            var entry = new QuestEntryState
            {
                questId = definition.questId,
                title = string.IsNullOrWhiteSpace(
                    definition.title)
                    ? definition.questId
                    : definition.title,
                description = definition.description,
                category = definition.category,
                status = QuestStatus.Active,
                tracked = track
            };

            if (definition.objectives != null)
            {
                foreach (QuestObjectiveDefinition objective
                         in definition.objectives)
                {
                    if (objective == null ||
                        string.IsNullOrWhiteSpace(
                            objective.objectiveId))
                    {
                        continue;
                    }

                    entry.objectives.Add(
                        new QuestObjectiveState
                        {
                            objectiveId =
                                objective.objectiveId,
                            description =
                                objective.description,
                            currentAmount = 0,
                            requiredAmount =
                                Mathf.Max(
                                    1,
                                    objective.requiredAmount),
                            optional = objective.optional,
                            completed = false
                        });
                }
            }

            if (track)
                ClearTrackingExcept(entry.questId);

            quests.Add(entry);
            QuestStarted?.Invoke(entry);
            Changed?.Invoke();
            return entry;
        }

        public QuestEntryState StartRuntimeQuest(
            string questId,
            string title,
            string description,
            IEnumerable<QuestObjectiveState> objectives,
            QuestCategory category = QuestCategory.Side,
            bool track = true)
        {
            if (string.IsNullOrWhiteSpace(questId))
                return null;

            QuestEntryState existing =
                FindQuest(questId);

            if (existing != null)
                return existing;

            var entry = new QuestEntryState
            {
                questId = questId,
                title = string.IsNullOrWhiteSpace(title)
                    ? questId
                    : title,
                description = description,
                category = category,
                status = QuestStatus.Active,
                tracked = track,
                objectives = objectives != null
                    ? objectives
                        .Where(value => value != null)
                        .Select(CloneObjective)
                        .ToList()
                    : new List<QuestObjectiveState>()
            };

            if (track)
                ClearTrackingExcept(questId);

            quests.Add(entry);
            QuestStarted?.Invoke(entry);
            Changed?.Invoke();
            return entry;
        }

        public bool AddProgress(
            string questId,
            string objectiveId,
            int amount = 1)
        {
            QuestObjectiveState objective =
                FindObjective(
                    questId,
                    objectiveId,
                    out QuestEntryState quest);

            if (objective == null ||
                quest.status != QuestStatus.Active ||
                objective.completed ||
                amount == 0)
            {
                return false;
            }

            objective.currentAmount =
                Mathf.Clamp(
                    objective.currentAmount + amount,
                    0,
                    Mathf.Max(1, objective.requiredAmount));

            objective.completed =
                objective.currentAmount >=
                Mathf.Max(1, objective.requiredAmount);

            NotifyUpdated(quest);
            EvaluateCompletion(quest);
            return true;
        }

        public bool SetProgress(
            string questId,
            string objectiveId,
            int amount)
        {
            QuestObjectiveState objective =
                FindObjective(
                    questId,
                    objectiveId,
                    out QuestEntryState quest);

            if (objective == null ||
                quest.status != QuestStatus.Active)
            {
                return false;
            }

            objective.currentAmount =
                Mathf.Clamp(
                    amount,
                    0,
                    Mathf.Max(1, objective.requiredAmount));

            objective.completed =
                objective.currentAmount >=
                Mathf.Max(1, objective.requiredAmount);

            NotifyUpdated(quest);
            EvaluateCompletion(quest);
            return true;
        }

        public bool CompleteObjective(
            string questId,
            string objectiveId)
        {
            QuestObjectiveState objective =
                FindObjective(
                    questId,
                    objectiveId,
                    out QuestEntryState quest);

            if (objective == null ||
                quest.status != QuestStatus.Active)
            {
                return false;
            }

            objective.currentAmount =
                Mathf.Max(1, objective.requiredAmount);
            objective.completed = true;

            NotifyUpdated(quest);
            EvaluateCompletion(quest);
            return true;
        }

        public bool CompleteQuest(string questId)
        {
            QuestEntryState quest = FindQuest(questId);
            if (quest == null ||
                quest.status != QuestStatus.Active)
            {
                return false;
            }

            quest.status = QuestStatus.Completed;
            quest.tracked = false;

            QuestCompleted?.Invoke(quest);
            Changed?.Invoke();
            TrackFirstActiveIfNeeded();
            return true;
        }

        public bool FailQuest(string questId)
        {
            QuestEntryState quest = FindQuest(questId);
            if (quest == null ||
                quest.status != QuestStatus.Active)
            {
                return false;
            }

            quest.status = QuestStatus.Failed;
            quest.tracked = false;

            QuestFailed?.Invoke(quest);
            Changed?.Invoke();
            TrackFirstActiveIfNeeded();
            return true;
        }

        public bool SetTracked(
            string questId,
            bool tracked = true)
        {
            QuestEntryState quest = FindQuest(questId);
            if (quest == null ||
                quest.status != QuestStatus.Active)
            {
                return false;
            }

            if (tracked)
                ClearTrackingExcept(questId);

            quest.tracked = tracked;
            Changed?.Invoke();
            return true;
        }

        public QuestEntryState GetTrackedQuest()
        {
            return quests.FirstOrDefault(
                value =>
                    value != null &&
                    value.status == QuestStatus.Active &&
                    value.tracked);
        }

        public QuestEntryState FindQuest(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId))
                return null;

            return quests.FirstOrDefault(
                value =>
                    value != null &&
                    string.Equals(
                        value.questId,
                        questId,
                        StringComparison.Ordinal));
        }

        public string CaptureJson()
        {
            var snapshot = new QuestJournalSnapshot
            {
                quests = quests
                    .Where(value => value != null)
                    .Select(CloneQuest)
                    .ToList()
            };

            return JsonUtility.ToJson(snapshot);
        }

        public void RestoreJson(string json)
        {
            quests.Clear();

            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    QuestJournalSnapshot snapshot =
                        JsonUtility.FromJson<QuestJournalSnapshot>(
                            json);

                    if (snapshot?.quests != null)
                    {
                        quests = snapshot.quests
                            .Where(value => value != null)
                            .Select(CloneQuest)
                            .ToList();
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(
                        "Quest journal could not be restored. " +
                        exception.Message);
                }
            }

            TrackFirstActiveIfNeeded();
            Changed?.Invoke();
        }

        public void ResetJournal()
        {
            quests.Clear();
            Changed?.Invoke();
        }

        private void EvaluateCompletion(
            QuestEntryState quest)
        {
            if (quest == null ||
                quest.status != QuestStatus.Active ||
                quest.objectives == null ||
                quest.objectives.Count == 0)
            {
                return;
            }

            bool allRequiredComplete =
                quest.objectives
                    .Where(value =>
                        value != null &&
                        !value.optional)
                    .All(value => value.completed);

            bool hasRequired =
                quest.objectives.Any(
                    value =>
                        value != null &&
                        !value.optional);

            if (hasRequired && allRequiredComplete)
                CompleteQuest(quest.questId);
        }

        private QuestObjectiveState FindObjective(
            string questId,
            string objectiveId,
            out QuestEntryState quest)
        {
            quest = FindQuest(questId);

            if (quest == null ||
                string.IsNullOrWhiteSpace(objectiveId))
            {
                return null;
            }

            return quest.objectives?.FirstOrDefault(
                value =>
                    value != null &&
                    string.Equals(
                        value.objectiveId,
                        objectiveId,
                        StringComparison.Ordinal));
        }

        private void NotifyUpdated(QuestEntryState quest)
        {
            QuestUpdated?.Invoke(quest);
            Changed?.Invoke();
        }

        private void ClearTrackingExcept(string questId)
        {
            foreach (QuestEntryState quest in quests)
            {
                if (quest == null)
                    continue;

                quest.tracked =
                    quest.status == QuestStatus.Active &&
                    string.Equals(
                        quest.questId,
                        questId,
                        StringComparison.Ordinal);
            }
        }

        private void TrackFirstActiveIfNeeded()
        {
            if (GetTrackedQuest() != null)
                return;

            QuestEntryState first =
                quests.FirstOrDefault(
                    value =>
                        value != null &&
                        value.status == QuestStatus.Active);

            if (first != null)
                first.tracked = true;
        }

        private static QuestEntryState CloneQuest(
            QuestEntryState source)
        {
            return new QuestEntryState
            {
                questId = source.questId,
                title = source.title,
                description = source.description,
                category = source.category,
                status = source.status,
                tracked = source.tracked,
                objectives = source.objectives != null
                    ? source.objectives
                        .Where(value => value != null)
                        .Select(CloneObjective)
                        .ToList()
                    : new List<QuestObjectiveState>()
            };
        }

        private static QuestObjectiveState CloneObjective(
            QuestObjectiveState source)
        {
            return new QuestObjectiveState
            {
                objectiveId = source.objectiveId,
                description = source.description,
                currentAmount = source.currentAmount,
                requiredAmount = Mathf.Max(
                    1,
                    source.requiredAmount),
                optional = source.optional,
                completed = source.completed
            };
        }
    }
}
