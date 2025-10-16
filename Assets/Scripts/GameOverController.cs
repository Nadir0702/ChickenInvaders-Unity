using TMPro;
using UnityEngine;

/// <summary>
/// Handles game over screen menu interactions
/// </summary>
public class GameOverController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI m_FinalScoreText;
    [SerializeField] private LeaderboardUI m_LeaderboardUI;
    
    private void OnEnable()
    {
        // Update final score display when game over panel is shown
        UpdateFinalScore();
        
        // Check for high score and show leaderboard
        CheckHighScore();
    }
    
    private void OnDisable()
    {
        // Hide leaderboard content when game over panel is hidden
        if (m_LeaderboardUI)
        {
            m_LeaderboardUI.HideLeaderboard();
        }
    }
    
    /// <summary>
    /// Update the final score display
    /// </summary>
    private void UpdateFinalScore()
    {
        if (m_FinalScoreText && GameManager.Instance != null)
        {
            m_FinalScoreText.text = $"Final Score: {GameManager.Instance.CurrentScore:N0}";
        }
    }
    
    /// <summary>
    /// Check if player achieved a high score
    /// </summary>
    private void CheckHighScore()
    {
        if (m_LeaderboardUI && GameManager.Instance != null)
        {
            m_LeaderboardUI.CheckForHighScore(GameManager.Instance.CurrentScore);
        }
        else
        {
            Debug.LogWarning($"GameOverController: Cannot check high score - LeaderboardUI: {m_LeaderboardUI != null}, GameManager: {GameManager.Instance != null}");
        }
    }
    
    
    /// <summary>
    /// Restart the current game - called by "Restart" button
    /// </summary>
    public void Restart()
    {
        GameManager.Instance?.RestartGame();
        AudioManager.Instance?.OnUIButtonClick();
    }
    
    /// <summary>
    /// Return to home screen - called by "Quit" button on game over screen
    /// </summary>
    public void QuitToMenu()
    {
        GameManager.Instance?.ReturnToMenu();
        AudioManager.Instance?.OnUIButtonClick();
    }
}
