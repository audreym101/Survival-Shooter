using UnityEngine;

public class MeleeEnemy : EnemyBase
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int attackDamage = 10;

    private Transform _player;
    private float _nextAttackTime;
    private Animator _animator;

    protected override void Awake()
    {
        base.Awake();

        // Look for Animator on this object or children
        _animator = GetComponentInChildren<Animator>();

        if (_animator == null)
        {
            Debug.LogWarning(gameObject.name + " has no Animator component.");
        }
    }

    private void Start()
    {
        if (Camera.main != null)
        {
            _player = Camera.main.transform;
        }

        AudioManager.Instance?.PlayEnemySpawn();
    }

    private void Update()
    {
        if (_player == null)
            return;

        float distance = Vector3.Distance(transform.position, _player.position);

        // Face player
        Vector3 lookPos = _player.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        // Move toward player
        if (distance > attackRange)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                _player.position,
                moveSpeed * Time.deltaTime
            );

            if (_animator != null)
                _animator.SetBool("isWalking", true);
        }
        else
        {
            if (_animator != null)
                _animator.SetBool("isWalking", false);

            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (Time.time < _nextAttackTime)
            return;

        _nextAttackTime = Time.time + attackCooldown;

        if (_animator != null)
            _animator.SetTrigger("attack");

        AudioManager.Instance?.PlayEnemyDamage();

        PlayerHealth player = _player.GetComponent<PlayerHealth>();

        if (player != null)
        {
            player.TakeDamage(attackDamage);
        }
    }

    protected override void Die()
    {
        if (_animator != null)
        {
            _animator.SetTrigger("die");
        }

        base.Die();
    }
}