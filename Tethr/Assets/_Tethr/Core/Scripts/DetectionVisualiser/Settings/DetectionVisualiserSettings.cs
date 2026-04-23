// Copyright (c) 2026, TheMGLegends. All rights reserved.

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace Tethr.DetectionVisualiser
{
    /// <summary>
    /// Represents configuration settings for rendering detection visualisations.
    /// </summary>
    [Serializable]
    public class DetectionSettings
    {
        [Tooltip("Colour of the visualisation when an object is not detected.")]
        public Color defaultColour = Color.white;

        [Tooltip("Colour of the visualisation when an object is detected.")]
        public Color activeColour = Color.green;

        [Tooltip("Alpha transparency of the inner fill of the visualisation.")]
        [Range(0.0f, 1.0f)] public float alpha = 0.1f;

        [Tooltip("Thickness of the visualisation lines.")]
        [Min(0.0f)] public float thickness = 2.5f;
    }

    /// <summary>
    /// Provides configuration settings for the Detection Visualiser tool, including visualisation appearance options.
    /// </summary>
    /// 
    /// <remarks>
    /// Intended for storing per-user preferences related to the Detection Visualiser tool.
    /// </remarks>
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
