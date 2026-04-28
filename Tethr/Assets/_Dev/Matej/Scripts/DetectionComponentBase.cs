// Copyright (c) 2026, TheMGLegends. All rights reserved.

using EditorAttributes;
using System;
using UnityEngine;

namespace Tethr
{
    /// <summary>
    /// Specifies the type of detection method and data to use for detecting targets.
    /// </summary>
    public enum DetectionType
    {
        Range,
        FieldOfView,
        Bounds
    }

    /// <summary>
    /// Represents configuration settings for spatial detection, including detection and obstacle layers, detection type, offsets 
    /// and parameters for different detection methods.
    /// </summary>
    [Serializable]
    public struct SpatialData
    {
        [Tooltip("Detection mask specifies which layers are checked when performing detection.")]
        public LayerMask detectionMask;

        [Tooltip("Obstacle mask specifies which layers can block line of sight.")]
        public LayerMask obstacleMask;

        [Header("Detection Type Settings")]
        [Tooltip("Detection type determines the method used for detecting targets.")]
        public DetectionType detectionType;

        [Tooltip("Detection offset allows you to specify an offset from the object's position for detection calculations. ")]
        public Vector3 detectionOffset;

        [Tooltip("Radius determines the distance from the object within which targets can be detected.")]
        [HideField(nameof(detectionType), DetectionType.Bounds), Min(0.0f)] public float radius;

        [Tooltip("Angle determines the field of view for detection. It specifies the angle in degrees within which targets can be detected.")]
        [ShowField(nameof(detectionType), DetectionType.FieldOfView), Range(0.0f, 360.0f)] public float angle;

        [Tooltip("Rotation offset allows you to specify an additional rotation offset for field of view detection.")]
        [ShowField(nameof(detectionType), DetectionType.FieldOfView), Range(0.0f, 360.0f)] public float rotationOffset;

        [Tooltip("Bounds Size determines the dimensions of the detection area when using bounds detection.")]
        [ShowField(nameof(detectionType), DetectionType.Bounds)] public Vector3 boundsSize;

        [HideInInspector] public bool isTargetVisible;

        public SpatialData(bool useDefaults)
        {
            if (useDefaults)
            {
                detectionMask = ~0;
                obstacleMask = ~0;
                detectionType = DetectionType.Range;
                detectionOffset = Vector3.zero;
                radius = 5.0f;
                angle = 180.0f;
                rotationOffset = 0.0f;
                boundsSize = Vector3.one;
                isTargetVisible = false;
            }
            else
            {
                detectionMask = default;
                obstacleMask = default;
                detectionType = default;
                detectionOffset = default;
                radius = default;
                angle = default;
                rotationOffset = default;
                boundsSize = default;
                isTargetVisible = default;
            }
        }
    }

    public abstract class DetectionComponentBase : MonoBehaviour
    {
        private enum DebugDrawType
        {
            Always,
            WhenSelected,
            Never
        }

        [Header("Debug Settings")]
        [SerializeField] private DebugDrawType debugDrawType = DebugDrawType.WhenSelected;

        [Space(10.0f)]

        [SerializeField] protected SpatialData spatialData = new(true);

        private void OnDrawGizmos()
        {
            if (debugDrawType != DebugDrawType.Always)
            {
                return;
            }

            DrawDetectionGizmos();
        }

        private void OnDrawGizmosSelected()
        {
            if (debugDrawType != DebugDrawType.WhenSelected)
            {
                return;
            }

            DrawDetectionGizmos();
        }

        protected abstract void DrawDetectionGizmos();
    }
}
