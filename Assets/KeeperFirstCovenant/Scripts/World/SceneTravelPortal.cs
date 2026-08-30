using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Core;
using KeeperFirstCovenant.Dialogue;
using UnityEngine;

namespace KeeperFirstCovenant.World
{
    public sealed class SceneTravelPortal :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField]
        private string interactionPrompt =
            "Перейти";

        [SerializeField]
        private string targetSceneName;

        [SerializeField]
        private string targetSpawnId =
            "default";

        [SerializeField]
        private string targetLocationName;

        [SerializeField]
        private bool saveBeforeTravel =
            true;

        public string InteractionPrompt =>
            interactionPrompt;

        public void Configure(
            string sceneName,
            string spawnId,
            string locationName,
            string prompt = "Перейти")
        {
            targetSceneName = sceneName;
            targetSpawnId = spawnId;
            targetLocationName = locationName;
            interactionPrompt = prompt;
        }

        public bool CanInteract(
            GameObject actor)
        {
            if (actor == null ||
                string.IsNullOrWhiteSpace(
                    targetSceneName) ||
                DialogueRunner.IsDialogueActive)
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

            GameFlowController.Instance
                .TravelToScene(
                    targetSceneName,
                    targetSpawnId,
                    targetLocationName,
                    saveBeforeTravel);
        }
    }
}
