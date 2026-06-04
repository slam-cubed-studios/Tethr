// Copyright (c) 2026, TheMGLegends. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tethr
{
    /// <summary>
    /// Specifies the direction of the boundary used in obstacle detection.
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
    /// the layer mask to check against, the size of the boundary area, and the offset from the object's position in the 
    /// specified direction.
    /// </summary>
    [Serializable]
    public struct Boundary
    {
        [Tooltip("Specifies the direction in which the boundary check will be performed.")]
        public BoundaryDirection BoundaryDirection;

        [Tooltip("Specifies which layers are checked when performing boundary detection.")]
        public LayerMask BoundaryMask;

        [Tooltip("Determines the dimensions of the area to check.")]
        [Min(0.0f)] public Vector3 Size;

        [Tooltip("Allows you to specify the offset from the object's position for boundary detection calculations.")]
        public Vector3 Offset;

        /// <summary>
        /// Gets the facing direction of the boundary in world space.
        /// </summary>
        /// 
        /// <param name="transform">
        /// The transform of the object for which the boundary is being calculated.
        /// </param>
        /// 
        /// <returns>
        /// The facing direction of the boundary as a unit vector in world space, taking into account the current boundary direction
        /// and rotation of the object.
        /// 
        /// </returns>
        public readonly Vector3 GetFacingDirection(Transform transform)
        {
            return BoundaryDirection switch
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
        /// Gets the facing direction of the boundary in world space, ignoring the rotation of the object.
        /// </summary>
        /// 
        /// <returns>
        /// The facing direction of the boundary as a unit vector in world space using the current boundary direction.
        /// </returns>
        public readonly Vector3 GetAbsoluteDirection()
        {
            return BoundaryDirection switch
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

    /// <summary>
    /// Represents configuration settings for ledge detection used in obstacle detection, including the layer mask to check against,
    /// the distance to check for ledges, and the offset from the object's position.
    /// </summary>
    [Serializable]
    public struct LedgeDetectionData
    {
        [Tooltip("Specifies which layers are checked when performing ledge detection.")]
        public LayerMask GroundMask;

        [Tooltip("Determines how far down the raycast will check for ground.")]
        [Min(0.0f)] public float Distance;

        [Tooltip("Allows you to specify an offset from the object's position for ledge detection calculations.")]
        [Min(0.0f)] public float Offset;

        public static LedgeDetectionData Default => new LedgeDetectionData
        {
            GroundMask = ~0,
            Distance = 1.0f,
            Offset = 1.0f
        };
    }

    public abstract class BaseObstacleDetectionComponent : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private DebugDrawType debugDrawType = DebugDrawType.WhenSelected;
        [SerializeField] private Color boundaryColour = Color.blue;
        [SerializeField] protected Color ledgeColour = Color.red;

        [Header("General Settings")]
        [Tooltip("Determines what space obstacle detection and gizmo visualisation should be performed in.")]
        [SerializeField] protected bool isAxisAligned = true;

        [Header("Ledge Detection Settings")]
        [SerializeField] protected LedgeDetectionData ledgeDetectionData = LedgeDetectionData.Default;

        [Header("Boundaries Detection Settings")]
        [SerializeField] protected List<Boundary> boundaries;

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

        /// <summary>
        /// Determines whether a ledge is detected in front of the object based on the current ledge detection settings.
        /// </summary>
        /// 
        /// <returns>
        /// <see langword="true"/> if a ledge is detected, <see langword="false"/> otherwise.
        /// </returns>
        public abstract bool IsLedgeDetected();

        /// <summary>
        /// Determines whether a boundary is detected by querying each boundary in the current boundaries list.
        /// </summary>
        /// 
        /// <remarks>
        /// Discards velocity adjustments. Use <see cref="IsBoundaryDetected(ref Vector3)"/> if you want to adjust 
        /// the velocity vector to prevent movement into the boundary.
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
        /// Determines whether a boundary is detected by querying each boundary in the current boundaries list,
        /// adjusting the provided velocity vector to prevent movement into the boundary if a boundary is detected.
        /// </summary>
        /// 
        /// <param name="velocity">
        /// The current velocity vector of the object.
        /// </param>
        /// 
        /// <returns> 
        /// <see langword="true"/> if a boundary is detected, <see langword="false"/> otherwise.
        /// </returns>
        public abstract bool IsBoundaryDetected(ref Vector3 velocity);

        protected virtual void DrawObstacleDetectionGizmos()
        {
            if (boundaries == null)
            {
                return;
            }

            // INFO: Align gizmos with the transform rotation if we aren't using axis aligned detection
            Matrix4x4 previousMatrix = Gizmos.matrix;
            if (!isAxisAligned)
            {
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            }

            // INFO: Boundary Gizmos
            foreach (Boundary boundary in boundaries)
            {
                Vector3 centre = isAxisAligned ? transform.position + boundary.Offset : boundary.Offset;

                Gizmos.color = boundaryColour;
                Gizmos.DrawWireCube(centre, boundary.Size);

                Gizmos.color = new Color(boundaryColour.r, boundaryColour.g, boundaryColour.b, 0.25f);
                Gizmos.DrawCube(centre, boundary.Size);
            }

            Gizmos.matrix = previousMatrix;
        }
    }
}
