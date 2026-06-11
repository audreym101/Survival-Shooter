using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int scoreValue = 10;

    protected int currentHealth;

    DamageFeedback _feedback;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        _feedback = GetComponent<DamageFeedback>();
    }

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= amount;
        AudioManager.Instance?.PlayEnemyDamage();
        _feedback?.ShowEnemyHit();
        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        ScoreManager.Instance?.AddScore(scoreValue);
        Destroy(gameObject);
    }
}
