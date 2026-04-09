using System;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Tethr.PathVisualiser
{
    [Serializable]
    public class LineSettings
    {
        public Color defaultColour = Color.red;
        public Color activeColour = Color.green;
        [Min(0.0f)] public float thickness = 10.0f;
    }

    [Serializable]
    public class DiscSettings
    {
        public Color colour = Color.white;
        [Min(0.0f)] public float radius = 0.5f;
    }

    [Serializable]
    public class LabelSettings
    {
        public Color textColour = Color.black;
        [Min(0)] public int fontSize = 64;
    }

    public class PathVisualiserSettings : ScriptableObject
    {
#if UNITY_EDITOR
        private const string ASSET_NAME = "PathVisualiserSettings";
        private const string FOLDER_PATH = "Assets/_Dev/Matej/Editor/PathVisualiserSettings/"; // TODO: Change once integrated into Tethr project
        private const string ASSET_PATH = FOLDER_PATH + ASSET_NAME + ".asset";

        [SerializeField] private LineSettings lineSettings = new();
        [SerializeField] private DiscSettings discSettings = new();
        [SerializeField] private LabelSettings labelSettings = new();

        public static PathVisualiserSettings GetOrCreateSettings()
        {
            PathVisualiserSettings settings = AssetDatabase.LoadAssetAtPath<PathVisualiserSettings>(ASSET_PATH);
            if (settings == null)
            {
                settings = CreateInstance<PathVisualiserSettings>();
                CreateFolderStructure();
                AssetDatabase.CreateAsset(settings, ASSET_PATH);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        public LineSettings GetLineSettings()
        {
            // INFO: Ensure lineSettings is never null
            lineSettings ??= new LineSettings();
            return lineSettings;
        }

        public DiscSettings GetDiscSettings()
        {
            // INFO: Ensure discSettings is never null
            discSettings ??= new DiscSettings();
            return discSettings;
        }

        public LabelSettings GetLabelSettings()
        {
            // INFO: Ensure labelSettings is never null
            labelSettings ??= new LabelSettings();
            return labelSettings;
        }

        internal static void CreateFolderStructure()
        {
            if (!Directory.Exists(FOLDER_PATH))
            {
                Directory.CreateDirectory(FOLDER_PATH);
                AssetDatabase.Refresh();
            }
        }

        internal static SerializedObject GetSerializedSettings() => new(GetOrCreateSettings());
#endif
    }
}
