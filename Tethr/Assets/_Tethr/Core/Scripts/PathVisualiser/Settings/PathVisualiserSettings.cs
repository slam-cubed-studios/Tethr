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
        public Color defaultColour;

        [Tooltip("Colour of the path line that is currently being traversed.")]
        public Color activeColour;

        [Tooltip("Thickness of the path lines.")]
        [Min(0.0f)] public float thickness;

        public LineSettings(bool useDefault)
        {
            if (useDefault)
            {
                defaultColour = Color.red;
                activeColour = Color.green;
                thickness = 15.0f;
            }
            else
            {
                defaultColour = default;
                activeColour = default;
                thickness = default;
            }
        }
    }

    /// <summary>
    /// Represents configuration settings for rendering the discs at path points.
    /// </summary>
    [Serializable]
    public struct DiscSettings
    {
        [Tooltip("Colour of the discs at path points.")]
        public Color colour;

        [Tooltip("Radius of the discs at path points.")]
        [Min(0.0f)] public float radius;

        public DiscSettings(bool useDefault)
        {
            if (useDefault)
            {
                colour = Color.white;
                radius = 0.5f;
            }
            else
            {
                colour = default;
                radius = default;
            }
        }
    }

    /// <summary>
    /// Represents configuration settings for rendering the labels at path points.
    /// </summary>
    [Serializable]
    public struct LabelSettings
    {
        [Tooltip("Colour of the label texts at path points.")]
        public Color textColour;

        [Tooltip("Font size of the labels at path points.")]
        [Min(0)] public int fontSize;

        public LabelSettings(bool useDefault)
        {
            if (useDefault)
            {
                textColour = Color.white;
                fontSize = 64;
            }
            else
            {
                textColour = default;
                fontSize = default;
            }
        }
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
        [SerializeField] private LineSettings lineSettings = new(true);
        [SerializeField] private DiscSettings discSettings = new(true);
        [SerializeField] private LabelSettings labelSettings = new(true);

        public ref readonly LineSettings GetLineSettings() => ref lineSettings;

        public ref readonly DiscSettings GetDiscSettings() => ref discSettings;

        public ref readonly LabelSettings GetLabelSettings() => ref labelSettings;

        internal static void Save() => instance.Save(true);

        internal static SerializedObject GetSerializedSettings() => new(instance);
    }
}
#endif
