// Copyright (c) 2026, TheMGLegends. All rights reserved.

using UnityEngine;
using System;
using UnityEngine.UIElements;



#if UNITY_EDITOR
using UnityEditor;

namespace Tethr.DetectionVisualiser
{
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

        public static void DrawRange(Vector3 centre, float radius, bool isTargetVisible = false)
        {
            if (!CanDrawRange(ref radius))
            {
                return;
            }

            DrawRangeHandles(centre, radius, isTargetVisible, Vector3.up);
        }

        public static void DrawRange2D(Vector3 centre, float radius, bool isTargetVisible = false)
        {
            if (!CanDrawRange(ref radius))
            {
                return;
            }

            DrawRangeHandles(centre, radius, isTargetVisible, Vector3.forward);
        }

        private static bool CanDrawRange(ref float radius)
        {
            // INFO: No need to draw a range if the radius is 0
            if (radius == 0.0f)
            {
                return false;
            }

            // INFO: Ensure radius is positive
            radius = Mathf.Abs(radius);

            return true;
        }

        private static void DrawRangeHandles(Vector3 centre, float radius, bool isTargetVisible, Vector3 normal)
        {
            DetectionSettings detectionSettings = Settings.DetectionSettings();
            Color colour = isTargetVisible ? detectionSettings.activeColour : detectionSettings.defaultColour;
            Color alphaColour = colour;
            alphaColour.a = detectionSettings.alpha;

            Handles.color = colour;
            Handles.DrawWireDisc(centre, normal, radius, detectionSettings.thickness);

            Handles.color = alphaColour;
            Handles.DrawSolidDisc(centre, normal, radius);
        }

        public static void DrawFieldOfView(Vector3 centre, float angle, float radius, bool isTargetVisible = false, float rotation = 0.0f)
        {
            if (!CanDrawFieldOfView(ref angle, ref radius))
            {
                return;
            }

            Tuple<Vector3, Vector3> viewDirections = GetViewDirections(angle, rotation);
            DrawFieldOfViewHandles(centre, angle, radius, isTargetVisible, Vector3.up, viewDirections.Item2, viewDirections.Item1);
        }

        public static void DrawFieldOfView2D(Vector3 centre, float angle, float radius, bool isTargetVisible = false, float rotation = 0.0f)
        {
            if (!CanDrawFieldOfView(ref angle, ref radius))
            {
                return;
            }

            Tuple<Vector3, Vector3> viewDirections = GetViewDirections2D(angle, rotation);
            DrawFieldOfViewHandles(centre, angle, radius, isTargetVisible, Vector3.forward, viewDirections.Item1, viewDirections.Item2);
        }

        private static bool CanDrawFieldOfView(ref float angle, ref float radius)
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

            // INFO: No need to draw a field of view if the radius is 0
            if (radius == 0.0f)
            {
                return false;
            }

            // INFO: Ensure radius is positive
            radius = Mathf.Abs(radius);

            return true;
        }

        private static void DrawFieldOfViewHandles(Vector3 centre, float angle, float radius, bool isTargetVisible,
                                                   Vector3 normal, Vector3 viewDirectionA, Vector3 viewDirectionB)
        {
            DetectionSettings detectionSettings = Settings.DetectionSettings();
            Color colour = isTargetVisible ? detectionSettings.activeColour : detectionSettings.defaultColour;
            Color alphaColour = colour;
            alphaColour.a = detectionSettings.alpha;

            Handles.color = colour;
            Handles.DrawWireArc(centre, normal, viewDirectionB, angle, radius, detectionSettings.thickness);

            // INFO: No need to draw the lines if the angle is 360 degrees
            if (angle < 360.0f)
            {
                Handles.DrawLine(centre, centre + viewDirectionA * radius, detectionSettings.thickness);
                Handles.DrawLine(centre, centre + viewDirectionB * radius, detectionSettings.thickness);
            }

            Handles.color = alphaColour;
            Handles.DrawSolidArc(centre, normal, viewDirectionB, angle, radius);
        }

        private static Tuple<Vector3, Vector3> GetViewDirections(float angle, float rotation)
        {
            float halfAngle = angle / 2.0f;
            Vector3 viewDirectionA = DirectionFromAngle(-halfAngle, rotation);
            Vector3 viewDirectionB = DirectionFromAngle(halfAngle, rotation);
            return new Tuple<Vector3, Vector3>(viewDirectionA, viewDirectionB);
        }

        private static Tuple<Vector3, Vector3> GetViewDirections2D(float angle, float rotation)
        {
            float halfAngle = angle / 2.0f;
            Vector3 viewDirectionA = DirectionFromAngle2D(-halfAngle, rotation);
            Vector3 viewDirectionB = DirectionFromAngle2D(halfAngle, rotation);
            return new Tuple<Vector3, Vector3>(viewDirectionA, viewDirectionB);
        }

        private static Vector3 DirectionFromAngle(float angle, float rotation)
        {
            angle -= rotation;
            float angleInRadians = angle * Mathf.Deg2Rad;

            // INFO: X-Z Plane (Ground)
            return new Vector3(Mathf.Sin(angleInRadians), 0.0f, Mathf.Cos(angleInRadians));
        }

        private static Vector3 DirectionFromAngle2D(float angle, float rotation)
        {
            angle -= rotation;
            float angleInRadians = angle * Mathf.Deg2Rad;

            // INFO: X-Y Plane (2D)
            return new Vector3(Mathf.Sin(angleInRadians), Mathf.Cos(angleInRadians), 0.0f);
        }

        public static void DrawBounds(Vector3 centre, Vector3 size, bool isTargetVisible = false)
        {
            if (!CanDrawBounds(ref size))
            {
                return;
            }

            DrawBoundsHandles(centre, size, isTargetVisible);
        }

        private static bool CanDrawBounds(ref Vector3 size)
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

        private static void DrawBoundsHandles(Vector3 centre, Vector3 size, bool isTargetVisible)
        {
            DetectionSettings detectionSettings = Settings.DetectionSettings();
            Color colour = isTargetVisible ? detectionSettings.activeColour : detectionSettings.defaultColour;
            Color alphaColour = colour;
            alphaColour.a = detectionSettings.alpha;

            Handles.color = colour;
            Handles.DrawWireCube(centre, size);

            Gizmos.color = alphaColour;
            Gizmos.DrawCube(centre, size);
        }
    }
}
#endif
