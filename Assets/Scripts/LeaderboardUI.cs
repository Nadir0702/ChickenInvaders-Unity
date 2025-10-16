using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI component for displaying and managing the leaderboard
/// </summary>
public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject m_LeaderboardContent;
    [SerializeField] private Transform m_LeaderboardContainer;
    [SerializeField] private GameObject m_LeaderboardEntryPrefab;
    [SerializeField] private TextMeshProUGUI m_NoScoresText;
    
    [Header("New High Score UI")]
    [SerializeField] private GameObject m_NewHighScorePanel;
    [SerializeField] private TMP_InputField m_PlayerNameInput;
    [SerializeField] private Button m_SubmitScoreButton;
    [SerializeField] private TextMeshProUGUI m_NewScoreText;
    
    private List<GameObject> m_LeaderboardEntryObjects = new List<GameObject>();
    private int m_PendingScore = 0;
    
    private void Start()
    {
        // Initialize UI - both panels start hidden
        if (m_LeaderboardContent) m_LeaderboardContent.SetActive(false);
        if (m_NewHighScorePanel) m_NewHighScorePanel.SetActive(false);
        
        // Set up submit button
        if (m_SubmitScoreButton)
        {
            m_SubmitScoreButton.onClick.AddListener(SubmitScore);
        }
        
        // Set up input field
        if (m_PlayerNameInput)
        {
            m_PlayerNameInput.onEndEdit.AddListener(OnNameInputEndEdit);
            m_PlayerNameInput.characterLimit = 12; // Reasonable name length limit
        }
    }
    
    /// <summary>
    /// Show the leaderboard content (embedded in game over panel)
    /// </summary>
    public void ShowLeaderboard()
    {
        Debug.Log($"LeaderboardUI: ShowLeaderboard called - LeaderboardContent: {m_LeaderboardContent != null}, NewHighScorePanel: {m_NewHighScorePanel != null}");
        
        // Hide new high score panel and show leaderboard
        if (m_NewHighScorePanel) m_NewHighScorePanel.SetActive(false);
        if (m_LeaderboardContent)
        {
            m_LeaderboardContent.SetActive(true);
            RefreshLeaderboard();
        }
        else
        {
            Debug.LogError("LeaderboardUI: m_LeaderboardContent is null! Cannot show leaderboard.");
        }
    }
    
    /// <summary>
    /// Hide the leaderboard content and new high score panel
    /// </summary>
    public void HideLeaderboard()
    {
        if (m_LeaderboardContent)
        {
            m_LeaderboardContent.SetActive(false);
        }
        if (m_NewHighScorePanel)
        {
            m_NewHighScorePanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Check if player achieved a high score and show name input if needed
    /// </summary>
    public void CheckForHighScore(int i_Score)
    {
        Debug.Log($"LeaderboardUI: CheckForHighScore called with score {i_Score}");
        
        if (LeaderboardManager.Instance == null)
        {
            Debug.LogError("LeaderboardUI: LeaderboardManager.Instance is null!");
            return;
        }
        
        if (LeaderboardManager.Instance.IsHighScore(i_Score))
        {
            Debug.Log($"LeaderboardUI: Score {i_Score} is a high score, showing input panel");
            ShowNewHighScoreInput(i_Score);
        }
        else
        {
            Debug.Log($"LeaderboardUI: Score {i_Score} is not a high score, showing regular leaderboard");
            // Add the score to leaderboard (even if it doesn't qualify for top 5)
            LeaderboardManager.Instance.AddScore(i_Score);
            // Show the regular leaderboard
            ShowLeaderboard();
        }
    }
    
    /// <summary>
    /// Show the new high score input panel (embedded in game over panel)
    /// </summary>
    private void ShowNewHighScoreInput(int i_Score)
    {
        m_PendingScore = i_Score;
        
        // Hide leaderboard and show new high score input
        if (m_LeaderboardContent) m_LeaderboardContent.SetActive(false);
        if (m_NewHighScorePanel)
        {
            m_NewHighScorePanel.SetActive(true);
        }
        
        if (m_NewScoreText)
        {
            m_NewScoreText.text = $"{i_Score:N0}";
        }
        
        if (m_PlayerNameInput)
        {
            // Pre-fill with saved name if available
            string savedName = LeaderboardManager.Instance?.GetPlayerName() ?? "";
            m_PlayerNameInput.text = savedName;
            m_PlayerNameInput.Select();
            m_PlayerNameInput.ActivateInputField();
        }
    }
    
    /// <summary>
    /// Submit the high score with player name
    /// </summary>
    private void SubmitScore()
    {
        string playerName = m_PlayerNameInput?.text?.Trim() ?? "";
        
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "Anonymous";
        }
        
        // Save the name for future use
        LeaderboardManager.Instance?.SetPlayerName(playerName);
        
        // Add the score to leaderboard
        LeaderboardManager.Instance?.AddScore(m_PendingScore, playerName);
        
        // Hide new high score panel and show leaderboard
        if (m_NewHighScorePanel)
        {
            m_NewHighScorePanel.SetActive(false);
        }
        
        ShowLeaderboard();
        
        // Play UI sound
        AudioManager.Instance?.OnUIButtonClick();
    }
    
    /// <summary>
    /// Handle input field submit (Enter key)
    /// </summary>
    private void OnNameInputEndEdit(string i_Name)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SubmitScore();
        }
    }
    
    /// <summary>
    /// Refresh the leaderboard display
    /// </summary>
    public void RefreshLeaderboard()
    {
        // Clear existing entries
        ClearLeaderboardEntries();
        
        if (LeaderboardManager.Instance == null) return;
        
        var entries = LeaderboardManager.Instance.GetLeaderboard();
        
        if (entries.Count == 0)
        {
            // Show "no scores" message
            if (m_NoScoresText)
            {
                m_NoScoresText.gameObject.SetActive(true);
            }
            return;
        }
        
        // Hide "no scores" message
        if (m_NoScoresText)
        {
            m_NoScoresText.gameObject.SetActive(false);
        }
        
        // Create leaderboard entries
        for (int i = 0; i < entries.Count; i++)
        {
            CreateLeaderboardEntry(i + 1, entries[i]);
        }
    }
    
    /// <summary>
    /// Create a single leaderboard entry UI element
    /// </summary>
    private void CreateLeaderboardEntry(int i_Rank, LeaderboardManager.LeaderboardEntry i_Entry)
    {
        if (m_LeaderboardEntryPrefab == null || m_LeaderboardContainer == null) return;
        
        GameObject entryObject = Instantiate(m_LeaderboardEntryPrefab, m_LeaderboardContainer);
        m_LeaderboardEntryObjects.Add(entryObject);
        
        // Find and set text components
        var rankText = entryObject.transform.Find("RankText")?.GetComponent<TextMeshProUGUI>();
        var nameText = entryObject.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        var scoreText = entryObject.transform.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
        var dateText = entryObject.transform.Find("DateText")?.GetComponent<TextMeshProUGUI>();
        
        if (rankText) rankText.text = $"{i_Rank}.";
        if (nameText) nameText.text = i_Entry.playerName;
        if (scoreText) scoreText.text = i_Entry.score.ToString("N0");
        if (dateText) dateText.text = i_Entry.date;
        
        // Highlight top 3 positions with different colors
        Color entryColor = i_Rank switch
        {
            1 => new Color(1f, 0.84f, 0f), // Gold
            2 => new Color(0.75f, 0.75f, 0.75f), // Silver
            3 => new Color(0.8f, 0.5f, 0.2f), // Bronze
            _ => Color.white // Default
        };
        
        if (rankText) rankText.color = entryColor;
        if (nameText) nameText.color = entryColor;
        if (scoreText) scoreText.color = entryColor;
    }
    
    /// <summary>
    /// Clear all leaderboard entry objects
    /// </summary>
    private void ClearLeaderboardEntries()
    {
        foreach (var entry in m_LeaderboardEntryObjects)
        {
            if (entry != null)
            {
                Destroy(entry);
            }
        }
        m_LeaderboardEntryObjects.Clear();
    }
    
    /// <summary>
    /// Clear all leaderboard data (for testing/reset button)
    /// </summary>
    public void ClearAllScores()
    {
        LeaderboardManager.Instance?.ClearLeaderboard();
        RefreshLeaderboard();
        AudioManager.Instance?.OnUIButtonClick();
    }
}
