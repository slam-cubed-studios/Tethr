using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// TODO: Add Attributes to values e.g. (Clamp and Tooltip...)

public enum PhysicsType
{
    Physics2D,
    Physics3D
}

[Serializable]
public class LineSettings
{
    public Color colour = Color.green;
    public Color normalColour = Color.yellow;
    public float thickness = 1.0f;
    public float normalLength = 2.0f;
}

[Serializable]
public class DiscSettings 
{
    public Color colour = Color.green;
    public float radius = 0.075f;
}

[Serializable]
public class SurfaceCheck : ISerializationCallbackReceiver
{
    public GameObject prefab = null;
    public Vector3 direction = Vector3.down;
    public float maxDistance = Mathf.Infinity;
    public LayerMask layerMask = 0;
    public Vector3 offset = Vector3.zero;

    public SurfaceCheck()
    {
        prefab = null;
        direction = Vector3.down;
        maxDistance = Mathf.Infinity;
        layerMask = 0;
        offset = Vector3.zero;
    }

    public void OnAfterDeserialize()
    {
        if (direction == Vector3.zero)
        {
            direction = Vector3.down;
        }

        if (maxDistance == 0.0f)
        {
            maxDistance = Mathf.Infinity;
        }
    }

    public void OnBeforeSerialize() { }
}

public class SurfaceAlignerSettings : ScriptableObject
{
    private const string ASSET_NAME = "SurfaceAlignerSettings";
    private const string ASSET_PATH = "Assets/_Dev/Matej/Editor/Resources/" + ASSET_NAME + ".asset"; // TODO: Change Path once Integrated

    [Header("General Settings")]

    [SerializeField] private PhysicsType physicsType = PhysicsType.Physics3D;
    [SerializeField] private LineSettings lineSettings = new();
    [SerializeField] private DiscSettings discSettings = new();

    [Space(10.0f)]

    [Header("Surface Checks")]

    [SerializeField] private List<SurfaceCheck> surfaceChecks = new();

    private readonly Dictionary<GameObject, SurfaceCheck> surfaceChecksDictionary = new();

    private void OnValidate()
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

    public PhysicsType GetPhysicsType()
    {
        return physicsType;
    }

    public LineSettings GetLineSettings() 
    { 
        return lineSettings; 
    }

    public DiscSettings GetDiscSettings()
    {
        return discSettings;
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

    public static SurfaceAlignerSettings LoadSurfaceAlignerSettings()
    {
        return Resources.Load<SurfaceAlignerSettings>(ASSET_NAME);
    }

    internal static SurfaceAlignerSettings GetOrCreateSettings()
    {
        SurfaceAlignerSettings settings = AssetDatabase.LoadAssetAtPath<SurfaceAlignerSettings>(ASSET_PATH);
        if (settings == null)
        {
            settings = CreateInstance<SurfaceAlignerSettings>();
            AssetDatabase.CreateAsset(settings, ASSET_PATH);
            AssetDatabase.SaveAssets();
        }

        return settings;
    }

    internal static SerializedObject GetSerializedSettings() => new(GetOrCreateSettings());
}
