// Copyright (c) 2026, TheMGLegends. All rights reserved.

using UnityEngine;

namespace Tethr
{
    public static class Vector3Utilities
    {
        /// <summary>
        /// A way to get a direction vector on the XY plane (Vertical Front Plane - 2D) based on an angle in degrees.
        /// </summary>
        /// 
        /// <remarks>
        /// The x-value is inverted when rotating on the z-axis because the 2D standard is to have the camera face the 
        /// forward direction of the world (positive z-axis), therefore the positive rotation direction now becomes 
        /// clockwise instead of counter-clockwise.
        /// </remarks>
        public static Vector3 DirectionFromAngleXY(float angleInDegrees)
        {
            float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
            return new Vector3(-Mathf.Sin(angleInRadians), Mathf.Cos(angleInRadians), 0.0f);
        }

        /// <summary>
        /// A way to get a direction vector on the XZ plane (Horizontal Ground Plane - 3D) based on an angle in degrees.
        /// </summary>
        public static Vector3 DirectionFromAngleXZ(float angleInDegrees)
        {
            float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(angleInRadians), 0.0f, Mathf.Cos(angleInRadians));
        }

        /// <summary>
        /// A way to get a direction vector on the YZ plane (Vertical Side Plane - 3D) based on an angle in degrees.
        /// </summary>
        public static Vector3 DirectionFromAngleYZ(float angleInDegrees)
        {
            float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
            return new Vector3(0.0f, Mathf.Sin(angleInRadians), Mathf.Cos(angleInRadians));
        }
    }
}