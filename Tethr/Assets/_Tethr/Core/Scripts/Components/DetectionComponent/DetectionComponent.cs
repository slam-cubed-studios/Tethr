// Copyright (c) 2026, TheMGLegends. All rights reserved.

using System;
using Tethr.DetectionVisualiser;
using UnityEngine;

namespace Tethr
{
    /// <summary>
    /// 3D version of the <see cref="BaseDetectionComponent"/>. It uses 3D physics methods for detecting targets and checking line of sight,
    /// as well as 3D-specific implementations for checking if a target is within range, field of view, or bounds.
    /// </summary>
    public class DetectionComponent : BaseDetectionComponent
    {
        private readonly Collider[] colliders = new Collider[MAX_TARGETS];

        public override Transform[] ScanForTargets(SpatialData spatialData)
        {
            if (spatialData == null)
            {
                return Array.Empty<Transform>();
            }

            Vector3 position = transform.position + transform.TransformDirection(spatialData.DetectionOffset);
            int targetCount = spatialData.DetectionType switch
            {
                DetectionType.Range => Physics.OverlapSphereNonAlloc(position, spatialData.Radius, colliders, spatialData.DetectionMask),
                DetectionType.FieldOfView => Physics.OverlapSphereNonAlloc(position, spatialData.Radius, colliders, spatialData.DetectionMask),
                DetectionType.Bounds => Physics.OverlapBoxNonAlloc(position, spatialData.BoundsSize / 2.0f, colliders, Quaternion.identity, spatialData.DetectionMask),
                _ => 0
            };

            if (targetCount == MAX_TARGETS)
            {
                Message.LogWarning("Maximum target limit reached. Some targets may not be detected. " +
                                   "Consider increasing the MAX_TARGETS constant if this is a common occurrence.");
            }

            // INFO: Find any valid targets
            int validCount = 0;
            Transform[] validTargets = new Transform[targetCount];
            for (int i = 0; i < targetCount; i++)
            {
                Transform target = colliders[i].transform;
                if (IsTargetValid(target, spatialData))
                {
                    validTargets[validCount++] = target;
                }
            }

            Array.Resize(ref validTargets, validCount);
            return validTargets;
        }

        public override Transform ScanForTarget(SpatialData spatialData)
        {
            if (spatialData == null)
            {
                return null;
            }

            Vector3 position = transform.position + transform.TransformDirection(spatialData.DetectionOffset);
            int targetCount = spatialData.DetectionType switch
             {
                 DetectionType.Range => Physics.OverlapSphereNonAlloc(position, spatialData.Radius, colliders, spatialData.DetectionMask),
                 DetectionType.FieldOfView => Physics.OverlapSphereNonAlloc(position, spatialData.Radius, colliders, spatialData.DetectionMask),
                 DetectionType.Bounds => Physics.OverlapBoxNonAlloc(position, spatialData.BoundsSize / 2.0f, colliders, Quaternion.identity, spatialData.DetectionMask),
                 _ => 0
             };

            if (targetCount == MAX_TARGETS)
            {
                Message.LogWarning("Maximum target limit reached. Some targets may not be detected. " +
                                   "Consider increasing the MAX_TARGETS constant if this is a common occurrence.");
            }

            // INFO: Find the first valid target
            for (int i = 0; i < targetCount; i++)
            {
                Transform target = colliders[i].transform;
                if (IsTargetValid(target, spatialData))
                {
                    return target;
                }
            }

            return null;
        }

        public override bool HasLineOfSight(Transform target, SpatialData spatialData)
        {
            if (target == null || spatialData == null)
            {
                return false;
            }

            Vector3 start = transform.position + transform.TransformDirection(spatialData.DetectionOffset);
            if (Physics.Linecast(start, target.position, out RaycastHit hit, spatialData.ObstacleMask) && hit.transform != target.transform)
            {
                if (hit.transform.IsChildOf(transform))
                {
                    Message.LogWarning("Linecast hit the object from which it was called from. This may indicate an issue with the " +
                                       "obstacle mask or the object's layer.");
                }

                return false;
            }

            return true;
        }

        public override bool CheckTargetInRange(Transform target, SpatialData spatialData)
        {
            if (target == null || spatialData == null)
            {
                return false;
            }

            Vector3 position = transform.position + transform.TransformDirection(spatialData.DetectionOffset);
            if (Vector3.Distance(position, target.position) <= spatialData.Radius)
            {
                return true;
            }

            return false;
        }

        public override bool CheckTargetInFieldOfView(Transform target, SpatialData spatialData)
        {
            if (!CheckTargetInRange(target, spatialData))
            {
                return false;
            }

            Vector3 centre = transform.position + transform.TransformDirection(spatialData.DetectionOffset);
            Vector3 directionToTarget = (target.position - centre);

            // INFO: Disregard height difference when calculating the angle to the target, effectively treating the
            //       field of view as a 2D cone on the XZ plane.
            directionToTarget.y = 0.0f;
            directionToTarget.Normalize();

            float rotationY = transform.eulerAngles.y + spatialData.RotationOffset;
            Vector3 fovFacingDirection = Vector3Utilities.DirectionFromAngleXZ(rotationY);

            float angleToTarget = Vector3.Angle(fovFacingDirection, directionToTarget);
            if (angleToTarget <= spatialData.Angle / 2)
            {
                return true;
            }

            return false;
        }

        public override bool CheckTargetInBounds(Transform target, SpatialData spatialData)
        {
            if (target == null || spatialData == null)
            {
                return false;
            }

            Vector3 centre = transform.position + transform.TransformDirection(spatialData.DetectionOffset);
            Bounds bounds = new(centre, spatialData.BoundsSize);
            if (bounds.Contains(target.position))
            {
                return true;
            }

            return false;
        }

        internal override void DrawDetectionGizmos(SpatialData spatialData)
        {
#if UNITY_EDITOR
            bool isTargetDetected = false;
            foreach (Transform target in ScanForTargets(spatialData))
            {
                if (IsTargetDetected(target, spatialData))
                {
                    isTargetDetected = true;
                    break;
                }
            }

            Vector3 centre = transform.position + transform.TransformDirection(spatialData.DetectionOffset);
            switch (spatialData.DetectionType)
            {
                case DetectionType.Range:
                    DetectionVisualiserUtility.DrawRange(centre, spatialData.Radius, isTargetDetected);
                    break;
                case DetectionType.FieldOfView:
                    float rotationY = transform.eulerAngles.y + spatialData.RotationOffset;
                    DetectionVisualiserUtility.DrawFieldOfView(centre, spatialData.Angle, spatialData.Radius, isTargetDetected, rotationY);
                    break;
                case DetectionType.Bounds:
                    DetectionVisualiserUtility.DrawBounds(centre, spatialData.BoundsSize, isTargetDetected);
                    break;
                default:
                    break;
            }
#endif
        }
    }
}
