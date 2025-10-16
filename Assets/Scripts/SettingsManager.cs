using UnityEngine;

/// <summary>
/// Manages game settings and player preferences using PlayerPrefs
/// </summary>
public class SettingsManager : Singleton<SettingsManager>
{
    [Header("Settings Keys")]
    private const string k_UseMouseControlsKey = "ChickenInvaders_UseMouseControls";
    private const string k_MouseSmoothingKey = "ChickenInvaders_MouseSmoothing";
    private const string k_KeyboardSmoothingKey = "ChickenInvaders_KeyboardSmoothing";
    private const string k_MasterVolumeKey = "ChickenInvaders_MasterVolume";
    private const string k_SFXVolumeKey = "ChickenInvaders_SFXVolume";
    private const string k_MusicVolumeKey = "ChickenInvaders_MusicVolume";
    
    [Header("Default Values")]
    [SerializeField] private bool m_DefaultUseMouseControls = true;
    [SerializeField] private bool m_DefaultMouseSmoothing = true;
    [SerializeField] private bool m_DefaultKeyboardSmoothing = true;
    [SerializeField] private float m_DefaultMasterVolume = 1.0f;
    [SerializeField] private float m_DefaultSFXVolume = 1.0f;
    [SerializeField] private float m_DefaultMusicVolume = 0.8f;
    
    // Current settings (cached for performance)
    private bool m_UseMouseControls;
    private bool m_MouseSmoothing;
    private bool m_KeyboardSmoothing;
    private float m_MasterVolume;
    private float m_SFXVolume;
    private float m_MusicVolume;
    
    protected override void Awake()
    {
        base.Awake();
        if (this == Instance)
        {
            LoadSettings();
            ApplySettings();
        }
    }
    
    /// <summary>
    /// Load all settings from PlayerPrefs
    /// </summary>
    public void LoadSettings()
    {
        m_UseMouseControls = PlayerPrefs.GetInt(k_UseMouseControlsKey, m_DefaultUseMouseControls ? 1 : 0) == 1;
        m_MouseSmoothing = PlayerPrefs.GetInt(k_MouseSmoothingKey, m_DefaultMouseSmoothing ? 1 : 0) == 1;
        m_KeyboardSmoothing = PlayerPrefs.GetInt(k_KeyboardSmoothingKey, m_DefaultKeyboardSmoothing ? 1 : 0) == 1;
        m_MasterVolume = PlayerPrefs.GetFloat(k_MasterVolumeKey, m_DefaultMasterVolume);
        m_SFXVolume = PlayerPrefs.GetFloat(k_SFXVolumeKey, m_DefaultSFXVolume);
        m_MusicVolume = PlayerPrefs.GetFloat(k_MusicVolumeKey, m_DefaultMusicVolume);
    }
    
    /// <summary>
    /// Save all settings to PlayerPrefs
    /// </summary>
    public void SaveSettings()
    {
        PlayerPrefs.SetInt(k_UseMouseControlsKey, m_UseMouseControls ? 1 : 0);
        PlayerPrefs.SetInt(k_MouseSmoothingKey, m_MouseSmoothing ? 1 : 0);
        PlayerPrefs.SetInt(k_KeyboardSmoothingKey, m_KeyboardSmoothing ? 1 : 0);
        PlayerPrefs.SetFloat(k_MasterVolumeKey, m_MasterVolume);
        PlayerPrefs.SetFloat(k_SFXVolumeKey, m_SFXVolume);
        PlayerPrefs.SetFloat(k_MusicVolumeKey, m_MusicVolume);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Apply current settings to game systems
    /// </summary>
    public void ApplySettings()
    {
        // Apply control settings to player controller
        var playerController = PlayerController.Instance;
        if (playerController != null)
        {
            // We'll need to modify PlayerController to accept these settings
            ApplyControlSettings(playerController);
        }
        
        // Apply audio settings
        var audioManager = AudioManager.Instance;
        if (audioManager != null)
        {
            ApplyAudioSettings(audioManager);
        }
    }
    
    /// <summary>
    /// Reset all settings to defaults
    /// </summary>
    public void ResetToDefaults()
    {
        m_UseMouseControls = m_DefaultUseMouseControls;
        m_MouseSmoothing = m_DefaultMouseSmoothing;
        m_KeyboardSmoothing = m_DefaultKeyboardSmoothing;
        m_MasterVolume = m_DefaultMasterVolume;
        m_SFXVolume = m_DefaultSFXVolume;
        m_MusicVolume = m_DefaultMusicVolume;
        
        SaveSettings();
        ApplySettings();
    }
    
    // Control Settings
    public bool UseMouseControls
    {
        get => m_UseMouseControls;
        set
        {
            if (m_UseMouseControls != value)
            {
                m_UseMouseControls = value;
                SaveSettings();
                ApplyControlSettings();
            }
        }
    }
    
    public bool MouseSmoothing
    {
        get => m_MouseSmoothing;
        set
        {
            if (m_MouseSmoothing != value)
            {
                m_MouseSmoothing = value;
                SaveSettings();
                ApplyControlSettings();
            }
        }
    }
    
    public bool KeyboardSmoothing
    {
        get => m_KeyboardSmoothing;
        set
        {
            if (m_KeyboardSmoothing != value)
            {
                m_KeyboardSmoothing = value;
                SaveSettings();
                ApplyControlSettings();
            }
        }
    }
    
    // Audio Settings
    public float MasterVolume
    {
        get => m_MasterVolume;
        set
        {
            m_MasterVolume = Mathf.Clamp01(value);
            SaveSettings();
            ApplyAudioSettings();
        }
    }
    
    public float SFXVolume
    {
        get => m_SFXVolume;
        set
        {
            m_SFXVolume = Mathf.Clamp01(value);
            SaveSettings();
            ApplyAudioSettings();
        }
    }
    
    public float MusicVolume
    {
        get => m_MusicVolume;
        set
        {
            m_MusicVolume = Mathf.Clamp01(value);
            SaveSettings();
            ApplyAudioSettings();
        }
    }
    
    private void ApplyControlSettings(PlayerController playerController = null)
    {
        if (playerController == null)
            playerController = PlayerController.Instance;
            
        if (playerController != null)
        {
            // We'll need to add public methods to PlayerController to set these
            playerController.SetUseMouseControls(m_UseMouseControls);
            playerController.SetMouseSmoothing(m_MouseSmoothing);
            playerController.SetKeyboardSmoothing(m_KeyboardSmoothing);
        }
    }
    
    private void ApplyAudioSettings(AudioManager audioManager = null)
    {
        if (audioManager == null)
            audioManager = AudioManager.Instance;
            
        if (audioManager != null)
        {
            // We'll need to add volume control methods to AudioManager
            audioManager.SetMasterVolume(m_MasterVolume);
            audioManager.SetSFXVolume(m_SFXVolume);
            audioManager.SetMusicVolume(m_MusicVolume);
        }
    }
}
