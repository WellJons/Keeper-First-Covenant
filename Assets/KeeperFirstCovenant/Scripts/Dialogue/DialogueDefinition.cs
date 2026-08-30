using System;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Quests;
using UnityEngine;

namespace KeeperFirstCovenant.Dialogue
{
    public enum DialogueConditionKind
    {
        WorldFlagSet,
        WorldFlagUnset,
        WorldValueAtLeast,
        WorldValueAtMost,
        QuestActive,
        QuestCompleted,
        QuestFailed,
        DiscoveryKnown,
        DiscoveryUnknown,
        PlayerAttributeAtLeast,
        PlayerAttributeAtMost,
        RelationshipAtLeast,
        RelationshipAtMost
    }

    public enum DialogueEffectKind
    {
        SetWorldFlag,
        ClearWorldFlag,
        SetWorldValue,
        AddWorldValue,
        StartQuest,
        AddQuestProgress,
        CompleteQuestObjective,
        CompleteQuest,
        FailQuest,
        AddRelationship,
        SetRelationship
    }

    [Serializable]
    public sealed class DialogueCondition
    {
        public DialogueConditionKind kind;
        public string key;
        public int intValue;
        public AbilityAttribute attribute =
            AbilityAttribute.Intellect;
    }

    [Serializable]
    public sealed class DialogueEffect
    {
        public DialogueEffectKind kind;
        public string key;
        public string objectiveId;
        public int intValue = 1;
        public QuestDefinition quest;
    }

    [Serializable]
    public sealed class DialogueChoice
    {
        public string choiceId;
        [TextArea(1, 4)] public string text;
        public string nextNodeId;
        public DialogueCondition[] conditions;
        public DialogueEffect[] effects;
    }

    [Serializable]
    public sealed class DialogueNode
    {
        public string nodeId;
        public string speakerId;
        public string speakerName;
        public Sprite speakerPortrait;
        [TextArea(3, 10)] public string text;
        public string nextNodeId;
        public DialogueCondition[] conditions;
        public DialogueEffect[] onEnterEffects;
        public DialogueChoice[] choices;
    }

    [CreateAssetMenu(
        menuName = "Keeper First Covenant/Dialogue/Dialogue",
        fileName = "Dialogue")]
    public sealed class DialogueDefinition : ScriptableObject
    {
        public string dialogueId;
        public string startNodeId = "start";
        public bool allowCancel;
        public DialogueNode[] nodes;
    }
}
