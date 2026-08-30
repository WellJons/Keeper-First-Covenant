using System;
using System.Linq;
using KeeperFirstCovenant.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KeeperFirstCovenant.World
{
    public sealed class PartySelectionService : MonoBehaviour
    {
        public static PartySelectionService Instance
        {
            get;
            private set;
        }

        private CombatantRuntime selectedMember;

        public CombatantRuntime SelectedMember =>
            selectedMember;

        public static event Action<CombatantRuntime>
            SelectionChanged;

        private void Awake()
        {
            if (Instance != null &&
                Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            ResolveDefaultSelection();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public bool Select(
            CombatantRuntime member)
        {
            if (!IsSelectable(member))
                return false;

            if (selectedMember == member)
                return true;

            selectedMember = member;

            SelectionChanged?.Invoke(
                selectedMember);

            return true;
        }

        public void ResolveDefaultSelection()
        {
            if (IsSelectable(selectedMember))
                return;

            CombatantRuntime[] party =
                FindObjectsByType<
                    CombatantRuntime>();

            CombatantRuntime next =
                party
                    .Where(IsSelectable)
                    .OrderBy(value =>
                        value.Faction ==
                            CombatFaction.Player
                            ? 0
                            : 1)
                    .ThenBy(value =>
                        value.Definition != null &&
                        value.Definition.characterId ==
                            "edward"
                            ? 0
                            : 1)
                    .ThenBy(value =>
                        value.Definition != null
                            ? value.Definition.characterId
                            : value.name)
                    .FirstOrDefault();

            if (next != null)
                Select(next);
        }

        public CombatantRuntime GetSelectedOrDefault()
        {
            if (!IsSelectable(selectedMember))
                ResolveDefaultSelection();

            return selectedMember;
        }

        private static bool IsSelectable(
            CombatantRuntime member)
        {
            return
                member != null &&
                member.IsAlive &&
                (member.Faction ==
                     CombatFaction.Player ||
                 member.Faction ==
                     CombatFaction.Ally);
        }
    }
}
