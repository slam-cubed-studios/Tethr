using Tethr;
using UnityEditor;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField] private bool is2D = true;

    private DetectionComponent2D detectionComponent2D;
    private DetectionComponent detectionComponent3D;

    private void Awake()
    {
        if (is2D)
        {
            detectionComponent2D = GetComponent<DetectionComponent2D>();
        }
        else
        {
            detectionComponent3D = GetComponent<DetectionComponent>();
        }
    }

    private void Update()
    {
        ScanForTarget();
    }

    private void ScanForTarget()
    {
        Transform target = is2D ? detectionComponent2D.ScanForTarget() : detectionComponent3D.ScanForTarget();

        if (is2D)
        {
            detectionComponent2D.IsTargetDetected(target);
        }
        else
        {
            detectionComponent3D.IsTargetDetected(target);
        }
    }
}
