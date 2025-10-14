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
    [SerializeField] private GameObject m_ControlsPanel;

    private void Start()
    {
        // Initialize all panels as inactive
        hideAllPanels();
        
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
        hideAllPanels();

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

    private void hideAllPanels()
    {
        m_GameOverPanel?.SetActive(false);
        m_PausePanel?.SetActive(false);
        m_HomeScreenPanel?.SetActive(false);
        m_ControlsPanel?.SetActive(false);
    }

    public void SetWeaponLevel(int i_WeaponLevel)
    {
        if(m_WeaponLevelText) m_WeaponLevelText.text = $"Wpn Lvl: {i_WeaponLevel}";
    }
    
    public void SetBombs(int i_Amount)
    {
        if (m_BombsAmountText) m_BombsAmountText.text = $"Bombs: {i_Amount}";
    }

    public void ShowControlsPanel()
    {
        hideAllPanels();
        
        m_ControlsPanel?.SetActive(true);
    }

    public void HideControlsPanel()
    {
        hideAllPanels();
        
        m_HomeScreenPanel?.SetActive(true);
    }
}
