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
    /// Represents configuration settings for rendering preview visuals when performing surface checks for alignment in the
    /// in the Unity Editor.
    /// </summary>
    [Serializable]
    public struct PreviewSettings
    {
        [Tooltip("Whether to show preview visuals in the Scene view when performing surface checks for alignment.")]
        public bool ShowPreviews;

        [Tooltip("Whether to show gizmos in the Scene view when performing surface checks for alignment.")]
        public bool ShowGizmos;

        [Tooltip("Alpha transparency of the preview visuals when performing surface checks for alignment.")]
        [Range(0.0f, 1.0f)] public float Alpha;

        [Tooltip("Colour of the preview visuals when performing surface checks for alignment. Only used for 3D object visualisation.")]
        public Color PreviewColour;

        public readonly Color AlphaAdjustedPreviewColour => new(PreviewColour.r, PreviewColour.g, PreviewColour.b, Alpha);

        public static PreviewSettings Default => new()
        {
            ShowPreviews = true,
            ShowGizmos = true,
            Alpha = 0.75f,
            PreviewColour = Color.red
        };
    }


    /// <summary>
    /// Represents configuration settings for rendering raycast and normal lines when performing surface checks for 
    /// alignment in the Unity Editor.
    /// </summary>
    [Serializable]
    public struct LineSettings
    {
        [Tooltip("Colour of the raycast line from the object to the ray's hit point.")]
        public Color RayColour;

        [Tooltip("Colour of the normal line at the ray's hit point.")]
        public Color NormalColour;

        [Tooltip("Thickness of the lines.")]
        [Min(0.0f)] public float Thickness;

        [Tooltip("Length of the normal line.")]
        [Min(0.0f)] public float NormalLength;

        public static LineSettings Default => new()
        {
            RayColour = Color.green,
            NormalColour = Color.yellow,
            Thickness = 2.0f,
            NormalLength = 1.0f
        };
    }

    /// <summary>
    /// Represents configuration settings for rendering a disc at the hit point when performing surface checks for 
    /// alignment in the Unity Editor.
    /// </summary>
    [Serializable]
    public struct DiscSettings
    {
        [Tooltip("Colour of the disc drawn at the hit point.")]
        public Color Colour;

        [Tooltip("Radius of the disc drawn at the hit point.")]
        [Min(0.0f)] public float Radius;

        public static DiscSettings Default => new()
        {
            Colour = Color.green,
            Radius = 0.075f
        };
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
        public GameObject Prefab = null;

        [Tooltip("Direction in which to perform the raycast for the surface check. Auto-normalized during assignment.")]
        public Vector3 Direction = Vector3.down;

        [Tooltip("Maximum distance for the raycast when performing the surface check.")]
        public float MaxDistance = Mathf.Infinity;

        [Tooltip("Surface mask specifies which layers should be considered when performing the raycast for the surface check.")]
        public LayerMask SurfaceMask = ~0;

        [Tooltip("Offset applied to object after it has been aligned to the surface. Typically used when visuals don't " +
                 "match surface alignment.")]
        public Vector3 Offset = Vector3.zero;

        public Vector3 OppositeDirection => -Direction;

        public void OnAfterDeserialize()
        {
            // INFO: Ensure direction is always normalized
            if (Direction != Vector3.zero)
            {
                Direction = Direction.normalized;
            }

            // INFO: Ensure maxDistance is never below or equal to 0
            if (MaxDistance <= 0.0f)
            {
                MaxDistance = Mathf.Infinity;
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
        [SerializeField] private PreviewSettings previewSettings = PreviewSettings.Default;
        [SerializeField] private LineSettings lineSettings = LineSettings.Default;
        [SerializeField] private DiscSettings discSettings = DiscSettings.Default;

        [Space(10.0f)]

        [Header("Surface Check Settings")]
        [SerializeField] private List<SurfaceCheck> surfaceChecks = new();

        public Action OnSettingsChanged;

        private readonly Dictionary<GameObject, SurfaceCheck> surfaceChecksDictionary = new();

        private void OnValidate()
        {
            // INFO: Ensure at least gizmos are always shown if previews are disabled
            if (!previewSettings.ShowPreviews && !previewSettings.ShowGizmos)
            {
                previewSettings.ShowGizmos = true;
            }

            EditorApplication.delayCall -= ExecuteSettingsChanged;
            EditorApplication.delayCall += ExecuteSettingsChanged;

            RebuildSurfaceChecksDictionary();
        }

        private void OnDestroy()
        {
            EditorApplication.delayCall -= ExecuteSettingsChanged;
        }

        public static SurfaceAlignerSettings GetOrCreateSettings()
        {
            SurfaceAlignerSettings settings = AssetDatabase.LoadAssetAtPath<SurfaceAlignerSettings>(ASSET_PATH);
            if (settings == null)
            {
                settings = CreateInstance<SurfaceAlignerSettings>();
                TryCreateDirectory();
                AssetDatabase.CreateAsset(settings, ASSET_PATH);
                AssetDatabase.SaveAssets();
            }

            return settings;
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

        public PhysicsType GetPhysicsType() => physicsType;

        public ref readonly PreviewSettings GetPreviewSettings() => ref previewSettings;

        public ref readonly LineSettings GetLineSettings() => ref lineSettings;

        public ref readonly DiscSettings GetDiscSettings() => ref discSettings;

        private void RebuildSurfaceChecksDictionary()
        {
            surfaceChecksDictionary.Clear();
            foreach (SurfaceCheck surfaceCheck in surfaceChecks)
            {
                if (surfaceCheck.Prefab != null)
                {
                    surfaceChecksDictionary.TryAdd(surfaceCheck.Prefab, surfaceCheck);
                }
            }
        }


        private void ExecuteSettingsChanged()
        {
            OnSettingsChanged?.Invoke();
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
