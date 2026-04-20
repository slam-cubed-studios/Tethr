// Copyright (c) 2026, TheMGLegends. All rights reserved.

using System;
using System.IO;
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
    /// Intended for use within the Unity Editor as a ScriptableObject asset. Supports loading, creation, and
    /// retrieval of settings for path visualisation.
    /// </remarks>
    public class PathVisualiserSettings : ScriptableObject
    {
        private const string ASSET_NAME = "PathVisualiserSettings";
        private const string FOLDER_PATH = "Assets/Editor/PathVisualiserSettings/";
        private const string ASSET_PATH = FOLDER_PATH + ASSET_NAME + ".asset";

        [Header("General Settings")]
        [SerializeField] private LineSettings lineSettings = new();
        [SerializeField] private DiscSettings discSettings = new();
        [SerializeField] private LabelSettings labelSettings = new();

        public static PathVisualiserSettings GetOrCreateSettings()
        {
            PathVisualiserSettings settings = AssetDatabase.LoadAssetAtPath<PathVisualiserSettings>(ASSET_PATH);
            if (settings == null)
            {
                settings = CreateInstance<PathVisualiserSettings>();
                TryCreateDirectory();
                AssetDatabase.CreateAsset(settings, ASSET_PATH);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

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

        private static void TryCreateDirectory()
        {
            if (!Directory.Exists(FOLDER_PATH))
            {
                Directory.CreateDirectory(FOLDER_PATH);
                AssetDatabase.Refresh();
            }
        }

        internal static SerializedObject GetSerializedSettings() => new(GetOrCreateSettings());
    }
}
#endif
