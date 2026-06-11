using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    public enum Difficulty { Easy, Hard }
    public Difficulty CurrentDifficulty { get; private set; } = Difficulty.Easy;

    // Easy settings
    const float EasySpawnInterval = 7f;
    const float EasyEnemySpeed = 1.5f;
    const int EasyEnemyHealth = 50;
    const float EasyGameDuration = 90f;

    // Hard settings
    const float HardSpawnInterval = 3f;
    const float HardEnemySpeed = 3f;
    const int HardEnemyHealth = 100;
    const float HardGameDuration = 60f;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetEasy()
    {
        CurrentDifficulty = Difficulty.Easy;
        PlayerPrefs.SetInt("Difficulty", 0);
        PlayerPrefs.Save();
    }

    public void SetHard()
    {
        CurrentDifficulty = Difficulty.Hard;
        PlayerPrefs.SetInt("Difficulty", 1);
        PlayerPrefs.Save();
    }

    public float SpawnInterval => CurrentDifficulty == Difficulty.Easy ? EasySpawnInterval : HardSpawnInterval;
    public float EnemySpeed => CurrentDifficulty == Difficulty.Easy ? EasyEnemySpeed : HardEnemySpeed;
    public int EnemyHealth => CurrentDifficulty == Difficulty.Easy ? EasyEnemyHealth : HardEnemyHealth;
    public float GameDuration => CurrentDifficulty == Difficulty.Easy ? EasyGameDuration : HardGameDuration;
}
