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
        private struct SurfaceAlignerData
        {
            public SurfaceCheck surfaceCheck;
            public List<(Component renderer, Component filter)> previewComponents;
        }

        private enum DragState
        {
            None,
            Dragging,
            Released
        }

        private static SurfaceAlignerSettings settings;
        private static Dictionary<GameObject, SurfaceAlignerData> surfaceAlignerProfiles;
        private static List<BasePreviewer> previewers;
        private static Material surfaceAlignerMaterial;
        private static DragState currentDragState;

        private static SurfaceAlignerSettings Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = SurfaceAlignerSettings.GetOrCreateSettings();
                }

                return settings;
            }
        }

        static SurfaceAlignerUtility()
        {
            EditorApplication.delayCall += static () =>
            {
                Initialise();
            };
        }

        private static void Initialise()
        {
            surfaceAlignerProfiles = new();
            previewers = new();
            surfaceAlignerMaterial = Resources.Load<Material>("SurfaceAlignerMaterial");
            currentDragState = DragState.None;

            RebuildSurfaceAlignerProfiles();
            ClearScenePreviewers();

            Settings.OnSettingsChanged += Reset;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Selection.selectionChanged += OnSelectionChanged;
            SceneView.duringSceneGui += DuringSceneGUI;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Deinitialise()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
            Selection.selectionChanged -= OnSelectionChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Settings.OnSettingsChanged -= Reset;
            EditorApplication.delayCall -= static () =>
            {
                Initialise();
            };

            currentDragState = DragState.None;
            surfaceAlignerMaterial = null;
            previewers = null;
            surfaceAlignerProfiles = null;
            settings = null;
        }

        private static void Reset()
        {
            foreach (BasePreviewer previewer in previewers)
            {
                if (previewer != null)
                {
                    Object.DestroyImmediate(previewer.gameObject);
                }
            }
            previewers.Clear();

            surfaceAlignerProfiles.Clear();
            RebuildSurfaceAlignerProfiles();
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
            RebuildSurfaceAlignerProfiles();
        }

        private static void DuringSceneGUI(SceneView sceneView)
        {
            UpdateDragState(Event.current);
            switch (currentDragState)
            {
                case DragState.Dragging:
                    {
                        DrawUtilityVisuals();
                        break;
                    }
                case DragState.Released:
                    {
                        AlignSelectedToSurfaces();
                        currentDragState = DragState.None;
                        break;
                    }
                case DragState.None:
                default:
                        break;
            }
        }

        private static void UpdateDragState(Event currentEvent)
        {
            if (Tools.current != Tool.Move)
            {
                currentDragState = DragState.None;
                return;
            }

            switch (currentDragState)
            {
                case DragState.None:
                    {
                        if (currentEvent.type == EventType.MouseDrag && currentEvent.button == (int)MouseButton.Left)
                        {
                            currentDragState = DragState.Dragging;
                        }

                        break;
                    }
                case DragState.Dragging:
                    {
                        // INFO: Release if we stopped dragging or left the scene view window
                        if ((currentEvent.type == EventType.MouseUp && currentEvent.button == (int)MouseButton.Left) ||
                            currentEvent.type == EventType.MouseLeaveWindow)
                        {
                            currentDragState = DragState.Released;
                        }

                        break;
                    }
                case DragState.Released:
                default:
                    break;
            }
        }

        private static void AlignSelectedToSurfaces()
        {
            MakePreviewersAvailable();

            PhysicsType physicsType = Settings.GetPhysicsType();
            foreach (var (gameObject, surfaceAlignerData) in surfaceAlignerProfiles)
            {
                if (gameObject == null || surfaceAlignerData.surfaceCheck == null)
                {
                    continue;
                }

                SurfaceCheck surfaceCheck = surfaceAlignerData.surfaceCheck;
                Transform transform = gameObject.transform;
                Undo.RecordObject(transform, "Align To Surface");
                switch (physicsType)
                {
                    case PhysicsType.Physics3D:
                        {
                            transform.AlignToSurface(surfaceCheck.Direction, surfaceCheck.MaxDistance,
                                                     surfaceCheck.SurfaceMask, surfaceCheck.Offset);
                            break;
                        }
                    case PhysicsType.Physics2D:
                        {
                            transform.AlignToSurface2D(surfaceCheck.Direction, surfaceCheck.MaxDistance,
                                                       surfaceCheck.SurfaceMask, surfaceCheck.Offset);
                            break;
                        }
                    default:
                        break;
                }
            }

            Undo.SetCurrentGroupName("Align To Surface");
        }

        private static void RebuildSurfaceAlignerProfiles()
        {
            GameObject[] selectedGameObjects = Selection.gameObjects;
            PhysicsType physicsType = Settings.GetPhysicsType();
            surfaceAlignerProfiles.Clear();
            foreach (GameObject selectedGameObject in selectedGameObjects)
            {
                SurfaceCheck surfaceCheck = Settings.TryGetSurfaceCheck(selectedGameObject);
                if (surfaceCheck == null)
                {
                    continue;
                }

                var previewComponents = new List<(Component renderer, Component filter)>();
                if (physicsType == PhysicsType.Physics2D)
                {
                    foreach (SpriteRenderer spriteRenderer in selectedGameObject.GetComponentsInChildren<SpriteRenderer>())
                    {
                        if (spriteRenderer == null || spriteRenderer.sprite == null || spriteRenderer.color.a == 0.0f)
                        {
                            continue;
                        }

                        previewComponents.Add((spriteRenderer, null));
                    }
                }
                else if (physicsType == PhysicsType.Physics3D)
                {
                    foreach (MeshRenderer meshRenderer in selectedGameObject.GetComponentsInChildren<MeshRenderer>())
                    {
                        if (meshRenderer == null || meshRenderer.sharedMaterials.Length == 0)
                        {
                            continue;
                        }

                        MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();
                        if (meshFilter == null || meshFilter.sharedMesh == null)
                        {
                            continue;
                        }

                        previewComponents.Add((meshRenderer, meshFilter));
                    }
                }

                surfaceAlignerProfiles.Add(selectedGameObject, new SurfaceAlignerData
                {
                    surfaceCheck = surfaceCheck,
                    previewComponents = previewComponents
                });
            }
        }

        private static void ClearScenePreviewers()
        {
            BasePreviewer[] existingPreviewers = Object.FindObjectsByType<BasePreviewer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (BasePreviewer previewer in existingPreviewers)
            {
                if (previewer != null)
                {
                    Object.DestroyImmediate(previewer.gameObject);
                }
            }
        }

        private static BasePreviewer GetAvailablePreviewer()
        {
            foreach (BasePreviewer previewer in previewers)
            {
                if (previewer != null && !previewer.IsActive())
                {
                    return previewer;
                }
            }

            PhysicsType physicsType = Settings.GetPhysicsType();
            BasePreviewer newPreviewer = physicsType switch
            {
                PhysicsType.Physics3D => new GameObject(nameof(Previewer3D)).AddComponent<Previewer3D>(),
                PhysicsType.Physics2D => new GameObject(nameof(Previewer2D)).AddComponent<Previewer2D>(),
                _ => null,
            };
            previewers.Add(newPreviewer);

            return newPreviewer;
        }

        private static void MakePreviewersAvailable()
        {
            foreach (BasePreviewer previewer in previewers)
            {
                if (previewer != null && previewer.IsActive())
                {
                    previewer.Deactivate();
                }
            }
        }

        private static void DrawUtilityVisuals()
        {
            MakePreviewersAvailable();
            surfaceAlignerMaterial.SetColor("_BaseColor", Settings.GetPreviewSettings().AlphaAdjustedPreviewColour);

            PhysicsType physicsType = Settings.GetPhysicsType();
            foreach (var (gameObject, surfaceAlignerData) in surfaceAlignerProfiles)
            {
                SurfaceCheck surfaceCheck = surfaceAlignerData.surfaceCheck;
                if (gameObject == null || surfaceCheck == null)
                {
                    continue;
                }

                Transform transform = gameObject.transform;
                switch (physicsType)
                {
                    case PhysicsType.Physics3D:
                        {
                            RaycastHit hit = transform.GetSurfaceHit(surfaceCheck.Direction, surfaceCheck.MaxDistance, surfaceCheck.SurfaceMask);
                            if (hit.collider != null)
                            {
                                DrawSurfaceHitGizmos(transform, surfaceCheck, hit.point, hit.normal, physicsType);
                                DrawMeshPreviews(gameObject, surfaceCheck, hit);
                            }

                            break;
                        }
                    case PhysicsType.Physics2D:
                        {
                            RaycastHit2D hit = transform.GetSurfaceHit2D(surfaceCheck.Direction, surfaceCheck.MaxDistance, surfaceCheck.SurfaceMask);
                            if (hit.collider != null)
                            {
                                DrawSurfaceHitGizmos(transform, surfaceCheck, hit.point, hit.normal, physicsType);
                                DrawSpritePreviews(gameObject, surfaceCheck, hit);
                            }

                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private static void DrawSurfaceHitGizmos(Transform transform, SurfaceCheck surfaceCheck, Vector3 hitPoint, Vector3 hitNormal, PhysicsType physicsType)
        {
            ref readonly PreviewSettings previewSettings = ref Settings.GetPreviewSettings();
            if (!previewSettings.ShowGizmos)
            {
                return;
            }

            ref readonly LineSettings lineSettings = ref Settings.GetLineSettings();
            ref readonly DiscSettings discSettings = ref Settings.GetDiscSettings();

            // INFO: Draw ray from object to hit point
            Handles.color = lineSettings.RayColour;
            Handles.DrawLine(transform.position, hitPoint, lineSettings.Thickness);

            // INFO: Draw normal at hit point if it's not opposite to the ray direction
            if (hitNormal != surfaceCheck.OppositeDirection)
            {
                Handles.color = lineSettings.NormalColour;
                Handles.DrawLine(hitPoint, hitPoint + hitNormal * lineSettings.NormalLength, lineSettings.Thickness);
            }

            // INFO: Draw disc at hit point (Oriented to surface normal for 3D, flat for 2D)
            Handles.color = discSettings.Colour;
            Handles.DrawSolidDisc(hitPoint, physicsType == PhysicsType.Physics3D ? hitNormal : Vector3.forward, discSettings.Radius);
        }

        private static void DrawMeshPreviews(GameObject gameObject, SurfaceCheck surfaceCheck, RaycastHit hit)
        {
            ref readonly PreviewSettings previewSettings = ref Settings.GetPreviewSettings();
            if (!previewSettings.ShowPreviews || surfaceAlignerMaterial == null)
            {
                return;
            }

            if (surfaceAlignerProfiles.TryGetValue(gameObject, out SurfaceAlignerData surfaceAlignerData))
            {
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                foreach (var (renderer, filter) in surfaceAlignerData.previewComponents)
                {
                    MeshRenderer meshRenderer = renderer as MeshRenderer;
                    if (meshRenderer == null)
                    {
                        continue;
                    }

                    MeshFilter meshFilter = filter as MeshFilter;
                    if (meshFilter == null)
                    {
                        continue;
                    }

                    Previewer3D previewer = GetAvailablePreviewer() as Previewer3D;
                    bool isChild = renderer.transform != gameObject.transform && renderer.transform.IsChildOf(gameObject.transform);
                    Vector3 scaledOffset = Vector3.Scale(isChild ? renderer.transform.localPosition : Vector3.zero, gameObject.transform.localScale) + surfaceCheck.Offset;
                    Vector3 position = hit.point + (rotation * scaledOffset);
                    previewer.Activate(hit.normal, position, renderer.transform.lossyScale, meshFilter.sharedMesh, renderer as MeshRenderer, surfaceAlignerMaterial);
                }
            }
        }

        private static void DrawSpritePreviews(GameObject gameObject, SurfaceCheck surfaceCheck, RaycastHit2D hit)
        {
            ref readonly PreviewSettings previewSettings = ref Settings.GetPreviewSettings();
            if (!previewSettings.ShowPreviews)
            {
                return;
            }

            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            Vector3 hitPoint = new(hit.point.x, hit.point.y, 0.0f);
            if (surfaceAlignerProfiles.TryGetValue(gameObject, out SurfaceAlignerData surfaceAlignerData))
            {
                foreach (var (renderer, _) in surfaceAlignerData.previewComponents)
                {
                    SpriteRenderer spriteRenderer = renderer as SpriteRenderer;
                    if (spriteRenderer == null)
                    {
                        continue;
                    }

                    Previewer2D previewer = GetAvailablePreviewer() as Previewer2D;
                    bool isChild = spriteRenderer.transform != gameObject.transform && spriteRenderer.transform.IsChildOf(gameObject.transform);
                    Vector3 scaledOffset = Vector3.Scale(isChild ? spriteRenderer.transform.localPosition : Vector3.zero, gameObject.transform.localScale) + surfaceCheck.Offset;
                    Vector3 position = hitPoint + (rotation * scaledOffset);
                    previewer.Activate(hit.normal, position, spriteRenderer.transform.lossyScale, previewSettings.Alpha, spriteRenderer);
                }
            }
        }
    }
}
