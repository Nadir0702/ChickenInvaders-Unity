using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : Singleton<UIManager>
{
    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI m_ScoreText;
    
    [Header("Lives Display")]
    [SerializeField] private Transform m_LivesContainer;
    [SerializeField] private Sprite m_HeartSprite; // Use Food sprite or any suitable sprite
    private List<HUDIcon> m_HeartInstances = new List<HUDIcon>();
    
    [Header("Bombs Display")]
    [SerializeField] private Transform m_BombsContainer;
    [SerializeField] private Sprite m_BombSprite; // Use Bomb sprite
    private List<HUDIcon> m_BombInstances = new List<HUDIcon>();
    
    [Header("Icon Settings")]
    [SerializeField] private Vector2 m_IconSize = new Vector2(32, 32);
    
    [Header("HUD Container")]
    [SerializeField] private GameObject m_HUDContainer; // Parent container for all HUD elements
    
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
        
        // Return icon instances to pool
        ReturnIconInstancesToPool(m_HeartInstances);
        ReturnIconInstancesToPool(m_BombInstances);
    }
    
    /// <summary>
    /// Returns a list of icon instances to the pool
    /// </summary>
    private void ReturnIconInstancesToPool(List<HUDIcon> instances)
    {
        foreach (var instance in instances)
        {
            if (instance != null)
            {
                PoolManager.Instance?.ReturnHUDIcon(instance);
            }
        }
        instances.Clear();
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
        if (m_ScoreText) m_ScoreText.text = $"{i_Value}";
    }
    
    public void SetLives(int i_Value)
    {
        Debug.Log($"UIManager: SetLives called with {i_Value} lives. HUD Container active: {(m_HUDContainer ? m_HUDContainer.activeInHierarchy.ToString() : "null")}");
        UpdateImageDisplay(m_HeartInstances, m_HeartSprite, m_LivesContainer, i_Value);
    }       

    private void showState(eGameState i_GameState)
    {
        // Handle HUD state transitions
        bool wasPlaying = m_HUDContainer && m_HUDContainer.activeInHierarchy;
        bool willBePlaying = i_GameState == eGameState.Playing;
        
        if (wasPlaying && !willBePlaying)
        {
            OnLeavePlayingState();
        }
        
        // Hide all panels first
        hideAllPanels();
        
        // Control HUD visibility - only show during gameplay
        if (m_HUDContainer)
        {
            m_HUDContainer.SetActive(willBePlaying);
        }

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
                // No panels active during gameplay, but HUD is visible
                OnEnterPlayingState();
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

    public void SetBombs(int i_Amount)
    {
        UpdateImageDisplay(m_BombInstances, m_BombSprite, m_BombsContainer, i_Amount);
    }
    
    /// <summary>
    /// Refresh HUD display - called when pools are ready
    /// </summary>
    public void RefreshHUDDisplay()
    {
        if (GameManager.Instance != null)
        {
            SetLives(GameManager.Instance.CurrentLives);
            // Get current bomb count from PlayerStats
            var playerStats = FindFirstObjectByType<PlayerStats>();
            if (playerStats != null)
            {
                SetBombs(playerStats.BombCount);
            }
        }
    }
    
    /// <summary>
    /// Called when entering playing state to ensure HUD is properly displayed
    /// </summary>
    public void OnEnterPlayingState()
    {
        // Force refresh HUD display when entering playing state
        RefreshHUDDisplay();
    }
    
    /// <summary>
    /// Called when leaving playing state to clean up HUD
    /// </summary>
    public void OnLeavePlayingState()
    {
        // Return HUD icons to pool when leaving playing state
        ReturnIconInstancesToPool(m_HeartInstances);
        ReturnIconInstancesToPool(m_BombInstances);
    }
    
    /// <summary>
    /// Updates an image-based display using pooled instances
    /// </summary>
    private void UpdateImageDisplay(List<HUDIcon> instances, Sprite sprite, Transform container, int targetCount)
    {
        if (!sprite || !container || !PoolManager.Instance) return;
        
        // Check if pool is ready
        if (!PoolManager.Instance.IsHUDIconPoolReady())
        {
            Debug.LogWarning("UIManager: HUD Icon pool not ready yet, deferring update");
            return;
        }
        
        // Return excess instances to pool
        while (instances.Count > targetCount)
        {
            int lastIndex = instances.Count - 1;
            if (instances[lastIndex] != null)
            {
                PoolManager.Instance.ReturnHUDIcon(instances[lastIndex]);
            }
            instances.RemoveAt(lastIndex);
        }
        
        // Get missing instances from pool
        while (instances.Count < targetCount)
        {
            HUDIcon newInstance = PoolManager.Instance.GetHUDIcon(Vector3.zero);
            if (newInstance != null)
            {
                SetupHUDIcon(newInstance, sprite, container);
                instances.Add(newInstance);
            }
            else
            {
                Debug.LogWarning("UIManager: Failed to get HUD icon from pool!");
                break;
            }
        }
    }
    
    /// <summary>
    /// Sets up a pooled HUD icon with the given sprite and parent
    /// </summary>
    private void SetupHUDIcon(HUDIcon hudIcon, Sprite sprite, Transform parent)
    {
        hudIcon.transform.SetParent(parent, false);
        hudIcon.SetSprite(sprite);
        hudIcon.SetSize(m_IconSize);
        
        // Set RectTransform properties for UI layout
        RectTransform rectTransform = hudIcon.GetComponent<RectTransform>();
        if (rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
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
