// Copyright (c) 2026, TheMGLegends. All rights reserved.

using UnityEditor;
using UnityEngine;

namespace Tethr
{
    /// <summary>
    /// 2D version of the ObstacleDetectionComponentBase. It uses 2D physics methods for detecting ledges and boundaries.
    /// </summary>
    public class ObstacleDetectionComponent2D : ObstacleDetectionComponentBase
    {
        /// <summary>
        /// Determines if there is a ledge in front of the object.
        /// </summary>
        ///
        /// <remarks>
        /// Ledge detection is always offset in the right direction of the object/world, users should therefore ensure that the 
        /// objects visuals are oriented in the right direction (or a negative offset can be used). Users should also rotate the 
        /// object as opposed to using other means like flipping the scale or using the sprite renderer's flipX property,
        /// as those methods would not affect the direction of the ledge detection.
        /// </remarks>
        public override bool IsLedgeDetected()
        {
            Vector3 offsetPosition = transform.position + ledgeDetectionData.offset * (isAxisAligned ? Vector3.right : transform.right);
            RaycastHit2D hit = Physics2D.Raycast(offsetPosition, isAxisAligned ? Vector3.down : -transform.up, 
                                                 ledgeDetectionData.distance, ledgeDetectionData.groundMask);

            return hit.collider == null;
        }

        public override bool IsBoundaryDetected(ref Vector3 velocity)
        {
            bool isBoundaryDetected = false;
            foreach (Boundary boundary in boundaries)
            {
                Vector3 direction = isAxisAligned ? boundary.GetWorldDirection() : boundary.GetFacingDirection(transform);
                RaycastHit2D hit = Physics2D.BoxCast(transform.position, boundary.size, isAxisAligned ? 0.0f : transform.eulerAngles.z, 
                                                     direction, boundary.offset, boundary.boundaryMask);
                if (hit.collider != null)
                {
                    velocity = Vector3.ProjectOnPlane(velocity, hit.normal);
                    isBoundaryDetected = true;
                }
            }

            velocity.Normalize();
            return isBoundaryDetected;
        }

        protected override void DrawObstacleDetectionGizmos()
        {
            base.DrawObstacleDetectionGizmos();

            Vector3 offsetPosition = transform.position + ledgeDetectionData.offset * (isAxisAligned ? Vector3.right : transform.right);
            Vector3 ledgeDetectionPosition = offsetPosition + ledgeDetectionData.distance * (isAxisAligned ? Vector3.down : -transform.up);

            Handles.color = ledgeColour;
            Handles.DrawSolidDisc(ledgeDetectionPosition, Vector3.forward, 0.1f);
        }
    }
}
