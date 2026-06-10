using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance { get; private set; }

    [SerializeField] GameObject bulletPrefab;
    [SerializeField] int poolSize = 20;

    readonly Queue<GameObject> _pool = new Queue<GameObject>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject b = Instantiate(bulletPrefab);
            b.SetActive(false);
            _pool.Enqueue(b);
        }
    }

    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject bullet = _pool.Count > 0 ? _pool.Dequeue() : Instantiate(bulletPrefab);
        bullet.transform.SetPositionAndRotation(position, rotation);
        bullet.SetActive(true);
        return bullet;
    }

    public void ReturnToPool(GameObject bullet)
    {
        bullet.SetActive(false);
        _pool.Enqueue(bullet);
    }
}
