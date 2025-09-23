using UnityEngine;

public class PowerUpPickup : MonoBehaviour
{
    [SerializeField] private float m_FallSpeed = 2f;
    
    private void Update() => transform.Translate(m_FallSpeed * Time.deltaTime * Vector2.down, Space.World);
    
    private void OnTriggerEnter2D(Collider2D i_Other)
    {
        var playerStats = i_Other.GetComponent<PlayerStats>();
        if (playerStats) playerStats.AddWeaponLevel(1);
        
        AudioManager.Instance?.Play(eSFXId.Pickup, 0.7f);
        Destroy(gameObject);
    }
}
