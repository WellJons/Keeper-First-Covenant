using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Player;
using UnityEngine;

namespace KeeperFirstCovenant.UI
{
    public sealed class CombatDebugHUD :
        MonoBehaviour
    {
        [SerializeField]
        private TacticalPlayerController
            playerController;

        private GUIStyle _box;
        private GUIStyle _title;
        private GUIStyle _text;

        private void Start()
        {
            if (playerController == null)
            {
                playerController =
                    FindFirstObjectByType<
                        TacticalPlayerController>();
            }
        }

        private void EnsureStyles()
        {
            if (_box != null)
                return;

            _box = new GUIStyle(GUI.skin.box)
            {
                alignment =
                    TextAnchor.UpperLeft,
                padding =
                    new RectOffset(
                        14,
                        14,
                        12,
                        12)
            };

            _title =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 17,
                    fontStyle =
                        FontStyle.Bold
                };

            _text =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14
                };
        }

        private void OnGUI()
        {
            EnsureStyles();

            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            if (director == null)
                return;

            GUILayout.BeginArea(
                new Rect(
                    18f,
                    18f,
                    470f,
                    520f),
                _box);

            GUILayout.Label(
                "KEEPER — TACTICAL COMBAT",
                _title);

            GUILayout.Space(4f);

            GUILayout.Label(
                $"State: {director.State}    " +
                $"Round: {director.Round}",
                _text);

            DrawPartyState();

            CombatantRuntime actor =
                director.CurrentActor;

            if (actor != null &&
                actor.Definition != null)
            {
                GUILayout.Label(
                    $"Turn: " +
                    $"{actor.Definition.displayName}",
                    _text);

                GUILayout.Label(
                    $"HP {actor.CurrentHealth}/" +
                    $"{actor.Definition.maxHealth}   " +
                    $"MP {actor.CurrentMana}/" +
                    $"{actor.Definition.maxMana}",
                    _text);

                GUILayout.Label(
                    $"AP {actor.CurrentActionPoints}   " +
                    $"Move " +
                    $"{actor.RemainingMovement:0.0} m   " +
                    $"Reaction " +
                    $"{actor.ReactionsRemaining}",
                    _text);

                bool partyControlled =
                    actor.Faction ==
                        CombatFaction.Player ||
                    actor.Faction ==
                        CombatFaction.Ally;

                if (partyControlled)
                {
                    GUILayout.Space(5f);

                    GUILayout.Label(
                        "LMB ground — move | " +
                        "1–8 — ability | " +
                        "Space — end turn",
                        _text);

                    GUILayout.Label(
                        "LMB target — use | " +
                        "RMB/Esc — cancel",
                        _text);

                    DrawAbilities(actor);
                    DrawPreview();
                }
            }
            else if (director.State ==
                     CombatState.Victory)
            {
                GUILayout.Space(8f);

                GUILayout.Label(
                    "Victory. Click a corpse/cache " +
                    "to search it.",
                    _text);
            }

            GUILayout.EndArea();
        }

        private void DrawPartyState()
        {
            CombatantRuntime[] party =
                FindObjectsByType<
                    CombatantRuntime>(
                    FindObjectsSortMode.None);

            bool any = false;

            foreach (CombatantRuntime member in party)
            {
                if (member == null ||
                    member.Definition == null ||
                    (member.Faction !=
                         CombatFaction.Player &&
                     member.Faction !=
                         CombatFaction.Ally))
                {
                    continue;
                }

                if (!any)
                {
                    GUILayout.Space(5f);
                    GUILayout.Label("PARTY", _title);
                    any = true;
                }

                string state =
                    member.IsDowned
                        ? $"DOWNED ({member.DownedRoundsRemaining}r)"
                        : member.IsDead
                            ? "DEAD"
                            : $"HP {member.CurrentHealth}/{member.Definition.maxHealth}";

                GUILayout.Label(
                    $"{member.Definition.displayName}: {state}",
                    _text);
            }

            if (any)
                GUILayout.Space(5f);
        }

        private void DrawAbilities(
            CombatantRuntime actor)
        {
            CombatActionDefinition[] actions =
                actor.GetAvailableActions();

            if (actions == null ||
                actions.Length == 0)
            {
                return;
            }

            GUILayout.Space(6f);
            GUILayout.Label("ABILITIES", _title);

            for (int i = 0;
                 i < actions.Length &&
                 i < 8;
                 i++)
            {
                CombatActionDefinition action =
                    actions[i];

                if (action == null)
                    continue;

                string marker =
                    playerController != null &&
                    playerController
                        .SelectedAction == action
                        ? "  <SELECTED>"
                        : string.Empty;

                string area =
                    action.areaRadius > 0.05f
                        ? $" AoE:{action.areaRadius:0.0}"
                        : string.Empty;

                GUILayout.Label(
                    $"{i + 1}. " +
                    $"{action.displayName}  " +
                    $"AP:{action.actionPointCost} " +
                    $"MP:{action.manaCost} " +
                    $"R:{action.rangeMeters:0.0}" +
                    $"{area}{marker}",
                    _text);
            }
        }

        private void DrawPreview()
        {
            if (playerController == null ||
                playerController.SelectedAction ==
                    null ||
                !playerController.HasHoverPreview)
            {
                return;
            }

            TacticalTargetPreview preview =
                playerController.CurrentPreview;

            GUILayout.Space(8f);
            GUILayout.Label(
                "TARGET PREVIEW",
                _title);

            if (!preview.Valid)
            {
                GUILayout.Label(
                    $"INVALID: {preview.Failure}",
                    _text);
                return;
            }

            GUILayout.Label(
                $"Hit: {preview.HitChance}%   " +
                $"Damage: " +
                $"{preview.DamageMin}-" +
                $"{preview.DamageMax}",
                _text);

            GUILayout.Label(
                $"Distance: " +
                $"{preview.Distance:0.0} m   " +
                $"Cover: {preview.Cover}",
                _text);

            string heightText =
                preview.HeightHitModifier == 0
                    ? "0"
                    : preview.HeightHitModifier > 0
                        ? $"+{preview.HeightHitModifier}%"
                        : $"{preview.HeightHitModifier}%";

            string coverText =
                preview.CoverHitModifier == 0
                    ? "0"
                    : $"{preview.CoverHitModifier}%";

            GUILayout.Label(
                $"Height modifier: " +
                $"{heightText}   " +
                $"Cover modifier: " +
                $"{coverText}",
                _text);

            GUILayout.Label(
                $"Line of sight: " +
                $"{(preview.HasLineOfSight ? "YES" : "NO")}   " +
                $"Affected: " +
                $"{preview.AffectedTargets}",
                _text);
        }
    }
}
