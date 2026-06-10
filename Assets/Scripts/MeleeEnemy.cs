using UnityEngine;

public class MeleeEnemy : EnemyBase
{
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float attackRange = 1.5f;
    [SerializeField] float attackCooldown = 1.5f;
    [SerializeField] int attackDamage = 10;

    Transform _player;
    float _nextAttackTime;
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

        if (distance > attackRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, _player.position, moveSpeed * Time.deltaTime);
            _animator?.SetBool("isWalking", true);
        }
        else
        {
            _animator?.SetBool("isWalking", false);
            TryAttack();
        }
    }

    void TryAttack()
    {
        if (Time.time < _nextAttackTime) return;
        _nextAttackTime = Time.time + attackCooldown;

        _animator?.SetTrigger("attack");
        AudioManager.Instance?.PlayEnemyDamage();

        PlayerHealth player = _player.GetComponent<PlayerHealth>();
        player?.TakeDamage(attackDamage);
    }

    protected override void Die()
    {
        _animator?.SetTrigger("die");
        base.Die();
    }
}
