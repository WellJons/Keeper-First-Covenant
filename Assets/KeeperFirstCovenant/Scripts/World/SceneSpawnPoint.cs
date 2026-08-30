using System.Linq;
using KeeperFirstCovenant.Combat;
using UnityEngine;

namespace KeeperFirstCovenant.World
{
    public sealed class SceneSpawnPoint : MonoBehaviour
    {
        [SerializeField]
        private string spawnId = "default";

        public string SpawnId =>
            string.IsNullOrWhiteSpace(spawnId)
                ? "default"
                : spawnId;

        public static bool TryPlaceParty(
            string requestedSpawnId)
        {
            string id =
                string.IsNullOrWhiteSpace(
                    requestedSpawnId)
                    ? "default"
                    : requestedSpawnId;

            SceneSpawnPoint point =
                FindObjectsByType<SceneSpawnPoint>(
                        FindObjectsSortMode.None)
                    .FirstOrDefault(
                        value =>
                            value != null &&
                            string.Equals(
                                value.SpawnId,
                                id,
                                System.StringComparison.Ordinal));

            if (point == null)
                return false;

            CombatantRuntime[] party =
                FindObjectsByType<CombatantRuntime>(
                        FindObjectsSortMode.None)
                    .Where(value =>
                        value != null &&
                        value.Definition != null &&
                        (value.Faction == CombatFaction.Player ||
                         value.Faction == CombatFaction.Ally))
                    .OrderBy(value =>
                        value.Faction == CombatFaction.Player
                            ? 0
                            : 1)
                    .ThenBy(value =>
                        value.Definition.characterId)
                    .ToArray();

            Vector2[] formation =
            {
                Vector2.zero,
                new Vector2(-1.2f, -1.6f),
                new Vector2(1.2f, -1.6f),
                new Vector2(0f, -2.8f),
                new Vector2(-1.8f, -3.0f),
                new Vector2(1.8f, -3.0f)
            };

            for (int i = 0;
                 i < party.Length;
                 i++)
            {
                CombatantRuntime member =
                    party[i];

                TacticalUnitMover mover =
                    member.GetComponent<TacticalUnitMover>();

                mover?.CancelMovement();

                Vector2 offset =
                    formation[
                        Mathf.Min(
                            i,
                            formation.Length - 1)];

                member.transform.position =
                    point.transform.position +
                    point.transform.right * offset.x +
                    point.transform.forward * offset.y;

                member.transform.rotation =
                    Quaternion.LookRotation(
                        point.transform.forward,
                        Vector3.up);
            }

            return true;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(
                transform.position,
                0.35f);

            Gizmos.DrawLine(
                transform.position,
                transform.position +
                transform.forward * 1.5f);
        }
    }
}
