using System.Collections.Generic;
using System.Linq;
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
        SaveScoreEntry(score, enemies, time);
    }

    public List<string> GetLeaderboardEntries()
    {
        return GetSavedLeaderboardEntries();
    }

    public static void SaveScoreEntry(int score, int enemies, int time)
    {
        var entries = LoadEntries();
        entries.Add(new LeaderboardEntry(score, enemies, time));
        entries = entries
            .OrderByDescending(entry => entry.Score)
            .ThenByDescending(entry => entry.Enemies)
            .ThenByDescending(entry => entry.Time)
            .Take(MaxEntries)
            .ToList();

        PlayerPrefs.SetInt(CountKey, entries.Count);

        for (int i = 0; i < entries.Count; i++)
        {
            PlayerPrefs.SetInt(ScoreKey + i, entries[i].Score);
            PlayerPrefs.SetInt(EnemiesKey + i, entries[i].Enemies);
            PlayerPrefs.SetInt(TimeKey + i, entries[i].Time);
        }

        PlayerPrefs.Save();
    }

    public static List<string> GetSavedLeaderboardEntries()
    {
        int count = Mathf.Min(PlayerPrefs.GetInt(CountKey, 0), MaxEntries);
        var entries = new List<string>();
        for (int i = 0; i < count; i++)
        {
            int score = PlayerPrefs.GetInt(ScoreKey + i, 0);
            int enemies = PlayerPrefs.GetInt(EnemiesKey + i, 0);
            int time = PlayerPrefs.GetInt(TimeKey + i, 0);
            entries.Add($"Score: {score}  Enemies: {enemies}  Time: {time}s");
        }
        return entries;
    }

    static List<LeaderboardEntry> LoadEntries()
    {
        int count = Mathf.Min(PlayerPrefs.GetInt(CountKey, 0), MaxEntries);
        var entries = new List<LeaderboardEntry>();

        for (int i = 0; i < count; i++)
        {
            entries.Add(new LeaderboardEntry(
                PlayerPrefs.GetInt(ScoreKey + i, 0),
                PlayerPrefs.GetInt(EnemiesKey + i, 0),
                PlayerPrefs.GetInt(TimeKey + i, 0)
            ));
        }

        return entries;
    }

    struct LeaderboardEntry
    {
        public readonly int Score;
        public readonly int Enemies;
        public readonly int Time;

        public LeaderboardEntry(int score, int enemies, int time)
        {
            Score = score;
            Enemies = enemies;
            Time = time;
        }
    }
}
