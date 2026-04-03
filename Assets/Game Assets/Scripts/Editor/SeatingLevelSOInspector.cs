#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace PeopleSeat.Gameplay.Editor
{
    [CustomEditor(typeof(SeatingLevelSO))]
    public class SeatingLevelSOInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Seat Level Editor", GUILayout.Height(24)))
                SeatingLevelEditorWindow.Open((SeatingLevelSO)target);

            EditorGUILayout.Space(4);
            DrawDefaultInspector();
        }
    }
}
#endif
