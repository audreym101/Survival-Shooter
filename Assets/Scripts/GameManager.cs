using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] public float gameDuration = 60f;

    public float TimeRemaining { get; private set; }
    public bool IsPlaying { get; private set; }

    public UnityEvent onGameStart;
    public UnityEvent onGameEnd;
    public UnityEvent<float> onTimeChanged;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartGame()
    {
        TimeRemaining = gameDuration;
        IsPlaying = true;
        ScoreManager.Instance?.ResetScore();
        onGameStart?.Invoke();
    }

    public void EndGame()
    {
        IsPlaying = false;
        onGameEnd?.Invoke();
        AudioManager.Instance?.PlayPlayerDeath();
    }

    void Update()
    {
        if (!IsPlaying) return;

        TimeRemaining -= Time.deltaTime;
        onTimeChanged?.Invoke(TimeRemaining);

        if (TimeRemaining <= 0)
        {
            TimeRemaining = 0;
            EndGame();
        }
    }
}
