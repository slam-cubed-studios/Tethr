// Copyright (c) 2026, TheMGLegends. All rights reserved.

using System;
using Tethr.DetectionVisualiser;
using UnityEngine;

namespace Tethr
{
    /// <summary>
    /// 3D version of the DetectionComponentBase. It uses 3D physics methods for detecting targets and checking line of sight, and 
    /// it also provides 3D-specific implementations for checking if a target is within range, field of view, or bounds.
    /// </summary>
    public class DetectionComponent : DetectionComponentBase
    {
        /// <summary>
        /// Scans for targets within the detection area based on the configured detection type using 3D physics methods.
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
            Vector3 position = transform.position + spatialData.detectionOffset;

            Collider[] colliders = spatialData.detectionType switch
            {
                DetectionType.Range => Physics.OverlapSphere(position, spatialData.radius, spatialData.detectionMask),
                DetectionType.FieldOfView => Physics.OverlapSphere(position, spatialData.radius, spatialData.detectionMask),
                DetectionType.Bounds => Physics.OverlapBox(position, spatialData.boundsSize / 2.0f, Quaternion.identity, spatialData.detectionMask),
                _ => Array.Empty<Collider>()
            };

            return Array.ConvertAll(colliders, collider => collider.transform);
        }

        /// <summary>
        /// Scans for a single target within the detection area based on the configured detection type using 3D physics methods.
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
            Vector3 position = transform.position + spatialData.detectionOffset;

            Collider[] colliders = new Collider[1];
            switch (spatialData.detectionType)
            {
                case DetectionType.Range:
                    Physics.OverlapSphereNonAlloc(position, spatialData.radius, colliders, spatialData.detectionMask);
                    break;
                case DetectionType.FieldOfView:
                    Physics.OverlapSphereNonAlloc(position, spatialData.radius, colliders, spatialData.detectionMask);
                    break;
                case DetectionType.Bounds:
                    Vector3 boundsHalfExtents = spatialData.boundsSize / 2.0f;
                    Physics.OverlapBoxNonAlloc(position, boundsHalfExtents, colliders, Quaternion.identity, spatialData.detectionMask);
                    break;
                default:
                    break;
            }

            return colliders[0] != null ? colliders[0].transform : null;
        }

        /// <summary>
        /// Used to check if there is a clear line of sight to the target using 3D physics methods and provided spatial data settings.
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
            if (Physics.Linecast(start, target.position, out RaycastHit hit, spatialData.obstacleMask) && hit.transform != target.transform)
            {
#if UNITY_EDITOR
                if (hit.transform == transform)
                {
                    Debug.LogWarning("Linecast hit the object from which it was called from. This may indicate an issue with the obstacle mask or " +
                                     "the object's layer.");
                }
#endif

                return false;
            }

            return true;
        }

        /// <summary>
        /// Used to check if the target is in range using 3D vector methods and provided spatial data settings.
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
            if (Vector3.Distance(position, target.position) <= spatialData.radius)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Used to check if the target is within the field of view using 3D vector methods and provided spatial data settings.
        /// </summary>
        /// 
        /// <remarks>
        /// Disregards the vertical component of the target's position when calculating the angle to the target, effectively 
        /// treating the field of view as a 2D cone on the XZ plane. Meaning the whole slice of the sphere from top to bottom 
        /// is considered as the field of view, as long as the target is within the specified angle on the XZ plane.
        /// </remarks>
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

            Vector3 centre = transform.position + spatialData.detectionOffset;
            Vector3 directionToTarget = (target.position - centre);

            // INFO: Disregard height difference when calculating the angle to the target, effectively treating the
            //       field of view as a 2D cone on the XZ plane.
            directionToTarget.y = 0.0f;
            directionToTarget.Normalize();

            float rotationY = transform.eulerAngles.y + spatialData.rotationOffset;
            Vector3 fovFacingDirection = Vector3Utilities.DirectionFromAngleXZ(rotationY);

            float angleToTarget = Vector3.Angle(fovFacingDirection, directionToTarget);
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

            Vector3 centre = transform.position + spatialData.detectionOffset;
            Bounds bounds = new(centre, spatialData.boundsSize);
            if (bounds.Contains(target.position))
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
                    DetectionVisualiserUtility.DrawRange(centre, spatialData.radius, spatialData.isTargetDetected);
                    break;
                case DetectionType.FieldOfView:
                    float rotationY = transform.eulerAngles.y + spatialData.rotationOffset;
                    DetectionVisualiserUtility.DrawFieldOfView(centre, spatialData.angle, spatialData.radius, spatialData.isTargetDetected, rotationY);
                    break;
                case DetectionType.Bounds:
                    DetectionVisualiserUtility.DrawBounds(centre, spatialData.boundsSize, spatialData.isTargetDetected);
                    break;
                default:
                    break;
            }
        }
    }
}
