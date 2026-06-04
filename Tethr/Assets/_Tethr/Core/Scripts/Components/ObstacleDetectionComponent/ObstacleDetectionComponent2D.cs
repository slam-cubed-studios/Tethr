// Copyright (c) 2026, TheMGLegends. All rights reserved.

using UnityEditor;
using UnityEngine;

namespace Tethr
{
    /// <summary>
    /// 2D version of the <see cref="BaseObstacleDetectionComponent"/>. It uses 2D physics methods for detecting ledges and boundaries.
    /// </summary>
    public class ObstacleDetectionComponent2D : BaseObstacleDetectionComponent
    {
        /// <summary>
        /// Determines whether a ledge is detected in front of the object based on the current ledge detection settings.
        /// </summary>
        /// 
        /// <remarks>
        /// For the 2D version of the component, ledge detection is always offset in the right direction of the object/world.
        /// Users should therefore ensure that object visuals are oriented in the right direction and rotated as opposed to
        /// visually flipped or scaled negatively, as those methods would not affect the direction of the ledge detection.
        /// </remarks>
        /// 
        /// <returns>
        /// <see langword="true"/> if a ledge is detected, <see langword="false"/> otherwise.
        /// </returns>
        public override bool IsLedgeDetected()
        {
            float scaleDirection = transform.lossyScale.x < 0.0f ? -1.0f : 1.0f;
            Vector3 offsetPosition = transform.position + ledgeDetectionData.Offset * ((isAxisAligned ? Vector3.right : transform.right) * scaleDirection);
            Vector3 direction = isAxisAligned ? Vector3.down : -transform.up;
            RaycastHit2D hit = Physics2D.Raycast(offsetPosition, direction, ledgeDetectionData.Distance, ledgeDetectionData.GroundMask);

            return hit.collider == null;
        }

        public override bool IsBoundaryDetected(ref Vector3 velocity)
        {
            bool isBoundaryDetected = false;
            foreach (Boundary boundary in boundaries)
            {
                if (isBoundaryDetected && Mathf.Approximately(velocity.sqrMagnitude, 0.0f))
                {
                    break;
                }

                Vector3 direction = isAxisAligned ? boundary.GetAbsoluteDirection() : boundary.GetFacingDirection(transform);
                Quaternion rotation = isAxisAligned ? Quaternion.identity : transform.rotation;
                Vector3 offsetPosition = transform.position + (rotation * boundary.Offset);
                if (Physics2D.OverlapBox(offsetPosition, boundary.Size, rotation.eulerAngles.z, boundary.BoundaryMask))
                {
                    velocity = Vector3.ProjectOnPlane(velocity, direction);
                    isBoundaryDetected = true;
                }
            }

            return isBoundaryDetected;
        }

        protected override void DrawObstacleDetectionGizmos()
        {
#if UNITY_EDITOR
            base.DrawObstacleDetectionGizmos();

            // INFO: Ledge Detection Gizmo
            float scaleDirection = Mathf.Sign(transform.lossyScale.x);
            Vector3 offsetPosition = transform.position + ledgeDetectionData.Offset * ((isAxisAligned ? Vector3.right : transform.right) * scaleDirection);
            Vector3 ledgeDetectionPosition = offsetPosition + ledgeDetectionData.Distance * (isAxisAligned ? Vector3.down : -transform.up);

            Handles.color = ledgeColour;
            Handles.DrawSolidDisc(ledgeDetectionPosition, Vector3.forward, 0.075f);
#endif
        }
    }
}
