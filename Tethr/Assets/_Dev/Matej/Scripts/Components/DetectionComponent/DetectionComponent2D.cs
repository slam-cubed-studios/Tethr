// Copyright (c) 2026, TheMGLegends. All rights reserved.

using System;
using Tethr.DetectionVisualiser;
using UnityEngine;

namespace Tethr
{
    /// <summary>
    /// 2D version of the DetectionComponentBase. It uses 2D physics methods for detecting targets and checking line of sight, and 
    /// it also provides 2D-specific implementations for checking if a target is within range, field of view, or bounds.
    /// </summary>
    public class DetectionComponent2D : DetectionComponentBase
    {
        /// <summary>
        /// Scans for targets within the detection area based on the configured detection type using 2D physics methods. 
        /// </summary>
        /// 
        /// <remarks>
        /// This method does not perform any checks for line of sight or whether the target is within the field of view. It is intended
        /// as an optional and efficient way to get potential targets within the detection area, which can then be further filtered using 
        /// <see cref="DetectionComponentBase.IsTargetDetected(Transform)"/> or other lower-level checks.
        /// </remarks>
        /// 
        /// <returns>
        /// An array of found targets or an empty array if no targets are detected.
        /// </returns>
        public override Transform[] ScanForTargets()
        {
            Vector2 point = transform.position + spatialData.detectionOffset;

            Collider2D[] colliders = spatialData.detectionType switch
            {
                DetectionType.Range => Physics2D.OverlapCircleAll(point, spatialData.radius, spatialData.detectionMask),
                DetectionType.FieldOfView => Physics2D.OverlapCircleAll(point, spatialData.radius, spatialData.detectionMask),
                DetectionType.Bounds => Physics2D.OverlapBoxAll(point, spatialData.boundsSize, 0.0f),
                _ => Array.Empty<Collider2D>()
            };

            return Array.ConvertAll(colliders, collider => collider.transform);
        }

        /// <summary>
        /// Scans for a single target within the detection area based on the configured detection type using 2D physics methods.
        /// </summary>
        /// 
        /// <remarks>
        /// This method does not perform any checks for line of sight or whether the target is within the field of view. It is intended
        /// as an optional and efficient way to get a potential target within the detection area, which can then be further filtered using
        /// <see cref="DetectionComponentBase.IsTargetDetected(Transform)"/> or other lower-level checks.
        /// </remarks>
        /// 
        /// <returns>
        /// An array of found targets or an empty array if no targets are detected.
        /// </returns>
        public override Transform ScanForTarget()
        {
            Vector2 point = transform.position + spatialData.detectionOffset;

            Collider2D collider = spatialData.detectionType switch
            {
                DetectionType.Range => Physics2D.OverlapCircle(point, spatialData.radius, spatialData.detectionMask),
                DetectionType.FieldOfView => Physics2D.OverlapCircle(point, spatialData.radius, spatialData.detectionMask),
                DetectionType.Bounds => Physics2D.OverlapBox(point, spatialData.boundsSize, 0.0f, spatialData.detectionMask),
                _ => null
            };

            return collider != null ? collider.transform : null;
        }

        /// <summary>
        /// Used to check if there is a clear line of sight to the target using 2D physics methods and provided spatial data settings.
        /// </summary>
        /// 
        /// <returns> 
        /// <see langword="true"/> if the target is in line of sight, <see langword="false"/> otherwise.
        /// </returns>
        public override bool HasLineOfSight(Transform target, in SpatialData spatialData)
        {
            if (target == null)
            {
                return false;
            }

            Vector3 start = transform.position + spatialData.detectionOffset;
            RaycastHit2D hit = Physics2D.Linecast(start, target.position, spatialData.obstacleMask);
            if (hit.collider != null && hit.transform != target.transform)
            {
#if UNITY_EDITOR
                if (hit.transform == transform)
                {
                    Debug.LogWarning("Linecast hit the object from which it was called from. This may indicate an issue with " +
                                     "the obstacle mask or the object's layer.");
                }
#endif

                return false;
            }

            return true;
        }

        /// <summary>
        /// Used to check if the target is in range using 2D vector methods and provided spatial data settings.
        /// </summary>
        /// 
        /// <returns> 
        /// <see langword="true"/> if the target is in range, <see langword="false"/> otherwise.
        /// </returns>
        public override bool CheckTargetInRange(Transform target, in SpatialData spatialData)
        {
            if (target == null)
            {
                return false;
            }

            Vector3 position = transform.position + spatialData.detectionOffset;
            if (Vector2.Distance(position, target.position) <= spatialData.radius)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Used to check if the target is within the field of view using 2D vector methods and provided spatial data settings.
        /// </summary>
        /// 
        /// <returns> 
        /// <see langword="true"/> if the target is in the field of view, <see langword="false"/> otherwise.
        /// </returns>
        public override bool CheckTargetInFieldOfView(Transform target, in SpatialData spatialData)
        {
            if (!CheckTargetInRange(target, spatialData))
            {
                return false;
            }

            Vector3 position = transform.position + spatialData.detectionOffset;
            Vector2 directionToTarget = (target.position - position).normalized;

            float rotationZ = transform.eulerAngles.z - spatialData.rotationOffset;
            Vector2 fovFacingDirection = Vector3Utilities.DirectionFromAngleXY(rotationZ);

            float angleToTarget = Vector2.Angle(fovFacingDirection, directionToTarget);
            if (angleToTarget < spatialData.angle / 2)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Used to check if the target is within the bounds using the provided spatial data settings.
        /// </summary>
        /// 
        /// <remarks>
        /// Rotation is not currently taken into account for bounds detection, as the bounds are always axis-aligned.
        /// Disregards z-position for 2D bounds detection.
        /// </remarks>
        /// 
        /// <returns> 
        /// <see langword="true"/> if the target is in bounds, <see langword="false"/> otherwise.
        /// </returns>
        public override bool CheckTargetInBounds(Transform target, in SpatialData spatialData)
        {
            if (target == null)
            {
                return false;
            }

            // INFO: Disregard z-position for 2D bounds detection
            Vector3 centre = transform.position + spatialData.detectionOffset;
            centre.z = 0.0f;
            Vector3 targetPosition = target.position;
            targetPosition.z = 0.0f;

            // INFO: Disregard z-size for 2D bounds detection
            Vector3 boundsSize = spatialData.boundsSize;
            boundsSize.z = 0.0f;

            Bounds bounds = new(centre, boundsSize);
            if (bounds.Contains(targetPosition))
            {
                return true;
            }

            return false;
        }

        protected override void DrawDetectionGizmos()
        {
            Vector3 centre = transform.position + spatialData.detectionOffset;

            switch (spatialData.detectionType)
            {
                case DetectionType.Range:
                    DetectionVisualiserUtility.DrawRange2D(centre, spatialData.radius, spatialData.isTargetDetected);
                    break;
                case DetectionType.FieldOfView:
                    float rotationZ = transform.eulerAngles.z - spatialData.rotationOffset;
                    DetectionVisualiserUtility.DrawFieldOfView2D(centre, spatialData.angle, spatialData.radius, spatialData.isTargetDetected, rotationZ);
                    break;
                case DetectionType.Bounds:
                    // INFO: Disregard z-size for 2D bounds visualisation
                    Vector3 boundsSize = spatialData.boundsSize;
                    boundsSize.z = 0.0f;
                    DetectionVisualiserUtility.DrawBounds(centre, boundsSize, spatialData.isTargetDetected);
                    break;
                default:
                    break;
            }
        }
    }
}
