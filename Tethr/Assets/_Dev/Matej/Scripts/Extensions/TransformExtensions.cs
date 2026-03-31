using UnityEngine;

public static class TransformExtensions
{
    public static bool AlignToSurface(this Transform transform, Vector3 direction, float maxDistance = Mathf.Infinity, int layerMask = Physics.AllLayers)
    {
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, maxDistance, layerMask))
        {
            transform.position = hit.point;
            transform.up = hit.normal;
            return true;
        }
#if UNITY_EDITOR
        else
        {
            Debug.LogWarning($"No surface found using the provided layerMask, in the direction {direction}, within a distance of {maxDistance}, for " +
                             $"{transform.gameObject.name} to align to. Try moving it elsewhere.");
            return false;
        }
#endif
    }

    public static bool AlignToSurface2D(this Transform transform, Vector2 direction, float maxDistance = Mathf.Infinity, int layerMask = Physics2D.AllLayers)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, maxDistance, layerMask);
        if (hit)
        {
            transform.position = new Vector3(hit.point.x, hit.point.y, transform.position.z);
            transform.up = hit.normal;
            return true;
        }
#if UNITY_EDITOR
        else
        {
            Debug.LogWarning($"No surface found using the provided layerMask, in the direction {direction}, within a distance of {maxDistance}, for " +
                             $"{transform.gameObject.name} to align to. Try moving it elsewhere.");
            return false;
        }
#endif
    }

    public static bool AlignToGround(this Transform transform, float maxDistance = Mathf.Infinity, int layerMask = Physics.AllLayers)
    {
        return AlignToSurface(transform, Vector3.down, maxDistance, layerMask);
    }

    public static bool AlignToGround2D(this Transform transform, float maxDistance = Mathf.Infinity, int layerMask = Physics2D.AllLayers)
    {
        return AlignToSurface2D(transform, Vector2.down, maxDistance, layerMask);
    }
}
