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

    [InitializeOnLoad]
    public static class PathVisualiserUtility
    {
#if UNITY_EDITOR
        private static PathVisualiserSettings settings;
        private static PathVisualiserSettings Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = PathVisualiserSettings.GetOrCreateSettings();
                }

                return settings;
            }
        }
        
        private static GUIStyle labelStyle;
        private static GUIStyle LabelStyle
        {
            get
            {
                if (labelStyle == null)
                {
                    CreateLabelStyle();
                }

                return labelStyle;
            }
        }

        static PathVisualiserUtility()
        {
            CreateLabelStyle();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForPlayMode()
        {
            labelStyle = null;
            settings = null;

            CreateLabelStyle();
        }

        private static void CreateLabelStyle()
        {
            labelStyle = new GUIStyle()
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }

        public static void DrawTargetLine(Vector3 currentPosition, Vector3 targetPosition)
        {
            // INFO: Draw Line Between Follower and Target Position
            LineSettings lineSettings = Settings.GetLineSettings();

            Handles.color = lineSettings.activeColour;
            Handles.DrawLine(currentPosition, targetPosition, ToWorldSpace(lineSettings.thickness, (currentPosition + targetPosition) / 2.0f));

            DiscSettings discSettings = Settings.GetDiscSettings();
            LabelSettings labelSettings = Settings.GetLabelSettings();
            Handles.color = discSettings.colour;
            LabelStyle.normal.textColor = labelSettings.textColour;
            Vector3 forward = Camera.current != null ? Camera.current.transform.forward : Vector3.forward;

            // INFO: Draw Follower Disc and Label
            Handles.DrawSolidDisc(currentPosition, forward, discSettings.radius);

            LabelStyle.fontSize = ToWorldSpace(labelSettings.fontSize, currentPosition);
            Handles.Label(currentPosition, (1).ToString(), LabelStyle);

            // INFO: Draw Target Disc and Label
            Handles.DrawSolidDisc(targetPosition, forward, discSettings.radius);

            LabelStyle.fontSize = ToWorldSpace(labelSettings.fontSize, targetPosition);
            Handles.Label(targetPosition, (2).ToString(), LabelStyle);
        }

        public static void DrawPath(Vector3[] path, int previousPointIndex = 0, int targetPointIndex = 0, PathTraversalType traversalType = PathTraversalType.Linear)
        {
            if (!IsPathValid(path, previousPointIndex, targetPointIndex, traversalType))
            {
                return;
            }

            bool isWrapping = false;
            if (!AreIndicesValid(previousPointIndex, targetPointIndex, path.Length, traversalType, ref isWrapping))
            {
                return;
            }

            LineSettings lineSettings = Settings.GetLineSettings();

            // INFO: Draw Path Lines
            Handles.color = lineSettings.defaultColour;
            int lowerIndex = Mathf.Min(previousPointIndex, targetPointIndex);
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
                    Handles.DrawLine(currentPoint, nextPoint, ToWorldSpace(lineSettings.thickness, (currentPoint + nextPoint) / 2.0f));
                }
            }

            // INFO: If Not Wrapping, and Traversal Type is Loop, Draw Line Between Last and First Point
            if (!isWrapping && traversalType == PathTraversalType.Loop)
            {
                Vector3 lastPoint = path[^1];
                Vector3 firstPoint = path[0];
                Handles.DrawLine(lastPoint, firstPoint, ToWorldSpace(lineSettings.thickness, (lastPoint + firstPoint) / 2.0f));
            }

            // INFO: Draw Current Path Line
            if (previousPointIndex != targetPointIndex)
            {
                Handles.color = lineSettings.activeColour;
                Vector3 previousPoint = path[previousPointIndex];
                Vector3 targetPoint = path[targetPointIndex];
                Handles.DrawLine(previousPoint, targetPoint, ToWorldSpace(lineSettings.thickness, (previousPoint + targetPoint) / 2.0f));
            }

            // INFO: Draw Path Discs and Labels
            DiscSettings discSettings = Settings.GetDiscSettings();
            LabelSettings labelSettings = Settings.GetLabelSettings();
            Handles.color = discSettings.colour;
            LabelStyle.normal.textColor = labelSettings.textColour;
            Vector3 forward = Camera.current != null ? Camera.current.transform.forward : Vector3.forward;
            for (int i = 0; i < path.Length; ++i)
            {
                Vector3 point = path[i];
                Handles.DrawSolidDisc(point, forward, discSettings.radius);

                LabelStyle.fontSize = ToWorldSpace(labelSettings.fontSize, point);
                Handles.Label(point, (i + 1).ToString(), LabelStyle);
            }
        }

        private static bool IsPathValid(Vector3[] path, int previousPointIndex, int nextPointIndex, PathTraversalType traversalType)
        {
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
        }

        private static bool AreIndicesValid(int previousPointIndex, int targetPointIndex, int pathLength, PathTraversalType traversalType, ref bool isWrapping)
        {
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
        }

        private static float ToWorldSpace(float value, Vector3 position)
        {
            return value / HandleUtility.GetHandleSize(position);
        }

        private static int ToWorldSpace(int value, Vector3 position)
        {
            return Mathf.RoundToInt(value / HandleUtility.GetHandleSize(position));
        }
#endif
    }
}
