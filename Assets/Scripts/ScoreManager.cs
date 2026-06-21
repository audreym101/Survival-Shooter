using UnityEngine;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int Score { get; private set; }
    public int EnemiesDefeated { get; private set; }

    public UnityEvent<int> onScoreChanged;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddScore(int points)
    {
        Score += points;
        EnemiesDefeated++;
        onScoreChanged?.Invoke(Score);
    }

    public void ResetScore()
    {
        Score = 0;
        EnemiesDefeated = 0;
        onScoreChanged?.Invoke(Score);
    }
}
