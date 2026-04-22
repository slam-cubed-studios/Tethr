// Copyright (c) 2026, TheMGLegends. All rights reserved.

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace Tethr.DetectionVisualiser
{
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
