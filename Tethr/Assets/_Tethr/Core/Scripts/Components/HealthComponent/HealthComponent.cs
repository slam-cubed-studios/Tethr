// Copyright (c) 2026, TheMGLegends. All rights reserved.

using EditorAttributes;
using System;
using UnityEngine;

namespace Tethr
{
    /// <summary>
    /// Manages an objects health, including damage, healing, and invincibility mechanics.
    /// </summary>
    public class HealthComponent : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField, Min(0.0f)] private float maxHealth = 1.0f;
        [SerializeField, ReadOnly] private float currentHealth = 0.0f;

        [Header("Invincibility Settings")]
        [SerializeField, Min(0.0f)] private float invincibilityDuration = 0.0f;

        public readonly struct OnHealthChangedData
        {
            public readonly float oldHealth;
            public readonly float newHealth;

            public OnHealthChangedData(float oldHealth, float newHealth)
            {
                this.oldHealth = oldHealth;
                this.newHealth = newHealth;
            }
        }

        public event Action<OnHealthChangedData> OnDamaged;
        public event Action<OnHealthChangedData> OnHealed;
        public event Action<OnHealthChangedData> OnRevived;
        public event Action OnDied;

        private float invincibleUntilTime;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        /// <summary>
        /// Damages the object by the specified amount, applying invincibility and invoking events as appropriate.
        /// </summary>
        /// 
        /// <param name="damageAmount">
        /// The amount of damage to apply. This value will be treated as positive regardless of the sign provided.
        /// </param>
        public void TakeDamage(float damageAmount)
        {
            // INFO: If the object is currently invincible or already dead
            if (IsInvincible() || IsDead())
            {
                return;
            }

            float oldHealth = currentHealth;
            currentHealth = Mathf.Max(currentHealth - Mathf.Abs(damageAmount), 0.0f);

            // INFO: If the damage didn't actually reduce health, don't invoke events or start invincibility
            if (Mathf.Approximately(currentHealth, oldHealth))
            {
                return;
            }

            OnDamaged?.Invoke(new OnHealthChangedData(oldHealth, currentHealth));

            // INFO: If the object just died from this damage
            if (IsDead())
            {
                OnDied?.Invoke();
            }
            else
            {
                TryBeginInvincibility();
            }
        }

        /// <summary>
        /// Heals the object by the specified amount, invoking events as appropriate. Healing will not occur if 
        /// the object is already dead.
        /// </summary>
        /// 
        /// <param name="healAmount">
        /// The amount of healing to apply. This value will be treated as positive regardless of the sign provided.
        /// </param>
        public void Heal(float healAmount)
        {
            if (IsDead())
            {
                return;
            }

            float oldHealth = currentHealth;
            currentHealth = Mathf.Min(currentHealth + Mathf.Abs(healAmount), maxHealth);

            // INFO: If the heal didn't actually increase health, don't invoke events
            if (Mathf.Approximately(currentHealth, oldHealth))
            {
                return;
            }

            OnHealed?.Invoke(new OnHealthChangedData(oldHealth, currentHealth));
        }

        public void Revive()
        {
            if (!IsDead())
            {
                return;
            }

            float oldHealth = currentHealth;
            currentHealth = maxHealth;

            // INFO: If the revive didn't actually change health, don't invoke events
            if (Mathf.Approximately(currentHealth, oldHealth))
            {
                return;
            }

            OnRevived?.Invoke(new OnHealthChangedData(oldHealth, currentHealth));
        }

        public bool IsDead()
        {
            return currentHealth <= 0.0f;
        }

        public bool IsInvincible()
        {
            return Time.time < invincibleUntilTime;
        }

        public float GetCurrentHealth() => currentHealth;

        public void SetMaxHealth(float maxHealth)
        {
            // INFO: Ensure new max health value is non-negative
            this.maxHealth = Mathf.Max(maxHealth, 0.0f);

            // INFO: Adjust current health if it exceeds the new max health
            currentHealth = Mathf.Clamp(currentHealth, 0.0f, this.maxHealth);
        }

        public float GetMaxHealth() => maxHealth;

        public void SetInvincibilityDuration(float invincibilityDuration)
        {
            // INFO: Ensure new invincibility duration value is non-negative
            this.invincibilityDuration = Mathf.Max(invincibilityDuration, 0.0f);
        }

        public float GetInvincibilityDuration() => invincibilityDuration;

        private void TryBeginInvincibility()
        {
            if (invincibilityDuration <= 0.0f)
            {
                return;
            }

            invincibleUntilTime = Time.time + invincibilityDuration;
        }
    }
}
