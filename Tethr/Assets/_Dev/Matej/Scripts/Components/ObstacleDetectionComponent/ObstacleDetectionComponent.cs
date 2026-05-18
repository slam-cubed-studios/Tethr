// Copyright (c) 2026, TheMGLegends. All rights reserved.

using UnityEditor;
using UnityEngine;

namespace Tethr
{
    /// <summary>
    /// 3D version of the ObstacleDetectionComponentBase. It uses 3D physics methods for detecting ledges and boundaries.
    /// </summary>
    public class ObstacleDetectionComponent : ObstacleDetectionComponentBase
    {
        /// <summary>
        /// Determines if there is a ledge in front of the object.
        /// </summary>
        /// 
        /// <remarks>
        /// Ledge detection is always offset in the forward direction of the object/world, users should therefore ensure that the
        /// objects visuals are oriented in the forward direction.
        /// </remarks>
        public override bool IsLedgeDetected()
        {
            Vector3 offsetPosition = transform.position + ledgeDetectionData.offset * (isAxisAligned ? Vector3.forward : transform.forward);
            if (Physics.Raycast(offsetPosition, isAxisAligned ? Vector3.down : -transform.up, 
                                ledgeDetectionData.distance, ledgeDetectionData.groundMask))
            {
                return false;
            }

            return true;
        }

        public override bool IsBoundaryDetected(ref Vector3 velocity)
        {
            bool isBoundaryDetected = false;
            foreach (Boundary boundary in boundaries)
            {
                Vector3 direction = isAxisAligned ? boundary.GetWorldDirection() : boundary.GetFacingDirection(transform);
                Vector3 offsetPosition = transform.position + boundary.offset * direction;

                if (Physics.CheckBox(offsetPosition, boundary.size * 0.5f, 
                                     isAxisAligned ? Quaternion.identity : transform.rotation, 
                                     ledgeDetectionData.groundMask))
                {
                    velocity = Vector3.ProjectOnPlane(velocity, direction);
                    isBoundaryDetected = true;
                }
            }

            velocity.Normalize();
            return isBoundaryDetected;
        }

        protected override void DrawObstacleDetectionGizmos()
        {
            base.DrawObstacleDetectionGizmos();

            Vector3 offsetPosition = transform.position + ledgeDetectionData.offset * (isAxisAligned ? Vector3.forward : transform.forward);
            Vector3 ledgeDetectionPosition = offsetPosition + ledgeDetectionData.distance * (isAxisAligned ? Vector3.down : -transform.up);
            Vector3 forward = Camera.current != null ? Camera.current.transform.forward : Vector3.forward;

            Handles.color = ledgeColour;
            Handles.DrawSolidDisc(ledgeDetectionPosition, forward, 0.1f);
        }
    }
}
