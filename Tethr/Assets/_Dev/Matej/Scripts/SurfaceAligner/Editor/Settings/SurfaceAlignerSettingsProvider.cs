// Copyright (c) 2026, TheMGLegends. All rights reserved.

using UnityEditor;
using UnityEngine;

namespace Tethr.SurfaceAligner
{
    /// <summary>
    /// Provides a settings provider for configuring Surface Aligner project settings in the Unity Editor.
    /// </summary>
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
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("surfaceChecks"), new GUIContent("Surface Checks"));
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                },
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "Surface", "Physics Type" })
            };
        }
    }
}
