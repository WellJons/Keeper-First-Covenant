using System.Text;
using UnityEngine;

namespace KeeperFirstCovenant.World
{
    public interface IPersistentWorldObject
    {
        string PersistenceId { get; }
        string CapturePersistentState();
        void RestorePersistentState(string json);
    }

    public static class WorldPersistenceUtility
    {
        public static string GetStableId(
            Component component,
            string explicitId = null)
        {
            if (!string.IsNullOrWhiteSpace(explicitId))
                return explicitId.Trim();

            if (component == null)
                return string.Empty;

            var builder = new StringBuilder();
            builder.Append(component.gameObject.scene.name);
            builder.Append("::");

            Transform current = component.transform;
            var parts = new System.Collections.Generic.List<string>();

            while (current != null)
            {
                parts.Add(
                    current.name + "[" +
                    current.GetSiblingIndex() + "]");

                current = current.parent;
            }

            parts.Reverse();
            builder.Append(string.Join("/", parts));
            builder.Append("::");
            builder.Append(component.GetType().FullName);

            return builder.ToString();
        }
    }
}
