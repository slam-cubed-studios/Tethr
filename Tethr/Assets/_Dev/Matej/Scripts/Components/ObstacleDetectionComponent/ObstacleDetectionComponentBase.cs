// Copyright (c) 2026, TheMGLegends. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tethr
{
    /// <summary>
    /// Specifies the direction of the boundary to check for obstacles.
    /// </summary>
    public enum BoundaryDirection
    {
        Left,
        Right,
        Up,
        Down,
        Forward,
        Backward
    }

    /// <summary>
    /// Represents configuration settings for a boundary used in obstacle detection, including the direction of the boundary, 
    /// the layer mask to check against, the size of the boundary area, and the offset from the object's position.
    /// </summary>
    [Serializable]
    public struct Boundary
    {
        [Tooltip("Boundary direction specifies the direction in which the boundary check will be performed.")]
        public BoundaryDirection boundaryDirection;

        [Tooltip("Boundary mask specifies which layers are checked when performing boundary detection.")]
        public LayerMask boundaryMask;

        [Tooltip("Boundary size determines the dimensions of the area to check.")]
        [Min(0.0f)] public Vector3 size;

        [Tooltip("Boundary offset allows you to specify an offset from the object's position for boundary detection calculations, " +
                 "offset is applied on different axis based on the boundary direction.")]
        [Min(0.0f)] public float offset;

        /// <summary>
        /// Allows you to get the facing direction of the boundary based on the specified boundary direction and the rotation of the object.
        /// </summary>
        public readonly Vector3 GetFacingDirection(Transform transform)
        {
            return boundaryDirection switch
            {
                BoundaryDirection.Left => -transform.right,
                BoundaryDirection.Right => transform.right,
                BoundaryDirection.Up => transform.up,
                BoundaryDirection.Down => -transform.up,
                BoundaryDirection.Forward => transform.forward,
                BoundaryDirection.Backward => -transform.forward,
                _ => Vector3.zero
            };
        }

        /// <summary>
        /// Gets the world space direction of the boundary based on the specified boundary direction, ignoring the rotation of the object.
        /// </summary>
        public readonly Vector3 GetWorldDirection()
        {
            return boundaryDirection switch
            {
                BoundaryDirection.Left => Vector3.left,
                BoundaryDirection.Right => Vector3.right,
                BoundaryDirection.Up => Vector3.up,
                BoundaryDirection.Down => Vector3.down,
                BoundaryDirection.Forward => Vector3.forward,
                BoundaryDirection.Backward => Vector3.back,
                _ => Vector3.zero
            };
        }
    }

    [Serializable]
    public struct LedgeDetectionData
    {
        [Tooltip("Ground mask specifies which layers are checked when performing ledge detection.")]
        public LayerMask groundMask;

        [Tooltip("Distance determines how far down the raycast for ledge detection will check for ground.")]
        [Min(0.0f)] public float distance;

        [Tooltip("Offset allows you to specify an offset from the object's position for ledge detection calculations.")]
        [Min(0.0f)] public float offset;

        public LedgeDetectionData(bool useDefaults)
        {
            if (useDefaults)
            {
                groundMask = ~0;
                distance = 1.0f;
                offset = 1.0f;
            }
            else
            {
                groundMask = default;
                distance = default;
                offset = default;
            }
        }
    }

    /// <summary>
    /// Provides an abstract base class for obstacle detection components that can be used to detect ledges and boundaries.
    /// </summary>
    public abstract class ObstacleDetectionComponentBase : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private DebugDrawType debugDrawType = DebugDrawType.WhenSelected;
        [SerializeField] private Color boundaryColour = Color.blue;
        [SerializeField] protected Color ledgeColour = Color.red;

        [Header("General Settings")]
        [Tooltip("Is Axis Aligned determines whether the obstacle detection and gizmo visualisation should be performed in " +
                 "world space (axis aligned) or in local space (not axis aligned).")]
        [SerializeField] protected bool isAxisAligned = true;

        [Header("Ledge Detection Settings")]
        [SerializeField] protected LedgeDetectionData ledgeDetectionData = new(true);

        [Header("Boundaries Detection Settings")]
        [SerializeField] protected List<Boundary> boundaries = new();

        private void OnDrawGizmos()
        {
            if (debugDrawType != DebugDrawType.Always)
            {
                return;
            }

            DrawObstacleDetectionGizmos();
        }

        private void OnDrawGizmosSelected()
        {
            if (debugDrawType != DebugDrawType.WhenSelected)
            {
                return;
            }

            DrawObstacleDetectionGizmos();
        }

        public abstract bool IsLedgeDetected();

        /// <summary>
        /// Determines whether a boundary is detected.
        /// </summary>
        /// 
        /// <remarks>
        /// This method overload discards the velocity adjustment that may occur when a boundary is detected. 
        /// </remarks>
        /// 
        /// <returns> 
        /// <see langword="true"/> if a boundary is detected, <see langword="false"/> otherwise.
        /// </returns>
        public bool IsBoundaryDetected()
        {
            Vector3 _ = Vector3.zero;
            return IsBoundaryDetected(ref _);
        }

        /// <summary>
        /// Determines whether a boundary is detected and adjusts the provided velocity vector to prevent the 
        /// object from moving into the boundary.
        /// </summary>
        /// 
        /// <param name="velocity">
        /// The velocity vector of the object checking for boundaries.
        /// </param>
        public abstract bool IsBoundaryDetected(ref Vector3 velocity);

        protected virtual void DrawObstacleDetectionGizmos()
        {
            // INFO: Align gizmos with the transform rotation if we aren't using axis aligned detection
            if (!isAxisAligned)
            {
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            }

            foreach (Boundary boundary in boundaries)
            {
                Vector3 direction = boundary.GetWorldDirection();
                Vector3 centre = isAxisAligned ? transform.position + direction * boundary.offset : boundary.offset * direction;

                Gizmos.color = boundaryColour;
                Gizmos.DrawWireCube(centre, boundary.size);

                Gizmos.color = new Color(boundaryColour.r, boundaryColour.g, boundaryColour.b, 0.25f);
                Gizmos.DrawCube(centre, boundary.size);
            }

            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
