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
    /// and unique variables based on different detection types.
    /// </summary>
    [Serializable]
    public class SpatialData
    {
        [Tooltip("Specifies which layers are checked when performing detection.")]
        public LayerMask DetectionMask = ~0;

        [Tooltip("Specifies which layers can block line of sight.")]
        public LayerMask ObstacleMask = ~0;

        [Header("Detection Type Settings")]
        [Tooltip("Determines the type of method to use for detecting targets.")]
        public DetectionType DetectionType = DetectionType.Range;

        [Tooltip("Allows you to specify an offset from the object's central position before detection calculations are carried out.")]
        public Vector3 DetectionOffset = Vector3.zero;

        [Tooltip("Determines the distance from the object within which targets can be detected.")]
        [HideField(nameof(DetectionType), DetectionType.Bounds), Min(0.0f)] public float Radius = 5.0f;

        [Tooltip("Determines the field of view within which targets can be detected.")]
        [ShowField(nameof(DetectionType), DetectionType.FieldOfView), Range(0.0f, 360.0f)] public float Angle = 180.0f;

        [Tooltip("Allows you to specify a rotation offset ontop of the object's current rotation before detection calculations are carried out.")]
        [ShowField(nameof(DetectionType), DetectionType.FieldOfView), Range(0.0f, 360.0f)] public float RotationOffset = 0.0f;

        [Tooltip("Determines the dimensions of the detection area within which targets can be detected.")]
        [ShowField(nameof(DetectionType), DetectionType.Bounds), Min(0.0f)] public Vector3 BoundsSize = Vector3.one;
    }

    public abstract class BaseDetectionComponent : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private DebugDrawType debugDrawType = DebugDrawType.WhenSelected;

        [Space(10.0f)]

        [SerializeField] protected SpatialData spatialData;

        private protected const int MAX_TARGETS = 10;

        private void OnDrawGizmos()
        {
            if (debugDrawType != DebugDrawType.Always)
            {
                return;
            }

            DrawDetectionGizmos(spatialData);
        }

        private void OnDrawGizmosSelected()
        {
            if (debugDrawType != DebugDrawType.WhenSelected)
            {
                return;
            }

            DrawDetectionGizmos(spatialData);
        }

        /// <summary>
        /// Scans for targets within the detection area based on the provided spatial data settings.
        /// </summary>
        /// 
        /// <param name="spatialData">
        /// Holds the configuration settings for spatial detection.
        /// </param>
        /// 
        /// <remarks>
        /// A valid target is determined by checking if the target is not null, not the same as the current transform
        /// and is in line of sight.
        /// </remarks>
        /// 
        /// <returns>
        /// An array of transforms representing the targets that have been detected.
        /// </returns>
        public abstract Transform[] ScanForTargets(SpatialData spatialData);

        /// <summary>
        /// Scans for targets within the detection area based on the detection components spatial data settings.
        /// </summary>
        /// 
        /// <remarks>
        /// A valid target is determined by checking if the target is not null, not the same as the current transform
        /// and is in line of sight.
        /// </remarks>
        /// 
        /// <returns>
        /// An array of transforms representing the targets that have been detected.
        /// </returns>
        public Transform[] ScanForTargets() => ScanForTargets(spatialData);

        /// <summary>
        /// Scans for a target within the detection area based on the provided spatial data settings.
        /// </summary>
        /// 
        /// <param name="spatialData">
        /// Holds the configuration settings for spatial detection.
        /// </param>
        /// 
        /// <remarks>
        /// A valid target is determined by checking if the target is not null, not the same as the current transform
        /// and is in line of sight.
        /// </remarks>
        /// 
        /// <returns>
        /// A transform representing the target that has been detected, or null if no target has been detected.
        /// </returns>
        public abstract Transform ScanForTarget(SpatialData spatialData);

        /// <summary>
        /// Scans for a target within the detection area based on the detection components spatial data settings.
        /// </summary>
        /// 
        /// <remarks>
        /// A valid target is determined by checking if the target is not null, not the same as the current transform
        /// and is in line of sight.
        /// </remarks>
        /// 
        /// <returns>
        /// A transform representing the target that has been detected, or null if no target has been detected.
        /// </returns>
        public Transform ScanForTarget() => ScanForTarget(spatialData);

        /// <summary>
        /// Checks if the target is detected based on the current detection type and provided spatial data settings.
        /// </summary>
        /// 
        /// <param name="target">
        /// The target we are checking for detection.
        /// </param>
        /// 
        /// <param name="spatialData">
        /// Holds the configuration settings for spatial detection.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the target has been detected, <see langword="false"/> otherwise.
        /// </returns>
        public bool IsTargetDetected(Transform target, SpatialData spatialData)
        {
            return spatialData.DetectionType switch
            {
                DetectionType.Range => CheckTargetInRange(target, spatialData),
                DetectionType.FieldOfView => CheckTargetInFieldOfView(target, spatialData),
                DetectionType.Bounds => CheckTargetInBounds(target, spatialData),
                _ => false
            };
        }

        /// <summary>
        /// Checks if the target is detected based on the current detection type and the detection components spatial data settings.
        /// </summary>
        /// 
        /// <param name="target">
        /// The target we are checking for detection.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the target has been detected, <see langword="false"/> otherwise.
        /// </returns>
        public bool IsTargetDetected(Transform target) => IsTargetDetected(target, spatialData);

        /// <summary>
        /// Checks if there is a clear line of sight to the target based on the provided spatial data settings.
        /// </summary>
        /// 
        /// <param name="target">
        /// The target we are checking line of sight to.
        /// </param>
        /// 
        /// <param name="spatialData">
        /// Holds the configuration settings for spatial detection.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the target is in line of sight, <see langword="false"/> otherwise.
        /// </returns>
        public abstract bool HasLineOfSight(Transform target, SpatialData spatialData);

        /// <summary>
        /// Checks if there is a clear line of sight to the target based on the detection components spatial data settings.
        /// </summary>
        /// 
        /// <param name="target">
        /// The target we are checking line of sight to.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the target is in line of sight, <see langword="false"/> otherwise.
        /// </returns>
        public bool HasLineOfSight(Transform target) => HasLineOfSight(target, spatialData);

        /// <summary>
        /// Checks if the target is in range based on the provided spatial data settings.
        /// </summary>
        /// 
        /// <param name="target">
        /// The target we are checking if it is in range.
        /// </param>
        /// 
        /// <param name="spatialData">
        /// Holds the configuration settings for spatial detection.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the target is in range, <see langword="false"/> otherwise.
        /// </returns>
        public abstract bool CheckTargetInRange(Transform target, SpatialData spatialData);

        /// <summary>
        /// Checks if the target is in range based on the detection components spatial data settings.
        /// </summary>
        /// 
        /// <param name="target">
        /// The target we are checking if it is in range.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the target is in range, <see langword="false"/> otherwise.
        /// </returns>
        public bool CheckTargetInRange(Transform target) => CheckTargetInRange(target, spatialData);

        /// <summary>
        /// Checks if the target is in the field of view based on the provided spatial data settings.
        /// </summary>
        /// 
        /// <param name="target">
        /// The target we are checking if it is in the field of view.
        /// </param>
        /// 
        /// <param name="spatialData">
        /// Holds the configuration settings for spatial detection.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the target is in the field of view, <see langword="false"/> otherwise.
        /// </returns>
        public abstract bool CheckTargetInFieldOfView(Transform target, SpatialData spatialData);

        /// <summary>
        /// Checks if the target is in the field of view based on the detection components spatial data settings.
        /// </summary>
        /// 
        /// <param name="target">
        /// The target we are checking if it is in the field of view.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the target is in the field of view, <see langword="false"/> otherwise.
        /// </returns>
        public bool CheckTargetInFieldOfView(Transform target) => CheckTargetInFieldOfView(target, spatialData);

        /// <summary>
        /// Checks if the target is in bounds based on the provided spatial data settings.
        /// </summary>
        /// 
        /// <param name="target">
        /// The target we are checking if it is in bounds.
        /// </param>
        /// 
        /// <param name="spatialData">
        /// Holds the configuration settings for spatial detection.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the target is in bounds, <see langword="false"/> otherwise.
        /// </returns>
        public abstract bool CheckTargetInBounds(Transform target, SpatialData spatialData);

        /// <summary>
        /// Checks if the target is in bounds based on the detection components spatial data settings.
        /// </summary>
        /// 
        /// <param name="target">
        /// The target we are checking if it is in bounds.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the target is in bounds, <see langword="false"/> otherwise.
        /// </returns>
        public bool CheckTargetInBounds(Transform target) => CheckTargetInBounds(target, spatialData);

        internal abstract void DrawDetectionGizmos(SpatialData spatialData);

        private protected bool IsTargetValid(Transform target, SpatialData spatialData)
        {
            return target != null && target != transform && HasLineOfSight(target, spatialData);
        }
    }
}
