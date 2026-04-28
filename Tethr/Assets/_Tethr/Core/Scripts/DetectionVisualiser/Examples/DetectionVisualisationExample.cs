// Copyright (c) 2026, TheMGLegends. All rights reserved.

using UnityEngine;

#if UNITY_EDITOR
using EditorAttributes;

namespace Tethr.DetectionVisualiser.Examples
{
    /// <summary>
    /// Demonstrates detection visualisation in the Unity Editor using different detection types.
    /// </summary>
    /// 
    /// <remarks>
    /// Supports Range, Field of View, and Bounds detection types. Visualises the detection area and 
    /// optionally target visibility in the editor. Intended for use as an example or reference implementation.
    /// </remarks>
    public class DetectionVisualisationExample : MonoBehaviour
    {
        private enum DetectionType
        {
            Range,
            FieldOfView,
            Bounds
        }

        [Header("Test Settings")]
        [SerializeField] private DetectionType detectionType = DetectionType.Range;
        [SerializeField, HideField(nameof(detectionType), DetectionType.Bounds)] private bool is3D = false;
        [SerializeField] private bool isTargetVisible = false;

        [Space(10.0f)]

        [Header("Detection Settings")]
        [SerializeField, ShowField(nameof(detectionType), DetectionType.FieldOfView), Range(0.0f, 360.0f)] private float angle = 270.0f;
        [SerializeField, HideField(nameof(detectionType), DetectionType.Bounds), Min(0.0f)] private float radius = 5.0f;
        [SerializeField, ShowField(nameof(detectionType), DetectionType.Bounds)] private Vector3 size = Vector3.one;
        [SerializeField, ShowField(nameof(detectionType), DetectionType.FieldOfView), Range(0.0f, 360.0f)] private float rotationOffset = 0.0f;
        [SerializeField] private Vector3 detectionOffset = Vector3.zero;

        private void OnDrawGizmos()
        {
            switch (detectionType)
            {
                case DetectionType.Range:
                    if (is3D)
                    {
                        DetectionVisualiserUtility.DrawRange(transform.position + detectionOffset, radius, isTargetVisible);
                    }
                    else
                    {
                        DetectionVisualiserUtility.DrawRange2D(transform.position + detectionOffset, radius, isTargetVisible);
                    }
                    break;
                case DetectionType.FieldOfView:
                    if (is3D)
                    {
                        DetectionVisualiserUtility.DrawFieldOfView(transform.position + detectionOffset, angle, radius, 
                                                                   isTargetVisible, transform.eulerAngles.y - rotationOffset);
                    }
                    else
                    {
                        DetectionVisualiserUtility.DrawFieldOfView2D(transform.position + detectionOffset, angle, radius, 
                                                                     isTargetVisible, transform.eulerAngles.z - rotationOffset);
                    }
                    break;
                case DetectionType.Bounds:
                    DetectionVisualiserUtility.DrawBounds(transform.position + detectionOffset, size, isTargetVisible);
                    break;
            }
        }
    }
}
#endif
