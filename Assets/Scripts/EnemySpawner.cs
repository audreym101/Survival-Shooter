using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject meleeEnemyPrefab;
    [SerializeField] GameObject shooterEnemyPrefab;
    [SerializeField] float spawnInterval = 5f;
    [SerializeField] float spawnRadius = 2f;

    ARPlaneManager _planeManager;
    readonly List<GameObject> _activeEnemies = new List<GameObject>();
    Coroutine _spawnCoroutine;

    void Awake() => _planeManager = FindObjectOfType<ARPlaneManager>();

    void Start()
    {
        if (DifficultyManager.Instance != null)
            spawnInterval = DifficultyManager.Instance.SpawnInterval;
    }

    public void StartSpawning()
    {
        _spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        if (_spawnCoroutine != null) StopCoroutine(_spawnCoroutine);
        ClearAllEnemies();
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        Vector3? spawnPos = GetSpawnPosition();
        if (spawnPos == null) return;

        bool spawnMelee = Random.value > 0.4f;
        GameObject prefab = spawnMelee ? meleeEnemyPrefab : shooterEnemyPrefab;
        GameObject enemy = Instantiate(prefab, spawnPos.Value, Quaternion.identity);
        _activeEnemies.Add(enemy);
    }

    Vector3? GetSpawnPosition()
    {
        foreach (var plane in _planeManager.trackables)
        {
            if (plane.alignment == PlaneAlignment.HorizontalUp)
            {
                Vector2 random = Random.insideUnitCircle * spawnRadius;
                return plane.transform.position + new Vector3(random.x, 0, random.y);
            }
        }
        return null;
    }

    void ClearAllEnemies()
    {
        foreach (var e in _activeEnemies)
            if (e != null) Destroy(e);
        _activeEnemies.Clear();
    }
}
