using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Core;
using KeeperFirstCovenant.Dialogue;
using UnityEngine;

namespace KeeperFirstCovenant.World
{
    [RequireComponent(typeof(Collider))]
    public sealed class AutosaveCheckpoint :
        MonoBehaviour
    {
        [SerializeField]
        private string checkpointId;

        [SerializeField]
        private string locationName;

        [SerializeField]
        private bool oncePerSave =
            true;

        private void Awake()
        {
            Collider collider =
                GetComponent<Collider>();

            collider.isTrigger = true;
        }

        private void OnTriggerEnter(
            Collider other)
        {
            CombatantRuntime player =
                other.GetComponentInParent<
                    CombatantRuntime>();

            if (player == null ||
                player.Faction !=
                    CombatFaction.Player ||
                DialogueRunner.IsDialogueActive)
            {
                return;
            }

            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            if (director != null &&
                director.State ==
                    CombatState.Active)
            {
                return;
            }

            string key =
                BuildCheckpointFlag();

            WorldState world =
                WorldState.Instance;

            bool wasSet =
                world != null &&
                world.HasFlag(key);

            if (oncePerSave && wasSet)
                return;

            world?.SetFlag(
                key,
                true);

            bool saved =
                GameFlowController.Instance
                    .SaveCurrentGame(
                        false,
                        locationName);

            if (!saved && !wasSet)
            {
                world?.SetFlag(
                    key,
                    false);
            }
        }

        private string BuildCheckpointFlag()
        {
            string id =
                string.IsNullOrWhiteSpace(
                    checkpointId)
                    ? WorldPersistenceUtility
                        .GetStableId(this)
                    : checkpointId;

            return "checkpoint.reached." +
                   id;
        }
    }
}
