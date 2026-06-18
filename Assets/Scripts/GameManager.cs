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

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        IsPlaying = false;
        TimeRemaining = gameDuration;

        // SAFE PLAYER DEATH HOOK
        PlayerHealth playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.onPlayerDeath.AddListener(EndGame);
    }

    void Update()
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

    // ─────────────────────────────
    // GAME FLOW
    // ─────────────────────────────

    public void StartGame()
    {
        if (DifficultyManager.Instance != null)
            gameDuration = DifficultyManager.Instance.GameDuration;

        TimeRemaining = gameDuration;
        IsPlaying = true;

        ScoreManager.Instance?.ResetScore();

        onGameStart?.Invoke();

        Debug.Log("Game Started");
    }

    public void EndGame()
    {
        if (!IsPlaying)
            return;

        IsPlaying = false;

        onGameEnd?.Invoke();

        AudioManager.Instance?.PlayPlayerDeath();

        Debug.Log("Game Ended");
    }

    public void RestartGame()
    {
        StartGame();
    }

    public void ResetToMenuState()
    {
        IsPlaying = false;
        TimeRemaining = gameDuration;
    }
}