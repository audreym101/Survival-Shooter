using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] int maxHealth = 100;
    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

    public UnityEvent<int> onHealthChanged;
    public UnityEvent onPlayerDeath;

    void Awake() => CurrentHealth = maxHealth;

    public void TakeDamage(int amount)
    {
        if (IsDead)
            return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        onHealthChanged?.Invoke(CurrentHealth);
        if (CurrentHealth == 0) onPlayerDeath?.Invoke();
    }

    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        onHealthChanged?.Invoke(CurrentHealth);
    }

    public float HealthPercent => (float)CurrentHealth / maxHealth;
}
