using UnityEngine;

public class ShooterEnemy : EnemyBase
{
    [SerializeField] float moveSpeed = 1.5f;
    [SerializeField] float shootingDistance = 6f;
    [SerializeField] float shootCooldown = 2f;
    [SerializeField] int attackDamage = 15;
    [SerializeField] float defaultFireHeight = 1.2f;
    [SerializeField] float defaultFireForwardOffset = 0.5f;
    [SerializeField] Transform firePoint;

    Transform _player;
    PlayerHealth _playerHealth;
    float _nextShootTime;
    Animator _animator;

    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponent<Animator>();
        
        if (_animator == null)
        {
            Debug.LogWarning($"{gameObject.name} has no Animator component!");
        }
        else if (_animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"{gameObject.name} Animator has no controller assigned!");
        }
    }

    void Start()
    {
        if (Camera.main != null)
            _player = Camera.main.transform;

        _playerHealth = ResolvePlayerHealth();
        AudioManager.Instance?.PlayEnemySpawn();
    }

    void Update()
    {
        if (_player == null) return;

        float distance = Vector3.Distance(transform.position, _player.position);
        Vector3 lookPosition = _player.position;
        lookPosition.y = transform.position.y;
        transform.LookAt(lookPosition);

        if (distance > shootingDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, _player.position, moveSpeed * Time.deltaTime);
            _animator?.SetBool("isWalking", true);
        }
        else
        {
            _animator?.SetBool("isWalking", false);
            TryShoot();
        }
    }

    void TryShoot()
    {
        if (Time.time < _nextShootTime) return;
        _nextShootTime = Time.time + shootCooldown;

        _animator?.SetTrigger("shoot");
        AudioManager.Instance?.PlayEnemyShoot();

        FireProjectile();
    }

    void FireProjectile()
    {
        if (_playerHealth == null)
            _playerHealth = ResolvePlayerHealth();

        if (BulletPool.Instance == null)
        {
            _playerHealth?.TakeDamage(attackDamage);
            return;
        }

        Vector3 origin = GetFirePosition();
        Vector3 direction = (_player.position - origin).normalized;
        if (direction == Vector3.zero)
            direction = transform.forward;

        GameObject bulletObject = BulletPool.Instance.Get(origin, Quaternion.LookRotation(direction));
        if (bulletObject.TryGetComponent(out Bullet bullet))
            bullet.ConfigureEnemyBullet(attackDamage, _playerHealth, _player);
    }

    Vector3 GetFirePosition()
    {
        if (firePoint != null)
            return firePoint.position;

        return transform.position
               + Vector3.up * defaultFireHeight
               + transform.forward * defaultFireForwardOffset;
    }

    PlayerHealth ResolvePlayerHealth()
    {
        if (_player == null)
            return FindFirstObjectByType<PlayerHealth>();

        PlayerHealth playerHealth = _player.GetComponent<PlayerHealth>()
                                    ?? _player.GetComponentInParent<PlayerHealth>()
                                    ?? _player.GetComponentInChildren<PlayerHealth>();

        return playerHealth != null
            ? playerHealth
            : FindFirstObjectByType<PlayerHealth>();
    }

    protected override void Die()
    {
        _animator?.SetTrigger("die");
        base.Die();
    }
}
