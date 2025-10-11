using TMPro;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private TextMeshProUGUI m_ScoreText;
    [SerializeField] private TextMeshProUGUI m_LivesText;
    [SerializeField] private TextMeshProUGUI m_WeaponLevelText;
    [SerializeField] private TextMeshProUGUI m_BombsAmountText;
    [SerializeField] private GameObject m_GameOverPanel;
    [SerializeField] private GameObject m_PausePanel;
    [SerializeField] private GameObject m_HomeScreenPanel;

    private void Start()
    {
        // Initialize all panels as inactive
        m_GameOverPanel?.SetActive(false);
        m_PausePanel?.SetActive(false);
        m_HomeScreenPanel?.SetActive(false);
        
        // Ensure we don't double-subscribe if this Start() is called multiple times
        GameManager.OnGameStateChanged -= showState; // Unsubscribe first (safe even if not subscribed)
        GameManager.OnGameStateChanged += showState; // Subscribe
        
        // Show initial state (Menu)
        showState(GameManager.Instance?.GameState ?? eGameState.Menu);
    }
    
    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= showState;
    }
    
    public void SetScore(int i_Value)
    {
        if (m_ScoreText) m_ScoreText.text = $"Score: {i_Value}";
    }
    
    public void SetLives(int i_Value)
    {
        if (m_LivesText) m_LivesText.text = $"Lives: {i_Value}";
    }       

    private void showState(eGameState i_GameState)
    {
        // Hide all panels first
        m_GameOverPanel?.SetActive(false);
        m_PausePanel?.SetActive(false);
        m_HomeScreenPanel?.SetActive(false);
        
        // Show appropriate panel for current state
        switch (i_GameState)
        {
            case eGameState.Menu:
                m_HomeScreenPanel?.SetActive(true);
                break;
            case eGameState.Paused:
                m_PausePanel?.SetActive(true);
                break;
            case eGameState.GameOver:
                m_GameOverPanel?.SetActive(true);
                break;
            case eGameState.Playing:
                // No panels active during gameplay
                break;
        }
    }

    public void SetWeaponLevel(int i_WeaponLevel)
    {
        if(m_WeaponLevelText) m_WeaponLevelText.text = $"Wpn Lvl: {i_WeaponLevel}";
    }
    
    public void SetBombs(int i_Amount)
    {
        if (m_BombsAmountText) m_BombsAmountText.text = $"Bombs: {i_Amount}";
    }
}
