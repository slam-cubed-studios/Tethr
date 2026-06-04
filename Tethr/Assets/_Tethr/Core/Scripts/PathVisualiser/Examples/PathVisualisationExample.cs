// Copyright (c) 2026, TheMGLegends. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using EditorAttributes;

namespace Tethr.PathVisualiser.Examples
{
    /// <summary>
    /// Demonstrates path traversal and visualisation in the Unity Editor using different traversal modes.
    /// </summary>
    /// 
    /// <remarks>
    /// Supports Once, Loop, and PingPong traversal types. Visualises the path and current traversal
    /// state in the editor. Intended for use as an example or reference implementation.
    /// </remarks>
    [ExecuteAlways]
    public class PathVisualisationExample : MonoBehaviour
    {
        [SerializeField] private PathTraversalType traversalType = PathTraversalType.Once;

        [Tooltip("Time in seconds between each traversal step.")]
        [SerializeField] private float delayTime = 1.0f;

        [Space(10.0f)]

        [SerializeField] private List<Vector3> path = new()
        {
            new Vector3(0, 0, 0),
            new Vector3(5, 0, 0),
            new Vector3(5, 5, 0),
            new Vector3(10, 5, 0),
            new Vector3(10, 15, 0)
        };

        [Button("Reverse Path")] public void ReversePathButton() => ReversePath();

        [Button("Reset Traversal")] public void ResetTraversalButton() => ResetLogic();

        private int currentPointIndex = 0;
        private int nextPointIndex = 1;
        private float timer = 0.0f;
        private bool hasFinishedLinearTraversal = false;
        private bool isReversing = false;

        private void OnValidate()
        {
            ResetLogic();
        }

        private void Update()
        {
            if (path == null || path.Count < 2)
            {
                return;
            }

            timer += Time.deltaTime;

            if (timer >= delayTime)
            {
                timer = 0.0f;

                switch (traversalType)
                {
                    case PathTraversalType.Once:
                        LinearLogic();
                        break;
                    case PathTraversalType.Loop:
                        LoopLogic();
                        break;
                    case PathTraversalType.PingPong:
                        PingPongLogic();
                        break;
                }
            }
        }

        private void ResetLogic()
        {
            currentPointIndex = 0;
            nextPointIndex = 1;
            timer = 0.0f;
            hasFinishedLinearTraversal = false;
            isReversing = false;
        }

        private void ReversePath()
        {
            path.Reverse();
            ResetLogic();
        }

        private void LinearLogic()
        {
            if (nextPointIndex < path.Count - 1)
            {
                currentPointIndex++;
                nextPointIndex++;
            }
            else if (!hasFinishedLinearTraversal)
            {
                hasFinishedLinearTraversal = true;
                currentPointIndex = path.Count - 1;
                nextPointIndex = path.Count - 1;
            }
        }

        private void LoopLogic()
        {
            currentPointIndex++;
            nextPointIndex++;
            if (nextPointIndex == path.Count)
            {
                nextPointIndex = 0;
            }

            if (currentPointIndex == path.Count)
            {
                currentPointIndex = 0;
            }
        }

        private void PingPongLogic()
        {
            if (isReversing)
            {
                currentPointIndex--;
                nextPointIndex--;

                if (nextPointIndex < 0)
                {
                    nextPointIndex = 1;
                    currentPointIndex = 0;
                    isReversing = false;
                }

            }
            else
            {
                currentPointIndex++;
                nextPointIndex++;

                if (nextPointIndex == path.Count)
                {
                    currentPointIndex = path.Count - 1;
                    nextPointIndex = currentPointIndex - 1;
                    isReversing = true;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (path != null && path.Count >= 2)
            {
                PathVisualiserUtility.DrawPath(path, traversalType, currentPointIndex, nextPointIndex);
            }
        }
    }
}
#endif
