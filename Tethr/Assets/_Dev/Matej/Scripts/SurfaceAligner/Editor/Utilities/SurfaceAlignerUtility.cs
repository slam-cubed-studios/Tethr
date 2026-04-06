// Copyright (c) 2026, TheMGLegends. All rights reserved.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace Tethr.SurfaceAligner
{
    /// <summary>
    /// Provides editor utilities for aligning selected transforms to detected surfaces in the Unity Editor Scene view.
    /// </summary>
    /// 
    /// <remarks>
    /// Supports both 2D and 3D physics alignment and visualises raycasts and normals during drag operations.
    /// </remarks>
    [InitializeOnLoad]
    public static class SurfaceAlignerUtility
    {
        private static SurfaceAlignerSettings settings;
        private static List<Transform> selectedTransforms;
        private static bool isDragging;

        static SurfaceAlignerUtility()
        {
            InitialiseUtility();
        }

        private static void InitialiseUtility()
        {
            SetSettings(SurfaceAlignerSettings.Get());
            selectedTransforms = new List<Transform>(Selection.transforms);
            isDragging = false;

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Selection.selectionChanged += OnSelectionChanged;
            SceneView.duringSceneGui += DuringSceneGUI;
        }

        public static void SetSettings(SurfaceAlignerSettings settings)
        {
            SurfaceAlignerUtility.settings = settings;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void CleanupForPlayMode()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
            Selection.selectionChanged -= OnSelectionChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            isDragging = false;
            selectedTransforms = null;
            settings = null;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                InitialiseUtility();
            }
        }

        private static void OnSelectionChanged()
        {
            UpdateSelectedTransforms();
        }

        private static void UpdateSelectedTransforms()
        {
            selectedTransforms = new List<Transform>(Selection.transforms);
        }

        private static void DuringSceneGUI(SceneView sceneView)
        {
            Event currentEvent = Event.current;

            if (!IsDragging(currentEvent))
            {
                return;
            }

            Dictionary<GameObject, SurfaceCheck> surfaceChecks = settings.GetSurfaceChecks(selectedTransforms);

            DrawUtilityVisuals(surfaceChecks);
            AlignSelectedToSurfaces(surfaceChecks, currentEvent);
        }

        private static bool IsDragging(Event currentEvent)
        {
            // INFO: Start drag operation on left mouse button down
            if (currentEvent.type == EventType.MouseDrag && currentEvent.button == (int)MouseButton.Left && !isDragging)
            {
                isDragging = true;
            }

            return isDragging;
        }

        private static void DrawUtilityVisuals(Dictionary<GameObject, SurfaceCheck> surfaceChecks)
        {
            foreach (Transform selectedTransform in selectedTransforms)
            {
                if (selectedTransform == null)
                {
                    continue;
                }

                GameObject selectedObject = selectedTransform.gameObject;

                // INFO: Get surface check for this object, if it exists
                surfaceChecks.TryGetValue(selectedObject, out SurfaceCheck surfaceCheck);
                if (surfaceCheck == null)
                {
                    continue;
                }

                switch (settings.GetPhysicsType())
                {
                    case PhysicsType.Physics3D:
                        {
                            RaycastHit hit = selectedTransform.GetSurfaceHit(surfaceCheck.direction, surfaceCheck.maxDistance, surfaceCheck.layerMask);
                            if (hit.collider != null)
                            {
                                DrawSurfaceHitGizmos(selectedTransform, surfaceCheck, hit.point, hit.normal);
                            }

                            break;
                        }
                    case PhysicsType.Physics2D:
                        {
                            RaycastHit2D hit = selectedTransform.GetSurfaceHit2D(surfaceCheck.direction, surfaceCheck.maxDistance, surfaceCheck.layerMask);
                            if (hit.collider != null)
                            {
                                DrawSurfaceHitGizmos(selectedTransform, surfaceCheck, hit.point, hit.normal);
                            }

                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private static void DrawSurfaceHitGizmos(Transform transform, SurfaceCheck surfaceCheck, Vector3 hitPoint, Vector3 hitNormal)
        {
            PhysicsType physicsType = settings.GetPhysicsType();
            LineSettings lineSettings = settings.GetLineSettings();
            DiscSettings discSettings = settings.GetDiscSettings();

            // INFO: Draw ray from object to hit point
            Handles.color = lineSettings.rayColour;
            Handles.DrawLine(transform.position, hitPoint, lineSettings.thickness);

            // INFO: Draw normal at hit point if it's not opposite to the ray direction
            if (hitNormal != surfaceCheck.OppositeDirection)
            {
                Handles.color = lineSettings.normalColour;
                Handles.DrawLine(hitPoint, hitPoint + hitNormal * lineSettings.normalLength, lineSettings.thickness);
            }

            // INFO: Draw disc at hit point (Oriented to surface normal for 3D, flat for 2D)
            Handles.color = discSettings.colour;
            Handles.DrawSolidDisc(hitPoint, physicsType == PhysicsType.Physics3D ? hitNormal : Vector3.forward, discSettings.radius);
        }

        private static void AlignSelectedToSurfaces(Dictionary<GameObject, SurfaceCheck> surfaceChecks, Event currentEvent)
        {
            if (!IsLeftMouseButtonReleased(currentEvent))
            {
                return;
            }

            foreach (Transform selectedTransform in selectedTransforms)
            {
                if (selectedTransform == null)
                {
                    continue;
                }

                GameObject selectedObject = selectedTransform.gameObject;

                // INFO: Get surface check for this object, if it exists
                surfaceChecks.TryGetValue(selectedObject, out SurfaceCheck surfaceCheck);
                if (surfaceCheck == null)
                {
                    continue;
                }

                Undo.RecordObject(selectedTransform, "Align To Surface");

                switch (settings.GetPhysicsType())
                {
                    case PhysicsType.Physics3D:
                        {
                            selectedTransform.AlignToSurface(surfaceCheck.direction, surfaceCheck.maxDistance, 
                                                             surfaceCheck.layerMask, surfaceCheck.offset);
                            break;
                        }
                    case PhysicsType.Physics2D:
                        {
                            selectedTransform.AlignToSurface2D(surfaceCheck.direction, surfaceCheck.maxDistance, 
                                                               surfaceCheck.layerMask, surfaceCheck.offset);
                            break;
                        }
                    default:
                        break;
                }
            }

            Undo.SetCurrentGroupName("Align To Surface");
        }

        private static bool IsLeftMouseButtonReleased(Event currentEvent)
        {
            // INFO: Check for left mouse button release to end drag operation
            if (currentEvent.type == EventType.MouseUp && currentEvent.button == (int)MouseButton.Left)
            {
                isDragging = false;
                return true;
            }

            return false;
        }
    }
}
