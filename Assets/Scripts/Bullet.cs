using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 15f;
    [SerializeField] float lifetime = 3f;
    [SerializeField] int damage = 20;
    [SerializeField] float enemyHitRadius = 0.35f;

    float _timer;
    bool _isEnemyBullet;
    int _activeDamage;
    PlayerHealth _targetPlayer;
    Transform _targetTransform;

    void OnEnable()
    {
        _timer = 0f;
        _activeDamage = damage;
        _isEnemyBullet = false;
        _targetPlayer = null;
        _targetTransform = null;
    }

    public void ConfigurePlayerBullet(int bulletDamage)
    {
        _activeDamage = bulletDamage;
        _isEnemyBullet = false;
        _targetPlayer = null;
        _targetTransform = null;
    }

    public void ConfigureEnemyBullet(int bulletDamage, PlayerHealth targetPlayer, Transform targetTransform)
    {
        _activeDamage = bulletDamage;
        _isEnemyBullet = true;
        _targetPlayer = targetPlayer;
        _targetTransform = targetTransform;
    }

    void Update()
    {
        Vector3 startPosition = transform.position;
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        _timer += Time.deltaTime;

        if (_isEnemyBullet && HasReachedTarget(startPosition, transform.position))
        {
            _targetPlayer?.TakeDamage(_activeDamage);
            Deactivate();
            return;
        }

        if (_timer >= lifetime) Deactivate();
    }

    void OnTriggerEnter(Collider other)
    {
        if (_isEnemyBullet)
        {
            PlayerHealth player = other.GetComponentInParent<PlayerHealth>()
                                  ?? other.GetComponentInChildren<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(_activeDamage);
                Deactivate();
            }
            return;
        }

        if (other.TryGetComponent(out EnemyBase enemy))
            enemy.TakeDamage(_activeDamage);

        Deactivate();
    }

    bool HasReachedTarget(Vector3 startPosition, Vector3 endPosition)
    {
        if (_targetPlayer == null || _targetTransform == null) return false;

        Vector3 targetPosition = _targetTransform.position;
        Vector3 bulletPath = endPosition - startPosition;
        float pathLengthSquared = bulletPath.sqrMagnitude;

        if (pathLengthSquared <= Mathf.Epsilon)
            return Vector3.Distance(endPosition, targetPosition) <= enemyHitRadius;

        float t = Vector3.Dot(targetPosition - startPosition, bulletPath) / pathLengthSquared;
        t = Mathf.Clamp01(t);

        Vector3 closestPoint = startPosition + bulletPath * t;
        return Vector3.Distance(closestPoint, targetPosition) <= enemyHitRadius;
    }

    void Deactivate()
    {
        if (BulletPool.Instance != null)
            BulletPool.Instance.ReturnToPool(gameObject);
        else
            gameObject.SetActive(false);
    }
}
