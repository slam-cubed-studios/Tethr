// Copyright (c) 2026, TheMGLegends. All rights reserved.

using EditorAttributes;
using System;
using System.Collections.Generic;
using Tethr.PathVisualiser;
using UnityEditor;
using UnityEngine;

namespace Tethr
{
    /// <summary>
    /// Specifies the type of movement behaviour, determining how the object moves and how its destination is updated.
    /// </summary>
    public enum MovementType
    {
        Manual,
        Waypoint,
        Roam
    }

    /// <summary>
    /// Represents configuration settings for waypoint-based movement, including the list of waypoints, traversal type
    /// and logic for determining the next waypoint position.
    /// </summary>
    [Serializable]
    public struct WaypointData
    {
        [InlineButton(nameof(ReversePath), "Reverse Path")]
        public PathTraversalType traversalType;

        public List<Vector3> waypoints;

        [HideInInspector] public int previousWaypointIndex;
        [HideInInspector] public int nextWaypointIndex;
        private bool needsInitialWaypoint;

        public WaypointData(bool useDefaults)
        {
            if (useDefaults)
            {
                traversalType = PathTraversalType.Once;
                waypoints = new();
                previousWaypointIndex = 0;
                nextWaypointIndex = 0;
                needsInitialWaypoint = true;
            }
            else
            {
                traversalType = default;
                waypoints = default;
                previousWaypointIndex = default;
                nextWaypointIndex = default;
                needsInitialWaypoint = default;
            }
        }

        public Vector3 GetNextWaypointPosition()
        {
            if (!IsValid())
            {
                Debug.LogWarning("Waypoint list is null or empty. Cannot get next waypoint position.");
                return Vector3.zero;
            }

            // INFO: Ensures that the entity walks to the first waypoint if it's not already there
            if (needsInitialWaypoint)
            {
                needsInitialWaypoint = false;
                return waypoints[0];
            }

            bool isAtEndOfPath = false;
            switch (traversalType)
            {
                case PathTraversalType.Once:
                    if (++nextWaypointIndex == waypoints.Count)
                    {
                        // INFO: Halts at the end of the path
                        isAtEndOfPath = true;
                        nextWaypointIndex = waypoints.Count - 1;
                        previousWaypointIndex = waypoints.Count - 1;
                    }
                    break;
                case PathTraversalType.Loop:
                    if (++nextWaypointIndex == waypoints.Count)
                    {
                        // INFO: Wraps around to the beginning of the path
                        isAtEndOfPath = true;
                        nextWaypointIndex = 0;
                        previousWaypointIndex = waypoints.Count - 1;
                    }
                    break;
                case PathTraversalType.PingPong:
                    if (++nextWaypointIndex == waypoints.Count)
                    {
                        // INFO: Reverse the path and walks back to what was previously considered
                        //       the beginning of the path
                        isAtEndOfPath = true;
                        nextWaypointIndex = 1;
                        previousWaypointIndex = 0;
                        waypoints.Reverse();
                    }
                    break;
            }

            if (!isAtEndOfPath)
            {
                previousWaypointIndex = nextWaypointIndex - 1;
            }

            return waypoints[nextWaypointIndex];
        }

        public readonly bool IsValid()
        {
            return waypoints != null && waypoints.Count > 0;
        }

        private readonly void ReversePath() => waypoints?.Reverse();
    }

    /// <summary>
    /// Represents configuration settings for roam-based movement, including the roam range, exclusion range and
    /// logic for determining the next roam position within the specified ranges.
    /// </summary>
    [Serializable]
    public struct RoamData
    {
        [Tooltip("The maximum distance the object can roam from its current position.")]
        [Min(0.0f)] public Vector3 roamRange;

        [Tooltip("The minimum distance around the object that will be ignored choosing a position to roam to.")]
        [Min(0.0f)] public Vector3 exclusionRange;

        public RoamData(bool useDefaults)
        {
            if (useDefaults)
            {
                roamRange = Vector3.zero;
                exclusionRange = Vector3.zero;
            }
            else
            {
                roamRange = default;
                exclusionRange = default;
            }
        }

        public readonly Vector3 GetRandomRoamPosition()
        {
            // INFO: Randomly determines the direction (Negative -1 or Positive 1)
            int directionX = UnityEngine.Random.Range(0, 2) * 2 - 1;
            int directionY = UnityEngine.Random.Range(0, 2) * 2 - 1;
            int directionZ = UnityEngine.Random.Range(0, 2) * 2 - 1;

            return new Vector3(UnityEngine.Random.Range(exclusionRange.x, roamRange.x) * directionX,
                               UnityEngine.Random.Range(exclusionRange.y, roamRange.y) * directionY,
                               UnityEngine.Random.Range(exclusionRange.z, roamRange.z) * directionZ);
        }

        public void Validate()
        {
            if (exclusionRange.x > roamRange.x)
            {
                exclusionRange.x = roamRange.x;
            }

            if (exclusionRange.y > roamRange.y)
            {
                exclusionRange.y = roamRange.y;
            }

            if (exclusionRange.z > roamRange.z)
            {
                exclusionRange.z = roamRange.z;
            }
        }

        public readonly void DrawRoamGizmos(Vector3 position)
        {
            // INFO: Roam Range Debug Visualisation
            Handles.color = Color.cyan;
            Handles.DrawWireCube(position, roamRange * 2.0f);

            // INFO: Exclusion Range Debug Visualisation
            Handles.color = Color.red;
            Handles.DrawWireCube(position, exclusionRange * 2.0f);
        }
    }

    /// <summary>
    /// Handles non-physics based movement logic for an object, allowing it to move towards a destination based on the specified 
    /// movement type (Manual, Waypoint, Roam).
    /// </summary>
    /// 
    /// <remarks>
    /// This component works for both 2D and 3D use cases, however users should be aware of the z-axis when working in 2D, as
    /// checks like <see cref="HasReachedDestination"/> will consider the z-axis when determining whether the object has reached 
    /// its destination.
    /// </remarks>
    public class MovementComponent : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private DebugDrawType debugDrawType = DebugDrawType.WhenSelected;

        [Header("General Settings")]
        [SerializeField, Min(0.0f)] private float movementSpeed = 1.0f;

        [Tooltip("The distance threshold at which the object is considered to have reached its destination. ")]
        [SerializeField, Min(0.01f)] private float destinationThreshold = 0.01f;

        [SerializeField] private MovementType movementType = MovementType.Manual;

        [Space(10.0f)]

        [ShowField(nameof(movementType), MovementType.Waypoint), Title("<b> Waypoint Settings</b>", 12, 0.0f, false)]
        [SerializeField] private WaypointData waypointData = new(true);

        [ShowField(nameof(movementType), MovementType.Roam), Title("<b> Roam Settings</b>", 12, 0.0f, false)]
        [SerializeField] private RoamData roamData = new(true);

        private Vector3 destination;

        private void OnValidate()
        {
            // INFO: Prevents the drawing of path gizmos in the editor view
            destination = transform.position;

            if (movementType == MovementType.Roam)
            {
                roamData.Validate();
            }
        }

        private void OnDrawGizmos()
        {
            if (debugDrawType != DebugDrawType.Always)
            {
                return;
            }

            DrawMovementGizmos();
        }

        private void OnDrawGizmosSelected()
        {
            if (debugDrawType != DebugDrawType.WhenSelected)
            {
                return;
            }

            DrawMovementGizmos();
        }

        /// <summary>
        /// Convenience method to move towards the current destination. The destination can be set 
        /// manually using <see cref="SetDestination(Vector3)"/>
        /// </summary>
        public void MoveToDestination()
        {
            MoveTo(destination);
        }

        /// <summary>
        /// Used to move towards a specified position at the configured movement speed. This method
        /// does not perform any checks and is primarily inteded to be used when movementType is set to Manual.
        /// </summary>
        /// <param name="position"></param>
        public void MoveTo(Vector3 position)
        {
            transform.position = Vector3.MoveTowards(transform.position, position, movementSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Determines whether the current position of the object is within the specified distance threshold of the destination.
        /// </summary>
        /// <returns></returns>
        public bool HasReachedDestination()
        {
            return Vector3.Distance(transform.position, destination) < destinationThreshold;
        }

        /// <summary>
        /// Updates the destination based on the current movement type. For Manual movement, a warning is logged as the destination 
        /// must be set manually.
        /// </summary>
        public void UpdateDestination()
        {
            switch (movementType)
            {
                case MovementType.Manual:
                    Debug.LogWarning("MovementType is set to Manual. Destination must be set manually using SetDestination().");
                    break;
                case MovementType.Waypoint:
                    SetDestination(waypointData.GetNextWaypointPosition());
                    break;
                case MovementType.Roam:
                    SetDestination(transform.position + roamData.GetRandomRoamPosition());
                    break;
                default:
                    break;
            }
        }

        public MovementType GetMovementType() => movementType;

        public void SetDestination(Vector3 destination) => this.destination = destination;

        public Vector3 GetDestination() => destination;

        private void DrawMovementGizmos()
        {
            switch (movementType)
            {
                case MovementType.Manual:
                    if (!HasReachedDestination())
                    {
                        PathVisualiserUtility.DrawTargetLine(transform.position, destination);
                    }
                    break;
                case MovementType.Waypoint:
                    if (waypointData.IsValid())
                    {
                        PathVisualiserUtility.DrawPath(waypointData.waypoints.ToArray(), waypointData.traversalType, 
                                                       waypointData.previousWaypointIndex, waypointData.nextWaypointIndex);
                    }
                    break;
                case MovementType.Roam:
                    roamData.DrawRoamGizmos(transform.position);

                    if (!HasReachedDestination())
                    {
                        PathVisualiserUtility.DrawTargetLine(transform.position, destination);
                    }
                    break;
            }
        }

        private void ReversePath() => waypointData.waypoints?.Reverse();
    }
}
