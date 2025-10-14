using UnityEngine;

/// <summary>
/// Handles home screen menu interactions
/// </summary>
public class HomeScreenController : MonoBehaviour
{
    /// <summary>
    /// Start a new game - called by "Start Game" button
    /// </summary>
    public void StartGame()
    {
        GameManager.Instance?.StartNewGame();
    }
    
    /// <summary>
    /// Quit the application entirely - called by "Quit" button on home screen
    /// </summary>
    public void QuitApplication()
    {
        GameManager.Instance?.QuitApplication();
    }

    public void OnControlsClick()
    {
        UIManager.Instance?.ShowControlsPanel();
    }
}
