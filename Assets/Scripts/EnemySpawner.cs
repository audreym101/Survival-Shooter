using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject meleeEnemyPrefab;
    [SerializeField] private GameObject shooterEnemyPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float spawnRadius = 2f;
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
        Debug.Log("START SPAWNING");

        if (spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnRoutine());
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
        Debug.Log("🛑 Enemy spawning stopped");
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
                continue;

            if (activeEnemies.Count < maxEnemies)
                SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPosition = Camera.main != null
            ? Camera.main.transform.position + Camera.main.transform.forward * 2f
            : Vector3.zero;

        bool spawnMelee = Random.value > 0.4f;

        GameObject prefab = spawnMelee ? meleeEnemyPrefab : shooterEnemyPrefab;

        if (prefab == null)
        {
            Debug.LogError("❌ Enemy prefab missing!");
            return;
        }

        Debug.Log("🧟 Spawning enemy at: " + spawnPosition);

        GameObject enemy = Instantiate(
            prefab,
            spawnPosition,
            Quaternion.identity
        );

        // Enable all renderers (they may be disabled in the FBX model)
        SkinnedMeshRenderer[] skinnedRenderers = enemy.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer renderer in skinnedRenderers)
        {
            renderer.enabled = true;
        }

        MeshRenderer[] meshRenderers = enemy.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in meshRenderers)
        {
            renderer.enabled = true;
        }

        activeEnemies.Add(enemy);

        Debug.Log("✅ Enemy spawned successfully");

        AudioManager.Instance?.PlayEnemySpawn();
    }

    private Vector3? GetSpawnPosition()
    {
#if UNITY_EDITOR
        // Editor fallback (NO AR PLANE)
        Vector2 random = Random.insideUnitCircle * spawnRadius;

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
                Vector2 random = Random.insideUnitCircle * spawnRadius;

                return plane.transform.position +
                       new Vector3(random.x, 0f, random.y);
            }
        }

        return null;
#endif
    }

    public void RemoveEnemy(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
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

    // OPTIONAL DEBUG TOOL
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            Debug.Log("🧪 Manual spawn test");
            SpawnEnemy();
        }
    }
}