using Tethr.DetectionVisualiser;
using UnityEngine;

public class MatejTestScript : MonoBehaviour
{
    public enum DetectionType
    {
        Range,
        FieldOfView,
        Bounds
    }

    public DetectionType detectionType = DetectionType.Range;
    public bool is3D = false;
    public bool isTargetVisible = false;

    [Space(10.0f)]

    [Range(0.0f, 360.0f)] public float angle = 45.0f;
    [Min(0.0f)] public float radius = 5.0f;

    [Space(10.0f)]

    [Range(0.0f, 360.0f)] public float rotationOffset = 0.0f;
    public Vector3 detectionOffset = Vector3.zero;

    [Space(10.0f)]

    public Vector3 size = Vector3.one;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        switch (detectionType)
        {
            case DetectionType.Range:
                if (!is3D)
                {
                    DetectionVisualiserUtility.DrawRange2D(transform.position + detectionOffset, radius, isTargetVisible);
                }
                else
                {
                    DetectionVisualiserUtility.DrawRange(transform.position + detectionOffset, radius, isTargetVisible);
                }
                break;
            case DetectionType.FieldOfView:
                if (!is3D)
                {
                    DetectionVisualiserUtility.DrawFieldOfView2D(transform.position + detectionOffset, angle, radius, isTargetVisible, transform.eulerAngles.z - rotationOffset);
                }
                else
                {
                    DetectionVisualiserUtility.DrawFieldOfView(transform.position + detectionOffset, angle, radius, isTargetVisible, transform.eulerAngles.y - rotationOffset);
                }
                break;
            case DetectionType.Bounds:
                DetectionVisualiserUtility.DrawBounds(transform.position + detectionOffset, size, isTargetVisible);
                break;
        }
    }
}
