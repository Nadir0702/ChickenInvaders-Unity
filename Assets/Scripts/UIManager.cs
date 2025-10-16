using TMPro;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI m_ScoreText;
    [SerializeField] private TextMeshProUGUI m_LivesText;
    [SerializeField] private TextMeshProUGUI m_WeaponLevelText;
    [SerializeField] private TextMeshProUGUI m_BombsAmountText;
    [SerializeField] private TextMeshProUGUI m_WaveNumberText;
    
    [Header("UI Panels")]
    [SerializeField] private GameObject m_GameOverPanel;
    [SerializeField] private GameObject m_PausePanel;
    [SerializeField] private GameObject m_HomeScreenPanel;
    [SerializeField] private GameObject m_ControlsPanel;
    [SerializeField] private GameObject m_SettingsPanel;

    private void Start()
    {
        // Fix Canvas scale issue that can make UI invisible
        FixCanvasScaleIssue();
        
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
    
    /// <summary>
    /// Fix Canvas scale issue that can occur with Canvas Scaler in "Scale With Screen Size" mode
    /// </summary>
    private void FixCanvasScaleIssue()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        Vector3 currentScale = canvas.transform.localScale;
        
        // If Canvas scale is zero, this makes all UI invisible
        if (currentScale.x == 0f || currentScale.y == 0f || currentScale.z == 0f)
        {
            Debug.LogWarning($"UIManager: Canvas scale is {currentScale}, fixing to (1,1,1)");
            canvas.transform.localScale = Vector3.one;
            
            // Also check if Canvas Scaler is causing issues
            var canvasScaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
            if (canvasScaler != null && canvasScaler.uiScaleMode == UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                Debug.Log($"UIManager: Canvas Scaler reference resolution: {canvasScaler.referenceResolution}, current screen: {Screen.width}x{Screen.height}");
            }
        }
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
        m_SettingsPanel?.SetActive(false);
    }

    public void SetWeaponLevel(int i_WeaponLevel)
    {
        if(m_WeaponLevelText) m_WeaponLevelText.text = $"Wpn Lvl: {i_WeaponLevel}";
    }
    
    public void SetBombs(int i_Amount)
    {
        if (m_BombsAmountText) m_BombsAmountText.text = $"Bombs: {i_Amount}";
    }
    
    public void SetWaveNumber(int i_WaveNumber)
    {
        if (m_WaveNumberText) m_WaveNumberText.text = $"Wave: {i_WaveNumber}";
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

    public void ShowSettingsPanel()
    {
        Debug.Log("UIManager: ShowSettingsPanel called");
        hideAllPanels();
        
        if (m_SettingsPanel)
        {
            m_SettingsPanel.SetActive(true);
            Debug.Log($"UIManager: Settings panel activated - Active: {m_SettingsPanel.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("UIManager: m_SettingsPanel is null!");
        }
    }

    public void HideSettingsPanel()
    {
        hideAllPanels();
        
        m_HomeScreenPanel?.SetActive(true);
    }
}
