#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Tethr.PathVisualiser
{
    public class PathVisualiserSettingsProvider
    {
#if UNITY_EDITOR
        [SettingsProvider]
        public static SettingsProvider CreatePathVisualiserSettingsProvider()
        {
            return new SettingsProvider("Project/Path Visualiser Settings", SettingsScope.User)
            {
                label = "Path Visualiser",
                guiHandler = _ =>
                {
                    SerializedObject serializedObject = PathVisualiserSettings.GetSerializedSettings();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("lineSettings"), new GUIContent("Line Settings"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("discSettings"), new GUIContent("Disc Settings"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("labelSettings"), new GUIContent("Label Settings"));
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                },
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "Path", "Visualiser" })
            };
        }
#endif
    }
}
