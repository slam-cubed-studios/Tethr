// Copyright (c) 2026, TheMGLegends. All rights reserved.

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace Tethr.PathVisualiser
{
    /// <summary>
    /// Represents configuration settings for rendering the lines between path points
    /// </summary>
    [Serializable]
    public class LineSettings
    {
        [Tooltip("Colour of path lines that are currently not being traversed.")]
        public Color defaultColour = Color.red;

        [Tooltip("Colour of the path line that is currently being traversed.")]
        public Color activeColour = Color.green;

        [Tooltip("Thickness of the path lines.")]
        [Min(0.0f)] public float thickness = 15.0f;
    }

    /// <summary>
    /// Represents configuration settings for rendering the discs at path points
    /// </summary>
    [Serializable]
    public class DiscSettings
    {
        [Tooltip("Colour of the discs at path points.")]
        public Color colour = Color.white;

        [Tooltip("Radius of the discs at path points.")]
        [Min(0.0f)] public float radius = 0.5f;
    }

    /// <summary>
    /// Represents configuration settings for rendering the labels at path points
    /// </summary>
    [Serializable]
    public class LabelSettings
    {
        [Tooltip("Colour of the label texts at path points.")]
        public Color textColour = Color.black;

        [Tooltip("Font size of the labels at path points.")]
        [Min(0)] public int fontSize = 64;
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
        [SerializeField] private LineSettings lineSettings = new();
        [SerializeField] private DiscSettings discSettings = new();
        [SerializeField] private LabelSettings labelSettings = new();

        public LineSettings GetLineSettings()
        {
            // INFO: Ensure lineSettings is never null
            return lineSettings ??= new LineSettings();
        }

        public DiscSettings GetDiscSettings()
        {
            // INFO: Ensure discSettings is never null
            return discSettings ??= new DiscSettings();
        }

        public LabelSettings GetLabelSettings()
        {
            // INFO: Ensure labelSettings is never null
            return labelSettings ??= new LabelSettings();
        }

        internal static SerializedObject GetSerializedSettings() => new(instance);
    }
}
#endif
