using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Dialogue;
using UnityEngine;

namespace KeeperFirstCovenant.World
{
    public sealed class DialogueInteractable :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField]
        private DialogueDefinition dialogue;

        [SerializeField]
        private string prompt =
            "Поговорить";

        public string InteractionPrompt =>
            prompt;

        public void Configure(
            DialogueDefinition definition,
            string interactionPrompt =
                "Поговорить")
        {
            dialogue = definition;

            if (!string.IsNullOrWhiteSpace(
                    interactionPrompt))
            {
                prompt =
                    interactionPrompt;
            }
        }

        public bool CanInteract(
            GameObject actor)
        {
            if (actor == null ||
                dialogue == null ||
                DialogueRunner
                    .IsDialogueActive)
            {
                return false;
            }

            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            return director == null ||
                   director.State !=
                   CombatState.Active;
        }

        public void Interact(
            GameObject actor)
        {
            if (!CanInteract(actor))
                return;

            DialogueRunner.Instance
                .StartDialogue(dialogue);
        }
    }
}
