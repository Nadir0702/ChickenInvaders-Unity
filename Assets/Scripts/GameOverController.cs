using UnityEngine;

/// <summary>
/// Handles game over screen menu interactions
/// </summary>
public class GameOverController : MonoBehaviour
{
    /// <summary>
    /// Restart the current game - called by "Restart" button
    /// </summary>
    public void Restart()
    {
        GameManager.Instance?.RestartGame();
    }
    
    /// <summary>
    /// Return to home screen - called by "Quit" button on game over screen
    /// </summary>
    public void QuitToMenu()
    {
        GameManager.Instance?.ReturnToMenu();
    }
}
