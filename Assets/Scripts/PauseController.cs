using UnityEngine;

public class PauseController : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(GameManager.Instance.GameState == eGameState.Playing)
            {
                GameManager.Instance.SetGameState(eGameState.Paused);
            }
            else if(GameManager.Instance.GameState == eGameState.Paused)
            {
                GameManager.Instance.SetGameState(eGameState.Playing);
            }
        }
    }
    
    public void Resume() => GameManager.Instance.ResumeGame();

    public void Quit() => GameManager.Instance.QuitGame();
    
    public void Restart() => GameManager.Instance.RestartGame();
}
