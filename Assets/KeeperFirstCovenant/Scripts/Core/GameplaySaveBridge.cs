using System;
using KeeperFirstCovenant.World;
using UnityEngine;

namespace KeeperFirstCovenant.Core
{
    [Serializable]
    internal sealed class WorldSavePayload
    {
        public int version = 1;
        public WorldStateSnapshot worldState;
        public int day = 1;
        public float hour = 9f;
    }

    public static class GameplaySaveBridge
    {
        public static void CaptureInto(SaveGameData save)
        {
            if (save == null)
                return;

            var payload = new WorldSavePayload();

            if (WorldState.Instance != null)
                payload.worldState = WorldState.Instance.CaptureSnapshot();

            if (WorldTimeSystem.Instance != null)
            {
                payload.day = Mathf.Max(1, WorldTimeSystem.Instance.Day);
                payload.hour = Mathf.Repeat(WorldTimeSystem.Instance.Hour, 24f);
            }

            save.worldStateJson = JsonUtility.ToJson(payload);
        }

        public static void RestoreFrom(SaveGameData save)
        {
            if (save == null || string.IsNullOrWhiteSpace(save.worldStateJson))
                return;

            try
            {
                WorldSavePayload payload =
                    JsonUtility.FromJson<WorldSavePayload>(save.worldStateJson);

                if (payload == null)
                    return;

                if (WorldState.Instance != null)
                    WorldState.Instance.RestoreSnapshot(payload.worldState);

                if (WorldTimeSystem.Instance != null)
                    WorldTimeSystem.Instance.SetTime(payload.hour, payload.day);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    "Keeper world state could not be restored. " + exception.Message);
            }
        }

        public static void ResetRuntimeState()
        {
            if (WorldState.Instance != null)
                WorldState.Instance.ResetState();

            if (WorldTimeSystem.Instance != null)
                WorldTimeSystem.Instance.SetTime(9f, 1);
        }
    }
}
