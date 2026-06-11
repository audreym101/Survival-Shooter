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

        // Spawn slightly above plane so they don't clip underground
        Vector3 pos = spawnPos.Value + Vector3.up * 0.1f;
        GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);

        // Make sure enemy is visible scale
        enemy.transform.localScale = Vector3.one;
        _activeEnemies.Add(enemy);
    }

    Vector3? GetSpawnPosition()
    {
        // Try AR planes first
        if (_planeManager != null)
        {
            foreach (var plane in _planeManager.trackables)
            {
                if (plane.alignment == PlaneAlignment.HorizontalUp)
                {
                    Vector2 random = Random.insideUnitCircle * spawnRadius;
                    return plane.transform.position + new Vector3(random.x, 0, random.y);
                }
            }
        }

        // Fallback for editor testing - spawn around the player
        Transform cam = Camera.main.transform;
        Vector2 editorRandom = Random.insideUnitCircle * spawnRadius;
        return cam.position + new Vector3(editorRandom.x, 0, editorRandom.y + 3f);
    }

    void ClearAllEnemies()
    {
        foreach (var e in _activeEnemies)
            if (e != null) Destroy(e);
        _activeEnemies.Clear();
    }
}
