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
    
    // Music state management
    private eMusic m_CurrentMusic = eMusic.Theme;
    private bool m_IsMusicPaused = false;
    private Coroutine m_FadeCoroutine;
    private float m_CurrentMusicTime = 0f; // Time position of currently playing music (for pause/resume)
    private float m_ThemeMusicTime = 0f; // Time position of theme music (for restoration between rounds)
    private eGameState m_PreviousGameState = eGameState.Menu;
    
    // Constants
    private const float DEFAULT_MUSIC_VOLUME = 0.3f;
    private const float DEFAULT_FADE_DURATION = 2f;
    
    protected override void Awake()
    {
        base.Awake();
        if (this == Instance)
        {
            initializeAudioSources();
            GameManager.OnGameStateChanged += OnGameStateChanged;
        }
    }
    
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.OnGameStateChanged -= OnGameStateChanged;
        }
    }
    
    private void Start()
    {
        PlayThemeMusic();
    }
    
    private void initializeAudioSources()
    {
        if (m_MusicAudioSource != null)
        {
            m_MusicAudioSource.loop = true;
            m_MusicAudioSource.volume = DEFAULT_MUSIC_VOLUME;
        }
    }
    
    // ========== SFX SYSTEM ==========
    
    public void Play(eSFXId i_SfxId, float i_Volume = 1f, float i_Pitch = 1f)
    {
        AudioClip clip = getSFXClip(i_SfxId);
        if (!clip) return;
        
        m_SFXAudioSource.pitch = i_Pitch;
        m_SFXAudioSource.PlayOneShot(clip, i_Volume);
    }
    
    public void OnUIButtonClick()
    {
        Play(eSFXId.UIClick, 0.5f, 1.5f);
    }
    
    private AudioClip getSFXClip(eSFXId i_SfxId)
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
                handleMenuState();
                break;
            case eGameState.Playing:
                handlePlayingState();
                break;
            case eGameState.Paused:
                handlePausedState();
                break;
            case eGameState.GameOver:
                handleGameOverState();
                break;
        }
        
        m_PreviousGameState = i_NewState;
    }
    
    private void handleMenuState()
    {
        if (m_PreviousGameState != eGameState.Menu)
        {
            PlayThemeMusic();
        }
    }
    
    private void handlePlayingState()
    {
        if (m_IsMusicPaused && m_PreviousGameState == eGameState.Paused)
        {
            ResumeMusic();
            ResumeSFX(); // Also resume SFX if it was paused
        }
        else if (m_PreviousGameState == eGameState.Menu && isThemePlaying())
        {
            // Continue theme music from menu
        }
        else if (!isBackgroundMusicPlaying())
        {
            PlayThemeMusic();
        }
    }
    
    private void handlePausedState()
    {
        // Pause any music except GameOver music
        if (m_CurrentMusic != eMusic.GameOverTheme)
        {
            PauseMusic();
        }
        
        // Also pause SFX if light speed is playing
        PauseSFX();
    }
    
    private void handleGameOverState()
    {
        PlayGameOverMusic();
    }
    
    private bool isThemePlaying()
    {
        return m_CurrentMusic == eMusic.Theme && m_MusicAudioSource.isPlaying;
    }
    
    private bool isBackgroundMusicPlaying()
    {
        return m_CurrentMusic == eMusic.Theme || m_CurrentMusic == eMusic.BossTheme || m_CurrentMusic == eMusic.LightSpeedTheme;
    }
    
    public void PlayMusic(eMusic i_Music, float i_Volume = DEFAULT_MUSIC_VOLUME, bool i_Loop = true, bool i_RestartFromBeginning = true)
    {
        AudioClip clip = getMusicClip(i_Music);
        if (!clip || m_MusicAudioSource == null) return;
        
        stopFadeCoroutine();
        
        if (shouldPlayMusic(i_Music, i_RestartFromBeginning))
        {
            startMusic(i_Music, clip, i_Volume, i_Loop);
        }
    }
    
    private AudioClip getMusicClip(eMusic i_Music)
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
    
    private bool shouldPlayMusic(eMusic i_Music, bool i_RestartFromBeginning)
    {
        return m_CurrentMusic != i_Music || i_RestartFromBeginning || !m_MusicAudioSource.isPlaying;
    }
    
    private void startMusic(eMusic i_Music, AudioClip i_Clip, float i_Volume, bool i_Loop)
    {
        m_CurrentMusic = i_Music;
        m_MusicAudioSource.clip = i_Clip;
        m_MusicAudioSource.loop = i_Loop;
        m_MusicAudioSource.volume = i_Volume;
        m_MusicAudioSource.Play();
        m_IsMusicPaused = false;
        
        Debug.Log($"Playing music: {i_Music} (Loop: {i_Loop})");
    }
    
    private void stopFadeCoroutine()
    {
        if (m_FadeCoroutine != null)
        {
            StopCoroutine(m_FadeCoroutine);
            m_FadeCoroutine = null;
        }
    }
    
    // ========== PUBLIC MUSIC METHODS ==========
    
    public void PlayThemeMusic()
    {
        stopMusicAndResetState();
        PlayMusic(eMusic.Theme);
    }
    
    public void ContinueThemeMusic()
    {
        // Continue theme music from its stored position
        FadeInMusicFromPosition(eMusic.Theme, DEFAULT_FADE_DURATION, m_ThemeMusicTime);
        Debug.Log($"Continuing theme music from time: {m_ThemeMusicTime}");
    }
    
    public void PlayBossMusic()
    {
        // Theme music time already stored by FadeMusic() before boss battle
        stopMusicAndResetPauseState(); // Don't reset theme time
        PlayMusic(eMusic.BossTheme, DEFAULT_MUSIC_VOLUME, true, true);
        m_CurrentMusicTime = 0f; // Reset current music time for new boss music
    }
    
    public void PlayLightSpeedMusic()
    {
        // Don't reset theme music time when playing light speed music
        PlayMusic(eMusic.LightSpeedTheme, DEFAULT_MUSIC_VOLUME, false, true); // No loop
    }
    
    public void PlayGameOverMusic()
    {
        PlayMusic(eMusic.GameOverTheme, DEFAULT_MUSIC_VOLUME, false, true); // No loop
    }
    
    public void OnGameRestart()
    {
        stopMusicAndResetState();
        PlayThemeMusic();
    }
    
    public void ResetMusicTime()
    {
        m_CurrentMusicTime = 0f;
        m_ThemeMusicTime = 0f;
    }
    
    private void stopMusicAndResetState()
    {
        if (m_MusicAudioSource != null)
        {
            m_MusicAudioSource.Stop();
        }
        
        m_IsMusicPaused = false;
        m_CurrentMusicTime = 0f;
        m_ThemeMusicTime = 0f;
    }
    
    private void stopMusicAndResetPauseState()
    {
        if (m_MusicAudioSource != null)
        {
            m_MusicAudioSource.Stop();
        }
        
        m_IsMusicPaused = false;
        m_CurrentMusicTime = 0f;
        // Don't reset m_ThemeMusicTime here - it's preserved for restoration
    }
    
    // ========== MUSIC CONTROL METHODS ==========
    
    public void PauseMusic()
    {
        if (m_MusicAudioSource != null && m_MusicAudioSource.isPlaying)
        {
            m_CurrentMusicTime = m_MusicAudioSource.time;
            
            // Also update theme music time if we're pausing theme music
            if (m_CurrentMusic == eMusic.Theme)
            {
                m_ThemeMusicTime = m_CurrentMusicTime;
            }
            
            m_MusicAudioSource.Pause();
            m_IsMusicPaused = true;
            Debug.Log($"Music paused at time: {m_CurrentMusicTime} (Current: {m_CurrentMusic})");
        }
    }
    
    public void ResumeMusic()
    {
        if (m_MusicAudioSource != null && m_IsMusicPaused)
        {
            m_MusicAudioSource.UnPause();
            m_IsMusicPaused = false;
            Debug.Log("Music resumed");
        }
    }
    
    public void StopMusic()
    {
        if (m_MusicAudioSource != null)
        {
            m_MusicAudioSource.Stop();
            m_IsMusicPaused = false;
            Debug.Log("Music stopped");
        }
    }
    
    // ========== FADE SYSTEM ==========
    
    public void FadeOutMusic(float i_FadeDuration = DEFAULT_FADE_DURATION, System.Action i_OnComplete = null)
    {
        if (m_MusicAudioSource != null && m_FadeCoroutine == null)
        {
            storeCurrentMusicTime();
            m_FadeCoroutine = StartCoroutine(FadeOutCoroutine(i_FadeDuration, i_OnComplete));
        }
    }
    
    public void FadeInMusic(eMusic i_Music, float i_FadeDuration = DEFAULT_FADE_DURATION, float i_TargetVolume = DEFAULT_MUSIC_VOLUME)
    {
        if (m_MusicAudioSource != null && m_FadeCoroutine == null)
        {
            m_FadeCoroutine = StartCoroutine(FadeInCoroutine(i_Music, i_FadeDuration, i_TargetVolume, 0f));
        }
    }
    
    public void FadeInMusicFromPosition(eMusic i_Music, float i_FadeDuration = DEFAULT_FADE_DURATION, float i_StartTime = 0f, float i_TargetVolume = DEFAULT_MUSIC_VOLUME)
    {
        if (m_MusicAudioSource != null && m_FadeCoroutine == null)
        {
            m_FadeCoroutine = StartCoroutine(FadeInCoroutine(i_Music, i_FadeDuration, i_TargetVolume, i_StartTime));
        }
    }
    
    private void storeCurrentMusicTime()
    {
        if (m_MusicAudioSource.isPlaying)
        {
            m_CurrentMusicTime = m_MusicAudioSource.time;
            
            // Also update theme music time if we're storing theme music
            if (m_CurrentMusic == eMusic.Theme)
            {
                m_ThemeMusicTime = m_CurrentMusicTime;
            }
        }
    }
    
    private System.Collections.IEnumerator FadeOutCoroutine(float i_Duration, System.Action i_OnComplete)
    {
        float startVolume = m_MusicAudioSource.volume;
        float timer = 0f;
        
        while (timer < i_Duration && m_MusicAudioSource != null)
        {
            timer += Time.unscaledDeltaTime;
            float normalizedTime = timer / i_Duration;
            m_MusicAudioSource.volume = Mathf.Lerp(startVolume, 0f, normalizedTime);
            yield return null;
        }
        
        if (m_MusicAudioSource != null)
        {
            m_MusicAudioSource.volume = 0f;
            m_MusicAudioSource.Stop();
        }
        
        m_FadeCoroutine = null;
        i_OnComplete?.Invoke();
        
        Debug.Log("Music fade out complete");
    }
    
    private System.Collections.IEnumerator FadeInCoroutine(eMusic i_Music, float i_Duration, float i_TargetVolume, float i_StartTime)
    {
        AudioClip clip = getMusicClip(i_Music);
        if (!clip || m_MusicAudioSource == null) yield break;
        
        setupMusicForFadeIn(i_Music, clip, i_StartTime);
        
        yield return fadeInVolume(i_Duration, i_TargetVolume);
        
        m_FadeCoroutine = null;
        Debug.Log($"Music fade in complete: {i_Music} from time {i_StartTime}");
    }
    
    private void setupMusicForFadeIn(eMusic i_Music, AudioClip i_Clip, float i_StartTime)
    {
        m_CurrentMusic = i_Music;
        m_MusicAudioSource.clip = i_Clip;
        m_MusicAudioSource.loop = (i_Music == eMusic.Theme || i_Music == eMusic.BossTheme);
        m_MusicAudioSource.volume = 0f;
        m_MusicAudioSource.time = i_StartTime;
        m_MusicAudioSource.Play();
        m_IsMusicPaused = false;
    }
    
    private System.Collections.IEnumerator fadeInVolume(float i_Duration, float i_TargetVolume)
    {
        float timer = 0f;
        while (timer < i_Duration && m_MusicAudioSource != null)
        {
            timer += Time.unscaledDeltaTime;
            float normalizedTime = timer / i_Duration;
            m_MusicAudioSource.volume = Mathf.Lerp(0f, i_TargetVolume, normalizedTime);
            yield return null;
        }
        
        if (m_MusicAudioSource != null)
        {
            m_MusicAudioSource.volume = i_TargetVolume;
        }
    }
    
    // ========== GAME EVENT HANDLERS ==========
    
    public void FadeMusic(float i_FadeDuration = DEFAULT_FADE_DURATION)
    {
        // Store theme music time before fading out (for restoration after lightspeed)
        if (m_MusicAudioSource != null && m_MusicAudioSource.isPlaying && m_CurrentMusic == eMusic.Theme)
        {
            m_ThemeMusicTime = m_MusicAudioSource.time;
            Debug.Log($"Stored theme music time before fade: {m_ThemeMusicTime}");
        }
        FadeOutMusic(i_FadeDuration);
    }
    
    public void OnBossWaveStart()
    {
        PlayBossMusic();
    }
    
    public void OnBossDefeated()
    {
        FadeOutMusic(DEFAULT_FADE_DURATION, () => {
            Debug.Log("Boss music faded out after boss defeat");
        });
    }
    
    public void OnNewWaveCycleStart()
    {
        // Wait for light speed music to finish, then fade in theme from stored position
        StartCoroutine(waitForLightSpeedThenFadeTheme());
    }
    
    private System.Collections.IEnumerator waitForLightSpeedThenFadeTheme()
    {
        // Wait for light speed music to finish (if it's currently playing)
        while (m_CurrentMusic == eMusic.LightSpeedTheme && m_MusicAudioSource.isPlaying)
        {
            yield return null;
        }
        
        // Now continue theme from its stored position
        ContinueThemeMusic();
    }
    
    public void OnLightSpeedStart()
    {
        // Theme music time should already be stored from boss music or fade
        // Just play light speed music
        PlayLightSpeedMusic();
    }
    
    
    /// <summary>
    /// Play LightSpeed SFX that can be paused/resumed like music
    /// </summary>
    public void PlayLightSpeedSFX()
    {
        // Use a separate method to play light speed SFX that can be controlled
        if (m_SFXAudioSource != null && m_LightSpeed != null)
        {
            m_SFXAudioSource.clip = m_LightSpeed;
            m_SFXAudioSource.loop = false;
            m_SFXAudioSource.Play();
        }
    }
    
    /// <summary>
    /// Pause SFX (for light speed SFX during pause)
    /// </summary>
    public void PauseSFX()
    {
        if (m_SFXAudioSource != null && m_SFXAudioSource.isPlaying)
        {
            m_SFXAudioSource.Pause();
        }
    }
    
    /// <summary>
    /// Resume SFX (for light speed SFX after pause)
    /// </summary>
    public void ResumeSFX()
    {
        if (m_SFXAudioSource != null && !m_SFXAudioSource.isPlaying && m_SFXAudioSource.clip != null)
        {
            m_SFXAudioSource.UnPause();
        }
    }
    
    /// <summary>
    /// Stop SFX (for light speed SFX cleanup)
    /// </summary>
    public void StopSFX()
    {
        if (m_SFXAudioSource != null)
        {
            m_SFXAudioSource.Stop();
            m_SFXAudioSource.clip = null;
        }
    }
}

