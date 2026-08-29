using KeeperFirstCovenant.Combat;
using UnityEngine;
using UnityEngine.Events;

namespace KeeperFirstCovenant.Discoveries
{
    [RequireComponent(typeof(Collider))]
    public sealed class WorldDiscoveryPoint : MonoBehaviour
    {
        [SerializeField]
        private string discoveryId;

        [SerializeField]
        private string title = "Новое место";

        [SerializeField, TextArea(3, 10)]
        private string description;

        [SerializeField]
        private DiscoveryCategory category =
            DiscoveryCategory.Location;

        [SerializeField]
        private string locationName;

        [SerializeField]
        private bool disableAfterDiscovery = true;

        [SerializeField]
        private UnityEvent onDiscovered;

        private Collider trigger;

        private void Awake()
        {
            trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
        }

        private void Start()
        {
            if (disableAfterDiscovery &&
                DiscoveryJournal.Instance
                    .HasDiscovery(
                        ResolveId()))
            {
                trigger.enabled = false;
            }
        }

        private void OnTriggerEnter(
            Collider other)
        {
            CombatantRuntime actor =
                other.GetComponentInParent<
                    CombatantRuntime>();

            if (actor == null ||
                !actor.IsAlive ||
                actor.Faction !=
                    CombatFaction.Player)
            {
                return;
            }

            Discover();
        }

        public void Configure(
            string id,
            string discoveryTitle,
            string discoveryDescription,
            DiscoveryCategory discoveryCategory,
            string discoveredLocation = null)
        {
            discoveryId = id;
            title = discoveryTitle;
            description = discoveryDescription;
            category = discoveryCategory;
            locationName = discoveredLocation;
        }

        public bool Discover()
        {
            string id = ResolveId();

            bool existed =
                DiscoveryJournal.Instance
                    .HasDiscovery(id);

            DiscoveryEntryState entry =
                DiscoveryJournal.Instance
                    .Discover(
                        id,
                        title,
                        description,
                        category,
                        locationName);

            if (entry == null)
                return false;

            if (!existed)
                onDiscovered?.Invoke();

            if (disableAfterDiscovery &&
                trigger != null)
            {
                trigger.enabled = false;
            }

            return !existed;
        }

        private string ResolveId()
        {
            return
                World.WorldPersistenceUtility
                    .GetStableId(
                        this,
                        discoveryId);
        }
    }
}
