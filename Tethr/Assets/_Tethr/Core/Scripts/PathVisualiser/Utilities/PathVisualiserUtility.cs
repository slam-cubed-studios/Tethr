// Copyright (c) 2026, TheMGLegends. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tethr.PathVisualiser
{
#if UNITY_EDITOR
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
        public static void DrawConnection(Vector3 currentPosition, Vector3 targetPosition)
        {
            // INFO: Line Between Current and Target Position
            ref readonly LineSettings lineSettings = ref Settings.GetLineSettings();
            Vector3 midpoint = (currentPosition + targetPosition) / 2.0f;

            DrawOpaqueLine(currentPosition, targetPosition, ScreenToWorldScale(lineSettings.Thickness, midpoint), lineSettings.ActiveColour);

            // INFO: Current Point
            DrawPoint(currentPosition, "C");

            // INFO: Target Point
            DrawPoint(targetPosition, "T");
        }

        /// <summary>
        /// Draws a path using the provided parameters.
        /// </summary>
        /// 
        /// <param name="path">
        /// The array of Vector3 points representing the path to be visualised.
        /// </param>
        /// 
        /// <param name="traversalType">
        /// The type of traversal behaviour for the path, determining how the path is visualised. For example, 
        /// loop visualises an additional line between the last and first point.
        /// </param>
        /// 
        /// <param name="previousPointIndex">
        /// The index of the previous point in the path. Used to determine which line segment to highlight as active.
        /// </param>
        /// 
        /// <param name="targetPointIndex">
        /// The index of the target point in the path. Used to determine which line segment to highlight as active.
        /// </param>
        public static void DrawPath(IReadOnlyList<Vector3> path, PathTraversalType traversalType = PathTraversalType.Once, 
                                    int previousPointIndex = 0, int targetPointIndex = 0)
        {
            if (!IsPathValid(path, previousPointIndex, targetPointIndex))
            {
                return;
            }

            if (!TryValidateTraversal(previousPointIndex, targetPointIndex, path.Count, traversalType, out bool isWrapping))
            {
                return;
            }

            ref readonly LineSettings lineSettings = ref Settings.GetLineSettings();
            const int INVALID_INDEX = -1;

            // INFO: Lines
            int lowerIndex = previousPointIndex.Equals(targetPointIndex) ? INVALID_INDEX : Mathf.Min(previousPointIndex, targetPointIndex);
            for (int i = 0; i < path.Count; ++i)
            {
                // INFO: Skip Current Line Between Previous and Target Point if Not Wrapping
                if (i == lowerIndex && !isWrapping)
                {
                    continue;
                }

                // INFO: Draw Inactive Path Line
                if (i + 1 < path.Count)
                {
                    Vector3 currentPoint = path[i];
                    Vector3 nextPoint = path[i + 1];
                    Vector3 midpoint = (currentPoint + nextPoint) / 2.0f;
                    DrawOpaqueLine(currentPoint, nextPoint, ScreenToWorldScale(lineSettings.Thickness, midpoint), lineSettings.DefaultColour);
                }
            }

            // INFO: Draw Inactive Path Line Between Last and First Point if Not Wrapping and Traversal Type is Loop
            if (!isWrapping && traversalType == PathTraversalType.Loop)
            {
                Vector3 lastPoint = path[^1];
                Vector3 firstPoint = path[0];
                Vector3 midpoint = (lastPoint + firstPoint) / 2.0f;
                DrawOpaqueLine(lastPoint, firstPoint, ScreenToWorldScale(lineSettings.Thickness, midpoint), lineSettings.DefaultColour);
            }

            // INFO: Draw Active Path Line
            if (previousPointIndex != targetPointIndex)
            {
                Vector3 previousPoint = path[previousPointIndex];
                Vector3 targetPoint = path[targetPointIndex];
                Vector3 midpoint = (previousPoint + targetPoint) / 2.0f;
                DrawOpaqueLine(previousPoint, targetPoint, ScreenToWorldScale(lineSettings.Thickness, midpoint), lineSettings.ActiveColour);
            }

            // INFO: Draw Path Points
            for (int i = 0; i < path.Count; ++i)
            {
                Vector3 point = path[i];
                DrawPoint(point, (i + 1).ToString());
            }
        }

        /// <summary>
        /// Overloaded method for DrawPath that accepts a Vector2 array for 2D path visualisation.
        /// </summary>
        /// 
        /// <param name="path">
        /// The array of Vector2 points representing the path to be visualised.
        /// </param>
        /// 
        /// <param name="traversalType">
        /// The type of traversal behaviour for the path, determining how the path is visualised.
        /// For example, Loop visualises an additional line between the last and first point.
        /// </param>
        /// 
        /// <param name="previousPointIndex">
        /// The index of the previous point in the path. Used to determine which line segment to highlight as active.
        /// </param>
        /// 
        /// <param name="targetPointIndex">
        /// The index of the target point in the path. Used to determine which line segment to highlight as active.
        /// </param>
        /// 
        /// <remarks>
        /// Internally converts to Vector3 with z = 0.
        /// </remarks>
        public static void DrawPath(IReadOnlyList<Vector2> path, PathTraversalType traversalType = PathTraversalType.Once,
                                      int previousPointIndex = 0, int targetPointIndex = 0)
        {
            // INFO: Convert Vector2 path to Vector3
            Vector3[] pathArray = new Vector3[path.Count];
            for (int i = 0; i < path.Count; ++i)
            {
                pathArray[i] = path[i];
            }

            DrawPath(pathArray, traversalType, previousPointIndex, targetPointIndex);
        }

        private static bool IsPathValid(IReadOnlyList<Vector3> path, int previousPointIndex, int nextPointIndex)
        {
            if (path == null || path.Count == 0)
            {
                return false;
            }

            // INFO: Ensure Indices are Within Bounds of the Path Array
            if (previousPointIndex < 0 || nextPointIndex < 0 || previousPointIndex >= path.Count || nextPointIndex >= path.Count)
            {
                Message.LogError($"Indices must be within the bounds of the path. Path Count: {path.Count}, " +
                                 $"Previous Point Index: {previousPointIndex}, Next Point Index: {nextPointIndex}");
                return false;
            }

            return true;
        }

        private static bool TryValidateTraversal(int previousPointIndex, int targetPointIndex, int pathLength, 
                                                 PathTraversalType traversalType, out bool isWrapping)
        {
            isWrapping = false;

            // INFO: Check for Wrapping from Last to First Point in Loop Traversal Type
            if (previousPointIndex == pathLength - 1 && targetPointIndex == 0 && traversalType == PathTraversalType.Loop)
            {
                isWrapping = true;
                return true;
            }

            // INFO: Ensure Indices are Adjacent or the Same for Non-Wrapping Cases
            if (Mathf.Abs(previousPointIndex - targetPointIndex) > 1)
            {
                Message.LogError($"Indices must be adjacent or the same. Previous Point Index: {previousPointIndex}, " +
                                 $"Next Point Index: {targetPointIndex}");
                return false;
            }

            return true;
        }

        private static float ScreenToWorldScale(float value, Vector3 position)
        {
            float handleSize = Mathf.Max(HandleUtility.GetHandleSize(position), 0.0001f);
            return Mathf.Clamp(value / handleSize, 0.0f, value * MAX_WORLD_SCALE_MULTIPLIER);
        }

        private static int ScreenToWorldScale(int value, Vector3 position)
        {
            float handleSize = Mathf.Max(HandleUtility.GetHandleSize(position), 0.0001f);
            return Mathf.RoundToInt(value / handleSize);
        }

        private static void DrawPoint(Vector3 position, string label)
        {
            // INFO: Disc at Position
            ref readonly DiscSettings discSettings = ref Settings.GetDiscSettings();
            Color previousColour = Handles.color;
            Vector3 forward = Camera.current != null ? Camera.current.transform.forward : Vector3.forward;

            Handles.color = discSettings.Colour;
            Handles.DrawSolidDisc(position, forward, discSettings.Radius);
            Handles.color = previousColour;

            // INFO: Label at Position
            ref readonly LabelSettings labelSettings = ref Settings.GetLabelSettings();
            LabelStyle.normal.textColor = labelSettings.TextColour;
            LabelStyle.fontSize = ScreenToWorldScale(labelSettings.FontSize, position);
            Handles.Label(position, label, LabelStyle);
        }

        private static void DrawOpaqueLine(Vector3 p1, Vector3 p2, float thickness, Color colour)
        {
            Color previousColour = Handles.color;

            // INFO: Extra lines drawn to give higher alpha value
            Handles.color = colour;
            for (int i = 0; i < LINE_DRAW_REPETITIONS; ++i)
            {
                Handles.DrawLine(p1, p2, thickness);
            }
            Handles.color = previousColour;
        }
    }
#endif
}
