using UnityEngine;

public class PowerUpPickup : MonoBehaviour
{
    [SerializeField] private float m_FallSpeed = 2f;
    
    private void Update() => transform.Translate(m_FallSpeed * Time.deltaTime * Vector2.down, Space.World);
    
    private void OnCollisionEnter2D(Collision2D i_Other)
    {
        if (!i_Other.collider.TryGetComponent(out PlayerStats playerStats)) return;
 
        playerStats.AddWeaponLevel(1);
        AudioManager.Instance?.Play(eSFXId.Pickup, 0.7f);
        Destroy(gameObject);
    }
}
