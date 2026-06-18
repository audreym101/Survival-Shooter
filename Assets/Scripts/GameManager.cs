using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] public float gameDuration = 60f;

    public float TimeRemaining { get; private set; }
    public bool IsPlaying { get; private set; }

    [Header("Events")]
    public UnityEvent onGameStart;
    public UnityEvent onGameEnd;
    public UnityEvent<float> onTimeChanged;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        IsPlaying = false;
        TimeRemaining = gameDuration;

        PlayerHealth playerHealth = Object.FindFirstObjectByType<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.onPlayerDeath.AddListener(EndGame);
        }
    }

    private void Update()
    {
        if (!IsPlaying)
            return;

        TimeRemaining -= Time.deltaTime;

        onTimeChanged?.Invoke(TimeRemaining);

        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            EndGame();
        }
    }

    // ===================================
    // START GAME
    // ===================================

    public void StartGame()
    {
        if (DifficultyManager.Instance != null)
        {
            gameDuration = DifficultyManager.Instance.GameDuration;
        }

        TimeRemaining = gameDuration;
        IsPlaying = true;

        ScoreManager.Instance?.ResetScore();

        EnemySpawner spawner =
            Object.FindFirstObjectByType<EnemySpawner>();

        if (spawner != null)
        {
            spawner.StartSpawning();
            Debug.Log("Enemy Spawner Started");
        }
        else
        {
            Debug.LogWarning("EnemySpawner not found in scene!");
        }

        onGameStart?.Invoke();

        Debug.Log("Game Started");
    }

    // ===================================
    // END GAME
    // ===================================

    public void EndGame()
    {
        if (!IsPlaying)
            return;

        IsPlaying = false;

        EnemySpawner spawner =
            Object.FindFirstObjectByType<EnemySpawner>();

        if (spawner != null)
        {
            spawner.StopSpawning();
            Debug.Log("Enemy Spawner Stopped");
        }

        onGameEnd?.Invoke();

        AudioManager.Instance?.PlayPlayerDeath();

        Debug.Log("Game Ended");
    }

    // ===================================
    // RESTART
    // ===================================

    public void RestartGame()
    {
        EndGame();
        StartGame();
    }

    // ===================================
    // RESET TO MENU
    // ===================================

    public void ResetToMenuState()
    {
        IsPlaying = false;
        TimeRemaining = gameDuration;
    }
}