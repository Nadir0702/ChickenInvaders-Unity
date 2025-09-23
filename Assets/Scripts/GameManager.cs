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
    
    // Automatic pickup thresholds
    private int m_LastBombScoreThreshold;
    private int m_LastLifeScoreThreshold;
    private const int BOMB_SCORE_INTERVAL = 10000;
    private const int LIFE_SCORE_INTERVAL = 50000;

    private void Awake()
    {
        m_Lives = m_StartingLives;
        Time.timeScale = 1f;
    }
    
    public void AddScore(int i_Amount)
    {
        m_Score += i_Amount;
        UIManager.Instance?.SetScore(m_Score);
        
        // Check for automatic bomb grants every 10000 points
        if (m_Score >= m_LastBombScoreThreshold + BOMB_SCORE_INTERVAL)
        {
            m_LastBombScoreThreshold = (m_Score / BOMB_SCORE_INTERVAL) * BOMB_SCORE_INTERVAL;
            var playerStats = FindFirstObjectByType<PlayerStats>();
            if (playerStats) playerStats.AddBomb(1);
        }
        
        // Check for automatic life grants every 50000 points
        if (m_Score >= m_LastLifeScoreThreshold + LIFE_SCORE_INTERVAL)
        {
            m_LastLifeScoreThreshold = (m_Score / LIFE_SCORE_INTERVAL) * LIFE_SCORE_INTERVAL;
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
        else
        {
            // Automatically activate shield on death (not game over)
            var playerStats = FindFirstObjectByType<PlayerStats>();
            if (playerStats) playerStats.ActivateShield(4f);
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
