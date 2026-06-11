using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

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
    [SerializeField] TMP_Text enemiesDefeatedText;
    [SerializeField] TMP_Text timeSurvivedText;

    float _timeSurvived;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        inGamePanel.SetActive(false);
        endGamePanel.SetActive(false);
        GameManager.Instance.onGameStart.AddListener(ShowInGame);
        GameManager.Instance.onGameEnd.AddListener(ShowEndGame);
        GameManager.Instance.onTimeChanged.AddListener(UpdateTimer);

        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.onHealthChanged.AddListener(UpdateHealth);

        ScoreManager.Instance.onScoreChanged.AddListener(UpdateScore);
    }

    // ── Panel Control ──────────────────────────────────────

    void ShowInGame()
    {
        inGamePanel.SetActive(true);
        endGamePanel.SetActive(false);
    }

    void ShowEndGame()
    {
        _timeSurvived = GameManager.Instance.gameDuration - GameManager.Instance.TimeRemaining;
        inGamePanel.SetActive(false);
        endGamePanel.SetActive(true);

        finalScoreText.text = $"Score: {ScoreManager.Instance.Score}";
        enemiesDefeatedText.text = $"Enemies: {ScoreManager.Instance.EnemiesDefeated}";
        timeSurvivedText.text = $"Time: {Mathf.FloorToInt(_timeSurvived)}s";

        LeaderboardManager.Instance?.SaveScore(
            ScoreManager.Instance.Score,
            ScoreManager.Instance.EnemiesDefeated,
            Mathf.FloorToInt(_timeSurvived));
    }

    // ── HUD Updates ────────────────────────────────────────

    void UpdateHealth(int health) => healthText.text = $"HP: {health}";
    void UpdateScore(int score) => scoreText.text = $"Score: {score}";
    void UpdateTimer(float time) => timerText.text = $"Time: {Mathf.CeilToInt(time)}";

    // ── Button Callbacks ───────────────────────────────────

    public void OnRestartPressed() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    public void OnMainMenuPressed() => SceneManager.LoadScene("MainMenuScene");
}
