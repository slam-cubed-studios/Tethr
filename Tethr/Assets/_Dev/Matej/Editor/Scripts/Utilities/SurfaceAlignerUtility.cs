using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

// TODO: Change all if (!Object) checks to if (Object == null) to conform to C# practices

/// <summary>
/// Editor-Only Utility
/// </summary>
[InitializeOnLoad]
public static class SurfaceAlignerUtility
{
    private static SurfaceAlignerSettings settings;
    private static Transform activeTransform;
    private static Vector3 initialPosition;

    static SurfaceAlignerUtility()
    {
        settings = SurfaceAlignerSettings.LoadSurfaceAlignerSettings();
        activeTransform = Selection.activeTransform ? Selection.activeTransform : null;
        initialPosition = activeTransform ? activeTransform.position : Vector3.zero;

        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        Selection.selectionChanged += OnSelectionChanged;
        SceneView.duringSceneGui += DuringSceneGUI;
        Undo.undoRedoEvent += OnUndoRedoEvent;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetForPlayMode()
    {
        Undo.undoRedoEvent -= OnUndoRedoEvent;
        SceneView.duringSceneGui -= DuringSceneGUI;
        Selection.selectionChanged -= OnSelectionChanged;
        Selection.selectionChanged += OnSelectionChanged;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        initialPosition = Vector3.zero;
        activeTransform = null;
        settings = null;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            settings = SurfaceAlignerSettings.LoadSurfaceAlignerSettings();
            activeTransform = Selection.activeTransform ? Selection.activeTransform : null;
            initialPosition = activeTransform ? activeTransform.position : Vector3.zero;

            Selection.selectionChanged += OnSelectionChanged;
            SceneView.duringSceneGui += DuringSceneGUI;
            Undo.undoRedoEvent += OnUndoRedoEvent;
        }
    }

    private static void OnSelectionChanged()
    {
        if (!Selection.activeTransform)
        {
            activeTransform = null;
            return;
        }

        if (Selection.activeTransform != activeTransform)
        {
            activeTransform = Selection.activeTransform;
            initialPosition = activeTransform.position;
        }
    }

    private static void DuringSceneGUI(SceneView sceneView)
    {
        if (!settings || !activeTransform)
        {
            return;
        }

        SurfaceCheck surfaceCheck = settings.TryGetSurfaceCheck(activeTransform.gameObject);
        if (surfaceCheck == null)
        {
            // TODO: Editor Log Warning
            return;
        }

        PhysicsType physicsType = settings.GetPhysicsType();
        LineSettings lineSettings = settings.GetLineSettings();
        DiscSettings discSettings = settings.GetDiscSettings();

        Vector3 normalizedDirection = surfaceCheck.direction.normalized;

        switch (physicsType)
        {
            case PhysicsType.Physics3D:
                {
                    RaycastHit hit = activeTransform.GetSurfaceHit(normalizedDirection, surfaceCheck.maxDistance, surfaceCheck.layerMask);
                    if (hit.collider != null && activeTransform.position != initialPosition)
                    {
                        // TODO: Probably need null checks for lineSettings and discSettings

                        Handles.color = lineSettings.colour;
                        Handles.DrawLine(activeTransform.position, hit.point, lineSettings.thickness);

                        if (hit.normal != -normalizedDirection)
                        {
                            Handles.color = lineSettings.normalColour;
                            Handles.DrawLine(hit.point, hit.point + hit.normal * lineSettings.normalLength, lineSettings.thickness);
                        }

                        Handles.color = discSettings.colour;
                        Handles.DrawSolidDisc(hit.point, hit.normal, discSettings.radius);
                    }

                    break;
                }
            case PhysicsType.Physics2D:
                {
                    RaycastHit2D hit = activeTransform.GetSurfaceHit2D(normalizedDirection, surfaceCheck.maxDistance, surfaceCheck.layerMask);
                    if (hit.collider != null && activeTransform.position != initialPosition)
                    {
                        // TODO: Probably need null checks for lineSettings and discSettings

                        Handles.color = lineSettings.colour;
                        Handles.DrawLine(activeTransform.position, hit.point, lineSettings.thickness);

                        if (hit.normal != -(Vector2)normalizedDirection)
                        {
                            Handles.color = lineSettings.normalColour;
                            Handles.DrawLine(hit.point, hit.point + hit.normal * lineSettings.normalLength, lineSettings.thickness);
                        }

                        Handles.color = discSettings.colour;
                        Handles.DrawSolidDisc(hit.point, Vector3.forward, discSettings.radius);
                    }

                    break;
                }
            default:
                break;
        }

        Event e = Event.current;
        if (e.type == EventType.MouseUp && e.button == (int)MouseButton.Left)
        {
            if (activeTransform.position != initialPosition)
            {
                Undo.RecordObject(activeTransform, "Align To Surface");

                switch (physicsType)
                {
                    case PhysicsType.Physics3D:
                        {
                            activeTransform.AlignToSurface(normalizedDirection, surfaceCheck.maxDistance, surfaceCheck.layerMask, surfaceCheck.offset);
                            break;
                        }
                    case PhysicsType.Physics2D:
                        {
                            activeTransform.AlignToSurface2D(normalizedDirection, surfaceCheck.maxDistance, surfaceCheck.layerMask, surfaceCheck.offset);
                            break;
                        }
                    default:
                        break;
                }

                initialPosition = activeTransform.position;

                Undo.SetCurrentGroupName("Align To Surface");
            }
        }
    }

    private static void OnUndoRedoEvent(in UndoRedoInfo undo)
    {
        if (!undo.isRedo && undo.undoName == "Align To Surface")
        {
            if (activeTransform)
            {
                initialPosition = activeTransform.position;
            }
        }
    }
}
