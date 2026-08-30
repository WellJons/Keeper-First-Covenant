using System;
using UnityEngine;

namespace KeeperFirstCovenant.Visual
{
    [Serializable]
    public sealed class WorldSpriteEntry
    {
        public string id;
        public Sprite sprite;
        public Vector2 worldSize = Vector2.one;
        public bool horizontal;
        public bool solid;
        public Vector2 colliderSize = Vector2.one;
        public Vector2 colliderOffset = Vector2.zero;
    }

    [CreateAssetMenu(
        menuName = "Keeper First Covenant/Visual/World Sprite Library",
        fileName = "WorldSpriteLibrary")]
    public sealed class WorldSpriteLibrary : ScriptableObject
    {
        public WorldSpriteEntry[] entries;

        public WorldSpriteEntry Find(string id)
        {
            if (entries == null)
                return null;

            for (int i = 0; i < entries.Length; i++)
            {
                WorldSpriteEntry entry = entries[i];
                if (entry != null && entry.id == id)
                    return entry;
            }

            return null;
        }
    }
}
