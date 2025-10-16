using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages inter-wave messages like wave announcements and motivational text
/// </summary>
public class InterWaveMessageManager : Singleton<InterWaveMessageManager>
{
    [Header("UI References")]
    [SerializeField] private GameObject m_MessagePanel;
    [SerializeField] private TextMeshProUGUI m_WaveNumberText;
    [SerializeField] private TextMeshProUGUI m_MessageText;
    [SerializeField] private CanvasGroup m_MessageCanvasGroup;
    
    [Header("Animation Settings")]
    [SerializeField] private float m_FadeInDuration = 0.5f;
    [SerializeField] private float m_WaveDisplayDuration = 2.0f;
    [SerializeField] private float m_BossDisplayDuration = 4.0f;
    [SerializeField] private float m_FadeOutDuration = 0.5f;
    
    [Header("Wave Messages")]
    [SerializeField] private string[] m_WaveStartMessages = {
        "Get Ready!",
        "Nice Work!",
        "Keep Going!",
        "Prepare to Dodge!",
        "Stay Sharp!",
        "Incoming Wave!",
        "Show No Mercy!",
        "Almost There!",
        "Don't Give Up!",
        "Victory Awaits!"
    };
    
    [SerializeField] private string[] m_BossMessages = {
        "BOSS INCOMING!",
        "PREPARE FOR BATTLE!",
        "FINAL CHALLENGE!",
        "SHOW YOUR SKILLS!",
        "ULTIMATE TEST!"
    };
    
    [SerializeField] private string[] m_BossDefeatedMessages = {
        "BOSS DEFEATED!",
        "EXCELLENT WORK!",
        "VICTORY IS YOURS!",
        "OUTSTANDING!",
        "LEGENDARY PILOT!",
        "MISSION COMPLETE!",
        "WELL DONE, ACE!"
    };
    
    private Coroutine m_CurrentMessageCoroutine;
    
    protected override void Awake()
    {
        base.Awake();
        if (this == Instance)
        {
            // Initialize message panel as hidden
            if (m_MessagePanel) m_MessagePanel.SetActive(false);
            if (m_MessageCanvasGroup) m_MessageCanvasGroup.alpha = 0f;
        }
    }
    
    /// <summary>
    /// Display wave start message with wave number
    /// </summary>
    public void ShowWaveMessage(int i_WaveNumber)
    {
        if (m_CurrentMessageCoroutine != null)
        {
            StopCoroutine(m_CurrentMessageCoroutine);
        }
        
        bool isBossWave = i_WaveNumber % 6 == 0;
        string message = isBossWave ? GetRandomBossMessage() : GetRandomWaveMessage();
        float displayDuration = isBossWave ? m_BossDisplayDuration : m_WaveDisplayDuration;
        
        m_CurrentMessageCoroutine = StartCoroutine(DisplayMessage($"WAVE {i_WaveNumber}", message, displayDuration));
    }
    
    /// <summary>
    /// Display custom message (for special events)
    /// </summary>
    public void ShowCustomMessage(string i_Title, string i_Message)
    {
        if (m_CurrentMessageCoroutine != null)
        {
            StopCoroutine(m_CurrentMessageCoroutine);
        }
        
        m_CurrentMessageCoroutine = StartCoroutine(DisplayMessage(i_Title, i_Message, m_WaveDisplayDuration));
    }
    
    /// <summary>
    /// Show boss defeated congratulations message
    /// </summary>
    public void ShowBossDefeatedMessage()
    {
        string message = GetRandomBossDefeatedMessage();
        
        Debug.Log($"InterWaveMessageManager: Showing boss defeated message: {message}");
        
        if (m_CurrentMessageCoroutine != null)
        {
            StopCoroutine(m_CurrentMessageCoroutine);
        }
        
        m_CurrentMessageCoroutine = StartCoroutine(DisplayMessage("", message, m_BossDisplayDuration));
    }
    
    private IEnumerator DisplayMessage(string i_WaveText, string i_Message, float i_DisplayDuration)
    {
        // Boss defeat messages (empty wave text) can show anytime, wave messages only during gameplay
        bool isBossDefeatedMessage = string.IsNullOrEmpty(i_WaveText);
        if (!isBossDefeatedMessage && GameManager.Instance?.GameState != eGameState.Playing) 
        {
            yield break; // Only skip wave messages if not playing
        }
        
        // Set text content
        if (m_WaveNumberText) m_WaveNumberText.text = i_WaveText;
        if (m_MessageText) m_MessageText.text = i_Message;
        
        // Show panel
        if (m_MessagePanel) m_MessagePanel.SetActive(true);
        
        // Fade in
        if (m_MessageCanvasGroup)
        {
            float elapsed = 0f;
            while (elapsed < m_FadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime; // Use unscaled time in case game is paused
                m_MessageCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / m_FadeInDuration);
                yield return null;
            }
            m_MessageCanvasGroup.alpha = 1f;
        }
        
        // Display duration (use the provided duration)
        yield return new WaitForSecondsRealtime(i_DisplayDuration);
        
        // Fade out
        if (m_MessageCanvasGroup)
        {
            float elapsed = 0f;
            while (elapsed < m_FadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                m_MessageCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / m_FadeOutDuration);
                yield return null;
            }
            m_MessageCanvasGroup.alpha = 0f;
        }
        
        // Hide panel
        if (m_MessagePanel) m_MessagePanel.SetActive(false);
        
        m_CurrentMessageCoroutine = null;
    }
    
    
    private string GetRandomWaveMessage()
    {
        if (m_WaveStartMessages.Length == 0) return "Get Ready!";
        return m_WaveStartMessages[Random.Range(0, m_WaveStartMessages.Length)];
    }
    
    private string GetRandomBossMessage()
    {
        if (m_BossMessages.Length == 0) return "BOSS INCOMING!";
        return m_BossMessages[Random.Range(0, m_BossMessages.Length)];
    }
    
    private string GetRandomBossDefeatedMessage()
    {
        if (m_BossDefeatedMessages.Length == 0) return "BOSS DEFEATED!";
        return m_BossDefeatedMessages[Random.Range(0, m_BossDefeatedMessages.Length)];
    }
    
    /// <summary>
    /// Hide any currently displayed message immediately
    /// </summary>
    public void HideMessage()
    {
        if (m_CurrentMessageCoroutine != null)
        {
            StopCoroutine(m_CurrentMessageCoroutine);
            m_CurrentMessageCoroutine = null;
        }
        
        if (m_MessagePanel) m_MessagePanel.SetActive(false);
        if (m_MessageCanvasGroup) m_MessageCanvasGroup.alpha = 0f;
    }
}
