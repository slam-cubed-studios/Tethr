// Copyright (c) 2026, TheMGLegends. All rights reserved.

using UnityEngine;

namespace Tethr.PathVisualiser.Examples
{
    /// <summary>
    /// Demonstrates the path visualisation to a target point in the Unity Editor Scene.
    /// </summary>
    /// 
    /// <remarks>
    /// Intended for use as an example or reference implementation.
    /// </remarks>
    public class TargetVisualisationExample : MonoBehaviour
    {
        [SerializeField] private Transform target;

        private void OnDrawGizmosSelected()
        {
            if (target)
            {
                PathVisualiserUtility.DrawTargetLine(transform.position, target.position);
            }
        }
    }
}
