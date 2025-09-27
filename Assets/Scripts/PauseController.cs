using UnityEngine;

/// <summary>
/// Handles pause panel button events - input handling moved to GameManager
/// </summary>
public class PauseController : MonoBehaviour
{
    public void Resume() => GameManager.Instance?.ResumeGame();

    public void QuitToMenu() => GameManager.Instance?.ReturnToMenu();

    public void Restart() => GameManager.Instance?.RestartGame();
}
