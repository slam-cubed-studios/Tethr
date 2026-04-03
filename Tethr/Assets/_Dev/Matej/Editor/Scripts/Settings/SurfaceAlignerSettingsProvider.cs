using UnityEditor;
using UnityEngine;

public static class SurfaceAlignerSettingsProvider
{
    [SettingsProvider]
    public static SettingsProvider CreateSurfaceAlignmentSettingsProvider()
    {
        return new SettingsProvider("Project/Surface Aligner Settings", SettingsScope.Project)
        {
            label = "Surface Aligner",
            guiHandler = _ =>
            {
                SerializedObject serializedObject = SurfaceAlignerSettings.GetSerializedSettings();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("physicsType"), new GUIContent("Physics Type"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lineSettings"), new GUIContent("Line Settings"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("discSettings"), new GUIContent("Disc Settings"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("surfaceChecks"), new GUIContent("Surface Checks"), true);
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            },
            keywords = new System.Collections.Generic.HashSet<string>(new[] { "Test" })
        };
    }
}
