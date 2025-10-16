using UnityEngine;

public class PlayerHitBox : MonoBehaviour
{
    [SerializeField] private PlayerStats m_PlayerStats;
    
    private PlayerRespawn m_PlayerRespawn;

    private void Awake() 
    {
        m_PlayerRespawn = GetComponent<PlayerRespawn>();
    }
    
    
    private void OnCollisionEnter2D(Collision2D i_Other)
    {
        
        if(!m_PlayerStats.ShieldActive && !m_PlayerRespawn.IsRespawning)
        {
            if (i_Other.collider.gameObject.layer == LayerMask.NameToLayer("Enemy") ||
                i_Other.collider.gameObject.layer == LayerMask.NameToLayer("EnemyBullet"))
            {
                handleTakeDamage();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D i_Other)
    {
        
        if(!m_PlayerStats.ShieldActive && !m_PlayerRespawn.IsRespawning)
        {
            if (i_Other.gameObject.layer == LayerMask.NameToLayer("Enemy") ||
                i_Other.gameObject.layer == LayerMask.NameToLayer("EnemyBullet"))
            {
                handleTakeDamage();
            }
        }
    }
    
    private void handleTakeDamage()
    {
        GameManager.Instance?.DamagePlayer(1);                          // 1. Take damage
        AudioManager.Instance?.Play(eSFXId.Explosion, 0.8f);            // 2. Play explosion sound
        m_PlayerStats.ActivateShield();                                         // 3. Activate shield
        m_PlayerStats.AddWeaponLevel(m_PlayerStats.WeaponLevel / 2 * -1); // 4. Cut weapon level in half
        
        m_PlayerRespawn?.TriggerExplosionAndRespawn();                          // 5. Trigger explosion and respawn
    }

}
