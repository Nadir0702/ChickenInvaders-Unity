using System.Collections;
using UnityEngine;

public class PlayerHitBox : MonoBehaviour
{
    [SerializeField] private PlayerStats m_PlayerStats;
    
    private Camera m_Camera;

    private void Awake() => m_Camera = Camera.main;
    
    
    private void OnCollisionEnter2D(Collision2D i_Other)
    {
        if (i_Other.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if(!m_PlayerStats.ShieldActive)
            {
                handleTakeDamage();
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
                handleTakeDamage();
                // (Optional) flash/hit VFX hook here
            }
        }
    }
    
    private void handleTakeDamage()
    {
        
        GameManager.Instance?.DamagePlayer(1);
        AudioManager.Instance?.Play(eSFXId.PlayerHit, 0.8f);
        m_PlayerStats.ActivateShield();
        m_PlayerStats.AddWeaponLevel(m_PlayerStats.WeaponLevel / 2 * -1);
        // transform.position = m_Camera.ViewportToWorldPoint(new Vector3(0f, -0.2f, 0f));
        
        if( GameManager.Instance?.GameState == eGameState.GameOver ) return;

        // StartCoroutine(returnToScreen());

        // (Optional) flash/hit VFX hook here

    }

    private IEnumerator returnToScreen()
    {
        yield return null;
    }
}
