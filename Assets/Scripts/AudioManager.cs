using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField]
    private AudioClip m_Shoot, m_EnemyHit, m_PlayerHit, m_UIClick, m_Pickup, m_Explosion, m_Eat, m_GameOver;
    [SerializeField] private AudioSource m_AudioSource;
    
    private void Awake() => DontDestroyOnLoad(this);
    
    public void Play (eSFXId i_SfxId, float i_Volume = 1f, float i_Pitch = 1f)
    {
        AudioClip clip = i_SfxId switch
        {
            eSFXId.Shoot => m_Shoot,
            eSFXId.EnemyHit => m_EnemyHit,
            eSFXId.PlayerHit => m_PlayerHit,
            eSFXId.UIClick => m_UIClick,
            eSFXId.Pickup => m_Pickup,
            eSFXId.Explosion => m_Explosion,
            eSFXId.Eat => m_Eat,
            eSFXId.GameOver => m_GameOver,
            _ => null
        };

        if (!clip) return;
        
        m_AudioSource.pitch = i_Pitch;
        m_AudioSource.PlayOneShot(clip, i_Volume);
    }

    public void OnUIButtonClick()
    {
        Play(eSFXId.UIClick, 0.5f, 1.5f);
    }
}
