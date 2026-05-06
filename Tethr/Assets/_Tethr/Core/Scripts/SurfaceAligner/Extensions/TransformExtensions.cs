// Copyright (c) 2026, TheMGLegends. All rights reserved.

using UnityEngine;

namespace Tethr.SurfaceAligner
{
    /// <summary>
    /// Custom transform extension methods for Surface Aligner functionality.
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// Attempts to align the transform to the detected surface using the provided values.
        /// </summary>
        /// 
        /// <remarks>
        /// This is the 3D version of the method. For 2D physics alignment, use 
        /// <see cref="AlignToSurface2D(Transform, Vector2, float, int, Vector2)"/> instead.
        /// </remarks>
        /// 
        /// <returns> 
        /// <see langword="true"/> if a viable surface was found and the transform was aligned successfully, <see langword="false"/> otherwise.
        /// </returns>
        public static bool AlignToSurface(this Transform transform, Vector3 direction, float maxDistance = Mathf.Infinity,
                                          int layerMask = Physics.AllLayers, Vector3 offset = default)
        {
            RaycastHit hit = transform.GetSurfaceHit(direction, maxDistance, layerMask);
            if (hit.collider != null)
            {
                transform.up = hit.normal;

                // INFO: Assign offset in local space to avoid issues with rotation
                transform.position = hit.point
                                   + transform.right * offset.x
                                   + transform.up * offset.y
                                   + transform.forward * offset.z;

                return true;
            }

#if UNITY_EDITOR
            Debug.LogWarning($"No viable surface found for '{transform.gameObject.name}' to align to. " +
                             $"Try adjusting your direction, maxDistance or layerMask values or moving the object elsewhere.");
#endif

            return false;
        }

        /// <summary>
        /// Attempts to get a valid surface hit via raycasting using the provided values. The method ignores any hits that are on the object 
        /// itself or its children to prevent self-alignment issues.
        /// </summary>
        /// 
        /// <remarks>
        /// This is the 3D version of the method. For 2D physics surface hits, use
        /// <see cref="GetSurfaceHit2D(Transform, Vector2, float, int)"/> instead.
        /// </remarks>
        /// 
        /// <returns>
        /// A valid <see cref="RaycastHit"/> if a viable surface was detected, or a default <see cref="RaycastHit"/> with no collider if not.
        /// </returns>
        public static RaycastHit GetSurfaceHit(this Transform transform, Vector3 direction, float maxDistance = Mathf.Infinity,
                                               int layerMask = Physics.AllLayers)
        {
            RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, maxDistance, layerMask);
            foreach (RaycastHit hit in hits)
            {
                // INFO: Find the first hit that isn't the object itself or a child of the object
                if (!hit.transform.IsChildOf(transform))
                {
                    return hit;
                }
            }

            return new RaycastHit();
        }

        /// <summary>
        /// Attempts to align the transform to the detected surface using the provided values.
        /// </summary>
        /// 
        /// <remarks>
        /// This is the 2D version of the method. For 3D physics alignment, use
        /// <see cref="AlignToSurface(Transform, Vector3, float, int, Vector3)"/> instead.
        /// </remarks>
        /// 
        /// <returns>
        /// <see langword="true"/> if a viable surface was found and the transform was aligned successfully, <see langword="false"/> otherwise.
        /// </returns>
        public static bool AlignToSurface2D(this Transform transform, Vector2 direction, float maxDistance = Mathf.Infinity,
                                            int layerMask = Physics2D.AllLayers, Vector2 offset = default)
        {
            RaycastHit2D hit = transform.GetSurfaceHit2D(direction, maxDistance, layerMask);
            if (hit.collider != null)
            {
                transform.up = hit.normal;

                Vector3 position = hit.point;
                position.z = transform.position.z; // INFO: Keep original Z position for 2D alignment

                // INFO: Assign offset in local space to avoid issues with rotation
                transform.position = position
                                   + transform.right * offset.x
                                   + transform.up * offset.y;

                return true;
            }

#if UNITY_EDITOR
            Debug.LogWarning($"No viable surface found for '{transform.gameObject.name}' to align to. " +
                             $"Try adjusting your direction, maxDistance or layerMask values or moving the object elsewhere.");
#endif

            return false;
        }

        /// <summary>
        /// Attempts to get a valid surface hit via raycasting using the provided values. The method ignores any hits that are on the object 
        /// itself or its children to prevent self-alignment issues.
        /// </summary>
        /// 
        /// <remarks>
        /// This is the 2D version of the method. For 3D physics surface hits, use
        /// <see cref="GetSurfaceHit(Transform, Vector3, float, int)"/> instead.
        /// </remarks>
        /// 
        /// <returns>
        /// A valid <see cref="RaycastHit"/> if a viable surface was detected, or a default <see cref="RaycastHit"/> with no collider if not.
        /// </returns>
        public static RaycastHit2D GetSurfaceHit2D(this Transform transform, Vector2 direction, float maxDistance = Mathf.Infinity,
                                                   int layerMask = Physics2D.AllLayers)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, maxDistance, layerMask);
            foreach (RaycastHit2D hit in hits)
            {
                // INFO: Find the first hit that isn't the object itself or a child of the object
                if (!hit.transform.IsChildOf(transform))
                {
                    return hit;
                }
            }

            return new RaycastHit2D();
        }

        /// <summary>
        /// Helper method that attempts to align the transform to the ground (downwards) using the provided values.
        /// </summary>
        /// 
        /// <remarks>
        /// This is the 3D version of the method. For 2D physics ground alignment, use
        /// <see cref="AlignToGround2D(Transform, float, int, Vector2)"/> instead.
        /// </remarks>
        /// 
        /// <returns>
        /// <see langword="true"/> if a viable surface was found and the transform was aligned successfully, <see langword="false"/> otherwise.
        /// </returns>
        public static bool AlignToGround(this Transform transform, float maxDistance = Mathf.Infinity,
                                         int layerMask = Physics.AllLayers, Vector3 offset = default)
        {
            return AlignToSurface(transform, Vector3.down, maxDistance, layerMask, offset);
        }

        /// <summary>
        /// Helper method that attempts to align the transform to the ground (downwards) using the provided values.
        /// </summary>
        /// 
        /// <remarks>
        /// This is the 2D version of the method. For 3D physics ground alignment, use
        /// <see cref="AlignToGround(Transform, float, int, Vector3)"/> instead.
        /// </remarks>
        /// 
        /// <returns>
        /// <see langword="true"/> if a viable surface was found and the transform was aligned successfully, <see langword="false"/> otherwise.
        /// </returns>
        public static bool AlignToGround2D(this Transform transform, float maxDistance = Mathf.Infinity,
                                           int layerMask = Physics2D.AllLayers, Vector2 offset = default)
        {
            return AlignToSurface2D(transform, Vector2.down, maxDistance, layerMask, offset);
        }
    }
}
