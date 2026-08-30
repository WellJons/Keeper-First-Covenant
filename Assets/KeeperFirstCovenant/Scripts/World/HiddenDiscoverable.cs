using System.Collections.Generic;
using System.Linq;
using KeeperFirstCovenant.Combat;
using UnityEngine;
using UnityEngine.Events;

namespace KeeperFirstCovenant.World
{
    public sealed class HiddenDiscoverable :
        MonoBehaviour,
        IPersistentWorldObject
    {
        [System.Serializable]
        private sealed class PersistentState
        {
            public bool discovered;
        }

        [SerializeField]
        private string persistenceId;

        [SerializeField, Min(1f)]
        private float discoveryRadius = 5f;

        [SerializeField, Min(1)]
        private int perceptionDifficulty = 12;

        [SerializeField, Min(0.1f)]
        private float scanInterval = 0.3f;

        [SerializeField]
        private bool hideRenderers = true;

        [SerializeField]
        private bool hideNonTriggerColliders = true;

        [SerializeField]
        private UnityEvent onDiscovered;

        private readonly HashSet<
            CombatantRuntime>
            _attemptedBy =
                new HashSet<
                    CombatantRuntime>();

        private bool _discovered;
        private float _nextScan;

        public bool IsDiscovered => _discovered;

        public string PersistenceId =>
            WorldPersistenceUtility.GetStableId(
                this,
                persistenceId);

        private void Awake()
        {
            ApplyHiddenState();
        }

        private void Update()
        {
            if (_discovered ||
                Time.unscaledTime < _nextScan)
            {
                return;
            }

            _nextScan =
                Time.unscaledTime +
                scanInterval;

            CombatantRuntime[] nearby =
                FindObjectsByType<
                        CombatantRuntime>(
                        FindObjectsSortMode.None)
                    .Where(x =>
                        x != null &&
                        x.IsAlive &&
                        (x.Faction ==
                             CombatFaction.Player ||
                         x.Faction ==
                             CombatFaction.Ally) &&
                        Vector3.Distance(
                            x.transform.position,
                            transform.position) <=
                        discoveryRadius)
                    .ToArray();

            foreach (CombatantRuntime member
                     in nearby)
            {
                if (!_attemptedBy.Add(member))
                    continue;

                int perception =
                    member.Definition != null
                        ? member.Definition
                            .GetAttribute(
                                AbilityAttribute.Perception)
                        : 0;

                SkillCheckResult result =
                    SkillCheckResolver.Resolve(
                        perception,
                        perceptionDifficulty);

                if (result.Success)
                {
                    Reveal();
                    return;
                }
            }
        }

        public void Reveal()
        {
            if (_discovered)
                return;

            _discovered = true;

            if (hideRenderers)
            {
                foreach (Renderer renderer in
                         GetComponentsInChildren<
                             Renderer>(true))
                {
                    renderer.enabled = true;
                }
            }

            if (hideNonTriggerColliders)
            {
                foreach (Collider collider in
                         GetComponentsInChildren<
                             Collider>(true))
                {
                    if (!collider.isTrigger)
                        collider.enabled = true;
                }
            }

            foreach (TrapMechanism trap in
                     GetComponentsInChildren<
                         TrapMechanism>(true))
            {
                trap.Reveal();
            }

            onDiscovered?.Invoke();

            TacticalGrid3D navigation =
                FindFirstObjectByType<
                    TacticalGrid3D>();

            navigation
                ?.RebuildForDynamicWorld();
        }

        public string CapturePersistentState()
        {
            return JsonUtility.ToJson(
                new PersistentState
                {
                    discovered = _discovered
                });
        }

        public void RestorePersistentState(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return;

            PersistentState state =
                JsonUtility.FromJson<PersistentState>(json);

            if (state == null)
                return;

            _attemptedBy.Clear();

            if (state.discovered)
            {
                if (!_discovered)
                    Reveal();
                else
                    ApplyRevealedState();
            }
            else
            {
                _discovered = false;
                ApplyHiddenState();
            }
        }

        private void ApplyRevealedState()
        {
            _discovered = true;

            if (hideRenderers)
            {
                foreach (Renderer renderer in
                         GetComponentsInChildren<Renderer>(true))
                {
                    renderer.enabled = true;
                }
            }

            if (hideNonTriggerColliders)
            {
                foreach (Collider collider in
                         GetComponentsInChildren<Collider>(true))
                {
                    if (!collider.isTrigger)
                        collider.enabled = true;
                }
            }
        }

        private void ApplyHiddenState()
        {
            if (_discovered)
                return;

            if (hideRenderers)
            {
                foreach (Renderer renderer in
                         GetComponentsInChildren<
                             Renderer>(true))
                {
                    renderer.enabled = false;
                }
            }

            if (hideNonTriggerColliders)
            {
                foreach (Collider collider in
                         GetComponentsInChildren<
                             Collider>(true))
                {
                    if (!collider.isTrigger &&
                        collider.GetComponentInParent<
                            TrapMechanism>() == null)
                    {
                        collider.enabled = false;
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(
                transform.position,
                discoveryRadius);
        }
    }
}
