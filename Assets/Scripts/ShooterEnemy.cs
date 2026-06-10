using UnityEngine;

public class ShooterEnemy : EnemyBase
{
    [SerializeField] float moveSpeed = 1.5f;
    [SerializeField] float shootingDistance = 6f;
    [SerializeField] float shootCooldown = 2f;
    [SerializeField] int attackDamage = 15;
    [SerializeField] Transform firePoint;

    Transform _player;
    float _nextShootTime;
    Animator _animator;

    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        _player = Camera.main.transform;
        AudioManager.Instance?.PlayEnemySpawn();
    }

    void Update()
    {
        if (_player == null) return;

        float distance = Vector3.Distance(transform.position, _player.position);
        transform.LookAt(_player);

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

        PlayerHealth player = _player.GetComponent<PlayerHealth>();
        player?.TakeDamage(attackDamage);
    }

    protected override void Die()
    {
        _animator?.SetTrigger("die");
        base.Die();
    }
}
