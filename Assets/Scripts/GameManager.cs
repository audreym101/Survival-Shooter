using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] public float gameDuration = 60f;

    public float TimeRemaining { get; private set; }
    public bool IsPlaying { get; private set; }

    [Header("Spawner")]
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Events")]
    public UnityEvent onGameStart;
    public UnityEvent onGameEnd;
    public UnityEvent<float> onTimeChanged;

    PlayerHealth _playerHealth;

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

        CachePlayerHealth();
    }

    private void Update()
    {
        if (!IsPlaying)
            return;

        TimeRemaining = Mathf.Max(0f, TimeRemaining - Time.deltaTime);

        onTimeChanged?.Invoke(TimeRemaining);

        if (TimeRemaining <= 0f)
        {
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

        Debug.Log("GAME STARTED");

        TimeRemaining = gameDuration;
        IsPlaying = true;

        CachePlayerHealth();
        _playerHealth?.ResetHealth();

        ScoreManager.Instance?.ResetScore();

        onGameStart?.Invoke();
        onTimeChanged?.Invoke(TimeRemaining);

        if (enemySpawner != null)
        {
            enemySpawner.StartSpawning();
        }
        else
        {
            Debug.LogWarning("EnemySpawner not assigned in GameManager!");
        }
    }

    // ===================================
    // END GAME
    // ===================================

    public void EndGame()
    {
        if (!IsPlaying)
            return;

        IsPlaying = false;

        enemySpawner?.StopSpawning();

        onGameEnd?.Invoke();

        AudioManager.Instance?.PlayPlayerDeath();

        Debug.Log("Game Ended");
    }

    // ===================================
    // RESTART
    // ===================================

    public void RestartGame()
    {
        if (IsPlaying)
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

    void CachePlayerHealth()
    {
        PlayerHealth foundHealth = Object.FindFirstObjectByType<PlayerHealth>();
        if (foundHealth == null || foundHealth == _playerHealth)
            return;

        if (_playerHealth != null)
            _playerHealth.onPlayerDeath.RemoveListener(EndGame);

        _playerHealth = foundHealth;
        _playerHealth.onPlayerDeath.RemoveListener(EndGame);
        _playerHealth.onPlayerDeath.AddListener(EndGame);
    }
}
