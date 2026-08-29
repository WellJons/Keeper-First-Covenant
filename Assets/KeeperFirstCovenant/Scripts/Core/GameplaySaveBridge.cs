using System;
using System.Collections.Generic;
using System.Linq;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Inventory;
using KeeperFirstCovenant.Quests;
using KeeperFirstCovenant.World;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KeeperFirstCovenant.Core
{
    [Serializable]
    internal sealed class PersistentWorldObjectPayload
    {
        public string persistenceId;
        public string typeName;
        public Vector3 position;
        public Vector3 eulerAngles;
        public string stateJson;
    }

    [Serializable]
    internal sealed class WorldSavePayload
    {
        public int version = 2;
        public WorldStateSnapshot worldState;
        public int day = 1;
        public float hour = 9f;
        public List<PersistentWorldObjectPayload> objects =
            new List<PersistentWorldObjectPayload>();
    }

    [Serializable]
    internal sealed class PartyMemberSavePayload
    {
        public string characterId;
        public Vector3 position;
        public Vector3 eulerAngles;
        public CombatantRuntimeSnapshot combatant;
        public InventorySnapshot inventory;
        public EquipmentSnapshot equipment;
    }

    [Serializable]
    internal sealed class PartySavePayload
    {
        public int version = 2;
        public string sceneName;
        public List<PartyMemberSavePayload> members =
            new List<PartyMemberSavePayload>();
    }

    public static class GameplaySaveBridge
    {
        public static void CaptureInto(SaveGameData save)
        {
            if (save == null)
                return;

            CaptureWorld(save);
            CaptureParty(save);
            save.questStateJson =
                QuestJournal.Instance.CaptureJson();
        }

        public static void RestoreFrom(SaveGameData save)
        {
            if (save == null)
                return;

            RestoreWorld(save);
            RestoreParty(save);
            QuestJournal.Instance.RestoreJson(
                save.questStateJson);
        }

        public static void ResetRuntimeState()
        {
            if (WorldState.Instance != null)
                WorldState.Instance.ResetState();

            if (WorldTimeSystem.Instance != null)
                WorldTimeSystem.Instance.SetTime(9f, 1);

            QuestJournal.Instance.ResetJournal();
        }

        private static void CaptureWorld(SaveGameData save)
        {
            WorldSavePayload payload =
                TryDecodeWorldPayload(save.worldStateJson) ??
                new WorldSavePayload();

            payload.version = 3;

            if (WorldState.Instance != null)
                payload.worldState = WorldState.Instance.CaptureSnapshot();

            if (WorldTimeSystem.Instance != null)
            {
                payload.day = Mathf.Max(1, WorldTimeSystem.Instance.Day);
                payload.hour = Mathf.Repeat(WorldTimeSystem.Instance.Hour, 24f);
            }

            var merged =
                new Dictionary<string, PersistentWorldObjectPayload>(
                    StringComparer.Ordinal);

            if (payload.objects != null)
            {
                foreach (PersistentWorldObjectPayload existing
                         in payload.objects)
                {
                    if (existing == null ||
                        string.IsNullOrWhiteSpace(
                            existing.persistenceId))
                    {
                        continue;
                    }

                    merged[existing.persistenceId] =
                        existing;
                }
            }

            MonoBehaviour[] behaviours =
                UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                    FindObjectsSortMode.None);

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (!(behaviour is IPersistentWorldObject persistent))
                    continue;

                string id = persistent.PersistenceId;
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                merged[id] =
                    new PersistentWorldObjectPayload
                    {
                        persistenceId = id,
                        typeName = behaviour.GetType().FullName,
                        position = behaviour.transform.position,
                        eulerAngles = behaviour.transform.eulerAngles,
                        stateJson = persistent.CapturePersistentState()
                    };
            }

            payload.objects = merged.Values
                .OrderBy(
                    value => value.persistenceId,
                    StringComparer.Ordinal)
                .ToList();

            save.worldStateJson =
                JsonUtility.ToJson(payload);
        }

        private static void RestoreWorld(SaveGameData save)
        {
            if (string.IsNullOrWhiteSpace(save.worldStateJson))
                return;

            try
            {
                WorldSavePayload payload =
                    TryDecodeWorldPayload(
                        save.worldStateJson);

                if (payload == null)
                    return;

                if (WorldState.Instance != null)
                    WorldState.Instance.RestoreSnapshot(payload.worldState);

                if (WorldTimeSystem.Instance != null)
                    WorldTimeSystem.Instance.SetTime(payload.hour, payload.day);

                RestorePersistentWorldObjects(payload.objects);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    "Keeper world state could not be restored. " + exception.Message);
            }
        }

        private static WorldSavePayload TryDecodeWorldPayload(
            string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonUtility.FromJson<WorldSavePayload>(
                    json);
            }
            catch
            {
                return null;
            }
        }

        private static void RestorePersistentWorldObjects(
            List<PersistentWorldObjectPayload> savedObjects)
        {
            if (savedObjects == null || savedObjects.Count == 0)
                return;

            MonoBehaviour[] behaviours =
                UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                    FindObjectsSortMode.None);

            var loaded =
                new Dictionary<string, IPersistentWorldObject>(
                    StringComparer.Ordinal);

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (!(behaviour is IPersistentWorldObject persistent))
                    continue;

                string id = persistent.PersistenceId;
                if (string.IsNullOrWhiteSpace(id) ||
                    loaded.ContainsKey(id))
                {
                    continue;
                }

                loaded[id] = persistent;
            }

            foreach (PersistentWorldObjectPayload saved in savedObjects)
            {
                if (saved == null ||
                    string.IsNullOrWhiteSpace(saved.persistenceId) ||
                    !loaded.TryGetValue(
                        saved.persistenceId,
                        out IPersistentWorldObject persistent))
                {
                    continue;
                }

                if (persistent is Component component)
                {
                    component.transform.position = saved.position;
                    component.transform.rotation =
                        Quaternion.Euler(saved.eulerAngles);
                }

                persistent.RestorePersistentState(saved.stateJson);
            }
        }

        private static void CaptureParty(SaveGameData save)
        {
            var payload = new PartySavePayload
            {
                sceneName =
                    SceneManager.GetActiveScene().name
            };

            CombatantRuntime[] combatants =
                UnityEngine.Object.FindObjectsByType<CombatantRuntime>(
                    FindObjectsSortMode.None);

            foreach (CombatantRuntime combatant in combatants)
            {
                if (!IsPartyMember(combatant))
                    continue;

                string characterId = combatant.Definition.characterId;

                var member = new PartyMemberSavePayload
                {
                    characterId = characterId,
                    position = combatant.transform.position,
                    eulerAngles = combatant.transform.eulerAngles,
                    combatant = combatant.CaptureRuntimeSnapshot()
                };

                InventoryComponent inventory =
                    combatant.GetComponent<InventoryComponent>();

                if (inventory != null)
                    member.inventory = inventory.CaptureSnapshot();

                EquipmentComponent equipment =
                    combatant.GetComponent<EquipmentComponent>();

                if (equipment != null)
                    member.equipment = equipment.CaptureSnapshot();

                payload.members.Add(member);
            }

            payload.members = payload.members
                .OrderBy(member => member.characterId, StringComparer.Ordinal)
                .ToList();

            save.partyStateJson = JsonUtility.ToJson(payload);
        }

        private static void RestoreParty(SaveGameData save)
        {
            if (string.IsNullOrWhiteSpace(save.partyStateJson))
                return;

            PartySavePayload payload;

            try
            {
                payload = JsonUtility.FromJson<PartySavePayload>(
                    save.partyStateJson);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    "Keeper party state could not be decoded. " + exception.Message);
                return;
            }

            if (payload?.members == null)
                return;

            string activeScene =
                SceneManager.GetActiveScene().name;

            if (!string.IsNullOrWhiteSpace(
                    payload.sceneName) &&
                !string.Equals(
                    payload.sceneName,
                    activeScene,
                    StringComparison.Ordinal))
            {
                return;
            }

            CombatantRuntime[] loaded =
                UnityEngine.Object.FindObjectsByType<CombatantRuntime>(
                    FindObjectsSortMode.None);

            Dictionary<string, CombatantRuntime> byId = loaded
                .Where(IsPartyMember)
                .GroupBy(value => value.Definition.characterId)
                .ToDictionary(
                    group => group.Key,
                    group => group.First(),
                    StringComparer.Ordinal);

            Dictionary<string, ItemDefinition> items =
                BuildLoadedItemCatalog();

            foreach (PartyMemberSavePayload saved in payload.members)
            {
                if (saved == null ||
                    string.IsNullOrWhiteSpace(saved.characterId) ||
                    !byId.TryGetValue(saved.characterId, out CombatantRuntime runtime))
                {
                    continue;
                }

                runtime.transform.position = saved.position;
                runtime.transform.rotation =
                    Quaternion.Euler(saved.eulerAngles);

                runtime.RestoreRuntimeSnapshot(saved.combatant);

                InventoryComponent inventory =
                    runtime.GetComponent<InventoryComponent>();

                inventory?.RestoreSnapshot(
                    saved.inventory,
                    itemId => ResolveItem(items, itemId));

                EquipmentComponent equipment =
                    runtime.GetComponent<EquipmentComponent>();

                equipment?.RestoreSnapshot(
                    saved.equipment,
                    itemId => ResolveItem(items, itemId));
            }
        }

        private static Dictionary<string, ItemDefinition>
            BuildLoadedItemCatalog()
        {
            return ItemCatalogService.BuildLookup();
        }

        private static ItemDefinition ResolveItem(
            IReadOnlyDictionary<string, ItemDefinition> catalog,
            string itemId)
        {
            if (catalog == null ||
                string.IsNullOrWhiteSpace(itemId))
            {
                return null;
            }

            return catalog.TryGetValue(itemId, out ItemDefinition item)
                ? item
                : null;
        }

        private static bool IsPartyMember(CombatantRuntime combatant)
        {
            return combatant != null &&
                   combatant.Definition != null &&
                   !string.IsNullOrWhiteSpace(
                       combatant.Definition.characterId) &&
                   (combatant.Faction == CombatFaction.Player ||
                    combatant.Faction == CombatFaction.Ally);
        }
    }
}
