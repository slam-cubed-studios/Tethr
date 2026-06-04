// Copyright (c) 2026, TheMGLegends. All rights reserved.

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tethr.DetectionVisualiser
{
#if UNITY_EDITOR
    /// <summary>
    /// Provides utility methods for visualising detection areas in the Unity Editor Scene view.
    /// </summary>
    /// 
    /// <remarks>
    /// Supports both 2D and 3D detection visualisation for Range, Field of View, and Bounds types. Intended 
    /// for use within the Unity Editor. Ideally called from OnDrawGizmos or similar editor-only contexts.
    /// </remarks>
    public static class DetectionVisualiserUtility
    {
        private static DetectionVisualiserSettings settings;

        private static DetectionVisualiserSettings Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = DetectionVisualiserSettings.instance;
                }

                return settings;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Deinitialise()
        {
            settings = null;
        }

        /// <summary>
        /// Draws a circular range visualisation in the X-Z plane (Ground).
        /// </summary>
        /// 
        /// <param name="centre">
        /// The centre point of the range visualisation.
        /// </param>
        /// 
        /// <remarks>
        /// This is the 3D version of the method. For a 2D range visualisation, use <see cref="DrawRange2D(Vector3, float, bool)"/> instead.
        /// </remarks>
        public static void DrawRange(Vector3 centre, float radius, bool isTargetDetected = false)
        {
            if (!ValidateRange(ref radius))
            {
                return;
            }

            DrawRangeHandles(centre, radius, isTargetDetected, true);
        }

        /// <summary>
        /// Draws a circular range visualisation in the X-Y plane (Vertical).
        /// </summary>
        /// 
        /// <param name="centre">
        /// The centre point of the range visualisation.
        /// </param>
        /// 
        /// <remarks>
        /// This is the 2D version of the method. For a 3D range visualisation, use <see cref="DrawRange(Vector3, float, bool)"/> instead.
        /// </remarks>
        public static void DrawRange2D(Vector3 centre, float radius, bool isTargetDetected = false)
        {
            if (!ValidateRange(ref radius))
            {
                return;
            }

            DrawRangeHandles(centre, radius, isTargetDetected, false);
        }

        /// <summary>
        /// Draws a field of view visualisation in the X-Z plane (Ground).
        /// </summary>
        /// 
        /// <param name="centre">
        /// The centre point of the field of view visualisation.
        /// </param>
        /// 
        /// <param name="angle">
        /// The angle of the field of view visualisation in degrees.
        /// </param>
        /// 
        /// <param name="rotation">
        /// The rotation of the game object in degrees, which offsets the field of view visualisation accordingly.
        /// </param>
        /// 
        /// <remarks>
        /// This is the 3D version of the method. For a 2D field of view visualisation, use 
        /// <see cref="DrawFieldOfView2D(Vector3, float, float, bool, float)"/> instead.
        /// </remarks>
        public static void DrawFieldOfView(Vector3 centre, float angle, float radius, bool isTargetDetected = false, float rotation = 0.0f)
        {
            if (!ValidateFieldOfView(ref angle, ref radius))
            {
                return;
            }

            float halfAngle = angle / 2.0f;
            Vector3 viewDirectionA = Vector3Utilities.DirectionFromAngleXZ(rotation + halfAngle);
            Vector3 viewDirectionB = Vector3Utilities.DirectionFromAngleXZ(rotation - halfAngle);
            DrawFieldOfViewHandles(centre, angle, radius, isTargetDetected, Vector3.up, viewDirectionA, viewDirectionB, true);
        }

        /// <summary>
        /// Draws a field of view visualisation in the X-Y plane (Vertical).
        /// </summary>
        /// 
        /// <param name="centre">
        /// The centre point of the field of view visualisation.
        /// </param>
        /// 
        /// <param name="angle">
        /// The angle of the field of view visualisation in degrees.
        /// </param>
        /// 
        /// <param name="rotation">
        /// The rotation of the game object in degrees, which offsets the field of view visualisation accordingly.
        /// </param>
        /// 
        /// <remarks>
        /// This is the 2D version of the method. For a 3D field of view visualisation, use 
        /// <see cref="DrawFieldOfView(Vector3, float, float, bool, float)"/> instead.
        /// </remarks>
        public static void DrawFieldOfView2D(Vector3 centre, float angle, float radius, bool isTargetDetected = false, float rotation = 0.0f)
        {
            if (!ValidateFieldOfView(ref angle, ref radius))
            {
                return;
            }

            float halfAngle = angle / 2.0f;
            Vector3 viewDirectionA = Vector3Utilities.DirectionFromAngleXY(rotation + halfAngle);
            Vector3 viewDirectionB = Vector3Utilities.DirectionFromAngleXY(rotation - halfAngle);
            DrawFieldOfViewHandles(centre, angle, radius, isTargetDetected, Vector3.forward, viewDirectionA, viewDirectionB, false);
        }

        /// <summary>
        /// Draws a bounds visualisation.
        /// </summary>
        /// 
        /// <param name="centre">
        /// The centre point of the bounds visualisation.
        /// </param>
        /// 
        /// <remarks>
        /// This method supports both 2D and 3D bounds visualisation.
        /// </remarks>
        public static void DrawBounds(Vector3 centre, Vector3 size, bool isTargetDetected = false)
        {
            if (!ValidateBounds(ref size))
            {
                return;
            }

            DrawBoundsHandles(centre, size, isTargetDetected);
        }

        private static bool ValidateRange(ref float radius)
        {
            // INFO: No need to draw a range if the radius is nearly 0
            if (Mathf.Approximately(radius, 0.0f))
            {
                return false;
            }

            // INFO: Ensure radius is positive
            radius = Mathf.Abs(radius);

            return true;
        }

        private static void DrawRangeHandles(Vector3 centre, float radius, bool isTargetDetected, bool draw3D)
        {
            float thickness = GetThickness();
            GetColours(isTargetDetected, out Color colour, out Color alphaColour);
            Color previousColour = Handles.color;

            Handles.color = colour;
            Handles.DrawWireDisc(centre, Vector3.forward, radius, thickness);

            if (draw3D)
            {
                Handles.DrawWireDisc(centre, Vector3.up, radius, thickness);
                Handles.DrawWireDisc(centre, Vector3.right, radius, thickness);

                Handles.color = alphaColour;
                Handles.SphereHandleCap(0, centre, Quaternion.identity, radius * 2.0f, EventType.Repaint);
            }
            else
            {
                Handles.color = alphaColour;
                Handles.DrawSolidDisc(centre, Vector3.forward, radius);
            }

            Handles.color = previousColour;
        }

        private static bool ValidateFieldOfView(ref float angle, ref float radius)
        {
            // INFO: No need to draw a field of view if the angle is 0 degrees
            if (angle == 0.0f)
            {
                return false;
            }

            // INFO: Ensure angle is positive and does not exceed 360 degrees
            angle = Mathf.Abs(angle);
            if (angle > 360.0f)
            {
                angle %= 360.0f;
            }

            // INFO: No need to draw a field of view if the radius is nearly 0
            if (Mathf.Approximately(radius, 0.0f))
            {
                return false;
            }

            // INFO: Ensure radius is positive
            radius = Mathf.Abs(radius);

            return true;
        }

        private static void DrawFieldOfViewHandles(Vector3 centre, float angle, float radius, bool isTargetDetected, Vector3 normal, 
                                                   Vector3 viewDirectionA, Vector3 viewDirectionB, bool draw3D)
        {
            float thickness = GetThickness();
            GetColours(isTargetDetected, out Color colour, out Color alphaColour);
            Color previousColour = Handles.color;

            Handles.color = colour;
            Handles.DrawWireArc(centre, normal, viewDirectionB, angle, radius, thickness);

            if (draw3D)
            {
                Handles.DrawWireDisc(centre, Vector3.up, radius, thickness);
                Handles.DrawWireDisc(centre, Vector3.right, radius, thickness);
                Handles.DrawWireDisc(centre, Vector3.forward, radius, thickness);
            }

            // INFO: No need to draw the lines if the angle is close to 360 degrees
            if (!Mathf.Approximately(angle, 360.0f))
            {
                Handles.DrawLine(centre, centre + viewDirectionA * radius, thickness);
                Handles.DrawLine(centre, centre + viewDirectionB * radius, thickness);
            }

            Handles.color = alphaColour;
            Handles.DrawSolidArc(centre, normal, viewDirectionB, angle, radius);

            Handles.color = previousColour;
        }

        private static bool ValidateBounds(ref Vector3 size)
        {
            // INFO: No need to draw bounds if the dimensions are 0
            if (size == Vector3.zero)
            {
                return false;
            }

            // INFO: Ensure dimensions are positive
            size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));

            return true;
        }

        private static void DrawBoundsHandles(Vector3 centre, Vector3 size, bool isTargetDetected)
        {
            GetColours(isTargetDetected, out Color colour, out Color alphaColour);
            Color previousHandlesColour = Handles.color;

            Handles.color = colour;
            Handles.DrawWireCube(centre, size);
            Handles.color = previousHandlesColour;

            Color previousGizmosColour = Gizmos.color;
            Gizmos.color = alphaColour;
            Gizmos.DrawCube(centre, size);
            Gizmos.color = previousGizmosColour;
        }

        private static void GetColours(bool isTargetDetected, out Color colour, out Color alphaColour)
        {
            ref readonly DetectionSettings detectionSettings = ref Settings.GetDetectionSettings();
            colour = isTargetDetected ? detectionSettings.ActiveColour : detectionSettings.DefaultColour;
            alphaColour = colour;
            alphaColour.a = detectionSettings.Alpha;
        }

        private static float GetThickness() => Settings.GetDetectionSettings().Thickness;
    }
#endif
}
