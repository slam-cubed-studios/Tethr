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
    [ExecuteInEditMode]
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
            if (nextPointIndex < 4)
            {
                currentPointIndex++;
                nextPointIndex++;
            }
            else if (!hasFinishedLinearTraversal)
            {
                hasFinishedLinearTraversal = true;
                currentPointIndex = 4;
                nextPointIndex = 4;
            }
        }

        private void LoopLogic()
        {
            currentPointIndex++;
            nextPointIndex++;
            if (nextPointIndex > 4)
            {
                nextPointIndex = 0;
            }

            if (currentPointIndex > 4)
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

                if (nextPointIndex > 4)
                {
                    nextPointIndex = 3;
                    currentPointIndex = 4;
                    isReversing = true;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            PathVisualiserUtility.DrawPath(path.ToArray(), traversalType, currentPointIndex, nextPointIndex);
        }
    }
}
#endif
