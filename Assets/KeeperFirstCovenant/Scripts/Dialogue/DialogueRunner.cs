using System;
using System.Collections.Generic;
using System.Linq;
using KeeperFirstCovenant.Quests;
using KeeperFirstCovenant.Relationships;
using KeeperFirstCovenant.World;
using UnityEngine;

namespace KeeperFirstCovenant.Dialogue
{
    public sealed class DialogueRunner : MonoBehaviour
    {
        private static DialogueRunner instance;

        private float previousTimeScale = 1f;
        private CursorLockMode previousCursorLock;
        private bool previousCursorVisible;

        private DialogueDefinition definition;
        private DialogueNode currentNode;

        private readonly List<DialogueChoice>
            availableChoices =
                new List<DialogueChoice>();

        public static DialogueRunner Instance
        {
            get
            {
                EnsureExists();
                return instance;
            }
        }

        public static DialogueRunner Current =>
            instance;

        public static bool IsDialogueActive =>
            instance != null &&
            instance.IsActive;

        public bool IsActive { get; private set; }

        public DialogueDefinition Definition =>
            definition;

        public DialogueNode CurrentNode =>
            currentNode;

        public IReadOnlyList<DialogueChoice>
            AvailableChoices =>
                availableChoices;

        public event Action DialogueStarted;
        public event Action<DialogueNode> NodeChanged;
        public event Action DialogueEnded;

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
                    DialogueRunner>();

            if (instance != null)
                return;

            GameObject root =
                new GameObject(
                    "Keeper_DialogueRunner");

            instance =
                root.AddComponent<
                    DialogueRunner>();
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
            if (instance != this)
                return;

            if (IsActive)
                RestoreGameplayState();

            instance = null;
        }

        public bool StartDialogue(
            DialogueDefinition newDefinition)
        {
            if (newDefinition == null ||
                IsActive ||
                newDefinition.nodes == null ||
                newDefinition.nodes.Length == 0)
            {
                return false;
            }

            DialogueNode start =
                FindNode(
                    newDefinition,
                    newDefinition.startNodeId);

            if (start == null ||
                !ConditionsMet(
                    start.conditions))
            {
                return false;
            }

            definition =
                newDefinition;

            previousTimeScale =
                Mathf.Max(
                    0.0001f,
                    Time.timeScale);

            previousCursorLock =
                Cursor.lockState;

            previousCursorVisible =
                Cursor.visible;

            Time.timeScale = 0f;
            Cursor.lockState =
                CursorLockMode.None;
            Cursor.visible = true;

            IsActive = true;
            DialogueStarted?.Invoke();

            return EnterNode(start);
        }

        public bool Continue()
        {
            if (!IsActive ||
                currentNode == null ||
                availableChoices.Count > 0)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(
                    currentNode.nextNodeId))
            {
                EndDialogue();
                return true;
            }

            DialogueNode next =
                FindNode(
                    definition,
                    currentNode.nextNodeId);

            if (next == null)
            {
                EndDialogue();
                return true;
            }

            return EnterNode(next);
        }

        public bool SelectChoice(int index)
        {
            if (!IsActive ||
                index < 0 ||
                index >=
                    availableChoices.Count)
            {
                return false;
            }

            DialogueChoice choice =
                availableChoices[index];

            if (choice == null ||
                !ConditionsMet(
                    choice.conditions))
            {
                RefreshChoices();
                NodeChanged?.Invoke(
                    currentNode);
                return false;
            }

            ApplyEffects(
                choice.effects);

            if (string.IsNullOrWhiteSpace(
                    choice.nextNodeId))
            {
                EndDialogue();
                return true;
            }

            DialogueNode next =
                FindNode(
                    definition,
                    choice.nextNodeId);

            if (next == null)
            {
                EndDialogue();
                return true;
            }

            return EnterNode(next);
        }

        public void Cancel()
        {
            if (!IsActive ||
                definition == null ||
                !definition.allowCancel)
            {
                return;
            }

            EndDialogue();
        }

        public void EndDialogue()
        {
            if (!IsActive)
                return;

            IsActive = false;
            currentNode = null;
            availableChoices.Clear();
            definition = null;

            RestoreGameplayState();
            DialogueEnded?.Invoke();
        }

        private bool EnterNode(
            DialogueNode node)
        {
            if (node == null ||
                !ConditionsMet(
                    node.conditions))
            {
                EndDialogue();
                return false;
            }

            currentNode = node;

            ApplyEffects(
                node.onEnterEffects);

            RefreshChoices();

            NodeChanged?.Invoke(
                currentNode);

            return true;
        }

        private void RefreshChoices()
        {
            availableChoices.Clear();

            if (currentNode?.choices == null)
                return;

            foreach (DialogueChoice choice
                     in currentNode.choices)
            {
                if (choice != null &&
                    ConditionsMet(
                        choice.conditions))
                {
                    availableChoices.Add(
                        choice);
                }
            }
        }

        private static DialogueNode FindNode(
            DialogueDefinition dialogue,
            string nodeId)
        {
            if (dialogue?.nodes == null ||
                string.IsNullOrWhiteSpace(
                    nodeId))
            {
                return null;
            }

            return dialogue.nodes
                .FirstOrDefault(
                    node =>
                        node != null &&
                        string.Equals(
                            node.nodeId,
                            nodeId,
                            StringComparison.Ordinal));
        }

        private static bool ConditionsMet(
            DialogueCondition[] conditions)
        {
            if (conditions == null ||
                conditions.Length == 0)
            {
                return true;
            }

            foreach (DialogueCondition condition
                     in conditions)
            {
                if (condition == null)
                    continue;

                if (!ConditionMet(condition))
                    return false;
            }

            return true;
        }

        private static bool ConditionMet(
            DialogueCondition condition)
        {
            WorldState world =
                WorldState.Instance;

            QuestEntryState quest =
                !string.IsNullOrWhiteSpace(
                    condition.key)
                    ? QuestJournal.Instance
                        .FindQuest(
                            condition.key)
                    : null;

            switch (condition.kind)
            {
                case DialogueConditionKind
                    .WorldFlagSet:
                    return world != null &&
                           world.HasFlag(
                               condition.key);

                case DialogueConditionKind
                    .WorldFlagUnset:
                    return world == null ||
                           !world.HasFlag(
                               condition.key);

                case DialogueConditionKind
                    .WorldValueAtLeast:
                    return world != null &&
                           world.GetValue(
                               condition.key) >=
                           condition.intValue;

                case DialogueConditionKind
                    .WorldValueAtMost:
                    return world != null &&
                           world.GetValue(
                               condition.key) <=
                           condition.intValue;

                case DialogueConditionKind
                    .QuestActive:
                    return quest != null &&
                           quest.status ==
                           QuestStatus.Active;

                case DialogueConditionKind
                    .QuestCompleted:
                    return quest != null &&
                           quest.status ==
                           QuestStatus.Completed;

                case DialogueConditionKind
                    .QuestFailed:
                    return quest != null &&
                           quest.status ==
                           QuestStatus.Failed;

                case DialogueConditionKind
                    .RelationshipAtLeast:
                    return RelationshipLedger
                               .Instance
                               .GetApproval(
                                   condition.key) >=
                           condition.intValue;

                case DialogueConditionKind
                    .RelationshipAtMost:
                    return RelationshipLedger
                               .Instance
                               .GetApproval(
                                   condition.key) <=
                           condition.intValue;

                default:
                    return true;
            }
        }

        private static void ApplyEffects(
            DialogueEffect[] effects)
        {
            if (effects == null)
                return;

            foreach (DialogueEffect effect
                     in effects)
            {
                if (effect != null)
                    ApplyEffect(effect);
            }
        }

        private static void ApplyEffect(
            DialogueEffect effect)
        {
            WorldState world =
                WorldState.Instance;

            QuestJournal quests =
                QuestJournal.Instance;

            switch (effect.kind)
            {
                case DialogueEffectKind
                    .SetWorldFlag:
                    world?.SetFlag(
                        effect.key,
                        true);
                    break;

                case DialogueEffectKind
                    .ClearWorldFlag:
                    world?.SetFlag(
                        effect.key,
                        false);
                    break;

                case DialogueEffectKind
                    .SetWorldValue:
                    world?.SetValue(
                        effect.key,
                        effect.intValue);
                    break;

                case DialogueEffectKind
                    .AddWorldValue:
                    world?.AddValue(
                        effect.key,
                        effect.intValue);
                    break;

                case DialogueEffectKind
                    .StartQuest:
                    if (effect.quest != null)
                    {
                        quests.StartQuest(
                            effect.quest);
                    }
                    break;

                case DialogueEffectKind
                    .AddQuestProgress:
                    quests.AddProgress(
                        effect.key,
                        effect.objectiveId,
                        effect.intValue);
                    break;

                case DialogueEffectKind
                    .CompleteQuestObjective:
                    quests.CompleteObjective(
                        effect.key,
                        effect.objectiveId);
                    break;

                case DialogueEffectKind
                    .CompleteQuest:
                    quests.CompleteQuest(
                        effect.key);
                    break;

                case DialogueEffectKind
                    .FailQuest:
                    quests.FailQuest(
                        effect.key);
                    break;

                case DialogueEffectKind
                    .AddRelationship:
                    RelationshipLedger
                        .Instance
                        .AddApproval(
                            effect.key,
                            effect.intValue);
                    break;

                case DialogueEffectKind
                    .SetRelationship:
                    RelationshipLedger
                        .Instance
                        .SetApproval(
                            effect.key,
                            effect.intValue);
                    break;
            }
        }

        private void RestoreGameplayState()
        {
            Time.timeScale =
                previousTimeScale > 0f
                    ? previousTimeScale
                    : 1f;

            Cursor.lockState =
                previousCursorLock;

            Cursor.visible =
                previousCursorVisible;
        }
    }
}
