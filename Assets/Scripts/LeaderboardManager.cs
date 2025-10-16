using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages persistent leaderboard using PlayerPrefs
/// </summary>
public class LeaderboardManager : Singleton<LeaderboardManager>
{
    [System.Serializable]
    public class LeaderboardEntry
    {
        public string playerName;
        public int score;
        public string date;
        
        public LeaderboardEntry(string name, int playerScore, string entryDate)
        {
            playerName = name;
            score = playerScore;
            date = entryDate;
        }
    }
    
    private const int k_MaxLeaderboardEntries = 5;
    private const string k_LeaderboardKey = "ChickenInvaders_Leaderboard";
    private const string k_PlayerNameKey = "ChickenInvaders_PlayerName";
    
    private List<LeaderboardEntry> m_LeaderboardEntries = new List<LeaderboardEntry>();
    
    protected override void Awake()
    {
        base.Awake();
        if (this == Instance)
        {
            LoadLeaderboard();
        }
    }
    
    /// <summary>
    /// Add a new score to the leaderboard
    /// </summary>
    /// <param name="i_Score">The score to add</param>
    /// <param name="i_PlayerName">Optional player name (will use saved name or "Anonymous")</param>
    /// <returns>True if the score made it to the leaderboard</returns>
    public bool AddScore(int i_Score, string i_PlayerName = null)
    {
        // Use provided name, or fall back to saved name, or default to "Anonymous"
        string playerName = i_PlayerName ?? GetPlayerName();
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "Anonymous";
        }
        
        // Check if this score qualifies BEFORE adding it
        bool qualifiesForLeaderboard = IsHighScore(i_Score);
        
        // Create new entry
        LeaderboardEntry newEntry = new LeaderboardEntry(
            playerName,
            i_Score,
            DateTime.Now.ToString("MM/dd/yyyy")
        );
        
        // Add to list
        m_LeaderboardEntries.Add(newEntry);
        
        // Sort by score (highest first)
        m_LeaderboardEntries.Sort((a, b) => b.score.CompareTo(a.score));
        
        // Keep only top entries
        if (m_LeaderboardEntries.Count > k_MaxLeaderboardEntries)
        {
            m_LeaderboardEntries.RemoveRange(k_MaxLeaderboardEntries, 
                m_LeaderboardEntries.Count - k_MaxLeaderboardEntries);
        }
        
        // Save to PlayerPrefs
        SaveLeaderboard();
        
        // Return whether this score qualified for the leaderboard
        return qualifiesForLeaderboard;
    }
    
    /// <summary>
    /// Get the current leaderboard entries
    /// </summary>
    public List<LeaderboardEntry> GetLeaderboard()
    {
        return new List<LeaderboardEntry>(m_LeaderboardEntries); // Return copy to prevent external modification
    }
    
    /// <summary>
    /// Check if a score qualifies for the leaderboard
    /// </summary>
    public bool IsHighScore(int i_Score)
    {
        if (m_LeaderboardEntries.Count < k_MaxLeaderboardEntries)
        {
            return true; // Leaderboard not full, any score qualifies
        }
        
        // Check if score is higher than the lowest entry
        return i_Score > m_LeaderboardEntries[m_LeaderboardEntries.Count - 1].score;
    }
    
    /// <summary>
    /// Get the player's saved name
    /// </summary>
    public string GetPlayerName()
    {
        return PlayerPrefs.GetString(k_PlayerNameKey, "");
    }
    
    /// <summary>
    /// Save the player's name
    /// </summary>
    public void SetPlayerName(string i_PlayerName)
    {
        PlayerPrefs.SetString(k_PlayerNameKey, i_PlayerName);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Clear all leaderboard data (for testing or reset)
    /// </summary>
    public void ClearLeaderboard()
    {
        m_LeaderboardEntries.Clear();
        PlayerPrefs.DeleteKey(k_LeaderboardKey);
        PlayerPrefs.Save();
    }
    
    private void LoadLeaderboard()
    {
        string leaderboardData = PlayerPrefs.GetString(k_LeaderboardKey, "");
        
        if (string.IsNullOrEmpty(leaderboardData))
        {
            // Initialize with default entries if no data exists
            InitializeDefaultLeaderboard();
            return;
        }
        
        try
        {
            // Parse JSON data
            LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(leaderboardData);
            m_LeaderboardEntries = new List<LeaderboardEntry>(data.entries);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to load leaderboard data: {e.Message}. Initializing with defaults.");
            InitializeDefaultLeaderboard();
        }
    }
    
    private void SaveLeaderboard()
    {
        try
        {
            LeaderboardData data = new LeaderboardData { entries = m_LeaderboardEntries.ToArray() };
            string jsonData = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(k_LeaderboardKey, jsonData);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save leaderboard data: {e.Message}");
        }
    }
    
    private void InitializeDefaultLeaderboard()
    {
        m_LeaderboardEntries.Clear();
        
        // Add some default scores to make the leaderboard look populated
        m_LeaderboardEntries.Add(new LeaderboardEntry("ACE PILOT", 50000, DateTime.Now.AddDays(-10).ToString("MM/dd/yyyy")));
        m_LeaderboardEntries.Add(new LeaderboardEntry("SKY MASTER", 35000, DateTime.Now.AddDays(-8).ToString("MM/dd/yyyy")));
        m_LeaderboardEntries.Add(new LeaderboardEntry("WING COMMANDER", 25000, DateTime.Now.AddDays(-5).ToString("MM/dd/yyyy")));
        m_LeaderboardEntries.Add(new LeaderboardEntry("ROOKIE", 15000, DateTime.Now.AddDays(-3).ToString("MM/dd/yyyy")));
        m_LeaderboardEntries.Add(new LeaderboardEntry("CADET", 10000, DateTime.Now.AddDays(-1).ToString("MM/dd/yyyy")));
        
        SaveLeaderboard();
    }
    
    /// <summary>
    /// Helper class for JSON serialization
    /// </summary>
    [System.Serializable]
    private class LeaderboardData
    {
        public LeaderboardEntry[] entries;
    }
}
