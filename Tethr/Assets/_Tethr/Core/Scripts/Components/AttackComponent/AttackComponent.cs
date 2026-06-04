// Copyright (c) 2026, TheMGLegends. All rights reserved.

using System;
using UnityEngine;

namespace Tethr
{
    /// <summary>
    /// Handles attack logic for an object, including checking if a target can be attacked based on 
    /// detection results and managing attack cooldowns.
    /// </summary>
    /// 
    /// <remarks>
    /// This component relies on a BaseDetectionComponent derived class to function properly.
    /// Make sure one is added to the same GameObject.
    /// </remarks>
    public class AttackComponent : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private DebugDrawType debugDrawType = DebugDrawType.WhenSelected;

        [Space(10.0f)]

        [Header("Detection Settings")]
        [SerializeField] private SpatialData spatialData;

        [Header("Attack Settings")]
        [SerializeField] private float attackCooldown = 1.0f;

        private BaseDetectionComponent detectionComponent;
        private float attackCooldownUntilTime;

        public event Action OnAttacked;

        private void OnValidate()
        {
            detectionComponent = GetComponent<BaseDetectionComponent>();
        }

        private void Awake()
        {
            detectionComponent = detectionComponent != null ? detectionComponent : GetComponent<BaseDetectionComponent>();
            if (detectionComponent == null)
            {
                Message.LogWarning($"AttackComponent on {gameObject.name} did not have a reference to a BaseDetectionComponent. " +
                                   $"Make sure to add a derived BaseDetectionComponent to the object");
            }
        }

        private void OnDrawGizmos()
        {
            if (debugDrawType != DebugDrawType.Always)
            {
                return;
            }

            if (detectionComponent != null)
            {
                detectionComponent.DrawDetectionGizmos(spatialData);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (debugDrawType != DebugDrawType.WhenSelected)
            {
                return;
            }

            if (detectionComponent != null)
            {
                detectionComponent.DrawDetectionGizmos(spatialData);
            }
        }

        /// <summary>
        /// Tries to attack the specified target.
        /// </summary>
        /// 
        /// <param name="target">
        /// The target which the component will attempt to attack.
        /// </param>
        public void TryAttack(Transform target)
        {
            if (CanAttack(target))
            {
                TryBeginCooldown();
                OnAttacked?.Invoke();
            }
        }

        /// <summary>
        /// Tries to attack the first detected target.
        /// </summary>
        public void TryAttack()
        {
            if (detectionComponent != null)
            {
                TryAttack(detectionComponent.ScanForTarget(spatialData));
            }
        }

        /// <summary>
        /// Checks if the specified target can be attacked based on detection results and cooldown status.
        /// </summary>
        /// 
        /// <param name="target">
        /// The target to check for attack validity.
        /// </param>
        public bool CanAttack(Transform target)
        {
            if (target == null || detectionComponent == null || IsAttackOnCooldown())
            {
                return false;
            }

            return detectionComponent.IsTargetDetected(target, spatialData);
        }

        public bool IsAttackOnCooldown()
        {
            return Time.time < attackCooldownUntilTime;
        }

        private void TryBeginCooldown()
        {
            if (attackCooldown <= 0.0f)
            {
                return;
            }

            attackCooldownUntilTime = Time.time + attackCooldown;
        }
    }
}
