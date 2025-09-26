using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public static event Action<eGameState> OnGameStateChanged;
    
    [SerializeField] private int m_StartingLives = 3;
    private int m_Lives;
    private int m_Score;
    public eGameState GameState { get; private set; } = eGameState.Playing;
    
    // Enemy scaling system
    public int CurrentDifficultyTier { get; private set; } = 1; // Tier 1 = waves 1-5, Tier 2 = waves 6-10, etc.
    
    // Automatic pickup thresholds
    private int m_LastBombScoreThreshold;
    private int m_LastLifeScoreThreshold;
    private const int k_BombScoreInterval = 10000;
    private const int k_LifeScoreInterval = 50000;

    private void Awake()
    {
        m_Lives = m_StartingLives;
        Time.timeScale = 1f;
        CurrentDifficultyTier = 1; // Reset difficulty on game start
    }
    
    public void SetDifficultyTier(int i_WaveNumber)
    {
        int newTier = Mathf.Max(1, (i_WaveNumber - 1) / 5 + 1);
        if (newTier != CurrentDifficultyTier)
        {
            CurrentDifficultyTier = newTier;
        }
    }
    
    public void AddScore(int i_Amount)
    {
        m_Score += i_Amount;
        UIManager.Instance?.SetScore(m_Score);
        
        // Check for automatic bomb grants every 10000 points
        if (m_Score >= m_LastBombScoreThreshold + k_BombScoreInterval)
        {
            m_LastBombScoreThreshold = (m_Score / k_BombScoreInterval) * k_BombScoreInterval;
            var playerStats = FindFirstObjectByType<PlayerStats>();
            if (playerStats) playerStats.AddBomb(1);
        }
        
        // Check for automatic life grants every 50000 points
        if (m_Score >= m_LastLifeScoreThreshold + k_LifeScoreInterval)
        {
            m_LastLifeScoreThreshold = (m_Score / k_LifeScoreInterval) * k_LifeScoreInterval;
            addLife(1);
        }
    }
    
    private void addLife(int i_Amount)
    {
        m_Lives += i_Amount;
        UIManager.Instance?.SetLives(m_Lives);
    }

    public void DamagePlayer(int i_Amount = 1)
    {
        if (GameState != eGameState.Playing) return;
        
        m_Lives = Mathf.Max(0, m_Lives - i_Amount);
        UIManager.Instance?.SetLives(m_Lives);
        
        if (m_Lives <= 0) 
        {
            AudioManager.Instance?.Play(eSFXId.GameOver);
            SetGameState(eGameState.GameOver);
        }
    }

    public void SetGameState(eGameState i_GameState)
    {
        if (GameState == i_GameState) return;
        
        GameState = i_GameState;
        
        if (GameState == eGameState.Paused) Time.timeScale = 0f;
        else Time.timeScale = 1f;
        
        OnGameStateChanged?.Invoke(GameState);
        UIManager.Instance?.ShowState(GameState);
    }

    public void RestartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        
#endif
        Application.Quit();

    }
    
    public void ResumeGame() => SetGameState(eGameState.Playing);
}
