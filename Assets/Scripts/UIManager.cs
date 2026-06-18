using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] GameObject inGamePanel;
    [SerializeField] GameObject endGamePanel;

    [Header("InGame UI")]
    [SerializeField] TMP_Text healthText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text timerText;

    [Header("EndGame UI")]
    [SerializeField] TMP_Text finalScoreText;
    [SerializeField] TMP_Text enemiesText;
    [SerializeField] TMP_Text timeSurvivedText;

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
        Debug.Log("UIManager initialized in GameScene");

        ShowInGame(); // default state (NO start menu anymore)

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager not found!");
            return;
        }

        GameManager.Instance.onGameStart.AddListener(ShowInGame);
        GameManager.Instance.onGameEnd.AddListener(ShowEndGame);
        GameManager.Instance.onTimeChanged.AddListener(UpdateTimer);

        PlayerHealth player = Object.FindFirstObjectByType<PlayerHealth>();
        if (player != null)
            player.onHealthChanged.AddListener(UpdateHealth);

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.onScoreChanged.AddListener(UpdateScore);
    }

    // ─────────────────────────────
    // PANEL CONTROL
    // ─────────────────────────────

    void ShowInGame()
    {
        inGamePanel.SetActive(true);
        endGamePanel.SetActive(false);
    }

    void ShowEndGame()
    {
        inGamePanel.SetActive(false);
        endGamePanel.SetActive(true);

        if (ScoreManager.Instance == null || GameManager.Instance == null)
            return;

        float timeSurvived = GameManager.Instance.gameDuration - GameManager.Instance.TimeRemaining;

        finalScoreText.text = "Score: " + ScoreManager.Instance.Score;
        enemiesText.text = "Enemies: " + ScoreManager.Instance.EnemiesDefeated;
        timeSurvivedText.text = "Time: " + Mathf.FloorToInt(timeSurvived) + "s";

        LeaderboardManager.Instance?.SaveScore(
            ScoreManager.Instance.Score,
            ScoreManager.Instance.EnemiesDefeated,
            Mathf.FloorToInt(timeSurvived)
        );
    }

    // ─────────────────────────────
    // HUD UPDATES
    // ─────────────────────────────

    void UpdateHealth(int health)
    {
        if (healthText != null)
            healthText.text = "HP: " + health;
    }

    void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    void UpdateTimer(float time)
    {
        if (timerText != null)
            timerText.text = "Time: " + Mathf.CeilToInt(time);
    }

    // ─────────────────────────────
    // BUTTON FUNCTIONS
    // ─────────────────────────────

    public void OnStartPressed()
    {
        Debug.Log("Start button pressed");
        GameManager.Instance.StartGame();
    }

    public void OnRestartPressed()
    {
        Debug.Log("Restart pressed");
        GameManager.Instance.StartGame();
    }

    public void OnMainMenuPressed()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
    }
}