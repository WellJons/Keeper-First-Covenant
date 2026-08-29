using System.Collections.Generic;
using System.Linq;
using KeeperFirstCovenant.Combat;
using UnityEngine;
using UnityEngine.Events;

namespace KeeperFirstCovenant.World
{
    public sealed class HiddenDiscoverable : MonoBehaviour
    {
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

        private readonly HashSet<int>
            _attemptedBy =
                new HashSet<int>();

        private bool _discovered;
        private float _nextScan;

        public bool IsDiscovered => _discovered;

        private void Start()
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
                int id =
                    member.GetInstanceID();

                if (!_attemptedBy.Add(id))
                    continue;

                int modifier =
                    member.Definition != null
                        ? member.Definition
                            .GetModifier(
                                AbilityAttribute.Perception)
                        : 0;

                int secretRoll =
                    Random.Range(1, 21) +
                    modifier;

                if (secretRoll >=
                    perceptionDifficulty)
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

            onDiscovered?.Invoke();
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
                    if (!collider.isTrigger)
                        collider.enabled = false;
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
