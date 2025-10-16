using System;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public static event Action<eGameState> OnGameStateChanged;
    
    [SerializeField] private int m_StartingLives = 3;
    private int m_Lives;
    private int m_Score;
    
    // Public properties for external access
    public int CurrentScore => m_Score;
    public int CurrentLives => m_Lives;
    public eGameState GameState { get; private set; } = eGameState.Menu;
    
    // Enemy scaling system
    public int CurrentDifficultyTier { get; private set; } = 1; // Tier 1 = waves 1-5, Tier 2 = waves 6-10, etc.
    
    // Automatic pickup thresholds
    private int m_LastBombScoreThreshold;
    private int m_LastLifeScoreThreshold;
    private const int k_BombScoreInterval = 10000;
    private const int k_LifeScoreInterval = 50000;

    protected override void Awake()
    {
        base.Awake(); // Call singleton Awake first
        if (this == Instance) // Only initialize if we're the active instance
        {
            initializeGame();
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameState == eGameState.Playing)
            {
                setGameState(eGameState.Paused);
            }
            else if (GameState == eGameState.Paused)
            {
                setGameState(eGameState.Playing);
            }
        }
    }
    
   
    private void initializeGame()
    {
        m_Lives = m_StartingLives;
        m_Score = 0;
        m_LastBombScoreThreshold = 0;
        m_LastLifeScoreThreshold = 0;
        CurrentDifficultyTier = 1;
        Time.timeScale = 1f;
        
        // Reset all object pools
        PoolManager.Instance?.ResetAllPools();
        
        // Clear all active pickups from previous game
        clearActivePickups();
        
        // Reset player stats
        var playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats) playerStats.ResetStats();
        
        // Initialize player as hidden (will spawn when game starts)
        var playerRespawn = FindFirstObjectByType<PlayerRespawn>();
        if (playerRespawn) playerRespawn.InitializePlayerAsHidden();
        
        // Update UI to reflect reset values
        UIManager.Instance?.SetScore(m_Score);
        UIManager.Instance?.SetLives(m_Lives);
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
            setGameState(eGameState.GameOver);
        }
    }

    private void setGameState(eGameState i_GameState)
    {
        if (GameState == i_GameState) return;
        
        GameState = i_GameState;
        
        // Handle time scale for different states
        if (GameState == eGameState.Paused || GameState == eGameState.Menu) 
            Time.timeScale = 0f;
        else 
            Time.timeScale = 1f;
        
        OnGameStateChanged?.Invoke(GameState);
    }

    /// <summary>
    /// Start a new game from the home screen
    /// </summary>
    public void StartNewGame()
    {
        // Reset wave director to start from wave 1
        WaveDirector.Instance?.ResetWaves();
        
        initializeGame();
        setGameState(eGameState.Playing);
        
        // Spawn player from bottom of screen
        var playerRespawn = FindFirstObjectByType<PlayerRespawn>();
        if (playerRespawn) 
        {
            playerRespawn.SpawnPlayerAtGameStart();
        }
        else
        {
            Debug.LogWarning("GameManager: PlayerRespawn component not found in StartNewGame!");
        }
    }
    
    /// <summary>
    /// Return to home screen (from pause or game over)
    /// </summary>
    public void ReturnToMenu()
    {
        initializeGame();
        setGameState(eGameState.Menu);
    }
    
    /// <summary>
    /// Resume game from pause
    /// </summary>
    public void ResumeGame()
    {
        setGameState(eGameState.Playing);
    }
    
    /// <summary>
    /// Restart current game (unified restart without scene reload)
    /// </summary>
    public void RestartGame()
    {
        // Reset wave director to start from wave 1
        WaveDirector.Instance?.ResetWaves();
        
        // Restart music from beginning
        AudioManager.Instance?.OnGameRestart();
        
        initializeGame();
        setGameState(eGameState.Playing);
        
        // Spawn player from bottom of screen
        var playerRespawn = FindFirstObjectByType<PlayerRespawn>();
        if (playerRespawn) 
        {
            playerRespawn.SpawnPlayerAtGameStart();
        }
        else
        {
            Debug.LogWarning("GameManager: PlayerRespawn component not found in RestartGame!");
        }
    }

    /// <summary>
    /// Quit application entirely (only from home screen)
    /// </summary>
    public void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    /// <summary>
    /// Clear weapon power-ups from the scene when starting a new game
    /// (Food and enemy bullets are handled by PoolManager.ResetAllPools())
    /// </summary>
    private void clearActivePickups()
    {
        // Clear weapon power-ups (these are instantiated, not pooled)
        var powerUps = FindObjectsByType<PowerUpPickup>(FindObjectsSortMode.None);
        foreach (var powerUp in powerUps)
        {
            if (powerUp && powerUp.gameObject.activeInHierarchy)
            {
                Destroy(powerUp.gameObject);
            }
        }
    }
}
