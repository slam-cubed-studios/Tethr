using EditorAttributes;
using System;
using UnityEngine;

/// <summary>
/// Component that manages health for a GameObject, including damage, healing, and invincibility mechanics.
/// </summary>
public class HealthComponent : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField, ReadOnly] private float currentHealth = 0.0f;
    [SerializeField, Min(0.0f)] private float maxHealth = 1.0f;

    [Header("Invincibility Settings")]
    [SerializeField, Min(0.0f)] private float invincibilityDuration = 0.0f;
    [SerializeField, ReadOnly] private bool isInvincible = false;

    public class OnHealthChangedEventArgs : EventArgs
    {
        public float oldHealth;
        public float newHealth;

        public OnHealthChangedEventArgs(float oldHealth, float newHealth)
        {
            this.oldHealth = oldHealth;
            this.newHealth = newHealth;
        }
    }
    public event EventHandler<OnHealthChangedEventArgs> OnHealed;
    public event EventHandler<OnHealthChangedEventArgs> OnDamaged;
    public event EventHandler OnDied;

    private void OnValidate()
    {
        if (currentHealth != maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damageAmount)
    {
        if (isInvincible || IsDead())
        {
            return;
        }

        // INFO: Ensure damage amount is positive
        damageAmount = Mathf.Abs(damageAmount);

        float oldHealth = currentHealth;
        float newHealth = Mathf.Max(currentHealth - damageAmount, 0.0f);
        currentHealth = newHealth;

        OnDamaged?.Invoke(this, new OnHealthChangedEventArgs(oldHealth, newHealth));

        if (IsDead())
        {
            OnDied?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            TryBeginInvincibility();
        }
    }

    public void Heal(float healAmount)
    {
        if (IsDead())
        {
            return;
        }

        // INFO: Ensure heal amount is positive
        healAmount = Mathf.Abs(healAmount);

        float oldHealth = currentHealth;
        float newHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        currentHealth = newHealth;

        OnHealed?.Invoke(this, new OnHealthChangedEventArgs(oldHealth, newHealth));
    }

    public bool IsDead()
    {
        return currentHealth <= 0.0f;
    }

    public bool IsInvincible() => isInvincible;

    public void SetCurrentHealth(float currentHealth)
    {
        // INFO: Ensure new current health value is positive
        currentHealth = Mathf.Abs(currentHealth);
        this.currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    public float GetCurrentHealth() => currentHealth;

    public void SetMaxHealth(float maxHealth)
    {
        // INFO: Ensure new max health value is positive
        maxHealth = Mathf.Abs(maxHealth);
        this.maxHealth = maxHealth;

        // INFO: Ensure current health does not exceed new max health
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    public float GetMaxHealth() => maxHealth;

    public void SetInvincibilityDuration(float invincibilityDuration)
    {
        // INFO: Ensure new invincibility duration value is positive
        invincibilityDuration = Mathf.Abs(invincibilityDuration);
        this.invincibilityDuration = invincibilityDuration;
    }

    public float GetInvincibilityDuration() => invincibilityDuration;

    private void TryBeginInvincibility()
    {
        if (invincibilityDuration <= 0.0f)
        {
            return;
        }

        isInvincible = true;
        Invoke(nameof(EndInvincibility), invincibilityDuration);
    }

    private void EndInvincibility()
    {
        isInvincible = false;
    }
}
