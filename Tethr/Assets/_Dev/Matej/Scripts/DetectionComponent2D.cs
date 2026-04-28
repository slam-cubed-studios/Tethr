// Copyright (c) 2026, TheMGLegends. All rights reserved.

using Tethr.DetectionVisualiser;
using UnityEngine;

namespace Tethr
{
    // TODO: 2D Version Summary of DetectionComponentBase
    public class DetectionComponent2D : DetectionComponentBase
    {
        protected override void DrawDetectionGizmos()
        {
            Vector3 centre = transform.position + spatialData.detectionOffset;

            switch (spatialData.detectionType)
            {
                case DetectionType.Range:
                    DetectionVisualiserUtility.DrawRange2D(centre, spatialData.radius, spatialData.isTargetVisible);
                    break;
                case DetectionType.FieldOfView:
                    float rotation = transform.eulerAngles.z - spatialData.rotationOffset;
                    DetectionVisualiserUtility.DrawFieldOfView2D(centre, spatialData.angle, spatialData.radius, spatialData.isTargetVisible, rotation);
                    break;
                case DetectionType.Bounds:
                    DetectionVisualiserUtility.DrawBounds(centre, spatialData.boundsSize, spatialData.isTargetVisible);
                    break;
                default:
                    break;
            }
        }
    }
}
