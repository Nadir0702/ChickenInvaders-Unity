using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource m_SFXAudioSource;
    [SerializeField] private AudioSource m_MusicAudioSource;
    
    [Header("Sound Effects")]
    [SerializeField] private AudioClip m_Shoot;
    [SerializeField] private AudioClip m_EnemyHit;
    [SerializeField] private AudioClip m_PlayerHit;
    [SerializeField] private AudioClip m_UIClick;
    [SerializeField] private AudioClip m_Pickup;
    [SerializeField] private AudioClip m_Explosion;
    [SerializeField] private AudioClip m_Eat;
    [SerializeField] private AudioClip m_GameOver;
    [SerializeField] private AudioClip m_BombLaunch;
    [SerializeField] private AudioClip m_LayEgg;
    [SerializeField] private AudioClip m_EggCrack;
    [SerializeField] private AudioClip m_LightSpeed;
    [SerializeField] private AudioClip m_NewRound;
    [SerializeField] private AudioClip m_BossHit;
    
    [Header("Music Themes")]
    [SerializeField] private AudioClip m_Theme;
    [SerializeField] private AudioClip m_BossTheme;
    [SerializeField] private AudioClip m_LightSpeedTheme;
    [SerializeField] private AudioClip m_GameOverTheme;
    
    // Simple state tracking
    private eMusic m_CurrentMusic = eMusic.Theme;
    private float m_ThemeMusicTime = 0f; // For resuming theme music after boss/lightspeed
    private Coroutine m_FadeCoroutine; // Simple fade system
    
    // Volume settings
    private float m_MasterVolume = 1.0f;
    private float m_SFXVolume = 1.0f;
    private float m_MusicVolume = 1.0f;
    
    // Constants - Rebalanced for better SFX/Music ratio
    private const float DEFAULT_MUSIC_VOLUME = 0.15f; // Reduced from 0.3f
    private const float DEFAULT_SFX_VOLUME = 0.8f; // New base SFX volume
    private const float FADE_DURATION = 1.5f;
    
    protected override void Awake()
    {
        base.Awake();
        if (this == Instance)
        {
            InitializeAudio();
            GameManager.OnGameStateChanged += OnGameStateChanged;
        }
    }
    
    private new void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.OnGameStateChanged -= OnGameStateChanged;
        }
    }
    
    private void Start()
    {
        PlayMusic(eMusic.Theme);
    }
    
    private void InitializeAudio()
    {
        if (m_MusicAudioSource != null)
        {
            m_MusicAudioSource.loop = true;
            m_MusicAudioSource.volume = DEFAULT_MUSIC_VOLUME * m_MusicVolume * m_MasterVolume;
        }
    }
    
    // ========== SFX SYSTEM ==========
    
    public void Play(eSFXId i_SfxId, float i_Volume = 1f, float i_Pitch = 1f)
    {
        AudioClip clip = GetSFXClip(i_SfxId);
        if (!clip || !m_SFXAudioSource) return;
        
        m_SFXAudioSource.pitch = i_Pitch;
        float finalVolume = i_Volume * DEFAULT_SFX_VOLUME * m_SFXVolume * m_MasterVolume;
        m_SFXAudioSource.PlayOneShot(clip, finalVolume);
    }
    
    public void OnUIButtonClick()
    {
        Play(eSFXId.UIClick, 0.5f, 1.5f);
    }
    
    private AudioClip GetSFXClip(eSFXId i_SfxId)
    {
        return i_SfxId switch
        {
            eSFXId.Shoot => m_Shoot,
            eSFXId.EnemyHit => m_EnemyHit,
            eSFXId.PlayerHit => m_PlayerHit,
            eSFXId.UIClick => m_UIClick,
            eSFXId.Pickup => m_Pickup,
            eSFXId.Explosion => m_Explosion,
            eSFXId.Eat => m_Eat,
            eSFXId.GameOver => m_GameOver,
            eSFXId.BombLaunch => m_BombLaunch,
            eSFXId.LayEgg => m_LayEgg,
            eSFXId.EggCrack => m_EggCrack,
            eSFXId.LightSpeed => m_LightSpeed,
            eSFXId.NewRound => m_NewRound,
            eSFXId.BossHit => m_BossHit,
            _ => null
        };
    }
    
    // ========== MUSIC SYSTEM ==========
    
    private void OnGameStateChanged(eGameState i_NewState)
    {
        switch (i_NewState)
        {
            case eGameState.Menu:
                PlayMusic(eMusic.Theme);
                break;
            case eGameState.GameOver:
                PlayMusic(eMusic.GameOverTheme, DEFAULT_MUSIC_VOLUME, false);
                break;
            case eGameState.Paused:
                PauseMusic();
                break;
            case eGameState.Playing:
                ResumeMusic();
                break;
        }
    }
    
    private void PlayMusic(eMusic i_Music, float i_Volume = DEFAULT_MUSIC_VOLUME, bool i_Loop = true)
    {
        AudioClip clip = GetMusicClip(i_Music);
        if (!clip || !m_MusicAudioSource) return;
        
        m_CurrentMusic = i_Music;
        m_MusicAudioSource.clip = clip;
        m_MusicAudioSource.loop = i_Loop;
        m_MusicAudioSource.volume = i_Volume * m_MusicVolume * m_MasterVolume;
        m_MusicAudioSource.time = 0f;
        m_MusicAudioSource.Play();
    }
    
    private AudioClip GetMusicClip(eMusic i_Music)
    {
        return i_Music switch
        {
            eMusic.Theme => m_Theme,
            eMusic.BossTheme => m_BossTheme,
            eMusic.LightSpeedTheme => m_LightSpeedTheme,
            eMusic.GameOverTheme => m_GameOverTheme,
            _ => null
        };
    }
    
    private void PauseMusic()
    {
        if (m_MusicAudioSource && m_MusicAudioSource.isPlaying)
        {
            m_MusicAudioSource.Pause();
        }
    }
    
    private void ResumeMusic()
    {
        if (m_MusicAudioSource && !m_MusicAudioSource.isPlaying && m_MusicAudioSource.clip)
        {
            m_MusicAudioSource.UnPause();
        }
    }
    
    // ========== GAME EVENT HANDLERS ==========
    
    public void FadeMusic()
    {
        // Store theme music time before boss battle
        if (m_CurrentMusic == eMusic.Theme && m_MusicAudioSource && m_MusicAudioSource.isPlaying)
        {
            m_ThemeMusicTime = m_MusicAudioSource.time;
        }
        
        // Start fade out
        FadeOut();
    }
    
    private void FadeOut(System.Action i_OnComplete = null)
    {
        if (m_FadeCoroutine != null)
        {
            StopCoroutine(m_FadeCoroutine);
        }
        
        // If no music is playing, just call the callback immediately
        if (!m_MusicAudioSource || !m_MusicAudioSource.isPlaying)
        {
            i_OnComplete?.Invoke();
            return;
        }
        
        m_FadeCoroutine = StartCoroutine(FadeOutCoroutine(i_OnComplete));
    }
    
    private void FadeIn(eMusic i_Music, float i_StartTime = 0f)
    {
        if (m_FadeCoroutine != null)
        {
            StopCoroutine(m_FadeCoroutine);
        }
        
        m_FadeCoroutine = StartCoroutine(FadeInCoroutine(i_Music, i_StartTime));
    }
    
    private System.Collections.IEnumerator FadeOutCoroutine(System.Action i_OnComplete)
    {
        if (!m_MusicAudioSource || !m_MusicAudioSource.isPlaying) yield break;
        
        float startVolume = m_MusicAudioSource.volume;
        float elapsed = 0f;
        
        while (elapsed < FADE_DURATION)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / FADE_DURATION;
            m_MusicAudioSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            yield return null;
        }
        
        m_MusicAudioSource.volume = 0f;
        m_MusicAudioSource.Stop();
        m_FadeCoroutine = null;
        
        i_OnComplete?.Invoke();
    }
    
    private System.Collections.IEnumerator FadeInCoroutine(eMusic i_Music, float i_StartTime)
    {
        AudioClip clip = GetMusicClip(i_Music);
        if (!clip || !m_MusicAudioSource) 
        {
            Debug.LogError($"AudioManager: Cannot fade in {i_Music} - missing clip or audio source");
            yield break;
        }
        
        // Setup music
        m_CurrentMusic = i_Music;
        m_MusicAudioSource.clip = clip;
        m_MusicAudioSource.loop = (i_Music != eMusic.LightSpeedTheme && i_Music != eMusic.GameOverTheme);
        m_MusicAudioSource.time = i_StartTime;
        m_MusicAudioSource.volume = 0f;
        m_MusicAudioSource.Play();
        
        // Light speed music should never use fade in - it's handled directly
        if (i_Music == eMusic.LightSpeedTheme)
        {
            Debug.LogError("AudioManager: Light speed music should not use fade in system!");
            yield break;
        }
        
        // Calculate target volume for other music
        float targetVolume = DEFAULT_MUSIC_VOLUME * m_MusicVolume * m_MasterVolume;
        
        // Fade in
        float elapsed = 0f;
        while (elapsed < FADE_DURATION)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / FADE_DURATION;
            m_MusicAudioSource.volume = Mathf.Lerp(0f, targetVolume, progress);
            yield return null;
        }
        
        m_MusicAudioSource.volume = targetVolume;
        m_FadeCoroutine = null;
    }
    
    public void OnBossWaveStart()
    {
        FadeOut(() => FadeIn(eMusic.BossTheme));
    }
    
    public void OnBossDefeated()
    {
        // Fade out boss music quickly, light speed will take over soon
        FadeOut();
    }
    
    public void OnLightSpeedStart()
    {
        // Store theme music time if currently playing theme
        if (m_CurrentMusic == eMusic.Theme && m_MusicAudioSource && m_MusicAudioSource.isPlaying)
        {
            m_ThemeMusicTime = m_MusicAudioSource.time;
        }
        
        // Stop any fade coroutine immediately - this is critical
        if (m_FadeCoroutine != null)
        {
            StopCoroutine(m_FadeCoroutine);
            m_FadeCoroutine = null;
        }
        
        // Stop current music immediately and play light speed music directly
        if (m_MusicAudioSource)
        {
            m_MusicAudioSource.Stop();
        }
        
        // Add a small delay to ensure clean transition
        StartCoroutine(PlayLightSpeedMusicDelayed());
    }
    
    public void OnNewWaveCycleStart()
    {
        // Resume theme music from where it was stored with fade in
        FadeIn(eMusic.Theme, m_ThemeMusicTime);
    }
    
    public void OnGameRestart()
    {
        m_ThemeMusicTime = 0f;
        FadeIn(eMusic.Theme, 0f);
    }
    
    // ========== LIGHT SPEED MUSIC & SFX ==========
    
    private System.Collections.IEnumerator PlayLightSpeedMusicDelayed()
    {
        // Small delay to ensure clean transition from any previous music
        yield return new WaitForSeconds(0.1f);
        PlayLightSpeedMusicDirect();
    }
    
    private void PlayLightSpeedMusicDirect()
    {
        AudioClip clip = GetMusicClip(eMusic.LightSpeedTheme);
        if (!clip || !m_MusicAudioSource)
        {
            Debug.LogError("AudioManager: Cannot play light speed music - missing clip or audio source");
            return;
        }
        
        // Ensure no fade coroutine is running that could interfere
        if (m_FadeCoroutine != null)
        {
            StopCoroutine(m_FadeCoroutine);
            m_FadeCoroutine = null;
        }
        
        // Play immediately at full volume - no fade
        m_CurrentMusic = eMusic.LightSpeedTheme;
        m_MusicAudioSource.clip = clip;
        m_MusicAudioSource.loop = false; // Light speed doesn't loop
        m_MusicAudioSource.time = 0f;
        m_MusicAudioSource.volume = DEFAULT_MUSIC_VOLUME * 5f * m_MusicVolume * m_MasterVolume; // Using boosted volume
        m_MusicAudioSource.Play();
    }
    
    public void PlayLightSpeedSFX()
    {
        if (m_SFXAudioSource && m_LightSpeed)
        {
            // Use PlayOneShot instead of directly modifying the AudioSource
            float lightSpeedVolume = 0.3f * DEFAULT_SFX_VOLUME * m_SFXVolume * m_MasterVolume;
            m_SFXAudioSource.PlayOneShot(m_LightSpeed, lightSpeedVolume);
        }
    }
    
    public void StopSFX()
    {
        if (m_SFXAudioSource)
        {
            m_SFXAudioSource.Stop();
            m_SFXAudioSource.clip = null;
        }
    }
    
    // ========== VOLUME CONTROL ==========
    
    public void SetMasterVolume(float i_Volume)
    {
        m_MasterVolume = Mathf.Clamp01(i_Volume);
        UpdateMusicVolume();
    }
    
    public void SetSFXVolume(float i_Volume)
    {
        m_SFXVolume = Mathf.Clamp01(i_Volume);
    }
    
    public void SetMusicVolume(float i_Volume)
    {
        m_MusicVolume = Mathf.Clamp01(i_Volume);
        UpdateMusicVolume();
    }
    
    private void UpdateMusicVolume()
    {
        if (m_MusicAudioSource && m_MusicAudioSource.isPlaying)
        {
            float baseVolume = m_CurrentMusic == eMusic.LightSpeedTheme ? DEFAULT_MUSIC_VOLUME * 5f : DEFAULT_MUSIC_VOLUME;
            m_MusicAudioSource.volume = baseVolume * m_MusicVolume * m_MasterVolume;
        }
    }
}