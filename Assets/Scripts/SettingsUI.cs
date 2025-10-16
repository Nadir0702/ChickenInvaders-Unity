using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI component for game settings
/// </summary>
public class SettingsUI : MonoBehaviour
{
    [Header("Settings Panel")]
    [SerializeField] private GameObject m_SettingsPanel;
    
    [Header("Control Settings")]
    [SerializeField] private Toggle m_MouseControlsToggle;
    [SerializeField] private Toggle m_MouseSmoothingToggle;
    [SerializeField] private Toggle m_KeyboardSmoothingToggle;
    
    [Header("Audio Settings")]
    [SerializeField] private Slider m_MasterVolumeSlider;
    [SerializeField] private Slider m_SFXVolumeSlider;
    [SerializeField] private Slider m_MusicVolumeSlider;
    [SerializeField] private TextMeshProUGUI m_MasterVolumePercentageText;
    [SerializeField] private TextMeshProUGUI m_SFXVolumePercentageText;
    [SerializeField] private TextMeshProUGUI m_MusicVolumePercentageText;
    [SerializeField] private TextMeshProUGUI m_MasterVolumeText;
    [SerializeField] private TextMeshProUGUI m_SFXVolumeText;
    [SerializeField] private TextMeshProUGUI m_MusicVolumeText;
    
    [Header("Buttons")]
    [SerializeField] private Button m_ResetToDefaultsButton;
    [SerializeField] private Button m_CloseButton;
    
    private void Start()
    {
        // Initialize panel as hidden
        if (m_SettingsPanel) m_SettingsPanel.SetActive(false);
        
        // Set up control toggles
        if (m_MouseControlsToggle)
        {
            m_MouseControlsToggle.onValueChanged.AddListener(OnMouseControlsChanged);
        }
        
        if (m_MouseSmoothingToggle)
        {
            m_MouseSmoothingToggle.onValueChanged.AddListener(OnMouseSmoothingChanged);
        }
        
        if (m_KeyboardSmoothingToggle)
        {
            m_KeyboardSmoothingToggle.onValueChanged.AddListener(OnKeyboardSmoothingChanged);
        }
        
        // Set up volume sliders
        if (m_MasterVolumeSlider)
        {
            m_MasterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
        
        if (m_SFXVolumeSlider)
        {
            m_SFXVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
        
        if (m_MusicVolumeSlider)
        {
            m_MusicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        
        if (m_MasterVolumeText)
        {
            m_MasterVolumeText.text = "Master Volume";
        }
        
        if (m_SFXVolumeText)
        {
            m_SFXVolumeText.text = "SFX Volume";
        }
        
        if (m_MusicVolumeText)
        {
            m_MusicVolumeText.text = "Music Volume";
        }
        
        // Set up buttons
        if (m_ResetToDefaultsButton)
        {
            m_ResetToDefaultsButton.onClick.AddListener(ResetToDefaults);
        }
        
        if (m_CloseButton)
        {
            m_CloseButton.onClick.AddListener(CloseSettings);
        }
    }
    
    /// <summary>
    /// Show the settings panel and load current values
    /// </summary>
    public void ShowSettings()
    {
        // Use UIManager to properly hide other panels
        UIManager.Instance?.ShowSettingsPanel();
        
        if (m_SettingsPanel)
        {
            m_SettingsPanel.SetActive(true);
            Debug.Log($"SettingsUI: Settings panel activated - Active: {m_SettingsPanel.activeInHierarchy}");
            LoadCurrentSettings();
        }
    }
    
    /// <summary>
    /// Hide the settings panel
    /// </summary>
    public void HideSettings()
    {
        if (m_SettingsPanel)
        {
            m_SettingsPanel.SetActive(false);
        }
        
        // Return to home screen
        UIManager.Instance?.HideSettingsPanel();
    }
    
    /// <summary>
    /// Load current settings into UI elements
    /// </summary>
    private void LoadCurrentSettings()
    {
        if (SettingsManager.Instance == null) return;
        
        // Load control settings
        if (m_MouseControlsToggle)
        {
            m_MouseControlsToggle.SetIsOnWithoutNotify(SettingsManager.Instance.UseMouseControls);
        }
        
        if (m_MouseSmoothingToggle)
        {
            m_MouseSmoothingToggle.SetIsOnWithoutNotify(SettingsManager.Instance.MouseSmoothing);
        }
        
        if (m_KeyboardSmoothingToggle)
        {
            m_KeyboardSmoothingToggle.SetIsOnWithoutNotify(SettingsManager.Instance.KeyboardSmoothing);
        }
        
        // Load audio settings
        if (m_MasterVolumeSlider)
        {
            m_MasterVolumeSlider.SetValueWithoutNotify(SettingsManager.Instance.MasterVolume);
            UpdateVolumeText(m_MasterVolumePercentageText, SettingsManager.Instance.MasterVolume);
        }
        
        if (m_SFXVolumeSlider)
        {
            m_SFXVolumeSlider.SetValueWithoutNotify(SettingsManager.Instance.SFXVolume);
            UpdateVolumeText(m_SFXVolumePercentageText, SettingsManager.Instance.SFXVolume);
        }
        
        if (m_MusicVolumeSlider)
        {
            m_MusicVolumeSlider.SetValueWithoutNotify(SettingsManager.Instance.MusicVolume);
            UpdateVolumeText(m_MusicVolumePercentageText, SettingsManager.Instance.MusicVolume);
        }
    }
    
    /// <summary>
    /// Update volume text display
    /// </summary>
    private void UpdateVolumeText(TextMeshProUGUI i_Text, float i_Volume)
    {
        if (i_Text)
        {
            i_Text.text = $"{Mathf.RoundToInt(i_Volume * 100)}%";
        }
    }
    
    // Control setting callbacks
    private void OnMouseControlsChanged(bool i_Value)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.UseMouseControls = i_Value;
        }
        AudioManager.Instance?.OnUIButtonClick();
    }
    
    private void OnMouseSmoothingChanged(bool i_Value)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.MouseSmoothing = i_Value;
        }
        AudioManager.Instance?.OnUIButtonClick();
    }
    
    private void OnKeyboardSmoothingChanged(bool i_Value)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.KeyboardSmoothing = i_Value;
        }
        AudioManager.Instance?.OnUIButtonClick();
    }
    
    // Audio setting callbacks
    private void OnMasterVolumeChanged(float i_Value)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.MasterVolume = i_Value;
        }
        UpdateVolumeText(m_MasterVolumePercentageText, i_Value);
    }
    
    private void OnSFXVolumeChanged(float i_Value)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SFXVolume = i_Value;
        }
        UpdateVolumeText(m_SFXVolumePercentageText, i_Value);
    }
    
    private void OnMusicVolumeChanged(float i_Value)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.MusicVolume = i_Value;
        }
        UpdateVolumeText(m_MusicVolumePercentageText, i_Value);
    }
    
    /// <summary>
    /// Reset all settings to defaults
    /// </summary>
    private void ResetToDefaults()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.ResetToDefaults();
            LoadCurrentSettings(); // Refresh UI
        }
        AudioManager.Instance?.OnUIButtonClick();
    }
    
    /// <summary>
    /// Close settings panel
    /// </summary>
    public void CloseSettings()
    {
        HideSettings();
        AudioManager.Instance?.OnUIButtonClick();
    }
}
