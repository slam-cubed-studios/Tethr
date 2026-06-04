// Copyright (c) 2026, TheMGLegends. All rights reserved.

using UnityEditor;
using UnityEngine;

namespace Tethr
{
    /// <summary>
    /// 3D version of the <see cref="BaseObstacleDetectionComponent"/>. It uses 3D physics methods for detecting ledges and boundaries.
    /// </summary>
    public class ObstacleDetectionComponent : BaseObstacleDetectionComponent
    {
        /// <summary>
        /// Determines whether a ledge is detected in front of the object based on the current ledge detection settings.
        /// </summary>
        /// 
        /// <remarks>
        /// For the 3D version of the component, ledge detection is always offset in the forward direction of the object/world.
        /// </remarks>
        /// 
        /// <returns>
        /// <see langword="true"/> if a ledge is detected, <see langword="false"/> otherwise.
        /// </returns>
        public override bool IsLedgeDetected()
        {
            Vector3 offsetPosition = transform.position + ledgeDetectionData.Offset * (isAxisAligned ? Vector3.forward : transform.forward);
            Vector3 direction = isAxisAligned ? Vector3.down : -transform.up;

            return !Physics.Raycast(offsetPosition, direction, ledgeDetectionData.Distance, ledgeDetectionData.GroundMask);
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
                if (Physics.CheckBox(offsetPosition, boundary.Size * 0.5f, rotation, boundary.BoundaryMask))
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
            Vector3 offsetPosition = transform.position + ledgeDetectionData.Offset * (isAxisAligned ? Vector3.forward : transform.forward);
            Vector3 ledgeDetectionPosition = offsetPosition + ledgeDetectionData.Distance * (isAxisAligned ? Vector3.down : -transform.up);
            Vector3 forward = Camera.current != null ? Camera.current.transform.forward : Vector3.forward;

            Handles.color = ledgeColour;
            Handles.DrawSolidDisc(ledgeDetectionPosition, forward, 0.075f);
#endif
        }
    }
}
