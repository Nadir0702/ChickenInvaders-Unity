using UnityEngine;

public class PlayerHitBox : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D i_Other)
    {
        if (i_Other.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            GameManager.Instance?.DamagePlayer(1);
            playDamageSFX();
            // (Optional) flash/hit VFX hook here
        }
    }

    private void OnTriggerEnter2D(Collider2D i_Other)
    {
        if (i_Other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            GameManager.Instance?.DamagePlayer(1);
            playDamageSFX();
            // (Optional) flash/hit VFX hook here
        }
    }
    
    private void playDamageSFX() => AudioManager.Instance?.Play(eSFXId.PlayerHit, 0.8f);
}
