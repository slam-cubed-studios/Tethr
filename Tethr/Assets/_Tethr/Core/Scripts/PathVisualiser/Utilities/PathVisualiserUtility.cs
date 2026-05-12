// Copyright (c) 2026, TheMGLegends. All rights reserved.

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace Tethr.PathVisualiser
{
    /// <summary>
    /// Provides utility methods for visualising paths in the Unity Editor Scene view.
    /// </summary>
    /// 
    /// <remarks>
    /// Supports both 2D and 3D path visualisation. Intended for use within the Unity Editor. Ideally called from OnDrawGizmos 
    /// or similar editor-only contexts.
    /// </remarks>
    public static class PathVisualiserUtility
    {
        private static PathVisualiserSettings settings;
        private static GUIStyle labelStyle;
        private const float MAX_WORLD_SCALE_MULTIPLIER = 2.5f;
        private const int LINE_DRAW_REPETITIONS = 3;

        private static PathVisualiserSettings Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = PathVisualiserSettings.instance;
                }

                return settings;
            }
        }
        
        private static GUIStyle LabelStyle
        {
            get
            {
                return labelStyle ??= new GUIStyle()
                {
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Deinitialise()
        {
            labelStyle = null;
            settings = null;
        }

        /// <summary>
        /// Draws a line between the current position and the target position, along with discs and labels at each position.
        /// </summary>
        public static void DrawTargetLine(Vector3 currentPosition, Vector3 targetPosition)
        {
            // INFO: Line Between Current and Target Position
            ref readonly LineSettings lineSettings = ref Settings.GetLineSettings();
            Vector3 midpoint = (currentPosition + targetPosition) / 2.0f;
            Handles.color = lineSettings.activeColour;
            DrawOpaqueLine(currentPosition, targetPosition, ScreenToWorldScale(lineSettings.thickness, midpoint));

            // INFO: Current Point
            DrawPoint(currentPosition, "C");

            // INFO: Target Point
            DrawPoint(targetPosition, "T");
        }

        /// <summary>
        /// Draws a path using the provided parameters.
        /// </summary>
        /// <param name="path">The array of Vector3 points representing the path to be visualised.</param>
        /// 
        /// <param name="traversalType">The type of traversal behaviour for the path, determining how the path is visualised.
        /// For example, Loop visualises an additional line between the last and first point.
        /// </param>
        /// 
        /// <param name="previousPointIndex">The index of the previous point in the path. Used to determine which line segment
        /// to highlight as active.</param>
        /// 
        /// <param name="targetPointIndex">The index of the target point in the path. Used to determine which line segment to
        /// highlight as active.</param>
        public static void DrawPath(Vector3[] path, PathTraversalType traversalType = PathTraversalType.Once, 
                                    int previousPointIndex = 0, int targetPointIndex = 0)
        {
            if (!IsPathValid(path, previousPointIndex, targetPointIndex))
            {
                return;
            }

            bool isCurrentlyWrapping = false;
            if (!AreIndicesValid(previousPointIndex, targetPointIndex, path.Length, traversalType, ref isCurrentlyWrapping))
            {
                return;
            }

            ref readonly LineSettings lineSettings = ref Settings.GetLineSettings();
            const int INVALID_INDEX = -1;

            // INFO: Lines
            Handles.color = lineSettings.defaultColour;
            int lowerIndex = previousPointIndex.Equals(targetPointIndex) ? INVALID_INDEX : Mathf.Min(previousPointIndex, targetPointIndex);
            for (int i = 0; i < path.Length; ++i)
            {
                // INFO: Skip Current Line Between Previous and Target Point if Not Wrapping
                if (i == lowerIndex && !isCurrentlyWrapping)
                {
                    continue;
                }

                // INFO: Draw Inactive Path Line
                if (i + 1 < path.Length)
                {
                    Vector3 currentPoint = path[i];
                    Vector3 nextPoint = path[i + 1];
                    Vector3 midpoint = (currentPoint + nextPoint) / 2.0f;
                    DrawOpaqueLine(currentPoint, nextPoint, ScreenToWorldScale(lineSettings.thickness, midpoint));
                }
            }

            // INFO: Draw Inactive Path Line Between Last and First Point if Not Wrapping and Traversal Type is Loop
            if (!isCurrentlyWrapping && traversalType == PathTraversalType.Loop)
            {
                Vector3 lastPoint = path[^1];
                Vector3 firstPoint = path[0];
                Vector3 midpoint = (lastPoint + firstPoint) / 2.0f;
                DrawOpaqueLine(lastPoint, firstPoint, ScreenToWorldScale(lineSettings.thickness, midpoint));
            }

            // INFO: Draw Active Path Line
            if (previousPointIndex != targetPointIndex)
            {
                Handles.color = lineSettings.activeColour;
                Vector3 previousPoint = path[previousPointIndex];
                Vector3 targetPoint = path[targetPointIndex];
                Vector3 midpoint = (previousPoint + targetPoint) / 2.0f;
                DrawOpaqueLine(previousPoint, targetPoint, ScreenToWorldScale(lineSettings.thickness, midpoint));
            }

            // INFO: Draw Path Points
            for (int i = 0; i < path.Length; ++i)
            {
                Vector3 point = path[i];
                DrawPoint(point, (i + 1).ToString());
            }
        }

        /// <summary>
        /// Overloaded method for DrawPath that accepts a Vector2 array for 2D path visualisation.
        /// </summary>
        /// <param name="path">The array of Vector2 points representing the path to be visualised.</param>
        /// 
        /// <param name="traversalType">The type of traversal behaviour for the path, determining how the path is visualised.
        /// For example, Loop visualises an additional line between the last and first point.
        /// </param>
        /// 
        /// <param name="previousPointIndex">The index of the previous point in the path. Used to determine which line segment
        /// to highlight as active.</param>
        /// 
        /// <param name="targetPointIndex">The index of the target point in the path. Used to determine which line segment to
        /// highlight as active.</param>
        /// 
        /// <remarks>
        /// Internally converts to Vector3 with z = 0.
        /// </remarks>
        public static void DrawPath(Vector2[] path, PathTraversalType traversalType = PathTraversalType.Once,
                                      int previousPointIndex = 0, int targetPointIndex = 0)
        {
            DrawPath(Array.ConvertAll(path, point => (Vector3)point), traversalType, previousPointIndex, targetPointIndex);
        }

        private static bool IsPathValid(Vector3[] path, int previousPointIndex, int nextPointIndex)
        {
            if (path == null || path.Length == 0)
            {
                return false;
            }

            // INFO: Ensure Indices are Within Bounds of the Path Array
            if (previousPointIndex < 0 || nextPointIndex < 0 || previousPointIndex >= path.Length || nextPointIndex >= path.Length)
            {
                Debug.LogError($"Indices must be within the bounds of the path. Path Length: {path.Length}, " +
                               $"Previous Point Index: {previousPointIndex}, Next Point Index: {nextPointIndex}");
                return false;
            }

            return true;
        }

        private static bool AreIndicesValid(int previousPointIndex, int targetPointIndex, int pathLength, 
                                            PathTraversalType traversalType, ref bool isWrapping)
        {
            // INFO: Check for Wrapping from Last to First Point in Loop Traversal Type
            if (previousPointIndex == pathLength - 1 && targetPointIndex == 0 && traversalType == PathTraversalType.Loop)
            {
                isWrapping = true;
                return true;
            }

            // INFO: Ensure Indices are Adjacent or the Same for Non-Wrapping Cases
            if (Mathf.Abs(previousPointIndex - targetPointIndex) > 1)
            {
                Debug.LogError($"Indices must be adjacent or the same. Previous Point Index: {previousPointIndex}, " +
                               $"Next Point Index: {targetPointIndex}");
                return false;
            }

            return true;
        }

        private static float ScreenToWorldScale(float value, Vector3 position)
        {
            return Mathf.Clamp(value / HandleUtility.GetHandleSize(position), 0.0f, value * MAX_WORLD_SCALE_MULTIPLIER);
        }

        private static int ScreenToWorldScale(int value, Vector3 position)
        {
            return Mathf.RoundToInt(value / HandleUtility.GetHandleSize(position));
        }

        private static void DrawPoint(Vector3 position, string label)
        {
            // INFO: Disc at Position
            ref readonly DiscSettings discSettings = ref Settings.GetDiscSettings();
            Handles.color = discSettings.colour;

            // INFO: Have Disc face the camera for better visibility in 3D scenes. In 2D scenes, this will simply be a flat disc.
            Vector3 forward = Camera.current != null ? Camera.current.transform.forward : Vector3.forward;
            Handles.DrawSolidDisc(position, forward, discSettings.radius);

            // INFO: Label at Position
            ref readonly LabelSettings labelSettings = ref Settings.GetLabelSettings();
            LabelStyle.normal.textColor = labelSettings.textColour;
            LabelStyle.fontSize = ScreenToWorldScale(labelSettings.fontSize, position);
            Handles.Label(position, label, LabelStyle);
        }

        private static void DrawOpaqueLine(Vector3 p1, Vector3 p2, float thickness)
        {
            for (int i = 0; i < LINE_DRAW_REPETITIONS; ++i)
            {
                Handles.DrawLine(p1, p2, thickness);
            }
        }
    }
}
#endif
