// Copyright (c) 2026, TheMGLegends. All rights reserved.

#if UNITY_EDITOR
using UnityEditor;

namespace Tethr.DetectionVisualiser
{
    /// <summary>
    /// Provides a settings provider for configuring Detection Visualiser preferences in the Unity Editor.
    /// </summary>
    public static class DetectionVisualiserSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreatePathVisualiserSettingsProvider()
        {
            return new SettingsProvider("Preferences/Detection Visualiser Settings", SettingsScope.User)
            {
                label = "Detection Visualiser",
                guiHandler = _ =>
                {
                    SerializedObject serializedObject = DetectionVisualiserSettings.GetSerializedSettings();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("detectionSettings"), true);
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                },
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "Detection", "Visualiser" })
            };
        }
    }
}
#endif
