using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 15f;
    [SerializeField] float lifetime = 3f;
    [SerializeField] int damage = 20;

    float _timer;

    void OnEnable() => _timer = 0f;

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        _timer += Time.deltaTime;
        if (_timer >= lifetime) BulletPool.Instance.ReturnToPool(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out EnemyBase enemy))
            enemy.TakeDamage(damage);
        BulletPool.Instance.ReturnToPool(gameObject);
    }
}
