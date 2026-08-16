using UnityEditor;
using UnityEngine;

namespace Dungeon.Gameplay.Player.Editor
{
    [CustomEditor(typeof(PlayerStats))]
    public class PlayerStatsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PlayerStats stats = (PlayerStats)target;

            if (GUILayout.Button("Heal"))
            {
                stats.Heal();
            }

            if (GUILayout.Button("Kill"))
            {
                stats.Kill();
            }
        }
    }
}
