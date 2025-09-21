using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public static event Action<eGameState> OnGameStateChanged;
    
    [SerializeField] private int m_StartingLives = 3;
    public int Lives { get; private set; }
    public int Score { get; private set; }
    public eGameState GameState { get; private set; } = eGameState.Playing;

    private void Awake()
    {
        Lives = m_StartingLives;
        Time.timeScale = 1f;
    }
    
    public void AddScore(int i_Amount)
    {
        Score += i_Amount;
        UIManager.Instance?.SetScore(Score);
    }

    public void DamagePlayer(int i_Amount = 1)
    {
        if (GameState != eGameState.Playing) return;
        
        Lives = Mathf.Max(0, Lives - i_Amount);
        UIManager.Instance?.SetLives(Lives);
        
        if (Lives <= 0) SetGameState(eGameState.GameOver);
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
