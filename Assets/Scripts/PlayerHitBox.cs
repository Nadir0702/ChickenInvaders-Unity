using UnityEngine;

public class PlayerHitBox : MonoBehaviour
{
    [SerializeField] private PlayerStats m_PlayerStats;
    
    private void OnCollisionEnter2D(Collision2D i_Other)
    {
        if (i_Other.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if(!m_PlayerStats.ShieldActive)
            {
                GameManager.Instance?.DamagePlayer(1);
                playDamageSFX();
                // (Optional) flash/hit VFX hook here
            }

        }
    }

    private void OnTriggerEnter2D(Collider2D i_Other)
    {
        if (i_Other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if(!m_PlayerStats.ShieldActive)
            {
                GameManager.Instance?.DamagePlayer(1);
                playDamageSFX();
                // (Optional) flash/hit VFX hook here
            }
        }
    }
    
    private void playDamageSFX() => AudioManager.Instance?.Play(eSFXId.PlayerHit, 0.8f);
}
