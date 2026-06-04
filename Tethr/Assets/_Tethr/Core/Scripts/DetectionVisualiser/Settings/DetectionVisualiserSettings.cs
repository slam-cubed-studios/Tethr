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
    public struct DetectionSettings
    {
        [Tooltip("Colour of the visualisation when an object is not detected.")]
        public Color DefaultColour;

        [Tooltip("Colour of the visualisation when an object is detected.")]
        public Color ActiveColour;

        [Tooltip("Alpha transparency of the inner fill of the visualisation.")]
        [Range(0.0f, 1.0f)] public float Alpha;

        [Tooltip("Thickness of the visualisation lines.")]
        [Min(0.0f)] public float Thickness;

        public static DetectionSettings Default => new()
        {
            DefaultColour = Color.white,
            ActiveColour = Color.green,
            Alpha = 0.1f,
            Thickness = 2.5f
        };
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
        [SerializeField] private DetectionSettings detectionSettings = DetectionSettings.Default;

        public ref readonly DetectionSettings GetDetectionSettings() => ref detectionSettings;

        internal static void Save() => instance.Save(true);

        internal static SerializedObject GetSerializedSettings() => new(instance);
    }
}
#endif
