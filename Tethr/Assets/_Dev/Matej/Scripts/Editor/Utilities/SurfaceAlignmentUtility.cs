using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

// TODO: Create "DeveloperSettings" container to hold global info (Debug Colour, Debug Line Thickness, Debug Disc Radius etc.)
//       as well per object info (Structs holding per prefab data like is3D, direction, maxDistance, LayerMask, offset etc.)
//       regarding surface alignment and pull the values from it to replace temporary values and calls here

/// <summary>
/// Editor-Only Utility
/// </summary>
[InitializeOnLoad]
public static class SurfaceAlignmentUtility
{
    static Transform activeTransform;
    static Vector3 initialPosition;

    static SurfaceAlignmentUtility()
    {
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
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
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
        if (!activeTransform)
        {
            return;
        }

        RaycastHit2D hit = activeTransform.GetSurfaceHit2D(Vector2.down, Mathf.Infinity, Physics2D.AllLayers);
        if (hit.collider != null && activeTransform.position != initialPosition)
        {
            Handles.color = Color.green;
            Handles.DrawLine(activeTransform.position, hit.point, 1.0f);
            Handles.DrawSolidDisc(hit.point, Vector3.forward, 0.075f);
        }

        Event e = Event.current;
        if (e.type == EventType.MouseUp && e.button == (int)MouseButton.Left)
        {
            if (activeTransform.position != initialPosition)
            {
                Undo.RecordObject(activeTransform, "Align To Surface");

                activeTransform.AlignToSurface2D(Vector2.down, Mathf.Infinity, Physics2D.AllLayers, new Vector2(0.0f, 0.5f));
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
