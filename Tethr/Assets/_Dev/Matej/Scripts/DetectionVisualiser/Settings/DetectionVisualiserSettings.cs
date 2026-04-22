// Copyright (c) 2026, TheMGLegends. All rights reserved.

using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;

namespace Tethr.DetectionVisualiser
{
    [Serializable]
    public class DetectionSettings
    {
        public Color defaultColour = Color.white;
        public Color activeColour = Color.green;
        [Range(0.0f, 1.0f)] public float alpha = 0.1f;
        [Min(0.0f)] public float thickness = 2.5f;
    }

    [FilePath("DetectionVisualiserSettings/DetectionVisualiserSettings.asset", FilePathAttribute.Location.PreferencesFolder)]
    public class DetectionVisualiserSettings : ScriptableSingleton<DetectionVisualiserSettings>
    {
        [Header("General Settings")]
        [SerializeField] private DetectionSettings detectionSettings = new();

        public DetectionSettings DetectionSettings()
        {
            // INFO: Ensure detectionSettings is never null
            return detectionSettings ??= new DetectionSettings();
        }

        internal static SerializedObject GetSerializedSettings() => new(instance);
    }
}
#endif
