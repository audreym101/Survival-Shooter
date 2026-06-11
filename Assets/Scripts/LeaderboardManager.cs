using System.Collections.Generic;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    const int MaxEntries = 5;
    const string ScoreKey = "Score_";
    const string EnemiesKey = "Enemies_";
    const string TimeKey = "Time_";
    const string CountKey = "EntryCount";

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SaveScore(int score, int enemies, int time)
    {
        int count = Mathf.Min(PlayerPrefs.GetInt(CountKey, 0), MaxEntries - 1);
        PlayerPrefs.SetInt(CountKey, count + 1);
        PlayerPrefs.SetInt(ScoreKey + count, score);
        PlayerPrefs.SetInt(EnemiesKey + count, enemies);
        PlayerPrefs.SetInt(TimeKey + count, time);
        PlayerPrefs.Save();
    }

    public List<string> GetLeaderboardEntries()
    {
        int count = Mathf.Min(PlayerPrefs.GetInt(CountKey, 0), MaxEntries);
        var entries = new List<string>();
        for (int i = count - 1; i >= 0; i--)
        {
            int score = PlayerPrefs.GetInt(ScoreKey + i, 0);
            int enemies = PlayerPrefs.GetInt(EnemiesKey + i, 0);
            int time = PlayerPrefs.GetInt(TimeKey + i, 0);
            entries.Add($"Score: {score}  Enemies: {enemies}  Time: {time}s");
        }
        return entries;
    }
}
