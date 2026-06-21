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

        Vector3 groundedPlayerPosition = GameWorldGround.ProjectToGround(_player.position);
        Vector3 groundedEnemyPosition = GameWorldGround.ProjectToGround(transform.position);
        transform.position = groundedEnemyPosition;

        float distance = Vector3.Distance(groundedEnemyPosition, groundedPlayerPosition);

        // Face player
        if ((groundedPlayerPosition - groundedEnemyPosition).sqrMagnitude > Mathf.Epsilon)
            transform.LookAt(groundedPlayerPosition);

        // Move toward player
        if (distance > attackRange)
        {
            transform.position = Vector3.MoveTowards(
                groundedEnemyPosition,
                groundedPlayerPosition,
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

        PlayerHealth player = _player.GetComponent<PlayerHealth>()
                              ?? _player.GetComponentInParent<PlayerHealth>()
                              ?? _player.GetComponentInChildren<PlayerHealth>();

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
