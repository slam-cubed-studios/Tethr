// Copyright (c) 2026, TheMGLegends. All rights reserved.

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace Tethr.PathVisualiser
{
    /// <summary>
    /// Represents configuration settings for rendering the lines between path points.
    /// </summary>
    [Serializable]
    public struct LineSettings
    {
        [Tooltip("Colour of path lines that are currently not being traversed.")]
        public Color DefaultColour;

        [Tooltip("Colour of the path line that is currently being traversed.")]
        public Color ActiveColour;

        [Tooltip("Thickness of the path lines.")]
        [Min(0.0f)] public float Thickness;

        public static LineSettings Default => new()
        {
            DefaultColour = Color.red,
            ActiveColour = Color.green,
            Thickness = 10.0f
        };
    }

    /// <summary>
    /// Represents configuration settings for rendering the discs at path points.
    /// </summary>
    [Serializable]
    public struct DiscSettings
    {
        [Tooltip("Colour of the discs at path points.")]
        public Color Colour;

        [Tooltip("Radius of the discs at path points.")]
        [Min(0.0f)] public float Radius;

        public static DiscSettings Default => new()
        {
            Colour = Color.white,
            Radius = 0.5f
        };
    }

    /// <summary>
    /// Represents configuration settings for rendering the labels at path points.
    /// </summary>
    [Serializable]
    public struct LabelSettings
    {
        [Tooltip("Colour of the label texts at path points.")]
        public Color TextColour;

        [Tooltip("Font size of the labels at path points.")]
        [Min(0)] public int FontSize;

        public static LabelSettings Default => new()
        {
            TextColour = Color.black,
            FontSize = 64
        };
    }

    /// <summary>
    /// Provides configuration settings for the Path Visualiser tool, including line, disc, and label appearance
    /// options.
    /// </summary>
    /// 
    /// <remarks>
    /// Intended for storing per-user preferences related to the Path Visualiser tool.
    /// </remarks>
    [FilePath("PathVisualiserSettings/PathVisualiserSettings.asset", FilePathAttribute.Location.PreferencesFolder)]
    public class PathVisualiserSettings : ScriptableSingleton<PathVisualiserSettings>
    {
        [Header("General Settings")]
        [SerializeField] private LineSettings lineSettings = LineSettings.Default;
        [SerializeField] private DiscSettings discSettings = DiscSettings.Default;
        [SerializeField] private LabelSettings labelSettings = LabelSettings.Default;

        public ref readonly LineSettings GetLineSettings() => ref lineSettings;

        public ref readonly DiscSettings GetDiscSettings() => ref discSettings;

        public ref readonly LabelSettings GetLabelSettings() => ref labelSettings;

        internal static void Save() => instance.Save(true);

        internal static SerializedObject GetSerializedSettings() => new(instance);
    }
}
#endif
