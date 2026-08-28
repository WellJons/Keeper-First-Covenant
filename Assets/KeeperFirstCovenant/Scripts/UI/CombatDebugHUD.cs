using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Player;
using UnityEngine;

namespace KeeperFirstCovenant.UI
{
    public sealed class CombatDebugHUD : MonoBehaviour
    {
        [SerializeField] private TacticalPlayerController playerController;

        private GUIStyle _box;
        private GUIStyle _title;
        private GUIStyle _text;

        private void Start()
        {
            if (playerController == null)
                playerController = FindFirstObjectByType<TacticalPlayerController>();
        }

        private void EnsureStyles()
        {
            if (_box != null)
                return;

            _box = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                padding = new RectOffset(14, 14, 12, 12)
            };

            _title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold
            };

            _text = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14
            };
        }

        private void OnGUI()
        {
            EnsureStyles();

            TurnCombatDirector director = TurnCombatDirector.Instance;
            if (director == null)
                return;

            GUILayout.BeginArea(new Rect(18f, 18f, 390f, 260f), _box);
            GUILayout.Label("KEEPER — COMBAT PROTOTYPE", _title);
            GUILayout.Space(4f);
            GUILayout.Label($"State: {director.State}    Round: {director.Round}", _text);

            CombatantRuntime actor = director.CurrentActor;
            if (actor != null && actor.Definition != null)
            {
                GUILayout.Label($"Turn: {actor.Definition.displayName}", _text);
                GUILayout.Label(
                    $"HP {actor.CurrentHealth}/{actor.Definition.maxHealth}   " +
                    $"MP {actor.CurrentMana}/{actor.Definition.maxMana}",
                    _text);

                GUILayout.Label(
                    $"AP {actor.CurrentActionPoints}   " +
                    $"Move {actor.RemainingMovement:0.0} m",
                    _text);

                if (actor.Faction == CombatFaction.Player)
                {
                    GUILayout.Space(5f);
                    GUILayout.Label("LMB ground — move", _text);
                    GUILayout.Label("1–8 — select ability | RMB/Esc — cancel", _text);
                    GUILayout.Label("LMB target — use ability | Space — end turn", _text);

                    if (actor.Definition.startingActions != null)
                    {
                        GUILayout.Space(4f);
                        for (int i = 0; i < actor.Definition.startingActions.Length && i < 8; i++)
                        {
                            CombatActionDefinition action = actor.Definition.startingActions[i];
                            if (action == null)
                                continue;

                            string marker =
                                playerController != null &&
                                playerController.SelectedAction == action
                                    ? "  <SELECTED>"
                                    : string.Empty;

                            GUILayout.Label(
                                $"{i + 1}. {action.displayName}  " +
                                $"AP:{action.actionPointCost} MP:{action.manaCost} " +
                                $"Range:{action.rangeMeters:0.0}{marker}",
                                _text);
                        }
                    }
                }
            }
            else if (director.State == CombatState.Victory)
            {
                GUILayout.Space(8f);
                GUILayout.Label("Victory. Click a corpse/cache to search it.", _text);
            }

            GUILayout.EndArea();
        }
    }
}
