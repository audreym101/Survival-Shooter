using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int scoreValue = 10;

    protected int currentHealth;

    protected virtual void Awake() => currentHealth = maxHealth;

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= amount;
        AudioManager.Instance?.PlayEnemyDamage();
        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        ScoreManager.Instance?.AddScore(scoreValue);
        Destroy(gameObject);
    }
}
