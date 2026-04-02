using UnityEngine;

public static class TransformExtensions
{
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

    public static bool AlignToGround(this Transform transform, float maxDistance = Mathf.Infinity,
                                     int layerMask = Physics.AllLayers, Vector3 offset = default)
    {
        return AlignToSurface(transform, Vector3.down, maxDistance, layerMask, offset);
    }

    public static bool AlignToGround2D(this Transform transform, float maxDistance = Mathf.Infinity,
                                       int layerMask = Physics2D.AllLayers, Vector2 offset = default)
    {
        return AlignToSurface2D(transform, Vector2.down, maxDistance, layerMask, offset);
    }
}
