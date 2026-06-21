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

#if !UNITY_EDITOR
        if (planeManager != null)
        {
            planeManager.enabled = true;
            planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
        }
#endif
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
        Vector3? groundedSpawnPosition = GetSpawnPosition();
        if (!groundedSpawnPosition.HasValue)
        {
            Debug.LogWarning("No AR/game-world ground found for enemy spawn yet.");
            return;
        }

        Vector3 spawnPosition = groundedSpawnPosition.Value;

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

        // Ensure the enemy and all children are active
        enemy.SetActive(true);
        foreach (Transform child in enemy.GetComponentsInChildren<Transform>(includeInactive: true))
        {
            child.gameObject.SetActive(true);
        }

        // Enable all SkinnedMeshRenderers (for animated models)
        SkinnedMeshRenderer[] skinnedRenderers = enemy.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
        foreach (SkinnedMeshRenderer renderer in skinnedRenderers)
        {
            renderer.enabled = true;
            Debug.Log($"✓ Enabled SkinnedMeshRenderer on {renderer.gameObject.name}");
        }

        // Enable all MeshRenderers (for static meshes)
        MeshRenderer[] meshRenderers = enemy.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
        foreach (MeshRenderer renderer in meshRenderers)
        {
            renderer.enabled = true;
            Debug.Log($"✓ Enabled MeshRenderer on {renderer.gameObject.name}");
        }

        activeEnemies.Add(enemy);

        Debug.Log("✅ Enemy spawned successfully at " + enemy.transform.position);

        AudioManager.Instance?.PlayEnemySpawn();
    }

    private Vector3? GetSpawnPosition()
    {
        if (GameWorldGround.HasWorld && Camera.main != null)
        {
            Vector2 random = Random.insideUnitCircle * spawnRadius;
            Vector3 forward = Camera.main.transform.forward;
            forward.y = 0f;

            if (forward.sqrMagnitude <= Mathf.Epsilon)
                forward = Vector3.forward;

            Vector3 center = GameWorldGround.ProjectToGround(Camera.main.transform.position)
                             + forward.normalized * spawnRadius;

            return GameWorldGround.ProjectToGround(center + new Vector3(random.x, 0f, random.y));
        }

        if (planeManager == null)
            return GetCameraFallbackSpawnPosition();

        foreach (ARPlane plane in planeManager.trackables)
        {
            if (plane.alignment == PlaneAlignment.HorizontalUp)
            {
                Vector2 random = Random.insideUnitCircle * spawnRadius;

                return plane.transform.position +
                       new Vector3(random.x, 0f, random.y);
            }
        }

        return GetCameraFallbackSpawnPosition();
    }

    private Vector3 GetCameraFallbackSpawnPosition()
    {
        Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
        if (cameraTransform == null)
            return transform.position;

        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude <= Mathf.Epsilon)
            forward = Vector3.forward;

        Vector2 random = Random.insideUnitCircle * spawnRadius;
        Vector3 position = cameraTransform.position
                           + forward.normalized * spawnRadius
                           + new Vector3(random.x, 0f, random.y);
        position.y = cameraTransform.position.y - 1.2f;

        return position;
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
