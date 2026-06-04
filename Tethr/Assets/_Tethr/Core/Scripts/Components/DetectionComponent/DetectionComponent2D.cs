// Copyright (c) 2026, TheMGLegends. All rights reserved.

using System;
using Tethr.DetectionVisualiser;
using UnityEngine;

namespace Tethr
{
    /// <summary>
    /// 2D version of the <see cref="BaseDetectionComponent"/>. It uses 2D physics methods for detecting targets and checking line of sight,
    /// as well as 2D-specific implementations for checking if a target is within range, field of view, or bounds.
    /// </summary>
    public class DetectionComponent2D : BaseDetectionComponent
    {
        private const int MAX_HITS = 5;

        private readonly Collider2D[] colliders = new Collider2D[MAX_TARGETS];
        private readonly RaycastHit2D[] hits = new RaycastHit2D[MAX_HITS];

        public override Transform[] ScanForTargets(SpatialData spatialData)
        {
            if (spatialData == null)
            {
                return Array.Empty<Transform>();
            }

            ContactFilter2D contactFilter = new()
            {
                layerMask = spatialData.DetectionMask,
                useLayerMask = true
            };
            Vector2 point = transform.position + transform.TransformDirection(spatialData.DetectionOffset);
            int targetCount = spatialData.DetectionType switch
            {
                DetectionType.Range => Physics2D.OverlapCircle(point, spatialData.Radius, contactFilter, colliders),
                DetectionType.FieldOfView => Physics2D.OverlapCircle(point, spatialData.Radius, contactFilter, colliders),
                DetectionType.Bounds => Physics2D.OverlapBox(point, spatialData.BoundsSize, 0.0f, contactFilter, colliders),
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

            ContactFilter2D contactFilter = new()
            {
                layerMask = spatialData.DetectionMask,
                useLayerMask = true
            };
            Vector2 point = transform.position + transform.TransformDirection(spatialData.DetectionOffset);
            int targetCount = spatialData.DetectionType switch
            {
                DetectionType.Range => Physics2D.OverlapCircle(point, spatialData.Radius, contactFilter, colliders),
                DetectionType.FieldOfView => Physics2D.OverlapCircle(point, spatialData.Radius, contactFilter, colliders),
                DetectionType.Bounds => Physics2D.OverlapBox(point, spatialData.BoundsSize, 0.0f, contactFilter, colliders),
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

            ContactFilter2D contactFilter = new()
            {
                layerMask = spatialData.ObstacleMask,
                useLayerMask = true
            };
            Vector3 start = transform.position + transform.TransformDirection(spatialData.DetectionOffset);
            int hitCount = Physics2D.Linecast(start, target.position, contactFilter, hits);

            if (hitCount == MAX_HITS)
            {
                Message.LogWarning("Maximum hit limit reached. Some hits may not be detected. " +
                                   "Consider increasing the MAX_HITS constant if this is a common occurrence.");
            }

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit = hits[i];
                if (hit.transform != target.transform)
                {
                    if (hit.transform.IsChildOf(transform))
                    {
                        Message.LogWarning("Linecast hit the object from which it was called from. This may indicate an issue with the " +
                                           "obstacle mask or the object's layer.");
                    }

                    return false;
                }
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
            if (Vector2.Distance(position, target.position) <= spatialData.Radius)
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

            Vector3 position = transform.position + transform.TransformDirection(spatialData.DetectionOffset);
            Vector2 directionToTarget = (target.position - position).normalized;

            float rotationZ = transform.eulerAngles.z - spatialData.RotationOffset;
            Vector2 fovFacingDirection = Vector3Utilities.DirectionFromAngleXY(rotationZ);

            float angleToTarget = Vector2.Angle(fovFacingDirection, directionToTarget);
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

            // INFO: Disregard z-position for 2D bounds detection
            Vector3 centre = transform.position + transform.TransformDirection(spatialData.DetectionOffset);
            centre.z = 0.0f;

            Vector3 targetPosition = target.position;
            targetPosition.z = 0.0f;

            // INFO: Disregard z-size for 2D bounds detection
            Vector3 boundsSize = spatialData.BoundsSize;
            boundsSize.z = 0.0f;

            Bounds bounds = new(centre, boundsSize);
            if (bounds.Contains(targetPosition))
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
                    DetectionVisualiserUtility.DrawRange2D(centre, spatialData.Radius, isTargetDetected);
                    break;
                case DetectionType.FieldOfView:
                    float rotationZ = transform.eulerAngles.z - spatialData.RotationOffset;
                    DetectionVisualiserUtility.DrawFieldOfView2D(centre, spatialData.Angle, spatialData.Radius, isTargetDetected, rotationZ);
                    break;
                case DetectionType.Bounds:
                    // INFO: Disregard z-size for 2D bounds visualisation
                    Vector3 boundsSize = spatialData.BoundsSize;
                    boundsSize.z = 0.0f;

                    DetectionVisualiserUtility.DrawBounds(centre, boundsSize, isTargetDetected);
                    break;
                default:
                    break;
            }
#endif
        }
    }
}
