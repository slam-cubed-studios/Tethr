#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Tethr.PathVisualiser
{
    public enum PathTraversalType
    {
        Linear,
        Loop,
        PingPong
    }

    public static class PathVisualiserUtility
    {
        private static bool IsPathValid(Vector3[] path, int previousPointIndex, int nextPointIndex, PathTraversalType traversalType)
        {
#if UNITY_EDITOR
            if (path == null)
            {
                Debug.LogError("Path cannot be null.");
                return false;
            }

            // INFO: Ensure Indices are Within Bounds of the Path (Negative or Exceeding Length)
            if (previousPointIndex < 0 || nextPointIndex < 0 || previousPointIndex >= path.Length || nextPointIndex >= path.Length)
            {
                Debug.LogError($"Indices must be within the bounds of the path. Path Length: {path.Length}, " +
                               $"Previous Point Index: {previousPointIndex}, Next Point Index: {nextPointIndex}");
                return false;
            }

            return true;
#else 
            return false;
#endif
        }

        private static bool AreIndicesValid(int previousPointIndex, int targetPointIndex, int pathLength, PathTraversalType traversalType, ref bool isWrapping)
        {
#if UNITY_EDITOR
            // INFO: If the previous index marks the end of the path and wraps to the start of the path, this is valid if the traversal type is Loop
            if (previousPointIndex == pathLength - 1 && targetPointIndex == 0 && traversalType == PathTraversalType.Loop)
            {
                isWrapping = true;
                return true;
            }

            if (Mathf.Abs(previousPointIndex - targetPointIndex) > 1)
            {
                Debug.LogError($"Indices must be adjacent or the same. Previous Point Index: {previousPointIndex}, " +
                               $"Next Point Index: {targetPointIndex}");
                return false;
            }

            return true;
#else
            return false;
#endif
        }

        public static void DrawPath(Vector3[] path, int previousPointIndex = 0, int targetPointIndex = 0, PathTraversalType traversalType = PathTraversalType.Linear)
        {
#if UNITY_EDITOR
            if (!IsPathValid(path, previousPointIndex, targetPointIndex, traversalType))
            {
                return;
            }

            bool isWrapping = false;
            if (!AreIndicesValid(previousPointIndex, targetPointIndex, path.Length, traversalType, ref isWrapping))
            {
                return;
            }

            // TODO: Temporary until we make it a InitialiseOnLoad and cache style using provided preferences from Provider
            Vector3 forward = Camera.current != null ? Camera.current.transform.forward : Vector3.forward;
            GUIStyle style = new()
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            style.normal.textColor = Color.black;

            float thickness;
            int lowerIndex = Mathf.Min(previousPointIndex, targetPointIndex);

            // INFO: Draw Path Lines
            Handles.color = Color.red;
            for (int i = 0; i < path.Length; ++i)
            {
                // INFO: Skip Current Line Between Previous and Target Point
                if (i == lowerIndex && !isWrapping)
                {
                    continue;
                }

                if (i + 1 < path.Length)
                {
                    Vector3 currentPoint = path[i];
                    Vector3 nextPoint = path[i + 1];
                    thickness = 10.0f / HandleUtility.GetHandleSize((currentPoint + nextPoint) / 2.0f);
                    Handles.DrawLine(currentPoint, nextPoint, thickness);
                }
            }

            // INFO: If Not Wrapping, and Traversal Type is Loop, Draw Line Between Last and First Point
            if (!isWrapping && traversalType == PathTraversalType.Loop)
            {
                Vector3 lastPoint = path[^1];
                Vector3 firstPoint = path[0];
                thickness = 10.0f / HandleUtility.GetHandleSize((lastPoint + firstPoint) / 2.0f);
                Handles.DrawLine(lastPoint, firstPoint, thickness);
            }

            // INFO: Draw Current Path Line
            if (previousPointIndex != targetPointIndex)
            {
                Handles.color = Color.green;
                Vector3 previousPoint = path[previousPointIndex];
                Vector3 targetPoint = path[targetPointIndex];
                thickness = 10.0f / HandleUtility.GetHandleSize((previousPoint + targetPoint) / 2.0f);
                Handles.DrawLine(previousPoint, targetPoint, thickness);
            }

            // INFO: Draw Path Discs and Labels
            Handles.color = Color.white;
            for (int i = 0; i < path.Length; ++i)
            {
                Vector3 point = path[i];

                Handles.DrawSolidDisc(point, forward, 0.5f);

                style.fontSize = (int)(64 / HandleUtility.GetHandleSize(point));
                Handles.Label(point, (i + 1).ToString(), style);
            }
#endif
        }
    }
}
