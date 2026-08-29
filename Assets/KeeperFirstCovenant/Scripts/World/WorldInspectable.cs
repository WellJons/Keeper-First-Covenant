using System;
using UnityEngine;
using UnityEngine.Events;

namespace KeeperFirstCovenant.World
{
    public enum InspectionCategory
    {
        Object,
        Place,
        Clue,
        Lore,
        Magic,
        Corpse,
        Mechanism
    }

    public sealed class WorldInspectable : MonoBehaviour
    {
        [SerializeField]
        private string inspectionId;

        [SerializeField]
        private string displayName = "Объект";

        [SerializeField]
        private InspectionCategory category =
            InspectionCategory.Object;

        [SerializeField, TextArea(3, 12)]
        private string description;

        [SerializeField]
        private bool requireDiscovery;

        [SerializeField]
        private HiddenDiscoverable discoverySource;

        [SerializeField]
        private bool invokeOncePerSave = true;

        [SerializeField]
        private UnityEvent onFirstInspected;

        public static event Action<
            WorldInspectable,
            GameObject> InspectionRequested;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(displayName)
                ? name
                : displayName;

        public string Description =>
            description ?? string.Empty;

        public InspectionCategory Category =>
            category;

        public string InspectionId =>
            WorldPersistenceUtility.GetStableId(
                this,
                inspectionId);

        public void Configure(
            string title,
            InspectionCategory inspectionCategory,
            string inspectionDescription,
            bool requiresDiscovery = false,
            HiddenDiscoverable source = null)
        {
            displayName =
                string.IsNullOrWhiteSpace(title)
                    ? name
                    : title;

            category = inspectionCategory;
            description =
                inspectionDescription ?? string.Empty;

            requireDiscovery =
                requiresDiscovery;

            discoverySource = source;
        }

        public bool CanInspect(GameObject actor)
        {
            if (actor == null)
                return false;

            if (!requireDiscovery)
                return true;

            HiddenDiscoverable source =
                discoverySource != null
                    ? discoverySource
                    : GetComponentInParent<
                        HiddenDiscoverable>();

            return source == null ||
                   source.IsDiscovered;
        }

        public void Inspect(GameObject actor)
        {
            if (!CanInspect(actor))
                return;

            string flag =
                "inspection.seen." +
                InspectionId;

            WorldState world =
                WorldState.Instance;

            bool firstTime =
                world == null ||
                !world.HasFlag(flag);

            if (firstTime)
            {
                world?.SetFlag(
                    flag,
                    true);

                onFirstInspected?.Invoke();
            }

            if (!invokeOncePerSave &&
                !firstTime)
            {
                onFirstInspected?.Invoke();
            }

            InspectionRequested?.Invoke(
                this,
                actor);
        }

        public bool WasInspected()
        {
            WorldState world =
                WorldState.Instance;

            return world != null &&
                   world.HasFlag(
                       "inspection.seen." +
                       InspectionId);
        }
    }
}
