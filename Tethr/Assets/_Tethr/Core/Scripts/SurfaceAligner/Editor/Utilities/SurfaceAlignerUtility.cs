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
        private static List<GameObject> selectedGameObjects;
        private static Dictionary<GameObject, SurfaceCheck> selectedSurfaceChecks;
        private static bool isDragging = false;

        static SurfaceAlignerUtility()
        {
            EditorApplication.delayCall += static () =>
            {
                Initialise();
            };
        }

        public static void SetSettings(SurfaceAlignerSettings settings)
        {
            SurfaceAlignerUtility.settings = settings;
        }

        private static void Initialise()
        {
            SetSettings(SurfaceAlignerSettings.GetOrCreateSettings());
            selectedGameObjects = new List<GameObject>(Selection.gameObjects);
            selectedSurfaceChecks = settings.GetSurfaceChecks(selectedGameObjects);
            isDragging = false;

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Selection.selectionChanged += OnSelectionChanged;
            SceneView.duringSceneGui += DuringSceneGUI;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void CleanupForPlayMode()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
            Selection.selectionChanged -= OnSelectionChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.delayCall -= static () =>
            {
                Initialise();
            };

            isDragging = false;
            selectedSurfaceChecks = null;
            selectedGameObjects = null;
            settings = null;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                Initialise();
            }
        }

        private static void OnSelectionChanged()
        {
            UpdateSelectedGameObjects();
        }

        private static void UpdateSelectedGameObjects()
        {
            selectedGameObjects = new List<GameObject>(Selection.gameObjects);

            // INFO: Update surface checks for new selection
            selectedSurfaceChecks = settings.GetSurfaceChecks(selectedGameObjects);
        }

        private static void DuringSceneGUI(SceneView sceneView)
        {
            if (settings == null)
            {
                return;
            }

            Event currentEvent = Event.current;

            if (!IsDragging(currentEvent))
            {
                return;
            }

            DrawUtilityVisuals();

            if (!IsLeftMouseButtonReleased(currentEvent))
            {
                return;
            }

            AlignSelectedToSurfaces();
        }

        private static void AlignSelectedToSurfaces()
        {
            PhysicsType physicsType = settings.GetPhysicsType();

            foreach (GameObject selectedGameObject in selectedGameObjects)
            {
                if (selectedGameObject == null)
                {
                    continue;
                }

                // INFO: Get surface check for this object, if it exists
                selectedSurfaceChecks.TryGetValue(selectedGameObject, out SurfaceCheck surfaceCheck);
                if (surfaceCheck == null)
                {
                    continue;
                }

                Transform selectedTransform = selectedGameObject.transform;

                Undo.RecordObject(selectedTransform, "Align To Surface");

                switch (physicsType)
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

        private static void DrawUtilityVisuals()
        {
            PhysicsType physicsType = settings.GetPhysicsType();

            foreach (GameObject selectedGameObject in selectedGameObjects)
            {
                if (selectedGameObject == null)
                {
                    continue;
                }

                // INFO: Get surface check for this object, if it exists
                selectedSurfaceChecks.TryGetValue(selectedGameObject, out SurfaceCheck surfaceCheck);
                if (surfaceCheck == null)
                {
                    continue;
                }

                Transform selectedTransform = selectedGameObject.transform;

                switch (physicsType)
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

        private static bool IsDragging(Event currentEvent)
        {
            // INFO: Only allow dragging if the current tool is Move
            if (Tools.current != Tool.Move)
            {
                return false;
            }

            // INFO: Start drag operation on left mouse button down
            if (currentEvent.type == EventType.MouseDrag && currentEvent.button == (int)MouseButton.Left && !isDragging)
            {
                isDragging = true;
            }

            return isDragging;
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
