using System;
using UnityEngine;

namespace KeeperFirstCovenant.World
{
    public sealed class WorldLightSource :
        MonoBehaviour,
        IInteractable,
        IPersistentWorldObject
    {
        [Serializable]
        private sealed class PersistentState
        {
            public bool lit;
        }

        [SerializeField]
        private string persistenceId;

        [SerializeField]
        private Light[] controlledLights;

        [SerializeField]
        private bool lit = true;

        [SerializeField]
        private bool canRelight = true;

        [SerializeField]
        private string extinguishPrompt =
            "Погасить огонь";

        [SerializeField]
        private string relightPrompt =
            "Зажечь огонь";

        [SerializeField]
        private GameObject[] visibleWhenLit;

        [SerializeField]
        private GameObject[] visibleWhenUnlit;

        public string InteractionPrompt =>
            lit
                ? extinguishPrompt
                : relightPrompt;

        public bool IsLit => lit;

        public string PersistenceId =>
            WorldPersistenceUtility.GetStableId(
                this,
                persistenceId);

        private void Awake()
        {
            if (controlledLights == null ||
                controlledLights.Length == 0)
            {
                controlledLights =
                    GetComponentsInChildren<
                        Light>(true);
            }

            Apply();
        }

        public void Configure(
            bool startsLit,
            bool allowRelight = true)
        {
            lit = startsLit;
            canRelight = allowRelight;

            if (controlledLights == null ||
                controlledLights.Length == 0)
            {
                controlledLights =
                    GetComponentsInChildren<
                        Light>(true);
            }

            Apply();
        }

        public bool CanInteract(
            GameObject actor)
        {
            return actor != null &&
                   (lit || canRelight);
        }

        public void Interact(
            GameObject actor)
        {
            if (!CanInteract(actor))
                return;

            lit = !lit;
            Apply();
        }

        public string CapturePersistentState()
        {
            return JsonUtility.ToJson(
                new PersistentState
                {
                    lit = lit
                });
        }

        public void RestorePersistentState(
            string json)
        {
            if (string.IsNullOrWhiteSpace(
                    json))
            {
                return;
            }

            PersistentState state =
                JsonUtility.FromJson<
                    PersistentState>(json);

            if (state == null)
                return;

            lit = state.lit;
            Apply();
        }

        private void Apply()
        {
            if (controlledLights != null)
            {
                foreach (Light light
                         in controlledLights)
                {
                    if (light != null)
                        light.enabled = lit;
                }
            }

            SetObjects(
                visibleWhenLit,
                lit);

            SetObjects(
                visibleWhenUnlit,
                !lit);
        }

        private static void SetObjects(
            GameObject[] objects,
            bool active)
        {
            if (objects == null)
                return;

            foreach (GameObject value
                     in objects)
            {
                if (value != null)
                    value.SetActive(active);
            }
        }
    }
}
