using System;
using UnityEngine;

namespace KeeperFirstCovenant.Quests
{
    public enum QuestCategory
    {
        Main,
        Side,
        Companion,
        Exploration
    }

    [Serializable]
    public sealed class QuestObjectiveDefinition
    {
        public string objectiveId;
        [TextArea] public string description;
        [Min(1)] public int requiredAmount = 1;
        public bool optional;
    }

    [CreateAssetMenu(
        menuName = "Keeper First Covenant/Quests/Quest",
        fileName = "Quest")]
    public sealed class QuestDefinition : ScriptableObject
    {
        public string questId;
        public string title = "Quest";
        [TextArea(3, 10)] public string description;
        public QuestCategory category = QuestCategory.Side;
        public QuestObjectiveDefinition[] objectives;
    }
}
