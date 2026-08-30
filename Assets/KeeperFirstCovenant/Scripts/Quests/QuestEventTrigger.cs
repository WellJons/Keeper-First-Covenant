using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.World;
using UnityEngine;

namespace KeeperFirstCovenant.Quests
{
    public enum QuestTriggerAction
    {
        StartQuest,
        AddProgress,
        CompleteObjective,
        CompleteQuest,
        FailQuest
    }

    public sealed class QuestEventTrigger : MonoBehaviour
    {
        [SerializeField] private QuestTriggerAction action = QuestTriggerAction.AddProgress;
        [SerializeField] private QuestDefinition questDefinition;
        [SerializeField] private string questId;
        [SerializeField] private string objectiveId;
        [SerializeField] private int amount = 1;
        [SerializeField] private bool oncePerSave = true;
        [SerializeField] private bool triggerOnPlayerEnter;
        [SerializeField] private string persistenceId;

        private void Awake()
        {
            if (!triggerOnPlayerEnter)
                return;

            Collider collider = GetComponent<Collider>();
            if (collider != null)
                collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!triggerOnPlayerEnter)
                return;

            CombatantRuntime actor = other.GetComponentInParent<CombatantRuntime>();
            if (actor == null || actor.Faction != CombatFaction.Player)
                return;

            Apply();
        }

        public void Apply()
        {
            string flag = "quest.trigger." +
                WorldPersistenceUtility.GetStableId(this, persistenceId);

            WorldState world = WorldState.Instance;

            if (oncePerSave &&
                world != null &&
                world.HasFlag(flag))
            {
                return;
            }

            bool applied = ApplyQuestAction();

            if (applied && oncePerSave)
                world?.SetFlag(flag, true);
        }

        private bool ApplyQuestAction()
        {
            QuestJournal journal = QuestJournal.Instance;

            switch (action)
            {
                case QuestTriggerAction.StartQuest:
                    return questDefinition != null &&
                           journal.StartQuest(questDefinition) != null;

                case QuestTriggerAction.AddProgress:
                    return journal.AddProgress(
                        ResolveQuestId(),
                        objectiveId,
                        amount);

                case QuestTriggerAction.CompleteObjective:
                    return journal.CompleteObjective(
                        ResolveQuestId(),
                        objectiveId);

                case QuestTriggerAction.CompleteQuest:
                    return journal.CompleteQuest(
                        ResolveQuestId());

                case QuestTriggerAction.FailQuest:
                    return journal.FailQuest(
                        ResolveQuestId());

                default:
                    return false;
            }
        }

        private string ResolveQuestId()
        {
            if (!string.IsNullOrWhiteSpace(questId))
                return questId;

            return questDefinition != null
                ? questDefinition.questId
                : string.Empty;
        }
    }
}
