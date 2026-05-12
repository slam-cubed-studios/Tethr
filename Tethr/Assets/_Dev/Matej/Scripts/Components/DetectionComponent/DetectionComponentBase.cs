// Copyright (c) 2026, TheMGLegends. All rights reserved.

using EditorAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
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
        [ShowField(nameof(detectionType), DetectionType.Bounds), Min(0.0f)] public Vector3 boundsSize;

        [HideInInspector] public bool isTargetDetected;

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
                isTargetDetected = false;
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
                isTargetDetected = default;
            }
        }
    }

    /// <summary>
    /// Provides an abstract base class for detection components that scan for targets and determine visibility/detection.
    /// </summary>
    public abstract class DetectionComponentBase : MonoBehaviour
    {
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

        public abstract Transform[] ScanForTargets();

        /// <summary>
        /// Retrieves an array of valid targets that have been found by the ScanForTargets method, filtering out any targets 
        /// that are null, the same as the current transform, or aren't in line of sight.
        /// </summary>
        public Transform[] GetValidTargets()
        {
            List<Transform> targets = ScanForTargets().ToList();
            targets.RemoveAll(target => target == null || target == transform || !HasLineOfSight(target, spatialData));
            return targets.ToArray();
        }

        public abstract Transform ScanForTarget();

        /// <summary>
        /// Retrieves a valid target that has been found by the ScanForTarget method, returning the target if it is not null, 
        /// not the same as the current transform, and is in line of sight, otherwise returns null.
        /// </summary>
        public Transform GetValidTarget()
        {
            Transform target = ScanForTarget();
            return target != null && target != transform && HasLineOfSight(target, spatialData) ? target : null;
        }

        /// <summary>
        /// Overloaded method used to check if the target is detected based on the current detection type and spatial 
        /// data settings.
        /// </summary>
        /// 
        /// <returns> 
        /// <see langword="true"/> if the target has been detected, <see langword="false"/> otherwise.
        /// </returns>
        public bool IsTargetDetected(Transform target) => IsTargetDetected(target, ref spatialData);

        /// <summary>
        /// Used to check if the target is detected based on the current detection type and provided spatial data settings.
        /// </summary>
        /// 
        /// <returns>
        /// <see langword="true"/> if the target has been detected, <see langword="false"/> otherwise.
        /// </returns>
        public bool IsTargetDetected(Transform target, ref SpatialData spatialData)
        {
            spatialData.isTargetDetected = spatialData.detectionType switch
            {
                DetectionType.Range => CheckTargetInRange(target, spatialData),
                DetectionType.FieldOfView => CheckTargetInFieldOfView(target, spatialData),
                DetectionType.Bounds => CheckTargetInBounds(target, spatialData),
                _ => false
            };

            return spatialData.isTargetDetected;
        }

        /// <summary>
        /// Overloaded method used to check if the target is in line of sight based on the current spatial data settings.
        /// </summary>
        /// 
        /// <returns> 
        /// <see langword="true"/> if the target is in line of sight, <see langword="false"/> otherwise.
        /// </returns>
        public bool HasLineOfSight(Transform target) => HasLineOfSight(target, spatialData);

        public abstract bool HasLineOfSight(Transform target, in SpatialData spatialData);

        /// <summary>
        /// Overloaded method used to check if the target is in range based on the current spatial data settings.
        /// </summary>
        /// 
        /// <returns> 
        /// <see langword="true"/> if the target is in range, <see langword="false"/> otherwise.
        /// </returns>
        public bool CheckTargetInRange(Transform target) => CheckTargetInRange(target, spatialData);

        public abstract bool CheckTargetInRange(Transform target, in SpatialData spatialData);

        /// <summary>
        /// Overloaded method used to check if the target is in the field of view based on the current spatial data settings.
        /// </summary>
        /// 
        /// <returns> 
        /// <see langword="true"/> if the target is in the field of view, <see langword="false"/> otherwise.
        /// </returns>
        public bool CheckTargetInFieldOfView(Transform target) => CheckTargetInFieldOfView(target, in spatialData);

        public abstract bool CheckTargetInFieldOfView(Transform target, in SpatialData spatialData);

        /// <summary>
        /// Overloaded method used to check if the target is in bounds based on the current spatial data settings.
        /// </summary>
        /// 
        /// <returns> 
        /// <see langword="true"/> if the target is in bounds, <see langword="false"/> otherwise.
        /// </returns>
        public bool CheckTargetInBounds(Transform target) => CheckTargetInBounds(target, spatialData);

        public abstract bool CheckTargetInBounds(Transform target, in SpatialData spatialData);

        protected abstract void DrawDetectionGizmos();
    }
}
