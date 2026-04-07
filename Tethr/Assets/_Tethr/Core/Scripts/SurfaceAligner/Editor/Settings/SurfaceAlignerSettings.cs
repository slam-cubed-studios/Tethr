// Copyright (c) 2026, TheMGLegends. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tethr.SurfaceAligner
{
    /// <summary>
    /// Specifies the type of physics simulation to use when performing surface checks for alignment.
    /// </summary>
    public enum PhysicsType
    {
        Physics2D,
        Physics3D
    }

    /// <summary>
    /// Represents configuration settings for rendering raycast and normal lines when performing surface checks for 
    /// alignment in the Unity Editor.
    /// </summary>
    [Serializable]
    public class LineSettings
    {
        [Tooltip("Colour of the raycast line from the object to the ray's hit point.")]
        public Color rayColour = Color.green;

        [Tooltip("Colour of the normal line at the ray's hit point.")]
        public Color normalColour = Color.yellow;

        [Tooltip("Thickness of the lines.")]
        [Min(0.0f)] public float thickness = 2.0f;

        [Tooltip("Length of the normal line.")]
        [Min(0.0f)] public float normalLength = 1.0f;
    }

    /// <summary>
    /// Represents configuration settings for rendering a disc at the hit point when performing surface checks for 
    /// alignment in the Unity Editor.
    /// </summary>
    [Serializable]
    public class DiscSettings
    {
        [Tooltip("Colour of the disc drawn at the hit point.")]
        public Color colour = Color.green;

        [Tooltip("Radius of the disc drawn at the hit point.")]
        [Min(0.0f)] public float radius = 0.075f;
    }

    /// <summary>
    /// Represents configuration data for performing a surface check using raycasting, including direction, distance,
    /// layer mask, and offset.
    /// </summary>
    /// 
    /// <remarks>
    /// Implements ISerializationCallbackReceiver to ensure direction is normalized and maxDistance is valid after
    /// deserialization.
    /// </remarks>
    [Serializable]
    public class SurfaceCheck : ISerializationCallbackReceiver
    {
        [Tooltip("Prefab to which this surface check configuration applies. The surface check will be performed when " +
                 "aligning objects instantiated from this prefab.")]
        public GameObject prefab = null;

        [Tooltip("Direction in which to perform the raycast for the surface check. Auto-normalized during assignment.")]
        public Vector3 direction = Vector3.down;

        [Tooltip("Maximum distance for the raycast when performing the surface check. Must be greater than 0.")]
        public float maxDistance = Mathf.Infinity;

        [Tooltip("Layer mask to specify which layers should be considered when performing the raycast for the surface check.")]
        public LayerMask layerMask = 0;

        [Tooltip("Offset applied to object after it has been aligned to the surface. Typically used when visuals don't " +
                 "match surface alignment.")]
        public Vector3 offset = Vector3.zero;

        public Vector3 OppositeDirection => -direction;

        public void OnAfterDeserialize()
        {
            // INFO: Ensure direction is always normalized
            if (direction != Vector3.zero)
            {
                direction = direction.normalized;
            }

            // INFO: Ensure maxDistance is never below or equal to 0
            if (maxDistance <= 0.0f)
            {
                maxDistance = Mathf.Infinity;
            }
        }

        public void OnBeforeSerialize() {}
    }

    /// <summary>
    /// Provides configuration settings for surface alignment operations, including physics type, line and disc
    /// visualisation, and surface check definitions.
    /// </summary>
    /// 
    /// <remarks>
    /// Intended for use within the Unity Editor as a ScriptableObject asset. Supports loading, creation, and 
    /// retrieval of settings and surface check data for aligning objects to surfaces.
    /// </remarks>
    public class SurfaceAlignerSettings : ScriptableObject
    {
        private const string ASSET_NAME = "SurfaceAlignerSettings";
        private const string FOLDER_PATH = "Assets/Editor/SurfaceAlignerSettings/";
        private const string ASSET_PATH = FOLDER_PATH + ASSET_NAME + ".asset";

        [Header("General Settings")]
        [SerializeField] private PhysicsType physicsType = PhysicsType.Physics3D;
        [SerializeField] private LineSettings lineSettings = new();
        [SerializeField] private DiscSettings discSettings = new();

        [Space(10.0f)]
        
        [Header("Surface Check Settings")]
        [SerializeField] private List<SurfaceCheck> surfaceChecks = new();

        private readonly Dictionary<GameObject, SurfaceCheck> surfaceChecksDictionary = new();

        private void OnValidate()
        {
            RebuildSurfaceChecksDictionary();
        }

        public static SurfaceAlignerSettings GetOrCreateSettings()
        {
            SurfaceAlignerSettings settings = AssetDatabase.LoadAssetAtPath<SurfaceAlignerSettings>(ASSET_PATH);
            if (settings == null)
            {
                settings = CreateInstance<SurfaceAlignerSettings>();
                CreateFolderStructure();
                AssetDatabase.CreateAsset(settings, ASSET_PATH);
                AssetDatabase.SaveAssets();

                // INFO: Ensure SurfaceAlignerUtility has reference to settings when created for the first time
                SurfaceAlignerUtility.SetSettings(settings);
            }

            return settings;
        }

        public Dictionary<GameObject, SurfaceCheck> GetSurfaceChecks(List<GameObject> gameObjects)
        {
            Dictionary<GameObject, SurfaceCheck> checksForGameObjects = new();
            foreach (GameObject gameObject in gameObjects)
            {
                if (gameObject == null)
                {
                    continue;
                }

                SurfaceCheck surfaceCheck = TryGetSurfaceCheck(gameObject);
                if (surfaceCheck != null)
                {
                    checksForGameObjects.TryAdd(gameObject, surfaceCheck);
                }
            }

            return checksForGameObjects;
        }

        public SurfaceCheck TryGetSurfaceCheck(GameObject gameObject)
        {
            GameObject prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            if (prefabSource == null)
            {
                return null;
            }

            if (surfaceChecksDictionary.TryGetValue(prefabSource, out SurfaceCheck surfaceCheck))
            {
                return surfaceCheck;
            }

            return null;
        }

        public PhysicsType GetPhysicsType()
        {
            return physicsType;
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

        private void RebuildSurfaceChecksDictionary()
        {
            surfaceChecksDictionary.Clear();
            foreach (SurfaceCheck surfaceCheck in surfaceChecks)
            {
                if (surfaceCheck.prefab != null)
                {
                    surfaceChecksDictionary.TryAdd(surfaceCheck.prefab, surfaceCheck);
                }
            }
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
    }
}
