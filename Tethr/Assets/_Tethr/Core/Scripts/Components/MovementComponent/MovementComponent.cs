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
    public class WaypointData
    {
        [InlineButton(nameof(ReversePath), "Reverse Path")]
        public PathTraversalType PathTraversalType = PathTraversalType.Once;

        public List<Vector3> Waypoints;

        [HideInInspector] public int PreviousWaypointIndex = 0;
        [HideInInspector] public int NextWaypointIndex = 0;

        private bool isEnteringPath = true;
        private int direction = 1;

        public Vector3 GetNextWaypointPosition()
        {
            if (!IsValid())
            {
                Message.LogWarning($"Cannot get next waypoint, due to the waypoints list not being valid. " +
                                   $"Ensure the list contains at least one waypoint, and at least two for PingPong traversal.");
                return Vector3.zero;
            }

            // INFO: Ensure the entity goes to the first waypoint
            if (isEnteringPath)
            {
                isEnteringPath = false;
                return Waypoints[0];
            }

            switch (PathTraversalType)
            {
                case PathTraversalType.Once:
                    if (++NextWaypointIndex >= Waypoints.Count)
                    {
                        // INFO: Halts at the end of the path
                        NextWaypointIndex = Waypoints.Count - 1;
                        PreviousWaypointIndex = Waypoints.Count - 1;
                    }
                    else
                    {
                        PreviousWaypointIndex = NextWaypointIndex - 1;
                    }

                    break;
                case PathTraversalType.Loop:
                    if (++NextWaypointIndex >= Waypoints.Count)
                    {
                        // INFO: Wraps around to the beginning of the path
                        NextWaypointIndex = 0;
                        PreviousWaypointIndex = Waypoints.Count - 1;
                    }
                    else
                    {
                        PreviousWaypointIndex = NextWaypointIndex - 1;
                    }

                    break;
                case PathTraversalType.PingPong:
                    NextWaypointIndex += direction;

                    if (NextWaypointIndex >= Waypoints.Count)
                    {
                        // INFO: Travels back down the path in reverse when the end is reached
                        direction = -1;
                        NextWaypointIndex = Waypoints.Count - 2;
                    }
                    else if (NextWaypointIndex < 0)
                    {
                        direction = 1;
                        NextWaypointIndex = 1;
                    }

                    PreviousWaypointIndex = NextWaypointIndex - direction;
                    break;
            }

            return Waypoints[NextWaypointIndex];
        }

        public void ResetPath()
        {
            PreviousWaypointIndex = 0;
            NextWaypointIndex = 0;
            isEnteringPath = true;
        }

        private bool IsValid()
        {
            bool isValid = Waypoints != null && Waypoints.Count > 0;
            if (PathTraversalType == PathTraversalType.PingPong)
            {
                isValid &= Waypoints.Count > 1;
            }

            return isValid;
        }

        private void ReversePath() => Waypoints?.Reverse();
    }

    /// <summary>
    /// Represents configuration settings for roam-based movement, including the roam range, exclusion range and
    /// logic for determining the next roam position within the specified ranges.
    /// </summary>
    [Serializable]
    public class RoamData
    {
        [Tooltip("The maximum distance the object can roam from its current position.")]
        [Min(0.0f)] public Vector3 RoamRange = Vector3.one;

        [Tooltip("The minimum distance around the object that will be ignored when choosing a position to roam to.")]
        [Min(0.0f)] public Vector3 ExclusionRange = Vector3.zero;

        [Header("Debug Settings")]
        public Color RoamRangeColour = Color.cyan;
        public Color ExclusionRangeColour = Color.red;

        public Vector3 GetRandomRoamPosition()
        {
            // INFO: Randomly determines the direction (Negative (-1) or Positive (1))
            int directionX = UnityEngine.Random.Range(0, 2) * 2 - 1;
            int directionY = UnityEngine.Random.Range(0, 2) * 2 - 1;
            int directionZ = UnityEngine.Random.Range(0, 2) * 2 - 1;

            return new Vector3(UnityEngine.Random.Range(ExclusionRange.x, RoamRange.x) * directionX,
                               UnityEngine.Random.Range(ExclusionRange.y, RoamRange.y) * directionY,
                               UnityEngine.Random.Range(ExclusionRange.z, RoamRange.z) * directionZ);
        }

        internal void Validate()
        {
            // INFO: Ensure exclusion range does not exceed roam range in any dimension
            ExclusionRange = Vector3.Min(ExclusionRange, RoamRange);
        }

        internal void DrawRoamGizmos(Vector3 centre)
        {
#if UNITY_EDITOR
            // INFO: Roam Range Debug Visualisation
            Vector3 roamRangeSize = RoamRange * 2.0f;
            Handles.color = RoamRangeColour;
            Handles.DrawWireCube(centre, roamRangeSize);

            Gizmos.color = new Color(RoamRangeColour.r, RoamRangeColour.g, RoamRangeColour.b, 0.05f);
            Gizmos.DrawCube(centre, roamRangeSize);

            // INFO: Exclusion Range Debug Visualisation
            Vector3 exclusionRangeSize = ExclusionRange * 2.0f;
            Handles.color = ExclusionRangeColour;
            Handles.DrawWireCube(centre, exclusionRangeSize);

            Gizmos.color = new Color(ExclusionRangeColour.r, ExclusionRangeColour.g, ExclusionRangeColour.b, 0.05f);
            Gizmos.DrawCube(centre, exclusionRangeSize);
#endif
        }
    }

    /// <summary>
    /// Handles non-physics based movement logic for objects, allowing them to move towards a destination based on the specified 
    /// movement type.
    /// </summary>
    /// 
    /// <remarks>
    /// This component works in both 2D and 3D scenarios, however users should be aware of the z-axis when working in 2D, as
    /// checks like <see cref="HasReachedDestination"/> naturally work with 3D space, so the z-axis may need to be ignored when 
    /// working in 2D. 
    /// </remarks>
    public class MovementComponent : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private DebugDrawType debugDrawType = DebugDrawType.WhenSelected;

        [Header("General Settings")]
        [SerializeField, Min(0.0f)] private float movementSpeed = 1.0f;

        [Tooltip("Determines the range at which the object is considered to have reached its destination.")]
        [SerializeField, Min(0.01f)] private float destinationThreshold = 0.01f;

        [SerializeField] private MovementType movementType = MovementType.Manual;

        [Space(10.0f)]

        [ShowField(nameof(movementType), MovementType.Waypoint), Title("<b> Waypoint Settings</b>", 12, 0.0f, false)]
        [SerializeField] private WaypointData waypointData;

        [ShowField(nameof(movementType), MovementType.Roam), Title("<b> Roam Settings</b>", 12, 0.0f, false)]
        [SerializeField] private RoamData roamData;

        private Vector3 destination;

        private void OnValidate()
        {
            if (movementType == MovementType.Roam)
            {
                roamData?.Validate();
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
        /// Moves the object towards the current destination position.
        /// </summary>
        /// 
        /// <remarks>
        /// The destination can be set manually using <see cref="SetDestination(Vector3)"/>, or automatically updated 
        /// based on the movement type using <see cref="UpdateDestination()"/>.
        /// </remarks>
        public void MoveToDestination()
        {
            MoveTo(destination);
        }

        /// <summary>
        /// Moves the object towards the specified position.
        /// </summary>
        ///
        /// <param name="position">
        /// The position to move towards.
        /// </param>
        public void MoveTo(Vector3 position)
        {
            transform.position = Vector3.MoveTowards(transform.position, position, movementSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Determines if the destination position has been reached using the specified distance threshold.
        /// </summary>
        /// 
        /// <returns>
        /// <see langword="true"/> if the destination position has been reached, <see langword="false"/> otherwise.
        /// </returns>
        public bool HasReachedDestination()
        {
            return Vector3.Distance(transform.position, destination) < destinationThreshold;
        }

        /// <summary>
        /// Updates the destination position based on the current movement type.
        /// </summary>
        /// 
        /// <remarks>
        /// The destination position using this method cannot be updated if the movement type is set to Manual, use 
        /// <see cref="SetDestination(Vector3)"/> instead.
        /// </remarks>
        public void UpdateDestination()
        {
            switch (movementType)
            {
                case MovementType.Manual:
                    Message.LogWarning("MovementType is set to Manual. Destination must be set manually using SetDestination.");
                    break;
                case MovementType.Waypoint:
                    SetDestination(waypointData?.GetNextWaypointPosition() ?? Vector3.zero);
                    break;
                case MovementType.Roam:
                    SetDestination(transform.position + (roamData?.GetRandomRoamPosition() ?? Vector3.zero));
                    break;
                default:
                    break;
            }
        }

        public void ResetPath()
        {
            if (movementType == MovementType.Waypoint)
            {
                waypointData?.ResetPath();
            }
        }

        public MovementType GetMovementType() => movementType;

        public void SetDestination(Vector3 destination)
        {
            this.destination = destination;
        }

        public Vector3 GetDestination() => destination;

        private void DrawMovementGizmos()
        {
#if UNITY_EDITOR
            switch (movementType)
            {
                case MovementType.Manual:
                    if (!HasReachedDestination() && Application.isPlaying)
                    {
                        PathVisualiserUtility.DrawConnection(transform.position, destination);
                    }
                    break;
                case MovementType.Waypoint:
                    if (waypointData != null)
                    {
                        PathVisualiserUtility.DrawPath(waypointData.Waypoints, waypointData.PathTraversalType,
                                                       waypointData.PreviousWaypointIndex, waypointData.NextWaypointIndex);
                    }
                    break;
                case MovementType.Roam:
                    roamData?.DrawRoamGizmos(transform.position);

                    if (!HasReachedDestination() && Application.isPlaying)
                    {
                        PathVisualiserUtility.DrawConnection(transform.position, destination);
                    }
                    break;
            }
#endif
        }

        private void ReversePath() => waypointData?.Waypoints.Reverse();
    }
}
