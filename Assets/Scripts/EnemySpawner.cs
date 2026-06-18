using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject meleeEnemyPrefab;
    [SerializeField] private GameObject shooterEnemyPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float spawnRadius = 3f;
    [SerializeField] private int maxEnemies = 10;

    private ARPlaneManager planeManager;
    private readonly List<GameObject> activeEnemies = new();
    private Coroutine spawnCoroutine;

    private void Awake()
    {
        planeManager = FindFirstObjectByType<ARPlaneManager>();
    }

    public void StartSpawning()
    {
        if (spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnRoutine());
            Debug.Log("Enemy spawning started");
        }
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        ClearAllEnemies();
    }

    private IEnumerator SpawnRoutine()
    {
        while (GameManager.Instance != null &&
               GameManager.Instance.IsPlaying)
        {
            if (activeEnemies.Count < maxEnemies)
            {
                SpawnEnemy();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        Vector3? spawnPosition = GetSpawnPosition();

        if (spawnPosition == null)
            return;

        bool spawnMelee = Random.value > 0.4f;

        GameObject prefab =
            spawnMelee ? meleeEnemyPrefab : shooterEnemyPrefab;

        if (prefab == null)
        {
            Debug.LogWarning("Enemy prefab missing!");
            return;
        }

        GameObject enemy = Instantiate(
            prefab,
            spawnPosition.Value,
            Quaternion.identity
        );

        activeEnemies.Add(enemy);

        AudioManager.Instance?.PlayEnemySpawn();
    }

    private Vector3? GetSpawnPosition()
    {
#if UNITY_EDITOR

        Vector2 random =
            Random.insideUnitCircle * spawnRadius;

        return new Vector3(
            random.x,
            0f,
            random.y + 5f
        );

#else

        if (planeManager == null)
            return null;

        foreach (ARPlane plane in planeManager.trackables)
        {
            if (plane.alignment == PlaneAlignment.HorizontalUp)
            {
                Vector2 random =
                    Random.insideUnitCircle * spawnRadius;

                return plane.transform.position +
                       new Vector3(
                           random.x,
                           0f,
                           random.y
                       );
            }
        }

        return null;

#endif
    }

    public void RemoveEnemy(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
    }

    private void ClearAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }

        activeEnemies.Clear();
    }
}