using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject leaderboardPanel;

    [Header("Leaderboard")]
    [SerializeField] TMP_Text leaderboardText;

    void Start() => ShowMain();

    void ShowMain()
    {
        mainPanel.SetActive(true);
        leaderboardPanel.SetActive(false);
    }

    public void OnPlayPressed() => SceneManager.LoadScene("GameScene");

    public void OnLeaderboardPressed()
    {
        mainPanel.SetActive(false);
        leaderboardPanel.SetActive(true);
        LoadLeaderboard();
    }

    public void OnCloseLeaderboardPressed() => ShowMain();

    public void OnQuitPressed()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void LoadLeaderboard()
    {
        var entries = LeaderboardManager.GetSavedLeaderboardEntries();

        if (entries == null || entries.Count == 0)
        {
            leaderboardText.text = "No scores yet";
            return;
        }

        leaderboardText.text = "";

        for (int i = 0; i < entries.Count; i++)
        {
            leaderboardText.text += $"{i + 1}. {entries[i]}\n";
        }
    }
}
