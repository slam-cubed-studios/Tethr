// Copyright (c) 2026, TheMGLegends. All rights reserved.

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace Tethr.PathVisualiser
{
    /// <summary>
    /// Provides a settings provider for configuring Path Visualiser preferences in the Unity Editor.
    /// </summary>
    public static class PathVisualiserSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreatePathVisualiserSettingsProvider()
        {
            return new SettingsProvider("Preferences/Path Visualiser Settings", SettingsScope.User)
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
    }
}
#endif
